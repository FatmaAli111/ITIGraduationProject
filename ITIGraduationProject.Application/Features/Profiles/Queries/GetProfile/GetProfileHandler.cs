using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.Interfaces.Persistence;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;



namespace ITIGraduationProject.Application.Features.Profiles.Queries.GetProfile
{
    public class GetProfileHandler : IRequestHandler<GetProfileQuery, Response<ProfileDTO>>
    {
        #region Dependency Injection
        private readonly IUnitOfWork _unitOfWork;

        public GetProfileHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Handle Method
        public async Task<Response<ProfileDTO>> Handle(GetProfileQuery request, CancellationToken cancellationToken){

            if (!Guid.TryParse(request.UserId, out Guid userGuid))
            {
                return new Response<ProfileDTO>
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Succeeded = false,
                    Message = "Invalid User ID format.",
                    Data = null
                };
            }

            var user = await _unitOfWork.Users.GetWithProfileCartAndPreferencesAsync(userGuid);

            if (user == null)
            {
                return new Response<ProfileDTO>
                {
                    StatusCode = System.Net.HttpStatusCode.NotFound,
                    Succeeded = false,
                    Message = "User not found",
                    Data = null
                };
            }

            #region Mapster
            var profileDTO = user.Adapt<ProfileDTO>();
            #endregion

            #region Calculated data using IUnitOfWork
            profileDTO.TotalOrdersCount = await _unitOfWork.Orders.GetTableNoTracking().CountAsync(
                order => order.User.Id == userGuid, cancellationToken);

            profileDTO.ItemsPurchasedCount = await _unitOfWork.Orders.GetTableNoTracking()
                .Where(order => order.User.Id == userGuid)
                .SelectMany(o => o.OrderItems)
                .SumAsync(item => item.Quantity, cancellationToken);

            profileDTO.TotalSpent = await _unitOfWork.Orders.GetTableNoTracking()
                .Where(order => order.User.Id == userGuid)
                .SumAsync(order => order.TotalAmount, cancellationToken);

            profileDTO.TemplatesCreatedCount = await _unitOfWork.Templates.GetTableNoTracking()
                .CountAsync(template => template.CreatorUserId == userGuid, cancellationToken);

            profileDTO.AvgTemplateRating = await _unitOfWork.Templates.GetTableNoTracking()
                .Where(templateRate => templateRate.CreatorUserId == userGuid)
                .AverageAsync(templateRate => (double?)templateRate.LikesCount, cancellationToken) ?? 0.0;

            profileDTO.FollowersCount = 500;
            profileDTO.FollowingCount = 488;
            #endregion

            #region Response
            return new Response<ProfileDTO>
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Succeeded = true,
                Message = "Profile retrieved successfully",
                Data = profileDTO
            };
            #endregion
        }
        #endregion

    }
}

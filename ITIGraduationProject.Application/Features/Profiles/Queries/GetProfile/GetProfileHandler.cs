using MediatR;
using ITIGraduationProject.Application.Bases;
using Microsoft.EntityFrameworkCore;
using ITIGraduationProject.Infrastructure.Data;
using Mapster;


namespace ITIGraduationProject.Application.Features.Profiles.Queries.GetProfile
{
    public class GetProfileHandler : IRequestHandler<GetProfileQuery, Response<ProfileDTO>>
    {
        private readonly AppDbContext _context;

        #region DependencyInjection
        public GetProfileHandler(AppDbContext context)
        {
            _context = context;
        }
        #endregion

        #region HandleMethod
        public async Task<Response<ProfileDTO>> Handle(GetProfileQuery request, CancellationToken cancellationToken){ 
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

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

            #region Controller
            profileDTO.TotalOrdersCount = 5;
            profileDTO.ItemsPurchasedCount = 6;
            profileDTO.TotalSpent = 1500m;
            profileDTO.TemplatesCreatedCount = 7;
            profileDTO.AvgTemplateRating = 4.6;
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

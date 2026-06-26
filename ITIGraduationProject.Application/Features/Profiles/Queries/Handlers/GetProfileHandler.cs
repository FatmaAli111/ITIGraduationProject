using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS.Profiles;
using ITIGraduationProject.Application.Features.Profiles.Queries.Models;
using ITIGraduationProject.Application.Interfaces.Persistence;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Profiles.Queries.Handlers
{
    public class GetProfileHandler : ResponseHandler, IRequestHandler<GetProfileQuery, Response<ProfileDTO>>
    {
        #region Dependency Injection
        private readonly IUnitOfWork _unitOfWork;

        public GetProfileHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Handle Method
        public async Task<Response<ProfileDTO>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        {

            var userBasic = await _unitOfWork.Users.GetTableNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (userBasic == null)
            {
                return NotFound<ProfileDTO>("User not found");
            }

            var userGuid = userBasic.Id;

            var user = await _unitOfWork.Users.GetWithProfileCartAndPreferencesAsync(userGuid);

            if (user == null)
            {
                return NotFound<ProfileDTO>("User not found");
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
            return Success(profileDTO);
            #endregion
        }
        #endregion
    }
}
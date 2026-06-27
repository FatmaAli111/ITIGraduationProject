using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Features.Profiles.Commands.Models;

namespace ITIGraduationProject.Application.Features.Profiles.Commands.Handlers
{
    public class UpdateProfileHandler : ResponseHandler, IRequestHandler<UpdateProfileCommand, Response<string>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProfileHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<string>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(Guid.Parse(request.UserId));

            if (user == null)
            {
                var userBasic = await _unitOfWork.Users.GetTableNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

                if (userBasic != null)
                {
                    user = await _unitOfWork.Users.GetByIdAsync(userBasic.Id);
                }
            }

            if (user == null)
            {
                return BadRequest<string>("User not found.");
            }

            if (request.ProfileImage != null && request.ProfileImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + request.ProfileImage.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ProfileImage.CopyToAsync(fileStream);
                }

                user.ProfileImageUrl = $"/images/profiles/{uniqueFileName}";
            }

            user.Name = request.Name;
            user.UserName = request.UserName;
            user.Email = request.Email;
            user.Bio = request.Bio;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return Success("Success");
        }
    }
}
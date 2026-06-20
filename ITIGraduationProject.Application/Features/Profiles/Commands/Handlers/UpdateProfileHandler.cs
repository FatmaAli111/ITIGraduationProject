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
using System.Threading.Tasks;
using ITIGraduationProject.Application.Features.Profiles.Commands.Models;



namespace ITIGraduationProject.Application.Features.Profiles.Commands.Handlers
{
    public class UpdateProfileHandler : ResponseHandler, IRequestHandler<UpdateProfileCommand, Response<string>> {

        #region Dependency Injection 
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProfileHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        #endregion

        #region Handle Method
        public async Task<Response<string>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken){

            var userGuid = Guid.Parse(request.UserId);
            var user = await _unitOfWork.Users.GetByIdAsync(userGuid);

            if (user == null)
            {
                return NotFound<string>("User not found.");
            }

            #region Handling Images Profile to Save them in the folder

            if (request.ProfileImage != null && request.ProfileImage.Length > 0) {

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");
                if (!Directory.Exists(uploadsFolder)) {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // create unique name for each image file
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + request.ProfileImage.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create)) {
                    await request.ProfileImage.CopyToAsync(fileStream);
                }

                user.ProfileImageUrl = $"/images/profiles/{uniqueFileName}";


            }
            #endregion

            #region Update Data From Request
            user.Name = request.Name;
            user.UserName = request.UserName;
            user.Email = request.Email;
            user.Bio = request.Bio;
            #endregion

            _unitOfWork.Users.Update(user);
            var result = await _unitOfWork.SaveChangesAsync();

            #region Updates Processes Result

            if (result > 0)
            {
                return Success("Success");
            }

            return BadRequest<string>("No changes were saved to the database.");
            #endregion
        }
        #endregion
    }
}

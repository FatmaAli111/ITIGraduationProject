using ITIGraduationProject.Application.Interfaces.Persistence;
using ITIGraduationProject.Domain.Entities.Designs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Studio.Commands.CreateGraphicAsset
{
    public class CreateGraphicAssetCommandHandler : IRequestHandler<CreateGraphicAssetCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateGraphicAssetCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(CreateGraphicAssetCommand request, CancellationToken cancellationToken)
        {
            var asset = new GraphicAsset
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                Name = request.Name,
                Type = request.Type,
                ImageUrl = request.ImageUrl,
                Tags = request.Tags
            };

            await _unitOfWork.GraphicAssets.AddAsync(asset);
            await _unitOfWork.SaveChangesAsync();

            return asset.Id;
        }
    }
}

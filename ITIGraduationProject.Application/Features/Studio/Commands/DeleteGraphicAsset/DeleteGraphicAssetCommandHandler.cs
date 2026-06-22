using ITIGraduationProject.Application.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Application.Features.Studio.Commands.DeleteGraphicAsset
{
    public class DeleteGraphicAssetCommandHandler : IRequestHandler<DeleteGraphicAssetCommand>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteGraphicAssetCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DeleteGraphicAssetCommand request, CancellationToken cancellationToken)
        {
            var asset = await _unitOfWork.GraphicAssets.GetTableAsTracking()
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (asset == null)
            {
                throw new KeyNotFoundException($"Entity \"GraphicAsset\" ({request.Id}) was not found.");
            }

            _unitOfWork.GraphicAssets.Delete(asset);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

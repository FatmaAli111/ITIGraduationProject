using ITIGraduationProject.Application.Bases;
using MediatR;

namespace ITIGraduationProject.Application.Features.Shop.Commands.Models
{
    public record ClearCartCommand : IRequest<Response<string>>;
}

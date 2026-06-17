using ITIGraduationProject.Application.Bases;
using ITIGraduationProject.Application.DTOS;
using ITIGraduationProject.Application.Features.Identitiy.Commands.Models;
using ITIGraduationProject.Application.Interfaces.IServices.IdentityServices;
using MediatR;
using MapsterMapper;

namespace ITIGraduationProject.Application.Features.Identitiy.Commands.Handlers
{
    public class CommandHandler :ResponseHandler,
        IRequestHandler<RegisterCommand, Response<string>>,
                IRequestHandler<ConfirmEmailCommand, Response<string>>

    {
        private readonly IIdentityService _identityService;
        private readonly IMapper _mapper;

        public CommandHandler(IIdentityService identityService, IMapper mapper)
        {
            _identityService = identityService;
            _mapper = mapper;
        }

        public async Task<Response<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var dto = _mapper.Map<RegisterRequestDTO>(request);

            var Result = await _identityService.RegisterAsync(dto);
            
            if(Result != "Registration successful. Please check your email to confirm your account.")
                    return BadRequest<string>(Result);
            else

                return Success<string>(Result, true);
        }

        public async Task<Response<string>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            var result = await _identityService.ConfirmEmailAsync(request.UserId, request.Token);

            if (!result.Succeeded)
                return BadRequest<string>(result.Message);

            return Success<string>(null, result.Message);
        }
    }
}

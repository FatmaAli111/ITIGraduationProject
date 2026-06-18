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
                IRequestHandler<ConfirmEmailCommand, Response<string>>,
        IRequestHandler<LoginCommand, Response<LoginResponseDTO>>
        ,IRequestHandler<RefreshTokenCommand,Response<LoginResponseDTO>>
        , IRequestHandler<LogoutCommand, Response<string>>
        , IRequestHandler<LogoutAllDevicesCommand, Response<string>>
        , IRequestHandler<ForgetPasswordCommand, Response<string>>
        , IRequestHandler<ResetPasswordCommand, Response<string>>

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
            return await _identityService.RegisterAsync(dto);
        }
        public async Task<Response<string>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            return await _identityService.ConfirmEmailAsync(request.UserId, request.Token);
        }
        public async Task<Response<LoginResponseDTO>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var dto = _mapper.Map<LoginRequestDTO>(request);
            return await _identityService.LoginAsync(dto);
        }

        public async Task<Response<LoginResponseDTO>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            return await _identityService.RefreshTokenAsync(request.RefreshToken);
        }
        public async Task<Response<string>> Handle(
    LogoutCommand request,
    CancellationToken cancellationToken)
        {
            return await _identityService.LogoutAsync(request.RefreshToken);
        }
        public async Task<Response<string>> Handle(
    LogoutAllDevicesCommand request,
    CancellationToken cancellationToken)
        {
            return await _identityService.LogoutAllDevicesAsync();
        }
        public async Task<Response<string>> Handle(
    ForgetPasswordCommand request,
    CancellationToken cancellationToken)
        {
            return await _identityService
                .ForgetPasswordAsync(request.Email);
        }
        public async Task<Response<string>> Handle(
    ResetPasswordCommand request,
    CancellationToken cancellationToken)
        {
            return await _identityService.ResetPasswordAsync(
                request.Email,
                request.Token,
                request.NewPassword);
        }
    }

}

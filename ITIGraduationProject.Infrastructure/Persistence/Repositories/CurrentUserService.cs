using ITIGraduationProject.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ITIGraduationProject.Infrastructure.Persistence.Repositories
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContext;

        public CurrentUserService(IHttpContextAccessor httpContext)
            => _httpContext = httpContext;

        public Guid UserId => Guid.Parse(
            _httpContext.HttpContext!.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }
}

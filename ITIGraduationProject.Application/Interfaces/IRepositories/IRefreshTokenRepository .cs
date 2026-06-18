using System;
using System.Threading.Tasks;
using ITIGraduationProject.Application.Repositories;
using ITIGraduationProject.Domain.Entities.Identity;

namespace ITIGraduationProject.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository : IGenericRepo<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<List<RefreshToken>> GetUserTokensAsync(Guid userId);
    }
}
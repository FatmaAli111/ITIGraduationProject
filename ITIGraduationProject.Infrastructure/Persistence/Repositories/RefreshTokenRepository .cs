// Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ITIGraduationProject.Application.Interfaces.Repositories;
using ITIGraduationProject.Domain.Entities.Identity;

namespace ITIGraduationProject.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : GenericRepo<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }
        public async Task<List<RefreshToken>> GetUserTokensAsync(Guid userId)
        {
            return await _context.RefreshTokens
                .Where(x => x.UserId == userId && !x.IsRevoked)
                .ToListAsync();
        }
    }
}
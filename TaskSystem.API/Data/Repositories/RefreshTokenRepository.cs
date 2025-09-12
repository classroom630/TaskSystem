using Microsoft.EntityFrameworkCore;
using TaskSystem.API.Models.Domain;

namespace TaskSystem.API.Data.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;
        
        public RefreshTokenRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        
        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }
        
        public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked && !rt.IsExpired)
                .ToListAsync();
        }
        
        public async Task RevokeUserTokensAsync(string userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();
                
            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }
            
            await _context.SaveChangesAsync();
        }
        
        public async Task RevokeTokenAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);
                
            if (refreshToken != null)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        
        public async Task CleanupExpiredTokensAsync()
        {
            var expiredTokens = await _context.RefreshTokens
                .Where(rt => rt.ExpiryDate <= DateTime.UtcNow || rt.IsRevoked)
                .ToListAsync();
                
            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }
    }
}
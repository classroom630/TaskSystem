using TaskSystem.API.Models.Domain;

namespace TaskSystem.API.Data.Repositories
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId);
        Task RevokeUserTokensAsync(string userId);
        Task RevokeTokenAsync(string token);
        Task CleanupExpiredTokensAsync();
    }
}
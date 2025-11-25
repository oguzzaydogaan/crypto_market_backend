using Microsoft.EntityFrameworkCore;
using Repositories.Entities;

namespace Repositories.Repositories
{
    public class CoinRepository
    {
        public AppDbContext _context;
        public DbSet<Coin> Coins;
        public CoinRepository(AppDbContext context)
        {
            _context = context;
            Coins = _context.Set<Coin>();
        }

        public async Task<List<Coin>> GetAllCoinsAsync()
        {
            return await Coins.ToListAsync();
        }

        public async Task<Coin?> GetCoinByIdAsync(int id)
        {
            return await Coins.FindAsync(id);
        }

        public async Task<Coin> AddCoinAsync(Coin coin)
        {
            await Coins.AddAsync(coin);
            await _context.SaveChangesAsync();
            return coin;
        }

        public async Task UpdateCoinAsync(Coin coin)
        {
            Coins.Update(coin);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCoinAsync(int id)
        {
            var coin = await Coins.FirstAsync(c => c.Id == id);
            Coins.Remove(coin);
            await _context.SaveChangesAsync();
        }
    }
}

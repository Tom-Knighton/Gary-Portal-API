using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GaryPortalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GaryPortalAPI.Services
{
    public interface IAppService : IDisposable
    {
        Task<ICollection<Sticker>> GetStickersAsync();
    }

    public class AppService : IAppService
    {
        private readonly AppDbContext _context;

        public AppService(AppDbContext context)
        {
            _context = context;
        }

        public void Dispose()
        {
        }

        public async Task<ICollection<Sticker>> GetStickersAsync()
        {
            return await _context.Stickers.AsNoTracking().Where(s => !s.StickerIsDeleted).ToListAsync();
        }
    }
}

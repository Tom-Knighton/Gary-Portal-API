using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GaryPortalAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GaryPortalAPI.Services
{
    public interface IAppService : IDisposable
    {
        Task<ICollection<Sticker>> GetStickersAsync();
        Task<ICollection<Event>> GetEventsAsync(int teamId, CancellationToken ct = default);
        Task<ICollection<Commandment>> GetCommandmentsAsync(CancellationToken ct = default);
        Task<ICollection<Flag>> GetAllFlagsAsync(CancellationToken ct = default);
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

        public async Task<ICollection<Event>> GetEventsAsync(int teamId = 0, CancellationToken ct = default)
        {
            return await _context
                .Events
                .AsNoTracking()
                .Where(e => e.EventTeamId == teamId || teamId == 0 || e.EventTeamId == null)
                .Where(e => e.EventEndsAt >= DateTime.UtcNow && !e.IsEventDeleted)
                .OrderBy(e => e.EventDate)
                .ToListAsync(ct);
        }

        public async Task<ICollection<Commandment>> GetCommandmentsAsync(CancellationToken ct = default)
        {
            return await _context
                .Commandments
                .AsNoTracking()
                .Where(c => !c.CommandmentIsDeleted)
                .ToListAsync(ct);
        }

        public async Task<ICollection<Flag>> GetAllFlagsAsync(CancellationToken ct = default)
        {
            return await _context
                .Flags
                .AsNoTracking()
                .Where(c => !c.FlagIsDeleted)
                .ToListAsync(cancellationToken: ct);
        }
    }
}

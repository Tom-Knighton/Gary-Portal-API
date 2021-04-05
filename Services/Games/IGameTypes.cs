using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GaryPortalAPI.Data;
using GaryPortalAPI.Models;
using GaryPortalAPI.Models.Games;
using Microsoft.EntityFrameworkCore;

namespace GaryPortalAPI.Services
{
    public interface IGameTypeService : IDisposable
    {
        Task<ICollection<GameType>> GetGameTypesAsync(int teamId, CancellationToken ct = default);
    }

    public class GameTypeService : IGameTypeService
    {
        private readonly AppDbContext _gameContext;
        public GameTypeService(AppDbContext gameContext)
        {
            _gameContext = gameContext;
        }

        public async Task<ICollection<GameType>> GetGameTypesAsync(int teamId, CancellationToken ct = default)
        {
            return await _gameContext.GameTypes
                .Where(gt => gt.GameIsEnabled && gt.GameTeamId == null || gt.GameTeamId == 0 || gt.GameTeamId == teamId)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public void Dispose()
        {
            _gameContext.Dispose();
        }
    }
}
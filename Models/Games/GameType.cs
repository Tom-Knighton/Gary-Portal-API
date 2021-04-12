using System;
namespace GaryPortalAPI.Models.Games
{
    public class GameType
    {
        public string GameUUID { get; set; }
        public string GameName { get; set; }
        public string GameDescription { get; set; }
        public int? GameTeamId { get; set; }
        public bool GameIsEnabled { get; set; }
        public string GameCoverUrl { get; set; }

        public virtual Team GameTeam { get; set; }
    }
}

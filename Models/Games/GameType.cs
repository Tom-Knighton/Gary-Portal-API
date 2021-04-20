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


    public class TicTacGaryGame
    {
        public string GameCode { get; set; }
        public int GameSize { get; set; }
        public string FirstPlayerUUID { get; set; }
        public string SecondPlayerUUID { get; set; }
        public int[,] GameMatrix { get; set; }
        public string WinnerUUID { get; set; }
        public int GameWinType { get; set; }

        public virtual UserDTO FirstUser { get; set; }
        public virtual UserDTO SecondUser { get; set; }
    }
}



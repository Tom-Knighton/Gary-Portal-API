using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GaryPortalAPI.Models.Chat;
using GaryPortalAPI.Models.Games;
using GaryPortalAPI.Services;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace GaryPortalAPI.Hubs
{
    public class GameHub : Hub 
    {
        private readonly IUserService _userService;
        private static List<TicTacGaryGame> _games = new List<TicTacGaryGame>();

        public GameHub(IUserService userService)
        {
            _userService = userService;
        }


        public async Task CreateTTGGame(string creatorUUID, int size)
        {
            TicTacGaryGame game = new TicTacGaryGame
            {
                GameCode = "",
                GameMatrix = new int[size, size],
                GameSize = size,
            };

            Random random = new Random();
            string gameCode = random.Next(0, 1000000).ToString("D6");
            game.GameCode = gameCode;
            _games.Add(game);
            await JoinTTGGame(creatorUUID, gameCode);
        }

        public async Task JoinTTGGame(string uuid, string code)
        {
            TicTacGaryGame game = _games.First(g => g.GameCode == code);
            if (game == null)
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ErrorJoiningGame", "There was no game with that game code found.");
                return;
            }

            if (!string.IsNullOrEmpty(game.FirstPlayerUUID) && !string.IsNullOrEmpty(game.SecondPlayerUUID))
            {
                await Clients.Client(Context.ConnectionId).SendAsync("ErrorJoiningGame", "The game is already full!");
                return;
            }

            bool isSecondPlayer = !string.IsNullOrEmpty(game.FirstPlayerUUID) && string.IsNullOrEmpty(game.SecondPlayerUUID);

            if (isSecondPlayer)
            {
                game.SecondPlayerUUID = uuid;
                game.SecondUser = await _userService.GetDTOByIdAsync(uuid);
            }
            else
            {
                game.FirstPlayerUUID = uuid;
                game.FirstUser = await _userService.GetDTOByIdAsync(uuid);
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, code);
            await Clients.Group(code).SendAsync("UpdateGameLobby", game);
        }



        public async Task StartTTGGame(string code)
        {
            await Clients.Group(code).SendAsync("TTG_StartGame");
        }
    }
}

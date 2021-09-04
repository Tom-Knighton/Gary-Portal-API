using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GaryPortalAPI.Data;
using GaryPortalAPI.Models.Games;
using GaryPortalAPI.Services;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GaryPortalAPI.Hubs
{
    public class GameHub : Hub 
    {
        private readonly IUserService _userService;
        private static List<TicTacGaryGame> _games = new List<TicTacGaryGame>();


        JsonSerializerSettings camelCaseFormatter = new JsonSerializerSettings();
        
        public GameHub(IUserService userService)
        {
            _userService = userService;
            camelCaseFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }



        #region Tic Tac Gary
        public async Task TTGCreateGame(string creatorUUID, int size)
        {
            TicTacGaryGame game = new TicTacGaryGame
            {
                GameCode = "",
                GameMatrix = new TTGCell[size, size],
                GameSize = size,
            };

            for (int col = 0; col < game.GameMatrix.GetLength(0); col++)
                for (int row = 0; row < game.GameMatrix.GetLength(1); row++)
                    game.GameMatrix[col, row] = new TTGCell { Content = "", Id = $"{col},{row}"};

            Random random = new Random();
            string gameCode = random.Next(0, 1000000).ToString("D6");
            game.GameCode = gameCode;
            _games.Add(game);
            await TTGJoinGame(creatorUUID, gameCode);
        }

        public async Task TTGJoinGame(string uuid, string code)
        {
            TicTacGaryGame game = _games.FirstOrDefault(g => g.GameCode == code);
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
            await Clients.Group(code).SendAsync("UpdateGameLobby", JsonConvert.SerializeObject(game, camelCaseFormatter));
        }

        public async Task TTGLeaveGame(string uuid, string code)
        {
            TicTacGaryGame game = _games.FirstOrDefault(g => g.GameCode == code);
            if (game == null)
                return;

            bool isHost = uuid == game.FirstPlayerUUID;
            if (isHost)
            {
                game.FirstPlayerUUID = null;
                game.FirstUser = null;
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, code);
                await Clients.Group(code).SendAsync("HostLeftLobby");

                _games.RemoveAll(g => g.GameCode == code);
            }
            else
            {
                game.SecondPlayerUUID = null;
                game.SecondUser = null;
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, code);
                await Clients.Group(code).SendAsync("UpdateGameLobby", JsonConvert.SerializeObject(game, camelCaseFormatter));
            }
        }

        public async Task TTGStartGame(string code)
        {
            await Clients.Group(code).SendAsync("TTG_StartGame");
        }

        public async Task TTGMakeMove(string code, string uuid, string cellId)
        {
            TicTacGaryGame game = _games.FirstOrDefault(g => g.GameCode == code);
            if (game == null)
                return;

            string[] cellIdSplit = cellId.Split(",");
            int col = Convert.ToInt32(cellIdSplit[0]);
            int row = Convert.ToInt32(cellIdSplit[1]);
            string symbol = uuid == game.FirstPlayerUUID ? "X" : "O"; 
            game.GameMatrix[col, row].Content = symbol;

            int winSize = game.GameSize == 3 ? 3 : 4;

            string currentRow = string.Join("", game.GameMatrix.GetRow(row).Select(r => r.Content));
            string currentCol = string.Join("", game.GameMatrix.GetCol(col).Select(c => c.Content));

            bool hasWonHorizontal = false, hasWonVertical = false, hasWonDiagonal = false;
            string winningWord = game.GameSize == 3 ? $"{symbol}{symbol}{symbol}" : $"{symbol}{symbol}{symbol}{symbol}";

            if (!string.IsNullOrEmpty(currentRow) && currentRow.Contains(winningWord))
            {
                hasWonHorizontal = true;
            }
            if (!hasWonHorizontal && !string.IsNullOrEmpty(currentCol) && currentCol.Contains(winningWord))
            {
                hasWonVertical = true;
            }
            if (!hasWonHorizontal && !hasWonVertical)
            {
                if (game.GameMatrix.GetLeftDiagonalStringFromCoord(row, col) is string leftdiagonal && !string.IsNullOrWhiteSpace(leftdiagonal) && leftdiagonal.Contains(winningWord))
                    hasWonDiagonal = true;
                else if (game.GameMatrix.GetRightDiagonalStringFromCoord(row, col) is string rightdiagonal && !string.IsNullOrWhiteSpace(rightdiagonal) && rightdiagonal.Contains(winningWord))
                    hasWonDiagonal = true;
            }

            if (hasWonDiagonal || hasWonHorizontal || hasWonHorizontal)
            {
                Console.WriteLine("GAME WON!!!!!");
                for (int newCol = 0; col < game.GameMatrix.GetLength(0); newCol++)
                    for (int newRow = 0; row < game.GameMatrix.GetLength(1); newRow++)
                        game.GameMatrix[col, row] = new TTGCell { Content = "", Id = $"{col},{row}" };

                if (uuid == game.FirstPlayerUUID)
                {
                    game.PlayerOneWins += 1;
                } else
                {
                    game.PlayerTwoWins += 1;
                }
                game.CurrentUUIDTurn = uuid == game.FirstPlayerUUID ? game.FirstPlayerUUID : game.SecondPlayerUUID;
                await Clients.Group(code).SendAsync("TTG_MovePlayed", uuid, cellId, symbol);
                await Clients.Group(code).SendAsync("TTG_GameWon", uuid);
            }
            else
            {
                game.CurrentUUIDTurn = game.CurrentUUIDTurn == game.FirstPlayerUUID ? game.SecondPlayerUUID : game.FirstPlayerUUID;
                await Clients.Group(code).SendAsync("TTG_MovePlayed", uuid, cellId, symbol);
            }
        }

        #endregion
    }
}

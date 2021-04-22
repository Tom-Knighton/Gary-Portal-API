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
                GameMatrix = new string[size, size],
                GameSize = size,
            };

            Random random = new Random();
            string gameCode = random.Next(0, 1000000).ToString("D6");
            game.GameCode = gameCode;
            _games.Add(game);
            await TTGJoinGame(creatorUUID, gameCode);
        }

        public async Task TTGJoinGame(string uuid, string code)
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
            await Clients.Group(code).SendAsync("UpdateGameLobby", JsonConvert.SerializeObject(game, camelCaseFormatter));
        }

        public async Task TTGLeaveGame(string uuid, string code)
        {
            TicTacGaryGame game = _games.First(g => g.GameCode == code);
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

        public async Task TTGMakeMove(string code, string uuid, int row, int col)
        {
            TicTacGaryGame game = _games.FirstOrDefault(g => g.GameCode == code);
            if (game == null)
                return;

            string symbol = uuid == game.FirstPlayerUUID ? "X" : "O";
            game.GameMatrix[row, col] = symbol;

            int winSize = game.GameSize == 3 ? 3 : 4;

            string currentRow = string.Join("", game.GameMatrix.GetRow(row));
            string currentCol = string.Join("", game.GameMatrix.GetCol(col));

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
                Array.Clear(game.GameMatrix, 0, game.GameMatrix.Length);
                if (uuid == game.FirstPlayerUUID)
                {
                    game.PlayerOneWins += 1;
                } else
                {
                    game.PlayerTwoWins += 1;
                }
                game.CurrentUUIDTurn = uuid == game.FirstPlayerUUID ? game.FirstPlayerUUID : game.SecondPlayerUUID;
                await Clients.Group("code").SendAsync("TTG_GameWon", uuid);
            } else
            {
                game.CurrentUUIDTurn = game.CurrentUUIDTurn == game.FirstPlayerUUID ? game.SecondPlayerUUID : game.FirstPlayerUUID;
                await Clients.Group("code").SendAsync("TTG_MovePlayed", uuid, row, col);
            }
        }

        #endregion
    }
}

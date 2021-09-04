using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GaryPortalAPI.Models;
using GaryPortalAPI.Models.Chat;
using Newtonsoft.Json;
using RestSharp;

namespace GaryPortalAPI.Services
{
    public interface IChatBotService : IDisposable
    {
        ICollection<ChatCommand> GetAllCommands();
        bool IsValidCommand(string input);
        Task<string> GetResponseForCommand(string input, string uuid, string version);
    }

    public class ChatBotService : IChatBotService
    {
        private readonly IUserService _userService;
        private readonly IStaffService _staffService;
        public ChatBotService(IUserService userService, IStaffService staffService)
        {
            _userService = userService;
            _staffService = staffService;
        }

        private readonly IReadOnlyList<ChatCommand> _commands = new List<ChatCommand>()
        {
            new ChatCommand { Command = "?help", CommandDescription = "Prints list of all commands", CommandFriendlyName = "Help", CommandUsage = "?help" },
            new ChatCommand { Command = "?users", CommandDescription = "Returns the number of total users", CommandFriendlyName = "User Count", CommandUsage = "?users" },
            new ChatCommand { Command = "?teamcount", CommandDescription = "Returns the number of users in your team", CommandFriendlyName = "Users in team", CommandUsage = "?teamcount" },
            new ChatCommand { Command = "?bots", CommandDescription = "Returns the number of bots", CommandFriendlyName = "Bot Count", CommandUsage = "?bots" },
            new ChatCommand { Command = "?latestversion", CommandDescription = "Returns the latest available app version", CommandFriendlyName = "Latest App Version", CommandUsage = "?latestversion" },
            new ChatCommand { Command = "?myversion", CommandDescription = "Returns your app version", CommandFriendlyName = "My App Version", CommandUsage = "?myversion" },
            new ChatCommand { Command = "?joke", CommandDescription = "Al Murray tells a joke!", CommandFriendlyName = "Tell a joke", CommandUsage = "?joke" },
            new ChatCommand { Command = "?myroles", CommandDescription = "Lists your roles and ranks", CommandFriendlyName = "My Roles", CommandUsage = "?myroles" },
            new ChatCommand { Command = "?mock", CommandDescription = "Mocks your input text", CommandFriendlyName = "Mock", CommandUsage = "?mock [text]" },
            new ChatCommand { Command = "?roll", CommandDescription = "Rolls a dice", CommandFriendlyName = "Roll a dice", CommandUsage = "?roll" },
            new ChatCommand { Command = "?8ball", CommandDescription = "Answers your question!", CommandFriendlyName = "Magic 8 Ball", CommandUsage = "?8ball [question]" },
            new ChatCommand { Command = "?stankrate", CommandDescription = "Rates your stankiness", CommandFriendlyName = "Stank Rate", CommandUsage = "?stankrate {username}" },
            new ChatCommand { Command = "?simprate", CommandDescription = "Rates your simpiness", CommandFriendlyName = "Simp Rate", CommandUsage = "?simprate {username}" },
            new ChatCommand { Command = "?hitman", CommandDescription = "Hires a hitman", CommandFriendlyName = "Hitman", CommandUsage = "?hitman {username}" },
            new ChatCommand { Command = "?gif", CommandDescription = "Searches for a gif", CommandFriendlyName = "Gif Search", CommandUsage = "?gif {search term}"},

            new ChatCommand { Command = "?globalban", CommandDescription = "Globally Bans a user", CommandFriendlyName = "Global Ban", RequiresAdmin = true, CommandUsage = "?globalban [username] {reason}" },
            new ChatCommand { Command = "?chatban", CommandDescription = "Bans a user from the chat", CommandFriendlyName = "Chat Ban", RequiresAdmin = true, CommandUsage = "?chatban [username] {reason}" },
            new ChatCommand { Command = "?feedban", CommandDescription = "Bans a user from the feed", CommandFriendlyName = "Feed Ban", RequiresAdmin = true, CommandUsage = "?feedban [username] {reason}"}
        };

        public ICollection<ChatCommand> GetAllCommands()
        {
            return (ICollection<ChatCommand>)_commands;
        }

        public bool IsValidCommand(string input)
        {
            string[] words = input.Split(' ');
            if (words.Length >= 1)
            {
                return this._commands.Any(cc => cc.Command == words[0]);
            } else
            {
                return false;
            }
        }

        public async Task<string> GetResponseForCommand(string input, string uuid, string version)
        {
            string[] words = input.Split(' ');
            if (!IsValidCommand(input))
            {
                return "Error: Invalid Command specified\nPlease use `?help` for more information.";
            }

            return (words[0].ToLower()) switch
            {
                "?help" => HandleHelpCommand(),
                "?users" => await HandleUsersCommand(),
                "?teamcount" => await HandleTeamCountCommand(uuid),
                "?bots" => "Bot Count: 1",
                "?latestversion" => "Latest Version: 3.0.0",
                "?myversion" => $"You are currently running Gary Portal: {version}",
                "?joke" => await HandleJoke(),
                "?myroles" => await HandleMyRoles(uuid),
                "?mock" => HandleMock(input),
                "?roll" => HandleRoll(),
                "?8ball" => Handle8Ball(),
                "?stankrate" => HandleStankRate(input),
                "?simprate" => HandleSimpRate(input),
                "?globalban" => await HandleGlobalBan(input, uuid),
                "?chatban" => await HandleChatBan(input, uuid),
                "?feedban" => await HandleFeedBan(input, uuid),
                "?hitman" => await HandleHitman(input),
                "?gif" => await HandleGif(input),
                _ => "Error: Invalid Command specified\nPlease use `?help` for more information.",
            };
        }

        private string HandleHelpCommand()
        {
            string response = "--Al Murray Bot Information--\n";
            foreach (ChatCommand command in _commands)
            {
                response += $"{command.Command}: {command.CommandDescription}. Usage: {command.CommandUsage}\n";
            }
            return response;
        }

        private async Task<string> HandleUsersCommand()
        {
            ICollection<User> users = await _userService.GetAllAsync();
            return $"Current active users: {users.Count + 9}";
        }

        private async Task<string> HandleTeamCountCommand(string uuid)
        {
            User user = await _userService.GetByIdAsync(uuid);
            ICollection<User> users = await _userService.GetAllAsync(user.UserTeam.TeamId);
            return $"Current active users in your team: {users.Count}";
        }

        private async Task<string> HandleJoke()
        {
            Joke joke = await _staffService.GetRandomJoke();
            return $"{joke.setup}\n{joke.punchline}";
        }

        private async Task<string> HandleMyRoles(string uuid)
        {
            User user = await _userService.GetByIdAsync(uuid);
            return $"{user.UserFullName}:\nTeam: {user.UserTeam.Team.TeamName}\nAmigo Rank: {user.UserRanks.AmigoRank.RankName}\nPositive Rank: {user.UserRanks.PositivityRank.RankName}" +
                $"\nStaff: {(user.HasUserFlag("Role.Staff") ? "Yes": "No")}\nAdmin: {(user.HasUserFlag("Role.Admin") ? "Yes" : "No")}";
        }

        private static string HandleMock(string input)
        {
            Random random = new Random();
            List<string> words = input.Split(" ").ToList();
            words.RemoveAll(s => string.IsNullOrWhiteSpace(s));
            if (words.Count < 2 || string.IsNullOrWhiteSpace(words[1].Trim()))
            {
                return "Usage: ?mock [text]";
            }
            int index = input.IndexOf(" ") + 1;
            string result = string.Empty;
            input = input[index..];
            for (int i = 0; i < input.Length; i++)
            {
                result += random.NextDouble() >= 0.5 ? input[i].ToString().ToUpper() : input[i].ToString().ToLower();
            }
            return result;
        }

        private static string HandleRoll()
        {
            Random random = new Random();
            int roll = random.Next(1, 7);
            return $"I rolled a: {roll}";
        }

        private static string Handle8Ball()
        {
            string[] answers = new string[] { "Yes!", "No!", "Hell no!", "Hell yes!", "It is certain", "It is decidedly so", "Without a doubt", "You may rely on it", "Most likely!", "Outlook good!", "Reply hazy, try again!", "Ask again later", "Better not tell you now!", "Cannot predict now", "Concentrate and ask again", "Don't count on it", "My sources say no", "Outlook not so good", "Very doubtful", "Definitely not" };
            Random random = new Random();
            int index = random.Next(0, answers.Length);
            return answers[index];
        }

        private static string HandleStankRate(string input)
        {
            List<string> words = input.Split(" ").ToList();
            words.RemoveAll(s => string.IsNullOrWhiteSpace(s));
            Random random = new Random();
            int rate = random.Next(0, 101);
            string restOfWord = input[(input.IndexOf(" ") + 1)..];
            return $"Stank rating{(words.Count > 1 ? $" for {restOfWord}" : "")}: {rate}/100";
        }

        private static string HandleSimpRate(string input)
        {
            List<string> words = input.Split(" ").ToList();
            words.RemoveAll(s => string.IsNullOrWhiteSpace(s));
            Random random = new Random();
            int rate = random.Next(0, 101);
            string restOfWord = input[(input.IndexOf(" ") + 1)..];
            return $"Simp rating{(words.Count > 1 ? $" for {restOfWord}" : "")}: {rate}/100";
        }

        private async Task<string> HandleGlobalBan(string input, string uuid)
        {
            User user = await _userService.GetByIdAsync(uuid);
            if (!user.HasUserFlag("Role.Admin"))
            {
                return "Error: Only Admin+ Can use this command here, if you are a staff member, you can also use the Staff Room in the app to manager user's bans";
            }
            string[] words = input.Split(" ");
            if (words.Length < 2)
            {
                return "Error: Usage: ?globalban [username] {reason}";
            }

            string username = words[1];
            string reason = string.Join(" ", input.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()).Skip(2));
            string banUUID = await _userService.GetUUIDFromUsername(username);
            User banUser = await _userService.GetByIdAsync(banUUID);
            if (banUser == null)
            {
                return "Error: That user does not exist";
            }

            UserBan ban = new UserBan { UserBanId = 0, UserUUID = banUUID, BanIssued = DateTime.UtcNow, BanExpires = DateTime.UtcNow.AddDays(1), BanTypeId = 1, BanReason = reason, BannedByUUID = uuid };
            await _userService.BanUserAsync(ban);
            return "--Al Murray Bot--\nTemporary global ban applied on user, manage this users's bans directly in the staff room to modify this ban";
        }

        private async Task<string> HandleChatBan(string input, string uuid)
        {
            User user = await _userService.GetByIdAsync(uuid);
            if (!user.HasUserFlag("Role.Admin"))
            {
                return "Error: Only Admin+ Can use this command here, if you are a staff member, you can also use the Staff Room in the app to manager user's bans";
            }
            string[] words = input.Split(" ");
            if (words.Length < 2)
            {
                return "Error: Usage: ?chatban [username] {reason}";
            }

            string username = words[1];
            string reason = string.Join(" ", input.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()).Skip(2));
            string banUUID = await _userService.GetUUIDFromUsername(username);
            User banUser = await _userService.GetByIdAsync(banUUID);
            if (banUser == null)
            {
                return "Error: That user does not exist";
            }

            UserBan ban = new UserBan { UserBanId = 0, UserUUID = banUUID, BanIssued = DateTime.UtcNow, BanExpires = DateTime.UtcNow.AddDays(1), BanTypeId = 2, BanReason = reason, BannedByUUID = uuid };
            await _userService.BanUserAsync(ban);
            return "--Al Murray Bot--\nTemporary chat ban applied on user, manage this users's bans directly in the staff room to modify this ban";
        }

        private async Task<string> HandleFeedBan(string input, string uuid)
        {
            User user = await _userService.GetByIdAsync(uuid);
            if (!user.HasUserFlag("Role.Admin"))
            {
                return "Error: Only Admin+ Can use this command here, if you are a staff member, you can also use the Staff Room in the app to manager user's bans";
            }
            string[] words = input.Split(" ");
            if (words.Length < 2)
            {
                return "Error: Usage: ?feedban [username] {reason}";
            }

            string username = words[1];
            string reason = string.Join(" ", input.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()).Skip(2));
            string banUUID = await _userService.GetUUIDFromUsername(username);
            User banUser = await _userService.GetByIdAsync(banUUID);
            if (banUser == null)
            {
                return "Error: That user does not exist";
            }

            UserBan ban = new UserBan { UserBanId = 0, UserUUID = banUUID, BanIssued = DateTime.UtcNow, BanExpires = DateTime.UtcNow.AddDays(1), BanTypeId = 3, BanReason = reason, BannedByUUID = uuid };
            await _userService.BanUserAsync(ban);
            return "--Al Murray Bot--\nTemporary feed ban applied on user, manage this users's bans directly in the staff room to modify this ban";
        }

        private async Task<string> HandleHitman(string input)
        {
            input = string.Join(" ", input.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()));
            string[] words = input.Split(" ");
            if (words.Length < 2)
            {
                return "You hired a hitman, but didn't tell him who to target!";
            }
            User user = await _userService.GetByIdAsync(await _userService.GetUUIDFromUsername(words[1]));
            if (user == null)
            {
                return "You hired a hitman, but he couldn't find that target";
            }
            return $"You sent a hitman after {user.UserFullName}, only time will tell if they succeed";
        }

        private static async Task<string> HandleGif(string input)
        {
            input = string.Join(" ", input.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()));
            string[] words = input.Split(" ");
            if (words.Length < 2)
            {
                return "";
            }
            string query = string.Join(" ", input.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim()).Skip(1));
            RestClient client = new RestClient("https://g.tenor.com/");
            RestRequest request = new RestRequest("v1/search", Method.GET);
            request.AddQueryParameter("q", query);
            request.AddParameter("key", "3U4OXKNOBIM0");
            request.AddParameter("limit", 1);
            var response = await client.ExecuteAsync<GifResponse>(request);
            string responseString = response.Content;
            GifResponse gifResponse = JsonConvert.DeserializeObject<GifResponse>(responseString);
            return gifResponse.results.FirstOrDefault().media.FirstOrDefault().gif.url;
        }

        public void Dispose()
        {
            
        }
    }
}

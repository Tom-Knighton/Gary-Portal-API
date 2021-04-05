using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CorePush.Apple;
using GaryPortalAPI.Data;
using GaryPortalAPI.Models;
using GaryPortalAPI.Services.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GaryPortalAPI.Services
{
    public interface IUserService : IDisposable
    {
        Task<ICollection<User>> GetAllAsync(int teamId = 0, bool includeQueued = false, CancellationToken ct = default);
        Task<ICollection<User>> GetAllQueuedAsync(CancellationToken ct = default);
        Task<User> GetByIdAsync(string userUUID, CancellationToken ct = default);
        Task<string> GetUUIDFromUsername(string username, CancellationToken ct = default);
        Task<string> GetUUIDFromEmail(string email, CancellationToken ct = default);
        Task<bool> IsUsernameFreeAsync(string username, CancellationToken ct = default);
        Task<bool> IsEmailFreeAsync(string email, CancellationToken ct = default);
        Task<UserPoints> GetPointsForUserAsync(string userUUID, CancellationToken ct = default);
        Task<UserPoints> UpdatePointsForUserAsync(string userUUID, UserPoints points, CancellationToken ct = default);
        Task ClearAllPrayersAsync(CancellationToken ct = default);
        Task ClearPrayersForUserAsync(string uuid, CancellationToken ct = default);
        Task<User> UpdateUserDetailsAsync(string uuid, UserDetails details, CancellationToken ct = default);
        Task UpdateBioForUserAsync(string uuid, string bio, CancellationToken ct = default);
        Task<User> StaffManageUserDetailsAsync(string uuid, StaffManagedUserDetails details, CancellationToken ct = default);
        Task<string> UpdateUserProfilePictureAsync(string uuid, IFormFile file, CancellationToken ct = default);
        Task<User> CreateNewUserAsync(UserRegistration creatingUser, CancellationToken ct = default);
        Task<ICollection<UserBan>> GetUserCurrentBansAsync(string uuid, CancellationToken ct = default);
        Task<bool> IsUserBannedAsync(string uuid, CancellationToken ct = default);
        Task<UserBan> GetFirstBanOfTypeIfAnyAsnc(string uuid, int banTypeId = 1, CancellationToken ct = default);
        Task<UserBan> BanUserAsync(UserBan ban, CancellationToken ct = default);
        Task RevokeBanAsync(int userBanId, CancellationToken ct = default);
        Task<UserBlock> BlockUserAsync(string uuid, string blockedUUID, CancellationToken ct = default);
        Task UnblockUserAsync(string uuid, string blockedUUID, CancellationToken ct = default);
        Task<ICollection<UserBlock>> GetAllBlocksAsync(string uuid, CancellationToken ct = default);
        Task ReportUserAsync(UserReport report, CancellationToken ct = default);
        Task MarkReportAsDeletedAsync(int reportId, CancellationToken ct = default);
        Task AddAPNS(string uuid, string apns);
        Task<ICollection<string>> GetAPNSFromUUIDAsync(string uuid, CancellationToken ct = default);
        Task PostNotification(string apns, Notification notification);


    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IHashingService _hashingService;
        private readonly ApiSettings _apiSettings;

        public UserService(AppDbContext context, IHashingService hashingService, IOptions<ApiSettings> settings)
        {
            _context = context;
            _hashingService = hashingService;
            _apiSettings = settings.Value;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<ICollection<User>> GetAllAsync(int teamId = 0, bool includeQueue = false, CancellationToken ct = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.UserAuthentication)
                .Include(u => u.UserPoints)
                .Include(u => u.UserRanks)
                    .ThenInclude(u => u.AmigoRank)
                .Include(u => u.UserRanks)
                    .ThenInclude(u => u.PositivityRank)
                .Include(u => u.UserTeam)
                    .ThenInclude(ut => ut.Team)
                .Include(u => u.UserBans.Where(ub => ub.BanExpires > DateTime.UtcNow))
                    .ThenInclude(ub => ub.BanType)
                .Where(u => !u.IsDeleted && (teamId == 0 || u.UserTeam.TeamId == teamId) && (includeQueue || !u.IsQueued))
                .ToListAsync(ct);
        }

        public async Task<ICollection<User>> GetAllQueuedAsync(CancellationToken ct = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.UserAuthentication)
                .Include(u => u.UserPoints)
                .Include(u => u.UserRanks)
                    .ThenInclude(u => u.AmigoRank)
                .Include(u => u.UserRanks)
                    .ThenInclude(u => u.PositivityRank)
                .Include(u => u.UserTeam)
                    .ThenInclude(ut => ut.Team)
                .Include(u => u.UserBans.Where(ub => ub.BanExpires > DateTime.UtcNow))
                    .ThenInclude(ub => ub.BanType)
                .Where(u => !u.IsDeleted && u.IsQueued)
                .ToListAsync(ct);
        }

        public async Task<User> GetByIdAsync(string userUUID, CancellationToken ct = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.UserAuthentication)
                .Include(u => u.UserPoints)
                .Include(u => u.UserRanks)
                    .ThenInclude(u => u.AmigoRank)
                .Include(u => u.UserRanks)
                    .ThenInclude(u => u.PositivityRank)
                .Include(u => u.UserTeam)
                    .ThenInclude(ut => ut.Team)
                .Include(u => u.BlockedUsers.Where(bu => bu.IsBlocked))
                .Include(u => u.UserBans.Where(ub => ub.BanExpires > DateTime.UtcNow))
                    .ThenInclude(ub => ub.BanType)
                .FirstOrDefaultAsync(u => u.UserUUID == userUUID, ct);
        }

        public async Task<string> GetUUIDFromUsername(string username, CancellationToken ct = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.UserName.ToLower() == username.ToLower())
                .Select(u => u.UserUUID)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<string> GetUUIDFromEmail(string email, CancellationToken ct = default)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.UserAuthentication)
                .Where(u => u.UserAuthentication.UserEmail.ToLower() == email.ToLower())
                .Select(u => u.UserUUID)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<bool> IsEmailFreeAsync(string email, CancellationToken ct = default)
        {
            return await _context.UserAuthentications.AsNoTracking().FirstOrDefaultAsync(u => u.UserEmail == email, ct) == null;
        }

        public async Task<bool> IsUsernameFreeAsync(string username, CancellationToken ct = default)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserName.ToLower() == username.ToLower(), ct) == null;
        }

        public async Task<UserPoints> GetPointsForUserAsync(string userUUID, CancellationToken ct = default)
        {
            return await _context.UserPoints.FindAsync(userUUID);
        }



        public async Task<UserPoints> UpdatePointsForUserAsync(string uuid, UserPoints points, CancellationToken ct = default)
        {
            UserPoints userpoints = await _context.UserPoints
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserUUID == uuid);
            if (userpoints != null)
            {
                userpoints = points;
                _context.Update(userpoints);
                await _context.SaveChangesAsync();
            }

            return userpoints ?? null;
        }

        public async Task ClearAllPrayersAsync(CancellationToken ct = default)
        {
            await _context.UserPoints.ForEachAsync(up =>
            {
                up.Prayers = 0;
                up.MeaningfulPrayers = 0;
            });
            await _context.SaveChangesAsync();
        }

        public async Task ClearPrayersForUserAsync(string uuid, CancellationToken ct = default)
        {
            UserPoints userpoints = await _context.UserPoints.FindAsync(uuid);
            userpoints.Prayers = 0;
            userpoints.MeaningfulPrayers = 0;
            await _context.SaveChangesAsync();
        }

        public async Task<User> UpdateUserDetailsAsync(string uuid, UserDetails details, CancellationToken ct = default)
        {
            User user = await GetByIdAsync(uuid, ct);
            if (user == null) return null;

            user.UserAuthentication.UserEmail = details.UserEmail;
            user.UserName = details.UserName.ToLower();
            user.UserFullName = details.FullName;
            user.UserProfileImageUrl = details.ProfilePictureUrl;
            user.NotificationsMuted = details.NotificationsMuted;
            _context.Update(user);

            await _context.SaveChangesAsync(ct);
            return user;
        }

        public async Task UpdateBioForUserAsync(string uuid, string bio, CancellationToken ct = default)
        {
            User user = await _context.Users.FirstOrDefaultAsync(u => u.UserUUID == uuid, ct);
            user.UserBio = bio;
            await _context.SaveChangesAsync(ct);
        }

        public async Task<User> StaffManageUserDetailsAsync(string uuid, StaffManagedUserDetails details, CancellationToken ct = default)
        {
            User user = await GetByIdAsync(uuid, ct);
            user.UserName = details.UserName.ToLower();
            user.UserSpanishName = details.SpanishName;
            user.UserProfileImageUrl = details.ProfilePictureUrl;
            user.UserPoints.AmigoPoints = details.AmigoPoints;
            user.UserPoints.PositivityPoints = details.PositivePoints;

            if (user.IsQueued != details.IsQueued)
            {
                user.IsQueued = details.IsQueued;
                if (details.IsQueued == false)
                {
                    ICollection<string> apns = await GetAPNSFromUUIDAsync(user.UserUUID);
                    Notification notification = Notification.CreateNotification(new APSAlert { body = "Your account has been activated! Have fun Garrying" });
                    foreach (string token in apns)
                    {
                        await PostNotification(token, notification);
                    }
                }
            }

            await UpdatePointsForUserAsync(uuid, user.UserPoints, ct);

            if (user.UserRanks != null)
            {
                _context.UserRanks.Remove(user.UserRanks);
            }
            if (user.UserTeam != null)
            {
                _context.UserTeams.Remove(user.UserTeam);
            }

            user.UserRanks = new UserRanks
            {
                AmigoRankId = details.AmigoRankId,
                PositivtyRankId = details.PositiveRankId
            };
            user.UserTeam = new UserTeam
            {
                TeamId = details.TeamId
            };


            _context.Update(user);
            await _context.SaveChangesAsync(ct);
            return await GetByIdAsync(uuid, ct);
        }

        public async Task<string> UpdateUserProfilePictureAsync(string uuid, IFormFile file, CancellationToken ct = default)
        {
            if (file == null) return null;

            User user = await _context.Users.FindAsync(uuid);
            Directory.CreateDirectory($"/var/www/cdn/GaryPortal/ProfilePics/{uuid}");
            var fileName = $"{Guid.NewGuid():N}.jpg";
            var filePath = $"/var/www/cdn/GaryPortal/ProfilePics/{uuid}/{fileName}";
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            string newProfileUrl = $"https://cdn.tomk.online/GaryPortal/ProfilePics/{uuid}/{fileName}";
            user.UserProfileImageUrl = newProfileUrl;
            await _context.SaveChangesAsync();
            return newProfileUrl;
        }

        public async Task<User> CreateNewUserAsync(UserRegistration creatingUser, CancellationToken ct = default)
        {
            if (await IsEmailFreeAsync(creatingUser.UserEmail, ct) && await IsUsernameFreeAsync(creatingUser.UserName, ct))
            {
                string newUUID = Guid.NewGuid().ToString("N");
                Tuple<string, string> hashedPassValues = _hashingService.NewHashAndSalt(creatingUser.UserPassword);
                User newUser = new User
                {
                    UserUUID = newUUID,
                    UserName = creatingUser.UserName,
                    UserFullName = creatingUser.UserFullName,
                    UserBio = "",
                    UserQuote = "",
                    UserSpanishName = "",
                    UserStanding = "",
                    UserProfileImageUrl = "",
                    UserIsAdmin = false,
                    UserIsStaff = false,
                    IsQueued = true,
                    UserGender = creatingUser.UserGender,
                    UserDateOfBirth = creatingUser.UserDOB,
                    NotificationsMuted = false,
                    UserAuthentication = new UserAuthentication
                    {
                        UserEmail = creatingUser.UserEmail,
                        UserEmailConfirmed = false,
                        UserPassHash = hashedPassValues.Item1,
                        UserPassSalt = hashedPassValues.Item2
                    },
                    UserPoints = new UserPoints
                    {
                        AmigoPoints = 0,
                        BowelsRelieved = 0,
                        MeaningfulPrayers = 0,
                        PositivityPoints = 0,
                        Prayers = 0
                    },
                    UserRanks = new UserRanks
                    {
                        AmigoRankId = 1,
                        PositivtyRankId = 1
                    }
                };
                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();
                User user = await GetByIdAsync(newUser.UserUUID, ct);
                return user;
            }

            return null;
        }

        public async Task<ICollection<UserBan>> GetUserCurrentBansAsync(string uuid, CancellationToken ct = default)
        {
            return await _context.UserBans.Where(ub => ub.UserUUID.Equals(uuid) && ub.BanExpires > DateTime.UtcNow).ToListAsync(ct);
        }

        public async Task<bool> IsUserBannedAsync(string uuid, CancellationToken ct = default)
        {
            return (await GetUserCurrentBansAsync(uuid, ct)).Count != 0;
        }

        public async Task<UserBan> GetFirstBanOfTypeIfAnyAsnc(string uuid, int banTypeId = 1, CancellationToken ct = default)
        {
            return await _context.UserBans.Where(ub => ub.UserUUID.Equals(uuid) && ub.BanExpires > DateTime.UtcNow && ub.BanTypeId == banTypeId).FirstOrDefaultAsync();

        }

        public async Task<UserBan> BanUserAsync(UserBan ban, CancellationToken ct = default)
        {
            await _context.UserBans.AddAsync(ban, ct);
            await _context.SaveChangesAsync(ct);
            return ban;
        }

        public async Task RevokeBanAsync(int userBanId, CancellationToken ct = default)
        {
            UserBan ban = await _context.UserBans.FindAsync(userBanId);
            if (ban != null)
            {
                ban.BanExpires = DateTime.UtcNow.AddHours(-3);
                _context.Update(ban);
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<UserBlock> BlockUserAsync(string uuid, string blockedUUID, CancellationToken ct = default)
        {

            if (await _context.UserBlocks.FindAsync(uuid, blockedUUID) != null)
            {
                UserBlock block = await _context.UserBlocks.FindAsync(uuid, blockedUUID);
                block.IsBlocked = true;
                _context.Update(block);
                await _context.SaveChangesAsync(ct);
                return block;
            }
            else
            {
                UserBlock block = new UserBlock
                {
                    BlockerUserUUID = uuid,
                    BlockedUserUUID = blockedUUID,
                    IsBlocked = true
                };

                await _context.AddAsync(block, ct);
                await _context.SaveChangesAsync(ct);
                return block;
            }         
        }

        public async Task UnblockUserAsync(string uuid, string blockedUUID, CancellationToken ct = default)
        {
            UserBlock block = await _context.UserBlocks.FindAsync(uuid, blockedUUID);
            if (block != null)
            {
                block.IsBlocked = false;
                _context.Update(block);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ICollection<UserBlock>> GetAllBlocksAsync(string uuid, CancellationToken ct = default)
        {
            ICollection<UserBlock> blocks = await _context.UserBlocks
                .Where(ub => ub.BlockerUserUUID == uuid && ub.IsBlocked)
                .Include(ub => ub.BlockedUser)
                .AsNoTracking()
                .ToListAsync(ct);

            foreach (UserBlock block in blocks)
            {
                block.BlockedUserDTO = block.BlockedUser.ConvertToDTO();
                block.BlockedUser = null;
            }
            return blocks;
        }

        public async Task ReportUserAsync(UserReport report, CancellationToken ct = default)
        {
            await _context.UserReports.AddAsync(report);
            await _context.SaveChangesAsync(ct);
        }

        public async Task MarkReportAsDeletedAsync(int reportId, CancellationToken ct = default)
        {
            UserReport report = await _context.UserReports.FindAsync(reportId);
            if (report != null)
            {
                report.IsDeleted = true;
                _context.Update(report);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddAPNS(string uuid, string apns)
        {
            UserAPNS newAPNS = new UserAPNS { UserAPNSId = 0, UserUUID = uuid, APNSToken = apns };
            UserAPNS existingAPNS = await _context.UserAPNS.Where(t => t.APNSToken == apns).FirstOrDefaultAsync();
            if (existingAPNS == null)
            {
                await _context.UserAPNS.AddAsync(newAPNS);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ICollection<string>> GetAPNSFromUUIDAsync(string uuid, CancellationToken ct = default)
        {
            if (await _context.Users.Where(u => u.UserUUID == uuid).Select(u => u.NotificationsMuted == false).FirstOrDefaultAsync(ct))
            {
                return await _context.UserAPNS.Where(u => u.UserUUID == uuid).Select(u => u.APNSToken).ToListAsync(ct);
            }
            return null;
        }

        public async Task PostNotification(string apns, Notification notification)
        {
            ApnSettings apnSettings = _apiSettings.APNSSettings.ConvertToAPNSettings();
            var apn = new ApnSender(apnSettings, new System.Net.Http.HttpClient());
            await apn.SendAsync(notification, apns);
        }
    }
}

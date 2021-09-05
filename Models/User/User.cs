using System;
using System.Collections.Generic;
using System.Linq;
using GaryPortalAPI.Models;
using GaryPortalAPI.Models.Feed;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GaryPortalAPI.Models
{

    public class UserDTO
    {
        public string UserUUID { get; set; }
        public string UserFullName { get; set; }
        public string UserProfileImageUrl { get; set; }
        public bool UserIsStaff { get; set; }
        public bool UserIsAdmin { get; set; }
    }

    public class UserDetails
    {
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string FullName { get; set; }
        public string ProfilePictureUrl { get; set; }
        public bool NotificationsMuted { get; set; }
    }

    public class StaffManagedUserDetails
    {
        public string UserName { get; set; }
        public string SpanishName { get; set; }
        public string ProfilePictureUrl { get; set; }
        public int TeamId { get; set; }
        public int AmigoPoints { get; set; }
        public int PositivePoints { get; set; }
        public int AmigoRankId { get; set; }
        public int PositiveRankId { get; set; }
        public bool IsQueued { get; set; }
    }

    public class User
    {
        public string UserUUID { get; set; }
        public string UserFullName { get; set; }
        public string UserSpanishName { get; set; }
        public string UserName { get; set; }
        public string UserProfileImageUrl { get; set; }
        public string UserQuote { get; set; }
        public string UserBio { get; set; }
        public string UserGender { get; set; }
        public string UserStanding { get; set; }
        public DateTime UserCreatedAt { get; set; }
        public DateTime UserDateOfBirth { get; set; }
        public bool IsDeleted { get; set; }

        [Obsolete("Please use the UserFlag system instead (HasUserFlag())")]
        public bool UserIsStaff => HasUserFlag("Role.Staff");
        [Obsolete("Please use the UserFlag system instead (HasUserFlag())")]
        public bool UserIsAdmin => HasUserFlag("Role.Admin");
        [Obsolete("Please use the UserFlag system instead (HasUserFlag())")]
        public bool IsQueued => HasUserFlag("IsInQueue");

        public UserAuthenticationTokens UserAuthTokens { get; set; }

        public virtual UserAuthentication UserAuthentication { get; set; }
        public virtual UserTeam UserTeam { get; set; }
        public virtual UserRanks UserRanks { get; set; }
        public virtual UserPoints UserPoints { get; set; }
        public virtual ICollection<UserBan> UserBans { get; set; }
        public virtual ICollection<UserBan> UsersBannedByMeAsPrivileged { get; set; }
        public virtual ICollection<UserBlock> BlockedUsers { get; set; }

        public virtual ICollection<UserRefreshToken> UserRefreshTokens { get; set; }
        public virtual ICollection<FeedPost> FeedPosts { get; set; }
        public virtual ICollection<FeedAnswerVote> FeedVotes { get; set; }
        public virtual ICollection<FeedLike> FeedLikes { get; set; }
        public virtual ICollection<FeedComment> FeedComments { get; set; }
        public virtual ICollection<AditLog> AditLogs { get; set; }

        public virtual ICollection<UserAPNS> APNSTokens { get; set; }

        public virtual ICollection<UserFlag> UserFlags { get; set; }

        public virtual UserDTO ConvertToDTO()
        {
            return new UserDTO
            {
                UserUUID = UserUUID,
                UserFullName = UserFullName,
                UserIsAdmin = HasUserFlag("Role.Admin"),
                UserIsStaff = HasUserFlag("Role.Staff"),
                UserProfileImageUrl = UserProfileImageUrl,
            };
        }

        public bool HasUserFlag(string flagName)
        {
            return UserFlags.Any(uf => uf.Flag.FlagName.ToLower() == flagName.ToLower() && uf.FlagEnabled && !uf.Flag.FlagIsDeleted);
        }
    }

    public class Flag
    {
        public int FlagId { get; set; }
        public string FlagName { get; set; }
        public int FlagAccessLevel { get; set; }
        public bool FlagIsDeleted { get; set; }
    }

    public class UserFlag
    {
        public string UserUUID { get; set; }
        public int FlagId { get; set; }
        public bool FlagEnabled { get; set; }

        public virtual Flag Flag { get; set; }
    }

    public class UserTeam
    {
        public string UserUUID { get; set; }
        public int TeamId { get; set; }

        public virtual Team Team { get; set; }
        public virtual User User { get; set; }
    }

    public class UserRanks
    {
        public string UserUUID { get; set; }
        public int AmigoRankId { get; set; }
        public int PositivtyRankId { get; set; }

        public virtual Rank AmigoRank { get; set; }
        public virtual Rank PositivityRank { get; set; }
        public virtual User User { get; set; }
    }

    public class UserPoints
    {
        public string UserUUID { get; set; }
        public int AmigoPoints { get; set; }
        public int PositivityPoints { get; set; }
        public int BowelsRelieved { get; set; }
        public int Prayers { get; set; }
        public int MeaningfulPrayers { get; set; }

        public virtual User User { get; set; }
    }

    public class UserBan
    {
        public int UserBanId { get; set; }
        public string UserUUID { get; set; }
        public DateTime BanIssued { get; set; }
        public DateTime BanExpires { get; set; }
        public int BanTypeId { get; set; }
        public string BanReason { get; set; }
        public string BannedByUUID { get; set; }

        public virtual User BannedUser { get; set; }
        public virtual BanType BanType { get; set; }
        public virtual User BannedBy { get; set; }
    }

    public class BanType
    {
        public int BanTypeId { get; set; }
        public string BanTypeName { get; set; }

        public virtual ICollection<UserBan> UserBans { get; set; }
    }

    public class UserBlock
    {
        public string BlockerUserUUID { get; set; }
        public string BlockedUserUUID { get; set; }
        public bool IsBlocked { get; set; }

        public virtual User BlockerUser { get; set; }
        public virtual User BlockedUser { get; set; }
        public virtual UserDTO BlockedUserDTO { get; set; }
    }

    public class UserAPNS
    {
        public int UserAPNSId{ get; set; }
        public string UserUUID { get; set; }
        public string APNSToken { get; set; }
    }
}
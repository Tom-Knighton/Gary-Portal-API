using System;
using GaryPortalAPI.Models;
using GaryPortalAPI.Models.Chat;
using GaryPortalAPI.Models.Feed;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


    public DbSet<User> Users { get; set; }
    public DbSet<Rank> Ranks { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Flag> Flags { get; set; }

    public DbSet<UserAuthentication> UserAuthentications { get; set; }
    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
    public DbSet<UserPoints> UserPoints { get; set; }
    public DbSet<UserRanks> UserRanks { get; set; }
    public DbSet<UserTeam> UserTeams { get; set; }
    public DbSet<UserBlock> UserBlocks { get; set; }
    public DbSet<UserAPNS> UserAPNS { get; set; }
    public DbSet<UserFlag> UserFlags { get; set; }

    public DbSet<UserAuthenticationConfirmation> UserAuthConfirmations { get; set; }
    public DbSet<UserPassResetToken> UserPassResetTokens { get; set; }

    public DbSet<BanType> BanTypes { get; set; }
    public DbSet<UserBan> UserBans { get; set; }

    public DbSet<UserReport> UserReports { get; set; }
    public DbSet<FeedReport> FeedReports { get; set; }
    public DbSet<ChatMessageReport> ChatMessageReports { get; set; }

    public DbSet<FeedPost> FeedPosts { get; set; }
    public DbSet<FeedMediaPost> FeedMediaPosts { get; set; }
    public DbSet<FeedPollPost> FeedPollPosts { get; set; }
    public DbSet<FeedLike> FeedPostLikes { get; set; }
    public DbSet<FeedComment> FeedPostComments { get; set; }
    public DbSet<FeedAnswerVote> FeedAnswerVotes { get; set; }
    public DbSet<FeedPollAnswer> FeedPollAnswers { get; set; }
    public DbSet<AditLog> FeedAditLogs { get; set; }

    public DbSet<StaffRoomAnnouncement> StaffRoomAnnouncements { get; set; }

    public DbSet<Chat> Chats { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<ChatMember> ChatMembers { get; set; }
    public DbSet<ChatMessageType> ChatMessageTypes { get; set; }

    public DbSet<Sticker> Stickers { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Commandment> Commandments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region User

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.UserUUID);
            entity.Ignore(u => u.UserAuthTokens);

            entity
                .HasOne(u => u.UserAuthentication)
                .WithOne(ua => ua.User)
                .HasForeignKey<UserAuthentication>(ua => ua.UserUUID)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity
                .HasMany(u => u.UserRefreshTokens)
                .WithOne(urt => urt.User)
                .HasForeignKey(urt => urt.UserUUID)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity
                .HasOne(u => u.UserPoints)
                .WithOne(u => u.User)
                .HasForeignKey<UserPoints>(up => up.UserUUID)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity
                .HasOne(u => u.UserRanks)
                .WithOne(ur => ur.User)
                .HasForeignKey<UserRanks>(ur => ur.UserUUID)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity
                .HasMany(u => u.UserBans)
                .WithOne(b => b.BannedUser)
                .HasForeignKey(b => b.UserUUID)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity
                .HasMany(u => u.UsersBannedByMeAsPrivileged)
                .WithOne(b => b.BannedBy)
                .HasForeignKey(b => b.BannedByUUID)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity
                .HasMany(u => u.BlockedUsers)
                .WithOne(bu => bu.BlockerUser)
                .HasForeignKey(bu => bu.BlockerUserUUID)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity
                .HasOne(u => u.UserTeam)
                .WithOne(ut => ut.User)
                .HasForeignKey<UserTeam>(ut => ut.UserUUID)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity
                .HasMany(u => u.APNSTokens)
                .WithOne()
                .HasForeignKey(apns => apns.UserUUID)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity
                .HasMany(u => u.UserFlags)
                .WithOne()
                .HasForeignKey(uf => uf.UserUUID)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity.Ignore(u => u.UserIsStaff);
            entity.Ignore(u => u.UserIsAdmin);
            entity.Ignore(u => u.IsQueued);
        });

        modelBuilder.Entity<UserAuthentication>(entity =>
        {
            entity.HasKey(ua => ua.UserUUID);
        });

        modelBuilder.Entity<UserAuthenticationConfirmation>(entity =>
        {
            entity.HasKey(uac => new { uac.UserUUID, uac.UserConfirmationHash });
            entity
                .HasOne(uac => uac.User)
                .WithMany()
                .HasForeignKey(uac => uac.UserUUID)
                .IsRequired();
        });

        modelBuilder.Entity<UserPassResetToken>(entity =>
        {
            entity.HasKey(uac => new { uac.UserUUID, uac.UserResetHash });
            entity
                .HasOne(uac => uac.User)
                .WithMany()
                .HasForeignKey(uac => uac.UserUUID)
                .IsRequired();
        });

        modelBuilder.Entity<UserRefreshToken>(entity =>
        {
            entity.HasKey(urt => new { urt.UserUUID, urt.RefreshToken });
        });

        modelBuilder.Entity<UserRanks>(entity =>
        {
            entity.HasKey(ur => ur.UserUUID);
            entity
                .HasOne(ur => ur.AmigoRank)
                .WithMany(r => r.UserAmigoRanks)
                .HasForeignKey(ur => ur.AmigoRankId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity
                .HasOne(ur => ur.PositivityRank)
                .WithMany(r => r.UserPositivityRanks)
                .HasForeignKey(ur => ur.PositivtyRankId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();      
        });

        modelBuilder.Entity<UserPoints>(entity =>
        {
            entity.HasKey(up => up.UserUUID);
        });

        modelBuilder.Entity<UserTeam>(entity =>
        {
            entity.HasKey(ut => ut.UserUUID);
        });

        modelBuilder.Entity<UserBan>(entity =>
        {
            entity
                .HasOne(b => b.BanType)
                .WithMany(bt => bt.UserBans)
                .HasForeignKey(b => b.BanTypeId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity<UserBlock>(entity =>
        {
            entity.HasKey(ub => new { ub.BlockerUserUUID, ub.BlockedUserUUID } );
            entity
                .HasOne(ub => ub.BlockedUser)
                .WithMany()
                .HasForeignKey(ub => ub.BlockedUserUUID)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            entity.Ignore(ub => ub.BlockedUserDTO);
        });

        modelBuilder.Entity<Flag>(entity =>
        {
            entity.Property(f => f.FlagId)
                .ValueGeneratedOnAdd();
            entity.HasKey(f => f.FlagId);
        });

        modelBuilder.Entity<UserFlag>(entity =>
        {
            entity.HasKey(uf => new { uf.FlagId, uf.UserUUID });
            entity
                .HasOne(uf => uf.Flag)
                .WithMany()
                .HasForeignKey(uf => uf.FlagId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        #endregion User

        #region Feed

        modelBuilder.Entity<FeedPost>(entity =>
        {
            entity.Property(fp => fp.PostId)
                .ValueGeneratedOnAdd();
            entity.HasKey(fp => fp.PostId);
            entity.Ignore(fp => fp.PosterDTO);
            entity.HasDiscriminator(fp => fp.PostType)
                .HasValue<FeedPost>("base")
                .HasValue<FeedMediaPost>("media")
                .HasValue<FeedPollPost>("poll");

            entity
                .HasOne(fp => fp.Poster)
                .WithMany(u => u.FeedPosts)
                .HasForeignKey(fp => fp.PosterUUID)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity
                .HasOne(fp => fp.PostTeam)
                .WithMany(ut => ut.FeedPosts)
                .HasForeignKey(fp => fp.TeamId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity
                .HasMany(fp => fp.Likes)
                .WithOne(l => l.Post)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity
                .HasMany(fp => fp.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .IsRequired();
        });

        modelBuilder.Entity<FeedPollPost>(entity =>
        {
            entity
                .HasMany(fpp => fpp.PollAnswers)
                .WithOne(pa => pa.Poll)
                .HasForeignKey(pa => pa.PollId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity<FeedPollAnswer>(entity =>
        {
            entity.Property(fpa => fpa.PollAnswerId)
                .ValueGeneratedOnAdd();
            entity.HasKey(fpa => fpa.PollAnswerId);
            entity
                .HasMany(fpa => fpa.Votes)
                .WithOne(fav => fav.PollAnswer)
                .HasForeignKey(fav => fav.PollAnswerId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity<FeedAnswerVote>(entity =>
        {
            entity.HasKey(fav => new { fav.PollAnswerId, fav.UserUUID });
            entity.Ignore(fav => fav.UserDTO);
            entity
                .HasOne(fav => fav.User)
                .WithMany(u => u.FeedVotes)
                .HasForeignKey(fav => fav.UserUUID)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity<FeedLike>(entity =>
        {
            entity.HasKey(fl => new { fl.PostId, fl.UserUUID });
            entity.Ignore(fl => fl.UserDTO);
            entity
                   .HasOne(fl => fl.User)
                   .WithMany(u => u.FeedLikes)
                   .HasForeignKey(fl => fl.UserUUID)
                   .OnDelete(DeleteBehavior.Cascade)
                   .IsRequired();

        });

        modelBuilder.Entity<FeedComment>(entity =>
        {
            entity.Property(fc => fc.FeedCommentId)
                .ValueGeneratedOnAdd();
            entity.Ignore(fc => fc.UserDTO);
            entity
                   .HasOne(fc => fc.User)
                   .WithMany(u => u.FeedComments)
                   .HasForeignKey(fc => fc.UserUUID)
                   .OnDelete(DeleteBehavior.Cascade)
                   .IsRequired();

        });

        modelBuilder.Entity<AditLog>(entity =>
        {
            entity.Property(al => al.AditLogId)
                .ValueGeneratedOnAdd();
            entity.Ignore(al => al.PosterDTO);
            entity
                .HasOne(al => al.Poster)
                .WithMany(p => p.AditLogs)
                .HasForeignKey(al => al.PosterUUID)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            entity
                .HasOne(al => al.AditLogTeam)
                .WithMany(t => t.AditLogs)
                .HasForeignKey(al => al.AditLogTeamId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        #endregion

        #region Reports

        modelBuilder.Entity<UserReport>(entity =>
        {
            entity
                .HasOne(ur => ur.ReportedUser)
                .WithMany()
                .HasForeignKey(ur => ur.UserUUID)
                .IsRequired();

            entity
                .HasOne(ur => ur.Reporter)
                .WithMany()
                .HasForeignKey(ur => ur.ReportByUUID)
                .IsRequired();
        });

        modelBuilder.Entity<FeedReport>(entity =>
        {
            entity
                .HasOne(fr => fr.ReportedPost)
                .WithMany()
                .HasForeignKey(fr => fr.FeedPostId)
                .IsRequired();
            entity
                .HasOne(fr => fr.Reporter)
                .WithMany()
                .HasForeignKey(fr => fr.ReportByUUID)
                .IsRequired();
        });

        modelBuilder.Entity<ChatMessageReport>(entity =>
        {
            entity
                .HasOne(cmr => cmr.ReportedMessage)
                .WithMany()
                .HasForeignKey(cmr => cmr.ChatMessageUUID)
                .IsRequired();

            entity
                .HasOne(cmr => cmr.Reporter)
                .WithMany()
                .HasForeignKey(cmr => cmr.ReportByUUID)
                .IsRequired();
        });

        #endregion

        #region Staff Room

        modelBuilder.Entity<StaffRoomAnnouncement>(entity =>
        {
            entity.HasKey(sra => sra.AnnouncementId);
            entity.Property(sra => sra.AnnouncementId)
                .ValueGeneratedOnAdd();

            entity
                .HasOne(sra => sra.User)
                .WithMany()
                .HasForeignKey(sra => sra.UserUUID);
            entity.Ignore(sra => sra.UserDTO);
        });

        #endregion

        #region Chat

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(c => c.ChatUUID);
            entity
                .HasMany(c => c.ChatMembers)
                .WithOne(cm => cm.Chat)
                .HasForeignKey(cm => cm.ChatUUID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity.Ignore(c => c.LastChatMessage);
        });

        modelBuilder.Entity<ChatMember>(entity =>
        {
            entity.HasKey(cm => new { cm.ChatUUID, cm.UserUUID });
            entity
                .HasOne(cm => cm.User)
                .WithMany()
                .HasForeignKey(cm => cm.UserUUID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(cm => cm.Chat)
                .WithMany(c => c.ChatMembers)
                .HasForeignKey(cm => cm.ChatUUID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity.Ignore(cm => cm.UserDTO);
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(cm => cm.ChatMessageUUID);
            entity
                .HasOne(cm => cm.Chat)
                .WithMany(c => c.ChatMessages)
                .HasForeignKey(cm => cm.ChatUUID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(cm => cm.User)
                .WithMany()
                .HasForeignKey(cm => cm.UserUUID)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(cm => cm.ChatMessageType)
                .WithMany()
                .HasForeignKey(cm => cm.MessageTypeId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            entity
                .HasOne(cm => cm.ReplyingTo)
                .WithMany()
                .HasForeignKey(cm => cm.ReplyingToUUID);

            entity.Ignore(cm => cm.UserDTO);
            entity.Ignore(cm => cm.ReplyingToDTO);
        });

        #endregion

        #region Misc

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasOne(e => e.EventTeam).WithMany(t => t.TeamEvents).HasForeignKey(e => e.EventTeamId).OnDelete(DeleteBehavior.Cascade);
        });

        #endregion
    }
}
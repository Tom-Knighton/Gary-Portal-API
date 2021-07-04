using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using GaryPortalAPI.Data;
using GaryPortalAPI.Models;
using GaryPortalAPI.Models.Feed;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace GaryPortalAPI.Services
{
    public interface IFeedService : IDisposable
    {
        Task<ICollection<FeedPost>> GetAllAsync(long startfrom, int teamId = 0, int limit = 10, bool includeComments = false, CancellationToken ct = default);
        Task<ICollection<FeedPostDTO>> GetAllDTOPostsForUserAsync(string uuid, CancellationToken ct = default);
        Task<FeedPost> GetByIdAsync(int feedPostId, CancellationToken ct = default);
        Task ToggleLikeForPostAsync(int feedPostId, string userUUID, CancellationToken ct = default);
        Task<bool> HasUserLikedPostAsync(string userUUID, int feedPostId, CancellationToken ct = default);
        Task<string> UploadMediaAttachmentAsync(IFormFile file, CancellationToken ct = default);
        Task<FeedPost> UploadNewPostAsync(FeedPost post, CancellationToken ct = default);
        Task MarkPostAsDeletedAsync(int feedPostId, CancellationToken ct = default);
        Task VoteForPollAsync(string userUUID, int feedPollAnswerId, bool voteFor = true, CancellationToken ct = default);
        Task ResetPollVotesAsync(int postId, CancellationToken ct = default);

        Task<ICollection<FeedComment>> GetCommentsForPostAsync(int postId, CancellationToken ct = default);
        Task<FeedComment> GetCommentByIdAsync(int commentId, CancellationToken ct = default);
        Task<FeedComment> AddCommentToPostAsync(FeedComment comment, CancellationToken ct = default);
        Task MarkFeedCommentAsDeletedAsync(int feedCommentId, CancellationToken ct = default);

        Task<ICollection<AditLog>> GetAllAditLogsAsync(int teamId = 0, CancellationToken ct = default);
        Task<AditLog> GetAditLogAsync(int aditLogId, CancellationToken ct = default);
        Task MarkAditLogAsDeletedAsync(int aditLogId, CancellationToken ct = default);
        Task<AditLogUrlResult> UploadAditLogMediaAsync(IFormFile aditLog, bool isVideo = false, CancellationToken ct = default);
        Task<AditLog> UploadNewAditLogAsync(AditLog aditLog, CancellationToken ct = default);
        Task WatchAditLogAsync(int aditLogId, string userUUID, CancellationToken ct = default);

        Task ReportPostAsync(FeedReport report, CancellationToken ct = default);
        Task MarkReportAsDeletedAsync(int reportId, CancellationToken ct = default);
    }

    public class FeedService : IFeedService
    {
        private readonly AppDbContext _context;
        private readonly ICDNService _cdnService;

        public FeedService(AppDbContext context, ICDNService cdnService)
        {
            _context = context;
            _cdnService = cdnService;
        }

        public async void Dispose()
        {
            await _context.DisposeAsync();
        }

        public async Task<ICollection<FeedPost>> GetAllAsync(long startfrom, int teamId = 0, int limit = 10, bool includeComments = false, CancellationToken ct = default)
        {
            DateTime fromDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(startfrom);
            ICollection<FeedPost> posts = await _context.FeedPosts
                .AsNoTracking()
                .Include(fp => fp.Likes.Where(fl => fl.IsLiked))
                .Include(fp => fp.Poster)
                .Include(fp => fp.Comments.Where(fc => !fc.IsDeleted))
                .Include(fp => fp.PostTeam)
                .Include(fp => ((FeedPollPost)fp).PollAnswers)
                    .ThenInclude(fpa => fpa.Votes.Where(fpv => !fpv.IsDeleted))
                .Where(fp => fp.PostCreatedAt <= fromDate && !fp.IsDeleted && (teamId == 0 || fp.PostIsGlobal || fp.TeamId == teamId))
                .OrderByDescending(fp => fp.PostCreatedAt)
                .Take(limit)
                .ToListAsync(ct);

            foreach (FeedPost post in posts)
            {
                post.PosterDTO = post.Poster.ConvertToDTO();
                post.Poster = null;
                if (includeComments)
                {
                    post.Comments = await GetCommentsForPostAsync(post.PostId, ct);
                }
            }
            return posts;
        }

        public async Task<ICollection<FeedPostDTO>> GetAllDTOPostsForUserAsync(string uuid, CancellationToken ct = default)
        {
            ICollection<FeedPost> posts = await _context.FeedPosts
                   .AsNoTracking()
                   .Where(p => p.PosterUUID == uuid && !p.IsDeleted)
                   .OrderByDescending(fp => fp.PostCreatedAt)
                   .ToListAsync(ct);

            return posts.Select(p => p is FeedMediaPost media ? media.ConvertToDTO() : p.ConvertToDTO()).ToList();
        }

        public async Task<FeedPost> GetByIdAsync(int feedPostId, CancellationToken ct = default)
        {
            FeedPost post = await _context.FeedPosts
                .AsNoTracking()
                .Include(fp => fp.Likes.Where(fl => fl.IsLiked))
                .Include(fp => fp.Poster)
                .Include(fp => fp.PostTeam)
                .Include(fp => fp.Comments)
                    .ThenInclude(fpp => fpp.User)
                .Include(fp => ((FeedPollPost)fp).PollAnswers)
                    .ThenInclude(fpa => fpa.Votes.Where(fpv => !fpv.IsDeleted))
                .FirstOrDefaultAsync(fp => fp.PostId == feedPostId, ct);
            post.PosterDTO = post.Poster.ConvertToDTO();
            post.Poster = null;
            foreach (FeedComment comment in post.Comments)
            {
                comment.UserDTO = comment.User.ConvertToDTO();
                comment.User = null;
            }
            return post;
        }

        public async Task<bool> HasUserLikedPostAsync(string userUUID, int feedPostId, CancellationToken ct = default)
        {
            FeedLike like = await _context.FeedPostLikes.AsNoTracking().FirstOrDefaultAsync(fpl => fpl.UserUUID == userUUID && fpl.PostId == feedPostId && fpl.IsLiked);
            return like != null;
        }

        public async Task ToggleLikeForPostAsync(int feedPostId, string userUUID, CancellationToken ct = default)
        {
            FeedPost post = await _context.FeedPosts
                    .AsNoTracking()
                    .Include(fp => fp.Likes)
                    .FirstOrDefaultAsync(fp => fp.PostId == feedPostId, ct);

            if (post == null)
                return;


            FeedLike like = post.Likes.FirstOrDefault(fl => fl.UserUUID == userUUID);
            if (await HasUserLikedPostAsync(userUUID, feedPostId))
            {
                like.IsLiked = false;
                post.PostLikeCount -= 1;
                _context.Update(like);
                _context.Update(post);
            } else
            {
                if (like == null)
                {
                    FeedLike newLike = new FeedLike
                    {
                        PostId = feedPostId,
                        UserUUID = userUUID,
                        IsLiked = true
                    };
                    await _context.FeedPostLikes.AddAsync(newLike);
                    post.PostLikeCount += 1;
                    _context.Update(post);
                } else
                {
                    like.IsLiked = !like.IsLiked;
                    _context.Update(like);
                    post.PostLikeCount = like.IsLiked ? (post.PostLikeCount += 1) : (post.PostLikeCount -= 1);
                }
            }
            await _context.SaveChangesAsync();
        }
 
        public async Task<string> UploadMediaAttachmentAsync(IFormFile file, CancellationToken ct = default)
        {
            if (file == null) return null;

            string uuid = Guid.NewGuid().ToString();
            Directory.CreateDirectory("/var/www/cdn/GaryPortal/Feed/Attachments/Media/");
            string newFileName = file.FileName.Replace(Path.GetFileNameWithoutExtension(file.FileName), uuid);
            await _cdnService.UploadFeedMedia(newFileName, file, ct);
            return $"https://cdn.tomk.online/GaryPortal/Feed/Attachments/Media/{newFileName}";
        }

        public async Task<FeedPost> UploadNewPostAsync(FeedPost post, CancellationToken ct = default)
        {
            if (post is FeedMediaPost)
            {
                post = post as FeedMediaPost;
            } else
            {
                post = post as FeedPollPost;
            }

            await _context.FeedPosts.AddAsync(post, ct);
            await _context.SaveChangesAsync(ct);
            return await GetByIdAsync(post.PostId, ct);
        }

        public async Task MarkPostAsDeletedAsync(int feedPostId, CancellationToken ct = default)
        {
            FeedPost post = await _context.FeedPosts.FindAsync(feedPostId);
            post.IsDeleted = true;
            _context.Update(post);
            await _context.SaveChangesAsync(ct);
        }


        public async Task<ICollection<FeedComment>> GetCommentsForPostAsync(int postId, CancellationToken ct = default)
        {
            ICollection<FeedComment> comments = await _context.FeedPostComments
                .AsNoTracking()
                .Include(u => u.User)
                .Where(fc => fc.PostId == postId && !fc.IsDeleted)
                .ToListAsync(ct);
            foreach (FeedComment comment in comments)
            {
                comment.UserDTO = comment.User.ConvertToDTO();
                comment.User = null;
            }
            return comments;
        }

        public async Task<FeedComment> GetCommentByIdAsync(int commentId, CancellationToken ct = default)
        {
            FeedComment comment = await _context.FeedPostComments
                .AsNoTracking()
                .Include(fc => fc.User)
                .FirstOrDefaultAsync(fc => fc.FeedCommentId == commentId, cancellationToken: ct);
            comment.UserDTO = comment.User.ConvertToDTO();
            comment.User = null;
            return comment;
        }

        public async Task<FeedComment> AddCommentToPostAsync(FeedComment comment, CancellationToken ct = default)
        {
            await _context.FeedPostComments.AddAsync(comment, ct);
            await _context.SaveChangesAsync(ct);
            return await GetCommentByIdAsync(comment.FeedCommentId, ct);
        }

        public async Task MarkFeedCommentAsDeletedAsync(int feedCommentId, CancellationToken ct = default)
        {
            FeedComment comment = await _context.FeedPostComments.FindAsync(feedCommentId);
            comment.IsDeleted = true;
            _context.Update(comment);
            await _context.SaveChangesAsync(ct);
        }

        public async Task VoteForPollAsync(string userUUID, int feedPollAnswerId, bool voteFor = true, CancellationToken ct = default)
        {
            FeedAnswerVote vote = new FeedAnswerVote
            {
                UserUUID = userUUID,
                PollAnswerId = feedPollAnswerId,
                IsDeleted = !voteFor
            };
            FeedAnswerVote existingVote = await _context.FeedAnswerVotes.FindAsync(feedPollAnswerId, userUUID);
            if (existingVote != null)
            {
                _context.Entry(existingVote).CurrentValues.SetValues(vote);
            } else
            {
                _context.Add(vote);
            }
            await _context.SaveChangesAsync(ct);          
        }

        public async Task ResetPollVotesAsync(int postId, CancellationToken ct = default)
        {
            if (await GetByIdAsync(postId, ct) is not FeedPollPost post)
                return;
            foreach (FeedPollAnswer answer in post.PollAnswers)
            {
                foreach (FeedAnswerVote vote in answer.Votes.Where(v => !v.IsDeleted))
                {
                    vote.IsDeleted = true;
                }
            }
            _context.Update(post);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<ICollection<AditLog>> GetAllAditLogsAsync(int teamId = 0, CancellationToken ct = default)
        {
            DateTime yesterday = DateTime.UtcNow.AddDays(-1);
            ICollection<AditLog> aditLogs = await _context.FeedAditLogs
                .AsNoTracking()
                .Include(al => al.AditLogTeam)
                .Include(al => al.Poster)
                .Where(al => (teamId == 0 || al.AditLogTeamId == teamId) && al.DatePosted >= yesterday)
                .OrderBy(al => al.DatePosted)
                .ToListAsync(ct);
            foreach (AditLog aditlog in aditLogs)
            {
                aditlog.PosterDTO = aditlog.Poster.ConvertToDTO();
                aditlog.Poster = null;
            }
            return aditLogs;
        }

        public async Task<AditLog> GetAditLogAsync(int aditLogId, CancellationToken ct = default)
        {
            AditLog aditLog = await _context.FeedAditLogs
                .AsNoTracking()
                .Include(al => al.AditLogTeam)
                .Include(al => al.Poster)
                .FirstOrDefaultAsync(al => al.AditLogId == aditLogId);
            aditLog.PosterDTO = aditLog.Poster.ConvertToDTO();
            aditLog.Poster = null;
            return aditLog;
        }

        public async Task MarkAditLogAsDeletedAsync(int aditLogId, CancellationToken ct = default)
        {
            AditLog aditlog = await _context.FeedAditLogs.FindAsync(aditLogId);
            aditlog.IsDeleted = true;
            _context.Update(aditlog);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<AditLogUrlResult> UploadAditLogMediaAsync(IFormFile aditLog, bool isVideo = false, CancellationToken ct = default)
        {
            if (aditLog == null) return null;

            string uuid = Guid.NewGuid().ToString();

            string newFileName = aditLog.FileName.Replace(Path.GetFileNameWithoutExtension(aditLog.FileName), uuid);

            await _cdnService.UploadFeedAditLog(newFileName, aditLog, ct);

            return new AditLogUrlResult
            {
                AditLogUrl = $"https://cdn.tomk.online/GaryPortal/Feed/Attachments/AditLogs/{newFileName}",
                AditLogThumbnailUrl = isVideo ? $"https://cdn.tomk.online/GaryPortal/Feed/Attachments/AditLogs/Thumbs/{newFileName}.jpg" : ""
            };

        }

        public async Task<AditLog> UploadNewAditLogAsync(AditLog aditLog, CancellationToken ct = default)
        {
            await _context.FeedAditLogs.AddAsync(aditLog, ct);
            await _context.SaveChangesAsync(ct);
            return await GetAditLogAsync(aditLog.AditLogId, ct);
        }

        public async Task WatchAditLogAsync(int aditLogId, string userUUID, CancellationToken ct = default)
        {
            AditLog aditLog = await _context.FeedAditLogs.FindAsync(aditLogId);
            if (aditLog != null)
            {
                aditLog.AditLogViews += 1;
                _context.Update(aditLog);
                await _context.SaveChangesAsync(ct);
            }
            return;
        }

        public async Task ReportPostAsync(FeedReport report, CancellationToken ct = default)
        {
            await _context.FeedReports.AddAsync(report);
            await _context.SaveChangesAsync(ct);
        }

        public async Task MarkReportAsDeletedAsync(int reportId, CancellationToken ct = default)
        {
            FeedReport report = await _context.FeedReports.FindAsync(reportId);
            if (report != null)
            {
                report.IsDeleted = true;
                _context.Update(report);
                await _context.SaveChangesAsync();
            }
        }
    }
}

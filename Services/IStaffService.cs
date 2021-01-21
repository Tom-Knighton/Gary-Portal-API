﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GaryPortalAPI.Models;
using GaryPortalAPI.Services.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
namespace GaryPortalAPI.Services
{
    public interface IStaffService : IDisposable
    {
        Task<ICollection<StaffRoomAnnouncement>> GetStaffRoomAnnouncementsAsync(CancellationToken ct = default);
        Task<StaffRoomAnnouncement> GetStaffRoomAnnouncementAsync(int id, CancellationToken ct = default);
        Task<StaffRoomAnnouncement> PostStaffRoomAnnouncementAsync(StaffRoomAnnouncement announcement, CancellationToken ct = default);
        Task MarkAnnouncementAsDeletedAsync(int id, CancellationToken ct = default);
        Task<ICollection<Team>> GetAllTeams(CancellationToken ct = default);
        Task<ICollection<BanType>> GetAllBanTypesAsync(CancellationToken ct = default);
        Task<ICollection<Rank>> GetAllRanksAsync(CancellationToken ct = default);
        Task<User> StaffManageUserDetailsAsync(string uuid, StaffManagedUserDetails details, CancellationToken ct = default);
    }

    public class StaffService : IStaffService
    {
        private readonly AppDbContext _context;
        private readonly IUserService _userService;

        public StaffService(AppDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<StaffRoomAnnouncement> GetStaffRoomAnnouncementAsync(int id, CancellationToken ct = default)
        {
            StaffRoomAnnouncement announcement = await _context.StaffRoomAnnouncements
                .AsNoTracking()
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AnnouncementId == id && !a.IsDeleted, cancellationToken: ct);
            announcement.UserDTO = announcement.User.ConvertToDTO();
            announcement.User = null;
            return announcement;
        }

        public async Task<ICollection<StaffRoomAnnouncement>> GetStaffRoomAnnouncementsAsync(CancellationToken ct = default)
        {
            ICollection<StaffRoomAnnouncement> announcements = await _context.StaffRoomAnnouncements
               .AsNoTracking()
               .Include(a => a.User)
               .Where(a => !a.IsDeleted)
               .OrderByDescending(a => a.AnnouncementDate)
               .ToListAsync(ct);
            foreach (StaffRoomAnnouncement announcement in announcements)
            {
                announcement.UserDTO = announcement.User.ConvertToDTO();
                announcement.User = null;
            }
            return announcements;
        }

        public async Task MarkAnnouncementAsDeletedAsync(int id, CancellationToken ct = default)
        {
            StaffRoomAnnouncement announcement = await _context.StaffRoomAnnouncements
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AnnouncementId == id, cancellationToken: ct);
            announcement.IsDeleted = true;
            _context.Update(announcement);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<StaffRoomAnnouncement> PostStaffRoomAnnouncementAsync(StaffRoomAnnouncement announcement, CancellationToken ct = default)
        {
            await _context.StaffRoomAnnouncements.AddAsync(announcement, ct);
            await _context.SaveChangesAsync();
            return await GetStaffRoomAnnouncementAsync(announcement.AnnouncementId, ct);
        }

        public async Task<ICollection<Team>> GetAllTeams(CancellationToken ct = default)
        {
            return await _context.Teams.ToListAsync(ct);
        }

        public async Task<ICollection<BanType>> GetAllBanTypesAsync(CancellationToken ct = default)
        {
            return await _context.BanTypes.ToListAsync(ct);
        }

        public async Task<ICollection<Rank>> GetAllRanksAsync(CancellationToken ct = default)
        {
            return await _context.Ranks.ToListAsync(ct);
        }

        public async Task<User> StaffManageUserDetailsAsync(string uuid, StaffManagedUserDetails details, CancellationToken ct = default)
        {
            User user = await _userService.GetByIdAsync(uuid, ct);
            user.UserName = details.UserName;
            user.UserSpanishName = details.SpanishName;
            user.UserProfileImageUrl = details.ProfilePictureUrl;
            user.UserTeam.TeamId = details.TeamId;
            user.UserPoints.AmigoPoints = details.AmigoPoints;
            user.UserPoints.PositivityPoints = details.PositivePoints;

            user.UserRanks = new UserRanks
            {
                AmigoRankId = details.AmigoRankId,
                PositivtyRankId = details.PositiveRankId
            };
            _context.Update(user);
            await _context.SaveChangesAsync(ct);
            return await _userService.GetByIdAsync(uuid, ct);
        }
    }
}

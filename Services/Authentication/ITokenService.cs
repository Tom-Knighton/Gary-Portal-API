using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GaryPortalAPI.Data;
using GaryPortalAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;

namespace GaryPortalAPI.Services.Authentication
{
    public interface ITokenService : IDisposable
    {
        public Task<UserAuthenticationTokens> GenerateInitialTokensForUserAsync(string userUUID, CancellationToken ct = default);
        public Task<string> CreateAuthTokenForUserAsync(string userUUID);
        public string CreateRefreshToken();
        public Task<UserAuthenticationTokens> RefreshTokensForUserAsync(string userUUID, string refreshToken, CancellationToken ct = default);
        public Task<bool> IsRefreshTokenValidAsync(string userUUID, string refreshToken, CancellationToken ct = default);
        public Task InvalidateAllRefreshTokensAsync(string userUUID);
    }

    public class TokenService : ITokenService
    {
        private readonly AppDbContext _context;
        private readonly ApiSettings _apiSettings;
        private readonly IUserService _userService; 
        public TokenService(AppDbContext context, IOptions<ApiSettings> apiSettings, IUserService userService)
        {
            _context = context;
            _apiSettings = apiSettings.Value;
            _userService = userService;
        }

        public async void Dispose()
        {
            await _context.DisposeAsync();
        }

        public async Task<string> CreateAuthTokenForUserAsync(string userUUID)
        {
            User user = await _context.Users.FirstOrDefaultAsync(u => u.UserUUID == userUUID);
            if (user == null)
                return null;

            UserBan chatBan = await _userService.GetFirstBanOfTypeIfAnyAsnc(userUUID, 2);
            UserBan feedBan = await _userService.GetFirstBanOfTypeIfAnyAsnc(userUUID, 3);

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            byte[] secretKey = Encoding.ASCII.GetBytes(_apiSettings.Secret);
            List<Claim> claims = new List<Claim> { new Claim(ClaimTypes.Name, userUUID) };
            claims.Add(new Claim(ClaimTypes.Role, user.HasUserFlag("Role.Admin") ? "admin" : user.HasUserFlag("Role.Staff") ? "staff" : "user"));
            if (chatBan == null) claims.Add(new Claim(ClaimTypes.UserData, "chatAllowed"));
            if (feedBan == null) claims.Add(new Claim(ClaimTypes.UserData, "feedAllowed"));

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims.ToArray()),
                Expires = DateTime.Now.AddMinutes(20),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _apiSettings.Issuer,
                Audience = _apiSettings.Issuer,
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string CreateRefreshToken()
        {
            RNGCryptoServiceProvider rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            byte[] randomBytes = new byte[64];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<UserAuthenticationTokens> GenerateInitialTokensForUserAsync(string userUUID, CancellationToken ct = default)
        {
            string newRefreshToken = CreateRefreshToken();
            UserRefreshToken refreshToken = new UserRefreshToken
            {
                RefreshToken = newRefreshToken,
                UserUUID = userUUID,
                TokenIssueDate = DateTime.UtcNow,
                TokenExpiryDate = DateTime.UtcNow.AddMonths(2),
                TokenClient = "N/A",
                TokenIsEnabled = true
            };
            await _context.UserRefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
            return new UserAuthenticationTokens
            {
                AuthenticationToken = await CreateAuthTokenForUserAsync(userUUID),
                RefreshToken = newRefreshToken
            };
        }

        public async Task<bool> IsRefreshTokenValidAsync(string userUUID, string refreshToken, CancellationToken ct = default)
        {
            UserRefreshToken token = await _context.UserRefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.UserUUID == userUUID && t.RefreshToken == refreshToken && t.TokenIsEnabled, ct);
            if (token != null)
            {
                return token.TokenExpiryDate > DateTime.UtcNow && !await _userService.IsUserBannedAsync(userUUID);
            }
            return false;
        }

        public async Task<UserAuthenticationTokens> RefreshTokensForUserAsync(string userUUID, string refreshToken, CancellationToken ct = default)
        {
            if (!await IsRefreshTokenValidAsync(userUUID, refreshToken))
                return null;

            UserRefreshToken token = await _context.UserRefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.RefreshToken == refreshToken && t.UserUUID == userUUID);
            if (token == null || await _userService.GetFirstBanOfTypeIfAnyAsnc(userUUID, 1, ct) != null)
                return null;

            token.TokenIsEnabled = false;
            string newRefreshToken = CreateRefreshToken();
            UserRefreshToken userRefreshToken = new UserRefreshToken
            {
                UserUUID = userUUID,
                RefreshToken = newRefreshToken,
                TokenClient = "N/A",
                TokenIssueDate = DateTime.UtcNow,
                TokenExpiryDate = DateTime.UtcNow.AddMonths(2),
                TokenIsEnabled = true
            };
            await _context.UserRefreshTokens.AddAsync(userRefreshToken);
            _context.UserRefreshTokens.Update(token);
            await _context.SaveChangesAsync();
            return
                new UserAuthenticationTokens
                {
                    AuthenticationToken = await CreateAuthTokenForUserAsync(userUUID),
                    RefreshToken = newRefreshToken
                };
        }

        public async Task InvalidateAllRefreshTokensAsync(string uuid)
        {
            ICollection<UserRefreshToken> tokens = await _context.UserRefreshTokens.Where(urt => urt.UserUUID == uuid).ToListAsync();
            foreach (UserRefreshToken token in tokens)
            {
                token.TokenIsEnabled = false;
            }
            await _context.SaveChangesAsync();
        }
    }
}

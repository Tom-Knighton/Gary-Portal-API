using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GaryPortalAPI.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using RestSharp;

namespace GaryPortalAPI.Services
{
    public interface ICDNService: IDisposable
    {
        Task UploadChatAttachment(string newName, IFormFile file, string chatUUID, CancellationToken ct = default);
        Task UploadProfilePicture(string newName, IFormFile file, string userUUID, CancellationToken ct = default);
        Task UploadFeedMedia(string newName, IFormFile file, CancellationToken ct = default);
        Task UploadFeedAditLog(string newName, IFormFile file, CancellationToken ct = default);
        Task UploadFeedAditThumbnail(string newName, IFormFile file, CancellationToken ct = default);
    }

    public class CDNService : ICDNService
    {
        private readonly ApiSettings _appSettings;
        public CDNService(IOptions<ApiSettings> apisettings)
        {
            _appSettings = apisettings.Value;
        }

        public void Dispose()
        {
            
        }

        public async Task UploadChatAttachment(string newName, IFormFile file, string chatUUID, CancellationToken ct = default)
        {
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            var fileBytes = ms.ToArray();
            RestClient client = new RestClient(_appSettings.CDNServer);
            RestRequest request = new RestRequest($"gpchatfeed/{chatUUID}", Method.POST);
            request.AddHeader("x-api-key", _appSettings.CDNServerKey);
            request.AddFile(newName, fileBytes, newName);
            request.AlwaysMultipartFormData = true;
            await client.ExecuteAsync(request, ct);
        }

        public async Task UploadFeedAditLog(string newName, IFormFile file, CancellationToken ct = default)
        {
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            var fileBytes = ms.ToArray();
            RestClient client = new RestClient(_appSettings.CDNServer);
            RestRequest request = new RestRequest($"gpfeed/aditlog", Method.POST);
            request.AddHeader("x-api-key", _appSettings.CDNServerKey);
            request.AddFile(newName, fileBytes, newName);
            request.AlwaysMultipartFormData = true;
            await client.ExecuteAsync(request, ct);
        }

        public async Task UploadFeedAditThumbnail(string newName, IFormFile file, CancellationToken ct = default)
        {
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            var fileBytes = ms.ToArray();
            RestClient client = new RestClient(_appSettings.CDNServer);
            RestRequest request = new RestRequest($"gpfeed/aditlog/thumb", Method.POST);
            request.AddHeader("x-api-key", _appSettings.CDNServerKey);
            request.AddFile(newName, fileBytes, newName);
            request.AlwaysMultipartFormData = true;
            await client.ExecuteAsync(request, ct);
        }

        public async Task UploadFeedMedia(string newName, IFormFile file, CancellationToken ct = default)
        {
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            var fileBytes = ms.ToArray();
            RestClient client = new RestClient(_appSettings.CDNServer);
            RestRequest request = new RestRequest($"gpfeed/media", Method.POST);
            request.AddHeader("x-api-key", _appSettings.CDNServerKey);
            request.AddFile(newName, fileBytes, newName);
            request.AlwaysMultipartFormData = true;
            await client.ExecuteAsync(request, ct);
        }

        public async Task UploadProfilePicture(string newName, IFormFile file, string userUUID, CancellationToken ct = default)
        {
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            var fileBytes = ms.ToArray();
            RestClient client = new RestClient(_appSettings.CDNServer);
            RestRequest request = new RestRequest($"gpprofile/{userUUID}", Method.POST);
            request.AddHeader("x-api-key", _appSettings.CDNServerKey);
            request.AddFile(newName, fileBytes, newName);
            request.AlwaysMultipartFormData = true;
            await client.ExecuteAsync(request, ct);
        }
    }
}

using GaryPortalAPI.Models;

namespace GaryPortalAPI.Data
{
    public class ApiSettings
    {
        public string Secret { get; set; }
        public string Connection { get; set; }
        public string Issuer { get; set; }
        public string MailApi { get; set; }
        public string CDNServer { get; set; }
        public string CDNServerKey { get; set; }
        public APNSSettings APNSSettings { get; set; }
    }
}

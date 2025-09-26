namespace Ecommerce.Application.Models.Email
{
    public class EmailFluentSettings
    {
        public string Email { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string BaseUrlClient { get; set; } = string.Empty;
        public string GunEmailKey { get; set; } = string.Empty;
        public string GunEmailSender { get; set; } = string.Empty;
        public string GunEmailSmtp { get; set; } = string.Empty;
        public string GunEmailPort { get; set; } = string.Empty;
        public string GunEmailPassword { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
    }
}

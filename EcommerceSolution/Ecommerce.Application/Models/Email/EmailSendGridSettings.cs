namespace Ecommerce.Application.Models.Email
{
    public class EmailSendGridSettings
    {
        public string? ApiKey { get; set; } 
        public string Email { get; set; } = string.Empty;
        public string? FromName { get; set; }
        public string BaseUrlClient { get; set; } = string.Empty;
    }
}

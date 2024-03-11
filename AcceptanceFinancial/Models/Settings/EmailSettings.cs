namespace AcceptanceFinancial.Models.Settings; 

public class EmailSettings {
    public const string Key = "EmailSettings";
    public string ToEmail { get; set; } = null!;
    public string FromEmail { get; set; } = null!;
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string UseSsl { get; set; } = null!;
    public string Subject { get; set; } = null!;
}
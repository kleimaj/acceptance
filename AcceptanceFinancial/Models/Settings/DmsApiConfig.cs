namespace AcceptanceFinancial.Models.Settings;

public class DmsApiConfig {
    public const string Key = "DmsApiConfig";
    public required string PostgresConnectionString { get; set; }
}
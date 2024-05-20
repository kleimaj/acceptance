using Marten.Schema;

namespace AcceptanceFinancial.Models;

public class Abandoned
{
    [Identity]
    public string Email { get; set; }
    public string? Phone { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

}
using CsvHelper.Configuration.Attributes;
using Marten.Schema;

namespace AcceptanceFinancial.Models;

public class DirectMail {
    [Identity]
    [Name("ext_id")]
    public string AccessCode { get; set; }
    [Name("first")]
    public string FirstName { get; set; }
    [Name("last")]
    public string LastName { get; set; }
    [Ignore]
    public string Email { get; set; }
    [Ignore]
    public string PrimaryPhone { get; set; }
    [Name("address")]
    public string Address { get; set; }
    [Name("city")]
    public string City { get; set; }
    [Name("st")]
    public string StateCode { get; set; }
    [Name("zip")]
    public string Zip { get; set; }
    [Name("inhome_date")]
    [Ignore]
    public DateTime MailDate { get; set; }
    
    [Name("EXP_DATE")]
    [Ignore]
    public DateTime Exp_Date { get; set; }
    
    public string Team { get; set; }
}

namespace AcceptanceFinancial.Models;

public class CustomerWithLoanInfo {
    public Guid Id { get; set; }
    public Customer Customer { get; set; }
    public CustomerLoanInfo CustomerLoanInfo { get; set; }
}
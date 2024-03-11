using AcceptanceFinancial.Models.Enums;

namespace AcceptanceFinancial.Models;

public class CustomerLoanInfo {
    public Guid Id { get; set; }
    public string EstimatedDebt { get; set; }

    public string MonthlyIncome { get; set; }

    public RadioYesNo IsCurrentWithCreditors { get; set; }

    public RadioYesNo IsEmployed { get; set; }

    public string LanguagePreference { get; set; }

    public LoanType LoanType { get; set; }
}
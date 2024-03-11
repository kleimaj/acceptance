using System.ComponentModel.DataAnnotations;

namespace AcceptanceFinancial.Models.Enums;

public enum LoanType {
    [Display(Name = "Consumer Loan")] Consumer = 1,

    [Display(Name = "Debt Resolution")] Resolution = 2,

    [Display(Name = "Debt Consolidation")] Consolidation = 3
}
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcceptanceFinancial.Models.Const; 

public static class LoanAmountList {
    public static List<SelectListItem> loanAmtList = new List<SelectListItem> {
        new() { Text = "Loan Amount", Value = "Loan Amount" },
        new() { Text = "Less than $7,500", Value = "Less than $7,500" },
        new() { Text = "$7,500-$20,000", Value = "$7,500-$20,000" },
        new() { Text = "$20,000-$50,000", Value = "$20,000-$50,000" },
        new() { Text = "$50,000-$100,000", Value = "$50,000-$100,000" },
        new() { Text = "$100,000+", Value = "$100,000+" }
    };
}
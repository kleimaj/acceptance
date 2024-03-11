using System.ComponentModel.DataAnnotations;

namespace AcceptanceFinancial.Models.Enums;

public enum RadioYesNo {
    [Display(Name = "Yes")] Yes = 1,

    [Display(Name = "No")] No = 2
}
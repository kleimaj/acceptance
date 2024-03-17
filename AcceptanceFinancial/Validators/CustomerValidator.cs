using FluentValidation;
using AcceptanceFinancial.Models;

namespace AcceptanceFinancial.Validators;

public class CustomerValidator : AbstractValidator<Customer> {
    public CustomerValidator() {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.");
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.");
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.").EmailAddress().WithMessage("Enter a valid email address.");
        RuleFor(x => x.Phone)
            .Length(13, 18).WithMessage("Phone number is not a valid number.");
        RuleFor(x => x.Cell)
            .NotEmpty().WithMessage("Cell phone number is required").Length(13, 18).WithMessage("Cell phone number is not a valid number.");
        RuleFor(x => x.Address)
          .NotEmpty().WithMessage("Address is required.");
        RuleFor(x => x.City)
          .NotEmpty().WithMessage("City is required.");
        RuleFor(x => x.State)
          .NotNull().NotEqual("").WithMessage("Please select a state.");
          RuleFor(x => x.Zip)
          .NotEmpty().WithMessage("Zip code is required.");
        RuleFor(x => x.Offer);
        RuleFor(x => x.Address2);
        RuleFor(x => x.TotalDebt)
            .NotEmpty().WithMessage("Please enter total debt.");
    }
}
using FluentValidation;
using AcceptanceFinancial.Models;

namespace AcceptanceFinancial.Validators;

public class ContactUsValidator : AbstractValidator<ContactUs> {
    public ContactUsValidator() {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.").EmailAddress()
            .WithMessage("Enter a valid email address.");
        RuleFor(x => x.Message)
            .MaximumLength(500);
        RuleFor(x => x.Subject)
            .MaximumLength(500);
        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.").Length(13, 18).WithMessage("Phone number is not a valid number.");

    }
}
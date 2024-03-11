using AcceptanceFinancial.Models;

namespace AcceptanceFinancial.Services;

public interface IMartenService {
    internal Task<bool> CreatePromocode(Promocode promocode);
    internal Task<bool> CreateCustomer(Customer customer);
    internal Task<bool> CreateShortCustomer(ShortCustomer customer);
    internal Task<bool> CreateContactUs(ContactUs contactUs);
    internal Task<bool> CreateCustomerLoanInfo(CustomerLoanInfo customerLoanInfo);
    internal Task<bool> CreateDirectMails(List<DirectMail> directMails);
    internal Task<DirectMail?> GetDirectMail(string Promocode);
}
﻿namespace AcceptanceFinancial.Models;

public class ShortCustomer {
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Offer { get; set; }
    public string? LoanAmount { get; set; }
    public string? LoanUse { get; set; }
    public string? IpAddress { get; set; }
}
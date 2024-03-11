using System.Diagnostics;
using FluentValidation;
using FormHelper;
using Microsoft.AspNetCore.Mvc;
using AcceptanceFinancial.Models;
using AcceptanceFinancial.Models.Jsn;
using AcceptanceFinancial.Services;

namespace AcceptanceFinancial.Controllers;

public class CareerController : Controller {
    private readonly ILogger<CareerController> _logger;
    private readonly IMartenService _martenService;
    private IValidator<CustomerLoanInfo> _validator;

    public CareerController(ILogger<CareerController> logger, IValidator<CustomerLoanInfo> validator,
        IMartenService martenService) {
        _logger = logger;
        _validator = validator;
        _martenService = martenService;
    }

    [HttpGet("careers")]
    public IActionResult Index(Guid customerGid) {
        ViewBag.CustomerGid = customerGid;
        return View(new CustomerLoanInfo { Id = customerGid });
    }

    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        _logger.LogError("{HttpContextTraceIdentifier}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    [FormValidator]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(CustomerLoanInfo customerLoanInfo) {
        await _martenService.CreateCustomerLoanInfo(customerLoanInfo);

        try {
            return FormResult.CreateSuccessResult("Customer saved.", Url.Action("", "ThankYou"), 500);
        }
        catch {
            return FormResult.CreateErrorResult("An error occurred!");
        }
    }
}
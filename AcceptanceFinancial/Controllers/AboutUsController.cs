using System.Diagnostics;
using FluentValidation;
using FluentValidation.AspNetCore;
using FormHelper;
using Microsoft.AspNetCore.Mvc;
using AcceptanceFinancial.Models;
using AcceptanceFinancial.Models.Jsn;
using AcceptanceFinancial.Services;

namespace AcceptanceFinancial.Controllers;

public class AboutUsController : Controller {
    private readonly ILogger<AboutUsController> _logger;
    private readonly IMartenService _martenService;
    private readonly IValidator<Customer> _validator;

    public AboutUsController(IValidator<Customer> validator, ILogger<AboutUsController> logger,
        IMartenService martenService) {
        _validator = validator;
        _logger = logger;
        _martenService = martenService;
    }

    [HttpGet("about-us")]
    public IActionResult Index(Guid customerGid) {
        ViewBag.CustomerGid = customerGid;
        return View(new Customer { Id = customerGid });
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
    public async Task<IActionResult> Save(Customer customer) {
        var result = await _validator.ValidateAsync(customer);
        if (!result.IsValid) {
            result.AddToModelState(ModelState);
            return View("Index", customer);
        }

        try {
            await _martenService.CreateCustomer(customer);

            return FormResult.CreateSuccessResult("Customer saved.",
                Url.Action("", "Career", new { customerGid = customer.Id }), 500);
        }
        catch {
            return FormResult.CreateErrorResult("An error occurred!");
        }
    }
}
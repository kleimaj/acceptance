using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AcceptanceFinancial.Models.Jsn;

namespace AcceptanceFinancial.Controllers;

public class TermsConditionController : Controller {
    private readonly ILogger<TermsConditionController> _logger;

    public TermsConditionController(ILogger<TermsConditionController> logger) {
        _logger = logger;
    }

    [HttpGet("terms-conditions")]
    public IActionResult Index() {
        return View();
    }

    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        _logger.LogError("{HttpContextTraceIdentifier}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AcceptanceFinancial.Models.Jsn;

namespace AcceptanceFinancial.Controllers;

public class FaqController : Controller {
    private readonly ILogger<FaqController> _logger;

    public FaqController(ILogger<FaqController> logger) {
        _logger = logger;
    }

    [HttpGet("faqs")]
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
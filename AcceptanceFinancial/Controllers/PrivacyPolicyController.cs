using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AcceptanceFinancial.Models.Jsn;

namespace AcceptanceFinancial.Controllers;

public class PrivacyPolicyController : Controller {
    private readonly ILogger<PrivacyPolicyController> _logger;

    public PrivacyPolicyController(ILogger<PrivacyPolicyController> logger) {
        _logger = logger;
    }

    [HttpGet("privacy-policy")]
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
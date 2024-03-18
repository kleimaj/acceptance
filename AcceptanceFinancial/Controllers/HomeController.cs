using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using FluentValidation;
using FluentValidation.AspNetCore;
using FormHelper;
using JSNLog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using AcceptanceFinancial.Models;
using AcceptanceFinancial.Models.Const;
using AcceptanceFinancial.Models.Jsn;
using AcceptanceFinancial.Models.Settings;
using AcceptanceFinancial.Services;
using AcceptanceFinancial.Validators;

namespace AcceptanceFinancial.Controllers;

public class HomeController : Controller {
    private readonly ILogger<HomeController> _logger;
    private readonly IMartenService _martenService;
    private readonly IValidator<ShortCustomer> _shortValidator;
    private readonly EmailSettings _emailSettings;

    public HomeController(ILogger<HomeController> logger, IMartenService martenService, IValidator<ShortCustomer> shortValidator, IOptions<EmailSettings> emailSettings) {
        _logger = logger;
        _martenService = martenService;
        _shortValidator = shortValidator;
        _emailSettings = emailSettings.Value;
    }

    [HttpGet]
    public IActionResult Index() {
        return View();
    }

    [HttpPost]
    [FormValidator]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveShort(ShortCustomer customer) {
        customer.Id = Guid.NewGuid();
        
        var form = await Request.ReadFormAsync();
        var token = form["cf-turnstile-response"];
        var ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();
        customer.IpAddress = ip;
        
        var formData = new MultipartFormDataContent();
        /*const string SECRET_KEY = "0x4AAAAAAAIUXGJaJKd7EGHsvUK8FToQJk4";*/
        const string SECRET_KEY = "1x0000000000000000000000000000000AA";
        formData.Add(new StringContent(SECRET_KEY), "secret");
        formData.Add(new StringContent(token), "response");
        formData.Add(new StringContent(ip), "remoteip");

        var url = "https://challenges.cloudflare.com/turnstile/v0/siteverify";
        var client = new HttpClient();
        var response = await client.PostAsJsonAsync(url, formData);


        if (response.IsSuccessStatusCode) {
            var result = await _shortValidator.ValidateAsync(customer);
            if (!result.IsValid) {
                result.AddToModelState(ModelState);
                return View("Index", customer);
            }

            if (customer.Offer != null) {
                var directMail = await _martenService.GetDirectMail(customer.Offer);
                var directMailCustomer = new Customer();
                if (directMail != null) {
                    directMailCustomer.Id = customer.Id;
                    directMailCustomer.Address = directMail.Address;
                    directMailCustomer.FirstName = directMail.FirstName;
                    directMailCustomer.LastName = directMail.LastName;
                    directMailCustomer.City = directMail.City;
                    directMailCustomer.Zip = directMail.Zip;
                    directMailCustomer.State = directMail.StateCode;
                    directMailCustomer.Offer = customer.Offer;
                    directMailCustomer.Email = customer.Email;
                    directMailCustomer.Cell = customer.Phone;
                    directMailCustomer.LoanAmount = customer.LoanAmount;
                    directMailCustomer.IpAddress = ip;
                    var controller = this.HttpContext.RequestServices.GetService(typeof(ApplyController));
                    var applyController = controller as ApplyController;
                    applyController?.Save(directMailCustomer);
                    
                    return FormResult.CreateSuccessResult("Getting your Quote.",
                        Url.Action("", "Congratulations", new { customerGid = customer.Id }), 500);

                }
            }
            else {
                try {
                    postShortToCRM(customer);
                }
                catch (Exception e) {
                    _logger.LogError("{HttpContextTraceIdentifier}",
                        Activity.Current?.Id ?? HttpContext.TraceIdentifier);
                }

                try {
                    EmailContact(customer);
                }
                catch (Exception e) {
                    _logger.LogError("{HttpContextTraceIdentifier}",
                        Activity.Current?.Id ?? HttpContext.TraceIdentifier);
                }

                try {
                    await _martenService.CreateShortCustomer(customer);

                    return FormResult.CreateSuccessResult("Getting your Quote.",
                        Url.Action("", "Congratulations", new { customerGid = customer.Id }), 500);
                }
                catch {
                    return FormResult.CreateErrorResult("An error occurred!");
                }
            }
        }
        else {
                return FormResult.CreateErrorResult("Capatcha result failed!");
        }
        return FormResult.CreateErrorResult("An error occurred!");
    }

    /// <summary>
    ///  Post to CRM based on loan amount and offer code
    /// </summary>
    /// <param name="customer"></param>
    /// <exception cref="InvalidOperationException"></exception>
    internal void postShortToCRM(ShortCustomer customer) {
        //var url = GetEndpointUrl(customer);

        var client = new RestClient("https://alv.debtpaypro.com/post/e35e20706992972b1ce929f1510b3c3825160560/");
        var request = new RestRequest($"") {
            Method = Method.Post
        };
      
        request.AddParameter("offer_code", string.IsNullOrEmpty(customer.Offer)?"None":customer.Offer);
        request.AddParameter("first_name",customer.FirstName );
        request.AddParameter("last_name", customer.LastName);
        request.AddParameter("email", customer.Email);
        request.AddParameter("cell_phone", customer.Phone);
        request.AddParameter("loan_amount", customer.LoanAmount);
        
        var response = client.Execute(request);
        if (response.IsSuccessful == false) throw new InvalidOperationException(response.ErrorMessage);
    }
    /// <summary>
    ///   Get endpoint url based on loan amount and offer code
    /// </summary>
    /// <param name="customer"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private string GetEndpointUrl(ShortCustomer customer) {
        var loanAmtList = LoanAmountList.loanAmtList;
        string url;
        var claEndpoint = CrmEndpoints.claEndpoint;
        var tplEndpoint = CrmEndpoints.tplEndpoint;
        // anything under 20k goes to CLA
        if (loanAmtList.FindIndex(x => x.Value.Contains(customer.LoanAmount)) < 3) {
            url = claEndpoint;
        }
        // everything else goes to TPL
        else {
            url = tplEndpoint;
        }

        // if offer is not null then check if offer code belongs to cla or tpl
        if (!string.IsNullOrEmpty(customer.Offer)) {
            // if cla file then cla
            var directMail = _martenService.GetDirectMail(customer.Offer).Result;
            if (directMail.Team.Contains("CLA")) {
                url = claEndpoint;
            }
            // if tpl file then tpl
            else if (directMail.Team.Contains("TPL")) {
                url = tplEndpoint;
            }
        }

        if (string.IsNullOrEmpty(url)) throw new InvalidOperationException("Endpoint url is empty");
        return url;
    }

    private bool EmailContact(ShortCustomer customerResponse) {
        
        var body = "First Name: " + customerResponse.FirstName;
        body += "\nLast Name: " + customerResponse.LastName;
        body += "\nEmail: " + customerResponse.Email;
        body += "\nPhone: " + customerResponse.Phone;
        body += "\nLoan Amount: " + customerResponse.LoanAmount;
        body += "\nOffer: " + (string.IsNullOrEmpty(customerResponse.Offer)?"None":customerResponse.Offer);
        body += "\nTime(PDT): " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
        
        using var message = new MailMessage();

        var emails = _emailSettings.ToEmail.Split(',');
        foreach (var email in emails) {
            message.To.Add(email);
        }
       
        message.From = new MailAddress(_emailSettings.Username);
        message.Subject = _emailSettings.Subject;
        message.Body = body;

        using var smtp = new SmtpClient();
        smtp.EnableSsl = true;

        smtp.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
        smtp.Host = _emailSettings.Host;
        smtp.Port = _emailSettings.Port;

        try {
            smtp.Send(message);
            return true;
        }
        catch (Exception ex) {
            _logger?.LogError("Unable to send email {ex}", ex);
            return false;
        }
    }

    [HttpPost]
    [Route("Log")]
    public void JsnLogger([FromBody] JsnLogMessage obj) {
        foreach (var jsnError in obj.Payload) {
            var jsnErrorMessage = jsnError.Message.Replace("\r", "").Replace("\n", "");
            switch (jsnError.Level) {
                case (int)Level.TRACE:
                    _logger.LogTrace("{@LogMessage}", jsnErrorMessage);
                    break;
                case (int)Level.DEBUG:
                    _logger.LogDebug("{@LogMessage}", jsnErrorMessage);
                    break;
                case (int)Level.INFO:
                    _logger.LogInformation("{@LogMessage}", jsnErrorMessage);
                    break;
                case (int)Level.WARN:
                    _logger.LogWarning("{@LogMessage}", jsnErrorMessage);
                    break;
                case (int)Level.ERROR:
                    _logger.LogError("{@LogMessage}", jsnErrorMessage);
                    break;
                case (int)Level.FATAL:
                    _logger.LogCritical("{@LogMessage}", jsnErrorMessage);
                    break;
            }
        }
    }
    
    [HttpGet]
    public async Task<DirectMail?> GetPromotion(string? accessCode = null) {
        DirectMail? result = null;
        if (accessCode != null) {
            result = await _martenService.GetDirectMail(accessCode);
        }

        return result;
    }

    [HttpGet]
    public async Task<IActionResult> SubmitCode(string? accessCode = null, string? referer = null) {
        var customerGid = Guid.NewGuid();
        var promocode = new Promocode
            { Id = customerGid, Code = accessCode ?? "Missing", Referer = referer ?? "Missing" };
        await _martenService.CreatePromocode(promocode);

        return RedirectToAction("", "AboutUs", new { customerGid });
    }

    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        _logger.LogError("{HttpContextTraceIdentifier}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
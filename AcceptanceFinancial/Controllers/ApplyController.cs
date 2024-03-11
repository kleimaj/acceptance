using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using FluentValidation;
using FluentValidation.AspNetCore;
using FormHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using RestSharp;
using AcceptanceFinancial.Models;
using AcceptanceFinancial.Models.Const;
using AcceptanceFinancial.Models.Jsn;
using AcceptanceFinancial.Models.Settings;
using AcceptanceFinancial.Services;

namespace AcceptanceFinancial.Controllers;

public class ApplyController : Controller {
    private readonly ILogger<ApplyController> _logger;
    private readonly IMartenService _martenService;
    private readonly IValidator<Customer> _validator;
    private readonly EmailSettings _emailSettings;
    
    public ApplyController(IValidator<Customer> validator,  ILogger<ApplyController> logger,
        IMartenService martenService, IOptions<EmailSettings> emailSettings) {
        _validator = validator;
        _logger = logger;
        _martenService = martenService;
        _emailSettings = emailSettings.Value;
    }

    [HttpGet]
    public IActionResult Index() {
        return View();
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
        var response = new HttpResponseMessage();
        
        if (Request != null) {
            var form = await Request.ReadFormAsync();
            var token = form["cf-turnstile-response"];
            var ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();
            customer.IpAddress = ip;
            var formData = new MultipartFormDataContent();
            const string SECRET_KEY = "0x4AAAAAAAIUXGJaJKd7EGHsvUK8FToQJk4";
            formData.Add(new StringContent(SECRET_KEY), "secret");
            formData.Add(new StringContent(token), "response");
            formData.Add(new StringContent(ip), "remoteip");

            var url = "https://challenges.cloudflare.com/turnstile/v0/siteverify";
            var client = new HttpClient();
            response = await client.PostAsJsonAsync(url, formData);
        }
        else {
           response = new HttpResponseMessage(HttpStatusCode.OK);
        }

        if (response.IsSuccessStatusCode) {
            var result = await _validator.ValidateAsync(customer);
            if (!result.IsValid) {
                result.AddToModelState(ModelState);
                return View("Index", customer);
            }
            
            try {
                postToCRM(customer);
            }
            catch (Exception e) {
                _logger.LogError("{HttpContextTraceIdentifier}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
            }
            
            try {
                EmailContact(customer);
            }
            catch (Exception e) {
                _logger.LogError("{HttpContextTraceIdentifier}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
            }
            try {
                await _martenService.CreateCustomer(customer);
                
                return FormResult.CreateSuccessResult("Getting your Quote.",
                    Url.Action("", "Congratulations", new { customerGid = customer.Id }), 500);
            }
            catch {
                return FormResult.CreateErrorResult("An error occurred!");
            }
        }
        else {
            return FormResult.CreateErrorResult("Capatcha result failed!");
        }
    }
    
   /// <summary>
   ///  Post to CRM based on loan amount and offer code
   /// </summary>
   /// <param name="customer"></param>
   /// <exception cref="InvalidOperationException"></exception>
    internal void postToCRM(Customer customer) {
        var url = GetEndpointUrl(customer);

        var client = new RestClient(url);
        var request = new RestRequest($"") {
            Method = Method.Post
        };
      
        request.AddParameter("first_name",customer.FirstName );
        request.AddParameter("last_name", customer.LastName);
        request.AddParameter("email", customer.Email);
      //  request.AddParameter("address", customer.Address);
        request.AddParameter("cell_phone", customer.Cell);
      //  request.AddParameter("state", customer.State);
       // request.AddParameter("city", customer.City);
      //  request.AddParameter("zip_code", customer.Zip);
        request.AddParameter("home_phone", customer.Phone);
        request.AddParameter("loan_amount", customer.LoanAmount);
        request.AddParameter("offer_code", string.IsNullOrEmpty(customer.Offer)?"None":customer.Offer);
        
        var response = client.Execute(request);
        if (response.IsSuccessful == false) throw new InvalidOperationException(response.ErrorMessage);
        
    }

    /// <summary>
    ///   Get endpoint url based on loan amount and offer code
    /// </summary>
    /// <param name="customer"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private string GetEndpointUrl(Customer customer) {
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
            if (directMail.Team.ToUpper().Contains("CLA")) {
                url = claEndpoint;
            }
            // if tpl file then tpl
            else if (directMail.Team.ToUpper().Contains("TPL")) {
                url = tplEndpoint;
            }
        }

        if (string.IsNullOrEmpty(url)) throw new InvalidOperationException("Endpoint url is empty");
        return url;
    }

    private bool EmailContact(Customer customerResponse) {
        
        var body = "First Name: " + customerResponse.FirstName;
        body += "\nLast Name: " + customerResponse.LastName;
        body += "\nEmail: " + customerResponse.Email;
      //  body += "\nAddress 1: " + customerResponse.Address;
      //  body += "\nAddress 2: " + customerResponse.Address2;
     //   body += "\nCity: " + customerResponse.City;
     //   body += "\nState: " + customerResponse.State;
      //  body += "\nZip Code: " + customerResponse.Zip;
        body += "\nCell Phone: " + customerResponse.Cell;
        body += "\nHome Phone: " + customerResponse.Phone;
        body += "\nloan Amount: " + customerResponse.LoanAmount;
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
}
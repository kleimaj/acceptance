using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using FluentValidation;
using FluentValidation.AspNetCore;
using FormHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestSharp;
using AcceptanceFinancial.Models;
using AcceptanceFinancial.Models.Jsn;
using AcceptanceFinancial.Models.Settings;
using AcceptanceFinancial.Services;

namespace AcceptanceFinancial.Controllers;

public class ContactUsController : Controller {
    private readonly ILogger<ContactUsController> _logger;
    private readonly IMartenService _martenService;
    private readonly IValidator<ContactUs> _validator;
    private readonly EmailSettings _emailSettings;
    
    public ContactUsController(IValidator<ContactUs> validator, ILogger<ContactUsController> logger,
        IMartenService martenService, IOptions<EmailSettings> emailSettings) {
        _validator = validator;
        _logger = logger;
        _martenService = martenService;
        _emailSettings = emailSettings.Value;
    }

    [HttpGet("contact-us")]
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
    public async Task<IActionResult> Save(ContactUs contactUs) {
        var result = await _validator.ValidateAsync(contactUs);
        if (!result.IsValid) {
            result.AddToModelState(ModelState);
            return View("Index", contactUs);
        }

        try {
            EmailContact(contactUs);
        }
        catch (Exception e) {
            _logger.LogError("{HttpContextTraceIdentifier}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        }

        try {
            await _martenService.CreateContactUs(contactUs);
            
            return FormResult.CreateSuccessResult("Message saved.",
                Url.Action("", "Home"), 500);
        }
        catch {
            return FormResult.CreateErrorResult("An error occurred!");
        }
    }
    
   private bool EmailContact(ContactUs customerResponse) {
        
        var body = "Name: " + customerResponse.Name;
        body += "\nEmail: " + customerResponse.Email;
        body += "\nSubject: " + customerResponse.Subject;
        body += "\nMessage: " + customerResponse.Message;
        body += "\nPhone: " + customerResponse.Phone;
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
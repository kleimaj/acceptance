using Marten;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog.Core;
using AcceptanceFinancial.Models;
using AcceptanceFinancial.Models.Settings;

namespace AcceptanceFinancial.Services;

public class MartenService : IMartenService {
    private readonly DmsApiConfig _configuration;
    private readonly EmailSettings _emailSettings;
    private readonly Logger _logger;
    private readonly DocumentStore? _store;

    public MartenService(IOptions<DmsApiConfig> dmsApiConfig, IOptions<EmailSettings> emailSettings,
        IDmsLoggerService dmsLoggerService, IDocumentStore documentStore) {
        _configuration = dmsApiConfig.Value;
        _emailSettings = emailSettings.Value;
        _logger = dmsLoggerService.Logger;
        _store = documentStore as DocumentStore;

        Console.WriteLine(JsonConvert.SerializeObject(_emailSettings) + JsonConvert.SerializeObject(_configuration));
        // _logger = //DmsLoggerService.GetLoggerConfig(_configuration.PostgresConnectionString, _emailSettings).CreateLogger();
        //_logger.Information("Email settings are {emailSettings}", JsonConvert.SerializeObject( _emailSettings));
        //_logger.Information("dmsApiConfig settings are {dmsApiConfig}", JsonConvert.SerializeObject( _configuration));
        _logger.Information("PostgresService created");
    }

    public async Task<bool> CreatePromocode(Promocode promocode) {
        try {
            await using var session = _store!.LightweightSession();
            session.Store(promocode);

            await session.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
        catch (Exception ex) {
            _logger.Error("Unable to create customer {@ex} {@promocode}", ex, promocode);
            return false;
        }
    }

    public async Task<bool> CreateCustomer(Customer customer) {
        try {
            await using var session = _store!.LightweightSession();
            session.Store(customer);

            await session.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
        catch (Exception ex) {
            _logger.Error("Unable to create customer {@ex} {@customer}", ex, customer);
            return false;
        }
    }
    public async Task<bool> CreateShortCustomer(ShortCustomer customer) {
        try {
            await using var session = _store!.LightweightSession();
            session.Store(customer);

            await session.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
        catch (Exception ex) {
            _logger.Error("Unable to create customer {@ex} {@customer}", ex, customer);
            return false;
        }
    }
    public async Task<bool> CreateContactUs(ContactUs contactUs) {
        try {
            await using var session = _store!.LightweightSession();
            session.Store(contactUs);

            await session.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
        catch (Exception ex) {
            _logger.Error("Unable to create contactUs {@ex} {@customer}", ex, contactUs);
            return false;
        }
    }
    public async Task<bool> CreateCustomerLoanInfo(CustomerLoanInfo customerLoanInfo) {
        try {
            await using var session = _store!.LightweightSession();
            session.Store(customerLoanInfo);

            await session.SaveChangesAsync().ConfigureAwait(false);
            return true;
        }
        catch (Exception ex) {
            _logger.Error("Unable to create customerLoanInfo {@ex} {@customerLoanInfo}", ex, customerLoanInfo);
            return false;
        }
    }
    public async Task<DirectMail?> GetDirectMail(string Promocode) {
        try {
            await using var session = _store!.LightweightSession();
            var directMail = await session.LoadAsync<DirectMail>(Promocode);

            return directMail;
        }
        catch (Exception ex) {
            _logger.Error("Unable to get DirectMail for {@ex} {@Promocode}", ex, Promocode);
            return null;
        }
    }
    public async Task<bool> CreateDirectMails(List<DirectMail> directMails) {
        try {
            await _store!.BulkInsertAsync(directMails.DistinctBy(x => x.AccessCode).ToList(),
                BulkInsertMode.IgnoreDuplicates, 500);
            return true;
        }
        catch (Exception ex) {
            _logger.Error("Unable to create DirectMails {@ex} {@directMails}", ex, directMails);
            return false;
        }
    }
}
using System.Net;
using Microsoft.Extensions.Options;
using NpgsqlTypes;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.Email;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;
using AcceptanceFinancial.Models.Settings;

namespace AcceptanceFinancial.Services;

public class DmsLoggerService : IDmsLoggerService {
    public DmsLoggerService(IOptions<DmsApiConfig> dmsApiConfig, IOptions<EmailSettings> emailSettings) {
        _dmsApiConfig = dmsApiConfig.Value;
        _emailSettings = emailSettings.Value;

        Logger = GetLoggerConfig().CreateLogger();
    }

    private DmsApiConfig _dmsApiConfig { get; }
    private EmailSettings _emailSettings { get; }
    public Logger Logger { get; set; }

    public IDictionary<string, ColumnWriterBase> GetColumnConfig() {
        return new Dictionary<string, ColumnWriterBase> {
            { "message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
            { "message_template", new MessageTemplateColumnWriter(NpgsqlDbType.Text) },
            { "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
            { "raise_date", new TimestampColumnWriter(NpgsqlDbType.TimestampTz) },
            { "exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
            { "properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
            { "props_test", new PropertiesColumnWriter(NpgsqlDbType.Jsonb) }, {
                "machine_name",
                new SinglePropertyColumnWriter("MachineName", PropertyWriteMethod.ToString, NpgsqlDbType.Text, "l")
            }
        };
    }

    public LoggerConfiguration GetLoggerConfig() {
        return new LoggerConfiguration()
            .MinimumLevel.Error()
            .Enrich.FromLogContext()
            .WriteTo.Console(LogEventLevel.Verbose)
            .WriteTo.Email(new EmailConnectionInfo {
                FromEmail = _emailSettings.FromEmail,
                ToEmail = _emailSettings.ToEmail,
                MailServer = _emailSettings.Host,
                NetworkCredentials = new NetworkCredential {
                    UserName = _emailSettings.Username,
                    Password = _emailSettings.Password
                },
                EnableSsl = _emailSettings.UseSsl == "true",
                Port = _emailSettings.Port,
                EmailSubject = _emailSettings.Subject
            }, restrictedToMinimumLevel: LogEventLevel.Error, batchPostingLimit: 1)
            .WriteTo.PostgreSQL(_dmsApiConfig.PostgresConnectionString, "logs", GetColumnConfig(),
                needAutoCreateTable: true,
                restrictedToMinimumLevel: LogEventLevel.Error, batchSizeLimit: 1);
    }

    public LoggerConfiguration GetLoggerConfig(string loggerDbConnectionString, EmailSettings emailSettings) {
        return new LoggerConfiguration()
            .MinimumLevel.Error()
            .Enrich.FromLogContext()
            .WriteTo.Console(LogEventLevel.Verbose)
            .WriteTo.Email(new EmailConnectionInfo {
                FromEmail = emailSettings.FromEmail,
                ToEmail = emailSettings.ToEmail,
                MailServer = emailSettings.Host,
                NetworkCredentials = new NetworkCredential {
                    UserName = emailSettings.Username,
                    Password = emailSettings.Password
                },
                EnableSsl = emailSettings.UseSsl == "true",
                Port = emailSettings.Port,
                EmailSubject = emailSettings.Subject
            }, restrictedToMinimumLevel: LogEventLevel.Error, batchPostingLimit: 1)
            .WriteTo.PostgreSQL(loggerDbConnectionString, "logs", GetColumnConfig(), needAutoCreateTable: true,
                restrictedToMinimumLevel: LogEventLevel.Error, batchSizeLimit: 1);
    }
}
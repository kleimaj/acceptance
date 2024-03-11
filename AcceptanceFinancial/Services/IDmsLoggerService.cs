using Serilog;
using Serilog.Core;
using Serilog.Sinks.PostgreSQL.ColumnWriters;

namespace AcceptanceFinancial.Services;

public interface IDmsLoggerService {
    public Logger Logger { get; set; }
    public LoggerConfiguration GetLoggerConfig();

    public IDictionary<string, ColumnWriterBase> GetColumnConfig();
}
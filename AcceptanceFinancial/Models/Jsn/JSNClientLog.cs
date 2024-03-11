using System.Text.Json.Serialization;

namespace AcceptanceFinancial.Models.Jsn;

public class JsnClientLog {
    public JsnLogMessage LogMessage { get; set; }
    public JsnPayload Payload { get; set; }
}

public class JsnLogMessage {
    [JsonPropertyName("r")] public string RequestId { get; set; }

    [JsonPropertyName("lg")] public JsnPayload[] Payload { get; set; }
}

public class JsnPayload {
    [JsonPropertyName("l")] public int Level { get; set; }

    [JsonPropertyName("m")] public string Message { get; set; }

    [JsonPropertyName("n")] public string Source { get; set; }

    [JsonPropertyName("t")] public long Timestamp { get; set; }

    [JsonPropertyName("u")] public long EventId { get; set; }
}
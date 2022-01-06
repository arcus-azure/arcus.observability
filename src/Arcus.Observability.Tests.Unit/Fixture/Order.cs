using System;
using System.Text.Json.Serialization;

namespace Arcus.Observability.Tests.Unit.Fixture
{
    public class Order
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("orderNumber")]
        public int OrderNumber { get; set; }

        [JsonPropertyName("scheduled")]
        public DateTimeOffset Scheduled { get; set; }
    }
}

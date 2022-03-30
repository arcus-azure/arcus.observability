using System;
using Bogus;

namespace Arcus.Observability.Tests.Unit.Telemetry
{
    public static class TimeSpanGenerator
    {
        private static readonly Faker _bogusGenerator = new Faker();

        public static TimeSpan GeneratePositiveDuration()
        {
            TimeSpan duration = _bogusGenerator.Date.Timespan();
            if (duration < TimeSpan.Zero)
            {
                return duration.Negate();
            }

            return duration;
        }
    }
}

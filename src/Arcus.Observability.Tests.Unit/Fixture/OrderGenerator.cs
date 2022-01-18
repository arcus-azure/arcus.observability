using System;
using System.Collections.Generic;
using System.Text;
using Bogus;
using Moq;

namespace Arcus.Observability.Tests.Unit.Fixture
{
    public static class OrderGenerator
    {
        public static Order Generate()
        {
            Order order = new Faker<Order>()
                .RuleFor(model => model.Id, bogus => bogus.Random.Guid().ToString())
                .RuleFor(model => model.OrderNumber, bogus => bogus.Random.Int())
                .RuleFor(model => model.Scheduled, bogus => bogus.Date.RecentOffset())
                .Generate();

            return order;
        }
    }
}

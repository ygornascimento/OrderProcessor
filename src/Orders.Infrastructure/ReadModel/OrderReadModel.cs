using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Orders.Infrastructure.ReadModel
{
    internal class OrderReadModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; init; }

        [BsonElement("customerName")]
        public string CustomerName { get; init; } = string.Empty;

        [BsonElement("amount")]
        public decimal Amount { get; init; }

        [BsonElement("orderDate")]
        public DateTime OrderDate { get; init; }
    }
}

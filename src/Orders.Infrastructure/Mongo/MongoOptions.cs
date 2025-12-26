using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Infrastructure.Mongo
{
    public class MongoOptions
    {
        public const string SectionName = "Mongo";

        public string ConnectionString { get; init; } = string.Empty;
        public string Database {  get; init; } = "OrdersReadDb";
        public string OrdersCollection {  get; init; } = "orders";
    }
}

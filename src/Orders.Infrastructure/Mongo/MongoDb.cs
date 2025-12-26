using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orders.Infrastructure.Mongo
{
    public class MongoDb
    {
        public IMongoDatabase Database { get; }

        public MongoDb(IOptions<MongoOptions> options)
        {
            var opt = options.Value;

            if (string.IsNullOrWhiteSpace(opt.ConnectionString))
            {
                throw new InvalidOperationException("Mongo: ConnectionString não configurada.");
            }

            var client = new MongoClient(opt.ConnectionString);
            Database = client.GetDatabase(opt.Database);
        }
    }
}

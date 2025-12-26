using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Orders.Infrastructure.ReadModel;

namespace Orders.Infrastructure.Mongo
{
    public class OrderReadModelWriter
    {
        private readonly MongoDb _mongoDb;
        private readonly MongoOptions _options;

        public OrderReadModelWriter(MongoDb mongoDb, IOptions<MongoOptions> options)
        {
            _mongoDb = mongoDb;
            _options = options.Value;
        }

        public async Task UpsertAsync(OrderReadModel doc, CancellationToken ct = default) 
        {
            var collection = _mongoDb.Database.GetCollection<OrderReadModel>(_options.OrdersCollection);
            var filter  = Builders<OrderReadModel>.Filter.Eq(x => x.Id, doc.Id);

            await collection.ReplaceOneAsync(
                filter: filter,
                replacement: doc,
                options: new ReplaceOptions { IsUpsert = true },
                cancellationToken: ct
                );
        }

    }
}

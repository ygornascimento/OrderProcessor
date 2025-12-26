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
    public class OrderReadModelReader
    {
        private readonly MongoDb _mongoDb;
        private readonly MongoOptions _options;

        public OrderReadModelReader(MongoDb mongoDb, IOptions<MongoOptions> options)
        {
            _mongoDb = mongoDb;
            _options = options.Value;
        }

        public async Task<IReadOnlyList<OrderReadModel>> GetLatestAsync(int limit, CancellationToken ct = default) 
        {
            var collection = _mongoDb.Database.GetCollection<OrderReadModel>(_options.OrdersCollection);

            // Todos os documentos
            var filter = Builders<OrderReadModel>.Filter.Empty;

            var docs = await collection
                .Find(filter)
                .SortByDescending(x => x.OrderDate)
                .Limit(limit)
                .ToListAsync(ct);

            return docs;
        }
    }
}

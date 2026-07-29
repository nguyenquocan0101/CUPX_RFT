

using System.Linq.Expressions;
using CouchDB.Driver;
using CouchDB.Driver.Types;


namespace WorkflowExecutorTest.CouchDbInteraction
{
    public class CouchDbClient
    {
        private readonly CouchClient _client;
        public CouchDbClient()
        {
            _client = new CouchClient("http://localhost:5984", builder => builder.UseBasicAuthentication("sa", "12345"));
        }

        public async Task<ICouchDatabase<T>> GetDbAsync<T>(string dbName) where T : CouchDocument
        {
            return await _client.GetOrCreateDatabaseAsync<T>(dbName);
        }

        public async Task<T?> GetDataAsync<T>(string dbName, Func<T,bool> predicate) where T : CouchDocument
        {
            var db = await GetDbAsync<T>(dbName);

            return db.Where(predicate).FirstOrDefault();

        }

        public async Task<bool> AddDocumentAsync<T>(string dbName, T document) where T : CouchDocument
        {
            var db = await GetDbAsync<T>(dbName);

            var addedModel = await db.AddAsync(document);
            return addedModel != null;

        }

    }
}

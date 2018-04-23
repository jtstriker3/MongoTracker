using System;
using System.Security.Authentication;
using MongoDB.Driver;
using MongoTracker.Interfaces;

namespace MongoTracker
{
    public class DefaultMongoDBFactory : IMongoDBFactory
    {
        public DefaultMongoDBFactory()
        {
        }

        public IMongoDatabase CreateMongoDatabaseConnection(MongoUrl mongoUrl, bool useSsl){
            MongoClientSettings settings = MongoClientSettings.FromUrl(mongoUrl);
            System.Diagnostics.Debug.WriteLine(mongoUrl);
            if (useSsl)
                settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };

            MongoClient client = new MongoClient(settings);
            return client.GetDatabase(mongoUrl.DatabaseName);
        }
    }
}

using MongoDB.Driver;

namespace MongoTracker.Interfaces
{
    public interface IMongoDBFactory
    {
        IMongoDatabase CreateMongoDatabaseConnection(MongoUrl mongoUrl, bool useSsl);
    }
}
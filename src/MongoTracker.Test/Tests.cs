using MongoTracker.Test.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MongoTracker.Test
{
    public class Tests
    {
        IConfigurationRoot Configuration { get; set; }


        public Tests()
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("config.json");

            Configuration = builder.Build();
        }

        [Fact]
        public async Task AddBaby()
        {
            var connectionString = Configuration.GetConnectionString("mongoTest");
            var context = new TestMongoContext(connectionString);

            var newBaby = new Baby() { Name = "Whitney", BirthDay = DateTime.Now };

            context.Babies.Add(newBaby);

            await context.SaveChanges();
        }

        [Fact]
        public async Task ModifyBaby()
        {

            var connectionString = Configuration.GetConnectionString("mongoTest");
            var context = new TestMongoContext(connectionString);

            var baby = context.Babies.FirstOrDefault(b => b.Name == "William" || b.Name == "Whitney");

            baby.Name = baby.Name == "Whitney" ? "William" : "Whitney";

            var result = await context.SaveChanges();

            Assert.Equal(1, result);
        }
    }
}

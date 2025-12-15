using MongoDB.Driver;
using ProMeet.Models;

namespace ProMeet.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var connectionString = configuration["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(configuration["MongoDB:DatabaseName"] ?? "ProMeetDB");
        }

        public IMongoDatabase Database => _database;

        
        public IMongoCollection<Professional> Professionals => _database.GetCollection<Professional>("Professionals");
        public IMongoCollection<Appointment> Appointments => _database.GetCollection<Appointment>("Appointments");
        public IMongoCollection<Availability> Availabilities => _database.GetCollection<Availability>("Availabilities");
        public IMongoCollection<Review> Reviews => _database.GetCollection<Review>("Reviews");
        public IMongoCollection<Category> Categories => _database.GetCollection<Category>("Categories");
        public IMongoCollection<Chat> Chats => _database.GetCollection<Chat>("Chats");
        public IMongoCollection<Message> Messages => _database.GetCollection<Message>("Messages");
        public IMongoCollection<Service> Services => _database.GetCollection<Service>("Services");
        public IMongoCollection<Annonce> Annonces => _database.GetCollection<Annonce>("Annonces");
        public IMongoCollection<Notification> Notifications => _database.GetCollection<Notification>("Notifications");
    }
}
using MongoDB.Bson;
using MongoDB.Driver;
using ProMeet.Models;

namespace ProMeet.Data
{
    public class MongoDbMigrationService
    {
        private readonly IMongoDatabase _database;
        private readonly MongoDbContext _context;

        public MongoDbMigrationService(MongoDbContext context)
        {
            _context = context;
            _database = context.Database;
        }

        public async Task<bool> MigrateAllModelsAsync()
        {
            try
            {
                // Create collections if they don't exist
                await CreateCollectionsAsync();
                
                // Create indexes for better performance
                await CreateIndexesAsync();
                
                // Seed initial data if collections are empty
                await SeedInitialDataAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Migration failed: {ex.Message}");
                return false;
            }
        }

        private async Task CreateCollectionsAsync()
        {
            var collections = await _database.ListCollectionNames().ToListAsync();
            var collectionNames = new[] { "users", "professionals", "appointments", "reviews", "categories", "availabilities", "chats", "messages" };
            
            foreach (var collectionName in collectionNames)
            {
                if (!collections.Contains(collectionName))
                {
                    await _database.CreateCollectionAsync(collectionName);
                    Console.WriteLine($"Created collection: {collectionName}");
                }
            }
        }

        private async Task CreateIndexesAsync()
        {
            // Users collection indexes
            var usersCollection = _context.Users;
            await usersCollection.Indexes.CreateOneAsync(new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.UserID),
                new CreateIndexOptions { Unique = true }));
            await usersCollection.Indexes.CreateOneAsync(new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.Email)));

            // Professionals collection indexes
            var professionalsCollection = _context.Professionals;
            await professionalsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Professional>(
                Builders<Professional>.IndexKeys.Ascending(p => p.ProfessionalID),
                new CreateIndexOptions { Unique = true }));
            await professionalsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Professional>(
                Builders<Professional>.IndexKeys.Ascending(p => p.UserID)));
            await professionalsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Professional>(
                Builders<Professional>.IndexKeys.Ascending(p => p.CategoryID)));

            // Appointments collection indexes
            var appointmentsCollection = _context.Appointments;
            await appointmentsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Appointment>(
                Builders<Appointment>.IndexKeys.Ascending(a => a.AppointmentID),
                new CreateIndexOptions { Unique = true }));
            await appointmentsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Appointment>(
                Builders<Appointment>.IndexKeys.Ascending(a => a.ClientID)));
            await appointmentsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Appointment>(
                Builders<Appointment>.IndexKeys.Ascending(a => a.ProfessionalID)));
            await appointmentsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Appointment>(
                Builders<Appointment>.IndexKeys.Ascending(a => a.Date)));

            // Reviews collection indexes
            var reviewsCollection = _context.Reviews;
            await reviewsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Review>(
                Builders<Review>.IndexKeys.Ascending(r => r.ReviewID),
                new CreateIndexOptions { Unique = true }));
            await reviewsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Review>(
                Builders<Review>.IndexKeys.Ascending(r => r.ProfessionalID)));
            await reviewsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Review>(
                Builders<Review>.IndexKeys.Ascending(r => r.AppointmentID)));

            // Categories collection indexes
            var categoriesCollection = _context.Categories;
            await categoriesCollection.Indexes.CreateOneAsync(new CreateIndexModel<Category>(
                Builders<Category>.IndexKeys.Ascending(c => c.CategoryID),
                new CreateIndexOptions { Unique = true }));

            // Availabilities collection indexes
            var availabilitiesCollection = _context.Availabilities;
            await availabilitiesCollection.Indexes.CreateOneAsync(new CreateIndexModel<Availability>(
                Builders<Availability>.IndexKeys.Ascending(av => av.AvailabilityID),
                new CreateIndexOptions { Unique = true }));
            await availabilitiesCollection.Indexes.CreateOneAsync(new CreateIndexModel<Availability>(
                Builders<Availability>.IndexKeys.Ascending(av => av.ProfessionalID)));

            // Chats collection indexes
            var chatsCollection = _context.Chats;
            await chatsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Chat>(
                Builders<Chat>.IndexKeys.Ascending(c => c.ChatID),
                new CreateIndexOptions { Unique = true }));
            await chatsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Chat>(
                Builders<Chat>.IndexKeys.Ascending(c => c.ClientID).Ascending(c => c.ProfessionalID)));

            Console.WriteLine("All indexes created successfully");
        }

        private async Task SeedInitialDataAsync()
        {
            // Seed categories if empty
            var categoriesCollection = _context.Categories;
            var categoryCount = await categoriesCollection.CountDocumentsAsync(_ => true);
            
            if (categoryCount == 0)
            {
                var categories = new[]
                {
                    new Category { CategoryID = 1, Name = "Doctor", Description = "Medical professionals", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Category { CategoryID = 2, Name = "Lawyer", Description = "Legal professionals", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Category { CategoryID = 3, Name = "Teacher", Description = "Education professionals", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Category { CategoryID = 4, Name = "Engineer", Description = "Engineering professionals", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                    new Category { CategoryID = 5, Name = "Consultant", Description = "Business consultants", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
                };
                
                await categoriesCollection.InsertManyAsync(categories);
                Console.WriteLine("Seeded initial categories");
            }

            // Seed sample users if empty
            var usersCollection = _context.Users;
            var userCount = await usersCollection.CountDocumentsAsync(_ => true);
            
            if (userCount == 0)
            {
                var users = new[]
                {
                    new User 
                    { 
                        UserID = 1, 
                        Name = "Admin User", 
                        Email = "admin@prommeet.com", 
                        Password = "admin123", // In real app, this should be hashed
                        UserType = "Admin", 
                        Phone = "1234567890",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new User 
                    { 
                        UserID = 2, 
                        Name = "John Doe", 
                        Email = "john@example.com", 
                        Password = "password123",
                        UserType = "Client", 
                        Phone = "0987654321",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };
                
                await usersCollection.InsertManyAsync(users);
                Console.WriteLine("Seeded initial users");
            }

            Console.WriteLine("Database migration completed successfully!");
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await _database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));
                Console.WriteLine("MongoDB connection test successful!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MongoDB connection test failed: {ex.Message}");
                return false;
            }
        }
    }
}
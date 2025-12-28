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


            // Professionals collection indexes
            var professionalsCollection = _context.Professionals;
            await professionalsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Professional>(
                Builders<Professional>.IndexKeys.Ascending(p => p.ProfessionalID),
                new CreateIndexOptions { Unique = true, Name = "idx_professional_id" }));

            await professionalsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Professional>(
                Builders<Professional>.IndexKeys.Ascending(p => p.CategoryID),
                new CreateIndexOptions { Name = "idx_professional_categoryId" }));

            // Appointments collection indexes
            var appointmentsCollection = _context.Appointments;
            await appointmentsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Appointment>(
                Builders<Appointment>.IndexKeys.Ascending(a => a.AppointmentID),
                new CreateIndexOptions { Unique = true, Name = "idx_appointment_id" }));
            await appointmentsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Appointment>(
                Builders<Appointment>.IndexKeys.Ascending(a => a.ClientID),
                new CreateIndexOptions { Name = "idx_appointment_clientId" }));
            await appointmentsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Appointment>(
                Builders<Appointment>.IndexKeys.Ascending(a => a.ProfessionalID),
                new CreateIndexOptions { Name = "idx_appointment_professionalId" }));
            await appointmentsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Appointment>(
                Builders<Appointment>.IndexKeys.Ascending(a => a.Date),
                new CreateIndexOptions { Name = "idx_appointment_date" }));

            // Reviews collection indexes
            var reviewsCollection = _context.Reviews;
            await reviewsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Review>(
                Builders<Review>.IndexKeys.Ascending(r => r.ReviewID),
                new CreateIndexOptions { Unique = true, Name = "idx_review_id" }));
            await reviewsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Review>(
                Builders<Review>.IndexKeys.Ascending(r => r.ProfessionalID),
                new CreateIndexOptions { Name = "idx_review_professionalId" }));
            await reviewsCollection.Indexes.CreateOneAsync(new CreateIndexModel<Review>(
                Builders<Review>.IndexKeys.Ascending(r => r.AppointmentID),
                new CreateIndexOptions { Name = "idx_review_appointmentId" }));

            // Categories collection indexes
            var categoriesCollection = _context.Categories;
            await categoriesCollection.Indexes.CreateOneAsync(new CreateIndexModel<Category>(
                Builders<Category>.IndexKeys.Ascending(c => c.CategoryID),
                new CreateIndexOptions { Unique = true, Name = "idx_category_id" }));

            // Availabilities collection indexes
            var availabilitiesCollection = _context.Availabilities;
            await availabilitiesCollection.Indexes.CreateOneAsync(new CreateIndexModel<Availability>(
                Builders<Availability>.IndexKeys.Ascending(av => av.AvailabilityID),
                new CreateIndexOptions { Unique = true, Name = "idx_availability_id" }));
            await availabilitiesCollection.Indexes.CreateOneAsync(new CreateIndexModel<Availability>(
                Builders<Availability>.IndexKeys.Ascending(av => av.ProfessionalID),
                new CreateIndexOptions { Name = "idx_availability_professionalId" }));

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
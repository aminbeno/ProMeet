using ProMeet.Data;

var builder = WebApplication.CreateBuilder(args);

// Add MongoDB configuration
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<MongoDbMigrationService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// Custom routes for better URL structure
app.MapControllerRoute(
    name: "professional_details",
    pattern: "professional/{id:int}",
    defaults: new { controller = "Professional", action = "Details" });

app.MapControllerRoute(
    name: "professional_list",
    pattern: "professionals",
    defaults: new { controller = "Professional", action = "Index" });

app.MapControllerRoute(
    name: "book_appointment",
    pattern: "book/{professionalId}",
    defaults: new { controller = "Appointment", action = "Book" });

app.MapControllerRoute(
    name: "appointment_confirm",
    pattern: "appointment/confirm/{id}",
    defaults: new { controller = "Appointment", action = "Confirm" });

app.MapControllerRoute(
    name: "user_appointments",
    pattern: "my-appointments",
    defaults: new { controller = "Appointment", action = "List" });

app.MapControllerRoute(
    name: "professional_dashboard",
    pattern: "dashboard",
    defaults: new { controller = "Professional", action = "Dashboard" });

app.MapControllerRoute(
    name: "professional_profile",
    pattern: "profile",
    defaults: new { controller = "Professional", action = "ManageProfile" });

app.MapControllerRoute(
    name: "professional_availability",
    pattern: "availability",
    defaults: new { controller = "Professional", action = "ManageAvailability" });

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Run MongoDB migrations
using (var scope = app.Services.CreateScope())
{
    var migrationService = scope.ServiceProvider.GetRequiredService<MongoDbMigrationService>();
    var mongoContext = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    
    // Test MongoDB connection
    var isConnected = await migrationService.TestConnectionAsync();
    if (isConnected)
    {
        Console.WriteLine("MongoDB connection successful. Running migrations...");
        var migrationSuccess = await migrationService.MigrateAllModelsAsync();
        if (migrationSuccess)
        {
            Console.WriteLine("MongoDB migrations completed successfully!");
        }
        else
        {
            Console.WriteLine("MongoDB migrations failed!");
        }
    }
    else
    {
        Console.WriteLine("Failed to connect to MongoDB. Please check your connection settings.");
    }
}

app.Run();

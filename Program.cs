using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using ProMeet.Models;
using System.Text;
using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using AspNetCore.Identity.MongoDbCore.Models;
using ProMeet.Data;

var builder = WebApplication.CreateBuilder(args);

// MongoDB Configuration
var mongoDbSettings = builder.Configuration.GetSection("MongoDB");
var connectionString = mongoDbSettings["ConnectionString"];
var databaseName = mongoDbSettings["DatabaseName"];

// Configure Identity
var identityOptions = (IdentityOptions options) =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
};

builder.Services.AddIdentity<ApplicationUser, MongoIdentityRole<Guid>>(identityOptions)
    .AddMongoDbStores<ApplicationUser, MongoIdentityRole<Guid>, Guid>(connectionString, databaseName)
    .AddDefaultTokenProviders();


// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JWT");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// MongoDB Client for custom collections
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    return new MongoClient(connectionString);
});

builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddScoped(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(databaseName);
});

// Add services to the container
builder.Services.AddTransient<RoleSeeder>();
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();

// Hosted Services
builder.Services.AddHostedService<ProMeet.Services.AppointmentReminderService>();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed roles
using (var scope = app.Services.CreateScope())
{
    var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
    roleSeeder.SeedRolesAsync().Wait();
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ProMeet.Hubs.NotificationHub>("/notificationHub");

app.Run();
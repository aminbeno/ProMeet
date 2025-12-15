using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;

namespace ProMeet.Data
{
    public class RoleSeeder
    {
        private readonly RoleManager<MongoIdentityRole<Guid>> _roleManager;

        public RoleSeeder(RoleManager<MongoIdentityRole<Guid>> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task SeedRolesAsync()
        {
            if (!await _roleManager.RoleExistsAsync("Professional"))
            {
                await _roleManager.CreateAsync(new MongoIdentityRole<Guid>("Professional"));
            }

            if (!await _roleManager.RoleExistsAsync("Client"))
            {
                await _roleManager.CreateAsync(new MongoIdentityRole<Guid>("Client"));
            }
        }
    }
}
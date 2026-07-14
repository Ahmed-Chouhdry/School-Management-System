using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Domain.Entities;
using SchoolManagementSystem.Domain.Enums;

namespace SchoolManagementSystem.Infrastructure.Persistence;

[ExcludeFromCodeCoverage]
public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        var users = new List<User>
        {
            new("Ahmed", "Javed", "ahmed.javed@school.edu", UserRole.Admin),
            new("Sara", "Khan", "sara.khan@school.edu", UserRole.Admin),
            new("Bilal", "Ahmed", "bilal.ahmed@school.edu", UserRole.Admin),
            new("Hina", "Riaz", "hina.riaz@school.edu", UserRole.Student),
            new("Zain", "Malik", "zain.malik@school.edu", UserRole.Student),
            new("Fatima", "Noor", "fatima.noor@school.edu", UserRole.Student, UserStatus.Inactive),
        };

        users[3].ApplyWalletAdjustment(500, "Initial wallet top-up (seed data)");
        users[4].ApplyWalletAdjustment(250, "Initial wallet top-up (seed data)");

        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
    }
}
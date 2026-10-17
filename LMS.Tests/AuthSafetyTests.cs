using System.ComponentModel.DataAnnotations;
using LMS.Models;
using LMS.ViewModels;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace LMS.Tests;

public class AuthSafetyTests
{
    private static bool IsValid(RegisterViewModel model)
    {
        return Validator.TryValidateObject(model,
            new ValidationContext(model), new List<ValidationResult>(), validateAllProperties: true);
    }

    private static RegisterViewModel ValidRegistration(string role = "Student") => new()
    {
        FullName = "Portfolio Test User",
        Email = "student@example.test",
        Password = "StrongPass1!",
        Role = role
    };

    [Theory]
    [InlineData("Student")]
    [InlineData("Teacher")]
    public void PublicSignup_AllowsOnlyExpectedRoles(string role)
    {
        Assert.True(IsValid(ValidRegistration(role)));
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("SuperAdmin")]
    [InlineData("")]
    public void PublicSignup_RejectsPrivilegedOrUnknownRoles(string role)
    {
        Assert.False(IsValid(ValidRegistration(role)));
    }

    [Theory]
    [InlineData("short")]
    [InlineData("nouppercase1!")]
    [InlineData("NOLOWERCASE1!")]
    public void PublicSignup_RejectsWeakPasswords(string password)
    {
        var model = ValidRegistration();
        model.Password = password;
        Assert.False(IsValid(model));
    }

    [Fact]
    public void PasswordHasher_NeverStoresRawPassword()
    {
        const string plaintext = "StrongPass1!";
        var user = new User { FullName = "Test", Email = "test@example.test", Role = "Student" };
        var hasher = new PasswordHasher<User>();

        var storedHash = hasher.HashPassword(user, plaintext);

        Assert.NotEqual(plaintext, storedHash);
        Assert.Equal(PasswordVerificationResult.Success,
            hasher.VerifyHashedPassword(user, storedHash, plaintext));
        Assert.Equal(PasswordVerificationResult.Failed,
            hasher.VerifyHashedPassword(user, storedHash, "WrongPass1!"));
    }
}

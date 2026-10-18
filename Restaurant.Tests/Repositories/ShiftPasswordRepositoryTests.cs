using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Restaurant.DAL;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Enums;

namespace Restaurant.Tests.Repositories;

public class ShiftPasswordRepositoryTests
{
    private readonly ShiftPasswordRepository _repository = new ShiftPasswordRepository();

    [Fact]
    public void Hash_DoesNotKeepThePlainPassword()
    {
        var hash = _repository.Hash("1234");

        Assert.NotEqual("1234", hash);
        Assert.StartsWith("AQAAAA", hash);
    }

    [Fact]
    public void Verify_RightPassword_ReturnsTrue()
    {
        var hash = _repository.Hash("1234");

        var result = _repository.Verify(hash, "1234", out var needsRehash);

        Assert.True(result);
        Assert.False(needsRehash);
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = _repository.Hash("1234");

        var result = _repository.Verify(hash, "9999", out _);

        Assert.False(result);
    }

    [Theory]
    [InlineData(null, "1234")]
    [InlineData("", "1234")]
    [InlineData("1234", null)]
    [InlineData("1234", "")]
    public void Verify_EmptyStoredOrEmptyPassword_ReturnsFalse(string stored, string password)
    {
        var result = _repository.Verify(stored, password, out var needsRehash);

        Assert.False(result);
        Assert.False(needsRehash);
    }

    [Fact]
    public void Verify_OldPlainTextPassword_WorksAndAsksForARehash()
    {
        var result = _repository.Verify("1234", "1234", out var needsRehash);

        Assert.True(result);
        Assert.True(needsRehash);
    }

    [Fact]
    public void Verify_OldPlainTextPasswordWithWrongPassword_ReturnsFalse()
    {
        var result = _repository.Verify("1234", "0000", out var needsRehash);

        Assert.False(result);
        Assert.False(needsRehash);
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Restaurant.DAL.Models;
using Restaurant.DAL.Repositories.Definitions;
using Shared.Helpers;

namespace Restaurant.Tests;

// Files that people upload: only small images (and a few document types), never pages or scripts.
public class UploadRulesTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    private static IFormFile File(string name, string contentType, long length = 100)
    {
        var stream = new MemoryStream(new byte[Math.Max(length, 1)]);

        return new FormFile(stream, 0, length, "file", name) { Headers = new HeaderDictionary(), ContentType = contentType };
    }

    [Theory]
    [InlineData("photo.jpg", "image/jpeg", true)]
    [InlineData("PHOTO.PNG", "image/png", true)]
    [InlineData("pic.webp", "image/webp", true)]
    [InlineData("page.html", "text/html", false)]
    [InlineData("vector.svg", "image/svg+xml", false)]
    [InlineData("run.exe", "application/octet-stream", false)]
    [InlineData("fake.png", "text/html", false)]
    [InlineData("photo.jpg.html", "image/jpeg", false)]
    [InlineData("noextension", "image/png", false)]
    public void Image_OnlyRealImageTypesAreAllowed(string name, string contentType, bool allowed)
    {
        Assert.Equal(allowed, DocumentSetting.IsAllowedImage(File(name, contentType)));
    }

    [Fact]
    public void Image_NoFileIsFine_EmptyAndHugeAreNot()
    {
        Assert.True(DocumentSetting.IsAllowedImage(null));
        Assert.False(DocumentSetting.IsAllowedImage(File("a.png", "image/png", 0)));
        Assert.True(DocumentSetting.IsAllowedImage(File("a.png", "image/png", DocumentSetting.MaxFileBytes)));
        Assert.False(DocumentSetting.IsAllowedImage(File("a.png", "image/png", DocumentSetting.MaxFileBytes + 1)));
    }

    [Theory]
    [InlineData("contract.pdf", true)]
    [InlineData("sheet.xlsx", true)]
    [InlineData("id.jpg", true)]
    [InlineData("page.html", false)]
    [InlineData("vector.svg", false)]
    [InlineData("script.js", false)]
    [InlineData("tool.exe", false)]
    public void Document_OnlyKnownTypesAreAllowed(string name, bool allowed)
    {
        Assert.Equal(allowed, DocumentSetting.IsAllowedDocument(File(name, "application/octet-stream")));
        Assert.True(DocumentSetting.IsAllowedDocument(null));
    }

    [Fact]
    public async Task Customer_WithABadFile_IsRefusedAndNothingIsSaved()
    {
        var sut = new CustomerRepository(_db.Context, _db.Http);
        var customer = new Customer { Account = new Account { ArName = "عميل", EnName = "Cus" } };

        var result = await sut.UpsertCustomerAsync(customer, File("evil.html", "text/html"));

        Assert.False(result.Success);
        Assert.Equal("InvalidFile", result.Message);
        Assert.Empty(_db.Context.Customers);
        Assert.Empty(_db.Context.Accounts);
    }

    [Fact]
    public async Task MenuItem_WithABadFile_IsRefused()
    {
        var sut = new MenuItemRepository(_db.Context, _db.Http);
        var dish = new MenuItem { ArName = "طبق", EnName = "Dish", Components = new List<MenuItemComponent> { new() { ProductId = 1, Quantity = 1 } } };

        var result = await sut.UpsertMenuItemAsync(dish, File("evil.svg", "image/svg+xml"));

        Assert.Equal("InvalidFile", result.Message);
        Assert.Empty(_db.Context.MenuItems);
    }

    [Fact]
    public async Task Employee_WithABadPhotoOrDocument_IsRefused()
    {
        var job = new Job { Code = "1", ArName = "طباخ", EnName = "Cook" };
        _db.Context.Jobs.Add(job);
        await _db.Context.SaveChangesAsync();
        var sut = new EmployeeRepository(_db.Context, _db.Http);
        Employee Fresh() => new() { ArName = "عمر", EnName = "Omar", JobId = job.Id };

        var badPhoto = await sut.UpsertEmployeeAsync(Fresh(), File("evil.html", "text/html"), null);
        var badDocument = await sut.UpsertEmployeeAsync(Fresh(), null, new List<IFormFile> { File("contract.pdf", "application/pdf"), File("tool.exe", "application/octet-stream") });

        Assert.Equal("InvalidFile", badPhoto.Message);
        Assert.Equal("InvalidFile", badDocument.Message);
        Assert.Empty(_db.Context.Employees);
    }
}

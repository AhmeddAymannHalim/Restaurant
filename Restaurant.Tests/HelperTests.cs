using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Shared.Constants;
using Shared.Enums.Others;
using Shared.Helpers;
using Shared.Helpers.Localization;

namespace Restaurant.Tests;

// These helpers use process-wide state (current folder, culture), so they never run beside other tests.
[CollectionDefinition("global-state", DisableParallelization = true)]
public class GlobalStateCollection
{
}

[Collection("global-state")]
public class HelperTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "helper-tests-" + Guid.NewGuid().ToString("N"));
    private readonly string _oldFolder = Environment.CurrentDirectory;
    private readonly CultureInfo _oldCulture = CultureInfo.CurrentCulture;
    private readonly CultureInfo _oldUiCulture = CultureInfo.CurrentUICulture;

    public HelperTests()
    {
        Directory.CreateDirectory(Path.Combine(_folder, "Resources"));
        Environment.CurrentDirectory = _folder;
    }

    public void Dispose()
    {
        Environment.CurrentDirectory = _oldFolder;
        CultureInfo.CurrentCulture = _oldCulture;
        CultureInfo.CurrentUICulture = _oldUiCulture;
        ErrorLog.Logger = null;
        try
        {
            Directory.Delete(_folder, true);
        }
        catch
        {
            // a locked temp file is not a test failure
        }
    }

    // ---------------------------------------------------------------- json localizer

    private JsonStringLocalizer Localizer(string culture, string json)
    {
        File.WriteAllText(Path.Combine(_folder, "Resources", culture + ".json"), json);
        CultureInfo.CurrentCulture = new CultureInfo(culture);

        return new JsonStringLocalizer(new MemoryCache(new MemoryCacheOptions()));
    }

    [Fact]
    public void Localizer_FindsTheKey_IgnoringCase_AndFormatsArguments()
    {
        var sut = Localizer("en", "{ \"Hello\": \"Hello {0}\", \"Plain\": \"Plain text\" }");

        Assert.Equal("Plain text", sut["plain"].Value);
        Assert.False(sut["Plain"].ResourceNotFound);
        Assert.Equal("Hello Ali", sut["Hello", "Ali"].Value);
    }

    [Fact]
    public void Localizer_MissingKey_ReturnsTheKeyAndSaysNotFound()
    {
        var sut = Localizer("en", "{ \"A\": \"a\" }");

        var missing = sut["Nope"];

        Assert.Equal("Nope", missing.Value);
        Assert.True(missing.ResourceNotFound);
        Assert.Equal("Nope", sut["Nope", "x"].Value);
        Assert.Equal(string.Empty, new string(sut[""].Value));
    }

    [Fact]
    public void Localizer_MissingFile_FallsBackToTheKey_AndTheCultureChoosesTheFile()
    {
        var sut = Localizer("en", "{ \"K\": \"english\" }");
        File.WriteAllText(Path.Combine(_folder, "Resources", "ar.json"), "{ \"K\": \"عربي\" }");

        Assert.Equal("english", sut["K"].Value);

        CultureInfo.CurrentCulture = new CultureInfo("ar");
        Assert.Equal("عربي", new JsonStringLocalizer(new MemoryCache(new MemoryCacheOptions()))["K"].Value);

        CultureInfo.CurrentCulture = new CultureInfo("fr");
        Assert.Equal("K", new JsonStringLocalizer(new MemoryCache(new MemoryCacheOptions()))["K"].Value);
    }

    [Fact]
    public void Localizer_ReadsTheFileOnlyOncePerKey_ThenUsesTheCache()
    {
        var sut = Localizer("en", "{ \"K\": \"first\" }");
        Assert.Equal("first", sut["K"].Value);

        File.WriteAllText(Path.Combine(_folder, "Resources", "en.json"), "{ \"K\": \"changed\" }");

        Assert.Equal("first", sut["K"].Value);
    }

    [Fact]
    public void Localizer_BrokenJson_ThrowsInsteadOfHidingTheProblem_AndFactoryBuildsLocalizers()
    {
        var sut = Localizer("en", "{ not json");

        Assert.ThrowsAny<Exception>(() => sut["K"]);
        Assert.Throws<NotImplementedException>(() => sut.GetAllStrings(true));

        var factory = new JsonStringLocalizerFactory(new MemoryCache(new MemoryCacheOptions()));
        Assert.IsType<JsonStringLocalizer>(factory.Create(typeof(string)));
        Assert.IsType<JsonStringLocalizer>(factory.Create("x", "y"));
    }

    [Fact]
    public void ArabicAndEnglishResourceFiles_HaveTheSameKeys()
    {
        var root = AppFixtureRoot();
        var ar = KeysOf(Path.Combine(root, "ResturantPL", "Resources", "ar.json"));
        var en = KeysOf(Path.Combine(root, "ResturantPL", "Resources", "en.json"));

        Assert.Empty(ar.Except(en));
        Assert.Empty(en.Except(ar));
        Assert.True(ar.Count > 500);
    }

    private static string AppFixtureRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "ResturantPL", "Resources")))
        {
            dir = dir.Parent;
        }

        return dir.FullName;
    }

    private static HashSet<string> KeysOf(string path) => System.Text.RegularExpressions.Regex
        .Matches(File.ReadAllText(path), "^\\s*\"([^\"]+)\"\\s*:", System.Text.RegularExpressions.RegexOptions.Multiline)
        .Select(m => m.Groups[1].Value).ToHashSet();

    // ---------------------------------------------------------------- toastr

    private class FakeTempData : Dictionary<string, object>, ITempDataDictionary
    {
        public void Load() { }
        public void Save() { }
        public void Keep() { }
        public void Keep(string key) { }
        public object Peek(string key) => this[key];
    }

    private static (ToastrService service, FakeTempData data) Toastr()
    {
        var data = new FakeTempData();
        var factory = new Mock<ITempDataDictionaryFactory>();
        factory.Setup(f => f.GetTempData(It.IsAny<HttpContext>())).Returns(data);

        return (new ToastrService(new HttpContextAccessor { HttpContext = new DefaultHttpContext() }, factory.Object), data);
    }

    [Fact]
    public void Toastr_KeepsEveryMessageInOrder_WithItsType()
    {
        var (service, data) = Toastr();

        service.Success("saved");
        service.Info("fyi");
        service.Warning("careful");
        service.Error("failed");

        var list = JsonConvert.DeserializeObject<List<ToastrData>>(data[ToastrDefaults.ToasterList].ToString());
        Assert.Equal(new[] { "saved", "fyi", "careful", "failed" }, list.Select(a => a.Message));
        Assert.Equal(new[] { ToastrType.Success, ToastrType.Info, ToastrType.Warning, ToastrType.Error }, list.Select(a => a.Type));
    }

    // ---------------------------------------------------------------- week days, language, dates, errors

    [Theory]
    [InlineData("Saturday", "Saturday")]
    [InlineData("  friday ", "Friday")]
    [InlineData("MONDAY", "Monday")]
    [InlineData("السبت", "Saturday")]
    [InlineData("الأحد", "Sunday")]
    [InlineData("الاحد", "Sunday")]
    [InlineData("الإثنين", "Monday")]
    [InlineData("الاثنين", "Monday")]
    [InlineData("الثلاثاء", "Tuesday")]
    [InlineData("الأربعاء", "Wednesday")]
    [InlineData("الاربعاء", "Wednesday")]
    [InlineData("الخميس", "Thursday")]
    [InlineData("الجمعة", "Friday")]
    [InlineData("Funday", null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData(null, null)]
    public void WeekDays_Normalize_AcceptsArabicAndEnglishNames(string input, string expected)
    {
        Assert.Equal(expected, WeekDays.Normalize(input));
    }

    [Fact]
    public void WeekDays_HaveSevenNames_StartingWithSaturday()
    {
        Assert.Equal(7, WeekDays.Names.Length);
        Assert.Equal("Saturday", WeekDays.Names[0]);
        Assert.Equal("Friday", WeekDays.Names[6]);
    }

    [Fact]
    public void LanguageHelper_FollowsTheUiCulture()
    {
        CultureInfo.CurrentUICulture = new CultureInfo("ar");
        Assert.True(LanguageHelper.IsArabic());
        CultureInfo.CurrentUICulture = new CultureInfo("ar-EG");
        Assert.True(LanguageHelper.IsArabic());
        CultureInfo.CurrentUICulture = new CultureInfo("en");
        Assert.False(LanguageHelper.IsArabic());
    }

    [Fact]
    public void DefaultDateRange_StartsAtTheBeginningOfThisYearsData_AndEndsToday()
    {
        Assert.Equal(new DateTime(2026, 1, 1), DefaultDateRange.Start);
        Assert.Equal(DateTime.Today, DefaultDateRange.End);
    }

    [Fact]
    public void ErrorLog_HidesDetailsUnlessAsked_AndAlwaysLogs()
    {
        var logger = new Mock<ILogger>();
        ErrorLog.Logger = logger.Object;
        var old = ErrorLog.ShowDetails;
        try
        {
            ErrorLog.ShowDetails = false;
            Assert.Equal("ErrorWhileSaving", ErrorLog.Message(new InvalidOperationException("secret")));
            Assert.Equal("custom", ErrorLog.Message(new InvalidOperationException("secret"), "custom"));

            ErrorLog.ShowDetails = true;
            Assert.Equal("secret", ErrorLog.Message(new InvalidOperationException("secret")));

            logger.Verify(l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Exactly(3));
        }
        finally
        {
            ErrorLog.ShowDetails = old;
        }
    }

    // ---------------------------------------------------------------- documents on disk

    private static IFormFile Pdf(string name, string text = "hello")
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);

        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", name);
    }

    [Fact]
    public void Documents_Upload_WritesTheFileUnderWwwroot_ReturnsItsUrl_AndDeleteRemovesIt()
    {
        var url = DocumentSetting.UploadFile(Pdf("contract.pdf", "content"), "Employee Documents");

        Assert.StartsWith("/Employee Documents/", url);
        Assert.EndsWith("_contract.pdf", url);
        var path = Path.Combine(_folder, "wwwroot", url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        Assert.Equal("content", File.ReadAllText(path));

        DocumentSetting.DeleteFile(url);
        Assert.False(File.Exists(path));
        DocumentSetting.DeleteFile(url);   // deleting twice is harmless
        DocumentSetting.DeleteFile("/nothing/here.png");
    }

    [Fact]
    public void Documents_Upload_NeverWritesOutsideItsFolder_AndSameNamesDoNotCollide()
    {
        var sneaky = DocumentSetting.UploadFile(Pdf("..\\..\\evil.pdf"), "Employee Documents");
        var first = DocumentSetting.UploadFile(Pdf("same.pdf"), "Employee Documents");
        var second = DocumentSetting.UploadFile(Pdf("same.pdf"), "Employee Documents");

        Assert.DoesNotContain("..", sneaky);
        Assert.StartsWith("/Employee Documents/", sneaky);
        Assert.NotEqual(first, second);
        Assert.False(File.Exists(Path.Combine(_folder, "evil.pdf")));
        Assert.False(File.Exists(Path.Combine(Path.GetDirectoryName(_folder), "evil.pdf")));
    }
}

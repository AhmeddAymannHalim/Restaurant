using ResturantPL;
using ResturantPL.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddWebServices();
builder.Services
    .AddPersistence(builder.Configuration)
    .AddEmail(builder.Configuration)
    .AddAuthenticationAndIdentity(builder.Configuration)
    .AddCorsAndSession()
    .AddSignalR();

var app = builder.Build();

app.UseWebPipeline();
await app.MigrateAndSeedAsync();

app.Run();

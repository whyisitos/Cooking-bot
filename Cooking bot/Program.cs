using CookingBot.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure MongoDB
builder.Services.AddSingleton<IMongoClient>(s =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDb");
    return new MongoClient(connectionString);
});

builder.Services.AddSingleton<RecipeService>();
builder.Services.AddHttpClient<SpoonacularService>();

// Configure Forwarded Headers for Fly.io proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Clear known networks/proxies to accept all forwarders
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Use Forwarded Headers middleware first
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();
app.Run();
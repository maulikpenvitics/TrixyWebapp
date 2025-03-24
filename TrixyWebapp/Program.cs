
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Repository.DbModels;
using Repository.FyersWebSocketServices;
using Repository.Hubs;
using Repository.IRepositories;
using Repository.Repositories;
using System.Net;

var builder = WebApplication.CreateBuilder(args);


ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };



builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

// 🔹 Register Hangfire with MongoDB
var mongoSettings = builder.Configuration.GetSection("MongoDB");
var connectionString = mongoSettings.GetValue<string>("ConnectionString");
var databaseName = mongoSettings.GetValue<string>("DatabaseName");
var mongoUrl = new MongoUrl(connectionString);
var mongoClient = new MongoClient(mongoUrl);
// Register Hangfire with MongoDB
//builder.Services.AddHangfire(config =>
//{
//    config.UseMongoStorage(mongoClient, databaseName, new MongoStorageOptions
//    {
//        MigrationOptions = new MongoMigrationOptions { MigrationStrategy = new MigrateMongoMigrationStrategy() }
//    });
//});
//builder.Services.AddHangfireServer();
//builder.Services.AddHostedService<JobSchedulerService>();
builder.Services.AddHostedService<StockNotificationService>();
// Configure Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set your session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});
builder.Services.AddSignalR();
builder.Services.AddHttpClient();


builder.Services.AddScoped(typeof(IRepository<>), typeof(MongoRepository<>));
builder.Services.AddSingleton<FyersWebSocketService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWebStockRepository, WebStockRepository>();
builder.Services.AddScoped<IStockSymbolRepository, StockSymbolRepository>();
builder.Services.AddScoped<IAdminSettingRepository, AdminSettingRepository>();



builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/Account/Login";
        option.LogoutPath = "/Account/Logout";
        option.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        option.SlidingExpiration = true;
        option.Cookie.HttpOnly = true;
        option.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

builder.Services.AddAuthorization();

builder.Services.Configure<FyersStockMarketSettings>(builder.Configuration.GetSection("FyersStockMarketData"));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope()) // ✅ Create a scope
{
    var webSocketService = scope.ServiceProvider.GetRequiredService<FyersWebSocketService>();
    webSocketService.Connect();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();
app.MapStaticAssets();
//app.MapRazorPages()
//   .WithStaticAssets();
app.UseSession();
app.UseStaticFiles();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapHub<StockHub>("/stockHub");
app.MapHub<StockNotificationHub>("/stockNotificationHub");

app.Run();

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
builder.Services.AddSession();



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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.MapHub<StockHub>("/stockHub");

app.Run();

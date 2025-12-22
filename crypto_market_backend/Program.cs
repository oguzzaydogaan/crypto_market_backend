using Microsoft.EntityFrameworkCore;
using Repositories;
using Repositories.Repositories;
using Services;
using Services.Automappers;
using Services.Helpers;
using Services.Hubs;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSignalR();

builder.Services.AddScoped<CoinRepository>();
builder.Services.AddScoped<CoinService>();
builder.Services.AddHttpClient<BinanceHelper>();
builder.Services.AddSingleton<BinanceStreamManager>();
builder.Services.AddHostedService<TickerHelper>();

builder.Services.AddAutoMapper(typeof(CoinProfile));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .SetIsOriginAllowed(origin => true)
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseCors("AllowAll");

app.MapHub<CryptoHub>("/cryptohub");
app.Run();

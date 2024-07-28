using Asos.Extensions;
using Asos.Redis;
using StackExchange.Redis;
using Asos.Helpers;
using Asos.Middlewares;
using Asos.Models;

var builder = WebApplication.CreateBuilder(args);

var redisOptions = builder.Configuration.GetSection("Redis").Get<RedisOptions>();
builder.Services.Configure<JwtCredentials>(builder.Configuration.GetSection("JwtCredentials"));
var redisConfiguration = $"{redisOptions?.Host}:{redisOptions?.Port}";

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConfiguration));

//  NIGGA

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureSwagger();
builder.Services.AddServices();
builder.Services.AddJwt(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.MapControllers();
app.UseAuthorization();
app.UseAuthentication();
app.UseHttpsRedirection();

app.Run();
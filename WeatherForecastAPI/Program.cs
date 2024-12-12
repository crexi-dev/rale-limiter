using RateLimiter.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Set up dependency injection for the custom rate limiter rules
builder.Services.AddCustomRateLimiterRules();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Apply the custom rate limiter middleware after the authorization middleware so any authentication and built-in authorization has already been checked
app.UseCustomRateLimiter();

app.MapControllers();

app.Run();

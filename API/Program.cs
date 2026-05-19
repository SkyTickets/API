using API.Enums;
using API.Model;
using Microsoft.EntityFrameworkCore;
using Npgsql.NameTranslation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var conString = builder.Configuration.GetConnectionString("DatabaseConnection") ?? throw new InvalidOperationException("Connection string 'DatabaseConnection' not found.");

var clientAddress = builder.Configuration["Addresses:Client"] ?? throw new InvalidOperationException("Address string 'Client' not found");

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen();

builder.Services.AddDbContextFactory<PostgresContext>((options) =>
{
options.UseLazyLoadingProxies().UseNpgsql(conString, o =>
 o.MapEnum<Role>("role", "skytickets", new NpgsqlNullNameTranslator())
.MapEnum<TicketStatus>("ticket_status", "skytickets", new NpgsqlNullNameTranslator())
.MapEnum<ClassOfService>("class_of_service", "skytickets", new NpgsqlNullNameTranslator())
   );
});

var myAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
options.AddPolicy(myAllowSpecificOrigins,
  policy =>
  {
  policy.WithOrigins(clientAddress)
  .AllowAnyHeader()
  .AllowAnyMethod()
  .AllowAnyOrigin();
  });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseCors(myAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();

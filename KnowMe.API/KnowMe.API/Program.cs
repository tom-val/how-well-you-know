using KnowMe.API.Database;
using KnowMe.API.Database.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOptions<DatabaseOptions>()
    .BindConfiguration(DatabaseOptions.ConfigSection)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddDbContext<KnowMeContext>((serviceProvider, options) =>
{
    var dbOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    options.UseNpgsql(dbOptions.ConnectionString);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<KnowMeContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

using Microsoft.EntityFrameworkCore;
using webapi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSqlServer<ApplicationDbContext>(
    builder.Configuration.GetConnectionString("DefaultConnection"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

var api = app.MapGroup("/api");

api.MapGet("/models", (ApplicationDbContext db) =>
    db.Model.GetEntityTypes().Select(et => new
    {
        Name = et.ShortName(),
        Properties = et.GetProperties().Select(p => p.Name)
    }));

api.MapGet("/models/{name}", (string name, bool temporal, ApplicationDbContext db) =>
{
    var entityType = db.Model.GetEntityTypes().FirstOrDefault(et => et.ShortName() == name);

    if (entityType is null)
    {
        return Results.NotFound(entityType);
    }

    var setMethod = typeof(ApplicationDbContext).GetMethod(nameof(ApplicationDbContext.Set), types: Type.EmptyTypes)
        !.MakeGenericMethod(entityType.ClrType);

    var result = setMethod.Invoke(db, null);

    if (!temporal)
    {
        return result;
    }

    var temporalAllMethod = typeof(SqlServerDbSetExtensions).GetMethod(nameof(SqlServerDbSetExtensions.TemporalAll))
        !.MakeGenericMethod(entityType.ClrType);

    result = temporalAllMethod.Invoke(null, new[] { result });
    
    return result;
});

app.Run();
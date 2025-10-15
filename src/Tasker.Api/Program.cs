using Microsoft.EntityFrameworkCore;
using Tasker.Api;
using Tasker.Api.Interfaces;
using Tasker.Application.Commands.Boards.CreateBoard;
using Tasker.Core;
using Tasker.Core.Boards;
using Tasker.Core.Users;
using Tasker.Data;
using Tasker.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("TaskerDb");
Console.WriteLine($"[CFG] TaskerDb = {builder.Configuration.GetConnectionString("TaskerDb")}");

builder.Services.AddDbContext<TaskerDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IBoardRepository, BoardRepository>();
builder.Services.AddScoped<IColumnRepository, ColumnRepository>();
builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly, typeof(CreateBoardCommand).Assembly)
);

builder.Services.AddSingleton<Tasker.Application.Mappers.BoardsMapper>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<TaskerDbContext>();
        await db.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}


app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/Api/IsAlive", () => Results.Ok(true));

app.Run();
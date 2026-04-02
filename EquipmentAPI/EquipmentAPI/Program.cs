using EquipmentAPI.Data;
using EquipmentAPI.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/equipments", async (CreateEquipmentDto dto, AppDbContext context) =>
{
    var equipment = new Equipment
    {
        Name = dto.Name,
        Category = dto.Category,
        Status = dto.Status,
        Location = dto.Location
    };

    context.Equipments.Add(equipment);
    await context.SaveChangesAsync();

    return Results.Created($"/equipments/{equipment.Id}", new EquipmentResponseDto
    {
        Id = equipment.Id,
        Name = equipment.Name,
        Category = equipment.Category,
        Status = equipment.Status,
        Location = equipment.Location
    });
})
.WithName("CreateEquipment")
.WithOpenApi();

app.MapGet("/equipments", async (AppDbContext context) =>
{
    var equipments = await context.Equipments.ToListAsync();

    var result = equipments.Select(e => new EquipmentResponseDto
    {
        Id = e.Id,
        Name = e.Name,
        Category = e.Category,
        Status = e.Status,
        Location = e.Location
    });

    return Results.Ok(result);
})
.WithName("GetEquipments")
.WithOpenApi();

app.MapGet("/equipments/{id:int:min(1)}", async (int id, AppDbContext context) =>
{
    var equipment = await context.Equipments.FindAsync(id);

    if (equipment == null)
        return Results.NotFound();

    return Results.Ok(new EquipmentResponseDto
    {
        Id = equipment.Id,
        Name = equipment.Name,
        Category = equipment.Category,
        Status = equipment.Status,
        Location = equipment.Location
    });
})
.WithName("GetEquipmentById")
.WithOpenApi();

app.Run();
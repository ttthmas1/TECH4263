using EquipmentAPI.Models;
using System.Linq;


var builder = WebApplication.CreateBuilder(args);
var equipments = new List<Equipment>();


// Add services to the container.
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

app.MapPost("/equipments", (CreateEquipmentDto dto) =>
{
    var equipment = new Equipment(dto.Name, dto.Category, dto.Status, dto.Location);
    equipments.Add(equipment);

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


app.MapGet("/equipments", () =>
{
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



app.MapGet("/equipments/{id:int:min(1)}", (int id) =>
{
    var found = equipments.FirstOrDefault(e => e.Id == id);

    if (found == null)
        return Results.NotFound();

    return Results.Ok(new EquipmentResponseDto
    {
        Id = found.Id,
        Name = found.Name,
        Category = found.Category,
        Status = found.Status,
        Location = found.Location
    });
})
.WithName("GetEquipmentById")
.WithOpenApi();


app.Run();



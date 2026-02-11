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

app.MapPost("/createequipment", (string name, string category, string status, string location) =>
{
    var newEq = new Equipment(name, category, status, location);
    equipments.Add(newEq);

    return Results.Created($"/getequipment/{newEq.Id}", newEq);
})
.WithName("CreateEquipment")
.WithOpenApi();


app.MapGet("/getequipments", () =>
{
    return Results.Ok(equipments);
})
.WithName("GetEquipments")
.WithOpenApi();


app.MapGet("/getequipment/{id}", (int id) =>
{
    var found = equipments.FirstOrDefault(e => e.Id == id);

    if (found == null)
        return Results.NotFound();

    return Results.Ok(found);
})
.WithName("GetEquipmentById")
.WithOpenApi();


app.Run();



using FarmAPI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "FarmAPI", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FarmAPI v1");
        c.RoutePrefix = string.Empty; // Makes Swagger UI available at root URL
    });
}

app.UseHttpsRedirection();

// In-memory storage for demo purposes
var animals = AnimalItem.GetSampleAnimals();

// API Routes
app.MapGet("/animals", () => animals)
    .WithName("GetAllAnimals")
    .WithOpenApi();

app.MapPost("/animals", (AnimalItem animal) =>
{
    // Validate the animal species before adding
    if (!AnimalUtilities.ValidateSpecies(animal.Species))
    {
        return Results.BadRequest($"Invalid species: {animal.Species}. Valid species are Cow, Sheep, Lamb, Chicken, Goat, Pig.");
    }
    animals.Add(animal);
    return Results.Created($"/animals/{animals.Count}", animal);
})
    .WithName("AddAnimal")
    .WithOpenApi();

app.MapDelete("/animals/{name}", (string name) =>
{
    var animal = animals.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    if (animal is null)
        return Results.NotFound();
    
    animals.Remove(animal);
    return Results.NoContent();
})
    .WithName("RemoveAnimal")
    .WithOpenApi();

// add a route to return all animals of a certain species
app.MapGet("/animals/species/{species}", (string species) =>
{
    var speciesAnimals = animals
        .Where(a => a.Species.Equals(species, StringComparison.OrdinalIgnoreCase))
        .ToList();
    
    if (speciesAnimals.Count == 0)
        return Results.NotFound($"No animals found with species: {species}");
    
    return Results.Ok(speciesAnimals);
})
    .WithName("GetAnimalsBySpecies")
    .WithOpenApi();

// add a route to return a random animal
app.MapGet("/animals/random", () =>
{
    var random = new Random();
    int index = random.Next(animals.Count);
    return animals[index];
})
    .WithName("GetRandomAnimal")
    .WithOpenApi();

app.Run();

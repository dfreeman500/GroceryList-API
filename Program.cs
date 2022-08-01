using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<GroceryDb>(opt => opt.UseInMemoryDatabase("GroceryList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

app.MapGet("/groceryitems", async (GroceryDb db) =>
    await db.Groceries.ToListAsync());

app.MapGet("/groceryitems/complete", async (GroceryDb db) =>
    await db.Groceries.Where(t => t.IsComplete).ToListAsync());

app.MapGet("/groceryitems/incomplete", async (GroceryDb db) =>
    await db.Groceries.Where(t => t.IsComplete == false).ToListAsync());

app.MapGet("/groceryitems/{id}", async (int id, GroceryDb db) =>
    await db.Groceries.FindAsync(id)
        is Grocery grocery
            ? Results.Ok(grocery)
            : Results.NotFound());

app.MapPost("/groceryitems", async (Grocery grocery, GroceryDb db) =>
{
    db.Groceries.Add(grocery);
    await db.SaveChangesAsync();

    return Results.Created($"/groceryitems/{grocery.Id}", grocery);
});

app.MapPut("/groceryitems/{id}", async (int id, Grocery inputGrocery, GroceryDb db) =>
{
    var grocery = await db.Groceries.FindAsync(id);

    if (grocery is null) return Results.NotFound();

    grocery.Name = inputGrocery.Name;
    grocery.IsComplete = inputGrocery.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/groceryitems/{id}", async (int id, GroceryDb db) =>
{
    if (await db.Groceries.FindAsync(id) is Grocery grocery)
    {
        db.Groceries.Remove(grocery);
        await db.SaveChangesAsync();
        return Results.Ok(grocery);
    }

    return Results.NotFound();
});

app.Run();

class Grocery
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}

class GroceryDb : DbContext
{
    public GroceryDb(DbContextOptions<GroceryDb> options)
        : base(options) { }

    public DbSet<Grocery> Groceries => Set<Grocery>();
}
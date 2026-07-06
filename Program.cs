using GenericCrud.Data;
using GenericCrud.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Generic CRUD engine services — everything the CrudController needs is registered here.
builder.Services.AddScoped<IEntityMetadataService, EntityMetadataService>();
builder.Services.AddScoped<IReflectionService, ReflectionService>();
builder.Services.AddScoped<IDropdownService, DropdownService>();
builder.Services.AddScoped<ICrudService, CrudService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// Root URL redirects straight into the CRUD engine for the Employee entity as a starting point.
app.MapGet("/", () => Results.Redirect("/Crud/Employee"));

app.Run();

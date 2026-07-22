using Microsoft.AspNetCore.Identity;
using RealStateApp.Core.Application;
using RealStateApp.Persistance;
using RealStateApp.Persistance.Identity;
using RealStateApp.Persistance.Seeders;
using RealStateApp.Persistance.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Registro de capas
builder.Services.AddPersistenceInfrastructure(builder.Configuration);
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddApplicationLayer();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// INTEGRACIÓN DE SEEDERS
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await IdentitySeeder.SeedRolesAsync(services);
        await IdentitySeeder.SeedUsersAsync(services);
        await PropertyDataSeeder.SeedDefaultDataAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al sembrar la base de datos.");
    }
}

app.Run();
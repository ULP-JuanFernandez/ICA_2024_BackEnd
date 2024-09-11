using ICA.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;


var builder = WebApplication.CreateBuilder(args);

//Configurar el contexto de la base de datos
builder.Services.AddDbContext<DbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Registrar la implementación del servicio IRepositorioInfo
builder.Services.AddScoped<IRepositorioGenero, RepositorioGenero>();
builder.Services.AddScoped<RepositorioGenero, RepositorioGenero>();

builder.Services.AddScoped<IRepositorioEtiquetas, RepositorioEtiquetas>();
builder.Services.AddScoped<RepositorioEtiquetas, RepositorioEtiquetas>();

builder.Services.AddScoped<IRepositorioTecnicatura, RepositorioTecnicatura>();
builder.Services.AddScoped<RepositorioTecnicatura, RepositorioTecnicatura>();

builder.Services.AddScoped<IRepositorioProyecto, RepositorioProyecto>();
builder.Services.AddScoped<RepositorioProyecto, RepositorioProyecto>();

builder.Services.AddScoped<IRepositorioSliders, RepositorioSliders>();
builder.Services.AddScoped<RepositorioSliders, RepositorioSliders>();

// Otros servicios
builder.Services.AddControllersWithViews();

var app = builder.Build();



app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();


app.UseStaticFiles(); // Habilita la configuración de archivos estáticos

var sharedImagesPath = @"C:\SharedImages";
if (!Directory.Exists(sharedImagesPath))
{
    Directory.CreateDirectory(sharedImagesPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(sharedImagesPath),
    RequestPath = "/SharedImages"
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

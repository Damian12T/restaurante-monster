using Microsoft.AspNetCore.Mvc;
using RestauranteMonster.Data;
using RestauranteMonster.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Todo POST, PUT o DELETE debe llevar un token antiforgery válido.
builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".RestauranteMonster.Session";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

Conexion.Configure(builder.Configuration.GetConnectionString("DefaultConnection")!);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Seguridad/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (contexto, siguiente) =>
{
    var h = contexto.Response.Headers;
    h["X-Content-Type-Options"] = "nosniff";
    h["X-Frame-Options"] = "DENY";
    h["Referrer-Policy"] = "same-origin";
    await siguiente();
});

// Soporte para AVIF y WEBP como tipos estáticos
var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
provider.Mappings[".avif"] = "image/avif";
provider.Mappings[".webp"] = "image/webp";
app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Seguridad}/{action=Login}/{id?}");

app.Run();

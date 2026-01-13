using MedilifeSaludV3.Web.Models;
using MedilifeSaludV3.Web.Services;
using MedilifeSaludV3.Web.Services.Excel;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web.UI;
using Microsoft.Identity.Web;


var builder = WebApplication.CreateBuilder(args);




if (builder.Environment.IsDevelopment())
{
    builder.Services.AddAuthentication("Dev")
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, DevAuthHandler>("Dev", _ => { });

    builder.Services.AddAuthorization();

    builder.Services.AddControllersWithViews();
}
else
{
    builder.Services
        .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

    builder.Services.AddAuthorization();

    builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.BackchannelTimeout = TimeSpan.FromMinutes(5);
    });

    builder.Services.AddControllersWithViews()
        .AddMicrosoftIdentityUI();
}


builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=medilife.db"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ExcelExportService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .RequireAuthorization();

// Seed rápido para prototipo
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.ObrasSociales.Any())
    {
        db.ObrasSociales.AddRange(
            new ObraSocial {  Nombre = "OSDE", Cuit = "30-00000000-0", Email = "contacto@osde.com.ar", TelFax = "011-0000-0000", Excento = false, CondicionAnteIVA = CondicionAnteIVA.Inscripto },
            new ObraSocial {  Nombre = "Swiss Medical", Cuit = "30-11111111-1", Email = "info@swissmedical.com.ar", TelFax = "011-1111-1111", Excento = false, CondicionAnteIVA = CondicionAnteIVA.Inscripto }
        );
        db.SaveChanges();
    }

    if (!db.Medicos.Any())
    {
        db.Medicos.AddRange(
            new Medico { Apellido = "Pérez", Nombre = "Juan", Comentario = "Cardiología" },
            new Medico { Apellido = "Gómez", Nombre = "Ana", Comentario = "Clínica" }
        );
        db.SaveChanges();
    }

    if (!db.Pacientes.Any())
    {
        db.Pacientes.AddRange(
            new Paciente { Apellido = "Fernández", Nombre = "Leandro" },
            new Paciente { Apellido = "Sosa", Nombre = "María" }
        );
        db.SaveChanges();
    }
}



app.Run();
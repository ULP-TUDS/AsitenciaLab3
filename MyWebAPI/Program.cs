using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using MimeKit;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
using MyWebAPI.Models;
using MyWebAPI.Services;
using Newtonsoft.Json.Serialization;



var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5000", "https://localhost:5001", "http://*:5000", "https://*:5001");

// Configurar servicios
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("sql");
builder.Services.AddScoped<EmailService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddDbContext<DataContext>(options => 
    options.UseMySQL(connectionString));

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.DateFormatString = "yyyy-MM-dd";
});

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<PresenciaService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuarios/Login";
        options.LogoutPath = "/Usuarios/Logout";
        options.AccessDeniedPath = "/Home/Restringido";
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["TokenAuthentication:Issuer"],
            ValidAudience = configuration["TokenAuthentication:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["TokenAuthentication:SecretKey"]))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/chatsegurohub") ||
                    path.StartsWithSegments("/api/propietarios/reset") ||
                    path.StartsWithSegments("/api/propietarios/token")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });
    

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy =>
            policy.RequireRole("Administrador"));

        options.AddPolicy("Empleado", policy =>
            policy.RequireRole("Empleado"));

               options.AddPolicy("AdministradorOEmpleado", policy =>
            policy.RequireRole("Administrador", "Empleado"));


});
builder.Services.AddMvc();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());


app.UseStaticFiles();
app.UseRouting();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.None,
});

app.UseAuthentication();
app.UseAuthorization();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute("login", "entrar/{**accion}", new { controller = "Usuarios", action = "Login" });
app.MapControllerRoute("rutaFija", "ruteo/{valor}", new { controller = "Home", action = "Ruta", valor = "defecto" });
app.MapControllerRoute("fechas", "{controller=Home}/{action=Fecha}/{anio}/{mes}/{dia}");

app.MapControllerRoute(
    name: "vista",
    pattern: "token",
    defaults: new { controller = "Token", action = "Index" });

app.MapControllerRoute(
    name: "postToken",
    pattern: "Token/Post",
    defaults: new { controller = "Token", action = "Post" });
   
    
app.Run();

// Mover el bloque de código de envío de correo a un controlador o servicio
public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public void SendEmail()
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Sistema", _config["Smtp:User"]));
            message.To.Add(new MailboxAddress("Destinatario", "destinatario@example.com"));
            message.Subject = "Asunto del Correo";
            message.Body = new TextPart("plain")
            {
                Text = "Cuerpo del correo"
            };

            using (var client = new SmtpClient())
            {
                client.Connect(_config["Smtp:Host"], int.Parse(_config["Smtp:Port"]), _config.GetValue<bool>("Smtp:SMTPUseSSL"));
                client.Authenticate(_config["Smtp:User"], _config["Smtp:Pass"]);
                
                client.Send(message);
                client.Disconnect(true);
            }

            Console.WriteLine("Correo enviado exitosamente.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error enviando correo: {ex.Message}");
        }
    }
}

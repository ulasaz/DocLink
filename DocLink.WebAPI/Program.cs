using System.Text;
using System.Text.Json.Serialization;
using DocLink.Core.Models;
using DocLink.Data;
using DocLink.Data.Interfaces;
using DocLink.Data.Repositories;
using DocLink.Services;
using DocLink.Services.Interfaces;
using DocLink.Services.Services;
using DocLink.Services.Workers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- CORS Configuration ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") 
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// --- Logging Connection String (Useful for debugging) ---
Console.WriteLine("Connection string:");
Console.WriteLine(builder.Configuration.GetConnectionString("Postgres"));

// --- Database Configuration ---
builder.Services.AddDbContext<ApplicationContext>(options =>
{
    if (!options.IsConfigured)
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
    }
});

// --- Identity Configuration ---
builder.Services.AddIdentity<Account, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationContext>()
    .AddDefaultTokenProviders();

// --- Authentication & JWT Configuration ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// --- Dependency Injection Registration ---

// Repositories
builder.Services.AddScoped<IAccountFactoryProvider, AccountFactoryProvider>();
builder.Services.AddScoped<ISpecialistRepository, SpecialistRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

// Services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ISpecialistService, SpecialistService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

builder.Services.AddScoped<IPatientWorker, PatientWorker>();
builder.Services.AddScoped<IScheduleWorker, ScheduleWorker>();

// Token Service (Registered both as Interface and Class to avoid resolution errors)
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<TokenService>(); 

// --- Controllers & JSON Options ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Middleware Pipeline ---

app.UseCors("AllowReactApp"); 

// Swagger should be available in both Dev and Production for testing convenience
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles(); 
app.UseDefaultFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

// Required for Integration Tests
public partial class Program { }
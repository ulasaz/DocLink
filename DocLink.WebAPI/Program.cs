using System.Text; // Required for Encoding
using System.Text.Json.Serialization;
using DocLink.Core.Models;
using DocLink.Data;
using DocLink.Data.Interfaces;
using DocLink.Data.Repositories;
using DocLink.Services;
using DocLink.Services.Interfaces;
using DocLink.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Required for JwtBearerDefaults
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; // Required for SymmetricSecurityKey

var builder = WebApplication.CreateBuilder(args);

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

Console.WriteLine("Connection string:");
Console.WriteLine(builder.Configuration.GetConnectionString("Postgres"));

builder.Services.AddDbContext<ApplicationContext>(options =>
{
    if (!options.IsConfigured)
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
    }
});
builder.Services.AddIdentity<Account, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationContext>()
    .AddDefaultTokenProviders();

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

builder.Services.AddScoped<IAccountFactoryProvider, AccountFactoryProvider>();
builder.Services.AddScoped<ISpecialistRepository, SpecialistRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ISpecialistService, SpecialistService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowReactApp"); 

app.UseHttpsRedirection();
app.UseStaticFiles(); 
app.UseDefaultFiles();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
public partial class Program { }
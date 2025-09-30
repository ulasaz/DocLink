using DocLink.Core;
using DocLink.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlite("Data Source=.DocLink.db"));

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
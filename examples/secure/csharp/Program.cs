// Minimal ASP.NET Core entry point required for the Microsoft.NET.Sdk.Web
// project to compile.  The controllers in this directory are educational
// examples demonstrating secure coding patterns for CodeQL analysis.

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();

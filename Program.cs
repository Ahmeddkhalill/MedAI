using MedAI;
using Scalar.AspNetCore;
using Arora.GlobalExceptionHandler;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies(builder.Configuration);

builder.Services.AddHttpClient("AI", client =>
{
    client.BaseAddress = new Uri("http://127.0.0.1:5000/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

app.MapOpenApi();

app.MapScalarApiReference();

app.UseHttpsRedirection();

app.UseCors(CorsPolicy.AllowAll);

app.UseAuthorization();

app.UseGlobalExceptionHandler();

app.UseStaticFiles();

app.MapControllers();

app.Run();

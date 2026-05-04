using MedAI;
using Scalar.AspNetCore;
using Arora.GlobalExceptionHandler;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies(builder.Configuration);

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

using MedAI;
using Scalar.AspNetCore;
using Arora.GlobalExceptionHandler;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies(builder.Configuration);

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
app.MapOpenApi();
app.MapScalarApiReference();
//}
app.UseHttpsRedirection();

app.UseCors(CorsPolicy.AllowAll);

app.UseAuthorization();

app.UseGlobalExceptionHandler();

app.UseStaticFiles();

app.MapControllers();

//app.MapGet("/download", () =>
//{
//    var filePath = Path.Combine(app.Environment.ContentRootPath, "MedAI.db");
//    var contentType = "application/octet-stream";
//    var fileName = "MedAI.db";
//    return Results.File(filePath, contentType, fileName);
//});

app.Run();

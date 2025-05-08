using Azure.Storage.Blobs;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApplicationInsightsTelemetry();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var configuration = builder.Configuration;
string blobConnectionString = configuration.GetConnectionString("StorageAccount");

//register blob service client by reading StorageAccount connection string from appsettings.json
builder.Services.AddSingleton(x => new BlobServiceClient(blobConnectionString));

var aiOptions = new ApplicationInsightsServiceOptions();

builder.Configuration.GetSection("ApplicationInsights").Bind(aiOptions);
builder.Services.AddApplicationInsightsTelemetry(aiOptions);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

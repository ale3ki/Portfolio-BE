using Microsoft.Azure.Cosmos;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using nkport_api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//Ignoring null on write within json serializer.
builder.Services.AddControllers().AddJsonOptions(options =>{
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Initialize Cosmos Client and Blob Client
BlobServiceClient blobServiceClient = new BlobServiceClient(builder.Configuration["BLOB_CONNECTION_STRING"]);
CosmosClient cosmosClient = new CosmosClient(
    builder.Configuration["COSMOS_ENDPOINT"],
    builder.Configuration["COSMOS_KEY"]);

builder.Services.AddSingleton(cosmosClient);
builder.Services.AddSingleton(blobServiceClient);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()){
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
// Add CORS policy
app.UseCors(policy => policy
    .WithOrigins("http://localhost:3000", "http://localhost:3000/") // React app's URI
    .AllowAnyMethod()
    .AllowAnyHeader());

app.MapControllers();
app.Run();

using FIndex.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<EmbeddingService>(c =>
    c.BaseAddress = new Uri("http://localhost:8000/embed"));

builder.Services.AddHttpClient<QdrantService>(c =>
    c.BaseAddress = new Uri("http://localhost:6333"));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
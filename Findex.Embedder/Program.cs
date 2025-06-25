using Findex.Embedder.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();   

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<FaceEmbeddingService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
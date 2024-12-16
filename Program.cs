using importer_app.Models;

var builder = WebApplication.CreateBuilder(args);
    string[] allowedOrigins;
    if (builder.Environment.IsDevelopment())
    {
        allowedOrigins = new[] { "http://localhost:3000" };
    }
    else
    {
        allowedOrigins = new[] { "https://importor-app.onrender.com" };
    }
// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin",
        build =>
        {
            build.WithOrigins(allowedOrigins)
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials()
                   .WithExposedHeaders("Content-Disposition"); ;
        });
});

builder.Services.AddControllers();
builder.Services.AddSingleton<List<Quiz>>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowOrigin");
app.UseAuthorization();

app.MapControllers();

app.Run();

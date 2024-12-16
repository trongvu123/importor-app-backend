using importer_app.Models;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;
// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowOrigin",
        build =>
        {
            if (env.IsDevelopment())
            {
                build.WithOrigins("http://localhost:3000"); // Môi tr??ng phát tri?n
            }
            else
            {
             
                build.WithOrigins("https://importor-app.onrender.com", "https://app-cua-quynh.vercel.app"); // Môi tr??ng s?n xu?t
            }

            build.AllowAnyHeader()
                 .AllowAnyMethod()
                 .AllowCredentials()
                 .WithExposedHeaders("Content-Disposition");
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

using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Model;
using Microsoft.AspNetCore.SignalR; // Add this using statement for SignalR
using WebApplication1.Hubs;
using WebApplication1.Controllers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddHttpClient();
// Configure services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHostedService<ServiceUpdater>();

// Allow all origins, methods, and headers
builder.Services.AddCors(options => options.AddPolicy("CorsPolicy",
        builder =>
        {
            builder.AllowAnyHeader()
                   .AllowAnyMethod()
                   .SetIsOriginAllowed((host) => true)
                   .AllowCredentials();
        }));

// Add SignalR services
builder.Services.AddSignalR(); // This line registers SignalR services

builder.Services.AddControllers();
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

// Apply the CORS policy to allow all origins
app.UseCors("AllowAllOrigins");

app.UseAuthorization();

// Map the controllers
app.MapControllers();

// Map the SignalR Hub endpoint
app.MapHub<TreeNodeHub>("/treeNodeHub"); // Ensure TreeNodeHub is defined as shown previously
app.UseCors("CorsPolicy");
app.Run();
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using SANTA.PoS.Business.Interfaces;
using SANTA.PoS.Business.Mappings;
using SANTA.PoS.Business.Services;
using SANTA.PoS.Data.Persistence;
using SANTA.PoS.Data.Repositories;
using SANTA.PoS.Middleware;


var builder = WebApplication.CreateBuilder(args);

// Add DB Context to the container.

builder.Services.AddDbContext<SantaContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));

// Add services to the container.
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ProductService>();

// Add AutoMapper
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "SANTA PoS API",
        Description = "API for managing products in the SANTA PoS system",
    });
});

var app = builder.Build();

// Add global exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

using Microsoft.EntityFrameworkCore;
using Slottet.Application.Interfaces;
using Slottet.Infrastructure.Data;
using Slottet.Infrastructure.Repositories;
using Slottet.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SlottetDb")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ICitizenRepository, FakeCitizenRepository>();
builder.Services.AddScoped<IDepartmentRepository, FakeDepartmentRepository>();
builder.Services.AddScoped<IOverlapSelectionService, OverlapSelectionService>();
builder.Services.AddScoped<IOverlapOverviewRepository, FakeOverlapOverviewRepository>();
builder.Services.AddScoped<IOverlapOverviewService, OverlapOverviewService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
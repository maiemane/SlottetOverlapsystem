using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Slottet.Api.Auth;
using Slottet.Application.Interfaces;
using Slottet.Infrastructure.Data;
using Slottet.Infrastructure.Repositories;
using Slottet.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                 ?? throw new InvalidOperationException("Jwt configuration is missing.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SlottetDb")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("SlottetWeb", policy =>
    {
        policy.WithOrigins("https://localhost:7169", "http://localhost:5150")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<JwtTokenGenerator>();
builder.Services.AddScoped<PasswordVerificationService>();

builder.Services.AddScoped<ICitizenRepository, FakeCitizenRepository>();
builder.Services.AddScoped<IDepartmentRepository, FakeDepartmentRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
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
app.UseCors("SlottetWeb");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

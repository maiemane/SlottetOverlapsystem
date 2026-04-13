using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Slottet.Application.Interfaces;
using Slottet.Application.Services.Audit;
using Slottet.Application.Services.Auth;
using Slottet.Application.Services.Employees;
using Slottet.Application.Services.Citizens;
using Slottet.Application.Services.Departments;
using Slottet.Application.Services.Medications;
using Slottet.Application.Services.Overlap;
using Slottet.Application.Services.PhoneAllocations;
using Slottet.Application.Services.Phones;
using Slottet.Application.Services.Responsibilities;
using Slottet.Application.Services.ShiftDefinitions;
using Slottet.Application.Services.ShiftTasks;
using Slottet.Application.Services.SpecialEvents;
using Slottet.Application.Services.Staffing;
using Slottet.Infrastructure.Auth;
using Slottet.Infrastructure.Data;
using Slottet.Infrastructure.Logging;
using Slottet.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
                 ?? throw new InvalidOperationException("Jwt configuration is missing.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("SlottetDb")));
builder.Services.AddHttpContextAccessor();

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

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    const string bearerSchemeName = "bearerAuth";

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Slottet API",
        Version = "v1"
    });

    options.AddSecurityDefinition(bearerSchemeName, new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Indsæt kun selve JWT tokenet her. Swagger tilføjer selv Bearer-prefix."
    });
    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference(bearerSchemeName, null, null)] = new List<string>()
    });
});
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ICreateCitizenService, CreateCitizenService>();
builder.Services.AddScoped<ICreateCitizenFixedMedicationService, CreateCitizenFixedMedicationService>();
builder.Services.AddScoped<ICitizenFixedMedicationService, CitizenFixedMedicationService>();
builder.Services.AddScoped<IMedicationRegistrationService, MedicationRegistrationService>();
builder.Services.AddScoped<IOverlapOverviewService, OverlapOverviewService>();
builder.Services.AddScoped<IPhoneAllocationService, PhoneAllocationService>();
builder.Services.AddScoped<IPhoneService, PhoneService>();
builder.Services.AddScoped<IResponsibilityAssignmentService, ResponsibilityAssignmentService>();
builder.Services.AddScoped<IResponsibilityTypeService, ResponsibilityTypeService>();
builder.Services.AddScoped<IShiftDefinitionService, ShiftDefinitionService>();
builder.Services.AddScoped<IShiftTaskService, ShiftTaskService>();
builder.Services.AddScoped<ISpecialEventService, SpecialEventService>();
builder.Services.AddScoped<IStaffAllocationService, StaffAllocationService>();
builder.Services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IPasswordHashingService, PasswordHashingService>();
builder.Services.AddScoped<IPasswordVerificationService, PasswordVerificationService>();

builder.Services.AddScoped<ICitizenCreationRepository, CitizenCreationRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IMedicationRegistrationRepository, MedicationRegistrationRepository>();
builder.Services.AddScoped<IOverlapOverviewRepository, OverlapOverviewRepository>();
builder.Services.AddScoped<IPhoneAllocationRepository, PhoneAllocationRepository>();
builder.Services.AddScoped<IPhoneRepository, PhoneRepository>();
builder.Services.AddScoped<IResponsibilityAssignmentRepository, ResponsibilityAssignmentRepository>();
builder.Services.AddScoped<IResponsibilityTypeRepository, ResponsibilityTypeRepository>();
builder.Services.AddScoped<IShiftDefinitionRepository, ShiftDefinitionRepository>();
builder.Services.AddScoped<IShiftTaskRepository, ShiftTaskRepository>();
builder.Services.AddScoped<ISpecialEventRepository, SpecialEventRepository>();
builder.Services.AddScoped<IStaffAllocationRepository, StaffAllocationRepository>();

var app = builder.Build();

await app.Services.SeedAuthenticationDataAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("SlottetWeb");
app.UseAuthentication();
app.UseMiddleware<AccessLogMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

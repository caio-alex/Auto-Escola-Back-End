using System.Text;
using System.Text.Json.Serialization;
using AutoEscola.Api.Api.Middlewares;
using AutoEscola.Api.Application.Services;
using AutoEscola.Api.Infrastructure.Data;
using AutoEscola.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Controllers + Enum as String in JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
app.UseCors("AllowFrontend");
// 2. Database Configuration (Supports In-Memory out-of-the-box or SQL Server)
bool useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase", true);
if (useInMemory)
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseInMemoryDatabase("AutoEscolaInMemoryDb"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// 3. Dependency Injection (Services)
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<LessonBookingService>();

// 4. JWT Authentication & RBAC Authorization
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSection["Secret"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});

builder.Services.AddAuthorization();

// 5. CORS Policy for Front-End (React / Vite / Next.js)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 6. Swagger with JWT Bearer Support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Auto Escola SaaS API (Remodelada)",
        Version = "v1",
        Description = "Back-End Sênior com Auth JWT, RBAC (Aluno, Instrutor, Admin), Wizard de Slots e Regras do DETRAN."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT no formato: Bearer {seu_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Seed Initial Demo Data (Admin, Instructors, Student, Fleet, Sample Lesson)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbInitializer.Seed(db);
}

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auto Escola SaaS API v1");
    c.RoutePrefix = string.Empty; // Opens Swagger directly on root URL
});

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

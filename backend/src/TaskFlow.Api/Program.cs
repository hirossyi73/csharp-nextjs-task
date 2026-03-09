using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Api.Middleware;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Services;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Repositories;
using TaskFlow.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<UserRepositoryInterface, UserRepository>();
builder.Services.AddScoped<RefreshTokenRepositoryInterface, RefreshTokenRepository>();
builder.Services.AddScoped<EmailVerificationTokenRepositoryInterface, EmailVerificationTokenRepository>();
builder.Services.AddScoped<PasswordResetTokenRepositoryInterface, PasswordResetTokenRepository>();
builder.Services.AddScoped<TaskRepositoryInterface, TaskRepository>();

// Services
builder.Services.AddScoped<PasswordHasherInterface, PasswordHasher>();
builder.Services.AddScoped<JwtServiceInterface, JwtService>();
builder.Services.AddScoped<EmailServiceInterface, EmailService>();
builder.Services.AddScoped<AuthServiceInterface, AuthService>();
builder.Services.AddScoped<TaskServiceInterface, TaskService>();

// Controllers
builder.Services.AddControllers();

// JWT 認証
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey が設定されていません");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration["App:FrontendUrl"] ?? "http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "TaskFlow API");

app.Run();

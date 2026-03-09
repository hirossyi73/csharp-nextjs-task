using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using TaskFlow.Api.Middleware;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Services;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Repositories;
using TaskFlow.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Serilog 構造化ログ
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

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
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // FW 標準の ProblemDetails ではなく、アプリ統一のエラー形式で返す
        options.InvalidModelStateResponseFactory = context =>
        {
            var details = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    e => e.Key,
                    e => e.Value!.Errors.Select(err => err.ErrorMessage).ToArray()
                );

            var response = new
            {
                error = new
                {
                    code = "VALIDATION_ERROR",
                    message = "入力内容に誤りがあります",
                    details
                }
            };

            return new UnprocessableEntityObjectResult(response);
        };
    });

// JWT 認証
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey が設定されていません");

// 本番環境でデフォルトの開発用キーが使われていないことを検証する
if (!builder.Environment.IsDevelopment()
    && jwtSecretKey == "ThisIsADevelopmentSecretKeyThatShouldBeAtLeast32CharactersLong!")
{
    throw new InvalidOperationException(
        "本番環境では JWT_SECRET_KEY に安全なランダム値を設定してください。開発用デフォルト値は使用できません。");
}

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

app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "TaskFlow API");

app.Run();

// WebApplicationFactory からアクセスするために必要
public partial class Program { }

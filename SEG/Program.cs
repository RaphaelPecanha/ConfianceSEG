using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SEG.Context;
using Confiance.SEG.CrossCutting.Filters;
using SEG.Models;
using Confiance.SEG.Infrastructure.Repositories;
using Confiance.SEG.Application.Interfaces;
using SEG.Extensions;
using SEG.Logging;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("CorsPolicy", builder =>
//    {
//        builder
//            .WithOrigins("http://localhost:5173") // sua aplicação React (Vite)
//            .AllowAnyHeader()
//            .AllowAnyMethod()
//            .AllowCredentials(); // se você estiver usando autenticação com cookies ou headers
//    });
//});


builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(ApiExceptionFilter));
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SEG", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

string mySqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(mySqlConnection, ServerVersion.AutoDetect(mySqlConnection)));

builder.Services.AddScoped<ApiLoggingFilter>();
// Repositories / Classes (from Infrastructure)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Confiance.SEG.Infrastructure.Repositories.Repository<>));
builder.Services.AddScoped<IUnitOfWork, Confiance.SEG.Infrastructure.Repositories.UnitOfWork>();
builder.Services.AddScoped<IUsuariosRepository, Confiance.SEG.Infrastructure.Repositories.UsuarioRepository>();

builder.Services.AddScoped<ITokenService, Confiance.SEG.Infrastructure.Services.TokenService>();

builder.Services.AddAuthorization( options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

var secretKey = builder.Configuration["JWT:SecretKey"] ?? throw new ArgumentException("Invalid secret key");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Em produ��o colocar essa op��o como true
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidAudience = builder.Configuration["JWT:ValidAudiences"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddApiVersioning(o => 
{
    o.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;
    o.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader());
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});


builder.Logging.AddProvider(new CustomLoggerProvider(
    new CustomLoggerProviderConfiguration
    {
        LogLevel = LogLevel.Information
    },
    builder.Configuration // Passa corretamente o IConfiguration
));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ConfigureExceptionHandler();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

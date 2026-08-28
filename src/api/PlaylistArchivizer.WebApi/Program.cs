using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PlaylistArchivizer.Application.Interfaces;
using PlaylistArchivizer.Application.Services;
using PlaylistArchivizer.Infrastructure.Persistence.Data;
using PlaylistArchivizer.Infrastructure.Persistence.Repositories;
using PlaylistArchivizer.Infrastructure.Persistence.Services;
using PlaylistArchivizer.Infrastructure.SpotifyApi.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

string jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("JWT Secret is missing from configuration.");

builder.Services.AddControllers();

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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddHttpClient();
builder.Services.AddDataProtection();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddMemoryCache();


builder.Services.AddTransient<ISpotifyLoginService, SpotifyLoginService>();
builder.Services.AddTransient<IAuthCodeService, AuthCodeService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

builder.Services.AddScoped<ISpotifyTokenRepository, SpotifyCredentialRepository>();

var app = builder.Build();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

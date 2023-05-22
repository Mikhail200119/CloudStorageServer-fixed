using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CloudStorage.Api;
using CloudStorage.Api.Mapping;
using CloudStorage.Api.Options;
using CloudStorage.Api.Services;
using CloudStorage.BLL.MappingProfiles;
using CloudStorage.BLL.Options;
using CloudStorage.BLL.Services;
using CloudStorage.BLL.Services.Interfaces;
using CloudStorage.DAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5173/", "http://localhost:5173/sign-up", "http://localhost:5173/sign-in")
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            BearerFormat = "JWT",
            In = ParameterLocation.Header
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = JwtBearerDefaults.AuthenticationScheme
                    }
                },
                Array.Empty<string>()
            }
        });
    });

builder.WebHost.ConfigureServices(services =>
{
    var databaseConnectionString = builder.Configuration.GetConnectionString("CloudStorageDatabaseConnection");

    services
        .AddTransient<ICloudStorageManager, CloudStorageManager>()
        .AddTransient<ICloudStorageUnitOfWork, CloudStorageUnitOfWork>()
        .AddTransient<IUserService, UserService>()
        .AddSingleton<IDataHasher, Sha1DataHasher>()
        .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
        .AddTransient<IFileStorageService, FileStorageService>()
        .AddTransient<SecurityTokenHandler, JwtSecurityTokenHandler>()
        .AddTransient<IJwtTokenProvider, JwtTokenProvider>()
        .AddTransient<IUsersManager, UsersManager>()
        .AddTransient<IAesEncryptor, AesEncryptor>()
        .AddTransient<IDeduplicationService, DeduplicationService>()
        .AddTransient<IWordToPdfConverter, WordToPdfConverter>()
        .AddTransient<IDisplayContentTypeMapper, DisplayContentTypeMapper>()
        .AddDbContext<CloudStorageUnitOfWork>(optionsBuilder =>
            optionsBuilder.UseNpgsql(databaseConnectionString));


    var fileStorageOptions = builder.Configuration.GetSection(nameof(FileStorageOptions)).Get<FileStorageOptions>();

    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    
    /*services.Configure<FileStorageOptions>(opt =>
    {
        opt.FilesDirectoryPath = Path.Combine(builder.Environment.WebRootPath, fileStorageOptions.FilesDirectoryPath);
        opt.FFmpegExecutablesPath = Path.Combine(builder.Environment.WebRootPath, fileStorageOptions.FFmpegExecutablesPath);
        opt.FilesDirectoryPath = "";
        opt.FFmpegExecutablesPath = "";
    });*/
    
    services.Configure<FileStorageOptions>(builder.Configuration.GetSection(nameof(FileStorageOptions)));
    services.Configure<ArchiveOptions>(builder.Configuration.GetSection(nameof(ArchiveOptions)));
    services.Configure<PdfConvertOptions>(builder.Configuration.GetSection(nameof(PdfConvertOptions)));
    services.Configure<DeduplicationOptions>(builder.Configuration.GetSection(nameof(DeduplicationOptions)));
    
    services.AddAutoMapper(
        typeof(FilesMappingProfile),
        typeof(FilesProfile));
});

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(bearerOptions =>
    {
        var jwtOptions = builder.Configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>();
        bearerOptions.SaveToken = true;

        bearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = false,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ValidAlgorithms = jwtOptions.EncryptionAlgorithms
        };
    });

builder.Services.AddAuthorization(opt =>
{
    opt.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

var deduplicator = app.Services.GetService<IDeduplicationService>();

var deduplicationThread = new Thread(async () =>
{
    while (true)
    {
        Thread.Sleep(TimeSpan.FromMinutes(1));
        await deduplicator.Deduplicate();
    }
}) { IsBackground = false };

deduplicationThread.Start();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(MyAllowSpecificOrigins);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
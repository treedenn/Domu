using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Infrastructure.Households;
using Domu.Api.Features.Spaces.Application.Items;
using Domu.Api.Features.Spaces.Application.Items.Ports;
using Domu.Api.Features.Spaces.Application.Spaces;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;
using Domu.Api.Features.Spaces.Infrastructure.Items;
using Domu.Api.Features.Spaces.Infrastructure.Spaces;
using Domu.Api.Features.Users.Application;
using Domu.Api.Features.Users.Application.Ports;
using Domu.Api.Features.Users.Infrastructure;
using Domu.Api.Features.Users.Interface.Auth;
using Domu.Api.Infrastructure.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ExternalAuthenticationOptions>(
    builder.Configuration.GetSection(ExternalAuthenticationOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string 'DefaultConnection' is required.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IHouseholdRepository, HouseholdRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<ISpaceRepository, SpaceRepository>();
builder.Services.AddScoped<ISpaceQueryService, SpaceQueryService>();
builder.Services.AddScoped<ICreateItemUseCase, CreateItemUseCase>();
builder.Services.AddScoped<IDeleteItemUseCase, DeleteItemUseCase>();
builder.Services.AddScoped<IUpdateItemUseCase, UpdateItemUseCase>();
builder.Services.AddScoped<IGetSpaceItemsUseCase, GetSpaceItemsUseCase>();
builder.Services.AddScoped<ICreateSpaceUseCase, CreateSpaceUseCase>();
builder.Services.AddScoped<IUpdateSpaceUseCase, UpdateSpaceUseCase>();
builder.Services.AddScoped<IDeleteSpaceUseCase, DeleteSpaceUseCase>();
builder.Services.AddScoped<IGetSpacesPageUseCase, GetSpacesPageUseCase>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEnsureUserUseCase, EnsureUserUseCase>();
builder.Services.AddScoped<IUserAccessor, UserAccessor>();
builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var settings = builder.Configuration
            .GetSection(ExternalAuthenticationOptions.SectionName)
            .Get<ExternalAuthenticationOptions>()
            ?? new ExternalAuthenticationOptions();

        if (!string.IsNullOrWhiteSpace(settings.Authority))
            options.Authority = settings.Authority;

        if (!string.IsNullOrWhiteSpace(settings.Audience))
            options.Audience = settings.Audience;

        options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
    });
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<AuthenticatedUserMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

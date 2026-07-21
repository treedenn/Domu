using System.Text.Json;
using System.Text.Json.Serialization;
using Domu.Api.Features.Auth.Application;
using Domu.Api.Features.Auth.Interface;
using Domu.Api.Features.Activities.Application;
using Domu.Api.Features.Activities.Infrastructure;
using Domu.Api.Features.Households.Application.Households;
using Domu.Api.Features.Households.Application.Households.Ports;
using Domu.Api.Features.Households.Application.Members;
using Domu.Api.Features.Households.Application.Members.Ports;
using Domu.Api.Features.Households.Infrastructure.Households;
using Domu.Api.Features.Households.Infrastructure.Members;
using Domu.Api.Features.Insights.Application;
using Domu.Api.Features.Insights.Application.Rules;
using Domu.Api.Features.ShoppingLists.Application.Items;
using Domu.Api.Features.ShoppingLists.Application.Items.Ports;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists;
using Domu.Api.Features.ShoppingLists.Application.ShoppingLists.Ports;
using Domu.Api.Features.ShoppingLists.Infrastructure.Items;
using Domu.Api.Features.ShoppingLists.Infrastructure.ShoppingLists;
using Domu.Api.Features.Spaces.Application.Items;
using Domu.Api.Features.Spaces.Application.Items.Ports;
using Domu.Api.Features.Spaces.Application.Expirations;
using Domu.Api.Features.Spaces.Application.Expirations.Ports;
using Domu.Api.Features.Spaces.Application.Search;
using Domu.Api.Features.Spaces.Application.Search.Ports;
using Domu.Api.Features.Spaces.Application.Spaces;
using Domu.Api.Features.Spaces.Application.Spaces.Ports;
using Domu.Api.Features.Spaces.Infrastructure.Items;
using Domu.Api.Features.Spaces.Infrastructure.Expirations;
using Domu.Api.Features.Spaces.Infrastructure.Search;
using Domu.Api.Features.Spaces.Infrastructure.Spaces;
using Domu.Api.Features.Users.Application;
using Domu.Api.Features.Users.Application.Ports;
using Domu.Api.Features.Users.Infrastructure;
using Domu.Api.Features.Users.Interface.Auth;
using Domu.Api.Infrastructure.Database;
using Domu.Api.Interface.RequestContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtAuthenticationOptions>(
    builder.Configuration.GetSection(JwtAuthenticationOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string 'DefaultConnection' is required.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention());

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IClientRequestContextAccessor, ClientRequestContextAccessor>();
builder.Services.AddScoped<IHouseholdActivityRecorder, HouseholdActivityRecorder>();
builder.Services.AddScoped<IHouseholdActivityQueryService, HouseholdActivityQueryService>();
builder.Services.AddScoped<IGetHouseholdInsightsUseCase, GetHouseholdInsightsUseCase>();
builder.Services.AddScoped<IInsightRule, FrequentShoppingListItemRule>();
builder.Services.AddScoped<IInsightRule, RestockCandidateRule>();
builder.Services.AddScoped<IInsightRule, ClearCheckedShoppingListItemsRule>();
builder.Services.AddScoped<IInsightRule, HouseholdSetupNextStepRule>();
builder.Services.AddScoped<IHouseholdRepository, HouseholdRepository>();
builder.Services.AddScoped<IHouseholdMembershipRepository, HouseholdMembershipRepository>();
builder.Services.AddScoped<IHouseholdInvitationSender, LoggingHouseholdInvitationSender>();
builder.Services.AddScoped<IHouseholdAccessService, HouseholdAccessService>();
builder.Services.AddScoped<CreateHouseholdUseCase>();
builder.Services.AddScoped<GetHouseholdUseCase>();
builder.Services.AddScoped<GetHouseholdsUseCase>();
builder.Services.AddScoped<UpdateHouseholdUseCase>();
builder.Services.AddScoped<DeleteHouseholdUseCase>();
builder.Services.AddScoped<GetHouseholdMembersUseCase>();
builder.Services.AddScoped<GetHouseholdMemberUseCase>();
builder.Services.AddScoped<UpdateHouseholdMemberUseCase>();
builder.Services.AddScoped<GetHouseholdInvitationsUseCase>();
builder.Services.AddScoped<InviteHouseholdMemberUseCase>();
builder.Services.AddScoped<AcceptHouseholdInvitationUseCase>();
builder.Services.AddScoped<IShoppingListRepository, ShoppingListRepository>();
builder.Services.AddScoped<IShoppingListItemRepository, ShoppingListItemRepository>();
builder.Services.AddScoped<GetShoppingListsUseCase>();
builder.Services.AddScoped<GetShoppingListUseCase>();
builder.Services.AddScoped<CreateShoppingListUseCase>();
builder.Services.AddScoped<UpdateShoppingListUseCase>();
builder.Services.AddScoped<DeleteShoppingListUseCase>();
builder.Services.AddScoped<GetShoppingListItemsUseCase>();
builder.Services.AddScoped<CreateShoppingListItemUseCase>();
builder.Services.AddScoped<UpdateShoppingListItemUseCase>();
builder.Services.AddScoped<SetShoppingListItemCheckedStateUseCase>();
builder.Services.AddScoped<DeleteShoppingListItemUseCase>();
builder.Services.AddScoped<ClearCheckedShoppingListItemsUseCase>();
builder.Services.AddScoped<SubmitCheckedShoppingListItemsUseCase>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IHouseholdExpirationQueryService, HouseholdExpirationQueryService>();
builder.Services.AddScoped<ISpaceRepository, SpaceRepository>();
builder.Services.AddScoped<ISpaceQueryService, SpaceQueryService>();
builder.Services.AddScoped<ISpacesAndItemsSearchService, SpacesAndItemsSearchService>();
builder.Services.AddScoped<ISpaceAccessService, SpaceAccessService>();
builder.Services.AddScoped<ICreateItemUseCase, CreateItemUseCase>();
builder.Services.AddScoped<IDeleteItemUseCase, DeleteItemUseCase>();
builder.Services.AddScoped<IUpdateItemUseCase, UpdateItemUseCase>();
builder.Services.AddScoped<IReplaceItemEntriesUseCase, ReplaceItemEntriesUseCase>();
builder.Services.AddScoped<IStageInventoryBatchesUseCase, StageInventoryBatchesUseCase>();
builder.Services.AddScoped<IInventoryItemLookup, InventoryItemLookup>();
builder.Services.AddScoped<IGetSpaceItemsUseCase, GetSpaceItemsUseCase>();
builder.Services.AddScoped<GetHouseholdExpirationsUseCase, GetHouseholdExpirationsUseCase>();
builder.Services.AddScoped<ICreateSpaceUseCase, CreateSpaceUseCase>();
builder.Services.AddScoped<IGetSpaceUseCase, GetSpaceUseCase>();
builder.Services.AddScoped<IUpdateSpaceUseCase, UpdateSpaceUseCase>();
builder.Services.AddScoped<IMoveSpaceUseCase, MoveSpaceUseCase>();
builder.Services.AddScoped<IDeleteSpaceUseCase, DeleteSpaceUseCase>();
builder.Services.AddScoped<IGetSpacesPageUseCase, GetSpacesPageUseCase>();
builder.Services.AddScoped<ISearchSpacesAndItemsUseCase, SearchSpacesAndItemsUseCase>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEnsureUserUseCase, EnsureUserUseCase>();
builder.Services.AddScoped<IActorAccessor, HttpContextActorAccessor>();
builder.Services.AddScoped<IActorResolver, ZitadelActorResolver>();
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
    });
builder.Services.AddAuthorization();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var settings = builder.Configuration
                           .GetSection(JwtAuthenticationOptions.SectionName)
                           .Get<JwtAuthenticationOptions>()
                       ?? new JwtAuthenticationOptions();

        if (!string.IsNullOrWhiteSpace(settings.Authority))
            options.Authority = settings.Authority;

        if (!string.IsNullOrWhiteSpace(settings.Audience))
            options.Audience = settings.Audience;

        options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
    });
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();

app.UseMiddleware<ClientRequestContextMiddleware>();
app.UseAuthentication();
app.UseMiddleware<AuthenticatedActorMiddleware>();
app.UseAuthorization();

app.MapGroup("/api/v1").MapControllers();

app.Run();

using System.Security.Claims;
using System.Text;
using Flow.Server.Hubs;
using Flow.Server.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Swagger Config
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

var databaseOptions = new DatabaseOptions();
// This binds the values from the db options section to a DatabaseOptions object
builder.Configuration.GetSection("DatabaseOptions").Bind(databaseOptions);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(databaseOptions.ConnectionString, action =>
    {
        action.CommandTimeout(databaseOptions.CommandTimeoutDuration); // Query will timeout in 30 seconds
    });

    options.EnableDetailedErrors(databaseOptions.EnableDetailedErrors);

    // enable sensetive logging in development env only
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging(databaseOptions.EnableSensetiveLogs);
    }
});

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // configure the password 

    // configure lockout settings
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromSeconds(30); // lockout the user for 30 seconds
    options.Lockout.MaxFailedAccessAttempts = 3;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// binds the values for the JWT settings in appsettings to an instance of JwtOptions
builder.Services.AddScoped(sp =>
{
    var jwtOptions = new JwtOptions();
    builder.Configuration.GetSection("JwtSettings").Bind(jwtOptions);

    return jwtOptions;
});

builder.Services.AddAuthentication(options =>
{
    // set the authentication scheme
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(bearerOptions =>
{
    var jwtOptions = new JwtOptions();
    builder.Configuration.GetSection("JwtSettings").Bind(jwtOptions);

    bearerOptions.Events = new JwtBearerEvents()
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrWhiteSpace(accessToken) && path.StartsWithSegments("/chat-threads-hub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };

    var key = Encoding.UTF8.GetBytes(jwtOptions.Secret);
    bearerOptions.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
    {
        ValidateIssuer = jwtOptions.ValidateIssuer,
        ValidateAudience = jwtOptions.ValidateAudience,
        ValidateLifetime = jwtOptions.ValidateLifetime,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        RequireExpirationTime = jwtOptions.RequiresExpirationTime,
        ValidIssuer = jwtOptions.Issuer,
        ValidAudience = jwtOptions.Audience,
    };
});

builder.Services.AddScoped(sp =>
{
    var userInfo = new UserInfo();
    var httpContextAccessor = sp.GetService<IHttpContextAccessor>();

    if (httpContextAccessor is not null)
    {
        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            userInfo.UserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            userInfo.Name = httpContext.User.FindFirstValue(ClaimTypes.Name);
        }
    }

    return userInfo;
});

#region Custom services registration

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IContactRequestsRepository, ContactRequestsRepository>();
builder.Services.AddScoped<IFilesRepository, FilesRepository>();
builder.Services.AddScoped<INotificationsRepository, NotificationsRepository>();
builder.Services.AddTransient<INotificationPublisherService, NotificationPublisherService>();
builder.Services.AddScoped<IThreadRepository, ThreadRepository>();
builder.Services.AddScoped<IMessagesRepository, MessagesRepository>();

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();

    // use Swagger UI in development
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapHub<NotificationsHub>("notifications");
app.MapHub<ChatHub>("chat-threads-hub");

app.MapFallbackToFile("index.html");


app.Run();

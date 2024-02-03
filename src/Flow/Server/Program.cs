using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

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

#region Custom services registration

builder.Services.AddScoped<IAuthRepository, AuthRepository>();

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
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
app.MapFallbackToFile("index.html");

app.Run();

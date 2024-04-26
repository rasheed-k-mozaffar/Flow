using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Flow.Client;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.SessionStorage;
using Flow.Client.State;
using System.Security.AccessControl;
using Flow.Client.Settings;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddAuthorizationCore();

builder.Services.AddTransient<AuthorizationMessageHandler>();
builder.Services.AddScoped<IJwtsManager, JwtsManager>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IContactRequestsService, ContactRequestsService>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
builder.Services.AddScoped<IThreadsService, ThreadsService>();
builder.Services.AddScoped<IMessagesService, MessagesService>();
builder.Services.AddScoped<IFilesService, FilesService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IGroupsService, GroupsService>();

builder.Services.AddSingleton<AppSettings>(sp =>
{
    AppSettings appSettings = new();
    builder.Configuration.Bind("AppSettings", appSettings);

    return appSettings;
});

builder.Services.AddSingleton<ApplicationState>();


builder.Services.AddHttpClient
(
    "Flow.ServerAPI",
    client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
).AddHttpMessageHandler<AuthorizationMessageHandler>();

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Flow.ServerAPI"));

await builder.Build().RunAsync();

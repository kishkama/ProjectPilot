using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using ProjectPilot.UI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient to point to the API server
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5242") });

// Add MudBlazor services
builder.Services.AddMudServices();

await builder.Build().RunAsync();

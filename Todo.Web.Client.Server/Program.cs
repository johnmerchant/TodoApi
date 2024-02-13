using Todo.Web.Client.Server;
using Todo.Web.Client.Server.Authentication;
using Todo.Web.Client.Server.Components;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddRazorPages().WithRazorPagesRoot("/Components/Pages");

builder.Services.AddHttpContextAccessor();

var todoUrl = builder.Configuration.GetServiceUri("todoapi")?.ToString() ??
              builder.Configuration["TodoApiUrl"] ?? 
              throw new InvalidOperationException("Todo API URL is not configured");

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpClient<AuthClient>(client =>
{
    client.BaseAddress = new(todoUrl);
});

builder.Services.AddHttpClient<TodoClient>(client =>
{
    client.BaseAddress = new Uri(todoUrl);

    // The cookie auth stack detects this header and avoids redirects for unauthenticated
    // requests
    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
});


// Configure auth with the front end
builder.AddAuthentication();
builder.Services.AddSession();
builder.Services.AddAuthorizationBuilder();

// Add razor pages so we can render the Blazor WASM todo component
builder.Services.AddRazorPages();

// Add the forwarder to make sending requests to the backend easier
builder.Services.AddHttpForwarder();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseSession();

// Configure the APIs
app.MapRazorPages();
app.MapRazorComponents<Todo.Web.Client.Server.Components.App>().AddInteractiveServerRenderMode().AddInteractiveWebAssemblyRenderMode();
    


app.MapTodos(todoUrl);

app.Run();
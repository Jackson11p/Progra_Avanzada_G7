using Proyecto_JN_G7.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// HttpClient + JWT (via Session)
builder.Services.AddHttpClient();                 
builder.Services.AddHttpContextAccessor();       
builder.Services.AddTransient<TokenHandler>();    

builder.Services.AddHttpClient("Api", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Start:ApiUrl"]!);
})
.AddHttpMessageHandler<TokenHandler>();

// Utilitarios + Session
builder.Services.AddScoped<IUtilitarios, Utilitarios>();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseSession();

app.UseExceptionHandler("/Error");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

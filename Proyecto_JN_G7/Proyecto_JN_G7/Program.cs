using Proyecto_JN_G7.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IUtilitarios, Utilitarios>();
builder.Services.AddSession();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
   app.UseHsts();
}

app.UseSession();

app.UseExceptionHandler("/Home/Error");

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

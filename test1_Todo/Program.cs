var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache(); // Il faut l ajouter pour garder la session valable entre plusieur requetes 
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});
// injection de dependance :  la possibilite de donner a une classe a de travailler avec une class b sans l instancier
//addscoped = 1 instance -> par requete
builder.Services.AddScoped<test1_Todo.Services.ISessionManagerService, test1_Todo.Services.SessionManagerService>();
// sengleton = 1 seule instance pour tte l'app , ( 1 seul acces au fichier , performance , log = ressource partagee ) 
builder.Services.AddSingleton<test1_Todo.Services.ILoggingService, test1_Todo.Services.FileLoggingService>();
// Register filter for dependency injection
builder.Services.AddScoped<test1_Todo.Filtres.LoggingFilter>();
// Register theme service (Scoped = 1 instance par requête HTTP)
builder.Services.AddScoped<test1_Todo.Services.IThemeService, test1_Todo.Services.ThemeService>();
// Register ThemeFilter pour dependency injection
builder.Services.AddScoped<test1_Todo.Filtres.ThemeFilter>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseSession(); // verifier la presence de session || si exist Ok || sinon il la cree || equivalant dans php a session_start()
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Inscription}/{id?}")
    .WithStaticAssets();


app.Run();

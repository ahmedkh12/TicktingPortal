using Portal.data;
using Portal.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Portal") ?? throw new InvalidOperationException("Connection string 'identity' not found.")));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Portal"));
    options.LogTo(Console.WriteLine); // Log SQL queries to console
                                      // options.LogTo(File.WriteAllText("path_to_log_file.txt")); // Log SQL queries to a file
});


builder.Services.AddIdentity<ApplicationUser, ApplicationRole>( // add application user to generic 
    options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequiredUniqueChars = 4;
        // Other settings can be configured here
        options.SignIn.RequireConfirmedEmail = true; //require email be confirmed


    }
    ).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();  //add user identity class to dbcontext and also we modify the custom  user password 


builder.Services.ConfigureApplicationCookie(options =>
{
    // If the LoginPath isn't set, ASP.NET Core defaults the path to /Account/Login.
    options.LoginPath = "/Account/Login"; // Set your login path here
    // If the AccessDenied isn't set, ASP.NET Core defaults the path to /Account/AccessDenied
    options.AccessDeniedPath = "/Account/AccessDenied"; // Set your access denied path here

});

builder.Services.AddTransient<ISenderEmail, EmailSender>(); //for email services

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();

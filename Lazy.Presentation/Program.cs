using System.Text;
using Lazy.Infrastructure;
using Lazy.Presentation.OptionsSetup;
using Lazy.Repository;
using Lazy.Services.Authentication;
using Lazy.Services.Author;
using Lazy.Services.Post;
using Lazy.Services.UserManagement;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Lazy.Presentation;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<LazyBlogDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("LazyBlogDbContext")));


        builder.Services.AddScoped<IAuthorService, AuthorService>();
        builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
        builder.Services.AddScoped<IPostService, PostService>();
        builder.Services.AddScoped<IPostRepository, PostRepository>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IJwtProvider, JwtProvider>();

        // Add services to the container.
        //  builder.Services.AddControllersWithViews();
        builder.Services.AddControllers();
        builder.Services.AddSwaggerGen();

        builder.Services.ConfigureOptions<JwtOptionSetup>();
        builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();


        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

      

       

        WebApplication app = builder.Build();

        CreateDbIfNotExists(app);


        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseStaticFiles();

       // app.UseRouting();

        app.UseAuthentication();
        
        app.UseAuthorization();
        
        app.MapControllers();

        //app.MapControllerRoute(
        //    name: "default",
        //    pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();


    }


    private static void CreateDbIfNotExists(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<LazyBlogDbContext>();
            DbInitializer.Initialize(context);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred creating the DB.");
        }
    }
}
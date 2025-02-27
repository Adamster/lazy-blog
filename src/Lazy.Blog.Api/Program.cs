using Lazy.Application.Behaviors;
using Lazy.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scrutor;
using FluentValidation;
using Lazy.App.Extensions;
using Lazy.App.OptionsSetup;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Domain.Entities;
using Lazy.Infrastructure.Authorization;
using Lazy.Persistence.Interceptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Serilog;
using AssemblyReference = Lazy.Infrastructure.AssemblyReference;

using Azure.Monitor.OpenTelemetry.AspNetCore;
using Lazy.Application.Abstractions.Email;
using Lazy.Blog.Api.OptionsSetup;
using Lazy.Infrastructure.Services.Impl;
using Scalar.AspNetCore;

string lazyCorsPolicyName = "lazy-blog";
var today = DateTime.Today;
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File($"Logs\\{today.Year}\\{today.Month}\\{today.Day}\\Logs.log")
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    builder.Host.UseSerilog();
    
    builder.Services.AddOpenTelemetry()
        .UseAzureMonitor();
    
    builder
        .Services
        .Scan(
            selector => selector
                .FromAssemblies(
                    AssemblyReference.Assembly,
                    Lazy.Persistence.AssemblyReference.Assembly)
                .AddClasses(false)
                .UsingRegistrationStrategy(RegistrationStrategy.Skip)
                .AsMatchingInterface()
                .WithScopedLifetime());

    builder.Services.AddMediatR(configuration =>
    {
        configuration.RegisterServicesFromAssembly(Lazy.Application.AssemblyReference.Assembly);
    });

    builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
    //TODO: add after adding few commands with Domain events
    //builder.Services.Decorate(typeof(INotificationHandler<>), typeof())

    builder.Services.AddValidatorsFromAssembly(
        Lazy.Application.AssemblyReference.Assembly,
        includeInternalTypes: true);

    string connectionString = builder.Configuration.GetConnectionString("Database")!;

    builder.Services.AddSingleton<UpdateAuditableEntitiesInterceptor>();
    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    builder.Services.AddSingleton<ICurrentUserContext, CurrentUserContext>();
    builder.Services.AddSingleton<IEmailService, SendGridEmailSender>();
    
    builder.Services.AddDbContext<LazyBlogDbContext>(
        (sp, optionsBuilder) =>
        {
            optionsBuilder.UseSqlServer(connectionString);
        });

    builder.Services.AddDefaultIdentity<User>()
        .AddEntityFrameworkStores<LazyBlogDbContext>();

    builder
        .Services
        .AddControllers()
        .AddApplicationPart(Lazy.Presentation.AssemblyReference.Assembly);

    OpenApi.AddOpenApi(builder.Services);

    builder.Services.AddCors(o => o.AddPolicy(lazyCorsPolicyName, policyBuilder =>
    {
        policyBuilder
            .WithOrigins("http://localhost:3000")
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    }));

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer();

    builder.Services.ConfigureOptions<JwtOptionsSetup>();
    builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();
    builder.Services.ConfigureOptions<AzureBlobStorageOptionsSetup>();
    builder.Services.ConfigureOptions<SendGridOptionsSetup>();

    var app = builder.Build();

    CreateDbIfNotExists(app);
    
    app.MapOpenApi();
    
    app.UseHttpsRedirection();

    app.UseAuthentication();

    app.UseAuthorization();

    app.UseCors(lazyCorsPolicyName);
    
    app.MapControllers();

    app.MapScalarApiReference(o => o.WithTheme(ScalarTheme.DeepSpace));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}


//TODO: Remove this in future
static void CreateDbIfNotExists(WebApplication app)
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
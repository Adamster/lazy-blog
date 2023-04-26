using Lazy.Application.Behaviors;
using Lazy.Domain.Repositories;
using Lazy.Persistence;
using Lazy.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scrutor;
using FluentValidation;
using Lazy.App.OptionsSetup;
using Lazy.Application.Abstractions.Authorization;
using Lazy.Domain.Entities;
using Lazy.Infrastructure.Authorization;
using Lazy.Persistence.Interceptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Serilog;
using AssemblyReference = Lazy.Infrastructure.AssemblyReference;


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    builder.Services.AddScoped<IUserRepository, UserRepository>();

    builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

    builder.Host.UseSerilog();

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

    builder.Services.AddDbContext<LazyBlogDbContext>(
        (sp, optionsBuilder) => { optionsBuilder.UseSqlServer(connectionString); });

    builder.Services.AddDefaultIdentity<User>()
        .AddEntityFrameworkStores<LazyBlogDbContext>();

    builder
        .Services
        .AddControllers()
        .AddApplicationPart(Lazy.Presentation.AssemblyReference.Assembly);

    builder.Services.AddSwaggerGen();

    builder.Services.AddCors(o => o.AddPolicy("lazy-blog", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    }));

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer();

    builder.Services.ConfigureOptions<JwtOptionsSetup>();
    builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();

    var app = builder.Build();

    CreateDbIfNotExists(app);

// Configure the HTTP request pipeline.

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();

    app.UseAuthorization();

    app.UseCors("lazy-blog");
    app.MapControllers();

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
using Lazy.Application.Behaviors;
using Lazy.Domain.Repositories;
using Lazy.Persistence;
using Lazy.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scrutor;
using FluentValidation;
using Lazy.Persistence.Interceptors;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder
    .Services
    .Scan(
        selector => selector
            .FromAssemblies(
                Lazy.Infrastructure.AssemblyReference.Assembly,
                Lazy.Persistence.AssemblyReference.Assembly)
            .AddClasses(false)
            .UsingRegistrationStrategy(RegistrationStrategy.Skip)
            .AsMatchingInterface()
            .WithScopedLifetime());

builder.Services.AddMediatR(Lazy.Application.AssemblyReference.Assembly);

builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
//TODO: add after adding few commands with Domain events
//builder.Services.Decorate(typeof(INotificationHandler<>), typeof())

builder.Services.AddValidatorsFromAssembly(
    Lazy.Application.AssemblyReference.Assembly, 
    includeInternalTypes: true);

string connectionString = builder.Configuration.GetConnectionString("Database")!;

builder.Services.AddSingleton<UpdateAuditableEntitiesInterceptor>();

builder.Services.AddDbContext<LazyBlogDbContext>(
    (sp, optionsBuilder) =>
    {
        optionsBuilder.UseSqlServer(connectionString);
    });

builder
    .Services
    .AddControllers()
    .AddApplicationPart(Lazy.Presentation.AssemblyReference.Assembly);

builder.Services.AddSwaggerGen();

var app = builder.Build();

CreateDbIfNotExists(app);

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

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
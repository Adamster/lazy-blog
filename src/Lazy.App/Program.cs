using Lazy.Application.Behaviors;
using Lazy.Domain.Repositories;
using Lazy.Persistence;
using Lazy.Infrastructure;
using Lazy.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scrutor;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();

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
builder.Services.Decorate(typeof(INotificationHandler<>), typeof())

string connectionString = builder.Configuration.GetConnectionString("Database")!;

builder.Services.AddDbContext<LazyBlogDbContext>(
    (sp, optionsBuilder) =>
    {
        optionsBuilder.UseSqlServer(connectionString);
    });

builder
    .Services
    .AddControllers()
    .AddApplicationPart(AssemblyReference.Assembly);

builder.Services.AddSwaggerGen();

var app = builder.Build();

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

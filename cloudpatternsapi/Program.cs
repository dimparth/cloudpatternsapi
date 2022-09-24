using cloudpatternsapi.controllers;
using cloudpatternsapi.controllers.exceptionhandler;
using cloudpatternsapi.dependencyinjections;
using NLog;
using NLog.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using cloudpatternsapi.implementation;
using cloudpatternsapi.models;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    logger.Info($"Web Api Started");
    var builder = WebApplication.CreateBuilder(args);
    var connectionString = builder.Configuration.GetSection("ConnectionStrings");
    var conns = builder.Configuration.GetConnectionString("DefaultConnection");
    var cacheConfig = builder.Configuration.GetSection("Caching").GetChildren()
        .ToDictionary(child => child.Key, child => TimeSpan.Parse(child.Value));
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddServices(conns,cacheConfig);
    builder.Services.AddIdentityServices(builder.Configuration);
    builder.Services.AddControllers()
                .AddNewtonsoftJson()
                .AddApplicationPart(typeof(AccountController).Assembly)
                .AddApplicationPart(typeof(AdminController).Assembly)
                .AddApplicationPart(typeof(BookingController).Assembly)
                .AddApplicationPart(typeof(HallsController).Assembly)
                .AddApplicationPart(typeof(ShowController).Assembly)
                .AddApplicationPart(typeof(UsersController).Assembly)
                .AddApplicationPart(typeof(HealthcheckController).Assembly);

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    var app = builder.Build();
    using var scope = app.Services.CreateScope();

    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<BookingContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    await context.Database.MigrateAsync();
    await Seed.SeedRolesAndAdmin(userManager, roleManager);
    //app.Configuration.ConfigureServicePointManager();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseDefaultFiles();
    app.UseMiddleware<ExceptionMiddleware>();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseCors(x => x.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithOrigins("https://localhost:4200"));

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
    app.Run();
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit
    NLog.LogManager.Shutdown();
}
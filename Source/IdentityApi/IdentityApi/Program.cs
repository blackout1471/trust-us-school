using IdentityApi.Filters;
using IdentityApi.Interfaces;
using IdentityApi.Managers;
using IdentityApi.Middlewares;
using IdentityApi.Providers;
using MessageService.Configurations;
using MessageService.MessageServices;
using MessageService.Providers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;
using System.Text;

namespace IdentityApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers(options => 
            {
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                options.Filters.Add<ExceptionFilter>();
                options.Filters.Add<ModelStateFilter>();
            });

            // Turn off default model state invalid filter.
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            AddJwtConfiguration(builder);

            // Read configurations file
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                    optional: true)
                .Build();

            //Configures SMTP
            builder.Services.Configure<SMTPConfigModel>(configuration.GetSection("SMTPConfiguration"));

            // Add serilog for logging
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .WriteTo.Console()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
                {
                    AutoRegisterTemplate = true,
                    IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name?.ToLower().Replace(".", "-")}-{builder.Environment.EnvironmentName?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
                    NumberOfReplicas = 0,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                    ModifyConnectionSettings = x => x.BasicAuthentication(configuration["ElasticConfiguration:Username"], configuration["ElasticConfiguration:Password"])
                                                        .ServerCertificateValidationCallback((o, certificate, arg3, arg4) => { return true; }), // TODO: Add not self signed certificate for elasticsearch
                    TypeName = null
                })
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            builder.Host.UseSerilog();
            
            // Providers
            builder.Services.AddScoped<IUserProvider, UserProvider>();
            builder.Services.AddScoped<ILeakedPasswordProvider, LeakedPasswordProvider>();
            builder.Services.AddScoped<IMessageProvider, EmailMessageProvider>();
            builder.Services.AddScoped<IMessageService, MailMessageService>();
            // Managers
            builder.Services.AddScoped<IUserManager, UserManager>();
            builder.Services.AddScoped<ITokenManager, TokenManager>();
            builder.Services.AddScoped<IUserLocationManager, UserLocationManager>();
            builder.Services.AddScoped<IUserLocationProvider, UserLocationProvider>();
            builder.Services.AddScoped<IMessageManager, MessageManager>();


            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });


            var app = builder.Build();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors(policyBuilder =>
            {
                policyBuilder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            // Middleware to push remote ip to log context
            app.UseClientLogging();

            app.MapControllers();

            app.Run();
        }

        public static void AddJwtConfiguration(WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", //Name the security scheme
                    new OpenApiSecurityScheme
                    {
                        Description = "JWT Authorization header using the Bearer scheme.",
                        Type = SecuritySchemeType.Http, //We set the scheme type to http since we're using bearer authentication
                        Scheme = "Bearer", //The name of the HTTP Authorization scheme to be used in the Authorization header. In this case "bearer".
                        In = ParameterLocation.Header
                    });
                c.AddSecurityRequirement(
                    new OpenApiSecurityRequirement()
                    {

                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "Bearer"
                                    }
                                },
                                new string[] { }
                            }
                    });
            });

        }
    }
}
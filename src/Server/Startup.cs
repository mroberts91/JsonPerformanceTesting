using CleanArchitecture.Application;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Server.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Server
{
    public class Startup
    {
        private static readonly SymmetricSecurityKey _securityKey = new(Guid.NewGuid().ToByteArray());

        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddApplication();
            services.AddInfrastructure(Configuration, Environment);

            services.AddScoped<ICurrentUserService, CurrentUserService>();

            services.AddHttpContextAccessor();

            services.AddHealthChecks()
                    .AddDbContextCheck<ApplicationDbContext>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
                {
                    policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireClaim(ClaimTypes.Name);
                });
            });
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateAudience = false,
                            ValidateIssuer = false,
                            ValidateActor = false,
                            ValidateLifetime = true,
                            IssuerSigningKey = _securityKey
                        };
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>();
                endpoints.MapGrpcService<TodoService>();

                endpoints.MapGet("/generateJwtToken", context =>
                {
                    string name = context.Request.Query["name"];
                    string email = context.Request.Query["email"];
                    JwtSecurityTokenHandler tokenHandler = new();

                    if (string.IsNullOrEmpty(name))
                        throw new InvalidOperationException("Name is not specified.");

                    Claim[] claims = new Claim[2] { new(ClaimTypes.Name, name), new(ClaimTypes.Email, email) };
                    SigningCredentials credentials = new(_securityKey, SecurityAlgorithms.HmacSha256);
                    JwtSecurityToken token = new("ExampleServer", "ExampleClients", claims, expires: DateTime.Now.AddSeconds(60), signingCredentials: credentials);

                    return context.Response.WriteAsync(tokenHandler.WriteToken(token));
                });

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client");
                });
            });
        }
    }
}

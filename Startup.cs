using System;
using System.Text;
using System.Text.Json;
using GaryPortalAPI.Data;
using GaryPortalAPI.Hubs;
using GaryPortalAPI.Services;
using GaryPortalAPI.Services.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace GaryPortalAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation()
                .AddNewtonsoftJson(option => {

                    option.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                    option.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                });

            services.AddRazorPages().AddRazorRuntimeCompilation();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            services.AddSwaggerGen(options =>
            {
                var jwtSecurity = new OpenApiSecurityScheme
                {
                    Name = "JWT Token",
                    Description = "Enter JWT Token here, Excluding 'Bearer '",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                options.AddSecurityDefinition(jwtSecurity.Reference.Id, jwtSecurity);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurity, Array.Empty<string>() }
                });
            });

            services.AddSignalR().AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            services.AddDbContext<AppDbContext>(o =>
            {
                o.UseMySql(
                        Configuration["AppSettings:Connection"],
                        new MySqlServerVersion(new Version(8, 0, 21)),
                        mySqlOptions => mySqlOptions
                            .CharSetBehavior(CharSetBehavior.NeverAppend))
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors();
            });

            services
                .AddAuthentication(options => options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = Configuration["AppSettings:Issuer"],
                        ValidAudience = Configuration["AppSettings:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["AppSettings:Secret"])),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.Configure<ApiSettings>(Configuration.GetSection("AppSettings"));
            services.AddHttpContextAccessor();

            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IFeedService, FeedService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IStaffService, StaffService>();
            services.AddScoped<IChatBotService, ChatBotService>();
            services.AddScoped<IAppService, AppService>();
            services.AddScoped<IGameTypeService, GameTypeService>();
            services.AddTransient<ICDNService, CDNService>();

            services.AddTransient<IHashingService, HashingService>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IEmailService, EmailService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gary Portal API v1");
                });
            }

            app.UseCors("CorsPolicy");
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("/chathub");
                endpoints.MapHub<GaryPortalHub>("/apphub");
                endpoints.MapHub<GameHub>("/gamehub");
            });
        }
    }
}

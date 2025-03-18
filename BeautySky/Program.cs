using Amazon;
using Amazon.S3;
using BeautySky.Models;
using BeautySky.Service;
using BeautySky.Service.Vnpay;
using BeautySky.Services.Vnpay;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Security.Claims;
using System.Text;

namespace BeautySky
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn session
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Thêm IHttpContextAccessor để sử dụng session trong controller
            builder.Services.AddHttpContextAccessor();

            // Add services to the container.
            builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = false;
            });

            builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    return new BadRequestObjectResult(context.ModelState);
                };
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("v1", new OpenApiInfo { Title = "BeautySky", Version = "v1" });
                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
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



            builder.Services.AddDbContext<ProjectSwpContext>(option =>
            {
                option.UseSqlServer(builder.Configuration.GetConnectionString("MyDBConnection"));
            });


            // Authentication configuration
            builder.Services.AddAuthentication(option =>
            {
                option.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            }).AddCookie().AddJwtBearer(option =>
            {
                option.SaveToken = true;
                option.RequireHttpsMetadata = false;
                option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))
                };
            }).AddGoogle(option =>
            {
                option.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                option.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
                option.CallbackPath = "/signin-google";
                //option.CallbackPath = "/google-respone";
                option.Events.OnCreatingTicket = async context =>
                {
                    var dbContext = context.HttpContext.RequestServices.GetService<ProjectSwpContext>();

                    var email = context.Principal.FindFirstValue(ClaimTypes.Email);

                    var name = context.Principal.FindFirstValue(ClaimTypes.Name);

                    var existingUser = dbContext.Users.FirstOrDefault(u => u.Email == email);

                    if (existingUser == null)
                    {
                        var newUser = new User
                        {
                            UserName = email.Split('@')[0],
                            FullName = name,
                            Email = email,
                            Password = "GOOGLE_LOGIN",
                            ConfirmPassword = "GOOGLE_LOGIN",
                            Phone = "Chưa cập nhật",
                            Address = "Chưa cập nhật",
                            RoleId = 1,
                            DateCreate = DateTime.UtcNow,
                            IsActive = true
                        };
                        dbContext.Users.Add(newUser);
                        await dbContext.SaveChangesAsync();
                    }
                };

            });

            builder.Services.AddSingleton<IAmazonS3>(option =>
            {
                var configuration = option.GetRequiredService<IConfiguration>();
                return new AmazonS3Client(
                    configuration["AWS:AccessKey"],
                    configuration["AWS:SecretKey"],
                    RegionEndpoint.GetBySystemName(configuration["AWS:Region"])
                );
            });

            builder.Services.AddSingleton<S3Service>();


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy => policy.AllowAnyOrigin()
                                    .AllowAnyMethod()
                                    .AllowAnyHeader());
            });

            builder.Services.AddScoped<IVnPayService, VnPayService>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseCors("AllowAll");
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseSession();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
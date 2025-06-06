using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoleKingECommerce.Configurations;
using SoleKingECommerce.Data;
using SoleKingECommerce.Models;
using SoleKingECommerce.Repositories.Implementations;
using SoleKingECommerce.Repositories.Interfaces;
using SoleKingECommerce.Services.Implementations;
using SoleKingECommerce.Services.Interfaces;
using System.Text.Json.Serialization;

namespace SoleKingECommerce
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            // Dành cho API
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                 {
                     options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                 });
            // Dành cho MVC
            builder.Services.AddControllersWithViews();

            //DbContext
            builder.Services.AddDbContext<SoleKingDbContext>(options =>
            {
                string connectionString = builder.Configuration.GetConnectionString("StrConnection")!;
                //options.UseSqlServer(connectionString);
                options.UseSqlServer(connectionString);
                Console.WriteLine($"Connection string: '{connectionString}'");
            });

            // Repositories & Services
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
            builder.Services.AddScoped<IImportRepository, ImportRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IColorRepository, ColorRepository>();
            builder.Services.AddScoped<IProductVariantRepository, ProductVariantRepository>();

            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IColorService, ColorService>();
            builder.Services.AddScoped<ISupplierService, SupplierService>();
            builder.Services.AddScoped<IImportService, ImportService>();

            //Identity
            builder.Services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<SoleKingDbContext>()
                .AddDefaultTokenProviders();

            // Truy cập IdentityOptions
            builder.Services.Configure<IdentityOptions>(options =>
            {
                //Thiết lập cho password
                options.Password.RequireDigit = false; // ko cần số
                options.Password.RequireLowercase = false; // ko cần chữ thường
                options.Password.RequireNonAlphanumeric = false; // ko cần ký tự đặc biệt
                options.Password.RequireUppercase = false; // ko cần chữ hoa
                options.Password.RequiredLength = 6; // độ dài tối thiểu
                options.Password.RequiredUniqueChars = 1; // số ký tự khác nhau tối thiểu

                // Cấu hình Lockout - khóa user
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
                options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 3 lần thì khóa
                options.Lockout.AllowedForNewUsers = true;

                // Cấu hình về User.
                options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;  // Email là duy nhất


                // Cấu hình đăng nhập.
                options.SignIn.RequireConfirmedEmail = true;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
                options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
                options.SignIn.RequireConfirmedAccount = true;
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
            });

            //Dịch vụ send mail
            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
            builder.Services.AddTransient<IEmailSenderService, SendMailService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}

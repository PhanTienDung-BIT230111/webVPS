using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            Console.WriteLine(">>> Inside SeedData.Initialize");

            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            // --- Seed Categories ---
            if (!context.Categories.Any())
            {
                Console.WriteLine(">>> Seeding Categories...");
                context.Categories.AddRange(
                    new Category { Name = "Trái cây", Description = "Trái cây tươi" },
                    new Category { Name = "Rau củ", Description = "Rau củ sạch" },
                    new Category { Name = "Hải sản", Description = "Hải sản tươi sống" },
                    new Category { Name = "Thịt", Description = "Thịt sạch" },
                    new Category { Name = "Đồ khô", Description = "Ngũ cốc, đậu" },
                    new Category { Name = "Đồ uống", Description = "Nước trái cây" },
                    new Category { Name = "Sữa", Description = "Sữa các loại" },
                    new Category { Name = "Đồ hộp", Description = "Đồ ăn nhanh" },
                    new Category { Name = "Gia vị", Description = "Gia vị nấu ăn" },
                    new Category { Name = "Khác", Description = "Danh mục khác" }
                );
                await context.SaveChangesAsync();
                Console.WriteLine(">>> Categories seeded");
            }

            // --- Seed Products ---
            
                Console.WriteLine(">>> Seeding Products...");

                // Lấy category đầu tiên làm ví dụ
                var firstCategory = context.Categories.FirstOrDefault();
                if (firstCategory != null)
                {
                    var products = new List<Product>
        {
            new Product {
                Name = "Mì Hảo Hảo", Description = "Mì ăn liền vị tôm chua cay",
                Price = 3500, Stock = 100, CategoryId = firstCategory.Id,
                ImageUrl = "https://acecookvietnam.vn/wp-content/uploads/2017/07/BAG-HAO-HAO-TCC.png"
            },
            new Product {
                Name = "Cá Hồi Nauy", Description = "Cá hồi tươi nhập khẩu từ Nauy",
                Price = 120000, Stock = 25, CategoryId = firstCategory.Id,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSw3PaofdjR_-7uAj_rXIDezhgDSR5ejwsVWw&s"
            },
            new Product {
                Name = "Sữa Tươi Vinamilk", Description = "Sữa tươi tiệt trùng không đường",
                Price = 26000, Stock = 40, CategoryId = firstCategory.Id,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSn5AcwFIg4eiY6Fv562svgmeYtSpk-4-Za_A&s"
            },
            new Product {
                Name = "Trứng Gà Ta", Description = "Trứng gà ta sạch, giàu dinh dưỡng",
                Price = 25000, Stock = 60, CategoryId = firstCategory.Id,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSQjE0lXVjoZEvOwJyu0m8AifsBsD15GHisiw&s"
            },
            new Product {
                Name = "Gạo ST25", Description = "Gạo thơm ngon nhất thế giới",
                Price = 18000, Stock = 50, CategoryId = firstCategory.Id,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSdMBaxGDQW6oehkfIqwdlf6pMBUBKrVVBIkQ&s"
            },
            new Product {
                Name = "Bánh Oreo", Description = "Bánh quy socola nhân kem vani",
                Price = 12000, Stock = 80, CategoryId = firstCategory.Id,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS2mlYHBkd5qq-McTtIDc70WP0vTCKTMauqdw&s"
            },
            new Product {
                Name = "Kem Wall's", Description = "Kem tươi vị dâu thơm ngon",
                Price = 15000, Stock = 30, CategoryId = firstCategory.Id,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRlhnXSwU9AKY9sq_8Xfj0HQaxs7JuXF5GQTA&s"
            },
            new Product {
                Name = "Nước Ngọt Pepsi", Description = "Nước giải khát có gas hương cola",
                Price = 10000, Stock = 90, CategoryId = firstCategory.Id,
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQSrbPgrWUn4vn21_Aq4Xy8Vdj8Qo3f2WLEBg&s"
            }
        };

                    context.Products.AddRange(products);
                    await context.SaveChangesAsync();

                    Console.WriteLine(">>> Products seeded");
                }
            


            // --- Seed Roles & Admin ---
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            string[] roles = { "Admin", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                    Console.WriteLine($">>> Role {role} created");
                }
            }

            var adminEmail = "phamhieuhp050505@gmail.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var newAdmin = new User
                {
                    UserName = "admin_phamhieu",
                    Email = adminEmail,
                    FullName = "Phạm Xuân Hiếu (Admin)",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(newAdmin, "05052005");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                    Console.WriteLine(">>> Admin user created and assigned to Admin role");
                }
            }
        }
    }
}

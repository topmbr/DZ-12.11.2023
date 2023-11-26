using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
namespace ConsoleApp83
{
    public class Company
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<User> Users { get; set; } = new();
    }

    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
        public int CompanyId { get; set; }
        public Company? Company { get; set; }
    }

    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Company> Companies { get; set; } = null!;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = @"Data Source=DESKTOP-2J3MN6S; Initial Catalog=DNDX; Trusted_Connection=True; TrustServerCertificate=True";
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    class Program
    {
        static void Main()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                // пересоздаем базу данных
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                Company microsoft = new Company { Name = "Microsoft" };
                Company google = new Company { Name = "Google" };

                db.Companies.AddRange(microsoft, google);

                User tan = new User { Name = "Tan", Age = 36, Company = microsoft };
                User vasya = new User { Name = "Vasya", Age = 39, Company = google };
                User alice = new User { Name = "Alice", Age = 28, Company = microsoft };
                User kate = new User { Name = "Kate", Age = 25, Company = google };

                db.Users.AddRange(tan, vasya, alice, kate);
                db.SaveChanges();
            }

            using (ApplicationContext db = new ApplicationContext())
            {
                // Пример запроса с использованием LINQ и Include для загрузки связанных данных
                var users = db.Users.Include(u => u.Company).Where(u => u.CompanyId == 1).ToList();

                foreach (var user in users)
                {
                    Console.WriteLine($"{user.Name} ({user.Age}) - {user.Company?.Name}");
                }
            }

            using (ApplicationContext db = new ApplicationContext())
            {
                var users = db.Users.Include(p => p.Company).Where(p => p.CompanyId == 1);
            }
            // Если необходимо отфильтровать получаемые данные,
            // то для этого можно использовать метод Where
            using (ApplicationContext db = new ApplicationContext())
            {
                var users = db.Users.Where(p => p.Company!.Name == "Google");
                foreach (User user in users) Console.WriteLine($"{user.Name} ({user.Age})");
            }
            // Аналогичный запос с помощью операторов LINQ
            using (ApplicationContext db = new ApplicationContext())
            {
                var users = (from user in db.Users where user.Company!.Name == "Google" select user).ToList();
                foreach (User user in users) Console.WriteLine($"{user.Name} ({user.Age})");
            }

            // EF. Functions.Like() можно задать условие запроса,
            // которое транслируется в Entity Framework Core в выражение с оператором LIKE
            using (ApplicationContext db = new ApplicationContext())
            {
                var users = db.Users.Where(p => EF.Functions.Like(p.Name!, "%Tom")); foreach (User user in users) Console.WriteLine($"{user.Name}({user.Age})");
            }
            // Для объединения таблиц по определенному критерию в Entity Framework Core // используется метод Join
            using (ApplicationContext db = new ApplicationContext())
            {
                var users = db.Users.Join(db.Companies, // второй набор
                u => u.CompanyId, // свойство-селектор объекта из первого набора
                c => c.Id, // свойство-селектор объекта из второго набора
                (u, c) => new // результат
                {
                    Name = u.Name,
                    Company = c.Name,
                    Age = u.Age
                });
                foreach (var u in users) Console.WriteLine($"{u.Name} ({u.Company}) - {u.Age}");
            }

            Console.ReadLine();
        }
    }

}
using System;
using System.Data.Entity;
using Blog1.DataAccess.Migrations;

namespace Blog1.DataAccess
{
    class Program
    {
        static void Main(string[] args)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<BlogContext, Configuration>());

            using (var db = new BlogContext())
            {
                db.Blogs.Add(new Blog {Name = "Another Blog"});
                db.SaveChanges();

                foreach (var blog in db.Blogs)
                {
                    Console.WriteLine(blog.Name);
                }
            }
        }
    }
}

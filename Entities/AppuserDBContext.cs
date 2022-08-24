using System;
using Microsoft.EntityFrameworkCore;
using WebService.DTO;

namespace WebService.Entities
{
    public class AppuserDBContext : DbContext
    {
        public AppuserDBContext()
        {
        }


        public AppuserDBContext(DbContextOptions<AppuserDBContext> options)
            : base(options)
        {
        }



        public virtual DbSet<Operator> Operators { get; set; }

        public virtual DbSet<User_Token> Token { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<Operator>(entity =>
            {
                entity.ToTable("operator");
                entity.HasNoKey();

            });

            modelBuilder.Entity<User_Token>(entity =>
            {
                entity.ToTable("user_token");
                entity.HasKey(e => e.device);

            });
         
        }

       
    }

}

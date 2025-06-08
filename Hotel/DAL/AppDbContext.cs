using Hotel.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hotel.DAL
{
    public class AppDbContext: IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        { 
        }
       
        public DbSet<Room> Rooms { get; set; }
        public DbSet<BookingRoom> BookingRoom { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Employee> Employers { get; set; }
        public DbSet<BookingStatus> BookingStatus { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<Positions> Positions { get; set; }
        public DbSet<Budget> Budgets { get; set; } 
        public DbSet<Benefit> Benefits { get; set; }
        public DbSet<Cost> Costs { get; set; }
       
       
    }
}

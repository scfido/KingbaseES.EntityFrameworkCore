using Kdbndp;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace kingbase_demo
{

    public enum State
    {
        None,
        有用,
        无用
    }

    public class Modules : DbContext
    {
        public DbSet<Blog_Test> Kdb_Blog_Tests => Set<Blog_Test>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseKdbndp("Server=localhost;UserName=ROOT;Password=123456;Database=BookStore;Port=54321;",
                options =>
                {
                });

            //KdbndpConnection conn = new()
            //{
            //    ConnectionString = "Server=localhost;UserName=ROOT;Password=123456;Database=BookStore;Port=54321;",

            //};

            //optionsBuilder.UseKdbndp(conn);
        }

    }

    public class Blog_Test
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? Ids { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool Sex { get; set; }

        public bool? Sexy { get; set; }

        public int Age { get; set; }

        public int? Ager { get; set; }

        public DateTime Birth { get; set; }

        public DateTime? Birthy { get; set; }

        public float Money { get; set; }

        public float? Moneies { get; set; }

        public double Pi { get; set; }

        public double? Pis { get; set; }

        public State State { get; set; }

        public State? States { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}

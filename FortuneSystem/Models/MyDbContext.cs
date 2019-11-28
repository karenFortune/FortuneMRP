using FortuneSystem.Models.Arte;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;

namespace FortuneSystem.Models
{
    public class MyDbContext : DbContext
    {
        public MyDbContext() : base("Fortune")
		{

        }

		/*protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			throw new UnintentionalCodeFirstException();
		}*/

		public DbSet<IMAGEN_ARTE> ImagenArte { get; set; }
        public DbSet<IMAGEN_ARTE_PNL> ImagenArtePnl { get; set; }
		public DbSet<IMAGEN_ARTE_ESTILO> ImagenArteEstilo { get; set; }
        public DbSet<ARTE> Arte { get; set; }
		public DbSet<UPC> UPCs { get; set; }


	}
}
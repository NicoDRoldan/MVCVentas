using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MVCVentas.Models;

namespace MVCVentas.Data
{
    public class MVCVentasContext : DbContext
    {
        public MVCVentasContext (DbContextOptions<MVCVentasContext> options)
            : base(options)
        {
        }

        public DbSet<MVCVentas.Models.VMUser> VMUser { get; set; } = default!;

        public DbSet<MVCVentas.Models.VMCategory> VMCategory { get; set; } = default!;

        public DbSet<MVCVentas.Models.VMArticle> VMArticle { get; set; }

        public DbSet<MVCVentas.Models.VMRubro> VMRubro { get; set; }

        public DbSet<MVCVentas.Models.VMPrice> VMPrice { get; set; }

        public DbSet<MVCVentas.Models.VMStock> VMStock { get; set; }

        public DbSet<MVCVentas.Models.VMPromoDescuento_E> VMPromoDescuento_E { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VMPromoDescuento_D>()
                .HasKey(pd => new { pd.Id_Promocion, pd.Id_Articulo });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<MVCVentas.Models.VMPromoDescuento_D> VMPromoDescuento_D { get; set; }

        public DbSet<MVCVentas.Models.VMTipoPromoDescuento> VMTipoPromoDescuento { get; set; }

        public DbSet<MVCVentas.Models.VMCliente> VMCliente { get; set; }

        public DbSet<MVCVentas.Models.VMFormaPago> VMFormaPago { get; set; }

        public DbSet<MVCVentas.Models.VMModulo> VMModulo { get; set; }

        public DbSet<MVCVentas.Models.VMConcepto> VMConcepto { get; set; }

        public DbSet<MVCVentas.Models.VMComprobante_E> VMComprobante_E { get; set; }
    }
}

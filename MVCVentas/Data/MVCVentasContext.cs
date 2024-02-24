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

        public DbSet<MVCVentas.Models.VMPromoDescuento_D> VMPromoDescuento_D { get; set; }

        public DbSet<MVCVentas.Models.VMTipoPromoDescuento> VMTipoPromoDescuento { get; set; }

        public DbSet<MVCVentas.Models.VMCliente> VMCliente { get; set; }

        public DbSet<MVCVentas.Models.VMFormaPago> VMFormaPago { get; set; }

        public DbSet<MVCVentas.Models.VMModulo> VMModulo { get; set; }

        public DbSet<MVCVentas.Models.VMConcepto> VMConcepto { get; set; }

        public DbSet<MVCVentas.Models.VMComprobante_E> VMComprobante_E { get; set; }

        public DbSet<MVCVentas.Models.VMSucursal> VMSucursal { get; set; }

        public DbSet<MVCVentas.Models.VMTipoFactura> VMTipoFactura { get; set; }

        public DbSet<MVCVentas.Models.VMComprobante_N> VMComprobante_N { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VMPromoDescuento_D>()
                .HasKey(pd => new { pd.Id_Promocion, pd.Id_Articulo });

            modelBuilder.Entity<VMComprobante_N>()
                .HasKey(cn => new { cn.CodComprobante, cn.CodModulo, cn.NumSucursal });

            modelBuilder.Entity<VMComprobante_E>()
                .HasKey(ce => new { ce.CodComprobante, ce.CodModulo });

            modelBuilder.Entity<VMVentas_E>()
                .HasKey(ve => new { ve.NumVenta, ve.CodComprobante, ve.CodModulo, ve.NumSucursal });

            modelBuilder.Entity<VMVentas_D>()
                .HasKey(vd => new { vd.NumVenta, vd.CodComprobante, vd.CodModulo, vd.NumSucursal, vd.Renglon });

            modelBuilder.Entity<VMVentas_I>()
                .HasKey(vi => new { vi.NumVenta, vi.CodComprobante, vi.CodModulo, vi.NumSucursal, vi.CodConcepto });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<MVCVentas.Models.VMVentas_E> VMVentas_E { get; set; }

        public DbSet<MVCVentas.Models.VMVentas_D> VMVentas_D { get; set; }

        public DbSet<MVCVentas.Models.VMVentas_I> VMVentas_I { get; set; }

        public DbSet<MVCVentas.Models.VMConfig> VMConfig { get; set; }

        public DbSet<MVCVentas.Models.VMCombo> VMCombo { get; set; }
    }
}

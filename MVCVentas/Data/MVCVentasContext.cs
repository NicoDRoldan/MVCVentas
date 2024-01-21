﻿using System;
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
    }
}

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Models;

namespace RestaurantAPI.Data
{
    public class RestaurantDbContext : IdentityDbContext<Usuario>
    {
        public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options)
        {
        }

        public DbSet<Mesa> Mesas { get; set; }
        public DbSet<CategoriaProducto> CategoriasProducto { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Ingrediente> Ingredientes { get; set; }
        public DbSet<ProductoIngrediente> ProductoIngredientes { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallesPedido { get; set; }
        public DbSet<MedioPago> MediosPago { get; set; }
        public DbSet<Pago> Pagos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Mesa
            modelBuilder.Entity<Mesa>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CodigoMesa).IsUnique();
                entity.Property(e => e.NumeroMesa).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CodigoMesa).IsRequired().HasMaxLength(8);
            });

            // Configuración de CategoriaProducto
            modelBuilder.Entity<CategoriaProducto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            });

            // Configuración de Producto
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Precio).HasPrecision(10, 2);

                entity.HasOne(e => e.Categoria)
                    .WithMany(c => c.Productos)
                    .HasForeignKey(e => e.CategoriaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de Ingrediente
            modelBuilder.Entity<Ingrediente>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            });

            // Configuración de ProductoIngrediente
            modelBuilder.Entity<ProductoIngrediente>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Producto)
                    .WithMany(p => p.ProductoIngredientes)
                    .HasForeignKey(e => e.ProductoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Ingrediente)
                    .WithMany(i => i.ProductoIngredientes)
                    .HasForeignKey(e => e.IngredienteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Pedido
            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.NumeroPedido).IsUnique();
                entity.Property(e => e.NumeroPedido).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Total).HasPrecision(10, 2);

                entity.HasOne(e => e.Mesa)
                    .WithMany(m => m.Pedidos)
                    .HasForeignKey(e => e.MesaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.UsuarioCreador)
                    .WithMany(u => u.PedidosCreados)
                    .HasForeignKey(e => e.UsuarioCreadorId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuración de DetallePedido
            modelBuilder.Entity<DetallePedido>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PrecioUnitario).HasPrecision(10, 2);
                entity.Property(e => e.Subtotal).HasPrecision(10, 2);

                entity.HasOne(e => e.Pedido)
                    .WithMany(p => p.DetallesPedido)
                    .HasForeignKey(e => e.PedidoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Producto)
                    .WithMany(p => p.DetallesPedido)
                    .HasForeignKey(e => e.ProductoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de MedioPago
            modelBuilder.Entity<MedioPago>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            });

            // Configuración de Pago
            modelBuilder.Entity<Pago>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Monto).HasPrecision(10, 2);

                entity.HasOne(e => e.Pedido)
                    .WithOne(p => p.Pago)
                    .HasForeignKey<Pago>(e => e.PedidoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.MedioPago)
                    .WithMany(m => m.Pagos)
                    .HasForeignKey(e => e.MedioPagoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.UsuarioCajero)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioCajeroId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configuración de Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasIndex(e => e.CodigoEmpleado).IsUnique();
            });
        }
    }
}
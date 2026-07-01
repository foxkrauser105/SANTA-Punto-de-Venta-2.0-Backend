using Microsoft.EntityFrameworkCore;
using SANTA.PoS.Domain.Entities;

namespace SANTA.PoS.Data.Persistence;

public partial class SantaContext : DbContext
{
    public SantaContext()
    {
    }

    public SantaContext(DbContextOptions<SantaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Descuento> Descuentos { get; set; }

    public virtual DbSet<NotasCredito> NotasCreditos { get; set; }

    public virtual DbSet<Notificacione> Notificaciones { get; set; }

    public virtual DbSet<PagosNotasCredito> PagosNotasCreditos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<RegistroNotasCredito> RegistroNotasCreditos { get; set; }

    public virtual DbSet<RegistroVenta> RegistroVentas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<VentaDia> VentaDia { get; set; }

    public virtual DbSet<Venta> Venta { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<RequisicionItem> RequisicionItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Modern_Spanish_CI_AS");

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Numcliente);

            entity.ToTable("clientes");

            entity.Property(e => e.Numcliente)
                .ValueGeneratedNever()
                .HasColumnName("numcliente");
            entity.Property(e => e.AMaterno)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("aMaterno");
            entity.Property(e => e.APaterno)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("aPaterno");
            entity.Property(e => e.Calle)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("calle");
            entity.Property(e => e.Colonia)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("colonia");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.NumeroExt).HasColumnName("numeroExt");
            entity.Property(e => e.NumeroInt).HasColumnName("numeroInt");
            entity.Property(e => e.Telefono)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("telefono");
            entity.Property(e => e.UsuclaveUltAct)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("usuclaveUltAct");

            entity.HasOne(d => d.UsuclaveUltActNavigation).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.UsuclaveUltAct)
                .HasConstraintName("FK_clientes_usuarios");
        });

        modelBuilder.Entity<Descuento>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("descuentos");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");

            entity.Property(e => e.ProductoId).HasColumnName("id_producto");
            entity.Property(e => e.CantidadMinima).HasColumnName("cantidadMinima");
            entity.Property(e => e.PrecioDescuento)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("precioDescuento");
            entity.Property(e => e.Status)
                .HasDefaultValue(1, "DF_descuentos_status")
                .HasColumnName("status");

            entity.HasOne(d => d.Producto).WithOne(p => p.Descuento)
                .HasForeignKey<Descuento>(d => d.ProductoId)
                .HasConstraintName("FK_descuentos_productos");
        });

        modelBuilder.Entity<NotasCredito>(entity =>
        {
            entity.HasKey(e => new { e.Numcliente, e.Ncfolio }).HasName("PK__notas_cr__38F1A0E355A198F5");

            entity.ToTable("notas_credito");

            entity.Property(e => e.Numcliente).HasColumnName("numcliente");
            entity.Property(e => e.Ncfolio).HasColumnName("ncfolio");
            entity.Property(e => e.FechaAlta)
                .HasColumnType("datetime")
                .HasColumnName("fechaAlta");
            entity.Property(e => e.FechaCompromiso).HasColumnName("fechaCompromiso");
            entity.Property(e => e.Monto)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("monto");
            entity.Property(e => e.MontoPagado)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("montoPagado");
            entity.Property(e => e.Status)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("status");
        });

        modelBuilder.Entity<Notificacione>(entity =>
        {
            entity.HasKey(e => e.IdMensaje);

            entity.ToTable("notificaciones");

            entity.Property(e => e.IdMensaje).HasColumnName("id_mensaje");
            entity.Property(e => e.FechaAlta)
                .HasColumnType("datetime")
                .HasColumnName("fecha_alta");
            entity.Property(e => e.FechaTermino)
                .HasColumnType("datetime")
                .HasColumnName("fecha_termino");
            entity.Property(e => e.IdProducto)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("id_producto");
            entity.Property(e => e.Mensaje).HasColumnName("mensaje");
            entity.Property(e => e.Prioridad)
                .HasMaxLength(50)
                .HasColumnName("prioridad");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .HasColumnName("tipo");
            entity.Property(e => e.Usuclave)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("usuclave");
            entity.Property(e => e.UsuclaveRecibe)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("usuclave_recibe");
        });

        modelBuilder.Entity<PagosNotasCredito>(entity =>
        {
            entity.HasKey(e => new { e.Numcliente, e.Ncfolio, e.Pago }).HasName("PK__pagos_notas_credito");

            entity.ToTable("pagos_notas_credito");

            entity.Property(e => e.Numcliente).HasColumnName("numcliente");
            entity.Property(e => e.Ncfolio).HasColumnName("ncfolio");
            entity.Property(e => e.Pago).HasColumnName("pago");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())", "DF_pagos_notas_credito_fecha")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.Importe)
                .HasColumnType("decimal(7, 2)")
                .HasColumnName("importe");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("productos");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");

            entity.Property(e => e.IdProducto)
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired()
                .HasColumnName("id_producto");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.Categoria)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("categoria");
            entity.Property(e => e.Fechaultact)
                .HasDefaultValueSql("(getdate())", "DF_productos_fechaultact")
                .HasColumnType("datetime")
                .HasColumnName("fechaultact");
            entity.Property(e => e.Marca)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("marca");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("precio");
            entity.Property(e => e.Status)
                .HasDefaultValue(1, "DF_productos_status")
                .HasColumnName("status");
        });

        modelBuilder.Entity<RegistroNotasCredito>(entity =>
        {
            entity.HasKey(e => new { e.Numcliente, e.Ncfolio, e.Detalle }).HasName("PK__registro__3363B4A14AFE0FD6");

            entity.ToTable("registro_notas_credito");

            entity.Property(e => e.Numcliente).HasColumnName("numcliente");
            entity.Property(e => e.Ncfolio).HasColumnName("ncfolio");
            entity.Property(e => e.Detalle).HasColumnName("detalle");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.Descuento).HasColumnName("descuento");
            entity.Property(e => e.FechaSurtido)
                .HasDefaultValueSql("(getdate())", "DF_registro_notas_credito_fechaSurtido")
                .HasColumnType("datetime")
                .HasColumnName("fechaSurtido");
            entity.Property(e => e.ProductoId)
                .HasColumnName("id_producto");
            entity.Property(e => e.Importe)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("importe");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("precio");

            entity.HasOne(d => d.Producto).WithMany(p => p.RegistroNotasCreditos)
                .HasForeignKey(d => d.ProductoId)
                .HasConstraintName("FK_registro_notas_credito_productos");
        });

        modelBuilder.Entity<RegistroVenta>(entity =>
        {
            entity.HasKey(e => e.IdRegistro);

            entity.ToTable("registro_ventas");

            entity.Property(e => e.IdRegistro).HasColumnName("id_registro");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.Descuento).HasColumnName("descuento");
            entity.Property(e => e.ProductoId)
                .HasColumnName("id_producto");
            entity.Property(e => e.IdVenta).HasColumnName("id_venta");
            entity.Property(e => e.Ncfolio).HasColumnName("ncfolio");
            entity.Property(e => e.Numcliente).HasColumnName("numcliente");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("precio");

            entity.HasOne(d => d.Producto).WithMany(p => p.RegistroVenta)
                .HasForeignKey(d => d.ProductoId)
                .HasConstraintName("FK_registro_ventas_productos");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.RegistroVentas)
                .HasForeignKey(d => d.IdVenta)
                .HasConstraintName("FK_registro_ventas_venta");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Usuclave);

            entity.ToTable("usuarios");

            entity.Property(e => e.Usuclave)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("usuclave");
            entity.Property(e => e.AMaterno)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("aMaterno");
            entity.Property(e => e.APaterno)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("aPaterno");
            entity.Property(e => e.FechaAlta)
                .HasDefaultValueSql("(getdate())", "DF_usuarios_fechaAlta")
                .HasColumnType("datetime")
                .HasColumnName("fechaAlta");
            entity.Property(e => e.FechaUltAct)
                .HasDefaultValueSql("(getdate())", "DF_usuarios_fechaUltAct")
                .HasColumnType("datetime")
                .HasColumnName("fechaUltAct");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Pass).HasColumnName("pass");
            entity.Property(e => e.Status)
                .HasDefaultValue(1, "DF_usuarios_status")
                .HasColumnName("status");
            entity.Property(e => e.Telefono)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("telefono");
            entity.Property(e => e.Rol)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("User")
                .HasColumnName("rol");
        });

        modelBuilder.Entity<VentaDia>(entity =>
        {
            entity.HasKey(e => e.IdVentadia);

            entity.ToTable("venta_dia");

            entity.Property(e => e.IdVentadia).HasColumnName("id_ventadia");
            entity.Property(e => e.Caja)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("caja");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.Final)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("final");
            entity.Property(e => e.Gasto)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("gasto");
            entity.Property(e => e.Inicio)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("inicio");
            entity.Property(e => e.Monedas)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("monedas");
            entity.Property(e => e.Proveedores)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("proveedores");
            entity.Property(e => e.Quedo)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("quedo");
            entity.Property(e => e.SaldoFinal)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("saldo_final");
            entity.Property(e => e.SaldoInicial)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("saldo_inicial");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("total");
            entity.Property(e => e.UsoMonedas)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("uso_monedas");
            entity.Property(e => e.VentaAbarrote)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("venta_abarrote");
            entity.Property(e => e.VentaSaldo)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("venta_saldo");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.IdVenta);

            entity.ToTable("venta");

            entity.Property(e => e.IdVenta).HasColumnName("id_venta");
            entity.Property(e => e.Fecha).HasColumnName("fecha");
            entity.Property(e => e.ImporteVenta)
                .HasColumnType("decimal(16, 2)")
                .HasColumnName("venta");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.Nombre);

            entity.ToTable("categorias");

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<RequisicionItem>(entity =>
        {
            entity.HasKey(e => new { e.Usuclave, e.ProductoId });

            entity.ToTable("requisicion");

            entity.Property(e => e.Usuclave)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("usuclave");
            entity.Property(e => e.ProductoId).HasColumnName("id_producto");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");

            entity.HasOne(d => d.Producto).WithMany()
                .HasForeignKey(d => d.ProductoId)
                .HasConstraintName("FK_requisicion_productos");

            entity.HasOne(d => d.Usuario).WithMany()
                .HasForeignKey(d => d.Usuclave)
                .HasConstraintName("FK_requisicion_usuarios");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

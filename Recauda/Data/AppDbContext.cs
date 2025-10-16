using Microsoft.EntityFrameworkCore;
using Recauda.Models;


namespace Recauda.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }        
        public DbSet<Recaudador> Recaudadores { get; set; }
        public DbSet<MotivoDeCobro> MotivoDeCobros { get; set; }
        public DbSet<Persona> Personas { get; set; }
        public DbSet<Compania> Companias { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Contribuyente> Contribuyentes { get; set; }
        public DbSet<Voluntarios> Voluntarios { get; set; }
        public DbSet<Generadores> Generadores { get; set; }
        public DbSet<Cobros> Cobros { get; set; }
        public DbSet<Anulaciones> Anulaciones { get; set; }
        public DbSet<CobrosAnulados> CobrosAnulados { get; set; }
        public DbSet<Pagos> Pagos { get; set; }
        public DbSet<PagosAnulados> PagosAnulados { get; set; }
        public DbSet<FormasDeRecaudacion> FormasDeRecaudacion { get; set; }
        public DbSet<ComprobantesPago> ComprobantesPago { get; set; }
        // Vistas
        public DbSet<VRecaudador> VRecaudadores { get; set; }
        public DbSet<VContribuyente> VContribuyentes { get; set; }
        public DbSet<VCobros> VCobros { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<VCobros>(entity =>
            {
                entity.ToView("VCobros"); 
                entity.HasKey(e => e.Id);
            });
            // Vista "Vrecaudadores"
            modelBuilder.Entity<VRecaudador>(entity =>
            {
                entity.ToView("Vrecaudadores");
                entity.HasKey(e => e.Id);
            });


            // ==========================================
            // CONFIGURACIÓN DE COMPROBANTES DE PAGO
            // ==========================================
            modelBuilder.Entity<ComprobantesPago>(entity =>
            {
                entity.ToTable("ComprobantesPago");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.comp_ruta_archivo)
                      .IsRequired()
                      .HasMaxLength(500)
                      .HasColumnType("nvarchar(500)");

                entity.Property(e => e.comp_nombre_original)
                      .IsRequired()
                      .HasMaxLength(255)
                      .HasColumnType("nvarchar(255)");

                entity.Property(e => e.comp_extension)
                      .HasMaxLength(10)
                      .HasColumnType("nvarchar(10)");

                entity.Property(e => e.comp_tamaño_kb)
                      .HasColumnType("decimal(10,2)");

                entity.Property(e => e.comp_tipo_comprobante)
                      .HasMaxLength(50)
                      .HasColumnType("nvarchar(50)");

                entity.Property(e => e.comp_descripcion)
                      .HasMaxLength(1000)
                      .HasColumnType("nvarchar(1000)");

                entity.Property(e => e.comp_activo)
                      .IsRequired()
                      .HasDefaultValue(true);

                entity.Property(e => e.FechaRegistro)
                      .HasColumnType("datetime")
                      .IsRequired();

                entity.Property(e => e.FechaModificacion)
                      .HasColumnType("datetime");

                // Relación ComprobantesPago -> Pagos
                entity.HasOne(cp => cp.Pago)
                      .WithMany(p => p.ComprobantesPago)
                      .HasForeignKey(cp => cp.pag_id)
                      .OnDelete(DeleteBehavior.Cascade); // Cuando se borre un pago, se borran sus comprobantes

                // Índices para optimización de consultas
                entity.HasIndex(cp => cp.pag_id)
                      .HasDatabaseName("IX_ComprobantesPago_Pago");

                entity.HasIndex(cp => cp.comp_activo)
                      .HasDatabaseName("IX_ComprobantesPago_Activo");

                entity.HasIndex(cp => cp.comp_tipo_comprobante)
                      .HasDatabaseName("IX_ComprobantesPago_TipoComprobante");

                entity.HasIndex(cp => cp.FechaRegistro)
                      .HasDatabaseName("IX_ComprobantesPago_FechaRegistro");

                // Índices compuestos para consultas frecuentes
                entity.HasIndex(cp => new { cp.pag_id, cp.comp_activo })
                      .HasDatabaseName("IX_ComprobantesPago_Pago_Activo");

                entity.HasIndex(cp => new { cp.comp_activo, cp.FechaRegistro })
                      .HasDatabaseName("IX_ComprobantesPago_Activo_FechaRegistro");

                entity.HasIndex(cp => new { cp.pag_id, cp.comp_tipo_comprobante })
                      .HasDatabaseName("IX_ComprobantesPago_Pago_TipoComprobante");
            });

            // ==========================================
            // CONFIGURACIÓN DE FORMAS DE RECAUDACIÓN
            // ==========================================
            modelBuilder.Entity<FormasDeRecaudacion>(entity =>
            {
                entity.ToTable("FormasDeRecaudacion");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.fdr_nombre)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");
            });

            // ==========================================
            // CONFIGURACIÓN DE RECAUDADORES
            // ==========================================
            modelBuilder.Entity<Recaudador>(entity =>
            {
                entity.ToTable("Recaudadores");
                entity.HasKey(e => e.Id);
                // Relación Recaudadores -> usuarios
                entity.HasOne(c => c.Usuario)
                      .WithMany(m => m.Recaudadores)
                      .HasForeignKey(c => c.usu_id)
                      .OnDelete(DeleteBehavior.Restrict);
                // Relación Recaudadores -> companias
                entity.HasOne(c => c.Compania)
                      .WithMany(m => m.Recaudadores)
                      .HasForeignKey(c => c.com_id)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.Property(e => e.FechaRegistro)
                  .HasColumnType("date")
                  .IsRequired(true);
            });
            // ==========================================
            // CONFIGURACIÓN DE MOTIVODECOBRO
            // ==========================================
            modelBuilder.Entity<MotivoDeCobro>(entity =>
            {
                entity.ToTable("MotivoDeCobros");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.mdc_nombre)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");
            });

            // ==========================================
            // CONFIGURACIÓN DE PERSONA
            // ==========================================
            modelBuilder.Entity<Persona>(entity =>
            {
                entity.ToTable("Personas");
                entity.HasKey(e => e.per_rut);

                entity.Property(e => e.per_rut)
                      .IsRequired()
                      .ValueGeneratedNever();

                entity.Property(e => e.per_vrut)
                    .HasMaxLength(1)
                    .IsRequired(false);

                entity.Property(e => e.per_paterno)
                    .HasMaxLength(50)
                    .IsRequired(false);

                entity.Property(e => e.per_materno)
                    .HasMaxLength(50)
                    .IsRequired(false);

                entity.Property(e => e.per_nombres)
                    .HasMaxLength(50)
                    .IsRequired(false);

                entity.Property(e => e.sex_codigo)
                    .HasMaxLength(25)
                    .IsRequired(false);

                entity.Property(e => e.per_fecnac)
                    .HasColumnType("date")
                    .IsRequired(false);

                entity.Property(e => e.per_email)
                    .HasMaxLength(100)
                    .IsRequired(false);

                entity.Property(e => e.per_movil)
                    .HasMaxLength(20)
                    .IsRequired(false);

                entity.Property(e => e.per_calle)
                    .HasMaxLength(80)
                    .IsRequired(false);

                entity.Property(e => e.per_numero)
                    .HasMaxLength(20)
                    .IsRequired(false);

                entity.Property(e => e.per_depto)
                    .HasMaxLength(10)
                    .IsRequired(false);

                entity.Property(e => e.per_block)
                    .HasMaxLength(35)
                    .IsRequired(false);

                entity.Property(e => e.per_comuna)
                    .HasMaxLength(50)
                    .IsRequired(false);
            });

            // ==========================================
            // CONFIGURACIÓN DE COMPANIA
            // ==========================================
            modelBuilder.Entity<Compania>(entity =>
            {
                entity.ToTable("Companias");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.com_nombre)
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnType("varchar(80)");
            });

            // ==========================================
            // CONFIGURACIÓN DE ROL
            // ==========================================
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");
            });

            // ==========================================
            // CONFIGURACIÓN DE COBROS
            // ==========================================
            modelBuilder.Entity<Cobros>(entity =>
            {
                entity.ToTable("Cobros");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.cob_fecha_emision)
                      .HasColumnType("datetime")
                      .IsRequired();

                entity.Property(e => e.cob_fecha_vencimiento)
                      .HasColumnType("datetime")
                      .IsRequired();

                entity.Property(e => e.cob_valor)
                      .HasColumnType("decimal(12,2)")
                      .IsRequired();

                entity.Property(e => e.FechaRegistro)
                      .HasColumnType("datetime");

                // Relación Cobros -> MotivoCobro
                entity.HasOne(c => c.MotivoCobro)
                      .WithMany(m => m.Cobros)
                      .HasForeignKey(c => c.mdc_id)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relación Cobros -> Contribuyente
                entity.HasOne(c => c.Contribuyente)
                      .WithMany(co => co.Cobros)
                      .HasForeignKey(c => c.con_id)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relación Cobros -> Generadores
                entity.HasOne(c => c.Generador)
                      .WithMany(g => g.Cobros)
                      .HasForeignKey(c => c.gen_id)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relación Cobros -> Compania
                entity.HasOne(c => c.Compania)
                      .WithMany(comp => comp.Cobros)
                      .HasForeignKey(c => c.com_id)
                      .OnDelete(DeleteBehavior.Restrict);

                // Índices para optimización de consultas
                entity.HasIndex(c => c.mdc_id)
                      .HasDatabaseName("IX_Cobros_MotivoCobro");

                entity.HasIndex(c => c.con_id)
                      .HasDatabaseName("IX_Cobros_Contribuyente");

                entity.HasIndex(c => c.gen_id)
                      .HasDatabaseName("IX_Cobros_Generador");

                entity.HasIndex(c => c.com_id)
                      .HasDatabaseName("IX_Cobros_Compania");

                entity.HasIndex(c => c.cob_fecha_emision)
                      .HasDatabaseName("IX_Cobros_FechaEmision");

                entity.HasIndex(c => c.cob_fecha_vencimiento)
                      .HasDatabaseName("IX_Cobros_FechaVencimiento");

                // Índices compuestos para consultas frecuentes
                entity.HasIndex(c => new { c.com_id, c.cob_fecha_emision })
                      .HasDatabaseName("IX_Cobros_Compania_FechaEmision");

                entity.HasIndex(c => new { c.con_id, c.cob_fecha_vencimiento })
                      .HasDatabaseName("IX_Cobros_Contribuyente_FechaVencimiento");

                entity.HasIndex(c => new { c.cob_fecha_vencimiento, c.com_id })
                      .HasDatabaseName("IX_Cobros_FechaVencimiento_Compania");
            });

            // ==========================================
            // CONFIGURACIÓN DE COBROS ANULADOS
            // ==========================================
            modelBuilder.Entity<CobrosAnulados>(entity =>
            {
                entity.ToTable("CobrosAnulados");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.caa_fecha_anulacion)
                      .HasColumnType("datetime")
                      .IsRequired();

                entity.Property(e => e.FechaRegistro)
                      .HasColumnType("datetime");

                // Relación CobrosAnulados -> Cobros
                entity.HasOne(ca => ca.Cobro)
                      .WithMany(c => c.CobrosAnulados)
                      .HasForeignKey(ca => ca.cob_id)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relación CobrosAnulados -> Anulaciones
                entity.HasOne(ca => ca.Anulacion)
                      .WithMany(a => a.CobrosAnulados)
                      .HasForeignKey(ca => ca.anu_id)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relación CobrosAnulados -> Generadores
                entity.HasOne(ca => ca.Generador)
                      .WithMany(g => g.CobrosAnulados)
                      .HasForeignKey(ca => ca.gen_id)
                      .OnDelete(DeleteBehavior.Restrict);

                // Índices para optimización de consultas
                entity.HasIndex(ca => ca.cob_id)
                      .HasDatabaseName("IX_CobrosAnulados_Cobro");

                entity.HasIndex(ca => ca.anu_id)
                      .HasDatabaseName("IX_CobrosAnulados_Anulacion");

                entity.HasIndex(ca => ca.gen_id)
                      .HasDatabaseName("IX_CobrosAnulados_Generador");

                entity.HasIndex(ca => ca.caa_fecha_anulacion)
                      .HasDatabaseName("IX_CobrosAnulados_FechaAnulacion");

                // Índices compuestos para consultas frecuentes
                entity.HasIndex(ca => new { ca.cob_id, ca.caa_fecha_anulacion })
                      .HasDatabaseName("IX_CobrosAnulados_Cobro_FechaAnulacion");

                entity.HasIndex(ca => new { ca.anu_id, ca.caa_fecha_anulacion })
                      .HasDatabaseName("IX_CobrosAnulados_Anulacion_FechaAnulacion");

                // Índice único para evitar anular el mismo cobro múltiples veces (opcional)
                //entity.HasIndex(ca => ca.cob_id)
                //      .IsUnique()
                //      .HasDatabaseName("IX_CobrosAnulados_Cobro_Unique");
            });

            // CONFIGURACIÓN DE PAGOS
            // ==========================================
            modelBuilder.Entity<Pagos>(entity =>
            {
                entity.ToTable("Pagos");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.pag_fecha)
                      .HasColumnType("datetime")
                      .IsRequired();

                entity.Property(e => e.pag_valor_pagado)
                      .HasColumnType("decimal(12,2)")
                      .IsRequired();

                entity.Property(e => e.FechaRegistro)
                      .HasColumnType("datetime");

                // Relación Pagos -> Cobros
                entity.HasOne(p => p.Cobro)
                      .WithMany(c => c.Pagos)
                      .HasForeignKey(p => p.cob_id)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relación Pagos -> Recaudadores
                entity.HasOne(p => p.Recaudador)
                      .WithMany(r => r.Pagos)
                      .HasForeignKey(p => p.rec_id)
                      .OnDelete(DeleteBehavior.Restrict);

                // Índices para optimización de consultas
                entity.HasIndex(p => p.cob_id)
                      .HasDatabaseName("IX_Pagos_Cobro");

                entity.HasIndex(p => p.rec_id)
                      .HasDatabaseName("IX_Pagos_Recaudador");

                entity.HasIndex(p => p.pag_fecha)
                      .HasDatabaseName("IX_Pagos_Fecha");

                entity.HasIndex(p => p.pag_valor_pagado)
                      .HasDatabaseName("IX_Pagos_Valor");

                // Índices compuestos para consultas frecuentes
                entity.HasIndex(p => new { p.cob_id, p.pag_fecha })
                      .HasDatabaseName("IX_Pagos_Cobro_Fecha");

                entity.HasIndex(p => new { p.rec_id, p.pag_fecha })
                      .HasDatabaseName("IX_Pagos_Recaudador_Fecha");

                entity.HasIndex(p => new { p.pag_fecha, p.pag_valor_pagado })
                      .HasDatabaseName("IX_Pagos_Fecha_Valor");

                // Índice para consultas de recaudación por período
                entity.HasIndex(p => new { p.rec_id, p.pag_fecha, p.pag_valor_pagado })
                      .HasDatabaseName("IX_Pagos_Recaudador_Fecha_Valor");
            });

            // ==========================================
            // CONFIGURACIÓN DE PAGOS ANULADOS
            // ==========================================
            modelBuilder.Entity<PagosAnulados>(entity =>
            {
                entity.ToTable("PagosAnulados");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.caa_fecha_anulacion)
                      .HasColumnType("datetime")
                      .IsRequired();

                entity.Property(e => e.FechaRegistro)
                      .HasColumnType("datetime");

                // Relación PagosAnulados -> Pagos
                entity.HasOne(pa => pa.Pago)
                      .WithMany(p => p.PagosAnulados)
                      .HasForeignKey(pa => pa.pag_id)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relación PagosAnulados -> Anulaciones
                entity.HasOne(pa => pa.Anulacion)
                      .WithMany(a => a.PagosAnulados)
                      .HasForeignKey(pa => pa.anu_id)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relación PagosAnulados -> Personas
                entity.HasOne(pa => pa.Recaudador)
                      .WithMany(r => r.PagosAnulados)
                      .HasForeignKey(pa => pa.rec_id)
                      .OnDelete(DeleteBehavior.Restrict);

                // Índices para optimización de consultas
                entity.HasIndex(pa => pa.pag_id)
                      .HasDatabaseName("IX_PagosAnulados_Pago");

                entity.HasIndex(pa => pa.anu_id)
                      .HasDatabaseName("IX_PagosAnulados_Anulacion");

                entity.HasIndex(pa => pa.rec_id)
                      .HasDatabaseName("IX_PagosAnulados_Recaudador");

                entity.HasIndex(pa => pa.caa_fecha_anulacion)
                      .HasDatabaseName("IX_PagosAnulados_FechaAnulacion");

                // Índices compuestos para consultas frecuentes
                entity.HasIndex(pa => new { pa.pag_id, pa.caa_fecha_anulacion })
                      .HasDatabaseName("IX_PagosAnulados_Pago_FechaAnulacion");

                entity.HasIndex(pa => new { pa.rec_id, pa.caa_fecha_anulacion })
                      .HasDatabaseName("IX_PagosAnulados_Recaudador_FechaAnulacion");

                entity.HasIndex(pa => new { pa.anu_id, pa.caa_fecha_anulacion })
                      .HasDatabaseName("IX_PagosAnulados_Anulacion_FechaAnulacion");

                // Índice único para evitar anular el mismo pago múltiples veces (opcional)
                entity.HasIndex(pa => pa.pag_id)
                      .IsUnique()
                      .HasDatabaseName("IX_PagosAnulados_Pago_Unique");
            });

            // ==========================================
            // CONFIGURACIÓN DE USUARIO
            // ==========================================
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Login)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.Rut)
                      .IsRequired();

                entity.Property(e => e.Dv)
                      .IsRequired()
                      .HasMaxLength(1);

                entity.Property(e => e.Nombre)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.Clave)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.FechaRegistro)
                      .HasColumnType("datetime");

                entity.Property(e => e.Activo)
                      .IsRequired();

                // Relación Usuario -> Rol
                entity.HasOne(e => e.Rol)
                      .WithMany(r => r.Usuarios)
                      .HasForeignKey(e => e.RolId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // CONFIGURACIÓN DE ANULACIONES
            // ==========================================
            modelBuilder.Entity<Anulaciones>(entity =>
            {
                entity.ToTable("Anulaciones");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.anu_descripcion)
                      .IsRequired()
                      .HasMaxLength(255)
                      .HasColumnType("varchar(255)");

                entity.Property(e => e.anu_activo)
                      .IsRequired();

                entity.Property(e => e.FechaRegistro)
                      .HasColumnType("datetime");

                // Índices para optimización de consultas
                entity.HasIndex(a => a.anu_activo)
                      .HasDatabaseName("IX_Anulaciones_Activo");

                entity.HasIndex(a => a.anu_descripcion)
                      .HasDatabaseName("IX_Anulaciones_Descripcion");
            });

            // ==========================================
            // CONFIGURACIÓN DE RECAUDADORES
            // ==========================================
            modelBuilder.Entity<Recaudador>(entity =>
            {
                entity.ToTable("Recaudadores");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.rec_activo)
                      .IsRequired();

                entity.Property(e => e.FechaRegistro)
                      .HasColumnType("datetime");

                // Relación Recaudadores -> Usuario
                entity.HasOne(r => r.Usuario)
                      .WithMany(u => u.Recaudadores)
                      .HasForeignKey(r => r.usu_id)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relación Recaudadores -> Compania
                entity.HasOne(r => r.Compania)
                      .WithMany(c => c.Recaudadores)
                      .HasForeignKey(r => r.com_id)
                      .OnDelete(DeleteBehavior.Restrict);

                // Índice único para evitar duplicados Usuario-Compania
                entity.HasIndex(r => new { r.usu_id, r.com_id })
                      .IsUnique()
                      .HasDatabaseName("IX_Recaudadores_Usuario_Compania");
            });

            // ==========================================
            // CONFIGURACIÓN DE MOTIVOCOBRO
            // ==========================================
            modelBuilder.Entity<MotivoDeCobro>(entity =>
            {
                entity.ToTable("MotivosDeCobro");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.mdc_nombre)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("varchar(50)");

                entity.Property(e => e.mdc_activo)
                    .IsRequired();
            });

            // ==========================================
            // CONFIGURACIÓN DE GENERADORES
            // ==========================================
            modelBuilder.Entity<Generadores>(entity =>
            {
                entity.ToTable("Generadores");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.gen_activo)
                      .IsRequired();

                entity.Property(e => e.FechaRegistro)
                      .HasColumnType("datetime");

                // Relación Generadores -> Usuario
                entity.HasOne(g => g.Usuario)
                      .WithMany(u => u.Generadores)
                      .HasForeignKey(g => g.usu_id)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relación Generadores -> Compania
                entity.HasOne(g => g.Compania)
                      .WithMany(c => c.Generadores)
                      .HasForeignKey(g => g.com_id)
                      .OnDelete(DeleteBehavior.Restrict);

                // Índices para optimización de consultas
                entity.HasIndex(g => g.usu_id)
                      .HasDatabaseName("IX_Generadores_Usuario");

                entity.HasIndex(g => g.com_id)
                      .HasDatabaseName("IX_Generadores_Compania");

                entity.HasIndex(g => g.gen_activo)
                      .HasDatabaseName("IX_Generadores_Activo");

                // Índice compuesto para consultas frecuentes
                entity.HasIndex(g => new { g.com_id, g.gen_activo })
                      .HasDatabaseName("IX_Generadores_Compania_Activo");
            });

            // ==========================================
            // CONFIGURACIÓN DE CONTRIBUYENTES
            // ==========================================
            modelBuilder.Entity<Contribuyente>(entity =>
            {
                entity.ToTable("Contribuyentes");
                entity.HasKey(e => e.Id);

                // Configurar propiedades básicas
                entity.Property(e => e.per_rut).IsRequired();
                entity.Property(e => e.mdc_id).IsRequired();
                entity.Property(e => e.con_valor_aporte)
                    .HasColumnType("decimal(12,0)").IsRequired();
                entity.Property(e => e.con_periodicidad_cobro)
                    .HasMaxLength(20).IsRequired();
                entity.Property(e => e.con_dia_del_cargo).IsRequired();
                entity.Property(e => e.con_fecha_inicio)
                    .HasColumnType("datetime").IsRequired();
                entity.Property(e => e.con_fecha_fin)
                    .HasColumnType("datetime").IsRequired(false);
                entity.Property(e => e.rec_id).IsRequired();
                entity.Property(e => e.con_activo).IsRequired();
                entity.Property(e => e.com_id).IsRequired();
                entity.Property(e => e.FechaRegistro)
                    .HasColumnType("datetime").IsRequired();

                // CLAVE: Ignorar TODAS las propiedades de navegación
                // y NO crear foreign keys automáticas
                entity.Ignore(e => e.Persona);
                entity.Ignore(e => e.MotivoCobro);
                entity.Ignore(e => e.Recaudador);
                entity.Ignore(e => e.Compania);
                entity.Ignore(e => e.Cobros);

                // Evitar que EF cree foreign keys automáticas
                // No definir relaciones HasOne/WithMany aquí
            });

            modelBuilder.Entity<Voluntarios>(entity =>
            {
                entity.ToTable("Voluntarios");
                entity.HasKey(e => e.id);

                entity.Property(e => e.vol_estado)
                      .IsRequired()
                      .HasMaxLength(50);

                // Relación Voluntarios -> Persona
                entity.HasOne(v => v.Persona)
                      .WithMany(p => p.Voluntarios)
                      .HasForeignKey(v => v.per_rut)
                      .OnDelete(DeleteBehavior.Restrict);

                // Relación Voluntarios -> Compania
                entity.HasOne(v => v.Compania)
                      .WithMany(c => c.Voluntarios)
                      .HasForeignKey(v => v.com_id)
                      .OnDelete(DeleteBehavior.Restrict);

                // Índices 
                entity.HasIndex(v => v.per_rut)
                      .HasDatabaseName("IX_Voluntarios_Persona");

                entity.HasIndex(v => v.com_id)
                      .HasDatabaseName("IX_Voluntarios_Compania");

                entity.HasIndex(v => v.vol_estado)
                      .HasDatabaseName("IX_Voluntarios_Estado");

                // Índice compuesto para consultas frecuentes
                entity.HasIndex(v => new { v.com_id, v.vol_estado })
                      .HasDatabaseName("IX_Voluntarios_Compania_Estado");
            });

            // ✅ AGREGAR: Vista "VContribuyentes"
            modelBuilder.Entity<VContribuyente>(entity =>
            {
                entity.ToView("VContribuyentes"); // Nombre de tu vista en la BD
                entity.HasKey(e => e.Id);

                // Configurar tipos de datos específicos
                entity.Property(e => e.per_vrut)
                    .HasMaxLength(1);

                entity.Property(e => e.per_paterno)
                    .HasMaxLength(50);

                entity.Property(e => e.per_materno)
                    .HasMaxLength(50);

                entity.Property(e => e.per_nombres)
                    .HasMaxLength(50);

                entity.Property(e => e.sex_codigo)
                    .HasMaxLength(25);

                entity.Property(e => e.per_email)
                    .HasMaxLength(100);

                entity.Property(e => e.per_movil)
                    .HasMaxLength(20);

                entity.Property(e => e.per_calle)
                    .HasMaxLength(80);

                entity.Property(e => e.per_numero)
                    .HasMaxLength(20);

                entity.Property(e => e.per_depto)
                    .HasMaxLength(10);

                entity.Property(e => e.per_block)
                    .HasMaxLength(35);

                entity.Property(e => e.per_comuna)
                    .HasMaxLength(50);

                entity.Property(e => e.com_nombre)
                    .HasMaxLength(80);

                entity.Property(e => e.mdc_nombre)
                    .HasMaxLength(50);

                entity.Property(e => e.con_valor_aporte)
                    .HasColumnType("decimal(12,0)");

                entity.Property(e => e.con_periodicidad_cobro)
                    .HasMaxLength(20);

                entity.Property(e => e.con_fecha_inicio)
                    .HasColumnType("datetime");

                entity.Property(e => e.con_fecha_fin)
                    .HasColumnType("datetime");

                entity.Property(e => e.per_fecnac)
                    .HasColumnType("date");

                entity.Property(e => e.FechaRegistro)
                    .HasColumnType("datetime");
            });
        }
    }

}
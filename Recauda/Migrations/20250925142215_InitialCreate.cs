using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recauda.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Anulaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    anu_descripcion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    anu_activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anulaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Companias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    com_nombre = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormasDeRecaudacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fdr_nombre = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormasDeRecaudacion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MotivosDeCobro",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    mdc_nombre = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    mdc_activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotivosDeCobro", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Personas",
                columns: table => new
                {
                    per_rut = table.Column<int>(type: "int", nullable: false),
                    per_vrut = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    per_paterno = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    per_materno = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    per_nombres = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    sex_codigo = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    per_fecnac = table.Column<DateTime>(type: "date", nullable: true),
                    per_email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    per_movil = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    per_calle = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    per_numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    per_depto = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    per_block = table.Column<string>(type: "nvarchar(35)", maxLength: 35, nullable: true),
                    per_comuna = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personas", x => x.per_rut);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Voluntarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    per_rut = table.Column<int>(type: "int", nullable: false),
                    com_id = table.Column<int>(type: "int", nullable: false),
                    vol_estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voluntarios", x => x.id);
                    table.ForeignKey(
                        name: "FK_Voluntarios_Companias_com_id",
                        column: x => x.com_id,
                        principalTable: "Companias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Voluntarios_Personas_per_rut",
                        column: x => x.per_rut,
                        principalTable: "Personas",
                        principalColumn: "per_rut",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Login = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Rut = table.Column<int>(type: "int", nullable: false),
                    Dv = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Clave = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RolId = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Generadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    usu_id = table.Column<int>(type: "int", nullable: false),
                    gen_activo = table.Column<bool>(type: "bit", nullable: false),
                    com_id = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Generadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Generadores_Companias_com_id",
                        column: x => x.com_id,
                        principalTable: "Companias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Generadores_Usuarios_usu_id",
                        column: x => x.usu_id,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Recaudadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    rec_activo = table.Column<bool>(type: "bit", nullable: false),
                    usu_id = table.Column<int>(type: "int", nullable: false),
                    com_id = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recaudadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recaudadores_Companias_com_id",
                        column: x => x.com_id,
                        principalTable: "Companias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Recaudadores_Usuarios_usu_id",
                        column: x => x.usu_id,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contribuyentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    per_rut = table.Column<int>(type: "int", nullable: false),
                    mdc_id = table.Column<int>(type: "int", nullable: false),
                    con_valor_aporte = table.Column<decimal>(type: "decimal(12,0)", nullable: false),
                    con_periodicidad_cobro = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    con_dia_del_cargo = table.Column<int>(type: "int", nullable: false),
                    con_fecha_inicio = table.Column<DateTime>(type: "datetime", nullable: false),
                    con_fecha_fin = table.Column<DateTime>(type: "datetime", nullable: true),
                    rec_id = table.Column<int>(type: "int", nullable: false),
                    con_activo = table.Column<bool>(type: "bit", nullable: false),
                    com_id = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: false),
                    CompaniaId = table.Column<int>(type: "int", nullable: true),
                    MotivoDeCobroId = table.Column<int>(type: "int", nullable: true),
                    Personaper_rut = table.Column<int>(type: "int", nullable: true),
                    RecaudadorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contribuyentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contribuyentes_Companias_CompaniaId",
                        column: x => x.CompaniaId,
                        principalTable: "Companias",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Contribuyentes_MotivosDeCobro_MotivoDeCobroId",
                        column: x => x.MotivoDeCobroId,
                        principalTable: "MotivosDeCobro",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Contribuyentes_Personas_Personaper_rut",
                        column: x => x.Personaper_rut,
                        principalTable: "Personas",
                        principalColumn: "per_rut");
                    table.ForeignKey(
                        name: "FK_Contribuyentes_Recaudadores_RecaudadorId",
                        column: x => x.RecaudadorId,
                        principalTable: "Recaudadores",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Cobros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cob_fecha_emision = table.Column<DateTime>(type: "datetime", nullable: false),
                    cob_fecha_vencimiento = table.Column<DateTime>(type: "datetime", nullable: false),
                    mdc_id = table.Column<int>(type: "int", nullable: false),
                    cob_valor = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    con_id = table.Column<int>(type: "int", nullable: false),
                    gen_id = table.Column<int>(type: "int", nullable: false),
                    com_id = table.Column<int>(type: "int", nullable: false),
                    ContribuyenteId = table.Column<int>(type: "int", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cobros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cobros_Companias_com_id",
                        column: x => x.com_id,
                        principalTable: "Companias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cobros_Contribuyentes_ContribuyenteId",
                        column: x => x.ContribuyenteId,
                        principalTable: "Contribuyentes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Cobros_Generadores_gen_id",
                        column: x => x.gen_id,
                        principalTable: "Generadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cobros_MotivosDeCobro_mdc_id",
                        column: x => x.mdc_id,
                        principalTable: "MotivosDeCobro",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CobrosAnulados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cob_id = table.Column<int>(type: "int", nullable: false),
                    caa_fecha_anulacion = table.Column<DateTime>(type: "datetime", nullable: false),
                    anu_id = table.Column<int>(type: "int", nullable: false),
                    gen_id = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CobrosAnulados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CobrosAnulados_Anulaciones_anu_id",
                        column: x => x.anu_id,
                        principalTable: "Anulaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CobrosAnulados_Cobros_cob_id",
                        column: x => x.cob_id,
                        principalTable: "Cobros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CobrosAnulados_Generadores_gen_id",
                        column: x => x.gen_id,
                        principalTable: "Generadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cob_id = table.Column<int>(type: "int", nullable: false),
                    pag_fecha = table.Column<DateTime>(type: "datetime", nullable: false),
                    pag_valor_pagado = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    rec_id = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_Cobros_cob_id",
                        column: x => x.cob_id,
                        principalTable: "Cobros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pagos_Recaudadores_rec_id",
                        column: x => x.rec_id,
                        principalTable: "Recaudadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PagosAnulados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    pag_id = table.Column<int>(type: "int", nullable: false),
                    caa_fecha_anulacion = table.Column<DateTime>(type: "datetime", nullable: false),
                    anu_id = table.Column<int>(type: "int", nullable: false),
                    rec_id = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosAnulados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosAnulados_Anulaciones_anu_id",
                        column: x => x.anu_id,
                        principalTable: "Anulaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PagosAnulados_Pagos_pag_id",
                        column: x => x.pag_id,
                        principalTable: "Pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PagosAnulados_Recaudadores_rec_id",
                        column: x => x.rec_id,
                        principalTable: "Recaudadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Anulaciones_Activo",
                table: "Anulaciones",
                column: "anu_activo");

            migrationBuilder.CreateIndex(
                name: "IX_Anulaciones_Descripcion",
                table: "Anulaciones",
                column: "anu_descripcion");

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_Compania",
                table: "Cobros",
                column: "com_id");

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_Compania_FechaEmision",
                table: "Cobros",
                columns: new[] { "com_id", "cob_fecha_emision" });

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_Contribuyente",
                table: "Cobros",
                column: "con_id");

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_Contribuyente_FechaVencimiento",
                table: "Cobros",
                columns: new[] { "con_id", "cob_fecha_vencimiento" });

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_ContribuyenteId",
                table: "Cobros",
                column: "ContribuyenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_FechaEmision",
                table: "Cobros",
                column: "cob_fecha_emision");

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_FechaVencimiento",
                table: "Cobros",
                column: "cob_fecha_vencimiento");

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_FechaVencimiento_Compania",
                table: "Cobros",
                columns: new[] { "cob_fecha_vencimiento", "com_id" });

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_Generador",
                table: "Cobros",
                column: "gen_id");

            migrationBuilder.CreateIndex(
                name: "IX_Cobros_MotivoCobro",
                table: "Cobros",
                column: "mdc_id");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosAnulados_Anulacion",
                table: "CobrosAnulados",
                column: "anu_id");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosAnulados_Anulacion_FechaAnulacion",
                table: "CobrosAnulados",
                columns: new[] { "anu_id", "caa_fecha_anulacion" });

            migrationBuilder.CreateIndex(
                name: "IX_CobrosAnulados_Cobro",
                table: "CobrosAnulados",
                column: "cob_id");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosAnulados_Cobro_FechaAnulacion",
                table: "CobrosAnulados",
                columns: new[] { "cob_id", "caa_fecha_anulacion" });

            migrationBuilder.CreateIndex(
                name: "IX_CobrosAnulados_FechaAnulacion",
                table: "CobrosAnulados",
                column: "caa_fecha_anulacion");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosAnulados_Generador",
                table: "CobrosAnulados",
                column: "gen_id");

            migrationBuilder.CreateIndex(
                name: "IX_Contribuyentes_CompaniaId",
                table: "Contribuyentes",
                column: "CompaniaId");

            migrationBuilder.CreateIndex(
                name: "IX_Contribuyentes_MotivoDeCobroId",
                table: "Contribuyentes",
                column: "MotivoDeCobroId");

            migrationBuilder.CreateIndex(
                name: "IX_Contribuyentes_Personaper_rut",
                table: "Contribuyentes",
                column: "Personaper_rut");

            migrationBuilder.CreateIndex(
                name: "IX_Contribuyentes_RecaudadorId",
                table: "Contribuyentes",
                column: "RecaudadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Generadores_Activo",
                table: "Generadores",
                column: "gen_activo");

            migrationBuilder.CreateIndex(
                name: "IX_Generadores_Compania",
                table: "Generadores",
                column: "com_id");

            migrationBuilder.CreateIndex(
                name: "IX_Generadores_Compania_Activo",
                table: "Generadores",
                columns: new[] { "com_id", "gen_activo" });

            migrationBuilder.CreateIndex(
                name: "IX_Generadores_Usuario",
                table: "Generadores",
                column: "usu_id");

            migrationBuilder.CreateIndex(
                name: "IX_Generadores_Usuario_Compania",
                table: "Generadores",
                columns: new[] { "usu_id", "com_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_Cobro",
                table: "Pagos",
                column: "cob_id");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_Cobro_Fecha",
                table: "Pagos",
                columns: new[] { "cob_id", "pag_fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_Fecha",
                table: "Pagos",
                column: "pag_fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_Fecha_Valor",
                table: "Pagos",
                columns: new[] { "pag_fecha", "pag_valor_pagado" });

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_Recaudador",
                table: "Pagos",
                column: "rec_id");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_Recaudador_Fecha",
                table: "Pagos",
                columns: new[] { "rec_id", "pag_fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_Recaudador_Fecha_Valor",
                table: "Pagos",
                columns: new[] { "rec_id", "pag_fecha", "pag_valor_pagado" });

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_Valor",
                table: "Pagos",
                column: "pag_valor_pagado");

            migrationBuilder.CreateIndex(
                name: "IX_PagosAnulados_Anulacion",
                table: "PagosAnulados",
                column: "anu_id");

            migrationBuilder.CreateIndex(
                name: "IX_PagosAnulados_Anulacion_FechaAnulacion",
                table: "PagosAnulados",
                columns: new[] { "anu_id", "caa_fecha_anulacion" });

            migrationBuilder.CreateIndex(
                name: "IX_PagosAnulados_FechaAnulacion",
                table: "PagosAnulados",
                column: "caa_fecha_anulacion");

            migrationBuilder.CreateIndex(
                name: "IX_PagosAnulados_Pago_FechaAnulacion",
                table: "PagosAnulados",
                columns: new[] { "pag_id", "caa_fecha_anulacion" });

            migrationBuilder.CreateIndex(
                name: "IX_PagosAnulados_Pago_Unique",
                table: "PagosAnulados",
                column: "pag_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PagosAnulados_Recaudador",
                table: "PagosAnulados",
                column: "rec_id");

            migrationBuilder.CreateIndex(
                name: "IX_PagosAnulados_Recaudador_FechaAnulacion",
                table: "PagosAnulados",
                columns: new[] { "rec_id", "caa_fecha_anulacion" });

            migrationBuilder.CreateIndex(
                name: "IX_Recaudadores_com_id",
                table: "Recaudadores",
                column: "com_id");

            migrationBuilder.CreateIndex(
                name: "IX_Recaudadores_Usuario_Compania",
                table: "Recaudadores",
                columns: new[] { "usu_id", "com_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolId",
                table: "Usuarios",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_Voluntarios_Compania",
                table: "Voluntarios",
                column: "com_id");

            migrationBuilder.CreateIndex(
                name: "IX_Voluntarios_Compania_Estado",
                table: "Voluntarios",
                columns: new[] { "com_id", "vol_estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Voluntarios_Estado",
                table: "Voluntarios",
                column: "vol_estado");

            migrationBuilder.CreateIndex(
                name: "IX_Voluntarios_Persona",
                table: "Voluntarios",
                column: "per_rut");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CobrosAnulados");

            migrationBuilder.DropTable(
                name: "FormasDeRecaudacion");

            migrationBuilder.DropTable(
                name: "PagosAnulados");

            migrationBuilder.DropTable(
                name: "Voluntarios");

            migrationBuilder.DropTable(
                name: "Anulaciones");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "Cobros");

            migrationBuilder.DropTable(
                name: "Contribuyentes");

            migrationBuilder.DropTable(
                name: "Generadores");

            migrationBuilder.DropTable(
                name: "MotivosDeCobro");

            migrationBuilder.DropTable(
                name: "Personas");

            migrationBuilder.DropTable(
                name: "Recaudadores");

            migrationBuilder.DropTable(
                name: "Companias");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}

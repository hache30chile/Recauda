using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Recauda.Models;

namespace Recauda.Services
{
    public class ComprobantePagoService
    {
        public byte[] GenerarComprobantePago(VCobros cobro, Pagos pago, string nombreRecaudador, string nombreCompania)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(50);

                    page.Content().Column(column =>
                    {
                        // Header con logo y datos de la empresa
                        column.Item().PaddingBottom(20).Row(row =>
                        {
                            // Logo
                            row.ConstantItem(80).Column(logoColumn =>
                            {
                                logoColumn.Item().Height(60).Image("wwwroot/img/logo_cb_san_pedro_de_la_paz.png");
                            });

                            row.RelativeItem().PaddingLeft(15).Column(headerColumn =>
                            {
                                headerColumn.Item().AlignCenter().Text(nombreCompania.ToUpper())
                                    .FontSize(14).Bold().FontColor(Colors.Black);

                                headerColumn.Item().AlignCenter().Text("SERVICIO DE UTILIDAD PÚBLICA")
                                    .FontSize(10).FontColor(Colors.Black);

                                headerColumn.Item().AlignCenter().Text("Dirección de la empresa")
                                    .FontSize(8).FontColor(Colors.Black);

                                headerColumn.Item().AlignCenter().Text("Teléfonos: 000-000000 - 000-000000")
                                    .FontSize(8).FontColor(Colors.Black);

                                headerColumn.Item().AlignCenter().Text("e-mail: contacto@empresa.cl")
                                    .FontSize(8).FontColor(Colors.Black);

                                headerColumn.Item().AlignCenter().Text("R.U.T: 00.000.000-0")
                                    .FontSize(8).FontColor(Colors.Black);
                            });

                            // Número de comprobante
                            row.ConstantItem(80).Column(numColumn =>
                            {
                                numColumn.Item().AlignRight().Text(pago.Id.ToString().PadLeft(4, '0'))
                                    .FontSize(16).Bold().FontColor(Colors.Red.Darken1);
                            });
                        });

                        // Título del documento
                        column.Item().PaddingVertical(15).AlignCenter()
                            .Text("RECIBO DE DINERO")
                            .FontSize(18).Bold().FontColor(Colors.Black);

                        // Línea de ciudad y fecha
                        column.Item().PaddingBottom(20).Row(row =>
                        {
                            row.RelativeItem().Text($"Concepción {ConvertirFechaATexto(pago.pag_fecha)}")
                                .FontSize(11).FontColor(Colors.Black);
                        });

                        // Datos del contribuyente
                        column.Item().PaddingBottom(15).Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"Recibimos del Señor(es) {cobro.per_nombre_completo}")
                                    .FontSize(11).FontColor(Colors.Black);
                                row.ConstantItem(120).Text($"R.U.T.: {cobro.per_rut}-{cobro.per_vrut}")
                                    .FontSize(11).FontColor(Colors.Black);
                            });

                            col.Item().PaddingTop(10).Text("Dirección ________________________________________________________________")
                                .FontSize(11).FontColor(Colors.Black);
                        });

                        // Valor en pesos y letras
                        column.Item().PaddingVertical(10).Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"la suma de $ {pago.pag_valor_pagado:N0}")
                                    .FontSize(11).FontColor(Colors.Black);
                                row.RelativeItem().Text($"( {ConvertirNumeroALetras(pago.pag_valor_pagado)} )")
                                    .FontSize(11).FontColor(Colors.Black);
                            });
                        });

                        // Concepto del pago con checkboxes
                        column.Item().PaddingVertical(15).Column(col =>
                        {
                            col.Item().Text("Por concepto de:")
                                .FontSize(11).FontColor(Colors.Black);

                            col.Item().PaddingTop(10).Column(conceptoCol =>
                            {
                                // Determinar cuál checkbox marcar basado en el concepto
                                var esAporteParticular = cobro.mdc_nombre.ToLower().Contains("aporte");
                                var esResolucionJudicial = cobro.mdc_nombre.ToLower().Contains("resolución") || cobro.mdc_nombre.ToLower().Contains("judicial");
                                var esAporteCompania = cobro.mdc_nombre.ToLower().Contains("compañía");
                                var esPrestacionServicios = !esAporteParticular && !esResolucionJudicial && !esAporteCompania;

                                conceptoCol.Item().Row(row =>
                                {
                                    row.ConstantItem(15).Text(esAporteParticular ? "✓" : "__")
                                        .FontSize(11).FontColor(Colors.Black);
                                    row.RelativeItem().Text("Aporte de particular")
                                        .FontSize(11).FontColor(Colors.Black);
                                });

                                conceptoCol.Item().PaddingTop(5).Row(row =>
                                {
                                    row.ConstantItem(15).Text(esResolucionJudicial ? "✓" : "__")
                                        .FontSize(11).FontColor(Colors.Black);
                                    row.RelativeItem().Text("Resolución Judicial")
                                        .FontSize(11).FontColor(Colors.Black);
                                });

                                conceptoCol.Item().PaddingTop(5).Row(row =>
                                {
                                    row.ConstantItem(15).Text(esAporteCompania ? "✓" : "__")
                                        .FontSize(11).FontColor(Colors.Black);
                                    row.RelativeItem().Text("Aporte de Compañía")
                                        .FontSize(11).FontColor(Colors.Black);
                                });

                                conceptoCol.Item().PaddingTop(5).Row(row =>
                                {
                                    row.ConstantItem(15).Text(esAporteCompania ? "✓" : "__")
                                        .FontSize(11).FontColor(Colors.Black);
                                    row.RelativeItem().Text("Cuota de Voluntario")
                                        .FontSize(11).FontColor(Colors.Black);
                                });

                                conceptoCol.Item().PaddingTop(5).Row(row =>
                                {
                                    row.ConstantItem(15).Text(esPrestacionServicios ? "✓" : "__")
                                        .FontSize(11).FontColor(Colors.Black);
                                    row.RelativeItem().Text($"Prestación de Servicios")
                                        .FontSize(11).FontColor(Colors.Black);
                                });
                            });
                        });

                        // Información adicional
                        column.Item().PaddingTop(30).Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"Fecha de Pago: {pago.pag_fecha:dd/MM/yyyy}")
                                    .FontSize(10).FontColor(Colors.Black);
                                row.RelativeItem().Text($"Recaudado por: {nombreRecaudador}")
                                    .FontSize(10).FontColor(Colors.Black);
                            });

                            col.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text($"Período: {cobro.cob_fecha_emision:MM/yyyy}")
                                    .FontSize(10).FontColor(Colors.Black);
                                row.RelativeItem().Text($"Fecha de Vencimiento: {cobro.cob_fecha_vencimiento:dd/MM/yyyy}")
                                    .FontSize(10).FontColor(Colors.Black);
                            });
                        });

                        //// Firma y sello (espacio en blanco)
                        //column.Item().PaddingTop(40).Row(row =>
                        //{
                        //    row.RelativeItem().Column(col =>
                        //    {
                        //        col.Item().AlignCenter().Text("_________________________")
                        //            .FontSize(11).FontColor(Colors.Black);
                        //        col.Item().AlignCenter().Text("Firma y Sello")
                        //            .FontSize(9).FontColor(Colors.Black);
                        //    });
                        //});
                    });
                });
            });

            return document.GeneratePdf();
        }

        private string ConvertirFechaATexto(DateTime fecha)
        {
            var meses = new string[]
            {
                "", "enero", "febrero", "marzo", "abril", "mayo", "junio",
                "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"
            };

            return $"{fecha.Day} de {meses[fecha.Month]} de {fecha.Year}";
        }

        private string ConvertirNumeroALetras(decimal numero)
        {
            // Implementación simple para convertir números a letras
            // Puedes expandir esta función según tus necesidades
            if (numero == 0) return "CERO PESOS";

            var entero = (int)numero;
            var palabras = new List<string>();

            if (entero >= 1000000)
            {
                var millones = entero / 1000000;
                palabras.Add($"{millones} MILLÓN{(millones > 1 ? "ES" : "")}");
                entero %= 1000000;
            }

            if (entero >= 1000)
            {
                var miles = entero / 1000;
                if (miles == 1)
                    palabras.Add("MIL");
                else
                    palabras.Add($"{miles} MIL");
                entero %= 1000;
            }

            if (entero > 0)
            {
                palabras.Add(entero.ToString());
            }

            return string.Join(" ", palabras) + " PESOS";
        }
    }
}
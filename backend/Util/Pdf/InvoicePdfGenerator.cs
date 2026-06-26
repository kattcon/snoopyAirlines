using System.Globalization;
using System.Net.Mime;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using snoopy_airlines_backend.Domain;
using SnoopyAirlines.Util.Email;

namespace SnoopyAirlines.Util.Pdf
{
    public sealed class InvoicePdfGenerator : IInvoicePdfGenerator
    {
        static InvoicePdfGenerator()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public Attachment GenerateInvoice(PurchaseOrderEmailData data)
        {
            return new Attachment(
                BuildFileName(data),
                MediaTypeNames.Application.Pdf,
                () => GenerateInvoiceStream(data));
        }

        private static Stream GenerateInvoiceStream(PurchaseOrderEmailData data)
        {
            var stream = new MemoryStream();
            CreateInvoiceDocument(data).GeneratePdf(stream);
            stream.Position = 0;

            return stream;
        }

        private static IDocument CreateInvoiceDocument(PurchaseOrderEmailData data)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(style => style.FontSize(10).FontFamily("Arial"));

                    page.Header().Element(header => ComposeHeader(header, data));
                    page.Content().PaddingVertical(24).Column(column =>
                    {
                        column.Spacing(18);
                        column.Item().Element(content => ComposeBookingSummary(content, data));
                        column.Item().Element(content => ComposeFlightSummary(content, data));
                        column.Item().Element(content => ComposeInvoiceLines(content, data));
                        column.Item().Element(content => ComposePassengers(content, data));
                    });
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Snoopy Airlines - ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            });
        }

        private static void ComposeHeader(IContainer container, PurchaseOrderEmailData data)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Snoopy Airlines")
                        .FontSize(22)
                        .Bold()
                        .FontColor(Colors.Blue.Darken3);

                    column.Item().Text("Factura de compra")
                        .FontSize(12)
                        .FontColor(Colors.Grey.Darken2);
                });

                row.ConstantItem(210).AlignRight().Column(column =>
                {
                    column.Item().AlignRight().Text("Factura")
                        .FontSize(28)
                        .Bold()
                        .FontColor(Colors.Blue.Darken3);

                    column.Item().AlignRight().Text($"Reserva {ValueOrDash(data.ConfirmationCode)}")
                        .FontSize(11)
                        .FontColor(Colors.Grey.Darken2);
                });
            });
        }

        private static void ComposeBookingSummary(IContainer container, PurchaseOrderEmailData data)
        {
            container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(12).Column(column =>
            {
                column.Spacing(8);
                column.Item().Text("Detalles de compra").Bold().FontSize(13);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(left =>
                    {
                        AddLabelValue(left, "Numero de reserva", data.ConfirmationCode);
                        AddLabelValue(left, "Fecha de compra", data.PurchaseDate);
                        AddLabelValue(left, "Metodo de pago", data.PaymentMethod);
                    });

                    row.RelativeItem().Column(right =>
                    {
                        AddLabelValue(right, "ID de transaccion", data.TransactionId);
                        AddLabelValue(right, "Orden de compra", data.PurchaseOrderId.ToString(CultureInfo.InvariantCulture));
                        AddLabelValue(right, "Estado", data.BookingStatus);
                    });
                });
            });
        }

        private static void ComposeFlightSummary(IContainer container, PurchaseOrderEmailData data)
        {
            container.Column(column =>
            {
                column.Spacing(8);
                column.Item().Text("Vuelo").Bold().FontSize(13);

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    AddHeaderCell(table, "Origen");
                    AddHeaderCell(table, "Destino");
                    AddHeaderCell(table, "Clase");

                    AddBodyCell(
                        table,
                        $"{ValueOrDash(data.DepartureCityName)} ({ValueOrDash(data.DepartureAirportCode)})\n{ValueOrDash(data.DepartureAirportName)}\n{FormatDateTime(data.DepartureAt)}");

                    AddBodyCell(
                        table,
                        $"{ValueOrDash(data.ArrivalCityName)} ({ValueOrDash(data.ArrivalAirportCode)})\n{ValueOrDash(data.ArrivalAirportName)}\n{FormatDateTime(data.ArrivalAt)}");

                    AddBodyCell(table, ValueOrDash(data.SeatClass));
                });
            });
        }

        private static void ComposeInvoiceLines(IContainer container, PurchaseOrderEmailData data)
        {
            container.Column(column =>
            {
                column.Spacing(8);
                column.Item().Text("Detalle de factura").Bold().FontSize(13);

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn();
                    });

                    AddHeaderCell(table, "Concepto");
                    AddHeaderCell(table, "Monto");

                    AddInvoiceRow(table, "Tarifa base", data.BaseFare);
                    AddInvoiceRow(table, "Impuestos y cargos", data.Taxes);
                    AddInvoiceRow(table, "Seguro de viaje", data.TravelInsurance);

                    table.Cell().Element(TotalCell).Text("Total").Bold();
                    table.Cell().Element(TotalCell).AlignRight().Text(FormatUsd(data.Total)).Bold();
                });
            });
        }

        private static void ComposePassengers(IContainer container, PurchaseOrderEmailData data)
        {
            container.Column(column =>
            {
                column.Spacing(8);
                column.Item().Text("Pasajeros").Bold().FontSize(13);

                if (data.Passengers.Count == 0)
                {
                    column.Item().Text("No hay pasajeros registrados.").FontColor(Colors.Grey.Darken1);
                    return;
                }

                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    AddHeaderCell(table, "Nombre");
                    AddHeaderCell(table, "Nacionalidad");
                    AddHeaderCell(table, "Nacimiento");

                    foreach (var passenger in data.Passengers)
                    {
                        AddBodyCell(table, $"{ValueOrDash(passenger.FirstName)} {ValueOrDash(passenger.LastName)}");
                        AddBodyCell(table, ValueOrDash(passenger.Nationality));
                        AddBodyCell(table, $"{ValueOrDash(passenger.BirthDay)} {ValueOrDash(passenger.BirthMonth)} {ValueOrDash(passenger.BirthYear)}");
                    }
                });
            });
        }

        private static void AddLabelValue(ColumnDescriptor column, string label, string value)
        {
            column.Item().Text(text =>
            {
                text.Span($"{label}: ").SemiBold();
                text.Span(ValueOrDash(value));
            });
        }

        private static void AddHeaderCell(TableDescriptor table, string value)
        {
            table.Cell().Element(HeaderCell).Text(value).SemiBold();
        }

        private static void AddBodyCell(TableDescriptor table, string value)
        {
            table.Cell().Element(BodyCell).Text(ValueOrDash(value));
        }

        private static void AddInvoiceRow(TableDescriptor table, string concept, string amount)
        {
            table.Cell().Element(BodyCell).Text(concept);
            table.Cell().Element(BodyCell).AlignRight().Text(FormatUsd(amount));
        }

        private static IContainer HeaderCell(IContainer container)
        {
            return container
                .Background(Colors.Grey.Lighten3)
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(6)
                .PaddingHorizontal(8);
        }

        private static IContainer BodyCell(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(7)
                .PaddingHorizontal(8);
        }

        private static IContainer TotalCell(IContainer container)
        {
            return container
                .BorderTop(1)
                .BorderColor(Colors.Grey.Darken1)
                .PaddingVertical(8)
                .PaddingHorizontal(8);
        }

        private static string FormatUsd(string amount)
        {
            return string.IsNullOrWhiteSpace(amount)
                ? "-"
                : $"${amount} USD";
        }

        private static string FormatDateTime(DateTime value)
        {
            return value == default
                ? "-"
                : value.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
        }

        private static string BuildFileName(PurchaseOrderEmailData data)
        {
            var invoiceId = string.IsNullOrWhiteSpace(data.ConfirmationCode)
                ? data.PurchaseOrderId.ToString(CultureInfo.InvariantCulture)
                : data.ConfirmationCode;

            return $"Factura-{SanitizeFileNamePart(invoiceId)}.pdf";
        }

        private static string SanitizeFileNamePart(string value)
        {
            var invalidCharacters = Path.GetInvalidFileNameChars();
            var builder = new StringBuilder(value.Length);

            foreach (var character in value)
            {
                builder.Append(invalidCharacters.Contains(character) ? '_' : character);
            }

            return builder.ToString();
        }

        private static string ValueOrDash(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }
    }
}

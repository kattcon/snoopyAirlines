namespace SnoopyAirlines.Domain
{
    public class AirlineDetailedReportRow
    {
        public DateOnly Fecha { get; set; }
        public string Origen { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public string? FlightCode { get; set; }
        public int PasajerosPrimeraClase { get; set; }
        public int PasajerosEconomia { get; set; }
        public string Aerolinea { get; set; } = string.Empty;
        public decimal VentaPasajeros { get; set; }
        public decimal VentaEquipajes { get; set; }
        public decimal TotalVenta { get; set; }
    }
}

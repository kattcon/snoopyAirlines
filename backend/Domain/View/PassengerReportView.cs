namespace SnoopyAirlines.Domain.View
{
    public class PassengerReportView
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Gender { get; set; } = null!;
        public string Nationality { get; set; } = null!;
        public string BirthDay { get; set; } = null!;
        public string BirthMonth { get; set; } = null!;
        public string BirthYear { get; set; } = null!;
        public int CarryOnLuggage { get; set; }
        public int CheckedLuggage { get; set; }
        public decimal CheckedLuggageTotalCost { get; set; }
    }
}

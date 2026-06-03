namespace snoopy_airlines_backend.Domain
{
    public class Passenger
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public string Gender { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BirthDay { get; set; }
        public string BirthMonth { get; set; }
        public string BirthYear { get; set; }
        public string Nationality { get; set; }
        public string HandLuggage { get; set; }
        public string CheckedLuggage { get; set; }
    }
}

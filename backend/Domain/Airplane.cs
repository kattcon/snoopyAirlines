namespace SnoopyAirlines.domain
{
    public class Airplane
    {
        public int Id { get; set; }
        required public string Model { get; set; }
        public int TouristRows { get; set; }
        public int TouristColumns { get; set; }
        public int FirstclassRows { get; set; }
        public int FirstclassColumns { get; set; }
        public float MaxWeight { get; set; }
    }
}

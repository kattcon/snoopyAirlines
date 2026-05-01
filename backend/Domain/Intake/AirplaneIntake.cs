namespace SnoopyAirlines.Domain.Intake
{
    public class AirplaneIntake
    {
        required public int ModelNumber { get; set; }
        required public int TouristRows { get; set; }
        required public int TouristColumns { get; set; }
        required public int FirstclassRows { get; set; }
        required public int FirstclassColumns { get; set; }
        required public float MaxWeight { get; set; }
    }
}

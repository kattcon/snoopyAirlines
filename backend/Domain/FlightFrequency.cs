namespace SnoopyAirlines.Domain
{
    public class FlightFrequency
    {
        private const byte MondayMask = 0b0100_0000;
        private const byte TuesdayMask = 0b0010_0000;
        private const byte WednesdayMask = 0b0001_0000;
        private const byte ThursdayMask = 0b0000_1000;
        private const byte FridayMask = 0b0000_0100;
        private const byte SaturdayMask = 0b0000_0010;
        private const byte SundayMask = 0b0000_0001;
        private const byte ValidDaysMask = 0b0111_1111;

        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }

        public static FlightFrequency FromByte(byte value)
        {
            value = (byte)(value & ValidDaysMask);

            return new FlightFrequency
            {
                Monday = (value & MondayMask) != 0,
                Tuesday = (value & TuesdayMask) != 0,
                Wednesday = (value & WednesdayMask) != 0,
                Thursday = (value & ThursdayMask) != 0,
                Friday = (value & FridayMask) != 0,
                Saturday = (value & SaturdayMask) != 0,
                Sunday = (value & SundayMask) != 0
            };
        }

        public byte ToByte()
        {
            byte value = 0;

            if (Monday) value |= MondayMask;
            if (Tuesday) value |= TuesdayMask;
            if (Wednesday) value |= WednesdayMask;
            if (Thursday) value |= ThursdayMask;
            if (Friday) value |= FridayMask;
            if (Saturday) value |= SaturdayMask;
            if (Sunday) value |= SundayMask;

            return value;
        }

        public bool HasAnyDay()
        {
            return (ToByte() & ValidDaysMask) != 0;
        }
    }
}

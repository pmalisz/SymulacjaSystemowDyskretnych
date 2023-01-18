namespace Backend.Consts
{
    public static class VehicleConsts
    {
        public const int LenghtInM = 30;

        public const int SafeSpaceInM = 50;

        public const float MaxSpeedInKmPerH = 25;

        public const float MaxSpeedInMPerS = MaxSpeedInKmPerH * 1000 / 3600;

        public const float MaxCrossSpeedInKmPerH = 10;

        public const float MaxCrossSpeedInMPerS = MaxCrossSpeedInKmPerH * 1000 / 3600;

        public const float TimeToMaxSpeedInS = 10;

        public const float AccelerationInMPerS2 = MaxSpeedInMPerS / TimeToMaxSpeedInS;
    }
}

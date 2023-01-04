using Backend.Consts;

namespace Backend.Commons.Helpers
{
    public static class PhysicsHelper
    {
        public static float GetNewSpeed(float oldSpeed, float deltaTime, bool increase) => 
            oldSpeed + (increase ? 1 : -1) * (deltaTime * VehicleConsts.AccelerationInMPerS2) * 3600 / 1000;

        public static float GetBrakingDistance(float speed)
        {
            float speedMS = speed * 1000 / 3600;
            return (speedMS * speedMS) / (2 * VehicleConsts.AccelerationInMPerS2);
        }

        public static float GetTranslation(float oldSpeed, float newSpeed, float deltaTime)
        {
            float newSpeedMS = newSpeed * 1000 / 3600;
            float oldSpeedMS = oldSpeed * 1000 / 3600;
            return deltaTime / 2 * (newSpeedMS + oldSpeedMS);
        }
    }
}

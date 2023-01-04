using Backend.Commons.Helpers;
using Microsoft.DirectX;

namespace Backend.Commons.Extensions
{
    public static class VectorExtensions
    {
        public static Vector3 ToVector3(this Vector2 v2)
        {
            return new Vector3(v2.X, v2.Y, 0);
        }

        public static float RealDistanceTo(this Vector2 v2, Vector2 coordinates)
        {
            return GeometryHelper.GetRealDistance(v2, coordinates);
        }
    }
}

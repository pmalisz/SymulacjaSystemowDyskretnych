using Backend.Consts;
using Microsoft.DirectX;
using System.Device.Location;
using System;

namespace Backend.Commons.Helpers
{
    public static class GeometryHelper
    {
        public static float GetDistance(Vector2 pA, Vector2 pB) =>
            (float)Math.Sqrt((pB.X - pA.X) * (pB.X - pA.X) + (pB.Y - pA.Y) * (pB.Y - pA.Y));

        public static float GetRealDistance(Vector2 pA, Vector2 pB)
        {
            var aCoord = new GeoCoordinate(pA.Y, pA.X);
            var bCoord = new GeoCoordinate(pB.Y, pB.X);
            return (float)aCoord.GetDistanceTo(bCoord);
        }

        public static Tuple<Vector2, Vector2> GetPerpendicularPointsWithDecimalOperations(float pAX, float pAY, float pBX, float pBY, float distance)
        {
            if (Math.Abs(pAY - pBY) < CalculationConsts.Epsilon)
                return Tuple.Create(new Vector2(pAX, pAY - distance), new Vector2(pAX, pAY + distance));

            if (Math.Abs(pAX - pBX) < CalculationConsts.Epsilon)
                return Tuple.Create(new Vector2(pAX - distance, pAY), new Vector2(pAX + distance, pAY));

            decimal aAB = (decimal)(pBY - pAY) / (decimal)(pBX - pAX);
            decimal aAC = -1 / aAB;
            decimal bAC = (decimal)pAY - (aAC * (decimal)pAX);

            decimal xA = 1 + aAC * aAC;
            decimal xB = -2 * (decimal)pAX + (2) * aAC * (bAC - (decimal)pAY);
            decimal xC = (decimal)pAX * (decimal)pAX + (bAC - (decimal)pAY) * (bAC - (decimal)pAY) - (decimal)distance * (decimal)distance;
            decimal delta = xB * xB - 4 * xA * xC;

            float x1 = (float)((-xB - (decimal)Math.Sqrt((double)delta)) / (2 * xA));
            float x2 = (float)((-xB + (decimal)Math.Sqrt((double)delta)) / (2 * xA));

            float y1 = (float)(aAC * (decimal)x1 + bAC);
            float y2 = (float)(aAC * (decimal)x2 + bAC);

            return Tuple.Create(new Vector2(x1, y1), new Vector2(x2, y2));
        }

        public static Vector2 GetLocactionBetween(float displacment, Vector2 pA, Vector2 pB)
        {
            if (Math.Abs(pA.X - pB.X) < CalculationConsts.Epsilon)
            {
                double d = GetDistance(pA, pB) * displacment / 100;
                return new Vector2(pA.X, pA.Y > pB.Y ? pA.Y - (float)d : pA.Y + (float)d);
            }
            
            if (Math.Abs(pA.Y - pB.Y) < CalculationConsts.Epsilon)
            {
                double d = GetDistance(pA, pB) * displacment / 100;
                return new Vector2(pA.X > pB.X ? pA.X - (float)d : pA.X + (float)d, pA.Y);
            }

            float x = Math.Abs(pB.X - pA.X) * displacment / 100;
            float y = Math.Abs(pB.Y - pA.Y) * displacment / 100;
            float cX = pA.X > pB.X ? pA.X - x : pA.X + x;
            float cY = pA.Y > pB.Y ? pA.Y - y : pA.Y + y;

            return new Vector2(cX, cY);
        }
    }
}

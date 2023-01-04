using Backend.Commons.Extensions;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using System;

namespace Backend.Commons.Helpers
{
    public static class DirectxHelper
    {
        public static CustomVertex.PositionColored[] CreateCircle(float pX, float pY, int color, float radius, int precision)
        {
            var vertex = new CustomVertex.PositionColored[precision + 1];

            float wedgeAngle = (float)(2 * Math.PI / precision);
            for (int i = 0; i <= precision; i++)
            {
                float theta = i * wedgeAngle;
                vertex[i].Position = new Vector3((float)(pX + radius * Math.Cos(theta)),
                                                 (float)(pY - radius * Math.Sin(theta)),
                                                 0);
            }

            for (int i = 0; i < vertex.Length; i++)
                vertex[i].Color = color;

            return vertex;
        }

        public static CustomVertex.PositionColored[] CreateLine(float pX1, float pY1, float pX2, float pY2, int color, float thickness)
        {
            var aLine = GeometryHelper.GetPerpendicularPointsWithDecimalOperations(pX1, pY1, pX2, pY2, thickness);
            var bLine = GeometryHelper.GetPerpendicularPointsWithDecimalOperations(pX2, pY2, pX1, pY1, thickness);

            var vertex = new CustomVertex.PositionColored[4];
            vertex[0].Position = aLine.Item1.ToVector3();
            vertex[1].Position = aLine.Item2.ToVector3();
            vertex[2].Position = bLine.Item1.ToVector3();
            vertex[3].Position = bLine.Item2.ToVector3();

            for (int i = 0; i < vertex.Length; i++)
                vertex[i].Color = color;

            return vertex;
        }
    }
}

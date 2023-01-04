using System.Drawing;

namespace Backend.Consts
{
    public static class VisualizationConsts
    {
        #region Camera Consts
        public const float ZoomOffset = 5;

        public const float SwipeOffset = 0.0005f;

        public const float StartCameraZ = 125;
        #endregion

        #region Drawing Consts
        public const float PointRadius = 0.0045f;

        public const int PointPrecision = 24;
        #endregion

        #region Color Consts
        public static readonly Color BasicLineColor = Color.LightSkyBlue;

        public static readonly Color UndergroundLineColor = Color.Black;

        public static readonly Color StopColor = Color.Red;
        #endregion
    }
}

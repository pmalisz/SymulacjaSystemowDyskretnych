using System.Windows.Forms;
using System.Drawing;
using System;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using Backend.Controllers;
using Backend.Consts;

namespace Visualization
{
    public partial class MainForm : Form
    {
        private MainController controller;
        private Device device;

        private Vector3 cameraPosition, cameraTarget;
        private Point lastClickedMouseLocation;

        public MainForm()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);

            cameraPosition = new Vector3(0, 0, VisualizationConsts.StartCameraZ);
            cameraTarget = new Vector3(0, 0, 0);
        }

        public void Init(MainController controller)
        {
            this.controller = controller;
            InitializeGraphics();

            rerenderTimer.Interval = (int)(ApplicationConsts.TimeIntervalInS * 1000);
            rerenderTimer.Start();
        }

        public void Render(Action<Device, Vector3> renderAction)
        {
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, renderPanel.Width / renderPanel.Height, 1f, 1000f);
            device.Transform.View = Matrix.LookAtLH(cameraPosition, cameraTarget, new Vector3(0, 1, 0));
            device.RenderState.Lighting = false;
            device.RenderState.CullMode = Cull.None;

            device.Clear(ClearFlags.Target, Color.WhiteSmoke, 1.0f, 0);

            device.BeginScene();

            device.VertexFormat = CustomVertex.PositionColored.Format;

            renderAction(device, cameraPosition);

            device.EndScene();
            device.Present();
            Invalidate();
        }

        private bool InitializeGraphics()
        {
            try
            {
                PresentParameters presentParams = new PresentParameters();
                presentParams.Windowed = true;
                presentParams.SwapEffect = SwapEffect.Discard;
                device = new Device(0, DeviceType.Hardware, renderPanel, CreateFlags.MixedVertexProcessing, presentParams);
                return true;
            }
            catch (DirectXException)
            {
                return false;
            }
        }

        private void rerenderTimer_Tick(object sender, EventArgs e)
        {
            controller.Update();
            Render(controller.Render);
        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            rerenderTimer.Stop();
        }

        private void renderPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
                ZoomIn();
            else if (e.Delta < 0)
                ZoomOut();
        }

        private void ZoomIn()
        {
            if (cameraPosition.Z > VisualizationConsts.ZoomOffset)
                cameraPosition.Z -= VisualizationConsts.ZoomOffset;
            else if (cameraPosition.Z > 1 + (VisualizationConsts.ZoomOffset / 20))
                cameraPosition.Z -= VisualizationConsts.ZoomOffset / 20;
        }

        private void ZoomOut()
        {
            if (cameraPosition.Z < VisualizationConsts.ZoomOffset)
                cameraPosition.Z += VisualizationConsts.ZoomOffset / 20;
            else if (cameraPosition.Z < VisualizationConsts.StartCameraZ * 1.5)
                cameraPosition.Z += VisualizationConsts.ZoomOffset;

            if (cameraPosition.Z > VisualizationConsts.StartCameraZ * 1.5)
                cameraPosition.Z = VisualizationConsts.StartCameraZ * 1.5f;
        }

        private void renderPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button.HasFlag(MouseButtons.Left))
            {
                if (!lastClickedMouseLocation.IsEmpty && lastClickedMouseLocation != e.Location)
                {
                    cameraPosition.X += GetCoordinateChange(lastClickedMouseLocation.X, e.Location.X);
                    cameraPosition.Y += GetCoordinateChange(lastClickedMouseLocation.Y, e.Location.Y);

                    cameraTarget.X = cameraPosition.X;
                    cameraTarget.Y = cameraPosition.Y;
                }

                lastClickedMouseLocation = e.Location;
            }
            else
            {
                lastClickedMouseLocation = Point.Empty;
            }
        }

        private float GetCoordinateChange(int lastClickedCoordinate, int currentCoordinate)
        {
            float difference = Math.Abs(lastClickedCoordinate - currentCoordinate);
            float scaledDifference = VisualizationConsts.SwipeOffset * cameraPosition.Z * difference;

            return lastClickedCoordinate > currentCoordinate ? -1 * scaledDifference : scaledDifference;
        }
    }
}

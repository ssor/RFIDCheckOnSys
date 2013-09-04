using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace LogisTechBase
{
    public partial class MatrixCircularProgressControl : UserControl
    {
        #region Constants

        private const int DEFAULT_INTERVAL = 60;
        private readonly Color DEFAULT_TICK_COLOR = Color.FromArgb(58, 58, 58);
        private const int DEFAULT_TICK_WIDTH = 2;
        private const int MINIMUM_INNER_RADIUS = 4;
        private const int MINIMUM_OUTER_RADIUS = 8;
        private Size MINIMUM_CONTROL_SIZE = new Size(28, 28);
        private const int MINIMUM_PEN_WIDTH = 2;
        private const float INNER_RADIUS_FACTOR = 0.175F;
        private const float OUTER_RADIUS_FACTOR = 0.3125F;

        #endregion

        #region Enums

        public enum Direction
        {
            CLOCKWISE,
            ANTICLOCKWISE
        }

        #endregion

        #region Structs

        struct Spoke
        {
            public PointF StartPoint;
            public PointF EndPoint;

            public Spoke(PointF pt1, PointF pt2)
            {
                StartPoint = pt1;
                EndPoint = pt2;
            }
        }

        #endregion

        #region Members

        private int m_Interval;
        Pen m_Pen = null;
        PointF m_CentrePt = new PointF();
        int m_InnerRadius = 0;
        int m_OuterRadius = 0;
        int m_SpokesCount = 0;
        int m_AlphaChange = 0;
        int m_AlphaLowerLimit = 0;
        float m_StartAngle = 0;
        float m_AngleIncrement = 0;
        Direction m_Rotation;
        System.Timers.Timer m_Timer = null;
        List<Spoke> m_Spokes = null;

        #endregion

        #region Properties

        /// <summary>
        /// Time interval for each tick.
        /// </summary>
        public int Interval
        {
            get
            {
                return m_Interval;
            }
            set
            {
                if (value > 0)
                {
                    m_Interval = value;
                }
                else
                {
                    m_Interval = DEFAULT_INTERVAL;
                }
            }
        }

        /// <summary>
        /// Color of the tick
        /// </summary>
        public Color TickColor { get; set; }

        /// <summary>
        /// Direction of rotation - CLOCKWISE/ANTICLOCKWISE
        /// </summary>
        public Direction Rotation
        {
            get
            {
                return m_Rotation;
            }
            set
            {
                m_Rotation = value;
                CalculateSpokesPoints();
            }
        }

        /// <summary>
        /// Angle at which the tick should start
        /// </summary>
        public float StartAngle
        {
            get
            {
                return m_StartAngle;
            }
            set
            {
                m_StartAngle = value;
            }
        }

        #endregion

        #region Construction/Initialization

        /// <summary>
        /// Ctor
        /// </summary>
        public MatrixCircularProgressControl()
        {
            this.DoubleBuffered = true;

            InitializeComponent();

            // Set Default Values
            this.BackColor = Color.Transparent;
            this.TickColor = DEFAULT_TICK_COLOR;
            this.MinimumSize = MINIMUM_CONTROL_SIZE;
            this.Interval = DEFAULT_INTERVAL;
            // Default starting angle is 12 o'clock
            this.StartAngle = 270;
            // Default number of Spokes in this control is 12
            m_SpokesCount = 12;
            // Set the Lower limit of the Alpha value (The spokes will be shown in 
            // alpha values ranging from 255 to m_AlphaLowerLimit)
            m_AlphaLowerLimit = 15;

            // Create the Pen
            m_Pen = new Pen(TickColor, DEFAULT_TICK_WIDTH);
            m_Pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            m_Pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;

            // Default rotation direction is clockwise
            this.Rotation = Direction.CLOCKWISE;

            // Calculate the Spoke Points
            CalculateSpokesPoints();

            m_Timer = new System.Timers.Timer(this.Interval);
            m_Timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerElapsed);
        }

        /// <summary>
        /// Calculate the Spoke Points and store them
        /// </summary>
        private void CalculateSpokesPoints()
        {
            m_Spokes = new List<Spoke>();

            // Calculate the angle between adjacent spokes
            m_AngleIncrement = (360 / (float)m_SpokesCount);
            // Calculate the change in alpha between adjacent spokes
            m_AlphaChange = (int)((255 - m_AlphaLowerLimit) / m_SpokesCount);

            // Calculate the location around which the spokes will be drawn
            int width = (this.Width < this.Height) ? this.Width : this.Height;
            m_CentrePt = new PointF(this.Width / 2, this.Height / 2);
            // Calculate the width of the pen which will be used to draw the spokes
            m_Pen.Width = (int)(width / 15);
            if (m_Pen.Width < MINIMUM_PEN_WIDTH)
                m_Pen.Width = MINIMUM_PEN_WIDTH;
            // Calculate the inner and outer radii of the control. The radii should not be less than the
            // Minimum values
            m_InnerRadius = (int)(width * INNER_RADIUS_FACTOR);
            if (m_InnerRadius < MINIMUM_INNER_RADIUS)
                m_InnerRadius = MINIMUM_INNER_RADIUS;
            m_OuterRadius = (int)(width * OUTER_RADIUS_FACTOR);
            if (m_OuterRadius < MINIMUM_OUTER_RADIUS)
                m_OuterRadius = MINIMUM_OUTER_RADIUS;

            float angle = 0;

            for (int i = 0; i < m_SpokesCount; i++)
            {
                PointF pt1 = new PointF(m_InnerRadius * (float)Math.Cos(ConvertDegreesToRadians(angle)), m_InnerRadius * (float)Math.Sin(ConvertDegreesToRadians(angle)));
                PointF pt2 = new PointF(m_OuterRadius * (float)Math.Cos(ConvertDegreesToRadians(angle)), m_OuterRadius * (float)Math.Sin(ConvertDegreesToRadians(angle)));

                // Create a spoke based on the points generated
                Spoke spoke = new Spoke(pt1, pt2);
                // Add the spoke to the List
                m_Spokes.Add(spoke);

                if (Rotation == Direction.CLOCKWISE)
                {
                    angle -= m_AngleIncrement;
                }
                else if (Rotation == Direction.ANTICLOCKWISE)
                {
                    angle += m_AngleIncrement;
                }
            }
        }

        #endregion

        #region EventHandlers

        /// <summary>
        /// Handler for the event when the size of the control changes
        /// </summary>
        /// <param name="e">EventArgs</param>
        protected override void OnClientSizeChanged(EventArgs e)
        {
            CalculateSpokesPoints();
        }

        /// <summary>
        /// Handle the Tick event
        /// </summary>
        /// <param name="sender">Timer</param>
        /// <param name="e">ElapsedEventArgs</param>
        void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Rotation == Direction.CLOCKWISE)
            {
                m_StartAngle += m_AngleIncrement;

                if (m_StartAngle >= 360)
                    m_StartAngle = 0;
            }
            else if (Rotation == Direction.ANTICLOCKWISE)
            {
                m_StartAngle -= m_AngleIncrement;

                if (m_StartAngle <= -360)
                    m_StartAngle = 0;
            }

            Invalidate();
        }


        /// <summary>
        /// Handles the Paint Event of the control
        /// </summary>
        /// <param name="e">PaintEventArgs</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            // Perform a Translation so that the centre of the control is the centre of Rotation
            e.Graphics.TranslateTransform(m_CentrePt.X, m_CentrePt.Y, System.Drawing.Drawing2D.MatrixOrder.Prepend);
            // Perform a Rotation about the control's centre
            e.Graphics.RotateTransform(m_StartAngle, System.Drawing.Drawing2D.MatrixOrder.Prepend);

            int alpha = 255;

            // Render the spokes
            for (int i = 0; i < m_SpokesCount; i++)
            {
                m_Pen.Color = Color.FromArgb(alpha, this.TickColor);
                e.Graphics.DrawLine(m_Pen, m_Spokes[i].StartPoint, m_Spokes[i].EndPoint);

                alpha -= m_AlphaChange;
                if (alpha < m_AlphaLowerLimit)
                    alpha = 255 - m_AlphaChange;
            }

            // Perform a reverse Rotation and Translation to obtain the original Transformation
            e.Graphics.RotateTransform(-m_StartAngle, System.Drawing.Drawing2D.MatrixOrder.Append);
            e.Graphics.TranslateTransform(-m_CentrePt.X, -m_CentrePt.Y, System.Drawing.Drawing2D.MatrixOrder.Append);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Converts Degrees to Radians
        /// </summary>
        /// <param name="degrees">Degrees</param>
        /// <returns></returns>
        private double ConvertDegreesToRadians(float degrees)
        {
            return ((Math.PI / (double)180) * degrees);
        }

        #endregion

        #region APIs

        /// <summary>
        /// Start the Tick Control rotation
        /// </summary>
        public void Start()
        {
            if (m_Timer != null)
            {
                m_Timer.Interval = this.Interval;
                m_Timer.Enabled = true;
            }
        }

        /// <summary>
        /// Stop the Tick Control rotation
        /// </summary>
        public void Stop()
        {
            if (m_Timer != null)
            {
                m_Timer.Enabled = false;
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModuloVisualizacionDatos
{
    public partial class HighlightTool : PictureBox
    {

        public List<Rectangle> rectangles = new List<Rectangle>();

        public Rectangle rect;

        public struct NormRect
        {
            public float Left;
            public float Bottom;
            public float Right;
            public float Top;
        }

        public struct PageSize
        {
            
            public float Right;
            public float Top;
        }
        public NormRect normRect;
        public NormRect pageSize;

        Point StartLocation;
        Point EndLcation;
        bool IsMouseDown = false;
        bool markPoint = true;
        bool highLightOn = false;
        int highlightHeight = 30;
        int nClick;

        public int NClick
        {
            get { return nClick; }
            set { NClick = value; }
        }

        public bool HighLightOn
        {
            get { return highLightOn; }
            set { highLightOn = value; }
        }

        public HighlightTool(int highlightHeight, int width, int height)
        {
            Size = new Size(width, height);
            BackColor = Color.FromArgb(255, 255, 255);
            this.highlightHeight = highlightHeight;
            nClick = 0;
            

            InitializeComponent();
            
        }

        public delegate void NumClicksEventHandler();
        public event NumClicksEventHandler NumClicks;


        public void RuntimeResize(int x, int y, int width, int height)
        {
            Location = new Point(x, y);
            Size = new Size(width, height);
            Refresh();
        }

        public void RuntimeFinalLocation(int x, int y)
        {
            
            Size = new Size(x - Location.X, y - Location.Y);
            Refresh();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            IsMouseDown = true;
            StartLocation = e.Location;
            //textBox1.AppendText("Mouse precionada \n");
            nClick += 1;
            
        }


        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            if (rect != null && highLightOn)
            {
                //e.Graphics.DrawRectangle(Pens.Red, GetRectangle());
                pe.Graphics.FillRectangle(Brushes.Yellow, GetRectangle(highlightHeight));
                PaintHighlight(pe);
            }

            if (rectangles.Count > 0 && highLightOn)
            {
                foreach (var item in rectangles)
                {
                    //e.Graphics.DrawRectangle(Pens.Red, GetRectangle());
                    pe.Graphics.FillRectangle(Brushes.Yellow, item);
                }
                
            }
        }

        private void PaintHighlight(PaintEventArgs pe)
        {
            foreach (var item in rectangles)
            {
                pe.Graphics.FillRectangle(Brushes.Yellow, item);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {

            if (IsMouseDown == true)
            {
                EndLcation = e.Location;
                Refresh();

                //textBox1.AppendText("Mouse buscando \n" + e.Location);
            }
        }

        public Rectangle GetRectangle(int HighlightHeight)
        {
          
            rect = new Rectangle();
            rect.X = Math.Min(StartLocation.X, EndLcation.X);
            rect.Y = Math.Min(StartLocation.Y, EndLcation.Y);
            rect.Width = Math.Abs(StartLocation.X - EndLcation.X);
            //rect.Height = Math.Abs(StartLocation.Y - EndLcation.Y);
            rect.Height = Math.Abs(HighlightHeight);
            normRect = new NormRect();
            normRect.Left = (float)rect.X / (float)Width;
            normRect.Bottom = (float)rect.Y / (float)Height;
            normRect.Right = (float)rect.Width / (float)Width;
            //rect.Height = Math.Abs(StartLocation.Y - EndLcation.Y);
            normRect.Top = (float)rect.Height / (float)Height;


            return rect;
        }

        public void GetRectangles(List<iTextSharp.text.Rectangle> RectangleIText)
        {

            rectangles.Clear();
            rectangles = new List<Rectangle>();
            foreach (var item in RectangleIText)
            {
                
                var rect = new Rectangle();
                var nRect = new NormRect();
                nRect.Left = (item.Left/pageSize.Right) *this.Width;
                nRect.Bottom = (1-(item.Bottom / pageSize.Top)- (item.Height / pageSize.Top)) * this.Height;
                nRect.Right = ((item.Width / pageSize.Right) )* this.Width;
                nRect.Top = ((item.Height/ pageSize.Top)) * this.Height;
                rect.X = (int)(nRect.Left);
                rect.Y = (int)(nRect.Bottom);
                rect.Width = (int)(nRect.Right);
                rect.Height = (int)(nRect.Top);
                rectangles.Add(rect);
            }
           
        }

        public Rectangle GetRectangle()
        {

            rect = new Rectangle();
            rect.X = Math.Min(StartLocation.X, EndLcation.X);
            rect.Y = Math.Min(StartLocation.Y, EndLcation.Y);
            rect.Width = Math.Abs(StartLocation.X - EndLcation.X);
            rect.Height = Math.Abs(StartLocation.Y - EndLcation.Y);
            

            return rect;
        }

    

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (IsMouseDown == true)
            {
                EndLcation = e.Location;
                IsMouseDown = false;
                if (rect != null)
                {
                    /*imgInput.ROI = rect; //Enviar imagen a box2
                    Image<Bgr, byte> temp = imgInput.CopyBlank();
                    imgInput.CopyTo(temp);
                    imgInput.ROI = Rectangle.Empty;
                    pictureBox2.Image = temp.Bitmap;*/
                }

                if (nClick >= 1)
                {
                    NumClicks();
                    nClick = 0;
                }
            }
        }
    }
}

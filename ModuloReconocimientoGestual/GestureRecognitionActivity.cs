using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Windows.Forms;

namespace ModuloReconocimientoGestual
{


    public class GestureRecognitionActivity
    {

        public delegate void selectRectagleEventHandler(Rectangle rectangle);
        public event selectRectagleEventHandler selectedRectangle;

        float sensorStart;
        float sensorEnd;
        float appEnd;
        float appStart;
        float sensorStarty;
        float sensorEndy;
        float appEndy;
        float appStarty;

        Image<Bgr, byte> imageOut;

        IPlugin plugin;

        bool activeX = false;
        bool activeY = false;

        int xFinal = 0;
        int yFinal = 0;

        int sensorX;
        public int SensorX
        {
            get { return sensorX; }
            
        }

        int sensorY;
        public int SensorY
        {
            get { return sensorY; }

        }


        bool clickDown = true;

        Rectangle rectangularSelection;

        int respX, respY;

        public IPlugin Plugin
        {
            get { return plugin; }
            set { plugin = value; }
        }

        public bool ActiveX
        {
            set { activeX = value;}
        }

        public bool ActiveY
        {
            set { activeY = value; }
        }

        public Rectangle RectangularSelection
        {
            get { return rectangularSelection; }
        }



       
        public Image<Bgr, byte> Capture_ImageGrabbed(VideoCapture captureGesture,float SStartX, float SEndX, float AStartX, float AEndX, float SStartY, float SEndY, float AStartY, float AEndY)
        {
            //try
            //{
                Mat m = new Mat();
                captureGesture.Retrieve(m);

                if (plugin == null)
                {
                    return m.ToImage<Bgr, byte>();
                }
                else
                {
                    imageOut = plugin.RunPlugin(captureGesture).ToImage<Bgr, byte>();
                    sensorX = plugin.Center.X;
                    sensorY = plugin.Center.Y;

                    if (plugin.DetectGesture)
                    {
                        sensorStart = SStartX;
                        sensorEnd = SEndX;
                        appStart = AStartX;
                        appEnd = AEndX;

                        sensorStarty = (float)SStartY;
                        sensorEndy = (float)SEndY;
                        appStarty = (float)AStartY;
                        appEndy = (float)AEndY;


                      

                        if (activeX)
                        {
                            xFinal = (int)Math.Abs((plugin.Center.X - sensorStart) * ((appEnd - appStart) / (sensorEnd - sensorStart)) + appStart);
                        }
                        else
                        {
                            xFinal = plugin.Center.X;
                        }

                        if (activeY)
                        {
                            yFinal = (int)Math.Abs((plugin.Center.Y - sensorStarty) * ((appEndy - appStarty) / (sensorEndy - sensorStarty)) + appStarty);
                        }
                        else
                        {
                            yFinal = 500;
                        }

                        MouseCursor.MoveCursor(xFinal, yFinal);

                        if (!clickDown && plugin.AutoClick)
                        {
                            Clicking.SendClick(xFinal, yFinal);
                            Console.WriteLine("     Gesture recognition Click down (autoclick)");
                            clickDown = true;
                            respX = plugin.Center.X;
                            respY = plugin.Center.Y;
                        }

                        if (!plugin.AutoClick)
                        {
                        if (plugin.DetectClick && clickDown) {
                            Clicking.SendClick(xFinal, yFinal);
                            Console.WriteLine("     Gesture recognition Click down");
                            clickDown = false;
                        }
                        if (!plugin.DetectClick && !clickDown)
                        {
                            Clicking.SendUpClick(xFinal, yFinal);
                            Console.WriteLine("     Gesture recognition Click up");
                            clickDown = true;
                        }

                    }
                }
                    else
                    {
                        if (clickDown && plugin.AutoClick)
                        {

                            Clicking.SendUpClick(xFinal, yFinal);
                            Console.WriteLine("     Gesture recognition Click up(autoclick)");
                        
                            if (plugin.AutoCamCapture)
                            {
                                //Disparo de evento al finalizar una seleccion rectangular
                                // si el plugin AutoCamCapture lo permite
                                rectangularSelection.X = plugin.Center.X;
                                rectangularSelection.Y = plugin.Center.Y;
                                rectangularSelection.Width = Math.Abs(respX - plugin.Center.X);
                                rectangularSelection.Height = Math.Abs(respY - plugin.Center.Y);
                                selectedRectangle(rectangularSelection);
                            }
                            clickDown = false;

                        }
                    }
                }

                return imageOut;

        }

        class MouseCursor
        {
            [DllImport("user32.dll")]

            private static extern bool SetCursorPos(int x, int y);

            public static void MoveCursor(int x, int y)
            {

                SetCursorPos(x, y);
            }

        }

        class Clicking
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

            public static void SendClick(int x, int y)
            {

                mouse_event(0x0002, 0, x, y, 0);
            }
            public static void SendUpClick(int x, int y)
            {

                mouse_event(0x0004, 0, x, y, 0);
            }


        }
    }
}


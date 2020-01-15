using System;
using System.Drawing;
using ModuloReconocimientoGestual;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using System.Windows.Forms;
using HandGestureRecognition.SkinDetector;

namespace HandSkinRecognition
{
    public class HandSkinRecognition : IPlugin
    {
        public string Name => "Hand Skin Recognition";


        Point centerSensor;
        public Point Center
        {
            get
            {
                return centerSensor;
            }
        }

        private bool detectGesture = false;
        public bool DetectGesture
        {
            get
            {
                return detectGesture;
            }
        }

        // plugin/complememto permite (true) o no 
        //click automatico o detectado
        private bool autoClick = false;
        public bool AutoClick
        {
            get
            {
                return autoClick;
            }
        }

        private bool detectClick = false;
        public bool DetectClick
        {
            get
            {
                return detectClick;
            }
        }

        // plugin/complememto permite (true) o no 
        //captura automatica de imagenes por camara
        private bool autoCamCapture = false;
        public bool AutoCamCapture
        {
            get
            {
                return autoCamCapture;
            }
        }

        public Mat RunPlugin(VideoCapture src)
        {

            capture = src;

            return DetectHandSkin();
        }

        private static VideoCapture capture;
        private static Mat imagen = new Mat();
        private static Mat imagenOut = new Mat();
        Mat picture = new Mat();

        //Color para graficas en el image (Mat) de salida
        private static MCvScalar drawingColor = new Bgr(Color.Red).MCvScalar;

        //Espacio de color YCbCr,  Y representan la componente de luma 
        // las señales CB y CR son los componentes de crominancia diferencia de azul y diferencia de rojo
        private static Ycc YCrCb_min = new Ycc(0, 131, 80);        
        private static Ycc YCrCb_max = new Ycc(255, 185, 135);
        private static YCrCbSkinDetector skinDetector;

        private int gestualNum;
        private int gestualNumRepite = 0;


        private  Mat DetectHandSkin()
        {
            if (capture == null)
            {
                return null;
            }
                        
                Mat m = new Mat();
                Mat n = new Mat();
                Mat o = new Mat();
                Mat binaryDiffFrame = new Mat();
                Mat denoisedDiffFrame = new Mat();
                Mat finalFrame = new Mat();
             

                capture.Read(m);
            Image<Bgr, byte> mr = m.ToImage<Bgr, byte>();
            mr = mr.Rotate(180, new Bgr(255, 255, 255), false);
            m = mr.Mat;

            if (!m.IsEmpty)
                {

                   Image<Bgr, byte> ret = m.ToImage<Bgr, byte>();
                

                skinDetector = new YCrCbSkinDetector();

                Image<Gray, Byte> skin = skinDetector.DetectSkin(ret, YCrCb_min, YCrCb_max);
                m.CopyTo(finalFrame);
                //DetectObject(skin.Mat, finalFrame);
                //return DetectObject(denoisedDiffFrame, finalFrame);
                return DetectObject(skin.Mat, finalFrame);
            }
            else
                {
                    // break;
                }             

            return null;
        }
      


        private static void WriteMultilineText(Mat frame, string[] lines, Point origin)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                int y = i * 10 + origin.Y; // Mueve havia abajo las lineas
                CvInvoke.PutText(frame, lines[i], new Point(origin.X, y), FontFace.HersheyPlain, 0.8, drawingColor);
            }
        }

        private static void MarkDetectedObject(Mat frame, VectorOfPoint contour, double area)
        {
            // Recibe el rectangulo minimo en un contour
            Rectangle box = CvInvoke.BoundingRectangle(contour);

            // Dibuja un contour y una caja alrededor
            CvInvoke.Polylines(frame, contour, true, drawingColor);
            CvInvoke.Rectangle(frame, box, drawingColor);

            // Write information next to marked object
            Point center = new Point(box.X + box.Width / 2, box.Y + box.Height / 2);

            var info = new string[] {
                $"Area: {area}",
                $"Position: {center.X}, {center.Y}"
            };

            WriteMultilineText(frame, info, new Point(box.Right + 5, center.Y));
        }

        private Mat DetectObject(Mat detectionFrame, Mat displayFrame)
        {
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                detectGesture = false;
                VectorOfPoint biggestContour = null;
                IOutputArray hirarchy = null;
                // Crear lista de contornos
                CvInvoke.FindContours(detectionFrame, contours, hirarchy, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                // Selecciona el contour mas grande
                if (contours.Size > 0)
                {
                    
                    double maxArea = 0;
                    int chosen = 0;
                    VectorOfPoint contour = null;
                    for (int i = 0; i < contours.Size; i++)
                    {
                        contour = contours[i];

                        double area = CvInvoke.ContourArea(contour);
                        if (area > maxArea)
                        {
                            maxArea = area;
                            chosen = i;
                        }
                    }

                    // Dibuja un frame
                    MarkDetectedObject(displayFrame, contours[chosen], maxArea);

                    VectorOfPoint hullPoints = new VectorOfPoint();
                    VectorOfInt hullInt = new VectorOfInt();

                    CvInvoke.ConvexHull(contours[chosen], hullPoints, true);
                    CvInvoke.ConvexHull(contours[chosen], hullInt, false);

                    Mat defects = new Mat();

                    if (hullInt.Size > 3)
                        detectGesture = true;
                    CvInvoke.ConvexityDefects(contours[chosen], hullInt, defects);
           
                    Rectangle box = CvInvoke.BoundingRectangle(hullPoints);
                    CvInvoke.Rectangle(displayFrame, box, drawingColor);

                    Point center = new Point(box.X + box.Width / 2, box.Y + box.Height / 2);

                    VectorOfPoint start_points = new VectorOfPoint();
                    VectorOfPoint far_points = new VectorOfPoint();

                    if (!defects.IsEmpty)
                    {
                        //Los datos del Mat no se pueden leer directamente, por lo que los convertimos a Matrix<>
                        Matrix<int> m = new Matrix<int>(defects.Rows, defects.Cols,
                           defects.NumberOfChannels);
                        defects.CopyTo(m);
                        gestualNum = 0;
                        int x = int.MaxValue, y = int.MaxValue;
                        for (int i = 0; i < m.Rows; i++)
                        {
                            int startIdx = m.Data[i, 0];
                            int endIdx = m.Data[i, 1];
                            int farIdx = m.Data[i, 2];
                            Point startPoint = contours[chosen][startIdx];
                            Point endPoint = contours[chosen][endIdx];
                            Point farPoint = contours[chosen][farIdx];
                            CvInvoke.Circle(displayFrame, endPoint, 3, new MCvScalar(0, 255, 255));
                            CvInvoke.Circle(displayFrame, startPoint, 3, new MCvScalar(255, 255, 0));

                            //if (true)
                            //{
                                if (endPoint.Y < y)
                                {
                                    x = endPoint.X;
                                    y = endPoint.Y;
                                    
                                }

                            //}

                            double distance = Math.Round(Math.Sqrt(Math.Pow((center.X - farPoint.X), 2) + Math.Pow((center.Y - farPoint.Y), 2)), 1);
                            if (distance < box.Height * 0.3)
                            {

                                CvInvoke.Circle(displayFrame, farPoint, 10, new MCvScalar(255, 0, 0),4);
                                gestualNum++;
                            }
                            //dibuja una línea que conecta el punto de inicio del defecto de convexidad y el punto final en una línea roja
                            CvInvoke.Line(displayFrame, startPoint, endPoint, new MCvScalar(0, 255, 255));
                            
                        }
                        
                        if (gestualNum >= 4)
                        {
                            //Console.WriteLine("numero gestual 4");
                            gestualNumRepite++;
                        }
                        else
                        {
                            gestualNumRepite = 0;
                        }
                        if (gestualNumRepite == 3)
                        {
                            Console.WriteLine("numero gestual 5 Click");
                            gestualNumRepite=0;

                            if (detectClick == true)
                            {
                                detectClick = false;
                            }
                            else
                            {
                                detectClick = true;
                            }
                        }
                        Console.WriteLine("numero gestual " + gestualNum);
                        //var info = new string[] { $"Puntero", $"Posicion: {x}, {y}" };

                        //WriteMultilineText(displayFrame, info, new Point(x + 30, y));
                        centerSensor.X = x;
                        centerSensor.Y = y;
                        CvInvoke.Circle(displayFrame, new Point(x, y), 20, new MCvScalar(255, 0, 255), 2);
                        //CvInvoke.Circle(picture, new Point(x * 2, y * 4), 20, new MCvScalar(255, 0, 255), 2);
                        return displayFrame;

                    }
                   // detectGesture = false;
                 //  return displayFrame;

                }
                
                return displayFrame;
            }
        }
    }
}

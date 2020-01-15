using System;
using System.Drawing;
using ModuloReconocimientoGestual;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using System.Windows.Forms;

namespace ColorPenRecognition
{
    public class ColorPenRecognition : IPlugin
    {
        public string Name => "Color Pen Recognition";

        public Point Center
        {
            get
            {
                return center;
            }
        }

        public bool DetectGesture
        {
            get
            {
                return detectGesture;
            }
        }

        private bool autoClick = true;
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

        private bool autoCamCapture = true;
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
         
            return DetectPen();
        }

        private static VideoCapture capture;
       
        private static Mat imagen = new Mat();
        private static Mat imagenOut = new Mat();
       
      
        Mat picture = new Mat();
        // Determina el limite del brillo al convertir la imagen en escala de grises en una imagen binaria (blanco y negro)
        private const int Threshold = 1;

        // Erode eliminar el ruido (reduce las zonas de white pixels)
        private const int ErodeIterations = 1;

        // Dilation mejora pixels de despues de erode (amplia zonas de white pixels)
        private const int DilateIterations = 7;

        //Color para graficas en el image (Mat) de salida
        private static MCvScalar drawingColor = new Bgr(Color.Red).MCvScalar;


        //Rango de colores para deteccion
        static int blue1 = 0;
        static int green1 = 0;
        static int red1 = 230;
        static int blue2 = 255;
        static int green2 = 255;
        static int red2 = 255;
        private bool detectGesture = false;
        Point center;


        private  Mat DetectPen()
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
                Rectangle cropbox = new Rectangle();

                capture.Read(m);

                if (!m.IsEmpty)
                {

                    Image<Bgr, byte> ret = m.ToImage<Bgr, byte>();
                    Image<Bgr, byte> img = m.ToImage<Bgr, byte>();
                    var image = img.InRange(new Bgr(blue1, green1, red1), new Bgr(blue2, green2, red2));
                    var mat = img.Mat;//nueva matriz igual a la anterior
                    mat.SetTo(new MCvScalar(0, 0, 255), image);
                    mat.CopyTo(ret);
            
                    Image<Bgr, byte> imgout = ret.CopyBlank();//imagen sin fondo negro
                    imgout._Or(img);
  

                    CvInvoke.AbsDiff(m, imgout, n);
                    // Aplica limite binario a la imagen en escala de grises (white pixels marcan la diferencia)
                    CvInvoke.CvtColor(n, o, ColorConversion.Bgr2Gray);
                    CvInvoke.Threshold(o, binaryDiffFrame, 5, 255, ThresholdType.Binary);// 5 determina el límite del brillo al convertir la imagen de escala de grises a imagen binaria (blanco y negro)

                // Remueve ruido con la operacion opening (eronde seguida de dilate)
                CvInvoke.Erode(binaryDiffFrame, denoisedDiffFrame, null, new Point(-1, -1), ErodeIterations, BorderType.Default, new MCvScalar(1));
                    CvInvoke.Dilate(denoisedDiffFrame, denoisedDiffFrame, null, new Point(-1, -1), DilateIterations, BorderType.Default, new MCvScalar(1));
                
                    m.CopyTo(finalFrame);

                m.Dispose();
                n.Dispose();
                o.Dispose();
                binaryDiffFrame.Dispose();
                
                return DetectObject(denoisedDiffFrame, finalFrame, cropbox);
              
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
                int y = i * 10 + origin.Y; // Moverse abajo  en cada linea
                CvInvoke.PutText(frame, lines[i], new Point(origin.X, y), FontFace.HersheyPlain, 0.8, drawingColor);
            }
        }

        private  Mat DetectObject(Mat detectionFrame, Mat displayFrame, Rectangle box)
        {
            Image<Bgr, Byte> buffer_im = displayFrame.ToImage<Bgr, Byte>();
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                
                IOutputArray hirarchy = null;
                // Construir lista de contur (contornos)
                CvInvoke.FindContours(detectionFrame, contours, hirarchy, RetrType.List, ChainApproxMethod.ChainApproxSimple);

                // seleccionar el contour (contorno) mas grande
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


                    VectorOfPoint hullPoints = new VectorOfPoint();
                    VectorOfInt hullInt = new VectorOfInt();

                    CvInvoke.ConvexHull(contours[chosen], hullPoints, true);
                    CvInvoke.ConvexHull(contours[chosen], hullInt, false);

                    Mat defects = new Mat();


                    if (hullInt.Size > 3)
                        CvInvoke.ConvexityDefects(contours[chosen], hullInt, defects);

                    box = CvInvoke.BoundingRectangle(hullPoints);
                    CvInvoke.Rectangle(displayFrame, box, drawingColor);//Box rectangulo que encierra el area mas grande
                                                                        
                    center = new Point(box.X + box.Width / 2, box.Y + box.Height / 2);//centro  rectangulo MOUSE
                                   
                    var infoCentro = new string[] { $"Centro", $"Posicion: {center.X}, {center.Y}" };
                    WriteMultilineText(displayFrame, infoCentro, new Point(center.X + 30, center.Y));
                    CvInvoke.Circle(displayFrame, new Point(center.X, center.Y), 2, new MCvScalar(0, 100, 0), 4);
                    detectGesture = true;

                    buffer_im.Dispose();
                    defects.Dispose();
                    detectionFrame.Dispose();
                    return displayFrame;

                }
                buffer_im.Dispose();
                detectionFrame.Dispose();
                detectGesture = false;
                return displayFrame;
            }            
        }
    }
}

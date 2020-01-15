using System;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using System.IO;
using System.Reflection;

namespace ModuloProcesamientoImagenes
{
    public class ColorRecognition
    {

        private Mat textImage;
        public Mat TextImage
        {
            get
            {
                return textImage;
            }
        }

        public Mat Recognition(VideoCapture src)
        {
            capture = src;
            return DetectTxt();
        }

        private static VideoCapture capture;
        
        private static Image imagenFinal;
        private static Mat imagen = new Mat();
        private static Mat imagenOut = new Mat();
           
        Mat picture = new Mat();

        // Determina el limite del brillo al convertir la imagen en escala de grises en una imagen binaria (blanco y negro)
        private const int Threshold = 1;

        // Erode eliminar el ruido (reduce las zonas de white pixels)
        private const int ErodeIterations = 1;

        // Dilation mejora pixels de despues de erode (amplia zonas de white pixels)
        private const int DilateIterations = 7;

        private static MCvScalar drawingColor = new Bgr(Color.Red).MCvScalar;
       
        //Rango de colores para deteccion
        static int blue1 = 0;
        static int green1 = 0;
        static int red1 = 120;
        static int blue2 = 125;
        static int green2 = 255;
        static int red2 = 255;



       

        private Mat DetectTxt()
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
                   
                    // Deteccion de formas usando imagen binaria(branco y negro)
                    return DetectObject(denoisedDiffFrame, finalFrame, cropbox);
                 
                }
                else
                {
                    // break;
                }

           
            return null;
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

                    // Draw on a frame
                    //MarkDetectedObject(displayFrame, contours[chosen], maxArea);//dibuja una envoltura roja

                    VectorOfPoint hullPoints = new VectorOfPoint();
                    VectorOfInt hullInt = new VectorOfInt();

                    CvInvoke.ConvexHull(contours[chosen], hullPoints, true);
                    CvInvoke.ConvexHull(contours[chosen], hullInt, false);

                    Mat defects = new Mat();


                    if (hullInt.Size > 3)
                        CvInvoke.ConvexityDefects(contours[chosen], hullInt, defects);

                    box = CvInvoke.BoundingRectangle(hullPoints);
                    CvInvoke.Rectangle(displayFrame, box, drawingColor);//Box rectangulo que encierra el area mas grande
                                                                        // cropbox = crop_color_frame(displayFrame, box);

                    buffer_im.ROI = box;

                    Image<Bgr, Byte> cropped_im = buffer_im.Copy();
                    textImage = cropped_im.Mat;
                    imagenFinal = cropped_im.Bitmap;

                    //imagenFinal.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    //if (imagenFinal != null)
                    //{
                    //    try

                    //    {
                    //        var assembly = Assembly.GetExecutingAssembly();
                    //        var folder = Path.GetDirectoryName(assembly.Location);
                    //        imagenFinal.Save(folder + "/texto.png", System.Drawing.Imaging.ImageFormat.Png);
                    //    }
                    //    catch (Exception)
                    //    {

                    //        throw;
                    //    }
                    //}
                 
                    return displayFrame;

                }
                textImage = detectionFrame;
                return displayFrame;
                
            }


        }

        Mat cropColorFrame(Mat input, Rectangle crop_region)
        {
            //copia de input
            Image<Bgr, Byte> buffer_im = input.ToImage<Bgr, Byte>();
            buffer_im.ROI = crop_region;

            Image<Bgr, Byte> cropped_im = buffer_im.Copy();


            return cropped_im.Mat;

        }

    }
}
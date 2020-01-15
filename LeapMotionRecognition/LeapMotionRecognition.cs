using System;
using System.Linq;
using ModuloReconocimientoGestual;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using Leap;
using System.Drawing.Imaging;

namespace LeapMotionRecognition
{
    public class LeapMotionRecognition : IPlugin
    {
        public string Name => "Leap Motion Recognition";

        public Point Center
        {
            get
            {
                return center;
            }
        }
        private bool detectGesture;
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

            //Image<Bgr, byte> imageCV = new Image<Bgr, byte>(bitmap);
            return imageCV.Mat;
            //return new Mat();
        }

        // interface principal de leap motion
        private Controller controller = new Controller();
        Image<Bgr, byte> imageCV;

        public Bitmap bitmap = new Bitmap(640, 480, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

        public Int64 changeTime;
        public Frame currentFrame;
        public Frame prevFrame;
        private long currentTime;
        private long previousTime;
        private long timeChange;
        public Leap.Vector leapPoint;
        public float xScreenIntersect;
        public float yScreenIntersect;
        public float zScreenIntersect;
        int nClick = 0;
        int clickState = 0;

        private byte[] imagedata = new byte[1];
        Point center;

        public LeapMotionRecognition()
        {

           
            controller.SetPolicy(Controller.PolicyFlag.POLICY_OPTIMIZE_HMD);// Optimizado para head mounted display
            //controller.EventContext = SynchronizationContext.;
            controller.FrameReady += newFrameHandler;
            controller.ImageReady += onImageReady;
            controller.FrameReady += OnFrame;
            controller.ImageRequestFailed += onImageRequestFailed;

            //establecer la paleta de escala de grises para el objeto de mapa de bits
            ColorPalette grayscale = bitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                grayscale.Entries[i] = Color.FromArgb((int)255, i, i, i);
            }
            bitmap.Palette = grayscale;
            imageCV = new Image<Bgr, byte>(bitmap);
            controller.StartConnection();
        }

        void newFrameHandler(object sender, FrameEventArgs eventArgs)
        {
            Frame frame = eventArgs.frame;
            controller.RequestImages(frame.Id, Leap.Image.ImageType.DEFAULT, imagedata);
        }

        void onImageReady(object sender, ImageEventArgs e)
        {
            Rectangle lockArea = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bitmapData = bitmap.LockBits(lockArea, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            byte[] rawImageData = imagedata;
            System.Runtime.InteropServices.Marshal.Copy(rawImageData, 0, bitmapData.Scan0, e.image.Width * e.image.Height * 2 * e.image.BytesPerPixel);
            bitmap.UnlockBits(bitmapData);
            imageCV = new Image<Bgr, byte>(bitmap);
   
        }

        public void OnFrame(object sender, FrameEventArgs args)
        {

            // abtiene el frame actual
            Frame currentFrame = controller.Frame();
            Vector LeapCenter;
            LeapCenter.x = 0;
            LeapCenter.y = 0;
            LeapCenter.z = 0;
            Vector size;
            size.x = 1000;
            size.y = 1000;
            size.z = 1000;

            detectGesture = false;
            currentTime = currentFrame.Timestamp;
            timeChange = currentTime - previousTime;

            if (timeChange > 10000)
            {
                if (currentFrame.Hands.Count() > 0)
                {
                    // Recibe el primer dendo en una lista de dedos 
                    Finger finger = controller.Frame().Hands[0].Fingers[1];

                    InteractionBox screen = new InteractionBox(LeapCenter, size);
          
                    // if (screen.IsValid)
                    if (screen.IsValid)
                    {
                        // Velocidad de la punta del dedo milimetros/segundo
                        float tipVelocity = finger.TipVelocity.Magnitude;


                        // tipVecity movimientos por mal pulso del la mano
                        // the cursor steady if (tipVelocity > 25)
                        if (tipVelocity > 0)
                        {
         
                            detectGesture = true;
                            leapPoint = finger.StabilizedTipPosition;

                            xScreenIntersect = screen.NormalizePoint(leapPoint, true).x;
                            yScreenIntersect = screen.NormalizePoint(leapPoint, true).y;
                            zScreenIntersect = screen.NormalizePoint(leapPoint, true).z;
                            center.X = (int)(xScreenIntersect * 100000);
                            if (true)
                            {
                                center.Y = (int)(yScreenIntersect * 100000);
                            }
                            else
                            {
                                center.Y = (int)(zScreenIntersect * 100000);
                            }
                        
                            if (xScreenIntersect.ToString() != "NaN")
                            {


                                if (clickState == 0)
                                {

                                    int extendedFingers = 0;
                                    for (int f = 0; f < controller.Frame().Hands[0].Fingers.Count; f++)
                                    {
                                        Finger digit = controller.Frame().Hands[0].Fingers[f];
                                        if (digit.IsExtended)
                                            extendedFingers++;
                                    }

                                    if (extendedFingers == 1)
                                    {
                                        Console.WriteLine("LeapMotion Click Down");
                                       
                                        nClick++;
                                        detectClick = true;
                                    }
                                    else
                                    {
                                        Console.WriteLine("LeapMotion Mano extendida");
                                     
                                        nClick = 0;
                                    }
                                    if (nClick >= 1)
                                    {


                                        //Clicking.SendClick(x, y);
                                        nClick = 0;
                                        clickState = 1;
                                    }
                                }

                                if (clickState == 1)
                                {
                                    int extendedFingers = 0;
                                    for (int f = 0; f < controller.Frame().Hands[0].Fingers.Count; f++)
                                    {
                                        Finger digit = controller.Frame().Hands[0].Fingers[f];
                                        if (digit.IsExtended)
                                            extendedFingers++;
                                    }

                                    if (extendedFingers == 5)
                                    {
                                        Console.WriteLine("LeapMotion Click up");
                                      
                                        nClick++;
                                        detectClick = false;

                                    }
                                    else
                                    {
                                        Console.WriteLine("LeapMotion Dedo indice extendido");
                                    
                                        nClick = 0;


                                    }
                                    if (nClick >= 1)
                                    {
                               
                                        //Clicking.SendUpClick(x, y);
                                        nClick = 0;
                                        clickState = 0;
                                    }
                                }

                            }

                        }
                    }

                }

                previousTime = currentTime;
            }

        }

        void onImageRequestFailed(object sender, ImageRequestFailedEventArgs e)
        {
            if (e.reason == Leap.Image.RequestFailureReason.Insufficient_Buffer)
            {
                imagedata = new byte[e.requiredBufferSize];
            }
            Console.WriteLine("Image request failed: " + e.message);
        }

    }
}
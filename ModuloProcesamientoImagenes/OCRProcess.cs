using System;
using System.Windows.Forms;
using Tesseract;
using System.Drawing;

namespace ModuloProcesamientoImagenes
{
   //Clase encargada de procesar una imagen por OCR
    public class OCRProcess
    {
        private static TesseractEngine engine;

        private static TesseractEngine Engine
        {
            get
            {
                if (engine == null || engine.IsDisposed)
                {
                    engine = new TesseractEngine("./tessdata", "eng", EngineMode.TesseractAndCube);

                }
                return engine;
            }
        }
        public static string TransformImage()
        {

            try
            {
                var img = new Bitmap("texto.png");
                img.RotateFlip(RotateFlipType.Rotate180FlipNone);//Rota la imagen

                using (var ocr = new TesseractEngine("./tessdata", "eng", EngineMode.TesseractAndCube))
                {
                    using (var page = ocr.Process(img))
                    {
                        return page.GetText();

                    }
                }
                
            }
            catch (Exception ex)
            {

                MessageBox.Show("OCRProcess :" + ex.Message);
            }

            return "";
        }

        public static string TransformImage(Bitmap img)
        {
            if (img != null)
            {
                try
                {

                    img.RotateFlip(RotateFlipType.Rotate180FlipNone);//Rota la imagen

                    //using (var ocr = new TesseractEngine("./tessdata", "eng", EngineMode.TesseractAndCube))
                    //{
                        using (var page = Engine.Process(img))
                        {
                            return page.GetText();

                        }
                  //  }

                }
                catch (Exception ex)
                {

                    MessageBox.Show("OCRProcess TransformImage :" + ex.Message);
                }
            }

            return "";
        }
    }
}

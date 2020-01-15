using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace ModuloConsistenciaDatos
{
    public class DigitalDocSync
    {
        PdfReader reader;
        public iTextSharp.text.Rectangle rectPage;
        private string outputFile;
        public List<Rectangle> listaRectangulos = new List<Rectangle>();
        //int nOpen = 0;
        string highLightFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Highlighted.pdf");
        string highLightFileTemp = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "HighlightedTemp.pdf");
        string highLightFileBac = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "HighlightedTemp.pdf.bac");
        public DigitalDocSync(string OutputFile)
        {
            outputFile = OutputFile;
            reader = new PdfReader(outputFile);
            rectPage = reader.GetPageSize(1);
            reader.Close();
            
        } 
           

       public string outputData;
        public string OutputData
        {
            
            set { OutputData = value; }
        }

        public void SaveAnno(float lx, float ly, float rx, float ry)
        {
        
            reader = new PdfReader(outputFile);
            rectPage = reader.GetPageSize(1);
            PutRectAnno(lx, ly,  rx,  ry);
            reader.Close();
        
            System.IO.File.Replace(highLightFile, outputFile, highLightFileBac);
            GetRectAnno();
        }
        


            public void PutRectAnno(float lx, float ly, float rx, float ry)
        {
            




            using (FileStream fs = new FileStream(highLightFile, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (PdfStamper stamper = new PdfStamper(reader, fs))
                {
       
                    iTextSharp.text.Rectangle rect = new iTextSharp.text.Rectangle(lx, ly, rx, ry);
                    
                    float[] quad = { rect.Left, rect.Top, rect.Right, rect.Top, rect.Left, rect.Bottom, rect.Right, rect.Bottom };


                    PdfAnnotation highlight = PdfAnnotation.CreateMarkup(stamper.Writer, rect, null, PdfAnnotation.MARKUP_HIGHLIGHT, quad);


                    highlight.Color = BaseColor.YELLOW;



                    if (rect.Width > 0 && rect.Height > 0)
                    {
                        stamper.AddAnnotation(highlight, 1);
                    }


                    stamper.Close();
                }
                fs.Close();
            }

        }

        public void GetRectAnno()
        {

            string appRootDir = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.FullName;

            string filePath = outputFile;
            int pageFrom = 0;
            int pageTo = 0;
            listaRectangulos.Clear();
            try
            {
                using (PdfReader reader = new PdfReader(filePath))
                {
                    pageTo = reader.NumberOfPages;

                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {


                        PdfDictionary page = reader.GetPageN(i);
                        PdfArray annots = page.GetAsArray(iTextSharp.text.pdf.PdfName.ANNOTS);
                        if (annots != null)
                            foreach (PdfObject annot in annots.ArrayList)
                            {

                                //abtiene Annotation de PDF File
                                PdfDictionary annotationDic = (PdfDictionary)PdfReader.GetPdfObject(annot);
                                PdfName subType = (PdfName)annotationDic.Get(PdfName.SUBTYPE);
                                //solo el subtype highlight
                                if (subType.Equals(PdfName.HIGHLIGHT))
                                {
                                    // Obtiene Quadpoints y Rectángulo de texto resaltado
                                    //Console.Write("HighLight at Rectangle {0} with QuadPoints {1}\n", annotationDic.GetAsArray(PdfName.RECT), annotationDic.GetAsArray(PdfName.QUADPOINTS));


                                    outputData = "HighLight at Rectangle {0} with QuadPoints {1}\n" + annotationDic.GetAsArray(PdfName.RECT) + annotationDic.GetAsArray(PdfName.QUADPOINTS);
                                    //Extraer texto usando la estrategia de rectángulo
                                    PdfArray coordinates = annotationDic.GetAsArray(PdfName.RECT);

                                    iTextSharp.text.Rectangle rect = new iTextSharp.text.Rectangle(float.Parse(coordinates.ArrayList[0].ToString(), CultureInfo.InvariantCulture.NumberFormat), float.Parse(coordinates.ArrayList[1].ToString(), CultureInfo.InvariantCulture.NumberFormat),
                                    float.Parse(coordinates.ArrayList[2].ToString(), CultureInfo.InvariantCulture.NumberFormat), float.Parse(coordinates.ArrayList[3].ToString(), CultureInfo.InvariantCulture.NumberFormat));
                                  
                                    listaRectangulos.Add(rect);
                                    

                                    RenderFilter[] filter = { new RegionTextRenderFilter(rect) };
                                    ITextExtractionStrategy strategy;
                                    StringBuilder sb = new StringBuilder();


                                    strategy = new FilteredTextRenderListener(new LocationTextExtractionStrategy(), filter);
                                    sb.AppendLine(PdfTextExtractor.GetTextFromPage(reader, i, strategy));

     
                                }

                            }

                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}

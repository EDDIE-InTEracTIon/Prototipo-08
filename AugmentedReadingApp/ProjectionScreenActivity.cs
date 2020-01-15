using System;
using System.Drawing;
using System.Windows.Forms;
using ModuloProcesamientoImagenes;
using ModuloVisualizacionDatos;

namespace AugmentedReadingApp
{
    public partial class ProjectionScreenActivity : Form
    {
        public int CameraNumber;
        InteractionCoordinator originalForm;
       
        bool markPoint = true;
        public ColorRecognition recTxt;
        
        public HighlightTool Highlight;

        public ProjectionScreenActivity(InteractionCoordinator incomingForm)
        {
            
            originalForm = incomingForm;
            InitializeComponent();
            
            pictureBox3.Image = Image.FromFile("x_mark_red_circle.png");
            pictureBox4.Image = Image.FromFile("x_mark_red_circle.png");
            pictureBox5.Image = Image.FromFile("x_mark_red_circle.png");

            markPoint = false;


            Highlight = new HighlightTool(30, 517, 400)
            {
                Name = "HighlightBox",
                BackColor = Color.FromArgb(255, 255, 255),
                
                Location = new Point(240, 100),


            };
            this.Controls.Add(Highlight);

            Highlight.NumClicks += ButtonAction;

        }



        private void Form2_Load(object sender, EventArgs e)
        {
        // Load del Form
        }
      

        private void button1_Click(object sender, EventArgs e)
        {
            if (markPoint)
            {


                pictureBox3.Image = Image.FromFile("x_mark_red_circle.png");
                pictureBox4.Image = Image.FromFile("x_mark_red_circle.png");
                pictureBox5.Image = Image.FromFile("x_mark_red_circle.png");

                markPoint = false;
            }
            else
            {
                pictureBox3.Image = null;
                pictureBox4.Image = null;
                pictureBox5.Image = null;
                markPoint = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (Highlight.HighLightOn)
            {

                originalForm.CaptureTxt();

                try
                {
                    textBox1.Text = OCRProcess.TransformImage(originalForm.recTxt.TextImage.Bitmap);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("OCRProcess Resaltado: " + ex.Message);
                }

            }
            else
            {
                originalForm.CaptureImage();

                try
                {
                    textBox1.Text = OCRProcess.TransformImage(originalForm.RectangleImage.Bitmap);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("OCRProcess rectangulo: " + ex.Message);
                }
            }
        }

 
        private void button3_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (Highlight.HighLightOn)
            {

                button3.Text = "TEXT/IMAGE";
                button3.BackColor = default(Color);
                Highlight.HighLightOn = false;

            }
            else
            {
                button3.BackColor = Color.Gray;
                button3.Text = "HIGHLIGHT";
                Highlight.HighLightOn = true;
            }


        }

        private void ButtonAction()
        {

            if (Highlight.HighLightOn)
            {

                originalForm.CaptureTxt();

                try
                {
                    textBox1.Text = OCRProcess.TransformImage(originalForm.recTxt.TextImage.Bitmap);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("OCRProcess Resaltado: " + ex.Message);
                }

            }
            else
            {
                if (originalForm.plugin.AutoCamCapture)
                {
                    originalForm.CaptureImage();

                    try
                    {
                        textBox1.Text = OCRProcess.TransformImage();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("OCRProcess rectangulo: " + ex.Message);
                    }
                }
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            
            float llx = (Highlight.normRect.Left) * originalForm.documentoSyn.rectPage.Right;
            float lly = (1 - Highlight.normRect.Bottom ) * originalForm.documentoSyn.rectPage.Top - (Highlight.normRect.Top) * originalForm.documentoSyn.rectPage.Top;
            float urx = (Highlight.normRect.Left) * originalForm.documentoSyn.rectPage.Right + (Highlight.normRect.Right) * originalForm.documentoSyn.rectPage.Right;
            float ury = (1 - Highlight.normRect.Bottom) * originalForm.documentoSyn.rectPage.Top;
            originalForm.documentoSyn.SaveAnno(llx, lly, urx, ury);
            Highlight.GetRectangles(originalForm.documentoSyn.listaRectangulos);
        }
    }
}


using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ModuloProcesamientoImagenes;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Reflection;
using System.IO;
using ModuloReconocimientoGestual;
using ModuloConsistenciaDatos;
//using Leap;


namespace AugmentedReadingApp
{
    public partial class InteractionCoordinator : Form
    {   //Clase y atributos para detectar y desplegar camaras
        CameraActivity camerasText = new CameraActivity();
        CameraActivity camerasGesture = new CameraActivity();
        private int _CameraTextIndex;
        private int _CameraGestureIndex;
        VideoCapture captureText;
        VideoCapture captureGesture;
        Dictionary<string, IPlugin> _plugins = new Dictionary<string, IPlugin>();

        public IPlugin plugin;

        public ColorRecognition recTxt = new ColorRecognition();
        public GestureRecognitionActivity recGestual = new GestureRecognitionActivity();
        public ProjectionScreenActivity projection;
        public DigitalDocSync documentoSyn;
        OpenFileDialog  openFilePDF = new OpenFileDialog();

        private Mat rectangleImage;
        public Mat RectangleImage
        {
            get
            {
                return rectangleImage;
            }
        }


        public InteractionCoordinator()
        {

            projection = new ProjectionScreenActivity(this);

            InitializeComponent();

            LoadComboBox(camerasText.ListCameras(), ComboBoxCameraList1);
            LoadComboBox(camerasGesture.ListCameras(), ComboBoxCameraList2);


            //Atributos y metodos reflexion
            var assembly = Assembly.GetExecutingAssembly();

            var folder = Path.GetDirectoryName(assembly.Location);

            LoadPlugins(folder);

            CreateFilterMenu();

            recGestual.selectedRectangle += PutRectangle;

            GetSettings();
        }

        //Clase reconociminto gestual
        void LoadPlugins(string folder)
        {
            _plugins.Clear();
            foreach (var dll in Directory.GetFiles(folder + "\\Plugins", "*.dll"))
            {
                try
                {
                    var asm = Assembly.LoadFrom(dll);
                    foreach (var type in asm.GetTypes())
                    {
                        if (type.GetInterface("IPlugin") == typeof(IPlugin))
                        {
                            var filter = Activator.CreateInstance(type) as IPlugin;
                            _plugins[filter.Name] = filter;


                        }
                    }

                }
                catch (BadImageFormatException ex) { MessageBox.Show("LoadPlugins :" + ex.Message); }

            }
        }

        void CreateFilterMenu()
        {
            complementoToolStripMenuItem.DropDownItems.Clear();

            foreach (KeyValuePair<string, IPlugin> pair in _plugins)
            {
                var item = new ToolStripMenuItem(pair.Key);
                item.Click += new EventHandler(menuItem_click);
                complementoToolStripMenuItem.DropDownItems.Add(item);
                // ((ToolStripMenuItem)menuItem).Checked = true;

            }

        }

        void menuItem_click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            plugin = _plugins[menuItem.Text];
            recGestual.Plugin = _plugins[menuItem.Text]; 
            UncheckOtherToolStripMenuItems((ToolStripMenuItem)sender);
            ((ToolStripMenuItem)menuItem).Checked = true;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                //pictureBox2.Image = _filter.RunPlugin(captureGesture);
            }
            //catch ()
            catch (Exception ex)
            {
                MessageBox.Show(".dll externo" + ex.Message);
                // no sabemos qué puede pasar al cargar en un .dll externo

            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        public void UncheckOtherToolStripMenuItems(ToolStripMenuItem selectedMenuItem)
        {
            selectedMenuItem.Checked = true;


            foreach (var ltoolStripMenuItem in (from object
                                                    item in selectedMenuItem.Owner.Items
                                                let ltoolStripMenuItem = item as ToolStripMenuItem
                                                where ltoolStripMenuItem != null
                                                where !item.Equals(selectedMenuItem)
                                                select ltoolStripMenuItem))
                (ltoolStripMenuItem).Checked = false;


            selectedMenuItem.Owner.Show();
        }

        void LoadComboBox(List<KeyValuePair<int, string>> ListData,
        ComboBox ComboBoxCameraList)
        {
            //Limpiar el comboBox que muestra las camaras

            ComboBoxCameraList.DataSource = null;
            ComboBoxCameraList.Items.Clear();

            //Enlase a comboBox que muestra las camaras
            ComboBoxCameraList.DataSource = new BindingSource(ListData, null);
            ComboBoxCameraList.DisplayMember = "Value";
            ComboBoxCameraList.ValueMember = "Key";
        }

        private void ComboCameras_SelectedIndexChangedText(object sender, EventArgs e)
        {
            //Obtiene el item seleccionado del combobox
            _CameraTextIndex = SelectItem((ComboBox)sender);

        }

        private void ComboCameras_SelectedIndexChangedGesture(object sender, EventArgs e)
        {
            //Obtiene el item seleccionado del combobox

            _CameraGestureIndex = SelectItem((ComboBox)sender);

        }



        int SelectItem(ComboBox ComboBoxCameraList)
        {
            //Obtiene el item seleccionado del combobox
            KeyValuePair<int, string> SelectedItem = (KeyValuePair<int, string>)ComboBoxCameraList.SelectedItem;

            //Asigna el numero del item camara selecionada a la salida
            return SelectedItem.Key;
        }

        private void ProyectionActivityBt(object sender, EventArgs e)
        {
            var CameraNumber = _CameraTextIndex;

            projection.Show();

            if (captureText == null)
            {
                captureText = new VideoCapture(CameraNumber);
                captureText.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, (double)numericUpDownResXText.Value);
                captureText.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, (double)numericUpDownResYText.Value);

                imageBox1.Image = recTxt.Recognition(captureText);
            }

        }

        private void comenzarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var CameraNumber = _CameraGestureIndex;
            if (captureGesture == null)
            {
                captureGesture = new VideoCapture(CameraNumber);
                captureGesture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, (double)numericUpDownXGestual.Value);
                captureGesture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, (double)numericUpDownYGestual.Value);
                captureGesture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps, 5);

            }
            captureGesture.ImageGrabbed += Capture_ImageGrabbed1;
            captureGesture.Start();
        }

        private delegate void SetValueDelegate(string prValue);

        private void SetValue_textBoxX(string hecho)
        {
            if (textBoxX.InvokeRequired)
            {
                SetValueDelegate delegado = new SetValueDelegate(SetValue_textBoxX);
                textBoxX.Invoke(delegado, new object[] { hecho });
            }
            textBoxX.Text = hecho;
        }

        

        private void SetValue_textBoxY(string hecho)
        {
            if (textBoxY.InvokeRequired)
            {
                SetValueDelegate delegado = new SetValueDelegate(SetValue_textBoxY);
                textBoxY.Invoke(delegado, new object[] { hecho });
            }
            textBoxY.Text = hecho;
        }

        private void Capture_ImageGrabbed1(object sender, EventArgs e)
        {
            recGestual.ActiveX = checkBoxX.Checked;
            recGestual.ActiveY = checkBoxY.Checked;
            imageBox2.Image = recGestual.Capture_ImageGrabbed(captureGesture, (float)numericUpDownSStartX.Value,
            (float)numericUpDownSEndX.Value, (float)numericUpDownAStartX.Value, (float)numericUpDownAEndX.Value,
            (float)numericUpDownSStartY.Value, (float)numericUpDownSEndY.Value, (float)numericUpDownAStartY.Value,
            (float)numericUpDownAEndY.Value);
            SetValue_textBoxX(recGestual.SensorX.ToString());
            SetValue_textBoxY(recGestual.SensorY.ToString());

        }


        private void detenerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            captureGesture.Stop();
            captureGesture.ImageGrabbed -= Capture_ImageGrabbed1;//retirar el evento de captura camara
        }


        public void CaptureTxt()
        {
            captureText.Start();
            try
            {
                imageBox1.Image = recTxt.Recognition(captureText);
                if (recTxt.TextImage != null)
                {
                    imageBox3.Image = recTxt.TextImage;
                }

            }
            catch (Exception ex)
            {

                MessageBox.Show("CaptureTxt: " + ex.Message);
            }

            captureText.Stop();

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

            PutRectangle(recGestual.RectangularSelection);

        }

        public string ImageToBase64(System.Drawing.Image image,
        System.Drawing.Imaging.ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        private void PutRectangle(Rectangle rectangle)
        {
            using (Mat m = new Mat())
            {

                if (captureGesture != null)
                {
                    captureGesture.Retrieve(m);

                    imageBox4.Image = cropColorFrame(m, rectangle).ToImage<Bgr, byte>();
                    ImageToBase64(cropColorFrame(m, rectangle).Bitmap, System.Drawing.Imaging.ImageFormat.Bmp);

                    //imageBox4.Image = cropColorFrame(m, new Rectangle(10,10,0,1)).ToImage<Bgr, byte>();
                }

            }

        }

        public void CaptureImage()
        {
            using (Mat m = new Mat())
            {
                if (captureGesture != null)
                {
                    captureGesture.Retrieve(m);

                    rectangleImage = cropColorFrame(m, recGestual.RectangularSelection);

                    if (rectangleImage != null)
                    {
                        imageBox4.Image = rectangleImage.ToImage<Bgr, Byte>();
                    }





                }
            }
        }


        Mat cropColorFrame(Mat input, Rectangle cropRegion)
        {
            //copia de input
            Image<Bgr, Byte> buffer_im = input.ToImage<Bgr, Byte>();
            if (cropRegion.Width > 0 && cropRegion.Height > 0)
            {
                buffer_im.ROI = cropRegion;

                Image<Bgr, Byte> cropped_im = buffer_im.Copy();


                return cropped_im.Mat;
            }

            return buffer_im.Mat;
        }

        private void PDFSelectBt_Click(object sender, EventArgs e)
        {
            openFilePDF.Filter = "PDF|*.pdf";
            if (openFilePDF.ShowDialog() == DialogResult.OK)
            {
                textBoxPathPDF.Text = openFilePDF.FileName;
                documentoSyn = new DigitalDocSync(openFilePDF.FileName);
                numericUpDown5.Value = (int)documentoSyn.rectPage.Right;
                numericUpDown6.Value = (int)documentoSyn.rectPage.Top;
                projection.Highlight.pageSize.Right = documentoSyn.rectPage.Right;
                projection.Highlight.pageSize.Top = documentoSyn.rectPage.Top;

            }
        }

        public void GetSettings()
        {
            numericUpDownSStartX.Value = Properties.Settings.Default.ValorX1;
            numericUpDownSStartY.Value = Properties.Settings.Default.ValorY1;
            numericUpDownSEndX.Value = Properties.Settings.Default.ValorX2;
            numericUpDownSEndY.Value = Properties.Settings.Default.ValorY2;

        }
        public void SaveSettings()
        {
            Properties.Settings.Default.ValorX1 = (int)numericUpDownSStartX.Value;
            Properties.Settings.Default.ValorY1 = (int)numericUpDownSStartY.Value;
            Properties.Settings.Default.ValorX2 = (int)numericUpDownSEndX.Value;
            Properties.Settings.Default.ValorY2 = (int)numericUpDownSEndY.Value;
            Properties.Settings.Default.Save();

        }

        private void buttonSetting_Click(object sender, EventArgs e)
        {
            SaveSettings();
            GetSettings();
        }

        private void Resize_Click(object sender, EventArgs e)
        {
            projection.Highlight.RuntimeFinalLocation((int)FinalLocateX.Value, (int)FinalLocateY.Value);

        }

        private void numericUpDownSStartX_Enter(object sender, EventArgs e)
        {

        }
    }
}


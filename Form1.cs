using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Drawing.Imaging;

namespace Group7ML
{
    public partial class Form1 : Form
    {
        #region global variables
        string path;
        protected bool validData;
        protected Image image;
        protected Thread getImageThread;
        protected string lastFilename = String.Empty;
        protected PictureBox thumbnail = new PictureBox();
        protected DragDropEffects effect;
        protected Image nextImage;
        protected Thread imageThread;
        protected ImageList recentsimges = new ImageList();
        /// <summary>
        /// path where are stored all paths of pictures
        /// </summary>
        protected List<string> paths = new List<string>();
        public delegate void AssignImageDlgt();
        #endregion



        public Form1()
        {
            InitializeComponent();
            this.AllowDrop = true;
            recentsimges.ImageSize = new Size(50, 50);
            listView1.View = View.SmallIcon;
            trackBar1.Value = 255;
        }


        #region openFileDialog
        private void OpenFileDialog(object sender, EventArgs e)
        {


            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.png;)| *.jpg; *.jpeg; *.gif; *.png;";
            if (open.ShowDialog() == DialogResult.OK)
            {

                pictureBox1.Image = new Bitmap(open.FileName);
                paths.Add(open.FileName);
                image = (Bitmap)pictureBox1.Image.Clone();
                trackBar1.Value = 255;
                numericUpDown1.Value = 255;
                populate();


            }
        }
        #endregion

        #region DragDrop Implementation

        private bool GetFilename(out string filename, DragEventArgs e)
        {
            bool ret = false;
            filename = String.Empty;
            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                Array data = ((IDataObject)e.Data).GetData("FileDrop") as Array;
                if (data != null)
                {
                    if ((data.Length == 1) && (data.GetValue(0) is String))
                    {
                        filename = ((string[])data)[0];
                        string ext = Path.GetExtension(filename).ToLower();
                        if ((ext == ".jpg") || (ext == ".png") || (ext == ".bmp"))
                        {
                            ret = true;
                            paths.Add(filename);
                        }
                    }
                }
            }
            return ret;
        }



        private void saveImage()
        {
            image = new Bitmap(path);
        }

        private bool GetImage(out string filename, DragEventArgs e)
        {
            bool rtun = false;
            filename = string.Empty;
            if ((e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                Array data = ((DataObject)e.Data).GetData("FileDropped") as Array;
                if (data != null)
                {
                    if ((data.Length == 1) && (data.GetValue(0) is string))
                    {
                        filename = ((string[])data)[0];
                        string extension = Path.GetExtension(filename).ToLower();
                        if ((extension == ".jpg" || extension == ".bmp" || extension == ".png" || extension == ".gif"))
                        {
                            rtun = true;
                            paths.Add(filename);
                        }
                    }
                }
            }
            return rtun;

        }

        private void pictureBox1_DragDrop(object sender, DragEventArgs e)
        {
            if (validData)
            {
                while (getImageThread.IsAlive)
                {
                    Application.DoEvents();
                    Thread.Sleep(0);
                }
                pictureBox1.Image = image;
            }

        }

        #endregion

        #region LoadImage

        protected void LoadImage()

        {
            image = new Bitmap(path);
        }
        #endregion


        #region pictureBox_DragEnter

        private void pictureBox1_DragEnter(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files) Console.WriteLine(file);
        }

        #endregion


        #region Form1_Load
        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.AllowDrop = true;
        }

        #endregion

        #region trackbar_scroll

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (image == null)
            {
                image = (Bitmap)pictureBox1.Image.Clone();
                return;

            }


            numericUpDown1.Value = trackBar1.Value;
            pictureBox1.Image = SetOpacity((Bitmap)image, trackBar1.Value);



        }
        #endregion
        #region ChangeOpacity()

        /// <summary>
        ///  method responsible for the transparence of the picture
        /// </summary>
        /// <param name="bmpIn"></param>
        /// <param name="opacity"></param>
        /// <returns></returns>
        private Image SetOpacity(Bitmap bmpIn, int opacity) // double opacity??
        {
            Bitmap bmpOut = new Bitmap(bmpIn.Width, bmpIn.Height);
            float a = opacity / 255f;
            Rectangle r = new Rectangle(0, 0, bmpIn.Width, bmpIn.Height);

            float[][] matrixItems = {
            new float[] {1, 0, 0, 0, 0},
            new float[] {0, 1, 0, 0, 0},
            new float[] {0, 0, 1, 0, 0},
            new float[] {0, 0, 0, a, 0},
            new float[] {0, 0, 0, 0, 1}};

            ColorMatrix colorMatrix = new ColorMatrix(matrixItems);

            ImageAttributes imageAtt = new ImageAttributes();
            imageAtt.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            using (Graphics g = Graphics.FromImage(bmpOut))
                g.DrawImage(bmpIn, r, r.X, r.Y, r.Width, r.Height, GraphicsUnit.Pixel, imageAtt);

            return bmpOut;
        }
        #endregion


        #region ChangeBackGroundImage
        private void ChangeBackgroundImage(object sender, EventArgs e)
        {

            pictureBox1.Invalidate();
            image = pictureBox1.Image;
            trackBar1.Value = 0;
        }

        #endregion

        #region dragDrop
        private void dragDropPanel_DragDrop(object sender, DragEventArgs e)
        {
            if (validData)
            {
                while (getImageThread.IsAlive)
                {
                    Application.DoEvents();
                    Thread.Sleep(0);
                }
                pictureBox1.Image = image;
                trackBar1.Value = 255;
                numericUpDown1.Value = 255;
                populate();
            }
        }
        #endregion

        #region DragEnter

        private void dragDropPanel_DragEnter(object sender, DragEventArgs e)
        {
            string filename;
            validData = GetFilename(out filename, e);
            if (validData)
            {
                path = filename;
                getImageThread = new Thread(new ThreadStart(LoadImage));
                getImageThread.Start();
                e.Effect = DragDropEffects.Copy;
            }
            else
                e.Effect = DragDropEffects.None;


        }

        #endregion
        #region numericPad 

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            trackBar1.Value = (int)numericUpDown1.Value;
            pictureBox1.Image = SetOpacity((Bitmap)image, trackBar1.Value);
        }
        #endregion


        #region populate()
        //populate with image POPULATE WITH IMGS AN DTEX
        private void populate()
        {

            recentsimges.Images.Clear();

            //load images from file and specify your path for the images
            try
            {
                foreach (String path in paths)
                {
                    recentsimges.Images.Add(Image.FromFile(path));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            //bind images to listview 
            listView1.Items.Clear();

            listView1.SmallImageList = recentsimges;
            int index = 0;
            foreach (string path in paths)
            {
                listView1.Items.Add(Path.GetFileName(path), index++);
            }

        }
        #endregion


        #region listView_mouseClick
        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            path = paths[listView1.SelectedIndices[0]];
            image = new Bitmap(path);
            pictureBox1.Image = image;

        }

        #endregion


        private void button1_next(object sender, EventArgs e)
        {
            
            if (listView1.SelectedIndices.Count > 0)
            {
                int oldSelection = listView1.SelectedIndices[0];
                listView1.SelectedIndices.Clear();

                if (oldSelection + 1 >= listView1.Items.Count)
                    listView1.SelectedIndices.Add(0);
                else
                    listView1.SelectedIndices.Add(oldSelection + 1);

            }

            pictureBox1.Image = new Bitmap(paths[listView1.SelectedIndices[0]]);

        }


        int index;
        private void button2_previous(object sender, EventArgs e)
        {
            try
            {
                if (index >= 0)
                {
                    index = listView1.SelectedIndices[0] - 1;
                    listView1.SelectedIndices.Clear();
                    listView1.SelectedIndices.Add(index);

                }


                pictureBox1.Image = new Bitmap(paths[listView1.SelectedIndices[0]]);

            }
            catch (ArgumentOutOfRangeException error)
            {
                MessageBox.Show("you are already at the top of the list");
            }

          
        }


        private void clear_button_Click(object sender, EventArgs e)
        {
            
            // remove item from the listview and picturebox
            listView1.Items[listView1.SelectedIndices[0]].Remove();

            var pathToDelete =  Path.GetFileName(paths[0].ToString());
            paths.Remove(pathToDelete);
            pictureBox1.Image = null;
            

        }
    }
}

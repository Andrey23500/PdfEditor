﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Drawing.Imaging;

namespace PdfEditor
{
    public partial class PdfEditor : Form
    {
        string selectedFolder;
        string[] filePaths;
        Dictionary<string, string> pathDc = new Dictionary<string, string>();
        int currentPage = 0;
        int maxPages = 0;
        PdfiumViewer.PdfDocument document = null;
        string documentPath = null;
        Image currentImage = null;

        //Рисование на форме
        bool isActiveDraw = false;
        Point lastPoint = Point.Empty;
        bool isMouseDown = new Boolean();
        bool isDraw = false;
        public PdfEditor()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Open files";

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                selectedFolder = fbd.SelectedPath;
                if (selectedFolder != String.Empty)
                {
                    DirectoryInfo info = new DirectoryInfo(selectedFolder);
                    FileInfo[] fileIO = info.GetFiles("*.pdf", SearchOption.AllDirectories);
                    List<FileInfo> filesList = new List<FileInfo>();
                    if (fileIO.Length == 0)
                    {
                        MessageBox.Show("Not found .pdf files");
                    }
                    else
                    {
                        for (int i = 0; i < fileIO.Length; i++)
                        {
                            if (fileIO[i].Exists)
                            {
                                filesList.Add(fileIO[i]);
                            }
                        }
                        filesList.Sort((x, y) => y.CreationTime.CompareTo(x.CreationTime));

                        List<string> filesListName = new List<string>();
                        for (int i = 0; i < filesList.Count; i++)
                        {
                            filesListName.Add(filesList[i].FullName);
                        }
                        for (int i = 0; i < filesListName.Count; i++)
                        {
                            string str = filesListName[i];
                            int indx = selectedFolder.Length;
                            indx++;
                            str = str.Remove(0, indx);
                            pathDc.Add(filesListName[i], str);
                        }
                        filePaths = filesListName.ToArray();
                        listBox1.Items.Clear();

                        foreach (KeyValuePair<string, string> item in pathDc)
                        {
                            if (!item.Value.Contains(".isx"))
                            {
                                listBox1.Items.Add(item.Value);
                            }
                        }
                        listBox1.Visible = true;
                        label5.Visible = true;
                        textBox1.Visible = true;
                        trackBar1.Minimum = 1;
                        trackBar1.Maximum = 5;
                        trackBar1.SmallChange = 1;
                        trackBar1.LargeChange = 1;
                        trackBar1.UseWaitCursor = false;
                    }
                }
            }
        }
        //Открытие документа
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string path = null;
                foreach (KeyValuePair<string, string> item in pathDc)
                {
                    if (listBox1.SelectedItem.ToString() == item.Value)
                    {
                        path = item.Key;
                    }
                }
                document = PdfiumViewer.PdfDocument.Load(path);
                documentPath = path;
                maxPages = document.PageCount;
                currentPage = 0;
                currentImage = document.Render(currentPage, 1000000, 1000000, true);
                pictureBox1.Image = currentImage;
                label1.Text = $" of {maxPages}";
                button3.Visible = true;
                button4.Visible = true;
                label1.Visible = true;
                label3.Visible = true;
                label4.Visible = true;
                textBox2.Visible = true;
                trackBar1.Visible = true;
                pictureBox1.Visible = true;
                panel1.Visible = true;
                trackBar1.Value = 1;
                textBox2.TextChanged -= textBox2_TextChanged;
                textBox2.Text = (currentPage + 1).ToString();
                textBox2.TextChanged += textBox2_TextChanged;
            }
        }
        //Страница назад 
        void prevPage()
        {
            if (currentPage > 0)
            {
                currentPage--;
                currentImage = document.Render(currentPage, 1000000, 1000000, true);
                pictureBox1.Image = currentImage;
                label1.Text = $" of {maxPages}";
                textBox2.TextChanged -= textBox2_TextChanged;
                textBox2.Text = (currentPage + 1).ToString();
                textBox2.TextChanged += textBox2_TextChanged;
                trackBar1.Value = 1;
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            prevPage();
        }
        //Страница вперед
        void nextPage()
        {
            if (currentPage < maxPages - 1)
            {
                currentPage++;
                currentImage = document.Render(currentPage, 1000000, 1000000, true);
                pictureBox1.Image = currentImage;
                label1.Text = $" of {maxPages}";
                textBox2.TextChanged -= textBox2_TextChanged;
                textBox2.Text = (currentPage + 1).ToString();
                textBox2.TextChanged += textBox2_TextChanged;
                trackBar1.Value = 1;
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            nextPage();
        }
        //Динамический поиск
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            foreach (KeyValuePair<string, string> item in pathDc)
            {
                if (!item.Value.Contains(".isx"))
                {
                    listBox1.Items.Add(item.Value);
                }
            }
            for (int i = listBox1.Items.Count - 1; i >= 0; i--)
            {
                if (listBox1.Items[i].ToString().ToLower().Contains(textBox1.Text.ToLower()))
                {
                    listBox1.SetSelected(i, true);
                }
                else
                {
                    listBox1.Items.RemoveAt(i);
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Text != String.Empty)
            {
                currentPage = Int16.Parse(textBox2.Text) - 1;
                if (currentPage >= maxPages)
                {
                    currentPage = maxPages - 1;

                }
                if (currentPage < 0)
                {
                    currentPage = 0;
                }
                currentImage = document.Render(currentPage, 1000000, 1000000, true);
                pictureBox1.Image = currentImage;
                label1.Text = $" of {maxPages}";
                textBox2.Text = (currentPage + 1).ToString();
                trackBar1.Value = 1;
            }
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (document != null)
            {
                if (e.KeyValue == (char)Keys.Right || e.KeyValue == (char)Keys.Down)
                {
                    nextPage();
                }
                if (e.KeyValue == (char)Keys.Left || e.KeyValue == (char)Keys.Up)
                {
                    prevPage();
                }
            }
        }
        //Переход по страницам
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
            isActiveDraw = false;
            if (isDraw) // Если было рисование на picturebox
            {
                toLog();
                EncoderParameters encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Compression, 100);
                currentImage.Save(documentPath + ".jpg", ImageCodecInfo.GetImageEncoders()[1], encoderParameters);

                PdfDocument doc = new PdfDocument();
                for (int i = 0; i < maxPages; i++)
                {
                    if (i == currentPage) //Измененная страница
                    {
                        doc.Pages.Add(new PdfPage());
                        XGraphics xgr = XGraphics.FromPdfPage(doc.Pages[currentPage]);
                        XImage img = XImage.FromFile(documentPath + ".jpg");
                        xgr.DrawImage(img, 0, 0, img.Width, img.Height);
                        img.Dispose();
                        xgr.Dispose();
                    }
                    else //Страницы без изменений
                    {
                        Image img = document.Render(i, 300, 300, true);
                        MemoryStream strm = new MemoryStream();
                        img.Save(strm, ImageFormat.Png);
                        XImage xImg = XImage.FromStream(strm);
                        doc.Pages.Add(new PdfPage());
                        XGraphics xgr = XGraphics.FromPdfPage(doc.Pages[i]);
                        xgr.DrawImage(xImg, 0, 0, xImg.Width, xImg.Height);
                        img.Dispose();
                        xgr.Dispose();
                    }
                }
                //Очистка
                document.Dispose();
                doc.Save(documentPath);
                document = PdfiumViewer.PdfDocument.Load(documentPath);
                doc.Close();
                if (File.Exists(documentPath + ".jpg"))
                {
                    File.Delete(documentPath + ".jpg");
                }
                isDraw = false;
            }
        }

        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {
            Cursor = Cursors.Cross;
            isActiveDraw = true;
        }
        Image ZoomPicture(Image img, double size)
        {
            Bitmap bm = new Bitmap(img, Convert.ToInt32(img.Width * size),
                Convert.ToInt32(img.Height * size));
            Graphics gpu = Graphics.FromImage(bm);
            gpu.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            return bm;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (trackBar1.Value != 0)
            {
                pictureBox1.Image = ZoomPicture(currentImage, trackBar1.Value);
            }
        }
        private void toLog()
        {
            StreamWriter sw = File.AppendText("logs.txt");
            sw.WriteLine($"Date:" + DateTime.UtcNow.Date.ToString("dd-MM-yy") + " | Time:" + DateTime.Now.ToString("HH:mm:ss tt") + "| File:" + documentPath + " | Page:" + (currentPage + 1).ToString());
            sw.Close();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isActiveDraw)
            {
                isMouseDown = false;
                lastPoint = Point.Empty;
                double scale = (double)(1.0 / (trackBar1.Value));
                currentImage = ZoomPicture(pictureBox1.Image, scale);
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isActiveDraw)
            {
                if (isMouseDown == true)//check to see if the mouse button is down
                {
                    if (lastPoint != null)//if our last point is not null, which in this case we have assigned above
                    {
                        if (pictureBox1.Image == null)//if no available bitmap exists on the picturebox to draw on
                        {
                            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                            pictureBox1.Image = bmp; //assign the picturebox.Image property to the bitmap created
                        }
                        using (Graphics g = Graphics.FromImage(pictureBox1.Image))
                        {//we need to create a Graphics object to draw on the picture box, its our main tool
                            g.DrawLine(new Pen(Color.Red, 2), lastPoint, e.Location);
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        }
                        pictureBox1.Invalidate();//refreshes the picturebox
                        lastPoint = e.Location;//keep assigning the lastPoint to the current mouse position
                    }
                }
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            //Сохранить файл BackUp
            int indxName = documentPath.LastIndexOf("\\") + 1;
            string fileName = documentPath.Substring(indxName, documentPath.Length - indxName).Split('.')[0];
            string filePath = documentPath.Remove(indxName);
            string prefix = ".isx.pdf";
            string newSave = filePath + fileName + prefix;
            bool flag = true;
            for (int i = 0; i < filePaths.Length; i++)
            {
                if (filePaths[i].Contains(newSave))
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                document.Save(newSave);
                filePaths = Directory.GetFiles(selectedFolder, "*.pdf", SearchOption.AllDirectories);
            }
            if (isActiveDraw)
            {
                lastPoint = e.Location;
                isMouseDown = true;
                isDraw = true;
            }
        }
    }
}

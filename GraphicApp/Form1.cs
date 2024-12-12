using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms; //instalacja przez NuGet Package Manager (używany do pracy z połączeniem przeglądarki)

namespace GraphicApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            SetSize(); //rozmiar płótna
            Cef.Initialize(new CefSettings());//połączenie przeglądarki
            InitializeBrowser(); 
        }

        private void InitializeBrowser()
        {
            ChromiumWebBrowser browser = new ChromiumWebBrowser("https://www.google.com/search?q=drawing+inspiration")
            {
                Dock = DockStyle.Fill
            };

            Panel browserPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 450
            };

            browserPanel.Controls.Add(browser);
            this.Controls.Add(browserPanel);
        }

        //Clasa przechowuje punkty używane do rysowania linii:
        private class ArrayPoints
        {
            private int index = 0;
            private Point[] points;

            public ArrayPoints(int size)
            {
                if (size <= 0)
                {
                    size = 2;
                }
                points = new Point[size];
            }

            public void SetPoint(int x, int y) //dodaje nowe pointy (kropki) w tablicę
            {
                if (index >= points.Length)
                {
                    index = 0; //jeżeli tablica full - zapisujemy pointy od początku
                }
                points[index] = new Point(x, y);
                index++;
            }

            public void ResetPoints()
            {
                index = 0; //resetuje indeks
            }

            public int GetCountPoints()
            {
                return index; //ilośc ounktów dodanych do tablicy
            }

            public Point[] GetPoints()
            {
                return points;
            }
        }

        private bool isMouse = false;
        private ArrayPoints arrayPoints = new ArrayPoints(2);

        Bitmap map = new Bitmap(100, 100);
        Graphics graphics;

        Pen pen = new Pen(Color.Black, 3f);


        //Dostosowuje rozmiary płótna w zależności od rozdzielczości ekranu.
        private void SetSize()
        {
            Rectangle rectangle = Screen.PrimaryScreen.Bounds;
            map = new Bitmap(rectangle.Width, rectangle.Height);
            graphics = Graphics.FromImage(map);

            pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            pen.Color = ((Button)sender).BackColor;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            isMouse = true;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isMouse = false;
            arrayPoints.ResetPoints();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isMouse)
            {
                return;
            }

            arrayPoints.SetPoint(e.X, e.Y);
            if (arrayPoints.GetCountPoints() >= 2)
            {
                graphics.DrawLines(pen, arrayPoints.GetPoints());
                pictureBox1.Image = map;
                arrayPoints.SetPoint(e.X, e.Y);
            }
        }

        private void button10_Click(object sender, EventArgs e) //paleta
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                pen.Color = colorDialog1.Color;
                ((Button)sender).BackColor = colorDialog1.Color;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            graphics.Clear(pictureBox1.BackColor);
            pictureBox1.Image = map;
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e) //grubość pędzla
        {
            pen.Width = trackBar1.Value;
        }

        private int GetCaesarKey()
        {
            if (int.TryParse(txtCaesarKey.Text, out int key))
            {
                return key;
            }
            else
            {
                MessageBox.Show("Please enter a valid numeric key for encryption/decryption.", "Input error");
                throw new Exception("Incorrect encryption key."); 
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "Encrypted Image (*.enc)|*.enc";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    byte[] imageBytes;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        map.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        imageBytes = ms.ToArray();
                    }

                    int key = GetCaesarKey();
                    byte[] encryptedData = EncryptCaesar(imageBytes, key);
                    File.WriteAllBytes(saveFileDialog1.FileName, encryptedData);

                    MessageBox.Show("The image was successfully saved in encrypted form.", "Success");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving: " + ex.Message, "Error");
                }
            }
        }


        //Szyfrowanie obrazu: przesuwa każdy bajt o określoną liczbę pozycji.
        private byte[] EncryptCaesar(byte[] data, int key)
        {
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)((data[i] + key) % 256);
            }
            return result;
        }

        private byte[] DecryptCaesar(byte[] encryptedData, int key)
        {
            byte[] result = new byte[encryptedData.Length];
            for (int i = 0; i < encryptedData.Length; i++)
            {
                result[i] = (byte)((encryptedData[i] - key + 256) % 256);
            }
            return result;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Encrypted Image (*.enc)|*.enc";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    byte[] encryptedData = File.ReadAllBytes(openFileDialog1.FileName);

                    int key = int.Parse(txtCaesarKey.Text); 
                    byte[] decryptedData = DecryptCaesar(encryptedData, key);

                    using (MemoryStream ms = new MemoryStream(decryptedData))
                    {
                        Bitmap decryptedImage = new Bitmap(ms);
                        pictureBox1.Image = decryptedImage;
                        graphics = Graphics.FromImage(decryptedImage);
                        map = decryptedImage;
                    }

                    MessageBox.Show("The file was opened and decrypted successfully.", "Success");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error opening: " + ex.Message, "Error");
                }
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            graphics.Clear(pictureBox1.BackColor);
            pictureBox1.Image = map;
        }
    }
}

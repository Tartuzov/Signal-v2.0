using NAudio.Wave;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System;
using System.Media;

namespace Signal_v2._0
{
    public partial class Form1 : Form
    {
        private int loop = 5;
        private bool loopalltime = false;
        private bool loopuntilnutral = false;
        private BackgroundWorker comparisonWorker;
        private volatile bool stopPlayingSound = false; // добавлен флаг для остановки звука

        public Form1()
        {
            InitializeComponent();
            InitializeBackgroundWorker();
            textBox2.Text = loop.ToString();
            button6.Enabled = false;
        }

        private void InitializeBackgroundWorker()
        {
            comparisonWorker = new BackgroundWorker();
            comparisonWorker.WorkerSupportsCancellation = true;
            comparisonWorker.DoWork += ComparisonWorker_DoWork;
            comparisonWorker.RunWorkerCompleted += ComparisonWorker_RunWorkerCompleted;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Создаем Bitmap для хранения скриншота
            Bitmap screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size);
            }

            Form2 form2 = new Form2(screenshot);
            form2.Show();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            loopalltime = checkBox2.Checked;
            checkBox3.Enabled = !loopalltime;
            textBox2.Enabled = !loopalltime;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            loopuntilnutral = checkBox3.Checked;
            checkBox2.Enabled = !loopuntilnutral;
            textBox2.Enabled = !loopuntilnutral;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(textBox2.Text, out int newLoop))
            {
                loop = newLoop;
            }
        }

        private void ComparisonWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            double difference = Check();
            if (difference <= 0.05)
            {
                e.Cancel = true;
                return;
            }

            string audioFilePath = Path.Combine(Application.StartupPath, "sound.wav");
            using (var audioFile = new AudioFileReader(audioFilePath))
            {
                double durationInSeconds = audioFile.TotalTime.TotalSeconds;
                int sleepDuration = (int)(durationInSeconds * 1000); // Convert to milliseconds

                if (loopalltime)
                {
                    while (!comparisonWorker.CancellationPending)
                    {
                        if (stopPlayingSound) return;
                        CompareAndPlaySound(audioFilePath);
                        Thread.Sleep(sleepDuration);
                    }
                }
                else if (loopuntilnutral)
                {
                    while (!comparisonWorker.CancellationPending)
                    {
                        if (stopPlayingSound) return;
                        CompareAndPlaySound(audioFilePath);
                        Thread.Sleep(sleepDuration);
                        if (Check() <= 0.05)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < loop; i++)
                    {
                        if (comparisonWorker.CancellationPending || stopPlayingSound)
                        {
                            break;
                        }
                        CompareAndPlaySound(audioFilePath);
                        Thread.Sleep(sleepDuration);
                    }
                }
            }
        }

        private void ComparisonWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                button5.BackColor = SystemColors.Control;
                button5.Enabled = true;
                button6.Enabled = false;
                button1.Enabled = true;
            }
            else
            {
                MessageBox.Show("Задача завершена.");
            }
        }

        private double Check()
        {
            string[] lines = File.ReadAllLines("Point.txt");
            int[] coordinates = Array.ConvertAll(lines, int.Parse);

            Rectangle rect = new Rectangle(coordinates[0], coordinates[1], coordinates[2] - coordinates[0], coordinates[3] - coordinates[1]);
            Bitmap screenshot = new Bitmap(rect.Width, rect.Height);
            using (Graphics g = Graphics.FromImage(screenshot))
            {
                g.CopyFromScreen(rect.Left, rect.Top, 0, 0, rect.Size);
            }

            string outputPath = Path.Combine(Application.StartupPath, "original1.jpg");
            screenshot.Save(outputPath, System.Drawing.Imaging.ImageFormat.Jpeg);

            string rootImagePath = Path.Combine(Application.StartupPath, "original.jpg");
            Bitmap rootImage = new Bitmap(rootImagePath);

            double difference = CompareImages(screenshot, rootImage);
            screenshot.Dispose();
            rootImage.Dispose();

            return difference;
        }

        private void CompareAndPlaySound(string path)
        {
            using (SoundPlayer simpleSound = new SoundPlayer(path))
            {
                simpleSound.PlaySync(); // PlaySync blocks until the sound is complete
                if (stopPlayingSound) return; // Exit if stop flag is set
            }
        }

        private static double CompareImages(Bitmap image1, Bitmap image2)
        {
            double difference = 0.0;

            if (image1.Size == image2.Size)
            {
                for (int y = 0; y < image1.Height; y++)
                {
                    for (int x = 0; x < image1.Width; x++)
                    {
                        Color pixel1 = image1.GetPixel(x, y);
                        Color pixel2 = image2.GetPixel(x, y);

                        difference += Math.Abs(pixel1.R - pixel2.R) / 255.0;
                        difference += Math.Abs(pixel1.G - pixel2.G) / 255.0;
                        difference += Math.Abs(pixel1.B - pixel2.B) / 255.0;
                    }
                }

                difference /= 3 * image1.Width * image1.Height;
            }
            else
            {
                throw new ArgumentException("Изображения имеют разные размеры");
            }

            return difference;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (!comparisonWorker.IsBusy)
            {
                stopPlayingSound = false; // Reset the stop flag
                comparisonWorker.RunWorkerAsync();
                button5.Enabled = false;
                button5.BackColor = Color.LightGreen;
                button6.Enabled = true;
                button1.Enabled = false;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            stop();
        }

        private void stop()
        {
            if (comparisonWorker.IsBusy)
            {
                stopPlayingSound = true; // Set the stop flag
                comparisonWorker.CancelAsync();
            }
        }
    }
}
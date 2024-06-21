
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace Signal_v2._0
{
    public partial class Form2 : Form
    {
        private Point startPoint; 
        private Point currentPoint;
        private bool drawing = false;
        private Image backgroundImage;
        public Form2(Image background)
        {
            InitializeComponent();
            DoubleBuffered = true;
            Rectangle bounds = Screen.GetBounds(Point.Empty);
            Size = new Size(bounds.Width, bounds.Height);
            backgroundImage = background;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            this.BackgroundImage = backgroundImage; // встановлення фото як заднього фону
            this.FormBorderStyle = FormBorderStyle.None; // забираємо рамку форми
            this.WindowState = FormWindowState.Maximized; // розгортаємо форму на весь екран
        }
        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            // Запам'ятовуємо початкову точку малювання та починаємо малювати
            startPoint = e.Location;
            drawing = true;

        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            // Оновлюємо поточну точку малювання під час руху миші
            if (drawing)
            {
                currentPoint = e.Location;
                this.Invalidate(); // Примушуємо форму перемалювати себе
            }
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            // Заканчиваем рисование при отпускании кнопки мыши
            drawing = false;
            this.Invalidate(); // Применяем перерисовку формы

            // Вычисляем координаты верхнего левого угла и ширину/высоту прямоугольника
            int x = Math.Min(startPoint.X, currentPoint.X);
            int y = Math.Min(startPoint.Y, currentPoint.Y);
            int width = Math.Abs(currentPoint.X - startPoint.X);
            int height = Math.Abs(currentPoint.Y - startPoint.Y);

            // Создаем прямоугольник для выделения
            Rectangle selection = new Rectangle(x, y, width, height);

            // Создаем новое изображение, копируя часть фона с помощью созданного прямоугольника
            Bitmap croppedImage = new Bitmap(selection.Width, selection.Height);
            using (Graphics g = Graphics.FromImage(croppedImage))
            {
                g.DrawImage(backgroundImage, new Rectangle(0, 0, croppedImage.Width, croppedImage.Height),selection, GraphicsUnit.Pixel);
            }

            // Сохраняем изображение в корневую папку
            string outputPath = Path.Combine(Application.StartupPath, "original.jpg");
            string ForXYoutputPath = Path.Combine(Application.StartupPath, "Point.txt");
            croppedImage.Save(outputPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            croppedImage.Dispose(); // Освобождаем ресурсы
            StreamWriter sw = new StreamWriter(ForXYoutputPath);
            //Write a line of text
            sw.WriteLine(selection.Left);
            sw.WriteLine(selection.Top);
            sw.WriteLine(selection.Right);
            sw.WriteLine(selection.Bottom);
            sw.Close();
            // Закрываем форму
            this.Close();
        }
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            if (drawing)
            {
                // Відображення прямокутника між початковою точкою та поточною
                int width = currentPoint.X - startPoint.X;
                int height = currentPoint.Y - startPoint.Y;
                Rectangle rect = new Rectangle(startPoint.X, startPoint.Y, width, height);

                // Створюємо Brush для заливки прямокутника синім кольором з прозорістю 30%
                SolidBrush brush = new SolidBrush(Color.FromArgb(76, Color.Blue));
                e.Graphics.FillRectangle(brush, rect);

                // Створюємо Pen для малювання контуру прямокутника без прозорості
                Pen pen = new Pen(Color.Black);
                e.Graphics.DrawRectangle(pen, rect);
            }
        }
    }
}

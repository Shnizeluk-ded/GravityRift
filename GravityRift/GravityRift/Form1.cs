using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GravityRift
{
    public partial class Form1 : Form
    {
        Timer timer = new Timer();

        float x = 400;
        float y = 100;
        int radius = 25;

        float speedX = 0;
        float speedY = 0;

        float gravity = 0.5f;

        //1-вниз, 2-вверх, 3-влево, 4-вправо
        int gravityDirection = 1;

        Image ballImage;

        public Form1()
        {
            InitializeComponent();

            string path = Application.StartupPath + "\\ball.jpg";

            ballImage = Image.FromFile(path);

            Size = new Size(800, 600);
            DoubleBuffered = true;

            KeyDown += Form1_KeyDown;
            
            timer.Interval = 16;
            timer.Tick += Timer_Tick;
            timer.Start();

            Paint += Form1_Paint;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    gravityDirection = 1; //вниз
                    break;
                case Keys.Up:
                    gravityDirection = 2; //вверх
                    break;
                case Keys.Left:
                    gravityDirection = 3; //влево
                    break;
                case Keys.Right:
                    gravityDirection = 4; //вправо
                    break;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            switch (gravityDirection)
            {
                case 1: //вниз
                    speedY = speedY + gravity;
                    break;
                case 2: //вверх
                    speedY = speedY - gravity;
                    break;
                case 3: //влево
                    speedX = speedX - gravity;
                    break;
                case 4: //вправо
                    speedX = speedX + gravity;
                    break;
            }

            x = x + speedX;
            y = y + speedY;

            //Проверка столкновений
            //Левый край
            if (x - radius < 0)
            {
                x = radius;
                speedX = 0;
            }
            //Правый край
            if (x + radius > ClientSize.Width)
            {
                x = ClientSize.Width - radius;
                speedX = 0;
            }
            //Верхний край
            if (y - radius < 0)
            {
                y = radius;
                speedY = 0;
            }
            //Нижний край
            if (y + radius > ClientSize.Height)
            {
                y = ClientSize.Height - radius;
                speedY = 0;
            }

            Invalidate();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(ballImage, x - radius, y - radius, radius * 2, radius * 2);
        }
    }
}

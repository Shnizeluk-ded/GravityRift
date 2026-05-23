using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GravityRift
{
    public partial class Form1 : Form
    {
        Timer timer = new Timer();
        Timer gameTimer = new Timer();
        Timer startDelayTimer = new Timer();

        float x = 400;
        float y = 600;
        int radius = 25;

        float speedX = 0;
        float speedY = 0;

        float gravity = 0.5f;

        int gravityDirection = 1;

        Image ballImage;

        float holeX = 3050;
        float holeY = 500; // Сместили дыру в центр по вертикали
        int holeRadius = 30;

        bool isWin = false;
        bool isGameOver = false;
        bool canWaveMove = false;

        float waveX = 0;
        float waveSpeed = 3f;

        Button restartButton = new Button();
        float elapsedTime = 0f;
        Label timerLabel = new Label();

        // Камера для горизонтального скролла
        float cameraX = 0;
        int worldWidth = 3500;

        // Стены лабиринта
        List<Rectangle> walls = new List<Rectangle>();

        public Form1()
        {
            InitializeComponent();

            string path = Application.StartupPath + "\\ball.jpg";
            ballImage = Image.FromFile(path);

            Size = new Size(1600, 1000);
            DoubleBuffered = true;

            CreateMaze();

            KeyDown += Form1_KeyDown;

            timer.Interval = 16;
            timer.Tick += Timer_Tick;
            timer.Start();

            gameTimer.Interval = 10;
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            startDelayTimer.Interval = 2000;
            startDelayTimer.Tick += StartDelayTimer_Tick;
            startDelayTimer.Start();

            Paint += Form1_Paint;

            timerLabel.Text = "Время: 0.00 сек";
            timerLabel.Font = new Font("Arial", 16, FontStyle.Bold);
            timerLabel.ForeColor = Color.White;
            timerLabel.BackColor = Color.Transparent;
            timerLabel.AutoSize = true;
            timerLabel.Location = new Point(10, 10);
            Controls.Add(timerLabel);

            restartButton.Text = "Рестарт";
            restartButton.Size = new Size(120, 50);
            restartButton.Visible = false;
            restartButton.Click += RestartButton_Click;
            Controls.Add(restartButton);

            this.KeyPreview = true;

            SetStartPosition();
        }

        private void SetStartPosition()
        {
            x = 50 + radius;
            y = ClientSize.Height / 2; // Старт в центре по вертикали

            while (CheckCollisionWithWalls(x, y, radius))
            {
                x += 10;
                y += 10;
                if (y > ClientSize.Height - 100) y = ClientSize.Height / 2;
            }
        }

        private void StartDelayTimer_Tick(object sender, EventArgs e)
        {
            canWaveMove = true;
            startDelayTimer.Stop();
        }

        private void CreateMaze()
        {
            int centerY = ClientSize.Height / 2;

            // Внешние границы (рамка)
            walls.Add(new Rectangle(0, 0, worldWidth, 5)); // Верх
            walls.Add(new Rectangle(0, 0, 5, ClientSize.Height)); // Лево
            walls.Add(new Rectangle(worldWidth - 5, 0, 5, ClientSize.Height)); // Право
            walls.Add(new Rectangle(0, ClientSize.Height - 5, worldWidth, 5)); // Низ

            // === Секция 1 (0 - 700) ===
            walls.Add(new Rectangle(300, centerY - 300, 200, 5));
            walls.Add(new Rectangle(250, centerY - 380, 5, 150));

            // === Секция 2 (700 - 1400) ===
            walls.Add(new Rectangle(700, centerY - 300, 250, 5));
            walls.Add(new Rectangle(550, centerY - 230, 5, 200));
            walls.Add(new Rectangle(850, centerY - 320, 5, 250));
            walls.Add(new Rectangle(200, centerY - 100, 250, 5));

            // === Секция 3(1400 - 2100) ===
            walls.Add(new Rectangle(1150, centerY - 350, 150, 5));
            walls.Add(new Rectangle(1380, centerY - 320, 5, 350));
            walls.Add(new Rectangle(1150, centerY - 280, 5, 200));
            walls.Add(new Rectangle(650, centerY - 100, 300, 5));
            walls.Add(new Rectangle(1100, centerY - 100, 200, 5));

            // === Секция 4 (2100 - 2800) ===
            walls.Add(new Rectangle(400, centerY + 100, 350, 5));
            walls.Add(new Rectangle(350, centerY - 20, 5, 200));
            walls.Add(new Rectangle(750, centerY + 20, 5, 250));
            walls.Add(new Rectangle(1050, centerY - 20, 5, 200));
            walls.Add(new Rectangle(900, centerY + 100, 250, 5));

            // === Секция 5 (2800 - 3500) ===
            walls.Add(new Rectangle(250, centerY + 300, 300, 5));
            walls.Add(new Rectangle(1280, centerY + 20, 5, 150));
            walls.Add(new Rectangle(150, centerY + 180, 5, 180));
            walls.Add(new Rectangle(600, centerY + 220, 5, 180));
            walls.Add(new Rectangle(950, centerY + 220, 5, 180));
            walls.Add(new Rectangle(1180, centerY + 180, 5, 200));
            walls.Add(new Rectangle(750, centerY + 300, 300, 5));
            walls.Add(new Rectangle(1250, centerY + 200, 150, 5));

            // === Дополнительные стены для длины ===
            walls.Add(new Rectangle(1600, centerY - 200, 200, 5));
            walls.Add(new Rectangle(1700, centerY, 5, 200));
            walls.Add(new Rectangle(1900, centerY - 300, 250, 5));
            walls.Add(new Rectangle(1950, centerY - 380, 5, 150));
            walls.Add(new Rectangle(2200, centerY - 100, 300, 5));
            walls.Add(new Rectangle(2250, centerY - 20, 5, 200));
            walls.Add(new Rectangle(2500, centerY - 250, 200, 5));
            walls.Add(new Rectangle(2550, centerY - 320, 5, 150));
            walls.Add(new Rectangle(2800, centerY + 100, 250, 5));
            walls.Add(new Rectangle(2850, centerY + 20, 5, 200));
            walls.Add(new Rectangle(3100, centerY - 200, 200, 5));
            walls.Add(new Rectangle(3150, centerY - 280, 5, 200));

            // Вертикальные стены в новых секциях
            walls.Add(new Rectangle(450, centerY + 370, 5, 80));
            walls.Add(new Rectangle(800, centerY + 370, 5, 80));
            walls.Add(new Rectangle(1100, centerY + 370, 5, 80));
            walls.Add(new Rectangle(1450, centerY + 370, 5, 80));
            walls.Add(new Rectangle(1800, centerY + 370, 5, 80));
            walls.Add(new Rectangle(2150, centerY + 370, 5, 80));
            walls.Add(new Rectangle(2500, centerY + 370, 5, 80));
            walls.Add(new Rectangle(2850, centerY + 370, 5, 80));
            walls.Add(new Rectangle(3200, centerY + 370, 5, 80));

            // Горизонтальные стены для разнообразия
            walls.Add(new Rectangle(1650, centerY + 250, 200, 5));
            walls.Add(new Rectangle(2050, centerY + 150, 250, 5));
            walls.Add(new Rectangle(2400, centerY - 150, 200, 5));
            walls.Add(new Rectangle(2700, centerY, 250, 5));
            walls.Add(new Rectangle(3000, centerY + 250, 200, 5));
            walls.Add(new Rectangle(3300, centerY - 50, 150, 5));

            // Препятствия в центре
            walls.Add(new Rectangle(500, centerY - 50, 100, 5));
            walls.Add(new Rectangle(800, centerY + 150, 100, 5));
            walls.Add(new Rectangle(1200, centerY - 150, 100, 5));
            walls.Add(new Rectangle(1600, centerY + 50, 100, 5));
            walls.Add(new Rectangle(2000, centerY - 100, 100, 5));
            walls.Add(new Rectangle(2400, centerY + 100, 100, 5));
            walls.Add(new Rectangle(2800, centerY - 50, 100, 5));
            walls.Add(new Rectangle(3200, centerY + 50, 100, 5));
        }

        private bool CheckCollisionWithWalls(float newX, float newY, float radius)
        {
            foreach (Rectangle wall in walls)
            {
                float closestX =


Math.Max(wall.X, Math.Min(newX, wall.X + wall.Width));
                float closestY = Math.Max(wall.Y, Math.Min(newY, wall.Y + wall.Height));

                float dx = newX - closestX;
                float dy = newY - closestY;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                if (distance < radius)
                {
                    return true;
                }
            }
            return false;
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (!isWin && !isGameOver)
            {
                elapsedTime += 0.01f;
                timerLabel.Text = $"Время: {elapsedTime:F2} сек";
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                    gravityDirection = 1;
                    break;
                case Keys.Up:
                    gravityDirection = 2;
                    break;
                case Keys.Left:
                    gravityDirection = 3;
                    break;
                case Keys.Right:
                    gravityDirection = 4;
                    break;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (isWin || isGameOver) return;

            float newX = x;
            float newY = y;
            float newSpeedX = speedX;
            float newSpeedY = speedY;

            switch (gravityDirection)
            {
                case 1:
                    newSpeedY = speedY + gravity;
                    break;
                case 2:
                    newSpeedY = speedY - gravity;
                    break;
                case 3:
                    newSpeedX = speedX - gravity;
                    break;
                case 4:
                    newSpeedX = speedX + gravity;
                    break;
            }

            newX = x + newSpeedX;
            newY = y + newSpeedY;

            // Ограничение по Y, чтобы мяч не выходил за пределы экрана по вертикали
            newY = Math.Max(radius + 5, Math.Min(ClientSize.Height - radius - 5, newY));

            // Ограничение по X в пределах мира
            newX = Math.Max(radius, Math.Min(worldWidth - radius, newX));

            if (!CheckCollisionWithWalls(newX, newY, radius))
            {
                x = newX;
                y = newY;
                speedX = newSpeedX;
                speedY = newSpeedY;
            }
            else
            {
                if (!CheckCollisionWithWalls(x + newSpeedX, y, radius))
                {
                    x += newSpeedX;
                    speedX = newSpeedX;
                }
                else
                {
                    speedX = 0;
                }

                if (!CheckCollisionWithWalls(x, y + newSpeedY, radius))
                {
                    y += newSpeedY;
                    speedY = newSpeedY;
                }
                else
                {
                    speedY = 0;
                }
            }

            // Обновление камеры (только горизонтальный скролл)
            cameraX = x - ClientSize.Width / 2;
            cameraX = Math.Max(0, Math.Min(cameraX, worldWidth - ClientSize.Width));

            // Движение волны
            if (canWaveMove)
            {
                waveX += waveSpeed;
            }

            // Проверка касания волны с шариком
            if (waveX > 0 && canWaveMove && x < waveX)
            {
                isGameOver = true;
                speedX = 0;
                speedY = 0;
                restartButton.Visible = true;
                gameTimer.Stop();
                restartButton.Location = new Point((ClientSize.Width - restartButton.Width) / 2,
                                                    (ClientSize.Height - restartButton.Height) /


2);
            }

            // Проверка победы
            float dxHole = x - holeX;
            float dyHole = y - holeY;
            float distance = (float)Math.Sqrt(dxHole * dxHole + dyHole * dyHole);

            if (distance < holeRadius)
            {
                isWin = true;
                speedX = 0;
                speedY = 0;
                restartButton.Visible = true;
                gameTimer.Stop();
                restartButton.Location = new Point((ClientSize.Width - restartButton.Width) / 2,
                                                    (ClientSize.Height - restartButton.Height) / 2);
            }

            Invalidate();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            // Сохраняем состояние graphics для скролла
            e.Graphics.TranslateTransform(-cameraX, 0);

            // Рисование волны
            if (!isGameOver && !isWin && canWaveMove)
            {
                using (Brush waveBrush = new SolidBrush(Color.FromArgb(150, 0, 100, 200)))
                {
                    RectangleF waveRect = new RectangleF(0, 0, waveX, ClientSize.Height);
                    e.Graphics.FillRectangle(waveBrush, waveRect);
                }

                if (waveX < worldWidth)
                {
                    using (Pen wavePen = new Pen(Color.Cyan, 3))
                    {
                        e.Graphics.DrawLine(wavePen, waveX, 0, waveX, ClientSize.Height);
                    }
                }
            }
            else if (isGameOver)
            {
                using (Brush waveBrush = new SolidBrush(Color.FromArgb(200, 0, 100, 200)))
                {
                    e.Graphics.FillRectangle(waveBrush, cameraX, 0, ClientSize.Width, ClientSize.Height);
                }
            }

            // Рисование стен
            using (Brush wallBrush = new SolidBrush(Color.Black))
            {
                foreach (Rectangle wall in walls)
                {
                    e.Graphics.FillRectangle(wallBrush, wall);
                }
            }

            // Рисование шара
            e.Graphics.DrawImage(ballImage, x - radius, y - radius, radius * 2, radius * 2);

            // Рисование лунки
            e.Graphics.FillEllipse(Brushes.Black, holeX - holeRadius, holeY - holeRadius,
                holeRadius * 2, holeRadius * 2);

            // Возвращаем трансформацию для текста
            e.Graphics.ResetTransform();

            // Сообщения
            if (isWin)
            {
                string text = "Победа!";
                string timeText = $"Время: {elapsedTime:F2} секунд";
                Font font = new Font("Arial", 48, FontStyle.Bold);
                Font timeFont = new Font("Arial", 32, FontStyle.Bold);

                SizeF textSize = e.Graphics.MeasureString(text, font);
                SizeF timeSize = e.Graphics.MeasureString(timeText, timeFont);

                e.Graphics.DrawString(text, font, Brushes.Green,
                    (ClientSize.Width - textSize.Width) / 2, 150);
                e.Graphics.DrawString(timeText, timeFont, Brushes.Gold,
                    (ClientSize.Width - timeSize.Width) / 2, 220);
            }
            else if (isGameOver)
            {
                string text = "Поражение!";
                Font font = new Font("Arial", 48, FontStyle.Bold);
                SizeF textSize = e.Graphics.MeasureString(text, font);
                e.Graphics.DrawString(text, font, Brushes.Red,
                    (ClientSize.Width - textSize.Width) / 2, 150);
            }
        }

        private void RestartButton_Click(object sender, EventArgs e)
        {
            SetStartPosition();
            speedX = 0;
            speedY = 0;
            gravityDirection = 1;
            waveX = 0;
            cameraX = 0;
            isWin = false;
            isGameOver = false;
            restartButton.Visible = false;

            canWaveMove = false;

            elapsedTime = 0f;
            timerLabel.Text = "Время: 0.00 сек";
            gameTimer.Start();
            startDelayTimer.Start();
        }
    }
}

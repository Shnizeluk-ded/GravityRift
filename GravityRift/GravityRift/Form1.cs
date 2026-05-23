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
        float holeY = 500;

        int holeRadius = 30;

        bool isWin = false;
        bool isGameOver = false;
        bool canWaveMove = false;

        float waveX = 0;
        float waveSpeed = 3f;

        Button restartButton = new Button();

        float elapsedTime = 0f;

        Label timerLabel = new Label();

        float cameraX = 0;

        int worldWidth = 3500;

        List<Rectangle> walls = new List<Rectangle>();

        List<Spike> spikes = new List<Spike>();

        Random random = new Random();

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

            KeyPreview = true;

            SetStartPosition();
        }

        private class Spike
        {
            public Rectangle Rect;
            public string Direction;

            public Spike(Rectangle rect, string direction)
            {
                Rect = rect;
                Direction = direction;
            }
        }

        private void SetStartPosition()
        {
            x = 50 + radius;

            y = ClientSize.Height / 2;

            while (CheckCollisionWithWalls(x, y, radius))
            {
                x += 10;
                y += 10;

                if (y > ClientSize.Height - 100)
                    y = ClientSize.Height / 2;
            }
        }

        private void StartDelayTimer_Tick(object sender, EventArgs e)
        {
            canWaveMove = true;

            startDelayTimer.Stop();
        }

        private void CreateMaze()
        {
            walls.Clear();
            spikes.Clear();

            //рамка
            walls.Add(new Rectangle(0, 0, worldWidth, 5));
            walls.Add(new Rectangle(0, 0, 5, ClientSize.Height));
            walls.Add(new Rectangle(worldWidth - 5, 0, 5, ClientSize.Height));
            walls.Add(new Rectangle(0, ClientSize.Height - 5, worldWidth, 5));

            //рандомные стены
            int wallCount = 80;

            for (int i = 0; i < wallCount; i++)
            {
                bool vertical = random.Next(2) == 0;

                int wallX = random.Next(100, worldWidth - 150);

                int wallY =


random.Next(50, ClientSize.Height - 150);

                int wallLength = random.Next(80, 250);

                Rectangle newWall;

                if (vertical)
                    newWall = new Rectangle(wallX, wallY, 5, wallLength);
                else
                    newWall = new Rectangle(wallX, wallY, wallLength, 5);

                bool intersects = false;

                foreach (Rectangle wall in walls)
                {
                    Rectangle expandedWall = wall;

                    expandedWall.Inflate(20, 20);

                    if (expandedWall.IntersectsWith(newWall))
                    {
                        intersects = true;
                        break;
                    }
                }

                Rectangle startZone = new Rectangle(0, ClientSize.Height / 2 - 150, 250, 300);

                Rectangle finishZone = new Rectangle((int)holeX - 150, (int)holeY - 150, 300, 300);

                if (!intersects && !newWall.IntersectsWith(startZone) && !newWall.IntersectsWith(finishZone))
                {
                    walls.Add(newWall);
                }
            }

            //шипы
            foreach (Rectangle wall in walls)
            {
                if (wall.Width == worldWidth ||
                    wall.Height == ClientSize.Height)
                    continue;

                if (random.Next(100) > 20)
                    continue;

                bool horizontal = wall.Width > wall.Height;

                //горизонтальная стена
                if (horizontal)
                {
                    int spikeX = random.Next(wall.X, wall.X + wall.Width - 30);

                    bool topSide = random.Next(2) == 0;

                    if (topSide)
                        spikes.Add(new Spike(new Rectangle(spikeX, wall.Y - 30, 30, 30), "UP"));
                    else
                        spikes.Add(new Spike(new Rectangle(spikeX, wall.Y + 5, 30, 30), "DOWN"));
                }
                else
                {
                    int spikeY = random.Next(wall.Y, wall.Y + wall.Height - 30);

                    bool leftSide = random.Next(2) == 0;

                    if (leftSide)
                        spikes.Add(new Spike(new Rectangle(wall.X - 30, spikeY, 30, 30), "LEFT"));
                    else
                        spikes.Add(new Spike(new Rectangle(wall.X + 5, spikeY, 30, 30), "RIGHT"));
                }
            }
        }

        private bool CheckCollisionWithWalls(float newX, float newY, float radius)
        {
            foreach (Rectangle wall in walls)
            {
                float closestX =
                    Math.Max(
                        wall.X,
                        Math.Min(newX, wall.X + wall.Width)
                    );

                float closestY =
                    Math.Max(
                        wall.Y,
                        Math.Min(newY, wall.Y + wall.Height)
                    );

                float dx = newX - closestX;
                float dy = newY - closestY;

                float distance =
                    (float)Math.Sqrt(dx * dx + dy * dy);

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

                timerLabel.Text =
                    $"Время: {elapsedTime:F2} сек";
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
            if (isWin || isGameOver)
                return;

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

            newY =
                Math.Max(
                    radius + 5,
                    Math.Min(ClientSize.Height - radius - 5, newY)
                );

            newX =
                Math.Max(
                    radius,
                    Math.Min(worldWidth - radius, newX

)
                );

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

            Rectangle playerRect = new Rectangle((int)(x - radius), (int)(y - radius), radius * 2, radius * 2);

            foreach (Spike spike in spikes)
            {
                if (playerRect.IntersectsWith(spike.Rect))
                {
                    isGameOver = true;

                    speedX = 0;
                    speedY = 0;

                    restartButton.Visible = true;

                    gameTimer.Stop();

                    restartButton.Location = new Point((ClientSize.Width - restartButton.Width) / 2, (ClientSize.Height - restartButton.Height) / 2);

                    return;
                }
            }

            cameraX = x - ClientSize.Width / 2;

            cameraX = Math.Max(0, Math.Min(cameraX, worldWidth - ClientSize.Width));

            if (canWaveMove)
            {
                waveX += waveSpeed;
            }

            if (waveX > 0 &&
                canWaveMove &&
                x < waveX)
            {
                isGameOver = true;

                speedX = 0;
                speedY = 0;

                restartButton.Visible = true;

                gameTimer.Stop();

                restartButton.Location = new Point((ClientSize.Width - restartButton.Width) / 2, (ClientSize.Height - restartButton.Height) / 2);
            }

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

                restartButton.Location = new Point((ClientSize.Width - restartButton.Width) / 2, (ClientSize.Height -


restartButton.Height) / 2);
            }

            Invalidate();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(-cameraX, 0);

            //волна
            if (!isGameOver && !isWin && canWaveMove)
            {
                Brush waveBrush = new SolidBrush(Color.FromArgb(150, 0, 100, 200));
                RectangleF waveRect = new RectangleF(0, 0, waveX, ClientSize.Height);
                e.Graphics.FillRectangle(waveBrush, waveRect);
                waveBrush.Dispose();

                if (waveX < worldWidth)
                {
                    Pen wavePen = new Pen(Color.Cyan, 3);
                    e.Graphics.DrawLine(wavePen, waveX, 0, waveX, ClientSize.Height);
                    wavePen.Dispose();
                }
            }

            //стены
            using (Brush wallBrush = new SolidBrush(Color.Black))
            {
                foreach (Rectangle wall in walls)
                {
                    e.Graphics.FillRectangle(wallBrush, wall);
                }
            }

            //шипы
            using (Brush spikeBrush =
                new SolidBrush(Color.Red))
            {
                foreach (Spike spike in spikes)
                {
                    Point[] triangle;

                    if (spike.Direction == "UP")
                    {
                        triangle = new Point[]
                        {
                            new Point(spike.Rect.X, spike.Rect.Y),
                            new Point(spike.Rect.X + 30, spike.Rect.Y),
                            new Point(spike.Rect.X + 15, spike.Rect.Y + 30)
                        };
                    }
                    else if (spike.Direction == "DOWN")
                    {
                        triangle = new Point[]
                        {
                            new Point(spike.Rect.X + 15, spike.Rect.Y),
                            new Point(spike.Rect.X, spike.Rect.Y + 30),
                            new Point(spike.Rect.X + 30, spike.Rect.Y + 30)
                        };
                    }
                    else if (spike.Direction == "LEFT")
                    {
                        triangle = new Point[]
                        {
                            new Point(spike.Rect.X + 30, spike.Rect.Y),
                            new Point(spike.Rect.X + 30, spike.Rect.Y + 30),
                            new Point(spike.Rect.X, spike.Rect.Y + 15)
                        };
                    }
                    else
                    {
                        triangle = new Point[]
                        {
                            new Point(spike.Rect.X, spike.Rect.Y),
                            new Point(spike.Rect.X + 30, spike.Rect.Y + 15),
                            new Point(spike.Rect.X, spike.Rect.Y + 30)
                        };
                    }

                    e.Graphics.FillPolygon(
                        spikeBrush,
                        triangle
                    );
                }
            }

            //лунка
            e.Graphics.FillEllipse(
                Brushes.Black,
                holeX - holeRadius,
                holeY - holeRadius,
                holeRadius * 2,
                holeRadius * 2
            );

            //игрок
            e.Graphics.DrawImage(
                ballImage,
                x - radius,
                y - radius,
                radius * 2,
                radius * 2
            );

            e.Graphics.ResetTransform();
        }

        private void RestartButton_Click(object sender, EventArgs e)
        {
            CreateMaze();

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
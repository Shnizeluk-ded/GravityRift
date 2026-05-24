using System;
using System.Drawing;
using System.Windows.Forms;

namespace GravityRift
{
    public class GameView : Form
    {
        public Button RestartButton{ get; }
        public Label TimerLabel{ get; }

        public event Action<Keys> OnKeyPressed;

        //ссылка на модель
        private GameModel model;
        private Image ballImage;
        private float cameraX;

        public GameView(GameModel model)
        {
            this.model = model;

            //загружаем картинку шарика
            string path = Application.StartupPath + "\\ball.jpg";
            ballImage = Image.FromFile(path);

            Size = new Size(1600, 1000);
            DoubleBuffered = true;
            KeyPreview = true;
            Text = "GravityRift";

            //таймер
            TimerLabel = new Label
            {
                Text = "Время: 0.00 сек",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(10, 10)
            };
            Controls.Add(TimerLabel);

            //кнопка рестарта
            RestartButton = new Button
            {
                Text = "Рестарт",
                Size = new Size(120, 50),
                Visible = false
            };
            Controls.Add(RestartButton);

            //ввод
            KeyDown += (s, e) => OnKeyPressed?.Invoke(e.KeyCode);

            Paint += Form1_Paint;
        }

        //обновление камеры
        public void UpdateCamera()
        {
            cameraX = model.X - ClientSize.Width / 2f;
            cameraX = Math.Max(0, Math.Min(cameraX, GameModel.WorldWidth - ClientSize.Width));
        }

        //позиционирование кнопки по центру экрана
        public void CenterRestartButton()
        {
            RestartButton.Location = new Point((ClientSize.Width  - RestartButton.Width)  / 2, (ClientSize.Height - RestartButton.Height) / 2);
        }

        //отрисовка
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(-cameraX, 0);

            DrawWave(e.Graphics);
            DrawWalls(e.Graphics);
            DrawBouncePads(e.Graphics);
            DrawSpikes(e.Graphics);
            DrawHole(e.Graphics);
            DrawPlayer(e.Graphics);

            e.Graphics.ResetTransform();

            DrawOverlay(e.Graphics);
        }

        private void DrawWave(Graphics g)
        {
            if (model.IsGameOver || model.IsWin || !model.CanWaveMove) return;

            using (Brush waveBrush = new SolidBrush(Color.FromArgb(150, 0, 100, 200)))
                g.FillRectangle(waveBrush, 0, 0, model.WaveX, ClientSize.Height);

            if (model.WaveX < GameModel.WorldWidth)
            {
                using (Pen wavePen = new Pen(Color.Cyan, 3))
                    g.DrawLine(wavePen, model.WaveX, 0, model.WaveX, ClientSize.Height);
            }
        }

        private void DrawWalls(Graphics g)
        {
            using (Brush wallBrush = new SolidBrush(Color.Black))
            {
                foreach (Rectangle wall in model.Walls)
                {
                    bool invisible = false;
                    foreach (DisappearingWall dw in model.DisappearingWalls)
                        if (dw.Rect == wall && !dw.Active) { invisible = true; break; }

                    if (!invisible)
                        g.FillRectangle(wallBrush, wall);
                }
            }
        }

        private void DrawBouncePads(Graphics g)
        {
            using (Brush brush = new SolidBrush(Color.Lime))
                foreach (BouncePad pad in model.BouncePads)
                    g.FillEllipse(brush, pad.Rect);
        }

        private void DrawSpikes(Graphics g)
        {
            using (Brush brush = new SolidBrush(Color.Red))
            {
                foreach (Spike spike in model.Spikes)
                {
                    Point[] tri;
                    int x = spike.Rect.X, y = spike.Rect.Y;

                    switch (spike.Direction)
                    {
                        case "UP":
                            tri = new[] { new Point(x, y), new Point(x + 30, y), new Point(x + 15, y + 30) };
                            break;
                        case "DOWN":
                            tri = new[] { new Point(x + 15, y), new Point(x, y + 30), new Point(x + 30, y + 30) };
                            break;
                        case "LEFT":
                            tri = new[] { new Point(x + 30, y), new Point(x + 30, y + 30), new Point(x, y + 15) };
                            break;
                        default: // RIGHT
                            tri = new[] { new Point(x, y), new Point(x + 30, y + 15), new Point(x, y + 30) };
                            break;
                    }
                    g.FillPolygon(brush, tri);
                }
            }
        }

        private void DrawHole(Graphics g)
        {
            float hr = model.HoleRadius;
            g.FillEllipse(Brushes.Black,
                model.HoleX - hr, model.HoleY - hr, hr * 2, hr * 2);
        }

        private void DrawPlayer(Graphics g)
        {
            int r = GameModel.Radius;
            g.DrawImage(ballImage, model.X - r, model.Y - r, r * 2, r * 2);
        }

        private void DrawOverlay(Graphics g)
        {
            if (model.IsWin)
            {
                string winText  = "ПОБЕДА!";
                string timeText = $"Время: {model.ElapsedTime:F2} секунд";

                using (Font wf = new Font("Arial", 48, FontStyle.Bold))
                using (Font tf = new Font("Arial", 32, FontStyle.Bold))
                {
                    SizeF ws = g.MeasureString(winText, wf);
                    SizeF ts = g.MeasureString(timeText, tf);
                    g.DrawString(winText,  wf, Brushes.LimeGreen, (ClientSize.Width - ws.Width) / 2, 150);
                    g.DrawString(timeText, tf, Brushes.Gold, (ClientSize.Width - ts.Width) / 2, 220);
                }
            }
            else if (model.IsGameOver)
            {
                string loseText = "ПОРАЖЕНИЕ!";
                using (Font lf = new Font("Arial", 48, FontStyle.Bold))
                {
                    SizeF ls = g.MeasureString(loseText, lf);
                    g.DrawString(loseText, lf, Brushes.Red, (ClientSize.Width - ls.Width) / 2, 150);
                }
            }
        }
    }
}

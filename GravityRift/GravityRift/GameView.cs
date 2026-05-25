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
        private Image trampolineImage;
        private Image spikeImage;
        private Bitmap background;
        private float cameraX;

        public GameView(GameModel model)
        {
            this.model = model;

            string backgroundPath = Application.StartupPath + "\\background.jpg";
            using (var raw = Image.FromFile(backgroundPath))
            {
                background = new Bitmap(1600, 1000);
                using (var g = Graphics.FromImage(background))
                g.DrawImage(raw, 0, 0, 1600, 1000);
            }

            // загружаем картинку шарика
            string path = Application.StartupPath + "\\ball.png";
            ballImage = Image.FromFile(path);

            // загружаем картинку батута
            string trampolinePath = Application.StartupPath + "\\trampoline.png";
            trampolineImage = Image.FromFile(trampolinePath);

            // загружаем картинку шипа
            string spikePath = Application.StartupPath + "\\spike.png";
            spikeImage = Image.FromFile(spikePath);

            Size = new Size(1600, 1000);
            DoubleBuffered = true;
            KeyPreview = true;
            Text = "GravityRift";

            //таймер
            TimerLabel = new Label
            {
                Text = "Время: 0.00 сек",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.Black,
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
            e.Graphics.DrawImageUnscaled(background, 0, 0);
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
            foreach (BouncePad pad in model.BouncePads)
            {
                Rectangle r = pad.Rect;
                var state = g.Save();

                float cx = r.X + r.Width / 2f;
                float cy = r.Y + r.Height / 2f;

                float angle;
                switch (pad.Direction)
                {
                    case "UP": angle = 0f; break;
                    case "DOWN": angle = 180f; break;
                    case "LEFT": angle = 90f; break;
                    case "RIGHT": angle = 270f; break;
                    default: angle = 0f; break;
                }

                g.TranslateTransform(cx, cy);
                g.RotateTransform(angle);
                g.TranslateTransform(-cx, -cy);

                // Рисуем квадратом от центра, не растягивая
                int size = Math.Min(r.Width, r.Height);
                Rectangle drawRect = new Rectangle(
                    (int)(cx - size / 2f),
                    (int)(cy - size / 2f),
                    size, size);

                g.DrawImage(trampolineImage, drawRect);

                g.Restore(state);
            }
        }

        private void DrawSpikes(Graphics g)
        {
            foreach (Spike spike in model.Spikes)
            {
                Rectangle r = spike.Rect;
                var state = g.Save();

                float cx = r.X + r.Width / 2f;
                float cy = r.Y + r.Height / 2f;

                float angle;
                switch (spike.Direction)
                {
                    case "UP": angle = 180f; break;
                    case "DOWN": angle = 0f; break;
                    case "LEFT": angle = 270f; break;
                    case "RIGHT": angle = 90f; break;
                    default: angle = 0f; break;
                }

                g.TranslateTransform(cx, cy);
                g.RotateTransform(angle);
                g.TranslateTransform(-cx, -cy);

                float scale = 1.5f;
                int scaledWidth = (int)(r.Width * scale);
                int scaledHeight = (int)(r.Height * scale);

                Rectangle scaledRect = new Rectangle(
                    (int)(cx - scaledWidth / 2f),
                    (int)(cy - scaledHeight / 2f),
                    scaledWidth,
                    scaledHeight
                );

                g.DrawImage(spikeImage, scaledRect);

                g.Restore(state);
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
            int newSize = 70;

            float drawX = model.X - newSize / 2f;
            float drawY = model.Y - newSize / 2f;

            g.DrawImage(ballImage, drawX, drawY, newSize, newSize);
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

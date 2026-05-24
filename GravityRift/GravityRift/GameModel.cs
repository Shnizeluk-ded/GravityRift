using System;
using System.Collections.Generic;
using System.Drawing;

namespace GravityRift
{
    public class Spike
    {
        public Rectangle Rect;
        public string Direction;
        public Spike(Rectangle rect, string direction) { Rect = rect; Direction = direction; }
    }

    public class BouncePad
    {
        public Rectangle Rect;
        public BouncePad(Rectangle rect) { Rect = rect; }
    }

    public class DisappearingWall
    {
        public Rectangle Rect;
        public bool Active = true;
        public DisappearingWall(Rectangle rect) { Rect = rect; }
    }

    //модель
    public class GameModel
    {
        //константы
        public const int WorldWidth = 3500;
        public const int Radius = 25;
        public const float Gravity = 0.5f;
        public const float WaveSpeed = 3f;

        //состояние шарика
        public float X { get; private set; }
        public float Y { get; private set; }
        public float SpeedX { get; private set; }
        public float SpeedY { get; private set; }

        //направление гравитации: 1=вниз 2=вверх 3=влево 4=вправо
        public int GravityDirection { get; set; } = 1;

        //лунка
        public float HoleX { get; } = 3050;
        public float HoleY { get; } = 500;
        public int HoleRadius { get; } = 30;

        // волна
        public float WaveX { get; set; }
        public bool CanWaveMove { get; set; }

        // таймер мигания исчезающих стен
        public int BlinkTimer { get; set; }

        // флаги
        public bool IsWin { get; set; }
        public bool IsGameOver { get; set; }

        // время
        public float ElapsedTime { get; set; }

        // мир
        public List<Rectangle> Walls { get; } = new List<Rectangle>();
        public List<Spike> Spikes { get; } = new List<Spike>();
        public List<BouncePad> BouncePads { get; } = new List<BouncePad>();
        public List<DisappearingWall> DisappearingWalls { get; } = new List<DisappearingWall>();

        //размер экрана
        private int screenHeight;
        private Random random = new Random();

        //инициализация
        public GameModel(int screenHeight)
        {
            this.screenHeight = screenHeight;
        }

        public void Reset()
        {
            SpeedX = SpeedY = 0;
            GravityDirection = 1;
            WaveX = 0;
            CanWaveMove = false;
            IsWin = false;
            IsGameOver = false;
            ElapsedTime = 0f;
            BlinkTimer = 0;

            CreateMaze();
            PlacePlayerAtStart();
        }

        //генерация уровня
        private void CreateMaze()
        {
            Walls.Clear();
            Spikes.Clear();
            BouncePads.Clear();
            DisappearingWalls.Clear();

            //рамка
            Walls.Add(new Rectangle(0, 0, WorldWidth, 5));
            Walls.Add(new Rectangle(0, 0, 5, screenHeight));
            Walls.Add(new Rectangle(WorldWidth - 5, 0, 5, screenHeight));
            Walls.Add(new Rectangle(0, screenHeight - 5, WorldWidth, 5));

            //случайные стены
            for (int i = 0; i < 80; i++)
            {
                bool vertical = random.Next(2) == 0;
                int  wallX = random.Next(100, WorldWidth - 150);
                int  wallY = random.Next(50, screenHeight - 150);
                int  wallLength = random.Next(80, 250);

                Rectangle newWall = vertical ? new Rectangle(wallX, wallY, 5, wallLength) : new Rectangle(wallX, wallY, wallLength, 5);

                bool intersects = false;
                foreach (Rectangle w in Walls)
                {
                    Rectangle expanded = w;
                    expanded.Inflate(20, 20);
                    if (expanded.IntersectsWith(newWall)) { intersects = true; break; }
                }

                Rectangle startZone = new Rectangle(0, screenHeight / 2 - 150, 250, 300);
                Rectangle finishZone = new Rectangle((int)HoleX - 150, (int)HoleY - 150, 300, 300);

                if (!intersects && !newWall.IntersectsWith(startZone) && !newWall.IntersectsWith(finishZone))
                {
                    Walls.Add(newWall);

                    if (random.Next(100) < 10)
                        DisappearingWalls.Add(new DisappearingWall(new Rectangle(newWall.X, newWall.Y, newWall.Width, newWall.Height)));
                }
            }

            //батуты
            foreach (Rectangle wall in Walls)
            {
                if (wall.Width == WorldWidth || wall.Height == screenHeight) continue;
                if (IsDisappearing(wall)) continue;
                if (random.Next(100) > 12) continue;

                bool vertical = wall.Height > wall.Width;
                BouncePads.Add(vertical ? new BouncePad(new Rectangle(wall.X - 10, wall.Y + wall.Height / 2, 25, 25)) : new BouncePad(new Rectangle(wall.X + wall.Width / 2, wall.Y - 10, 25, 25)));
            }

            // шипы
            foreach (Rectangle wall in Walls)
            {
                if (wall.Width == WorldWidth || wall.Height == screenHeight) continue;
                if (IsDisappearing(wall)) continue;
                if (random.Next(100) > 20) continue;

                bool horizontal = wall.Width > wall.Height;
                if (horizontal)
                {
                    int spikeX = random.Next(wall.X, wall.X + wall.Width - 30);
                    bool topSide = random.Next(2) == 0;
                    Spikes.Add(topSide ? new Spike(new Rectangle(spikeX, wall.Y - 30, 30, 30), "DOWN") : new Spike(new Rectangle(spikeX, wall.Y + 5, 30, 30),  "UP"));
                }
                else
                {
                    int  spikeY = random.Next(wall.Y, wall.Y + wall.Height - 30);
                    bool leftSide = random.Next(2) == 0;
                    Spikes.Add(leftSide ? new Spike(new Rectangle(wall.X - 30, spikeY, 30, 30), "LEFT") : new Spike(new Rectangle(wall.X + 5,  spikeY, 30, 30), "RIGHT"));
                }
            }
        }

        private bool IsDisappearing(Rectangle wall)
        {
            foreach (DisappearingWall dw in DisappearingWalls)
                if (dw.Rect == wall) return true;
            return false;
        }

        private void PlacePlayerAtStart()
        {
            X = 50 + Radius;
            Y = screenHeight / 2f;

            while (CheckCollision(X, Y))
            {
                X += 10;
                Y += 10;
                if (Y > screenHeight - 100) Y = screenHeight / 2f;
            }
        }

        //обновление физики
        public void Update()
        {
            if (IsWin || IsGameOver) return;

            //мигание стен
            BlinkTimer++;
            foreach (DisappearingWall dw in DisappearingWalls)
                dw.Active = (BlinkTimer / 100) % 2 == 0;

            //применяем гравитацию
            float newSpeedX = SpeedX;
            float newSpeedY = SpeedY;

            switch (GravityDirection)
            {
                case 1: newSpeedY += Gravity; break;
                case 2: newSpeedY -= Gravity; break;
                case 3: newSpeedX -= Gravity; break;
                case 4: newSpeedX += Gravity; break;
            }

            float newX = X + newSpeedX;
            float newY = Y + newSpeedY;

            newY = Math.Max(Radius + 5, Math.Min(screenHeight - Radius - 5, newY));
            newX = Math.Max(Radius, Math.Min(WorldWidth - Radius, newX));

            if (!CheckCollision(newX, newY))
            {
                X = newX; Y = newY;
                SpeedX = newSpeedX; SpeedY = newSpeedY;
            }
            else
            {
                if (!CheckCollision(X + newSpeedX, Y)) { X += newSpeedX; SpeedX = newSpeedX; }
                else SpeedX = 0;

                if (!CheckCollision(X, Y + newSpeedY)) { Y += newSpeedY; SpeedY = newSpeedY; }
                else SpeedY = 0;
            }

            Rectangle playerRect = new Rectangle((int)(X - Radius), (int)(Y - Radius), Radius * 2, Radius * 2);

            //шипы
            foreach (Spike spike in Spikes)
            {
                if (playerRect.IntersectsWith(spike.Rect))
                {
                    TriggerGameOver();
                    return;
                }
            }

            //батуты
            foreach (BouncePad pad in BouncePads)
            {
                if (playerRect.IntersectsWith(pad.Rect))
                {
                    switch (GravityDirection)
                    {
                        case 1: SpeedY = -15; break;
                        case 2: SpeedY = 15; break;
                        case 3: SpeedX = 15; break;
                        case 4: SpeedX = -15; break;
                    }
                }
            }

            //волна
            if (CanWaveMove) WaveX += WaveSpeed;

            if (CanWaveMove && WaveX > 0 && X < WaveX)
            {
                TriggerGameOver();
                return;
            }

            //лунка
            float dx = X - HoleX, dy = Y - HoleY;
            if (Math.Sqrt(dx * dx + dy * dy) < HoleRadius)
                TriggerWin();
        }

        public void TickTimer()
        {
            if (!IsWin && !IsGameOver)
                ElapsedTime += 0.01f;
        }

        public bool CheckCollision(float px, float py)
        {
            foreach (Rectangle wall in Walls)
            {
                bool invisible = false;
                foreach (DisappearingWall dw in DisappearingWalls)
                    if (dw.Rect == wall && !dw.Active) { invisible = true; break; }
                if (invisible) continue;

                float cx = Math.Max(wall.X, Math.Min(px, wall.X + wall.Width));
                float cy = Math.Max(wall.Y, Math.Min(py, wall.Y + wall.Height));
                float dx = px - cx, dy = py - cy;
                if (dx * dx + dy * dy < Radius * Radius) return true;
            }
            return false;
        }

        private void TriggerGameOver()
        {
            SpeedX = SpeedY = 0;
            IsGameOver = true;
        }

        private void TriggerWin()
        {
            SpeedX = SpeedY = 0;
            IsWin = true;
        }
    }
}

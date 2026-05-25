using System.Windows.Forms;

namespace GravityRift
{
    public class GamePresenter
    {
        private readonly GameModel model;
        private readonly GameView  view;

        private readonly Timer physicsTimer = new Timer(); 
        private readonly Timer gameTimer = new Timer(); 
        private readonly Timer startDelayTimer = new Timer();

        private bool wasWin;
        private bool wasGameOver;

        public GamePresenter(GameModel model, GameView view)
        {
            this.model = model;
            this.view  = view;

            this.view.OnKeyPressed += OnKeyPressed;
            this.view.RestartButton.Click += OnRestartClicked;

            //таймеры
            physicsTimer.Interval = 16;
            physicsTimer.Tick    += OnPhysicsTick;

            gameTimer.Interval = 10;
            gameTimer.Tick    += OnGameTimerTick;

            startDelayTimer.Interval = 2000;
            startDelayTimer.Tick    += OnStartDelayTick;
        }

        //запуск
        public void Start()
        {
            model.Reset();

            physicsTimer.Start();
            gameTimer.Start();
            startDelayTimer.Start();
        }

        //обработка ввода
        private void OnKeyPressed(Keys key)
        {
            switch (key)
            {
                case Keys.Down: model.GravityDirection = 1; view.PlaySound("gravity.mp3"); break;
                case Keys.Up: model.GravityDirection = 2; view.PlaySound("gravity.mp3"); break;
                case Keys.Left: model.GravityDirection = 3; view.PlaySound("gravity.mp3"); break;
                case Keys.Right: model.GravityDirection = 4; view.PlaySound("gravity.mp3"); break;
            }
        }

        //тики
        private void OnPhysicsTick(object sender, System.EventArgs e)
        {
            model.Update();

            // победа
            if (model.IsWin && !wasWin)
            {
                view.PlaySound("win.mp3");
                wasWin = true;
            }

            // поражение / смерть
            if (model.IsGameOver && !wasGameOver)
            {
                view.PlaySound("death.mp3");
                wasGameOver = true;
            }

            // отскок от батута
            if (model.JustBounced)
            {
                view.PlaySound("bounce.mp3");
                model.JustBounced = false;
            }

            view.UpdateCamera();

            view.TimerLabel.Text = $"Время: {model.ElapsedTime:F2} сек";

            if (model.IsWin || model.IsGameOver)
            {
                gameTimer.Stop();

                view.RestartButton.Visible = true;
                view.CenterRestartButton();
            }

            view.Invalidate();
        }

        private void OnGameTimerTick(object sender, System.EventArgs e)
        {
            model.TickTimer();
        }

        private void OnStartDelayTick(object sender, System.EventArgs e)
        {
            model.CanWaveMove = true;
            startDelayTimer.Stop();
        }

        //рестарт
        private void OnRestartClicked(object sender, System.EventArgs e)
        {
            model.Reset();

            wasWin = false;
            wasGameOver = false;

            view.RestartButton.Visible = false;
            view.TimerLabel.Text = "Время: 0.00 сек";

            gameTimer.Start();
            startDelayTimer.Start();
        }

        public static void Run()
        {
            GameModel model = new GameModel(1000 - 40);
            GameView view = new GameView(model);
            GamePresenter presenter = new GamePresenter(model, view);

            presenter.Start();

            Application.Run(view);
        }
    }
}

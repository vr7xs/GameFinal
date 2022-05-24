using Game.Resources;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Game
{
    public partial class GameBackGround : Form
    {
        private GameModel gameModel;
        private EventHandler action;
        private int invulnerability = 0;
        private int count = 0;
        private int winCount = 1;
        private int enemySpawnCount = 400;
        private bool isLevelEnd = true;
        private bool isStoryStart = true;

        public GameBackGround()
        {
            InitializeComponent();
            gameModel = new GameModel();
            GetHP();
            StoryTell();
            gameModel.Hero.OnMove += location =>
            {
                MainHero.Location = location;
            };
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);
            UpdateStyles();
        }

        private void Sprite_Load(object sender, EventArgs e)
        {
            LoadParameters.BulletSpecifications();
            LoadParameters.BoxSpecifications(MainHero);
            MainHero.Image = HeroMoveAnimations.right;
            gameModel.Hero.location = MainHero.Location;
            MainHero.DataBindings.Add(new Binding("", gameModel.Hero, ""));
        }

        public void IntersectsWithEnemy()
        {
            PictureBox deleteEnemy = null;
            foreach (PictureBox en in LoadParameters.enemyCount)
            {
                if (LoadParameters.bullet.Bounds.IntersectsWith(en.Bounds))
                {
                    PlaySound.Play(Sounds.hitEnemy);
                    Controls.Remove(LoadParameters.bullet);
                    gameModel.Hero.KillCountUpdate();
                    Controls.Remove(en);
                    deleteEnemy = en;
                }
                if (MainHero.Bounds.IntersectsWith(en.Bounds))
                {
                    if(count - invulnerability > 100)
                    {
                        invulnerability = count;
                        PlaySound.Play(Sounds.hitHeart);
                        gameModel.Hit();
                        GetHP();
                    }
                }
            }
            LoadParameters.enemyCount.Remove(deleteEnemy);
        }

        private void GetHP()
        {
            LivePicture.SizeMode = PictureBoxSizeMode.Zoom;
            switch (gameModel.liveCount)
            {
                case 3:
                    LivePicture.Image = HP._3hp;
                    break;
                case 2:
                    LivePicture.Image = HP._2hp;
                    break;
                case 1:
                    LivePicture.Image = HP._1hp;
                    break;
            }
        }

        private void StoryTell()
        {
            StoryMode();
            switch (gameModel.currentLevel)
            {
                case 0:
                    BackgroundImage = Story.start;
                    break;
                case 5:
                    BackgroundImage = Story.winner;
                    break;
                default:
                    BackgroundImage = Story.castle;
                    break;
            }
            BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void LevelChange()
        {
            GamePlayMode();
            switch (gameModel.currentLevel)
            {
                case 1:
                    BackgroundImage = GameBG.lvl_1;
                    LoadParameters.enemyImage = EnemyMovement.spider;
                    break;
                case 2:
                    BackgroundImage = GameBG.lvl_2;
                    LoadParameters.enemyImage = EnemyMovement.skelet;
                    break;
                case 3:
                    BackgroundImage = GameBG.lvl_3;
                    LoadParameters.enemyImage = EnemyMovement.ghost;
                    break;
                case 4:
                    BackgroundImage = GameBG.lvl_4;
                    LoadParameters.enemyImage = EnemyMovement.zombi;
                    break;
                case 5:
                    BackgroundImage = GameBG.lvl_5;
                    LoadParameters.enemyImage = EnemyMovement.eye;
                    break;
                case 6:
                    EndOfGame.Text = "Вы победили";
                    EndOfGame.Visible = true;
                    GameProgressTimer.Stop();
                    break;
            }
            BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void StoryMode()
        {
            LivePicture.Visible = false;
            MainHero.Visible = false;
            EndOfGame.Visible = false;
            KillLable.Visible = false;
            BoxCount.Visible = false;
            LoadParameters.enemyCount.Clear();
            GameProgressTimer.Stop();
        }

        private void GamePlayMode()
        {
            LivePicture.Visible = true;
            MainHero.Visible = true;
            EndOfGame.Visible = true;
            KillLable.Visible = true;
            BoxCount.Visible = true;
            Controls.Add(LoadParameters.box);
            GameProgressTimer.Start();
        }

        public void IntersectsWithBox()
        {
            if (MainHero.Bounds.IntersectsWith(LoadParameters.box.Bounds))
            {
                gameModel.PickBox();
                PlaySound.Play(Sounds.pickupBox);
                Controls.Remove(LoadParameters.box);
                LoadParameters.BoxSpecifications(MainHero);
                Controls.Add(LoadParameters.box);
            }
        }

        private void Game_KeyDown(object sender, KeyEventArgs e)
        {
            MoveTimer.Start();
            MoveTimer.Tick -= action;
            action = (obj, args) => gameModel.HeroMove(e.KeyCode, MainHero);
            MoveTimer.Tick += action;
            if (isStoryStart && e.KeyCode == Keys.Enter)
            {
                StoryTell();
                isStoryStart = false;
            }
            else if (isLevelEnd && e.KeyCode == Keys.Enter)
            {
                NextLevelGeneration();
                enemySpawnCount -= 15;
                winCount += 25;
                isLevelEnd = false;
                if(gameModel.currentLevel == 6) { Application.Exit(); }
            }
        }

        private void Game_KeyUp(object sender, KeyEventArgs e)
        {
            MoveTimer.Stop();
        }

        private void GameProgressTimer_Tick(object sender, EventArgs e)//проверяет состояние обьектов каждый фрэйм
        {
            IntersectsWithBox();
            IntersectsWithEnemy();
            KillLable.Text = $"Противников убито: {gameModel.Hero.enemyKillCount}";
            BoxCount.Text = $"Коробочек собранно: {gameModel.Hero.boxCount}";
            count++;
            gameModel.Hero.Shoot();
            Controls.Add(LoadParameters.bullet);
            if (count % 10 == 0)
            {
                foreach(var child in Controls)
                {
                    if (child is PictureBox box)
                        if(box.DataBindings.Count != 0)
                            if (box.DataBindings[0].DataSource is Enemy enemy)
                                enemy.Move();
                }
            }
            if (count % enemySpawnCount == 0)
            {
                LoadParameters.EnemySpawn(MainHero);
                Controls.Add(LoadParameters.enemySprite);
            }
            if (gameModel.liveCount == 0) GameOver();
            if (gameModel.Hero.boxCount >= winCount) GameWon();
        }

        private void GameWon()
        {
            PlaySound.Play(Sounds.win);
            EndOfGame.Visible = true;
            EndOfGame.Text = "Уровень пройден.";
            invulnerability = 0;
            count = 0;
            GameProgressTimer.Stop();
            foreach(PictureBox enemy in LoadParameters.enemyCount)
            {
                Controls.Remove(enemy);
            }
            Controls.Remove(LoadParameters.box);
            isStoryStart = true;
            isLevelEnd = true;
        }

        private void NextLevelGeneration()
        {
            gameModel.NextLevel();
            GetHP();
            LevelChange();
            foreach (PictureBox en in LoadParameters.enemyCount)
            {
                Controls.Remove(en);
            }
            LoadParameters.enemyCount.Clear();
            EndOfGame.Visible = false;
            Controls.Add(LoadParameters.box);
            GameProgressTimer.Start();
        }

        private void GameOver()
        {
            PlaySound.Play(Sounds.lose);
            LivePicture.Image = HP._0hp;
            MainHero.Visible = false;
            EndOfGame.Visible = true;
            EndOfGame.Text = "Игра окончена.";
            GameProgressTimer.Stop();
        }
    }
}

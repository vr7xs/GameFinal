using System;
using System.Drawing;
using System.Windows.Forms;

namespace Game
{
    class Enemy : Character
    {
        private Point direction = Point.Empty;
        private int enemyMoveSpeed { get; set; }

        public Enemy(int speed) : base(speed) 
        {
            enemyMoveSpeed = speed;
        }

        public void Move()//рандомное перемещени врагов
        {
            Random random = new Random();
            foreach (PictureBox enemy in LoadParameters.enemyCount)
            {
                direction.X = random.Next(-1, 2);
                direction.Y = random.Next(-1, 2);
                Point point = enemy.Location;
                if (point.X < 0) enemy.Left = 1190;
                if (point.X > 1190) enemy.Left = 0;
                if (point.Y < 0) enemy.Top = 590;
                if (point.Y > 590) enemy.Top = 0;
                enemy.Left += direction.X * enemyMoveSpeed * 4;
                enemy.Top += direction.Y * enemyMoveSpeed * 4;
            }
        }
    }
}

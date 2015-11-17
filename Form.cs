using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleShooter
{
    public struct Coords
    {
        public int X, Y;
    }

    public partial class Form : System.Windows.Forms.Form
    {
        public Form()
        {
            InitializeComponent();
        }

        public delegate void MoveHandle(Player pl);

        int RowCount, ColumnCount;

        private BufferedGraphicsContext context;
        private BufferedGraphics grafx;

        PictureBox icoAshe = new PictureBox();
        PictureBox icoKayle = new PictureBox();
        PictureBox icoNidalee = new PictureBox();
        PictureBox icoSivir = new PictureBox();
        PictureBox icoVi = new PictureBox();

        UserPlayer user;

        List<Player> players = new List<Player>();
        List<Barrier> barriers = new List<Barrier>();
        List<RipStone> rips = new List<RipStone>();
        Dictionary<Coords, Player> PlayerCoords = new Dictionary<Coords, Player>();
        Dictionary<Coords, Barrier> BarrierCoords = new Dictionary<Coords, Barrier>();

        private List<Rectangle> shotRadiusRectangles = new List<Rectangle>();
        private List<Rectangle> userMoveRectangles = new List<Rectangle>();
        private List<Rectangle> userShotRectangles = new List<Rectangle>();

        bool gameIsRunning = false;

        private void btNewGame_Click(object sender, EventArgs e)
        {
            tableLayoutPanel.Controls.Clear();
            tableLayoutPanel.Visible=false;

            RowCount = tableLayoutPanel.RowCount;
            ColumnCount = tableLayoutPanel.ColumnCount;

            PlayerCoords.Clear();
            BarrierCoords.Clear();
            players.Clear();
            barriers.Clear();
            rips.Clear();

            userMoveRectangles.Clear();
            userShotRectangles.Clear();
            shotRadiusRectangles.Clear();

            Player Ashe = new Player("Эш", picbAshe);
            Player Kayle = new Player("Кайла", picbKayle);
            Player Nidalee = new Player("Нидали", picbNidalee);
            Player Sivir = new Player("Сивир", picbSivir);
            Player Vi = new Player("Ви", picbVi);

            players.Add(Ashe);
            players.Add(Kayle);
            players.Add(Nidalee);
            players.Add(Sivir);
            players.Add(Vi);

            Coords C = new Coords();
            Random r = new Random();
            

            //Размещение препятствий
            for (int i = 0; i < 40; i++)
            {
                barriers.Add(new Barrier());

                barriers[barriers.Count - 1].Picture.BackgroundImage = (Bitmap)picbStone.BackgroundImage.Clone();
                barriers[barriers.Count - 1].Picture.BackgroundImageLayout = ImageLayout.Zoom;

                bool done = false;
                while (!done)
                {
                    C.X = r.Next(20);
                    C.Y = r.Next(20);

                    //проверка занятости ячейки
                    if (tableLayoutPanel.GetControlFromPosition(C.X, C.Y) == null)
                    {
                        tableLayoutPanel.Controls.Add(barriers[barriers.Count - 1].Picture, C.X, C.Y);

                        barriers[barriers.Count - 1].X = C.X;
                        barriers[barriers.Count - 1].Y = C.Y;

                        done = true;
                    }

                }

                BarrierCoords.Add(C, barriers[barriers.Count - 1]);
            }


            //Размещение персонажей
            foreach (Player pl in players)
            {
                bool done = false;
                while (!done)
                {
                    C.X = r.Next(20);
                    C.Y = r.Next(20);

                    //проверка занятости ячейки
                    if(tableLayoutPanel.GetControlFromPosition(C.X, C.Y) == null)
                    {
                        tableLayoutPanel.Controls.Add(pl.Picture, C.X, C.Y);

                        pl.X = C.X;
                        pl.Y = C.Y;
                        PlayerCoords.Add(C, pl);

                        done = true;
                    }
                }
            }

            user = new UserPlayer("Удир", picbUdyr, "StoneSword");//создание пользовательского игрока

            bool[,] ableUserPoses = new bool[20, 20];
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    ableUserPoses[i, j] = true;
                }
            }
            foreach(Player pl in players)
            {
                List<Rectangle> shotArea = GetShotRadiusRectangles(pl);

                //простреливаемые места недоступны
                foreach (Rectangle rect in shotArea)
                {
                    ableUserPoses[MapX2Cell(rect.X + 2), MapY2Cell(rect.Y + 2)] = false;
                }

                pl.Picture.MouseClick += user.MouseClick;//чтобы нажатие на игрока обрабатывалось игроком пользователя
            }
            foreach(Barrier barrier in barriers)
            {
                ableUserPoses[barrier.X, barrier.Y] = false;//места, заняты барьерами, недоступны

                barrier.Picture.MouseClick += user.MouseClick;//чтобы нажатие на барьер обрабатывалось игроком пользователя
            }

            List<Coords> ablePoses = new List<Coords>();//список непростреливаемых и доступных мест
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    if (ableUserPoses[i, j])
                    {
                        C.X = i;
                        C.Y = j;
                        ablePoses.Add(C);
                    }
                }
            }

            int k = r.Next(ablePoses.Count);//выбор случайного из свободных мест

            user.X = ablePoses[k].X;
            user.Y = ablePoses[k].Y;

            tableLayoutPanel.Controls.Add(user.Picture, user.X, user.Y);//размещение пользовательского игрока на поле боя

            tableLayoutPanel.Visible = true;

            context = BufferedGraphicsManager.Current;
            grafx = context.Allocate(tableLayoutPanel.CreateGraphics(), new Rectangle(0, 0, tableLayoutPanel.Width, tableLayoutPanel.Height));


            tableLayoutPanel.MouseClick += user.MouseClick;
            user.mapX = MapX2Cell;//привязка для UserPlayer
            user.mapY = MapY2Cell;
            user.isBusy = IsBusy;
            user.damagePos = DamagePos;
            user.step = CheckCellUnderStep;
            user.endStep = AIActions;

            gameIsRunning = true;
        }


        /// <summary>
        /// Функция хождения игрока
        /// </summary>
        /// <param name="pl"></param>
        public void Turn(Player pl)
        {
            pl.NextPosX = user.X - pl.X;
            pl.NextPosY = user.Y - pl.Y;

            Coords DP = DamagePos(pl);
            if(DP.X!=0 && DP.Y!=0)//если атака идёт по диагонали
            {
                if(Math.Abs(DP.X)>pl.Weapon.Radius-1)
                {
                    DP.X = Math.Sign(DP.X) * (pl.Weapon.Radius - 1);
                    DP.Y = Math.Sign(DP.Y) * (pl.Weapon.Radius - 1);
                }
            }
            else
            {
                if (Math.Abs(DP.X) > pl.Weapon.Radius || Math.Abs(DP.Y) > pl.Weapon.Radius)
                {
                    DP.X = Math.Sign(DP.X) * pl.Weapon.Radius;
                    DP.Y = Math.Sign(DP.Y) * pl.Weapon.Radius;
                }
            }
            
            if (DP.X + pl.X == user.X && DP.Y + pl.Y == user.Y)//если пользователь в зоне обстрела и досягаем  // && 
            {

                MessageBox.Show("Вы проиграли! Вас убил игрок "+pl.Name+".");
                
                tableLayoutPanel.Controls.Clear();
                players.Clear();
                PlayerCoords.Clear();
                gameIsRunning = false;
            }
            else//тогда ходим
            {
                bool[,] variousSteps = new bool[3, 3];
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        variousSteps[i, j] = true;
                    }
                }


                if (pl.X == 19)//если игрок на правой границе
                {
                    for (int j = 0; j < 3; j++)
                    {
                        variousSteps[2, j] = false;
                    }
                }
                else if (pl.X == 0)//если игрок на левой границе
                {
                    for (int j = 0; j < 3; j++)
                    {
                        variousSteps[0, j] = false;
                    }
                }

                if (pl.Y == 19)//если игрок на нижней границе
                {
                    for (int i = 0; i < 3; i++)
                    {
                        variousSteps[i, 2] = false;
                    }
                }
                else if (pl.Y == 0)//если игрок на верхней границе
                {
                    for (int i = 0; i < 3; i++)
                    {
                        variousSteps[i, 0] = false;
                    }
                }

                List<Coords> ableSteps = new List<Coords>();
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (variousSteps[i, j])
                        {
                            Coords C = new Coords();
                            C.X = i;
                            C.Y = j;
                            ableSteps.Add(C);
                        }
                    }
                }

                List<Coords> subAbleSteps = ableSteps.ToList();

                foreach (Coords C in subAbleSteps)
                {
                    if (tableLayoutPanel.GetControlFromPosition(pl.X - 1 + C.X, pl.Y - 1 + C.Y) != null)
                        ableSteps.Remove(C);
                }

                Random r = new Random();
                int k = r.Next(ableSteps.Count);

                Coords Coords = new Coords();

                //старые координаты игрока
                Coords.X = pl.X;
                Coords.Y = pl.Y;

                //удалить текущего игрока из словаря положений
                PlayerCoords.Remove(Coords);

                pl.X = pl.X - 1 + ableSteps[k].X;
                pl.Y = pl.Y - 1 + ableSteps[k].Y;

                //новые координаты игрока
                Coords.X = pl.X;
                Coords.Y = pl.Y;

                //добавить текущего игрока с новыми координатами в словарь положений
                PlayerCoords.Add(Coords, pl);
            }
        }


        /// <summary>
        /// Занята ли клетка
        /// </summary>
        public bool IsBusy(Coords C)
        {
            bool isBusy = false;

            if (PlayerCoords.ContainsKey(C) || BarrierCoords.ContainsKey(C))
                isBusy = true;

            return isBusy;
        }


        public Coords DamagePos(Player player)
        {
            Coords C = new Coords();

            int dx = Math.Sign(player.NextPosX);
            int dy = Math.Sign(player.NextPosY);

            int count = Math.Abs(player.NextPosX);
            if (count == 0)
                count = Math.Abs(player.NextPosY);

            for (int i = 1; i < count + 1; i++)
            {
                C.X = player.X + dx * i;
                C.Y = player.Y + dy * i;
                if (IsBusy(C) || (C.X==user.X && C.Y==user.Y))//если на пути атаки встретился игрок или барьер
                {
                    break;//выходим из просчёта траектории атаки
                }
            }

            if (count != 0)
            {
                C.X -= player.X;
                C.Y -= player.Y;
            }
            return C;
        }

        //получение координат квадрата, на который хочет сходить игрок
        private void CheckCellUnderStep(UserPlayer user)
        {
            userMoveRectangles.Clear();
            userShotRectangles.Clear();
            
            Coords C = new Coords();

            if (user.NextMoveType == MoveType.Move)
            {
                bool isBusy = false;//занята ли клетка

                foreach (var player in players)
                {
                    C.X = user.X + user.NextPosX;
                    C.Y = user.Y + user.NextPosY;

                    if (IsBusy(C))//если в намеченной клетке есть игрок или препятствие
                    {
                        isBusy = true;
                        break;
                    }
                }

                if (!isBusy)//если выбранная клетка не занята
                    userMoveRectangles.Add(CellCoords(user.Y + user.NextPosY, user.X + user.NextPosX));//отметить её в траектории движения
                else//иначе никуда не идти
                {
                    user.NextPosY = user.NextPosX = 0;
                }

            }

            //если следующим ходом будем стрелять
            else
            {
                int dx = Math.Sign(user.NextPosX);
                int dy = Math.Sign(user.NextPosY);

                int count = Math.Abs(user.NextPosX);
                if (count == 0)
                    count = Math.Abs(user.NextPosY);

                for (int i = 1; i < count + 1; i++)
                {
                    userShotRectangles.Add(CellCoords(user.Y + dy * i, user.X + dx * i));
                }
            }

        }


        public void AIActions()
        {
            if(user.NextMoveType==MoveType.Move)//если пользователь ходит
                tableLayoutPanel.Controls.Add(user.Picture, user.X, user.Y);
            else//если пользователь стреляет
            {
                Coords C = new Coords();//координаты цели выстрела
                C.X = user.X + user.NextPosX;
                C.Y = user.Y + user.NextPosY;

                if (PlayerCoords.ContainsKey(C))//если есть игрок с такими координатами
                {
                    Player player = PlayerCoords[C];

                    //убиваем игрока противника
                    tableLayoutPanel.Controls.Remove(player.Picture);
                    user.Weapon = player.Weapon;
                    players.Remove(player);
                    PlayerCoords.Remove(C);

                    barriers.Add(new Barrier());//добавляем новый барьер (для могилы)
                    barriers[barriers.Count - 1].Picture.BackgroundImage = (Bitmap)picbRip.BackgroundImage.Clone();
                    barriers[barriers.Count - 1].Picture.BackgroundImageLayout = ImageLayout.Zoom;
                    tableLayoutPanel.Controls.Add(barriers[barriers.Count - 1].Picture, C.X, C.Y);
                    BarrierCoords.Add(C, barriers[barriers.Count - 1]);
                    barriers[barriers.Count - 1].X = C.X;
                    barriers[barriers.Count - 1].Y = C.Y;
                    barriers[barriers.Count - 1].Picture.MouseClick += user.MouseClick;//чтобы нажатие на могилу обрабатывалось игроком пользователя

                    if (players.Count == 0)//если противников не осталось
                    {
                        MessageBox.Show("Вы выиграли!");
                        tableLayoutPanel.Controls.Clear();
                        players.Clear();
                        PlayerCoords.Clear();
                        gameIsRunning = false;

                        return;
                    }
                }
                else if(BarrierCoords.ContainsKey(C))//если стреляем в барьер
                {
                    Barrier barrier = BarrierCoords[C];

                    tableLayoutPanel.Controls.Remove(barrier.Picture);
                    barriers.Remove(barrier);
                    BarrierCoords.Remove(C);
                }
                
            }

            userMoveRectangles.Clear();
            userShotRectangles.Clear();
            shotRadiusRectangles.Clear();

            if (players != null)//если есть игроки
                foreach (Player pl in players)//то они по порядку ходят
                {
                    Turn(pl);
                    tableLayoutPanel.Controls.Add(pl.Picture, pl.X, pl.Y);
                }

        }


        /// <summary>
        /// Функция игровых действий после хода пользователя
        /// </summary>
        public void AIActions(object sender, EventArgs e)
        {
            AIActions();
        }


        //отрисовка радиуса стрельбы, и возможных ходов
        void DrawElements(object sender, PaintEventArgs e)
        {
            if (gameIsRunning)
            {
                Graphics g = grafx.Graphics;
                g.Clear(Color.AliceBlue);

                DrawGrid(g);

                Pen p = new Pen(Color.Black, 3);
                foreach (var rectangle in shotRadiusRectangles)//отрисовка радиуса поражения
                {
                    g.DrawRectangle(p, rectangle);
                }

                p = new Pen(Color.Green, 3);
                foreach (var rectangle in userMoveRectangles)//отрисовка траектории движения
                {
                    g.DrawRectangle(p, rectangle);
                }

                p = new Pen(Color.Red, 3);
                foreach (var rectangle in userShotRectangles)//отрисовка траектории атаки
                {
                    g.DrawRectangle(p, rectangle);
                }
            }

        }


        //отрисовка поля игры
        private void DrawGrid(Graphics gr)
        {
            int cellWidth = (int)(((float)tableLayoutPanel.Width) / ((float)ColumnCount));
            int cellHeight = (int)(((float)tableLayoutPanel.Height) / ((float)RowCount));

            for (int i = 0; i < ColumnCount + 1; i++)
            {
                gr.DrawLine(Pens.RoyalBlue, i * cellWidth, 0, i * cellWidth, tableLayoutPanel.Height);
            }

            for (int i = 0; i < RowCount + 1; i++)
            {
                gr.DrawLine(Pens.RoyalBlue, 0, i * cellHeight, tableLayoutPanel.Width, i * cellHeight);
            }
        }


        /// <summary>
        /// Находит X-номер ячейки, на которой мышь в tableLayoutPanel
        /// </summary>
        /// <param name="x">x-координата мыши относительно tableLayoutPanel</param>
        int MapX2Cell(int x)
        {
            return (int)((float)x / (float)tableLayoutPanel.Width * ColumnCount);
        }

        int MapY2Cell(int y)
        {
            return (int)((float)y / (float)tableLayoutPanel.Height * RowCount);
        }

        /// <summary>
        /// Получение координат клетки
        /// </summary>
        /// <param name="row">Номер строки</param>
        /// <param name="col">Номер столбца</param>
        Rectangle CellCoords(int row, int col)
        {
            int cellWidth = (int)(((float)tableLayoutPanel.Width) / ((float)ColumnCount));
            int cellHeight = (int)(((float)tableLayoutPanel.Height) / ((float)RowCount));

            return new Rectangle(col * cellWidth, row * cellHeight, cellWidth, cellHeight);
        }


        /// <summary>
        /// Нахождение зоны поражения игрока
        /// </summary>
        /// <param name="player">Объект игрока</param>
        /// <returns></returns>
        private List<Rectangle> GetShotRadiusRectangles(Player player)
        {
            List<Rectangle> shotRadRects = new List<Rectangle>();

            shotRadiusRectangles.Add(CellCoords(player.Y, player.X));

            //восток
            for (int i = 1; i <= player.Weapon.Radius; i++)
            {
                if (player.X + i < ColumnCount)
                {
                    shotRadRects.Add(CellCoords(player.Y, player.X + i));
                    if (tableLayoutPanel.GetControlFromPosition(player.X + i, player.Y) != null)//если попался камень или противник
                        break;//дальше не стреляет
                }
            }

            //запад
            for (int i = 1; i <= player.Weapon.Radius; i++)
            {
                if (player.X - i > -1)
                {
                    shotRadRects.Add(CellCoords(player.Y, player.X - i));
                    if (tableLayoutPanel.GetControlFromPosition(player.X - i, player.Y) != null)//если попался камень или противник
                        break;//дальше не стреляет
                }
            }

            //юг
            for (int i = 1; i <= player.Weapon.Radius; i++)
            {
                if (player.Y + i < RowCount)
                {
                    shotRadRects.Add(CellCoords(player.Y + i, player.X));
                    if (tableLayoutPanel.GetControlFromPosition(player.X, player.Y + i) != null)//если попался камень или противник
                        break;//дальше не стреляет
                }
            }

            //север
            for (int i = 1; i <= player.Weapon.Radius; i++)
            {
                if (player.Y - i > -1)
                {
                    shotRadRects.Add(CellCoords(player.Y - i, player.X));
                    if (tableLayoutPanel.GetControlFromPosition(player.X, player.Y - i) != null)//если попался камень или противник
                        break;//дальше не стреляет
                }
            }

            //юго-восток
            for (int i = 1; i < player.Weapon.Radius; i++)
            {
                if (player.X + i < ColumnCount && player.Y + i < RowCount)
                {
                    shotRadRects.Add(CellCoords(player.Y + i, player.X + i));
                    if (tableLayoutPanel.GetControlFromPosition(player.X + i, player.Y + i) != null)//если попался камень или противник
                        break;//дальше не стреляет
                }
            }

            //северо-восток
            for (int i = 1; i < player.Weapon.Radius; i++)
            {
                if (player.X + i < ColumnCount && player.Y - i > -1)
                {
                    shotRadRects.Add(CellCoords(player.Y - i, player.X + i));
                    if (tableLayoutPanel.GetControlFromPosition(player.X + i, player.Y - i) != null)//если попался камень или противник
                        break;//дальше не стреляет
                }
            }

            //северо-запад
            for (int i = 1; i < player.Weapon.Radius; i++)
            {
                if (player.X - i > -1 && player.Y - i > -1)
                {
                    shotRadRects.Add(CellCoords(player.Y - i, player.X - i));
                    if (tableLayoutPanel.GetControlFromPosition(player.X - i, player.Y - i) != null)//если попался камень или противник
                        break;//дальше не стреляет
                }
            }

            //юго-запад
            for (int i = 1; i < player.Weapon.Radius; i++)
            {
                if (player.X - i > -1 && player.Y + i < RowCount)
                {
                    shotRadRects.Add(CellCoords(player.Y + i, player.X - i));
                    if (tableLayoutPanel.GetControlFromPosition(player.X - i, player.Y + i) != null)//если попался камень или противник
                        break;//дальше не стреляет
                }
            }

            return shotRadRects;
        }

        private void picbUdyr_MouseClick(object sender, MouseEventArgs e)
        {
            user.MouseClick(sender, e);
        }


        //Нахождение поля поражения каждого игрока при наведении курсора на него
        private void DrawRadiusIfPlayer(object sender, MouseEventArgs e)
        {
            if (gameIsRunning)
            {
                shotRadiusRectangles.Clear();//очистка старой зоны поражения

                int senderX = e.X;
                int senderY = e.Y;
                if (sender is PictureBox)
                {
                    senderX = ((PictureBox)sender).Left;
                    senderY = ((PictureBox)sender).Top;
                }

                Coords C = new Coords();
                C.X = MapX2Cell(senderX);
                C.Y = MapY2Cell(senderY);

                //если есть игрок с такими координатами
                if (PlayerCoords.ContainsKey(C))
                {
                    Player player = PlayerCoords[C];
                    shotRadiusRectangles=GetShotRadiusRectangles(player);
                }
                else if(user.X==C.X && user.Y == C.Y)//если пользовательский игрок имеет такие координаты
                {
                    shotRadiusRectangles = GetShotRadiusRectangles(user);
                }

                DrawElements(tableLayoutPanel, new PaintEventArgs(grafx.Graphics, tableLayoutPanel.Bounds));
                grafx.Render();
            }

        }
    }
}

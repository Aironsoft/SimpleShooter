using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;//для PictureBox

namespace SimpleShooter
{
    public delegate int MapX2Column(int x);
    public delegate int MapY2Row(int y);
    public delegate void StepToDraw(UserPlayer user);
    public delegate void EndStep();
    public delegate bool IsBusy(Coords C);

    public class UserPlayer :Player
    {
        public MapX2Column mapX;//привязано к MapX2Cell в Form
        public MapY2Row mapY;//привязано к MapY2Cell в Form
        public StepToDraw step;
        public EndStep endStep;
        public IsBusy isBusy;

        private Coords selectedStep = new Coords();

        public UserPlayer(string name, PictureBox picture, string weaponName): base(name, picture, weaponName)
        {

        }


        public void MouseClick(object sender, MouseEventArgs args)
        {
            if (args.Clicks > 0)
            {
                int senderX = args.X;
                int senderY = args.Y;

                if (sender is PictureBox)
                {
                    senderX = ((PictureBox)sender).Left;
                    senderY = ((PictureBox)sender).Top;
                }

                //если нажата ПКМ
                if (args.Button == MouseButtons.Right)
                {
                    //то двигаться
                    NextMoveType = MoveType.Move;
                    NextPosX = mapX(senderX) - X;
                    NextPosY = mapY(senderY) - Y;
                    
                    NextPosX = Math.Sign(NextPosX);
                    NextPosY = Math.Sign(NextPosY);

                    Coords C = new Coords();
                    C.X= X + NextPosX;
                    C.Y= Y + NextPosY;
                    if (isBusy(C))
                    {
                        NextPosX = 0;
                        NextPosY = 0;
                    }

                    if (selectedStep.X == NextPosX && selectedStep.Y == NextPosY)
                    {
                        selectedStep.X = 21;
                        selectedStep.Y = 21;

                        X += NextPosX;
                        Y += NextPosY;

                        endStep();
                    }
                    else
                    {
                        selectedStep.X = NextPosX;
                        selectedStep.Y = NextPosY;

                        step(this);
                    }
                }
                else if (args.Button == MouseButtons.Left)//если нажата ЛКМ
                {
                    //то атаковать
                    NextMoveType = MoveType.Shot;
                    NextPosX = mapX(senderX) - X;
                    NextPosY = mapY(senderY) - Y;

                    int len = (int)Math.Sqrt(NextPosX * NextPosX + NextPosY * NextPosY);


                    if (Math.Abs(NextPosX) == Math.Abs(NextPosY) || (NextPosX == 0 ^ NextPosY == 0))//если стреляем по диагонали или по прямой
                    {
                        if(len >= Weapon.Radius)
                        {
                            if (NextPosX != 0 && NextPosY != 0)//если атака ведётся по диагонали
                            {
                                NextPosX = Math.Sign(NextPosX) * (Weapon.Radius - 1);
                                NextPosY = Math.Sign(NextPosY) * (Weapon.Radius - 1);
                            }
                            else
                            {
                                if (NextPosY == 0)//если атака по горизонтали
                                    NextPosX = Math.Sign(NextPosX) * Weapon.Radius;
                                else
                                    NextPosY = Math.Sign(NextPosY) * Weapon.Radius;
                            }
                        }

                        Coords C = damagePos(this);
                        NextPosX = C.X;
                        NextPosY = C.Y;

                        if (selectedStep.X == NextPosX && selectedStep.Y == NextPosY)//если цель атаки подтверждена
                        {
                            selectedStep.X = 21;
                            selectedStep.Y = 21;

                            endStep();
                        }
                        else
                        {
                            selectedStep.X = NextPosX;
                            selectedStep.Y = NextPosY;

                            step(this);
                        }
                    }

                }
            }
            
        }
    }
}

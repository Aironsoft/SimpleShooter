using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;//для PictureBox

namespace SimpleShooter
{
    public enum MoveType
    {
        Move,
        Shot
    }

    public delegate Coords DamagePos(Player pl);

    public class Player
    {
        public MoveType NextMoveType { get; set; }
        public DamagePos damagePos;

        //координаты на поле
        public int X { get; set; }
        public int Y { get; set; }

        public PictureBox Picture = new PictureBox();

        public string Name { get; set; }

        public Weapon Weapon { get; set; }

        public int NextPosX { get; set; }
        public int NextPosY { get; set; }


        public Player(string name, string pictureAdress, string weaponName)
        {
            Picture.ImageLocation = pictureAdress;
            Name = name;
        }

        public Player(string name, PictureBox PB)
        {
            Picture = PB;
            Name = name;
            Weapon = Weapon.Random();
        }

        public Player(string name, PictureBox PB, string weaponName)
        {
            Picture = PB;
            Name = name;
            Weapon = Weapon.Get(weaponName);
        }
    }
}

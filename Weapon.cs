using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;//для Bitmap
using System.Threading.Tasks;
using System.Windows.Forms;//для PictureBox

namespace SimpleShooter
{
    public class Weapon
    {
        public int Radius { get; set; }
        public string Name { get; set; }

        public PictureBox Picture = new PictureBox();

        public static Dictionary<string, Weapon> weapons;

        public static Random r;

        private Weapon(int radius, string name, Bitmap b)
        {
            Radius = radius;
            Name = name;
            Picture.BackgroundImageLayout = ImageLayout.Zoom;
            Picture.BackgroundImage = b;
        }

        static Weapon()
        {
            weapons = new Dictionary<string, Weapon>();
            //weapons["WoodenSword"] = new Weapon(1, "Деревянный меч", SimpleShooter.Properties.Resources.WoodenSword);
            weapons["StoneSword"] = new Weapon(2, "Каменый меч", SimpleShooter.Properties.Resources.StoneSword);
            weapons["IronSword"] = new Weapon(3, "Железный меч", SimpleShooter.Properties.Resources.IronSword);
            weapons["GoldSword"] = new Weapon(4, "Золотой меч", SimpleShooter.Properties.Resources.GoldSword);
            weapons["DiamondSword"] = new Weapon(5, "Алмазный меч", SimpleShooter.Properties.Resources.DiamondSword);

            r = new Random();

        }


        static public Weapon Random()
        {
            return weapons.Values.ElementAt<Weapon>(r.Next(weapons.Values.Count));
        }

        public static Weapon Get(string name)
        {
            if (weapons.ContainsKey(name))
                return weapons[name];
            else return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;//для PictureBox

namespace SimpleShooter
{
    class RipStone
    {
        public PictureBox Picture = new PictureBox();

        //координаты на поле
        public int X { get; set; }
        public int Y { get; set; }

        public RipStone()
        {

        }
    }
}

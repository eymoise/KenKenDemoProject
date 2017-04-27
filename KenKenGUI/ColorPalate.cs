using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace KenKenGUI
{
    class ColorPalate
    {

        private List<Color> innerList = new List<Color>
        {
            Brushes.SkyBlue.Color,
            Brushes.Coral.Color,
            Brushes.Crimson.Color,
            Brushes.DarkSalmon.Color,
            Brushes.DarkOliveGreen.Color,
            Brushes.Indigo.Color,
            Brushes.YellowGreen.Color
        };

        private int count = 0;

        public Color NextColor()
        {
            Color retColor = innerList[count];
            count++;
            if (count >= innerList.Count)
            {
                count = 0;
            }

            return retColor;
        }
    }
}

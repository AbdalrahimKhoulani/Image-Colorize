using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multimedia_ITE_HW_1.Models
{
    class CustomPoint
    {
        public CustomPoint(Point location, Color color)
        {
            Point = location;
            Color = color;
        }

        public Point Point { get; set; }
        public Color Color { get; set; }
    }
}

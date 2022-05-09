using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace lab_3_core
{
    public class PictureModel
    {
        public int Number { get; set; }
        public Image<Bgr, byte> Image { get; set; }
        public int Y1 { get; set; }
        public int Y2 { get; set; }
        public int X1 { get; set; }
        public int X2 { get; set; }

    }
}

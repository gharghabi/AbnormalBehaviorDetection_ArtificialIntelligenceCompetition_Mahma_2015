using System;
using System.Collections.Generic;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System;

namespace TheEnd1
{
    class ContourCell
    {
        public Image<Bgr, Byte> _SegmentedContour;
        public double _AppeareFrame;
        public int _UpLeftX;
        public int _UpleftY;
        public bool _Selected;
        public bool _Appeared;
        public ContourCell next;
        public void AddContourCell(Image<Bgr, Byte> SegmentedContour, double AppeareFrame, int UpLeftX, int UpLeftY)
        {
            _SegmentedContour = SegmentedContour;
            _AppeareFrame = AppeareFrame;
            _UpLeftX = UpLeftX;
            _UpleftY = UpLeftY;
            _Selected = false;
            _Appeared = false;
            next = null;

        }
 
    }
}

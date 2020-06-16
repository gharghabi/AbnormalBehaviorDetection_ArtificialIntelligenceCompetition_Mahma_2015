using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace TheEnd1
{
   class GaborFilter
    {

        public Image<Gray, float> Convolution(Image<Gray, float> src, GaborKernel kernel, GABOR_TYPE type)
        {
            Image<Gray, float> _conv = null;
            Point _center = new Point(kernel.Width / 2 + 1, kernel.Width / 2 + 1);
            ConvolutionKernelF _kernel;

            switch (type)
            {
                case GABOR_TYPE.GABOR_REAL:
                    _kernel = new ConvolutionKernelF(kernel.EvenKernel, _center);
                    _conv = src.Convolution(_kernel);
                    break;

                case GABOR_TYPE.GABOR_IMAG:
                    _kernel = new ConvolutionKernelF(kernel.OddKernel, _center);
                    _conv = src.Convolution(_kernel);
                    break;

                case GABOR_TYPE.GABOR_MAG:
                    Image<Gray, float> _temp1, _temp2;
                    _kernel = new ConvolutionKernelF(kernel.EvenKernel, _center);
                    _temp1 = src.Convolution(_kernel);

                    _kernel = new ConvolutionKernelF(kernel.OddKernel, _center);
                    _temp2 = src.Convolution(_kernel);

                    _temp1 = _temp1.Pow(2);
                    _temp2 = _temp2.Pow(2);
                    _temp1 = _temp1.Add(_temp2);
                    _conv = _temp1.Pow(0.5);
                    break;

                case GABOR_TYPE.GABOR_PHASE:
                    break;
            }
            
            return _conv;
        }
    }
}

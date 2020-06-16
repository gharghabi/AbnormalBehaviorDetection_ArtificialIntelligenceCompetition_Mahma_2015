using System.Linq;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;

namespace TheEnd1
{
    class Abandon
    {

        private Capture _capture;
        public int DetectAbnBagNum = 0;

        Image<Bgr, Byte> temp;
        double framenumber;
        public void run(String Path)
        {
            try
            {

            _capture = new Capture(@Path);

            _capture.QueryFrame();
            Image<Bgr, Single> frame;
            Image<Bgr, Single> CheckNullframe;
            int PrvContourNum = 0;
            frame = _capture.QueryFrame().Convert<Bgr, Single>();

            ContourDetection Firstcontour = new ContourDetection();
            bool exist;
            Image<Gray, Single> FirstDFT = new Image<Gray, Single>(frame.Size);

            Image<Bgr, Single> FirstBackground = _capture.QueryFrame().Convert<Bgr, Single>();
            framenumber = _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES);
            Image<Gray, Single> FirstDFTBack = new Image<Gray, Single>(frame.Size);//ino khodam avaz kardam ERROR midad

            CvInvoke.cvDFT(frame.Convert<Gray, Single>(), FirstDFT, Emgu.CV.CvEnum.CV_DXT.CV_DXT_FORWARD, -1);
            CvInvoke.cvDFT(FirstBackground.Convert<Gray, Single>().Ptr, FirstDFTBack.Ptr, Emgu.CV.CvEnum.CV_DXT.CV_DXT_FORWARD, -1);

            Image<Gray, Single> FirstoriginalLeft = new Image<Gray, Single>(frame.Size);
            Image<Gray, Single> FirstoriginalRight = new Image<Gray, Single>(frame.Size);
            Image<Gray, Single> Firstsum = new Image<Gray, Single>(frame.Size);

            CvInvoke.cvDFT((FirstDFTBack - FirstDFT).Convert<Gray, Single>().Ptr, FirstoriginalLeft.Ptr, Emgu.CV.CvEnum.CV_DXT.CV_DXT_INVERSE, -1);

            CvInvoke.cvDFT((FirstDFT - FirstDFTBack).Ptr, FirstoriginalRight.Ptr, Emgu.CV.CvEnum.CV_DXT.CV_DXT_INVERSE, -1);
            Firstsum = FirstoriginalLeft - FirstoriginalRight;
            float r = 20.0f;
            List<ContourCell> PrvFrameContours = Firstcontour.FindContourSperatly(Firstsum.Convert<Bgr, byte>(), framenumber);
            CheckNullframe = _capture.QueryFrame().Convert<Bgr, Single>();
            List<int> xList = new List<int>();
            while (CheckNullframe != null)
            {
                //j++;
                ContourDetection contour = new ContourDetection();

                Image<Gray, Single> DFT = new Image<Gray, Single>(frame.Size);
                Image<Bgr, Single> Background = CheckNullframe.Convert<Bgr, Single>();
                framenumber = _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES);
                Image<Gray, Single> DFTBack = new Image<Gray, Single>(frame.Size);//ino khodam avaz kardam ERROR midad


                CvInvoke.cvDFT(frame.Convert<Gray, Single>(), DFT, Emgu.CV.CvEnum.CV_DXT.CV_DXT_FORWARD, -1);


                CvInvoke.cvDFT(Background.Convert<Gray, Single>().Ptr, DFTBack.Ptr, Emgu.CV.CvEnum.CV_DXT.CV_DXT_FORWARD, -1);

                Image<Gray, Single> originalLeft = new Image<Gray, Single>(frame.Size);
                Image<Gray, Single> originalRight = new Image<Gray, Single>(frame.Size);
                Image<Gray, Single> sum = new Image<Gray, Single>(frame.Size);

                CvInvoke.cvDFT((DFTBack - DFT).Convert<Gray, Single>().Ptr, originalLeft.Ptr, Emgu.CV.CvEnum.CV_DXT.CV_DXT_INVERSE, -1);

                CvInvoke.cvDFT((DFT - DFTBack).Ptr, originalRight.Ptr, Emgu.CV.CvEnum.CV_DXT.CV_DXT_INVERSE, -1);
                sum = originalLeft - originalRight;

                List<ContourCell> ThisFrameContours = contour.FindContourSperatly(sum.Convert<Bgr, byte>(), framenumber);
                // List<ContourCell> NewList = contour.DetectAbandonedBag(PrvFrameContours, ThisFrameContours, framenumber);
                PrvFrameContours = contour.DetectAbandonedBag(PrvFrameContours, ThisFrameContours, framenumber);
                PrvContourNum = PrvFrameContours.Count;
                // int n = NewList.Count;
                while (PrvContourNum != 0)
                // while (n != 0)
                {
                    if (PrvFrameContours.ElementAt(PrvContourNum - 1)._AppeareFrame <= (framenumber - 6))
                    // if (NewList.ElementAt(n - 1)._AppeareFrame <= (framenumber - 4))
                    {
                        Image<Bgr, Byte> BoundryImage = CheckNullframe.Convert<Bgr, byte>();
                        exist = false;

                        PointF center = new PointF(PrvFrameContours.ElementAt(PrvContourNum - 1)._UpLeftX + (PrvFrameContours.ElementAt(PrvContourNum - 1)._SegmentedContour.Width / 2), PrvFrameContours.ElementAt(PrvContourNum - 1)._UpleftY + (PrvFrameContours.ElementAt(PrvContourNum - 1)._SegmentedContour.Height / 2));
                
                        if (PrvFrameContours.ElementAt(PrvContourNum - 1)._Appeared == false)
                        //    if (NewList.ElementAt(n - 1)._Appeared == false)
                        {
                            //while(xlis)
                            if (xList.Count == 0)
                            {
                                xList.Add(PrvFrameContours.ElementAt(PrvContourNum - 1)._UpLeftX);
                                DetectAbnBagNum++;
                                PrvFrameContours.ElementAt(PrvContourNum - 1)._Appeared = true;
                            }
                            else
                            {
                                int m = xList.Count;
                                while (m != 0)
                                {
                                    if (PrvFrameContours.ElementAt(PrvContourNum - 1)._UpLeftX <= xList.ElementAt(m - 1) + 10 || PrvFrameContours.ElementAt(PrvContourNum - 1)._UpLeftX >= xList.ElementAt(m - 1) - 10)
                                    {
                                        exist = true;
                                    }
                                    m--;
                                }
                                if (exist == false)
                                {
                                    xList.Add(PrvFrameContours.ElementAt(PrvContourNum - 1)._UpLeftX);
                                    DetectAbnBagNum++;
                                    PrvFrameContours.ElementAt(PrvContourNum - 1)._Appeared = true;
                                }
                            }
                        }

                        CircleF circle = new CircleF(center, r);
                        BoundryImage.Draw(circle, new Bgr(0, 140, 255), 3);
                    }
                    PrvContourNum--;
                    // n--;
                }
                // PrvFrameContours = NewList;
                temp = _capture.QueryFrame();
                if (temp != null)
                {
                    CheckNullframe = temp.Convert<Bgr, Single>(); //frame badi

                }
                else
                {
                    CheckNullframe = null;
                }
                DFT.Dispose();
                Background.Dispose();
                DFTBack.Dispose();
                originalLeft.Dispose();
                originalRight.Dispose();
                Firstsum.Dispose();

            }//while

            _capture.Dispose();
       
            FirstDFT.Dispose();
            FirstBackground.Dispose();
            FirstDFTBack.Dispose();
            FirstoriginalLeft.Dispose();
            FirstoriginalRight.Dispose();
            Firstsum.Dispose();


        }//try
            catch (OutOfMemoryException excpt)
            {
            }
        }
    }
}


 //private Capture _capture;
 //       public int DetectAbnBagNum = 0;

 //       Image<Bgr, Byte> temp;
 //       double framenumber;
 //       public void run(String Path)
 //       {

 //           _capture = new Capture(@Path);

 //           _capture.QueryFrame();
 //           Image<Bgr, Single> frame;
 //           Image<Bgr, Single> CheckNullframe;
 //           int PrvContourNum = 0;
 //           frame = _capture.QueryFrame().Convert<Bgr, Single>();

 //           ContourDetection Firstcontour = new ContourDetection();
 //           bool exist;
 //           Image<Gray, Single> FirstDFT = new Image<Gray, Single>(frame.Size);

 //           Image<Bgr, Single> FirstBackground = _capture.QueryFrame().Convert<Bgr, Single>();
 //           framenumber = _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES);
 //           Image<Gray, Single> FirstDFTBack = new Image<Gray, Single>(frame.Size);//ino khodam avaz kardam ERROR midad
 //           CvInvoke.cvDFT(frame.Convert<Gray, Single>(), FirstDFT, Emgu.CV.CvEnum.CV_DXT.CV_DXT_FORWARD, -1);
 //           CvInvoke.cvDFT(FirstBackground.Convert<Gray, Single>().Ptr, FirstDFTBack.Ptr, Emgu.CV.CvEnum.CV_DXT.CV_DXT_FORWARD, -1);

 //           Image<Gray, Single> FirstoriginalLeft = new Image<Gray, Single>(frame.Size);
 //           Image<Gray, Single> FirstoriginalRight = new Image<Gray, Single>(frame.Size);
 //           Image<Gray, Single> Firstsum = new Image<Gray, Single>(frame.Size);

 //           CvInvoke.cvDFT((FirstDFTBack - FirstDFT).Convert<Gray, Single>().Ptr, FirstoriginalLeft.Ptr, Emgu.CV.CvEnum.CV_DXT.CV_DXT_INVERSE, -1);

 //           CvInvoke.cvDFT((FirstDFT - FirstDFTBack).Ptr, FirstoriginalRight.Ptr, Emgu.CV.CvEnum.CV_DXT.CV_DXT_INVERSE, -1);
 //           Firstsum = FirstoriginalLeft - FirstoriginalRight;

 //           float r = 20.0f;
 //           List<ContourCell> PrvFrameContours = Firstcontour.FindContourSperatly(Firstsum.Convert<Bgr, byte>(), framenumber);
 //           CheckNullframe = _capture.QueryFrame().Convert<Bgr, Single>();
 //           int DetectAbnBagNum = 0;
 //           List<int> xList = new List<int>();
 //           Image<Gray, Single> DFT = new Image<Gray, Single>(frame.Size);
 //           Image<Bgr, Single> Background = CheckNullframe.Convert<Bgr, Single>();
 //           Image<Gray, Single> DFTBack = new Image<Gray, Single>(frame.Size);
 //           Image<Gray, Single> originalLeft = new Image<Gray, Single>(frame.Size);
 //           Image<Gray, Single> originalRight = new Image<Gray, Single>(frame.Size);
 //           Image<Gray, Single> sum = new Image<Gray, Single>(frame.Size);
 //           Image<Bgr, Byte> BoundryImage = new Image<Bgr, Byte>(frame.Size); ;
 //           ContourDetection contour = new ContourDetection();
 //           while (CheckNullframe != null)
 //           {
                
 //               Background = CheckNullframe.Convert<Bgr, Single>();
 //               framenumber = _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_POS_FRAMES);

 //               CvInvoke.cvDFT(frame.Convert<Gray, Single>(), DFT, Emgu.CV.CvEnum.CV_DXT.CV_DXT_FORWARD, -1);
 //               CvInvoke.cvDFT(Background.Convert<Gray, Single>().Ptr, DFTBack.Ptr, Emgu.CV.CvEnum.CV_DXT.CV_DXT_FORWARD, -1);
 //               CvInvoke.cvDFT((DFTBack - DFT).Convert<Gray, Single>().Ptr, originalLeft.Ptr, Emgu.CV.CvEnum.CV_DXT.CV_DXT_INVERSE, -1);
 //               CvInvoke.cvDFT((DFT - DFTBack).Ptr, originalRight.Ptr, Emgu.CV.CvEnum.CV_DXT.CV_DXT_INVERSE, -1);
 //               sum = originalLeft - originalRight;
 //               List<ContourCell> ThisFrameContours = contour.FindContourSperatly(sum.Convert<Bgr, byte>(), framenumber);
 //               PrvFrameContours = contour.DetectAbandonedBag(PrvFrameContours, ThisFrameContours, framenumber);
 //               PrvContourNum = PrvFrameContours.Count;

 //               while (PrvContourNum != 0)
 //               {
 //                   if (PrvFrameContours.ElementAt(PrvContourNum - 1)._AppeareFrame <= (framenumber - 7))
 //                   {  //----------- Image<Bgr, Byte> BoundryImage = CheckNullframe.Convert<Bgr, byte>();
 //                       BoundryImage = CheckNullframe.Convert<Bgr, byte>();
 //                       exist = false;

 //                       PointF center = new PointF(PrvFrameContours.ElementAt(PrvContourNum - 1)._UpLeftX + (PrvFrameContours.ElementAt(PrvContourNum - 1)._SegmentedContour.Width / 2), PrvFrameContours.ElementAt(PrvContourNum - 1)._UpleftY + (PrvFrameContours.ElementAt(PrvContourNum - 1)._SegmentedContour.Height / 2));

 //                       if (PrvFrameContours.ElementAt(PrvContourNum - 1)._Appeared == false)
 //                       {

 //                           if (xList.Count == 0)
 //                           {
 //                               xList.Add(PrvFrameContours.ElementAt(PrvContourNum - 1)._UpLeftX);
 //                               DetectAbnBagNum++;
 //                               PrvFrameContours.ElementAt(PrvContourNum - 1)._Appeared = true;
 //                           }
 //                           else
 //                           {
 //                               int m = xList.Count;
 //                               while (m != 0)
 //                               {
 //                                   if (PrvFrameContours.ElementAt(PrvContourNum - 1)._UpLeftX <= xList.ElementAt(m - 1) + 10 || PrvFrameContours.ElementAt(PrvContourNum - 1)._UpLeftX >= xList.ElementAt(m - 1) - 10)
 //                                   {
 //                                       exist = true;
 //                                   }
 //                                   m--;
 //                               }
 //                               if (exist == false)
 //                               {
 //                                   xList.Add(PrvFrameContours.ElementAt(PrvContourNum - 1)._UpLeftX);
 //                                   DetectAbnBagNum++;
 //                                   PrvFrameContours.ElementAt(PrvContourNum - 1)._Appeared = true;
 //                               }
 //                           }
 //                       }

 //                       CircleF circle = new CircleF(center, r);
 //                       BoundryImage.Draw(circle, new Bgr(0, 140, 255), 3);
                       
 //                   }
 //                   PrvContourNum--;
 //               }

 //               temp = _capture.QueryFrame();
 //               if (temp != null)
 //               {
 //                   CheckNullframe = temp.Convert<Bgr, Single>(); //frame badi

 //               }
 //               else
 //               {
 //                   CheckNullframe = null;
 //               }

 //           }//While
 //       }//run

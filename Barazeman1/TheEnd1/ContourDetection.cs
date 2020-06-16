using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;

namespace TheEnd1
{
    class ContourDetection
    {
        public Image<Bgr, Byte> FindContour(Image<Bgr, Byte> image)//tasviri ke mikhahid contour ha dar an peyda shavand be onvane vorudi be aan dade mishavad
        {
            Gray cannyThreshold = new Gray(180);
            Gray cannyThresholdLinking = new Gray(120);
            Image<Gray, Byte> grayImage = image.Convert<Gray, Byte>();
            Image<Gray, Byte> cannyImage = new Image<Gray, Byte>(grayImage.Size);
            CvInvoke.cvCanny(grayImage, cannyImage, 230, 360, 3);//threshold baraye canny 

            Image<Bgr, Byte> BoundryImage = image.CopyBlank();

            StructuringElementEx kernel = new StructuringElementEx(3, 3, 1, 1, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
            CvInvoke.cvDilate(cannyImage, cannyImage, kernel, 1);

            IntPtr cont = IntPtr.Zero;

            Point[] pts;
            Point p = new Point(0, 0);

            using (MemStorage storage = new MemStorage()) //allocate storage for contour approximation
                for (Contour<Point> contours = cannyImage.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                  Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL); contours != null; contours = contours.HNext)
                {
                    pts = contours.ToArray();


                    //---------------keshidan khat dor ta dore contour---------------*/
                    BoundryImage.DrawPolyline(pts, true, new Bgr(255, 0, 255), 3);


                }

            cannyImage.Dispose();
           
            return BoundryImage;
        }



        //*----------------------- Segmentation --------------------*//
        public List<ContourCell> FindContourSperatly(Image<Bgr, Byte> image, double frameNum)
        {
            List<ContourCell> ImageSegmentedInContour = new List<ContourCell>();
            // LinkedList<ContourCell> ImageSegmentedInContour = new LinkedList<ContourCell>();


            Gray cannyThreshold = new Gray(180);
            Gray cannyThresholdLinking = new Gray(120);
            Image<Gray, Byte> grayImage = image.Convert<Gray, Byte>();
            Image<Gray, Byte> cannyImage = new Image<Gray, Byte>(grayImage.Size);
            CvInvoke.cvCanny(grayImage, cannyImage, 230, 360, 3);//10,60
            Image<Gray, Byte> cannyEdges = grayImage.Canny(180, 120);
            Image<Bgr, Byte> BoundryImage = image.CopyBlank();



            StructuringElementEx kernel = new StructuringElementEx(3, 3, 1, 1, Emgu.CV.CvEnum.CV_ELEMENT_SHAPE.CV_SHAPE_ELLIPSE);
            CvInvoke.cvDilate(cannyImage, cannyImage, kernel, 1);

            IntPtr cont = IntPtr.Zero;

            Point[] pts;
            Point p = new Point(0, 0);

            //int NumCandidate = 0;

            using (MemStorage storage = new MemStorage()) //allocate storage for contour approximation
                for (Contour<Point> contours = cannyImage.FindContours(Emgu.CV.CvEnum.CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE,
                  Emgu.CV.CvEnum.RETR_TYPE.CV_RETR_EXTERNAL); contours != null; contours = contours.HNext)
                {
                    pts = contours.ToArray();

                    //********keshidan khat dor ta dore contour******************/
                    BoundryImage.DrawPolyline(pts, true, new Bgr(255, 0, 255), 3);

                    /***************joda kardane ghesmate contoure tasvir****************/
                    if (contours.BoundingRectangle.Height > 100 && contours.BoundingRectangle.Height < 130 &&
                        contours.BoundingRectangle.Width < 90 && contours.BoundingRectangle.Width > 35)//Candidate for bag
                    {

                        ContourCell CandidateContour = new ContourCell();
                        CandidateContour.AddContourCell(image.Copy(contours.BoundingRectangle),
                                                        frameNum,
                                                        contours.BoundingRectangle.X,
                                                        contours.BoundingRectangle.Y);//in 1 contour candidate ast
                        ImageSegmentedInContour.Add(CandidateContour);
                    }



                }

            grayImage.Dispose();
            cannyImage.Dispose();
            cannyEdges.Dispose();
            BoundryImage.Dispose();
            kernel.Dispose();


            return ImageSegmentedInContour;

        }

        //---------------------- Detect Abandoned Bag------------------//
        public List<ContourCell> DetectAbandonedBag(List<ContourCell> PrvFrame, List<ContourCell> ThisFrame, double frameNum)
        {
            List<ContourCell> AbandCandidate = new List<ContourCell>();
            //LinkedList<ContourCell> AbandCandidate = new LinkedList<ContourCell>();

            int cntThis = 0;
            int cntPrv = 0;
            ContourCell CellTFrame = new ContourCell();
            ContourCell CellPFrame = new ContourCell();
            //CellTFrame = ThisFrame.First();
            cntThis = ThisFrame.Count;
            cntPrv = PrvFrame.Count;
            while (cntThis != 0)
            //(CellTFrame != null)// be ezaye tamame ajzayi ke alan hads zade kifan frame ghabli ro chek kon
            {
                //CellPFrame = PrvFrame.First();
                while (cntPrv != 0)
                // (CellPFrame != null)
                {

                    if ((ThisFrame.ElementAt(cntThis - 1)._UpLeftX >= (PrvFrame.ElementAt(cntPrv - 1)._UpLeftX - 5)) &&
                        (ThisFrame.ElementAt(cntThis - 1)._UpLeftX <= (PrvFrame.ElementAt(cntPrv - 1)._UpLeftX + 5)) &&
                        (ThisFrame.ElementAt(cntThis - 1)._UpleftY >= (PrvFrame.ElementAt(cntPrv - 1)._UpleftY - 5)) &&
                        (ThisFrame.ElementAt(cntThis - 1)._UpleftY <= (PrvFrame.ElementAt(cntPrv - 1)._UpleftY + 5)))
                    //sharte makan 2ta jesm dar 1 mahal nabashand age hastan hamun jesme va jesme dgei nis
                    {
                        if ((ThisFrame.ElementAt(cntThis - 1)._SegmentedContour.Height >= (PrvFrame.ElementAt(cntPrv - 1)._SegmentedContour.Height - 5)) &&
                            (ThisFrame.ElementAt(cntThis - 1)._SegmentedContour.Height <= (PrvFrame.ElementAt(cntPrv - 1)._SegmentedContour.Height + 5)) &&
                            (ThisFrame.ElementAt(cntThis - 1)._SegmentedContour.Width >= (PrvFrame.ElementAt(cntPrv - 1)._SegmentedContour.Width - 5)) &&
                            (ThisFrame.ElementAt(cntThis - 1)._SegmentedContour.Width <= (PrvFrame.ElementAt(cntPrv - 1)._SegmentedContour.Width + 5)))
                        //masahat ham 1i bashe shart ezafas mishe bardash
                        {
                            // if (PrvFrame.ElementAt(cntPrv - 1)._Appeared==false)
                            //{
                            ThisFrame.ElementAt(cntThis - 1)._Selected = true;
                            AbandCandidate.Add(PrvFrame.ElementAt(cntPrv - 1));
                            //   }
                        }
                    }//faghat ham 1 khune az prv bahash match shode va add shode

                    cntPrv--;
                }
                if (ThisFrame.ElementAt(cntThis - 1)._Selected == false)//yani makan sabet nis momkene taze ezafe shode bashad
                {
                    ThisFrame.ElementAt(cntThis - 1)._Selected = true;
                    AbandCandidate.Add(ThisFrame.ElementAt(cntThis - 1));
                }
                cntThis--;
            }
            cntThis = 0;
            

            return AbandCandidate;
        } 

    }
}

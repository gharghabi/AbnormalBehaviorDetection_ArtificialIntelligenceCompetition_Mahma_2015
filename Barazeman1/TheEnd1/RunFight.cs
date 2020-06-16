using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.VideoSurveillance;
using Emgu.Util;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using System.Diagnostics;
using Emgu.CV.ML;

namespace TheEnd1
{
    class RunFight
    {
        BackgroundSubtractorMOG2 SubBack;
        public String FightdetectStr=" ";
        public void run(String Path)
        {
          try 
          {

            int Imgrow = 400;//satre frame 
            int Imgcol = 500;//sotun frame
            int FrameNumForsegment = 29;//25;
            Capture _capture;
            _capture = new Capture(@Path);

            List<Particles> ParticlesArray = new List<Particles>();
            float response = -1;

            Image<Bgr, Byte> currGet = _capture.QueryFrame();
            Image<Bgr, Byte> currResized = new Image<Bgr, byte>(Imgrow, Imgcol);
            currResized = currGet.Resize(Imgrow, Imgcol, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR).Copy();
            Image<Bgr, Byte> prev = new Image<Bgr, byte>(Imgrow, Imgcol);
            prev = currResized.Copy();
            currGet = _capture.QueryFrame();
            currResized = currGet.Resize(Imgrow, Imgcol, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR).Copy();
            int FeatureCounttoAdd = 0;
            int FrameCount = 0;
            List<double> EnergySum = new List<double>();

            for (int i = 1; i < currResized.Height; i += 4)
            {
                for (int j = 1; j < currResized.Width; j += 4)
                //har 4 pixcel satri va sotuni 1 particle darim
                {

                    Particles parti = new Particles(i, j);
                    ParticlesArray.Add(parti); //be 1 listi az jense particle particle haye jadido ezafe mikonim
                }
            }
            Matrix<float> FeatureForTest = new Matrix<float>(1, FrameNumForsegment * ParticlesArray.Count * 5);//avalesh ye matrise sefr misaze
            //be ezaye har segment va be ezaye particle haye har frame va vizhegihaye har frame ke 5 tast feature darim

            SubBack = new BackgroundSubtractorMOG2(40, 10, false);// Mixture of Gaussian baraye tashkhise fard
            double EnergyMax = 0;
            int EnergyCount = 0;
            //Action play = new Action(() =>
            //{

                while (currGet != null)
                {
                   
                    if (FrameCount != FrameNumForsegment)//be ezaye har 25 frame
                    {
                        currResized = currGet.Resize(Imgrow, Imgcol, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR).Copy();

                        Matrix<double> FeatureOut = new Matrix<double>(1, ParticlesArray.Count);
                        //be ezaye tedade particlehaye 1 frame
                        FeatureOut = CalculateFeature(prev, currResized, ParticlesArray);
                        //be ezaye har frame inkaro mikone bad chon 25ta frame dar har segmente 25 ta az ina kenare ham darim dar nahayat
                        for (int i = 0; i < ParticlesArray.Count; ++i)
                        {
                            FeatureForTest[0, FeatureCounttoAdd] = (float)FeatureOut[0, i];
                            ++FeatureCounttoAdd;//chasbundan feature out ha be ezaye har frame
                        }
                        prev = currResized.Copy();//frame feliro bezar tu ghabli
                        currGet = _capture.QueryFrame();//frame feli ro begir

                        ++FrameCount;
                        /**********************energy calculate***********/
                        EnergySum.Add(calculateEnergy(ParticlesArray)); //frame be frame hesab mishe ba ham jam mishan ta 1 segment beshan
                        /**********************energy calculate***********/

                    }
                    else
                    {
                        FeatureCounttoAdd = 0;
                        FrameCount = 0;
                        ParticlesArray.Clear();
                        for (int i = 1; i < currResized.Height; i += 4)
                        {
                            for (int j = 1; j < currResized.Width; j += 4)
                            {

                                Particles parti = new Particles(i, j);
                                ParticlesArray.Add(parti);
                            }
                        }

                        FeatureForTest.SetZero();

                    }//if

                    //           ParticlesArray.Clear();
                }//While
                EnergyMax = WriteEnergyinText(EnergySum);//energy har frame ro darim

                if (EnergyMax < 7 * Math.Pow(10.0, 6.0) && EnergyMax > 2.2 * Math.Pow(10.0, 6.0))//8*10^6
                {
                    FightdetectStr = "1 8";
                }
                else
                {
                    if (EnergyMax >= 10.5 * Math.Pow(10.0, 6.0))
                    {
                        FightdetectStr = "1 1";
                    }
                }
                _capture.Dispose();
                currGet.Dispose();
                currResized.Dispose();
                prev.Dispose();
              
        }
        catch(OutOfMemoryException exp)
          {

    }
             
            //});//action

            //play.BeginInvoke(null, null);
           
        }
        public double calculateEnergy(List<Particles> ParticlesArrayfunc)//energy 1 frame
        {
            //tebghe maghale energy ro hesab mikonim
            double Energy = 0;
            List<double> w = new List<double>(ParticlesArrayfunc.Count());
            double maxAngle = 0;//??nemidunim in chande
            List<double> DeltaAngleM = new List<double>(ParticlesArrayfunc.Count());

            for (int i = 0; i < ParticlesArrayfunc.Count(); ++i)
            {
                if (ParticlesArrayfunc[i].angle > maxAngle)
                    maxAngle = ParticlesArrayfunc[i].angle;

            }



            for (int i = 0; i < ParticlesArrayfunc.Count(); ++i)
            {
                DeltaAngleM.Add(ParticlesArrayfunc[i].angle - maxAngle);
                w.Add(Math.Pow((Math.Abs(ParticlesArrayfunc[i].DeltaAngle) / Math.PI * 10), 2) + Math.Pow((Math.Abs(DeltaAngleM[i])) / Math.PI, 2) * 10);
                Energy += w[i] * Math.Pow(ParticlesArrayfunc[i].v, 2);
            }
            return Energy;
        }
        public double WriteEnergyinText(List<double> EnergySum)
        {
            /* wr = File.CreateText("Energy" + DateTime.Now.Hour + DateTime.Now.Minute + ".txt");//be ezaye particleha energyshuno minevise
                   for (int i = 0; i < EnergySum.Count; ++i)
                   {
                       int f = i + 1;
                       wr.WriteLine("x("+ f.ToString() +")" +"="+ EnergySum[i].ToString()+";");
                   }
                  
                   wr.Close();*/
            double EnergyMax = 0;
            for (int i = 0; i < EnergySum.Count; i++)
            {
                if (EnergySum.ElementAt(i) > EnergyMax)
                {
                    EnergyMax = EnergySum.ElementAt(i);
                }
            }
            return EnergyMax;
        }
        public Matrix<double> CalculateFeature(Image<Bgr, Byte> prev, Image<Bgr, Byte> curr, List<Particles> ParticlesArray)
        {

            Matrix<double> FeaturesInFraem = new Matrix<double>(1, 5 * ParticlesArray.Count);

            Image<Gray, Byte> FImg = new Image<Gray, byte>(curr.Size);
            Image<Gray, Byte> currGray = new Image<Gray, byte>(curr.Size);
            Image<Gray, Byte> prevGray = new Image<Gray, byte>(curr.Size);

            Image<Gray, float> velx = new Image<Gray, float>(curr.Size);
            Image<Gray, float> vely = new Image<Gray, float>(curr.Size);
            Image<Gray, float> vel = new Image<Gray, float>(curr.Size);
            Image<Gray, float> velxMedianOwh = new Image<Gray, float>(curr.Size);
            Image<Gray, float> velyMedianOwh = new Image<Gray, float>(curr.Size);

            int FeauturesNum = -1;
            currGray = curr.Convert<Gray, Byte>();
            prevGray = prev.Convert<Gray, Byte>();
            curr._SmoothGaussian(3);
            SubBack.Update(curr);
            FImg = SubBack.ForegroundMask; //rootasvire gaussian zadim va bad baraye joda kardane fard MOG
            //if (FImg != null)// zadane contour ro hazf kardam
            //    FImgC = Contours.FindContour(FImg.Convert<Bgr, Byte>());//??parametr haye cannio dorost kon
            //else


            OpticalFlow.HS(prevGray, currGray, true, velx, vely, 0.1d, new MCvTermCriteria(100));
            velxMedianOwh = velx.SmoothMedian(5);
            velyMedianOwh = vely.SmoothMedian(5); //bar ruye optical flow median zade

            //   pictureBox1.Image = FImg.ToBitmap();
            for (int i = 0; i < ParticlesArray.Count; ++i)
            {
                //tebghe maghale feature particle haro mohasebe konim
                #region update

                int ix = ParticlesArray[i].x;
                int iy = ParticlesArray[i].y;
                if ((FImg != null) && (FImg.Data[ix, iy, 0] == 255))//ya 0???????
                {
                    ParticlesArray[i].v = (float)(Math.Sqrt(Math.Pow(velxMedianOwh[ix, iy].Intensity, 2) + Math.Pow(velyMedianOwh[ix, iy].Intensity, 2)));
                    ParticlesArray[i].deltaX = (float)(velxMedianOwh.Data[ix, iy, 0]);
                    ParticlesArray[i].deltaY = (float)(velyMedianOwh.Data[ix, iy, 0]);
                    if ((ParticlesArray[i].x + Convert.ToInt32(velxMedianOwh.Data[ix, iy, 0]) < curr.Height) & (ParticlesArray[i].x + Convert.ToInt32(velxMedianOwh.Data[ix, iy, 0]) > 0))
                    {
                        ParticlesArray[i].x += Convert.ToInt32(velxMedianOwh.Data[ix, iy, 0]);
                    }
                    if ((ParticlesArray[i].y + Convert.ToInt32(velyMedianOwh.Data[ix, iy, 0]) < curr.Width) & (ParticlesArray[i].y + Convert.ToInt32(velyMedianOwh.Data[ix, iy, 0]) > 0))
                    {
                        ParticlesArray[i].y += Convert.ToInt32(velyMedianOwh.Data[ix, iy, 0]);
                    }
                    ParticlesArray[i].DeltaAngle = Math.Atan2(velyMedianOwh.Data[ix, iy, 0], velxMedianOwh.Data[ix, iy, 0]);
                    ParticlesArray[i].angle = Math.Atan2(Convert.ToDouble(velyMedianOwh.Data[ix, iy, 0]), Convert.ToDouble(velxMedianOwh.Data[ix, iy, 0]));
                }
                //angel tu che rangie??
                //be x, y tavajoh kon momken ast satro sotun ro eshtebah dade basham
                #endregion
                //tu araye avalie featureharye particle haro kenare ham bezare
                #region set feature vector
                FeaturesInFraem[0, ++FeauturesNum] = ParticlesArray[i].angle;
                FeaturesInFraem[0, ++FeauturesNum] = ParticlesArray[i].DeltaAngle;
                FeaturesInFraem[0, ++FeauturesNum] = ParticlesArray[i].deltaX;
                FeaturesInFraem[0, ++FeauturesNum] = ParticlesArray[i].deltaY;
                FeaturesInFraem[0, ++FeauturesNum] = ParticlesArray[i].v;
                #endregion

            }
            FImg.Dispose();
            currGray.Dispose();
            prevGray.Dispose();
            velx.Dispose();
            vely.Dispose();
            vel.Dispose();
            velxMedianOwh.Dispose();
            velyMedianOwh.Dispose();
            return FeaturesInFraem;//be ezaye har frame inkaro mikone
        }

    }
}

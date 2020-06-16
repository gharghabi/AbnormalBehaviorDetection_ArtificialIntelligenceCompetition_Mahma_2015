using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.VideoSurveillance;
using Emgu.CV.CvEnum;
using System.Diagnostics;
using Emgu.CV.ML;
using Emgu.CV.ML.Structure;

namespace TheEnd1
{
    class MaskDetection
    {


       
        static int ImgNumber = 26;
        static int Imgrow = 27;
        static int Imgcol = 18;
        public int NumMaskDetect = 0;
        public int NumKol = 0;
        public bool endframe=false;
        static string path =Application.StartupPath;// "C:/Users/shaghayegh/Documents/Visual Studio 2010/Projects/visionProject/visionProject";
        Image<Bgr, Byte> src = new Image<Bgr, Byte>(path + "/Resources/src.jpg");
        Image<Gray, float> Graysrc = new Image<Gray, float>(path + "/Resources/Graysrc.png");
        GaborKernel[,] Gabor8x5 = new GaborKernel[7, 4];
        Matrix<float> IMGDB = new Matrix<float>(2 * ImgNumber, Imgrow * Imgcol * 40);//27*18*40     
        Matrix<float> Classes = new Matrix<float>(2 * ImgNumber, 1);
        SVM model = new SVM();
        Image<Gray, float> test = new Image<Gray, float>(path + "/Resources/test.png");
        Matrix<float> sample = new Matrix<float>(1, Imgrow * Imgcol * 40);
        GaborFilter filter = new GaborFilter();//convolve tasvir
        public String DetectMaskStri=" ";
        public void run()
        {
            try
            {
                CreatGabor();
                CreateDB();
                TrainSVM();
                //Movie(VideoPath);
                int i = 0;
            }
            catch (OutOfMemoryException exp)
            {

            }

        }
        public void CreatGabor()
        {
            double sigma;
            double kmax;
           
                sigma = 2 * Math.PI;
                kmax = Math.PI / 2;
            

            for (int i = 0; i < 7; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    Gabor8x5[i, j] = new GaborKernel(i, j, sigma, kmax);
                }
            }

        }
        public void CreateDB()
        {

            int count = 0;//andazeye arayeye image 27*18*40
            for (int i = 0; i < ImgNumber; ++i)//be ezaye tamame aksae foldere face
            {
                //mituni in adreso az vorudi bekhuni*******
                String ImgName = i.ToString();
                Image<Gray, float> GraysrcDB = new Image<Gray, float>(path + "/Resources/face/" + ImgName + ".png");

                /********************convolve tasvir ba kernele gabor************************/
                count = 0;
                for (int s = 0; s < 7; ++s)
                {
                    for (int q = 0; q < 4; ++q)
                    {
                        Image<Gray, float> trans1 = filter.Convolution(GraysrcDB, Gabor8x5[s, q], GABOR_TYPE.GABOR_MAG);
                        for (int a = 0; a < trans1.Rows; ++a)
                        {
                            for (int z = 0; z < trans1.Cols; ++z)
                            {
                                IMGDB[i, count] = trans1.Data[a, z, 0];
                                ++count;
                            }
                        }
                    }
                }
                Classes[i, 0] = 0;//motealegh be kelase face hastan
            }


            /**********************occluded-face****************************************/
            for (int i = ImgNumber; i < 2 * ImgNumber; ++i)//be ezaye tamame aksaye foldere non-face
            {
                //havaset bashe DB ro az tedade aksa ha be bad shoro koni

                //mituni in adreso az vorudi bekhuni*******
                String ImgName = (i - ImgNumber).ToString();
                Image<Gray, float> GraysrcDB = new Image<Gray, float>(path + "/Resources/occluded-face/" + ImgName + ".png");

                /********************convolve tasvir ba kernele gabor************************/
                count = 0;
                for (int s = 0; s < 7; ++s)
                {
                    for (int q = 0; q < 4; ++q)
                    {
                        Image<Gray, float> trans1 = filter.Convolution(GraysrcDB, Gabor8x5[s, q], GABOR_TYPE.GABOR_MAG);

                        //GaborPictureBox.Image = trans1.ToBitmap();

                        for (int a = 0; a < trans1.Rows; ++a)
                        {
                            for (int z = 0; z < trans1.Cols; ++z)
                            {
                                IMGDB[i, count] = trans1.Data[a, z, 0];
                                ++count;
                            }
                        }
                    }
                }
                Classes[i, 0] = 1;//motealegh be kelase face hastan

            }
        }
        public void TrainSVM()
        {
            SVMParams p = new SVMParams();
            p.KernelType = Emgu.CV.ML.MlEnum.SVM_KERNEL_TYPE.LINEAR;//avaz kardam
            p.SVMType = Emgu.CV.ML.MlEnum.SVM_TYPE.C_SVC;
            p.C = 1;
            p.TermCrit = new MCvTermCriteria(100, 0.00001);
            p.Gamma = 1;
            bool trained = model.Train(IMGDB, Classes, null, null, p);
        }
        public void Movie(String Path)
        {
            try
            {
                Image<Bgr, byte> Frame;
                Capture _capture = new Capture(@Path);
                Frame = _capture.QueryFrame();

                //Action play = new Action(() =>
                //  {

                while (Frame != null)
                {
                    FaceDetectState(Frame);

                    Frame = _capture.QueryFrame();
                }
                endframe = true;
                if (Frame != null)
                    Frame.Dispose();
                _capture.Dispose();
                //});

                //play.BeginInvoke(null, null);

                //_capture = new Capture(@Path);  //set up cature//path + "/Resources/EN.avi"
                //_capture.ImageGrabbed += ProcessFramet; //set up capture event handler

                //_capture.Start(); //start aquasitio
            }
            catch (OutOfMemoryException ec)
            {
            }
        }
        
        public void FaceDetectState(Image<Bgr, Byte> In)
        {

            Image<Bgr, Byte> image; //Read the files as an 8-bit Bgr image  
            image = In.Copy();
            long detectionTime;
            List<Rectangle> faces = new List<Rectangle>();
       
           
            DetectFace.Detect(image, "haarcascade_frontalface_default.xml", faces,  out detectionTime);
            Bgr color = new Bgr(100, 2, 1);
            foreach (Rectangle face in faces)
            {
                int x = face.X;
                int y = face.Y;
                int height = face.Height;
                int width = face.Width;
                Image<Gray, float> detectfaceImage = new Image<Gray, float>(height, width);
                Image<Gray, Byte> imageGray = new Image<Gray, Byte>(image.Rows, image.Cols);
                imageGray = image.Convert<Gray, Byte>();
                for (int i = 0; i < width; ++i)
                    for (int j = 0; j < height; ++j)
                    {
                        detectfaceImage.Data[i, j, 0] = imageGray.Data[y + i, x + j, 0];
                    }

                detectfaceImage = detectfaceImage.Resize(18, 27, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);

                float response = SvmResponse(detectfaceImage);//tashkhise kelase tasvir
                ++NumKol;
                if ((response == 1)&&(DetectMaskStri==" "))//abnormal
                {
                    ++NumMaskDetect;
               //     DetectMaskStri = "1 5";
                }
                detectfaceImage.Dispose();
                imageGray.Dispose();

            }
          
        }
      public float SvmResponse(Image<Gray, float> test)
        {
            int count = 0;
            for (int s = 0; s < 7; ++s)
            {
                for (int q = 0; q < 4; ++q)
                {
                    Image<Gray, float> trans1 = filter.Convolution(test, Gabor8x5[s, q], GABOR_TYPE.GABOR_MAG);
                    // Gabor.Image = trans1.ToBitmap();
                    for (int a = 0; a < trans1.Rows; ++a)
                    {
                        for (int z = 0; z < trans1.Cols; ++z)
                        {
                            sample[0, count] = trans1.Data[a, z, 0];
                            ++count;
                        }
                    }
                    trans1.Dispose();
                }
            }

            float response = model.Predict(sample);
            return response;
        }

    }

}

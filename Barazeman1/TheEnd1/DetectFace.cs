using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.GPU;

namespace TheEnd1
{
    class DetectFace
    {
     
        public static void Detect(Image<Bgr, Byte> image, String faceFileName, List<Rectangle> faces, out long detectionTime)
        {
            Stopwatch watch;

            //if (GpuInvoke.HasCuda)
            //{
            //    using (GpuCascadeClassifier face = new GpuCascadeClassifier(faceFileName))
            //          {
            //        watch = Stopwatch.StartNew();
            //        using (GpuImage<Bgr, Byte> gpuImage = new GpuImage<Bgr, byte>(image))
            //        using (GpuImage<Gray, Byte> gpuGray = gpuImage.Convert<Gray, Byte>())
            //        {
            //            Rectangle[] faceRegion = face.DetectMultiScale(gpuGray, 1.1, 10, Size.Empty);
            //            faces.AddRange(faceRegion);
                        
            //        }
            //        watch.Stop();
            //    }
            //}
            //else
            //{
            //    //Read the HaarCascade objects
                using (CascadeClassifier face = new CascadeClassifier(faceFileName))
                
                {
                    watch = Stopwatch.StartNew();
                    using (Image<Gray, Byte> gray = image.Convert<Gray, Byte>()) //Convert it to Grayscale
                    {
                        //normalizes brightness and increases contrast of the image
                        gray._EqualizeHist();
                        
                        Rectangle[] facesDetected = face.DetectMultiScale(
                           gray,
                           1.1,
                           10,
                           new Size(20, 20),
                           Size.Empty);
                        faces.AddRange(facesDetected);

                    }
                    watch.Stop();
                }
       //     }
            detectionTime = watch.ElapsedMilliseconds;
       }
    }
}

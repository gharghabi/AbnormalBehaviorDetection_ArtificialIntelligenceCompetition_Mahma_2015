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
    public partial class Barazeman : Form
    {
        
        public Barazeman()
        {
            InitializeComponent();
        }
        //SVM dsdsd = new SVM();
        StreamWriter wr = null;
        private void OpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog Openfile = new OpenFileDialog();
            StreamReader FileReader;
      
            FileReader = new StreamReader(@"c:\har\input.txt");
            wr = File.CreateText(@"C:\har\results\result.txt");
            MaskDetection mask= new MaskDetection();
            Abandon BagAbandon;
            RunFight RunFightDetect;
            String res=" ";
            string line;
            mask.run();
          //  FileReader.ReadLine();
            int AbnormCount = 0;
            int[] ActionName=new int[9];
            while ((line = FileReader.ReadLine()) != null)
            {
               
        /*************************mask*************************/
                wr.WriteLine(line + " ");
                wr.Flush();
                mask.Movie(line);

                if (mask.NumKol - mask.NumMaskDetect < 3)
                {
                    ActionName[5] = 1;
                    wr.Write("1 5 ");
                    wr.Flush();
                    ++AbnormCount;
                }
                mask.NumKol = 0;
                mask.NumMaskDetect = 0;
            ///******************Bag**************************/
                //BagAbandon = new Abandon();
                //BagAbandon.run(line);
                //if (BagAbandon.DetectAbnBagNum > 0)
                //{
                //     ActionName[2]=1;
                //    ++AbnormCount;
                //}
               
            /********************RunFight******************/
                //if (RunFight.Checked)
                //{
                //    RunFightDetect = new RunFight();
                //    RunFightDetect.run(line);
                //    if (RunFightDetect.FightdetectStr == "1 8")
                //    {
                //        ActionName[8] = 1;
                //    }
                //    else if (RunFightDetect.FightdetectStr == "1 1")
                //    {
                //        ActionName[1] = 1;
                //    }
                //}
                int sum = 0;
                   for (int i=0;i<9;++i)
                   {
                       sum+=ActionName[i];
                   }
                        wr.Write(sum.ToString());
                        wr.Flush();
                   for (int i = 0; i < 9; ++i)
                   {
                       if (ActionName[i] == 1)
                       {
                           wr.Write(i.ToString());
                           wr.Flush();
                       }
                       ActionName[i] = 0;
                   }
                    wr.WriteLine();
                    wr.Flush();

            }
            
            
        }

        private void Barazeman_FormClosing(object sender, FormClosingEventArgs e)
        {
            wr.Close();
        }

    }
}
  
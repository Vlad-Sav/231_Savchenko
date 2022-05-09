using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.ML;
using Emgu.CV.ML.MlEnum;
using Emgu.CV.Structure;

namespace lab_3_core
{
    public partial class Form1 : Form
    {
        public const int WIDTH = 80;
        public const int HEIGHT = 200;
        List<PictureModel> TrainDataSet = null;
        List<PictureModel> TestDataSet = null;
        Matrix<float> xTrain = null;
        Matrix<int> yTrain = null;
        Matrix<float> xTest = null;
        Matrix<int> yTest = null;
        Matrix<float> xTestOne = null;
        Matrix<int> yTestOne = null;
        SVM svmModel = null;
        List<int> PredictedLabels = null;
        List<int> ActualLabels = null;
        string pathLearning;// = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\LearningData\\";
        string pathTest; //= Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\TestData\\";
        string learningSupportFile;
        string testSupportFile;
        public Form1()
        {
            InitializeComponent();
        }
        private void button9_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    pathLearning = fbd.SelectedPath;
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Text files(*.txt;*.idl;)|*.txt;*.idl;";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                learningSupportFile = dialog.FileName;

            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    pathTest = fbd.SelectedPath;
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Text files(*.txt;*.idl;)|*.txt;*.idl;";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                testSupportFile = dialog.FileName;

            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            
            
          
           
            try
            {
                TrainDataSet = new List<PictureModel>();
                List<string[]> coordinatesLearning = Helper.ReadSupportFile(learningSupportFile);
                var files = Directory.GetFiles(pathLearning,"*.png");
                files = files.OrderBy(f => Int32.Parse(f.Split("\\")[f.Split("\\").Length - 1].Split(".")[0])).ToArray();
                var i = 0;
                foreach (var file in files)
                {
                    var img = new Image<Bgr, byte>(file);
                    TrainDataSet.Add(new PictureModel { 
                        Image = img,
                        Y1 = int.Parse(coordinatesLearning[i][0]),
                        X1 = int.Parse(coordinatesLearning[i][1]),
                        Y2 = int.Parse(coordinatesLearning[i][2]),
                        X2 = int.Parse(coordinatesLearning[i][3])
                    });
                    i++;
                }
                
            }
            catch (Exception ex)
            {
                
            }
            try
            {
                TestDataSet = new List<PictureModel>();
                List<string[]> coordinatesTest = Helper.ReadSupportFile(testSupportFile);
                var files = Directory.GetFiles(pathTest, "*.png");
                files = files.OrderBy(f => Int32.Parse(f.Split("\\")[f.Split("\\").Length-1].Split(".")[0])).ToArray(); 

                var i = 0;
                foreach (var file in files)
                {
                    var img = new Image<Bgr, byte>(file);
                    TestDataSet.Add(new PictureModel
                    {
                        Image = img,
                        Y1 = int.Parse(coordinatesTest[i][0]),
                        X1 = int.Parse(coordinatesTest[i][1]),
                        Y2 = int.Parse(coordinatesTest[i][2]),
                        X2 = int.Parse(coordinatesTest[i][3])
                    });
                    i++;
                }

            }
            catch (Exception ex)
            {

            }
            lbl_data.Text = "Data is uploaded";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            if (TrainDataSet != null)
            {
                (xTrain, yTrain) = CalculateHoGFeatures(TrainDataSet);
                lbl_data.Text = "Training: Hog extracted.";
            }

        }
        static int index = 0;
        public List<Bitmap> CropBackground(PictureModel img, int step)
        {
            List<Bitmap> res = new List<Bitmap>();
            for (int i = 0; i < img.Image.ToBitmap().Width; i += step)
            {
                if (i + WIDTH < img.X1 || i > img.X2 && i + WIDTH < img.Image.ToBitmap().Width)
                {
                    var back = CropImage(img.Image.ToBitmap(), i, 0, WIDTH, HEIGHT);
                    res.Add(back);
         
                    index++;
                }
            }
            return res;
        }
        private (Matrix<float>, Matrix<int>) CalculateHoGFeatures(List<PictureModel> data)
        {
          
            HOGDescriptor hog = new HOGDescriptor(new Size(WIDTH, HEIGHT), new Size(16, 16),
                new Size(8, 8), new Size(8, 8));

            List<float[]> hogfeatures = new List<float[]>();
            List<int> labels = new List<int>();
            int i = 0;
            foreach (var d in data)
            {
                var pers = CropImage(d.Image.ToBitmap(), d.X1, d.Y1, WIDTH, HEIGHT);
           
               
                var features = hog.Compute(pers.ToImage<Bgr,Byte>());
                hogfeatures.Add(features);
                labels.Add(1);
                
                var back = CropBackground(d, 10);
                foreach(var b in back)
                {
                    
                    features = hog.Compute(b.ToImage<Bgr, Byte>());
                    hogfeatures.Add(features);
                    labels.Add(2);
                   
                }
                i++;
              
            }

            var xtrain = new Matrix<float>(Helper.To2D<float>(hogfeatures.ToArray()));
            var ytrain = new Matrix<int>(labels.ToArray());

            return (xtrain, ytrain);

        }
        private (Matrix<float>, Matrix<int>) CalculateHoGFeatures(Bitmap img, int label)
        {

            HOGDescriptor hog = new HOGDescriptor(new Size(WIDTH, HEIGHT), new Size(16, 16),
                new Size(8, 8), new Size(8, 8));

            List<float[]> hogfeatures = new List<float[]>();
            List<int> labels = new List<int>();

           
            var features = hog.Compute(img.ToImage<Bgr, Byte>());
            hogfeatures.Add(features);
            labels.Add(label) ;

              
                

            var xtrain = new Matrix<float>(Helper.To2D<float>(hogfeatures.ToArray()));
            var ytrain = new Matrix<int>(labels.ToArray());

            return (xtrain, ytrain);

        }
        public static Bitmap CropImage(Bitmap src, int x, int y, int width, int height)
        {
            Rectangle cropRect = new Rectangle(x, y, width, height);
          
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }
            return target;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
               
                lbl_data.Text = "";
                lbl_data.ForeColor = Color.Black;


                if (xTrain == null || xTrain.Rows < 1)
                {
                    throw new Exception("Load Training data features.");
                }

                svmModel = new SVM();
                if (File.Exists("model_svm"))
                {
                    svmModel.Load("model_svm");
                    lbl_data.Text = "Trained Model Loaded.";
                }
                else
                {
                    svmModel.SetKernel(SVM.SvmKernelType.Rbf);
                    svmModel.Type = SVM.SvmType.CSvc;
                    svmModel.TermCriteria = new MCvTermCriteria(1000, 0.00001);
                    svmModel.C = 250;
                    svmModel.Gamma = 0.001;
                   
                    TrainData traindata = new TrainData(xTrain, DataLayoutType.RowSample, yTrain);
                    if (svmModel.Train(traindata))
                    {
                        svmModel.Save("model_svm");
                        lbl_data.Text = "Model Trained & Saved.";
                    }
                    else
                    {
                        lbl_data.Text = "Model failed to train.";
                        lbl_data.ForeColor = Color.Red;
                    }

                }



            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Cursor = Cursors.Default;

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
                if (svmModel == null)
                {
                    throw new Exception("SVM is not trained.");
                }

                (xTest, yTest) = CalculateHoGFeatures(TestDataSet);

                if (xTest == null || xTest.Rows < 1)
                {
                    throw new Exception("Load Testing data.");
                }


                PredictedLabels = new List<int>();
                ActualLabels = new List<int>();


                for (int i = 0; i < xTest.Rows; i++)
                {
                    var prediction = svmModel.Predict(xTest.GetRow(i));
                    PredictedLabels.Add((int)prediction);
                    ActualLabels.Add(yTest[i, 0]);
                }

                var cm = Helper.ComputeConfusionMatrix(ActualLabels.ToArray(), PredictedLabels.ToArray());
                var metrics = Helper.CalculateMetrics(cm, ActualLabels.ToArray(), PredictedLabels.ToArray());
                string results = $"Test Samples = {ActualLabels.Count} \n Accuracy = {metrics[0] * 100}% " +
                    $"\nPrecision = {metrics[1] * 100}% \n Recall = {metrics[2] * 100}%";
                
                label1.Text = results;
                        
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (svmModel == null)
            {
                throw new Exception("SVM is not trained.");
            }

            Image<Bgr, byte> img = null;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files(*.jpg;*.png;)|*.jpg;*.png;";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                img = new Image<Bgr, byte>(dialog.FileName);
                    
            }
            var origImage = img.AsBitmap();
            Bitmap tempBitmap = new Bitmap(origImage.Width, origImage.Height);
            List<int> predLabels = new List<int>();
            using (Graphics g = Graphics.FromImage(tempBitmap))
            {
                g.DrawImage(origImage, 0, 0);
              
            }
            for (int i = 0; i < origImage.Width && i + WIDTH < origImage.Width; i+=5)
            {
                var crop = CropImage(origImage, i, 0, WIDTH, HEIGHT);
                (xTestOne, yTestOne) = CalculateHoGFeatures(crop, 1);
                predLabels = new List<int>();
                for (int j = 0; j < xTestOne.Rows; j++)
                {
                    var prediction = svmModel.Predict(xTestOne.GetRow(j));
                    predLabels.Add((int)prediction);
                   
                }
                if (predLabels.Contains(1))
                {
                   AddRectToImage(tempBitmap, i, 0, WIDTH, HEIGHT);
                }
                else
                {

                }
                
            }
            pictureBox1.Image = tempBitmap;



           


        }

        private Bitmap AddRectToImage(Bitmap image, int x, int y, int width, int height)
        {
            
            using (Graphics g = Graphics.FromImage(image))
            {
                g.DrawImage(image, 0, 0);
                Color customColor = Color.Green;
                Brush brush = new SolidBrush(customColor);
                g.DrawLine(new Pen(brush), new Point(x, y), new Point(x + width, y));
                g.DrawLine(new Pen(brush), new Point(x, y), new Point(x , y + height));
                g.DrawLine(new Pen(brush), new Point(x + width, y), new Point(x + width, y + height));
                g.DrawLine(new Pen(brush), new Point(x, y + height), new Point(x + width, y + height));
            }
            return image;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            File.Delete("model_svm");
        }
    }
}

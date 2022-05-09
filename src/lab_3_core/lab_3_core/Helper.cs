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
using Emgu.CV.Structure;


namespace lab_3_core
{
    public class Helper
    {
        public static T[,] To2D<T>(T[][] source)
        {
            try
            {
                int FirstDim = source.Length;
                int SecondDim = source.GroupBy(row => row.Length).Single().Key; // throws InvalidOperationException if source is not rectangular

                var result = new T[FirstDim, SecondDim];
                for (int i = 0; i < FirstDim; ++i)
                    for (int j = 0; j < SecondDim; ++j)
                        result[i, j] = source[i][j];

                return result;
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("The given jagged array is not rectangular.");
            }
        }
        public static List<string[]> ReadSupportFile(string fileName)
        {
            
            List<string[]> res = new List<string[]>();
            IEnumerable<string> lines = File.ReadLines(fileName);
            foreach (var item in lines)
            {
                var s = item.Split("\t");
                res.Add(new string[] { s[1], s[2], s[3], s[4]});
            }
            return res;
        }
        public static int[,] ComputeConfusionMatrix(int[] actual, int[] predicted)
        {
            
                if (actual.Length != predicted.Length)
                {
                    throw new Exception("Vectors lengths not matched");
                }

                int NoClasses = actual.Distinct().Count();
                int[,] CM = new int[NoClasses, NoClasses];
                for (int i = 0; i < actual.Length; i++)
                {
                    int r = predicted[i] - 1;
                    int c = actual[i] - 1;
                    CM[r, c]++;
                }
                return CM;
          
        }
        public static double[] CalculateMetrics(int[,] CM, int[] actual, int[] predicted)
        {
            try
            {
                double[] metrics = new double[3];
                int samples = actual.Length;
                int classes = (int)CM.GetLongLength(0);
                var diagonal = GetDiagonal(CM);
                var diagnolSum = diagonal.Sum();

                int[] ColTotal = GetSumCols(CM);
                int[] RowTotal = GetSumRows(CM);

                // Accuracy
                var accuracy = diagnolSum / (double)samples;

                // predicion
                var precision = new double[classes];
                for (int i = 0; i < classes; i++)
                {
                    precision[i] = diagonal[i] == 0 ? 0 : (double)diagonal[i] / ColTotal[i];
                }

                // Recall
                var recall = new double[classes];
                for (int i = 0; i < classes; i++)
                {
                    recall[i] = diagonal[i] == 0 ? 0 : (double)diagonal[i] / RowTotal[i];
                }

                metrics[0] = accuracy;
                metrics[1] = precision.Average();
                metrics[2] = recall.Average();

                return metrics;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static int[] GetDiagonal(int[,] matrix)
        {
            return Enumerable.Range(0, matrix.GetLength(0)).Select(i => matrix[i, i]).ToArray();
        }
        public static int[] GetSumCols(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int[] colSum = new int[cols];

            for (int col = 0; col < cols; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    colSum[col] += matrix[row, col];
                }
            }
            return colSum;
        }
        public static int[] GetSumRows(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int[] rowSum = new int[cols];

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    rowSum[row] += matrix[row, col];
                }
            }
            return rowSum;
        }

    }
}

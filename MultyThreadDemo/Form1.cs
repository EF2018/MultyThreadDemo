using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;


namespace MultyThreadDemo
{
    public partial class Form1 : Form
    {
        static int name;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Matrix matrix = new Matrix(Convert.ToInt32(textBox2.Text), Convert.ToInt32(textBox1.Text));
            List<long> data;
            switch (comboBox1.Text)
            {
                case "Tasks":
                    data = matrix.StartTasks();
                    break;
                case "Threads":
                    data = matrix.StartThreads();
                    break;
                case "OuterTask":
                    data = matrix.StartOuterTask();
                    break;
                case "Parallel":
                    data = matrix.StartParallel();
                    break;
                default:
                    data = matrix.Start();
                    break;
            }


            Series mySeriesOfPoint = new Series(name++.ToString());
            mySeriesOfPoint.ChartType = SeriesChartType.Line;
            mySeriesOfPoint.ChartArea = "Math functions";

            for (int i = 0; i < data.Count; i++)
            {
                mySeriesOfPoint.Points.AddXY(i, data[i]);
            }
            //Добавляем созданный набор точек в Chart
            myChart.Series.Add(mySeriesOfPoint);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < myChart.Series.Count; i++)
            {
                myChart.Series[i].Points.Clear();
            }
        }
    }
}

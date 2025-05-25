using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using delRendererFunction = Plot3D.Graph3D.delRendererFunction;
using cPoint3D = Plot3D.Graph3D.cPoint3D;
using eRaster = Plot3D.Graph3D.eRaster;
using cScatter = Plot3D.Graph3D.cScatter;
using eNormalize = Plot3D.Graph3D.eNormalize;
using eSchema = Plot3D.ColorSchema.eSchema;

namespace LAB_CHM_2023_3_1
{
    public partial class Form4 : Form
    {
        double[,] ARR;
        double[,] arr;
        double[,] arr2;
        int N;
        int M;
        bool flag;
        public Form4(double[][] u, double[][] v1, int n, int m, bool r)
        {
            InitializeComponent();

            ARR = new double[n + 1, m + 1];
            N = n;
            M = m;

            if (r == false)
            {
                Text = "Графики точного и численного решений";
                label1.Text = "Точное решение u*(x, y)";
                label2.Text = "Численное решение v(x, y)";
                flag = false;

                // Инициализация массивов
                arr = new double[n + 1, m + 1];
                arr2 = new double[n + 1, m + 1];

                // Заполнение массивов
                for (int i = 0; i <= n; i++)
                {
                    for (int j = 0; j <= m; j++)
                    {
                        arr[i, j] = u[i][j];
                        arr2[i, j] = v1[i][j];
                    }
                }

                // Построение графиков
                cPoint3D[,] i_Points3D = new cPoint3D[n + 1, m + 1];
                cPoint3D[,] i_Points3D2 = new cPoint3D[n + 1, m + 1];

                for (int X = 0; X <= n; X++)
                {
                    for (int Y = 0; Y <= m; Y++)
                    {
                        double realX = X * 1.0 / n;  // Масштабирование к [0,1]
                        double realY = Y * 1.0 / m;
                        i_Points3D[X, Y] = new cPoint3D(realX, realY, arr[X, Y]);
                        i_Points3D2[X, Y] = new cPoint3D(realX, realY, arr2[X, Y]);
                    }
                }

                // Настройка графиков
                graph3D1.AxisX_Legend = "X from 0 to 1";
                graph3D1.AxisY_Legend = "Y from 0 to 1";
                graph3D1.AxisZ_Legend = "U*";

                graph3D2.AxisX_Legend = "X from 0 to 1";
                graph3D2.AxisY_Legend = "Y from 0 to 1";
                graph3D2.AxisZ_Legend = "V";

                graph3D1.SetSurfacePoints(i_Points3D, eNormalize.Separate);
                graph3D2.SetSurfacePoints(i_Points3D2, eNormalize.Separate);

                graph3D1.Raster = (eRaster)3;
                graph3D2.Raster = (eRaster)3;

                Color[] c_Colors = Plot3D.ColorSchema.GetSchema((eSchema)0);
                graph3D1.SetColorScheme(c_Colors, 4);

                Color[] c_Colors2 = Plot3D.ColorSchema.GetSchema((eSchema)2);
                graph3D2.SetColorScheme(c_Colors2, 1);

                // Вычисление разности для случая r == false
                for (int i = 0; i <= n; i++)
                {
                    for (int j = 0; j <= m; j++)
                    {
                        ARR[i, j] = Math.Abs(arr[i, j] - arr2[i, j]);
                    }
                }
            }
            // Для основной задачи (решения с разным шагом)
            else
            {
                Text = "Графики решений с полным и половинным шагом";
                label1.Text = "Решение с полным шагом";
                label2.Text = "Решение с половинным шагом";
                flag = true;

                // Создаем массивы для данных
                double[,] arr = new double[n + 1, m + 1];
                double[,] arr2 = new double[2 * n + 1, 2 * m + 1];

                // Заполнение массивов данными
                for (int i = 0; i <= n; i++)
                {
                    for (int j = 0; j <= m; j++)
                    {
                        arr[i, j] = u[i][j];
                    }
                }

                for (int i = 0; i <= 2 * n; i++)
                {
                    for (int j = 0; j <= 2 * m; j++)
                    {
                        arr2[i, j] = v1[i][j];
                    }
                }

                // Масштабирование координат
                double xScale = 1.0 / n;
                double yScale = 1.0 / m;
                double xScaleHalf = 1.0 / (2 * n);
                double yScaleHalf = 1.0 / (2 * m);

                // Подготовка точек для графиков
                cPoint3D[,] i_Points3D = new cPoint3D[n + 1, m + 1];
                cPoint3D[,] i_Points3D2 = new cPoint3D[2 * n + 1, 2 * m + 1];

                for (int X = 0; X <= n; X++)
                {
                    for (int Y = 0; Y <= m; Y++)
                    {
                        double realX = X * xScale;
                        double realY = Y * yScale;
                        i_Points3D[X, Y] = new cPoint3D(realX, realY, arr[X, Y]);
                    }
                }

                for (int X = 0; X <= 2 * n; X++)
                {
                    for (int Y = 0; Y <= 2 * m; Y++)
                    {
                        double realX = X * xScaleHalf;
                        double realY = Y * yScaleHalf;
                        i_Points3D2[X, Y] = new cPoint3D(realX, realY, arr2[X, Y]);
                    }
                }

                // Настройка графиков
                graph3D1.AxisX_Legend = "X from 0 to 1";
                graph3D1.AxisY_Legend = "Y from 0 to 1";
                graph3D1.AxisZ_Legend = "V";

                graph3D2.AxisX_Legend = "X from 0 to 1";
                graph3D2.AxisY_Legend = "Y from 0 to 1";
                graph3D2.AxisZ_Legend = "V_2";

                // Построение графиков
                graph3D1.SetSurfacePoints(i_Points3D, eNormalize.Separate);
                graph3D2.SetSurfacePoints(i_Points3D2, eNormalize.Separate);

                graph3D1.Raster = (eRaster)3;
                graph3D2.Raster = (eRaster)3;

                Color[] c_Colors = Plot3D.ColorSchema.GetSchema((eSchema)0);
                graph3D1.SetColorScheme(c_Colors, 4);

                Color[] c_Colors2 = Plot3D.ColorSchema.GetSchema((eSchema)2);
                graph3D2.SetColorScheme(c_Colors2, 1);

                // Вычисление разности решений в общих узлах
                for (int i = 0; i <= n; i++)
                {
                    for (int j = 0; j <= m; j++)
                    {
                        ARR[i, j] = Math.Abs(arr[i, j] - arr2[i * 2, j * 2]);
                    }
                }
            }
        }

        private void SetFormula(string s_Formula)
        {
            try
            {
                delRendererFunction f_Function = Plot3D.FunctionCompiler.Compile(s_Formula);

                // IMPORTANT: Normalize maintainig the relation between X,Y,Z values otherwise the function will be distorted.
                graph3D1.SetFunction(f_Function, new PointF(-10, -10), new PointF(10, 10), 0.5, eNormalize.MaintainXYZ);
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void SetSurface(double[,] s32_Values)
        {
            cPoint3D[,] i_Points3D = new cPoint3D[s32_Values.GetLength(0), s32_Values.GetLength(1)];
            for (int X = 0; X < s32_Values.GetLength(0); X++)
            {
                for (int Y = 0; Y < s32_Values.GetLength(1); Y++)
                {
                    i_Points3D[X, Y] = new cPoint3D(X * 10, Y * 500, s32_Values[X, Y]);
                }
            }

            // IMPORTANT: Normalize X,Y,Z separately because there is an extreme mismatch
            // between X values (< 300) and Z values (> 30000)
            graph3D1.SetSurfacePoints(i_Points3D, eNormalize.Separate);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form5 form5 = new Form5(ARR, N, M, flag);
            form5.Show();
        }
    }
}

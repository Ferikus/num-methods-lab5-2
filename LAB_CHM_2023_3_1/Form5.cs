using System;
using System.Drawing;
using System.Windows.Forms;

using delRendererFunction = Plot3D.Graph3D.delRendererFunction;
using cPoint3D = Plot3D.Graph3D.cPoint3D;
using eRaster = Plot3D.Graph3D.eRaster;
using cScatter = Plot3D.Graph3D.cScatter;
using eNormalize = Plot3D.Graph3D.eNormalize;
using eSchema = Plot3D.ColorSchema.eSchema;

namespace LAB_CHM_2023_3_1
{
    public partial class Form5 : Form
    {
        public Form5(double[,] ARR, int n, int m, bool r)
        {
            InitializeComponent();

            // Установка заголовка формы в зависимости от типа разности
            Text = r ? "График разности решений с разным шагом"
                    : "График разности точного и численного решений";

            // Масштабирование координат в диапазон [0,1]
            double xScale = 1.0 / n;
            double yScale = 1.0 / m;

            // Подготовка точек для графика
            cPoint3D[,] i_Points3D = new cPoint3D[ARR.GetLength(0), ARR.GetLength(1)];
            for (int X = 0; X < ARR.GetLength(0); X++)
            {
                for (int Y = 0; Y < ARR.GetLength(1); Y++)
                {
                    // Реальные координаты в [0,1]×[0,1]
                    double realX = X * xScale;
                    double realY = Y * yScale;
                    i_Points3D[X, Y] = new cPoint3D(realX, realY, ARR[X, Y]);
                }
            }

            // Настройка осей графика
            graph3D1.AxisX_Legend = "X from 0 to 1";
            graph3D1.AxisY_Legend = "Y from 0 to 1";
            graph3D1.AxisZ_Legend = r ? "V - V₂ (разность решений)"
                                     : "U* - V (погрешность)";

            // Построение графика с раздельной нормализацией осей
            graph3D1.SetSurfacePoints(i_Points3D, eNormalize.Separate);

            // Настройка отображения сетки и цветовой схемы
            graph3D1.Raster = (eRaster)3; // Используем доступный тип сетки

            // Более контрастная цветовая схема для разностей
            Color[] colors = Plot3D.ColorSchema.GetSchema((eSchema)12); // Используем доступную схему
            graph3D1.SetColorScheme(colors, 32); // Увеличиваем количество цветовых уровней
        }
    }
}
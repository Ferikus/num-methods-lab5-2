using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;


namespace LAB_CHM_2023_3_1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void тестоваяЗадачаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3();
            form3.Show();
        }

        private void основнаяЗадачаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
        }

        // =========  FUNCTIONS =========

        double u1(double x, double y) // U* Решение тестовой задачи
        {
            return Math.Exp(Math.Pow(Math.Sin(Math.PI * x * y), 2));
        }
        double f1(double x, double y) // Функция полученная через Лапласса
        {
            //return -0.5 * Math.PI * Math.PI * (x * x + y * y) * Math.Exp(Math.Pow(Math.Sin(Math.PI * x * y), 2))
            //    * (-4 * Math.Cos(2 * Math.PI * x * y) + Math.Cos(4 * Math.PI * x * y) - 1);
            return -Math.PI * Math.PI * (x * x + y * y) * (Math.Pow(Math.Sin(2 * Math.PI * x * y), 2) + 2
                * Math.Cos(2 * Math.PI * x * y)) * Math.Exp(Math.Pow(Math.Sin(Math.PI * x * y), 2));
        }

        double f2(double x, double y) // F*
        {
            return Math.Pow(Math.Sin(Math.PI * x * y), 2);
        }

        // ========= BOUNDARY CONDITIONS =========

        double mu1(double y)
        {
            return Math.Sin(Math.PI * y);
        }

        double mu2(double y)
        {
            return Math.Sin(Math.PI * y);
        }

        double mu3(double x)
        {
            return x - x * x;
        }

        double mu4(double x)
        {
            return x - x * x;
        }

        double[][] v1;
        double[][] u;
        double[][] v2;
        double[][] v2_2;

        private void button1_Click(object sender, EventArgs e)
        {
            int n = Convert.ToInt32(textBoxNTest.Text);                // Количество участков по x
            int m = Convert.ToInt32(textBoxMTest.Text);                // Количество участков по y
            int N_max = Convert.ToInt32(textBoxMaxIterTest.Text);      // Максимальное число итераций
            double Eps = Convert.ToDouble(textBoxPrecisionTest.Text);  // Критерий останова по невязке

            double a = 0.0; // Левая граница по x
            double b = 1.0; // Правая граница по x
            double c = 0.0; // Левая граница по y
            double d = 1.0; // Правая граница по y

            double h = (b - a) / (double)n, k = (d - c) / (double)m;   // Шаги по x и y

            // Коэффициенты разностной схемы
            double h2 = 1.0 / (h * h), k2 = 1.0 / (k * k);
            double A = -2 * (h2 + k2);

            double[][] rhs;   // Вектор правой части
            double[] x, y;  // Границы по х и по y
            char[] buffer = new char [100];
            double errorMax = 0.0;
            double error;

            double[][] r;        // Вектор невязки
            double initR = 0.0;  // Начальная невязка
            double currR = 0.0;   // Текущая невязка

            x = new double[n + 1];
            y = new double[m + 1];
            v1 = new double[n + 1][];
            rhs = new double[n + 1][];
            u = new double[n + 1][];
            r = new double[n + 1][];

            for (int i = 0; i <= n; i++)
            {
                v1[i] = new double[m + 1];
                rhs[i] = new double[m + 1];
                u[i] = new double[m + 1];
                r[i] = new double[m + 1];
            }

            // =========  FILLING THE GRID =========

            // Заполнение массива x
            for (int i = 0; i <= n; i++)
            {
                x[i] = a + i * h;
            }

            // Заполнение массива y
            for (int j = 0; j <= m; j++)
            {
                y[j] = c + j * k;
            }

            // Заполнение массивов f и u
            for (int j = 0; j <= m; j++)
            {
                for (int i = 0; i <= n; i++)
                {
                    rhs[i][j] = -f1(x[i], y[j]);
                    u[i][j] = u1(x[i], y[j]);
                    r[i][j] = 0;
                }
            }

            // ========= FILLING BOUNDARY CONDITIONS IN ARRAY v1 =========

            for (int j = 0; j <= m; j++)
            {
                v1[0][j] = u1(a, y[j]);
                v1[n][j] = u1(b, y[j]);
            }

            for (int i = 0; i <= n; i++)
            {
                v1[i][0] = u1(x[i], c);
                v1[i][m] = u1(x[i], d);
            }

            // ========= INITIAL GUESS =========

            for (int j = 1; j < m; j++)
            {
                for (int i = 1; i < n; i++)
                {
                    v1[i][j] = 0.0;
                }
            }

            // =========  CONJUGATE GRADIENT METHOD (исправленная версия) =========

            double[][] v_old = new double[n + 1][]; // Предыдущее приближение
            for (int i = 0; i <= n; i++)
            {
                v_old[i] = new double[m + 1];
                Array.Copy(v1[i], v_old[i], m + 1); // Копируем начальное приближение
            }

            double[][] p = new double[n + 1][];
            double[][] Ap = new double[n + 1][];
            double rr_old = 0.0;

            // Инициализация массивов
            for (int i = 0; i <= n; i++)
            {
                p[i] = new double[m + 1];
                Ap[i] = new double[m + 1];
            }

            // Начальная невязка r = A*v1 - f (евклидова норма)
            double rr_sum = 0.0;
            for (int j = 1; j < m; j++)
            {
                for (int i = 1; i < n; i++)
                {
                    r[i][j] = A * v1[i][j] + h2 * (v1[i - 1][j] + v1[i + 1][j]) + k2 * (v1[i][j - 1] + v1[i][j + 1]) - rhs[i][j];
                    p[i][j] = r[i][j];
                    rr_sum += r[i][j] * r[i][j];
                }
            }
            initR = Math.Sqrt(rr_sum);
            currR = initR;
            rr_old = rr_sum;

            double pAp = 0.0;
            double alpha, beta, rr_new;
            int iter_num = 0;
            double maxDiff = double.MaxValue; // Максимальная разница между итерациями

            while (iter_num < N_max && maxDiff > Eps) // Критерий останова по разности итераций
            {
                pAp = 0.0;
                for (int j = 1; j < m; j++)
                {
                    for (int i = 1; i < n; i++)
                    {
                        // Сохраняем текущее приближение перед обновлением
                        v_old[i][j] = v1[i][j];
                        // Вычисляем pAp = p * Ap = p * (A * p)
                        Ap[i][j] = A * p[i][j] + h2 * (p[i - 1][j] + p[i + 1][j]) + k2 * (p[i][j - 1] + p[i][j + 1]);
                        pAp += p[i][j] * Ap[i][j];
                    }
                }
                alpha = -rr_old / pAp;

                // Обновление решения и невязки
                for (int j = 1; j < m; j++)
                {
                    for (int i = 1; i < n; i++)
                    {
                        v1[i][j] += alpha * p[i][j]; // Обновляем v1
                        r[i][j] += alpha * Ap[i][j]; // Обновляем невязку
                    }
                }

                // Новая норма невязки (евклидова)
                rr_new = 0.0;
                for (int j = 1; j < m; j++)
                {
                    for (int i = 1; i < n; i++)
                    {
                        rr_new += r[i][j] * r[i][j];
                    }
                }
                currR = Math.Sqrt(rr_new); // Евклидова норма текущей невязки

                // Вычисляем максимальную разницу между текущей и предыдущей итерациями
                maxDiff = 0.0;
                for (int j = 1; j < m; j++)
                {
                    for (int i = 1; i < n; i++)
                    {
                        double diff = Math.Abs(v1[i][j] - v_old[i][j]);
                        if (diff > maxDiff)
                        {
                            maxDiff = diff;
                        }
                    }
                }

                beta = rr_new / rr_old;
                rr_old = rr_new;

                // Обновление направления
                for (int j = 1; j < m; j++)
                {
                    for (int i = 1; i < n; i++)
                    {
                        p[i][j] = r[i][j] + beta * p[i][j];
                    }
                }

                iter_num++;
            }

            // =========  FILLING THE TABLE =========

            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add("C1", "");
            dataGridView1.Columns[0].Width = 50;
            dataGridView1.Columns[0].Frozen = true;
            dataGridView1.Columns.Add("C2", "i");
            dataGridView1.Columns[1].Width = 50;
            dataGridView1.Columns[1].Frozen = true;

            dataGridView2.Rows.Clear();
            dataGridView2.Columns.Clear();
            dataGridView2.Columns.Add("C2", "");
            dataGridView2.Columns[0].Width = 50;
            dataGridView2.Columns[0].Frozen = true;
            dataGridView2.Columns.Add("C3", "i");
            dataGridView2.Columns[1].Width = 50;
            dataGridView2.Columns[1].Frozen = true;

            dataGridView3.Rows.Clear();
            dataGridView3.Columns.Clear();
            dataGridView3.Columns.Add("C4", "");
            dataGridView3.Columns[0].Width = 50;
            dataGridView3.Columns[0].Frozen = true;
            dataGridView3.Columns.Add("C5", "i");
            dataGridView3.Columns[1].Width = 50;
            dataGridView3.Columns[1].Frozen = true;

            // Создание столбцов для таблиц
            for (int i = 0; i <= n; i++)
            {
                dataGridView1.Columns.Add(Convert.ToString(buffer), Convert.ToString(buffer));
                dataGridView2.Columns.Add(Convert.ToString(buffer), Convert.ToString(buffer));
                dataGridView3.Columns.Add(Convert.ToString(buffer), Convert.ToString(buffer));
            }

            // Создание второй строки
            dataGridView1.Rows.Add("j", "Y\\X");
            dataGridView2.Rows.Add("j", "Y\\X");
            dataGridView3.Rows.Add("j", "Y\\X");

            // Заполнение второй строки
            for (int i = 0; i <= n; i++)
            {
                dataGridView1.Columns[i + 2].HeaderText = i.ToString();
                dataGridView2.Columns[i + 2].HeaderText = i.ToString();
                dataGridView3.Columns[i + 2].HeaderText = i.ToString();

                dataGridView1.Rows[0].Cells[i + 2].Value = x[i];
                dataGridView2.Rows[0].Cells[i + 2].Value = x[i];
                dataGridView3.Rows[0].Cells[i + 2].Value = x[i];

            }
            // Заполнение первых двух столбцов
            for (int j = 0; j <= m; j++)
            {
                dataGridView1.Rows.Add();
                dataGridView2.Rows.Add();
                dataGridView3.Rows.Add();

                for (int i = 0; i <= 1; i++)
                {
                    dataGridView1.Rows[j + 1].Cells[0].Value = j;
                    dataGridView1.Rows[j + 1].Cells[1].Value = y[j];
                    dataGridView2.Rows[j + 1].Cells[0].Value = j;
                    dataGridView2.Rows[j + 1].Cells[1].Value = y[j];
                    dataGridView3.Rows[j + 1].Cells[0].Value = j;
                    dataGridView3.Rows[j + 1].Cells[1].Value = y[j];
                }
            }
            double xMax = 0.0;
            double yMax = 0.0;

            //// Заполнение таблиц значениями
            for (int j = 0; j <= m; j++)
            {
                for (int i = 0; i <= n; i++)
                {
                    error = Math.Abs(u[i][j] - v1[i][j]);

                    dataGridView1.Rows[j + 1].Cells[i + 2].Value = u[i][j].ToString("0.000000000");
                    dataGridView2.Rows[j + 1].Cells[i + 2].Value = v1[i][j].ToString("0.000000000");
                    dataGridView3.Rows[j + 1].Cells[i + 2].Value = error.ToString("0.000000000");

                    if (error > errorMax)
                    {
                        errorMax = error;
                        xMax = x[i];
                        yMax = y[j];
                    }
                }

            }

            // =========  FILLING THE REFERENCE =========

            textBoxIterNumTest.Text = iter_num.ToString();
            textBoxErrorTest.Text = maxDiff.ToString("E4");
            textBoxMaxDevTest.Text = errorMax.ToString("E4"); // Погрешность решения
            textBoxResidualInitTest.Text = initR.ToString("E4");  // Начальная невязка
            textBoxResidualTest.Text = currR.ToString("E4");
            textBoxMaxDevXTest.Text = xMax.ToString("F4");
            textBoxMaxDevYTest.Text = yMax.ToString("F4");
            textBoxInitGuess.Text = "Нулевое начальное приближение";

        }

        private void button2_Click(object sender, EventArgs e)
        {
            // =========  VARIABLE INITIALIZATION =========

            double a = 0.0; // Левая граница по x
            double b = 1.0; // Правая граница по x
            double c = 0.0; // Левая граница по y
            double d = 1.0; // Правая граница по y

            // Для основной сетки:

            int n = Convert.ToInt32(textBoxNMain.Text);                // Количество участков по x
            int m = Convert.ToInt32(textBoxMMain.Text);                // Количество участков по y
            int N_max = Convert.ToInt32(textBoxMaxIterMain.Text);      // Максимальное число итераций
            double Eps = Convert.ToDouble(textBoxPrecisionMain.Text);  // Критерий останова по невязке

            double h = (b - a) / (double)n; // Шаг по x
            double k = (d - c) / (double)m; // Шаг по  y

            // Коэффициенты разностной схемы
            double h2 = 1.0 / (h * h), k2 = 1.0 / (k * k);
            double A = -2 * (h2 + k2);

            double[][] rhs = new double[n + 1][];   // Вектор правой части
            double[] x = new double[n + 1];
            double[] y = new double[m + 1];    // Границы по х и по y
            char[] buffer = new char[100];

            double errorMax = 0.0; // Максимальное отклонение |U_U2|
            double error; // Отклонение |U_U2|

            double[][] v_old = new double[n + 1][]; ;                    // Предыдущее приближение
            double[][] p = new double[n + 1][];  // Направление
            double[][] Ap = new double[n + 1][]; // A * p (матрица * вектор = вектор)
            double pAp;                          // p * Ap (вектор * вектор = число)
            double alpha, beta;                  // Вспомогательные коэффициенты МСГ
            int iter_num = 0;                    // Использованное количество итераций
            double maxDiff = double.MaxValue;    // Максимальная разница между итерациями

            double[][] r = new double[n + 1][]; ;  // Вектор невязки
            double rr_old = 0.0; // Невязка до обновления
            double rr_new = 0.0; // Невязка после обновления
            double initR = 0.0;  // Начальная невязка
            double currR = 0.0;  // Текущая невязка для метода

            v2 = new double[n + 1][];

            for (int i = 0; i <= n; i++)
            {
                v2[i] = new double[m + 1];
                rhs[i] = new double[m + 1];
                r[i] = new double[m + 1];
                v_old[i] = new double[m + 1];
                p[i] = new double[m + 1];
                Ap[i] = new double[m + 1];
            }

            // Для сетки с половинным шагом:

            int n2 = 2 * n;
            int m2 = 2 * m;
            double h2_half = (b - a) / n2;
            double k2_half = (d - c) / m2;

            double[][] p_half = new double[n2 + 1][];
            double[][] Ap_half = new double[n2 + 1][];
            double[][] v_old_half = new double[n2 + 1][]; // Для хранения предыдущего приближения

            // Коэффициенты для сетки с половинным шагом
            double h2_h = 1.0 / (h2_half * h2_half);
            double k2_h = 1.0 / (k2_half * k2_half);
            double A_h = -2 * (h2_h + k2_h);

            double[] x_half = new double[n2 + 1];
            double[] y_half = new double[m2 + 1];
            v2_2 = new double[n2 + 1][];
            double[][] rhs_half = new double[n2 + 1][];
            double[][] r_half = new double[n2 + 1][];

            for (int i = 0; i <= n2; i++)
            {
                p_half[i] = new double[m2 + 1];
                Ap_half[i] = new double[m2 + 1];
                v_old_half[i] = new double[m2 + 1];
                v2_2[i] = new double[m2 + 1];
                rhs_half[i] = new double[m2 + 1];
                r_half[i] = new double[m2 + 1];
            }

            // ========= SOLUTION WITH NORMAL STEP =========

            // =========  FILLING THE GRID =========

            for (int i = 0; i <= n; i++) x[i] = a + i * h;
            for (int j = 0; j <= m; j++) y[j] = c + j * k;
                  
            for (int j = 0; j <= m; j++)
            {
                for (int i = 0; i <= n; i++)
                {
                    rhs[i][j] = -f2(x[i], y[j]);
                    r[i][j] = 0;
                }
            }

            // ========= FILLING BOUNDARY CONDITIONS IN ARRAY v2 =========

            for (int j = 0; j <= m; j++)
            {
                v2[0][j] = mu1(y[j]); // Граничные условия слева
                v2[n][j] = mu2(y[j]); // Граничные условия справа
            }

            for (int i = 0; i <= n; i++)
            {
                v2[i][0] = mu3(x[i]); // Граничные условия снизу
                v2[i][m] = mu4(x[i]); // Граничные условия сверху
            }

            // ========= INITIAL GUESS =========

            for (int j = 1; j < m; j++)
            {
                for (int i = 1; i < n; i++)
                {
                    v2[i][j] = 0.0; // Нулевое начальное приближение
                }
            }

            for (int i = 0; i <= n; i++)
            {
                Array.Copy(v2[i], v_old[i], m + 1); // Копируем начальное приближение
            }

            // =========  CONJUGATE GRADIENT METHOD =========

            // Начальная невязка r = A*v2 - f (евклидова норма)
            for (int j = 1; j < m; j++)
            {
                for (int i = 1; i < n; i++)
                {
                    r[i][j] = A * v2[i][j] + h2 * (v2[i - 1][j] + v2[i + 1][j]) + k2 * (v2[i][j - 1] + v2[i][j + 1]) - rhs[i][j];
                    p[i][j] = r[i][j]; // Начальное направление p0 = r0
                    rr_old += r[i][j] * r[i][j];
                }
            }
            initR = Math.Sqrt(rr_old); // Начальная невязка в евклидовой норме
            currR = initR;

            while (iter_num < N_max && maxDiff > Eps)
                // Критерий останова по максимальному числу итераций
                // и по точности метода
            {
                pAp = 0.0;
                for (int j = 1; j < m; j++)
                {
                    for (int i = 1; i < n; i++)
                    {
                        // Сохраняем текущее приближение перед обновлением
                        v_old[i][j] = v2[i][j];
                        // Вычисляем pAp = p * Ap = p * (A * p)
                        Ap[i][j] = A * p[i][j] + h2 * (p[i - 1][j] + p[i + 1][j]) + k2 * (p[i][j - 1] + p[i][j + 1]);
                        pAp += p[i][j] * Ap[i][j];
                    }
                }

                // Расчет alpha
                alpha = -rr_old / pAp;

                // Обновление решения и невязки
                for (int j = 1; j < m; j++)
                {
                    for (int i = 1; i < n; i++)
                    {
                        v2[i][j] += alpha * p[i][j]; // Обновляем v2
                        r[i][j] += alpha * Ap[i][j]; // Обновляем невязку
                    }
                }

                // Новая норма невязки (евклидова)
                rr_new = 0.0;
                for (int j = 1; j < m; j++)
                {
                    for (int i = 1; i < n; i++)
                    {
                        rr_new += r[i][j] * r[i][j];
                    }
                }
                currR = Math.Sqrt(rr_new); // Текущая невязка в евклидовой норме

                // Вычисляем максимальную разницу между текущей и предыдущей итерациями
                maxDiff = 0.0;
                for (int j = 1; j < m; j++)
                {
                    for (int i = 1; i < n; i++)
                    {
                        double diff = Math.Abs(v2[i][j] - v_old[i][j]);
                        if (diff > maxDiff)
                        {
                            maxDiff = diff;
                        }
                    }
                }

                beta = rr_new / rr_old;
                rr_old = rr_new;

                // Обновление направления
                for (int j = 1; j < m; j++)
                {
                    for (int i = 1; i < n; i++)
                    {
                        p[i][j] = r[i][j] + beta * p[i][j];
                    }
                }

                iter_num++;
            }

            // ========= SOLUTION WITH HALF STEP FOR ACCURACY CONTROL =========

            // Заполнение сетки с половинным шагом
            for (int i = 0; i <= n2; i++) x_half[i] = a + i * h2_half;
            for (int j = 0; j <= m2; j++) y_half[j] = c + j * k2_half;

            // Заполнение правой части
            for (int j = 0; j <= m2; j++)
                for (int i = 0; i <= n2; i++)
                    rhs_half[i][j] = -f2(x_half[i], y_half[j]);

            // Граничные условия
            for (int j = 0; j <= m2; j++)
            {
                v2_2[0][j] = mu1(y_half[j]);
                v2_2[n2][j] = mu2(y_half[j]);
            }
            for (int i = 0; i <= n2; i++)
            {
                v2_2[i][0] = mu3(x_half[i]);
                v2_2[i][m2] = mu4(x_half[i]);
            }

            // Начальное приближение
            for (int j = 1; j < m2; j++)
                for (int i = 1; i < n2; i++)
                    v2_2[i][j] = 0.0;

            for (int i = 0; i <= n2; i++)
            {
                Array.Copy(v2_2[i], v_old_half[i], m2 + 1);
            }

            // Начальная невязка
            double rr_old_half = 0.0;
            for (int j = 1; j < m2; j++)
            {
                for (int i = 1; i < n2; i++)
                {
                    r_half[i][j] = A_h * v2_2[i][j] + h2_h * (v2_2[i - 1][j] + v2_2[i + 1][j]) + k2_h * (v2_2[i][j - 1] + v2_2[i][j + 1]) - rhs_half[i][j];
                    p_half[i][j] = r_half[i][j];
                    rr_old_half += r_half[i][j] * r_half[i][j];
                }
            }
            double initR_half = Math.Sqrt(rr_old_half);
            double currR_half = initR_half;
            int iter_num_half = 0;
            double maxDiff_half = double.MaxValue;

            while (iter_num_half < N_max && maxDiff_half > Eps)
            {
                // Вычисление Ap_half
                for (int j = 1; j < m2; j++)
                {
                    for (int i = 1; i < n2; i++)
                    {
                        v_old_half[i][j] = v2_2[i][j];
                        Ap_half[i][j] = A_h * p_half[i][j] + h2_h * (p_half[i - 1][j] + p_half[i + 1][j]) + k2_h * (p_half[i][j - 1] + p_half[i][j + 1]);
                    }
                }

                // Расчет alpha
                double pAp_half = 0.0;
                for (int j = 1; j < m2; j++)
                {
                    for (int i = 1; i < n2; i++)
                    {
                        pAp_half += p_half[i][j] * Ap_half[i][j];
                    }
                }

                double alpha_half = -rr_old_half / pAp_half;

                // Обновление решения и невязки
                for (int j = 1; j < m2; j++)
                {
                    for (int i = 1; i < n2; i++)
                    {
                        v2_2[i][j] += alpha_half * p_half[i][j];
                        r_half[i][j] += alpha_half * Ap_half[i][j];
                    }
                }

                // Новая норма невязки
                double rr_new_half = 0.0;
                for (int j = 1; j < m2; j++)
                {
                    for (int i = 1; i < n2; i++)
                    {
                        rr_new_half += r_half[i][j] * r_half[i][j];
                    }
                }

                currR_half = Math.Sqrt(rr_new_half);

                // Вычисляем максимальную разницу между итерациями
                maxDiff_half = 0.0;
                for (int j = 1; j < m2; j++)
                {
                    for (int i = 1; i < n2; i++)
                    {
                        double diff = Math.Abs(v2_2[i][j] - v_old_half[i][j]);
                        if (diff > maxDiff_half)
                        {
                            maxDiff_half = diff;
                        }
                    }
                }

                double beta_half = rr_new_half / rr_old_half;
                rr_old_half = rr_new_half;

                // Обновление направления
                for (int j = 1; j < m2; j++)
                {
                    for (int i = 1; i < n2; i++)
                    {
                        p_half[i][j] = r_half[i][j] + beta_half * p_half[i][j];
                    }
                }

                iter_num_half++;
            }

            // ========= CALCULATE MAX DEVIATION BETWEEN SOLUTIONS =========

            errorMax = 0.0;
            double xMax = 0.0, yMax = 0.0;

            for (int j = 0; j <= m; j++)
            {
                for (int i = 0; i <= n; i++)
                {
                    error = Math.Abs(v2[i][j] - v2_2[2 * i][2 * j]);
                    if (error > errorMax)
                    {
                        errorMax = error;
                        xMax = x[i];
                        yMax = y[j];
                    }
                }
            }

            // ========= FILLING THE TABLES =========

            // Очистка и настройка таблиц
            dataGridView4.Rows.Clear();
            dataGridView4.Columns.Clear();
            dataGridView5.Rows.Clear();
            dataGridView5.Columns.Clear();
            dataGridView6.Rows.Clear();
            dataGridView6.Columns.Clear();

            dataGridView4.Columns.Add("C1", "");
            dataGridView4.Columns[0].Width = 50;
            dataGridView4.Columns[0].Frozen = true;
            dataGridView4.Columns.Add("C2", "i");
            dataGridView4.Columns[1].Width = 50;
            dataGridView4.Columns[1].Frozen = true;

            dataGridView5.Columns.Add("C1", "");
            dataGridView5.Columns[0].Width = 50;
            dataGridView5.Columns[0].Frozen = true;
            dataGridView5.Columns.Add("C2", "i");
            dataGridView5.Columns[1].Width = 50;
            dataGridView5.Columns[1].Frozen = true;

            dataGridView6.Columns.Add("C1", "");
            dataGridView6.Columns[0].Width = 50;
            dataGridView6.Columns[0].Frozen = true;
            dataGridView6.Columns.Add("C2", "i");
            dataGridView6.Columns[1].Width = 50;
            dataGridView6.Columns[1].Frozen = true;

            // Создание столбцов для таблиц
            for (int i = 0; i <= n; i++)
            {
                dataGridView4.Columns.Add(Convert.ToString(buffer), Convert.ToString(buffer));
                dataGridView5.Columns.Add(Convert.ToString(buffer), Convert.ToString(buffer));
                dataGridView6.Columns.Add(Convert.ToString(buffer), Convert.ToString(buffer));
            }

            // Создание второй строки
            dataGridView4.Rows.Add("j", "Y\\X");
            dataGridView5.Rows.Add("j", "Y\\X");
            dataGridView6.Rows.Add("j", "Y\\X");

            // Заполнение второй строки
            for (int i = 0; i <= n; i++)
            {
                dataGridView4.Columns[i + 2].HeaderText = i.ToString();
                dataGridView5.Columns[i + 2].HeaderText = i.ToString();
                dataGridView6.Columns[i + 2].HeaderText = i.ToString();

                dataGridView4.Rows[0].Cells[i + 2].Value = x[i];
                dataGridView5.Rows[0].Cells[i + 2].Value = x[i];
                dataGridView6.Rows[0].Cells[i + 2].Value = x[i];
            }

            // Заполнение первых двух столбцов
            for (int j = 0; j <= m; j++)
            {
                dataGridView4.Rows.Add();
                dataGridView5.Rows.Add();
                dataGridView6.Rows.Add();

                for (int i = 0; i <= 1; i++)
                {
                    dataGridView4.Rows[j + 1].Cells[0].Value = j;
                    dataGridView4.Rows[j + 1].Cells[1].Value = y[j];
                    dataGridView5.Rows[j + 1].Cells[0].Value = j;
                    dataGridView5.Rows[j + 1].Cells[1].Value = y[j];
                    dataGridView6.Rows[j + 1].Cells[0].Value = j;
                    dataGridView6.Rows[j + 1].Cells[1].Value = y[j];
                }
            }

            // Заполнение таблиц значениями
            for (int j = 0; j <= m; j++)
            {
                for (int i = 0; i <= n; i++)
                {
                    error = Math.Abs(v2[i][j] - v2_2[2 * i][2 * j]);

                    dataGridView4.Rows[j + 1].Cells[i + 2].Value = v2[i][j].ToString("0.000000000");
                    dataGridView5.Rows[j + 1].Cells[i + 2].Value = v2_2[2 * i][2 * j].ToString("0.000000000");
                    dataGridView6.Rows[j + 1].Cells[i + 2].Value = error.ToString("0.000000000");
                }
            }

            // ========= FILLING THE REFERENCE =========

            textBoxIterNumMain.Text = iter_num.ToString();                 // Количество итераций
            textBoxErrorMain.Text = maxDiff.ToString("E4");                // Максимальная разница между итерациями
            textBoxMaxDevMain.Text = errorMax.ToString("E4");              // Погрешность решения (разность между сетками)
            textBoxResidualInitMain.Text = initR.ToString("E4");           // Начальная невязка на основной сетке
            textBoxResidualMaxMain.Text = currR.ToString("E4");            // Текущая невязка на основной сетке
            textBoxMaxDevXMain.Text = xMax.ToString("F4");
            textBoxMaxDevYMain.Text = yMax.ToString("F4");

            // Сетка с половинным шагом
            textBoxIterNumHalfStep.Text = iter_num_half.ToString();        // Количество итераций с половинным шагом
            textBoxErrorHalfStep.Text = maxDiff_half.ToString("E4");       // Максимальная разница между итерациями
            textBoxResidualInitHalfStep.Text = initR_half.ToString("E4");  // Начальная невязка на сетке с половинным шагом
            textBoxResidualMaxHalfStep.Text = currR_half.ToString("E4");   // Текущая невязка на сетке с половинным шагом
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            int n = Convert.ToInt32(textBoxNTest.Text);
            int m = Convert.ToInt32(textBoxMTest.Text);
            Form4 form4 = new Form4(u, v1, n, m, false);
            form4.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int n = Convert.ToInt32(textBoxNMain.Text);
            int m = Convert.ToInt32(textBoxMMain.Text);
            Form4 form4 = new Form4(v2, v2_2, n, m, true);
            form4.Show();
        }
    }
}

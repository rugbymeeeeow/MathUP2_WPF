﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SimplexMethod
{
    public partial class MainWindow : Window
    {
        private int numVars;
        private int numConstraints;
        private List<TextBox> constraintInputs = new List<TextBox>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtNumVars.Text, out numVars) && int.TryParse(txtNumConstraints.Text, out numConstraints))
            {
                InputsPanel.Visibility = Visibility.Visible;
                ConstraintsInputs.Items.Clear();
                constraintInputs.Clear();
                for (int i = 0; i < numConstraints; i++)
                {
                    var constraintInput = new TextBox { Width = 300, Margin = new Thickness(0, 5, 0, 0) };
                    ConstraintsInputs.Items.Add(constraintInput);
                    constraintInputs.Add(constraintInput);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите корректные числа для переменных и ограничений.");
            }
        }

        private void AddConstraintButton_Click(object sender, RoutedEventArgs e)
        {
            var constraintInput = new TextBox { Width = 300, Margin = new Thickness(0, 5, 0, 0) };
            ConstraintsInputs.Items.Add(constraintInput);
            constraintInputs.Add(constraintInput);
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация ввода коэффициентов целевой функции
                string[] objectiveCoefficients = txtObjectiveCoefficients.Text.Split(' ');
                if (objectiveCoefficients.Length != numVars)
                {
                    MessageBox.Show("Количество коэффициентов целевой функции не соответствует числу переменных.");
                    return;
                }

                double[,] tableau = new double[numConstraints + 1, numVars + numConstraints + 1];

                // Заполнение таблицы для целевой функции
                for (int j = 0; j < numVars; j++)
                {
                    tableau[numConstraints, j] = double.Parse(objectiveCoefficients[j]);
                }

                // Заполнение таблицы для ограничений
                for (int i = 0; i < numConstraints; i++)
                {
                    string[] constraintInput = constraintInputs[i].Text.Split(' ');
                    if (constraintInput.Length != numVars + 1)
                    {
                        MessageBox.Show($"Некорректный ввод для ограничения {i + 1}. Убедитесь, что количество коэффициентов равно {numVars + 1}.");
                        return;
                    }

                    for (int j = 0; j < numVars; j++)
                    {
                        tableau[i, j] = double.Parse(constraintInput[j]);
                    }

                    tableau[i, numVars + i] = 1;
                    tableau[i, numVars + numConstraints] = double.Parse(constraintInput[numVars]);
                }

                // Нормализация целевой функции
                for (int j = 0; j < numVars; j++)
                {
                    tableau[numConstraints, j] *= -1;
                }

                string result = SimplexMethod(tableau);
                ResultsTextBlock.Text = result;
            }
            catch (FormatException)
            {
                MessageBox.Show("Некорректный ввод. Убедитесь, что все значения являются числами.");
            }
        }

        public static string SimplexMethod(double[,] tableau)
        {
            int numRows = tableau.GetLength(0);
            int numCols = tableau.GetLength(1);
            string result = "";

            while (true)
            {
                int pivotCol = -1;
                for (int j = 0; j < numCols - 1; j++)
                {
                    if (tableau[numRows - 1, j] < 0)
                    {
                        pivotCol = j;
                        break;
                    }
                }

                if (pivotCol == -1)
                    break;

                int pivotRow = -1;
                double minRatio = double.MaxValue;

                for (int i = 0; i < numRows - 1; i++)
                {
                    if (tableau[i, pivotCol] > 0)
                    {
                        double ratio = tableau[i, numCols - 1] / tableau[i, pivotCol];
                        if (ratio < minRatio)
                        {
                            minRatio = ratio;
                            pivotRow = i;
                        }
                    }
                }

                if (pivotRow == -1)
                {
                    return "Неограниченное решение.";
                }

                double pivotValue = tableau[pivotRow, pivotCol];
                for (int j = 0; j < numCols; j++)
                    tableau[pivotRow, j] /= pivotValue;

                for (int i = 0; i < numRows; i++)
                {
                    if (i != pivotRow)
                    {
                        double factor = tableau[i, pivotCol];
                        for (int j = 0; j < numCols; j++)
                            tableau[i, j] -= factor * tableau[pivotRow, j];
                    }
                }
            }

            result += "Оптимальное решение:\n";
            for (int i = 0; i < numRows - 1; i++)
            {
                result += $"x{i + 1} = {tableau[i, numCols - 1]:F2}\n";
            }
            result += $"Максимальное значение Z = {tableau[numRows - 1, numCols - 1]:F2}\n";

            result += "\nФинальная таблица симплекс-метода:\n";
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    result += $"{tableau[i, j]:F2}\t";
                }
                result += "\n";
            }

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;


namespace TransportTask
{
    public partial class MainWindow : Window
    {
        private List<List<TextBox>> costMatrixTextBoxes; // Список текстовых полей для матрицы
        private List<int> supply; // Поставки
        private List<int> demand; // Потребности

        public MainWindow()
        {
            InitializeComponent();
        }

        // Метод для создания матрицы на основе введенных данных
        private void CreateMatrixButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int rows = int.Parse(numRows.Text);
                int cols = int.Parse(numCols.Text);

                // Очистка предыдущей матрицы, если она есть
                costMatrixGrid.Children.Clear();
                costMatrixTextBoxes = new List<List<TextBox>>();

                // Заполнение таблицы TextBox для ввода данных
                for (int i = 0; i < rows; i++)
                {
                    var row = new List<TextBox>();
                    for (int j = 0; j < cols; j++)
                    {
                        var textBox = new TextBox
                        {
                            Width = 50,
                            Height = 25,
                            Margin = new Thickness(j * 60, i * 30, 0, 0),
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Text = "0"
                        };
                        costMatrixGrid.Children.Add(textBox);
                        row.Add(textBox);
                    }
                    costMatrixTextBoxes.Add(row);
                }

                // Инициализация списков для сумм поставок и потребностей
                supply = new List<int>(new int[rows]);
                demand = new List<int>(new int[cols]);

                // Отображение сумм поставок и потребностей
                supplySum.Text = string.Join(",", supply);
                demandSum.Text = string.Join(",", demand);

                // Проверка сбалансированности задачи
                BalanceTaskIfNeeded(rows, cols);
            }
            catch (Exception ex)
            {
                errorMessage.Text = "Ошибка при создании матрицы: " + ex.Message;
            }
        }

        // Метод для сбалансировки задачи
        private void BalanceTaskIfNeeded(int rows, int cols)
        {
            int supplySumValue = supply.Sum();
            int demandSumValue = demand.Sum();

            // Если сумма поставок больше суммы потребностей, добавляем столбец
            if (supplySumValue > demandSumValue)
            {
                int diff = supplySumValue - demandSumValue;
                demand.Add(diff); // Добавляем столбец с разницей в потребностях

                // Обновляем интерфейс
                demandSum.Text = string.Join(",", demand);
                AddEmptyColumn(cols, diff); // Добавляем столбец с нулями
            }
            // Если сумма потребностей больше суммы поставок, добавляем строку
            else if (demandSumValue > supplySumValue)
            {
                int diff = demandSumValue - supplySumValue;
                supply.Add(diff); // Добавляем строку с разницей в поставках

                // Обновляем интерфейс
                supplySum.Text = string.Join(",", supply);
                AddEmptyRow(rows, diff); // Добавляем строку с нулями
            }
        }

        // Метод для добавления пустой строки
        private void AddEmptyRow(int rows, int diff)
        {
            // Добавляем строку с нулями
            for (int j = 0; j < costMatrixTextBoxes[0].Count; j++)
            {
                var textBox = new TextBox
                {
                    Width = 50,
                    Height = 25,
                    Margin = new Thickness(j * 60, rows * 30, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Text = "0"  // Заполняем строку нулями
                };
                costMatrixGrid.Children.Add(textBox);
            }
        }

        // Метод для добавления пустого столбца
        private void AddEmptyColumn(int cols, int diff)
        {
            // Добавляем столбец с нулями
            for (int i = 0; i < costMatrixTextBoxes.Count; i++)
            {
                var textBox = new TextBox
                {
                    Width = 50,
                    Height = 25,
                    Margin = new Thickness(cols * 60, i * 30, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Text = "0"  // Заполняем столбец нулями
                };
                costMatrixGrid.Children.Add(textBox);
            }
        }

        // Метод для решения задачи
        private void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка сбалансированности задачи
                if (!CheckBalance(supply, demand))
                {
                    errorMessage.Text = "Задача несбалансирована!"; // Если не сбалансирована
                    return;
                }

                int rows = costMatrixTextBoxes.Count;
                int cols = costMatrixTextBoxes[0].Count;
                int[,] matrix = new int[rows, cols];

                // Считываем данные из текстовых полей
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        matrix[i, j] = int.Parse(costMatrixTextBoxes[i][j].Text);
                    }
                }

                // Считываем поставки и потребности
                for (int i = 0; i < rows; i++)
                {
                    supply[i] = int.Parse(supplySum.Text.Split(',')[i].Trim());
                }
                for (int j = 0; j < cols; j++)
                {
                    demand[j] = int.Parse(demandSum.Text.Split(',')[j].Trim());
                }

                // Решение задачи методом северо-западного угла
                var solution = SolveByNorthWestCorner(matrix, supply.ToArray(), demand.ToArray());

                // Вывод решения задачи
                string result = "Решение задачи:\n";
                int totalTransportationCost = 0; // Стоимость перевозки

                for (int i = 0; i < solution.GetLength(0); i++)
                {
                    for (int j = 0; j < solution.GetLength(1); j++)
                    {
                        result += solution[i, j] + "\t";
                        totalTransportationCost += solution[i, j] * matrix[i, j]; // Вычисление стоимости
                    }
                    result += "\n";
                }

                errorMessage.Text = result;
                totalCost.Text = totalTransportationCost.ToString(); // Отображаем стоимость
            }
            catch (Exception ex)
            {
                errorMessage.Text = "Ошибка: " + ex.Message;
            }
        }

        // Проверка на сбалансированность задачи
        private bool CheckBalance(List<int> supply, List<int> demand)
        {
            return supply.Sum() == demand.Sum(); // Задача сбалансирована, если сумма поставок = сумма потребностей
        }

        // Метод северо-западного угла для нахождения начального опорного плана
        private int[,] SolveByNorthWestCorner(int[,] costMatrix, int[] supply, int[] demand)
        {
            int m = costMatrix.GetLength(0); // количество строк (поставок)
            int n = costMatrix.GetLength(1); // количество столбцов (потребностей)
            int[,] solution = new int[m, n];

            int i = 0, j = 0;

            while (i < m && j < n)
            {
                int transportAmount = Math.Min(supply[i], demand[j]);
                solution[i, j] = transportAmount;

                supply[i] -= transportAmount;
                demand[j] -= transportAmount;

                if (supply[i] == 0)
                    i++;

                if (demand[j] == 0)
                    j++;
            }

            return solution;
        }

        private void LoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string[] lines = File.ReadAllLines(openFileDialog.FileName);

                    // Первая строка: количество поставок и потребностей
                    var sizes = lines[0].Split(',');
                    int rows = int.Parse(sizes[0]);
                    int cols = int.Parse(sizes[1]);

                    numRows.Text = rows.ToString();
                    numCols.Text = cols.ToString();

                    CreateMatrixButton_Click(null, null); // Создаём матрицу

                    // Следующие строки: матрица
                    for (int i = 0; i < rows; i++)
                    {
                        var values = lines[1 + i].Split(',');
                        for (int j = 0; j < cols; j++)
                        {
                            costMatrixTextBoxes[i][j].Text = values[j];
                        }
                    }

                    // Следующие две строки: supply и demand
                    supplySum.Text = lines[1 + rows];
                    demandSum.Text = lines[2 + rows];

                    errorMessage.Text = "Данные успешно загружены.";
                }
                catch (Exception ex)
                {
                    errorMessage.Text = "Ошибка при загрузке файла: " + ex.Message;
                }
            }
        }

        private void SaveToFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    List<string> lines = new List<string>();

                    int rows = costMatrixTextBoxes.Count;
                    int cols = costMatrixTextBoxes[0].Count;

                    // Размеры
                    lines.Add($"{rows},{cols}");

                    // Матрица
                    for (int i = 0; i < rows; i++)
                    {
                        var row = costMatrixTextBoxes[i].Select(tb => tb.Text).ToArray();
                        lines.Add(string.Join(",", row));
                    }

                    // Поставки и потребности
                    lines.Add(supplySum.Text);
                    lines.Add(demandSum.Text);

                    // Результат
                    lines.Add("Решение:");
                    lines.Add(errorMessage.Text);
                    lines.Add("Стоимость перевозки:");
                    lines.Add(totalCost.Text);

                    File.WriteAllLines(saveFileDialog.FileName, lines);
                    errorMessage.Text = "Результат успешно сохранён.";
                }
                catch (Exception ex)
                {
                    errorMessage.Text = "Ошибка при сохранении файла: " + ex.Message;
                }
            }
        }


    }
}

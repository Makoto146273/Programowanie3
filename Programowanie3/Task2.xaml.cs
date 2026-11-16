
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Programowanie3
{
    public partial class Task2 : Window
    {
        private string currentPlayer = "X";
        private bool gameOver = false;

        private Button[] buttons;

        private int[,] winningCombinations =
        {
            { 0, 1, 2 },
            { 3, 4, 5 },
            { 6, 7, 8 },
            { 0, 3, 6 },
            { 1, 4, 7 },
            { 2, 5, 8 },
            { 0, 4, 8 },
            { 2, 4, 6 }
        };

        public Task2()
        {
            InitializeComponent();

            buttons = new Button[]
            {
                B0, B1, B2,
                B3, B4, B5,
                B6, B7, B8
            };

            UpdateIndicator();
        }

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;

            if (gameOver || clickedButton.Content != null)
                return;

            clickedButton.Content = currentPlayer;

            if (currentPlayer == "X")
                clickedButton.Foreground = Brushes.Red;
            else
                clickedButton.Foreground = Brushes.Blue;

            if (CheckWinner())
            {
                gameOver = true;

                HighlightWinner();

                if (currentPlayer == "X")
                    StatusIndicator.Background =
                        new SolidColorBrush(Color.FromRgb(255, 100, 100));
                else
                    StatusIndicator.Background =
                        new SolidColorBrush(Color.FromRgb(100, 150, 255));

                return;
            }

            if (CheckDraw())
            {
                gameOver = true;
                StatusIndicator.Background = Brushes.Gray;
                return;
            }

            currentPlayer = currentPlayer == "X" ? "O" : "X";

            UpdateIndicator();
        }

        private bool CheckWinner()
        {
            for (int i = 0; i < winningCombinations.GetLength(0); i++)
            {
                int a = winningCombinations[i, 0];
                int b = winningCombinations[i, 1];
                int c = winningCombinations[i, 2];

                if (buttons[a].Content != null &&
                    buttons[a].Content == buttons[b].Content &&
                    buttons[b].Content == buttons[c].Content)
                {
                    return true;
                }
            }

            return false;
        }

        private void HighlightWinner()
        {
            for (int i = 0; i < winningCombinations.GetLength(0); i++)
            {
                int a = winningCombinations[i, 0];
                int b = winningCombinations[i, 1];
                int c = winningCombinations[i, 2];

                if (buttons[a].Content != null &&
                    buttons[a].Content == buttons[b].Content &&
                    buttons[b].Content == buttons[c].Content)
                {
                    Brush winnerColor;

                    if (currentPlayer == "X")
                        winnerColor = new SolidColorBrush(
                            Color.FromRgb(255, 100, 100));
                    else
                        winnerColor = new SolidColorBrush(
                            Color.FromRgb(100, 150, 255));

                    buttons[a].Background = winnerColor;
                    buttons[b].Background = winnerColor;
                    buttons[c].Background = winnerColor;

                    return;
                }
            }
        }

        private bool CheckDraw()
        {
            foreach (Button button in buttons)
            {
                if (button.Content == null)
                    return false;
            }

            return true;
        }

        private void UpdateIndicator()
        {
            if (currentPlayer == "X")
                StatusIndicator.Background = Brushes.Red;
            else
                StatusIndicator.Background = Brushes.Blue;
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            foreach (Button button in buttons)
            {
                button.Content = null;
                button.Foreground = Brushes.Black;
                button.Background = SystemColors.ControlBrush;
            }

            currentPlayer = "X";
            gameOver = false;

            UpdateIndicator();
        }
    }
}

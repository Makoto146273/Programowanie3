using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Programowanie3
{
    public partial class MainWindow : Window
    {
        private decimal firstNumber = 0;
        private string operation = "";
        private bool newNumber = true;

        public MainWindow()
        {
            InitializeComponent();

            Button0.Click += Number_Click;
            Button1.Click += Number_Click;
            Button2.Click += Number_Click;
            Button3.Click += Number_Click;
            Button4.Click += Number_Click;
            Button5.Click += Number_Click;
            Button6.Click += Number_Click;
            Button7.Click += Number_Click;
            Button8.Click += Number_Click;
            Button9.Click += Number_Click;

            ButtonPLUS.Click += Operation_Click;
            ButtonMIN.Click += Operation_Click;
            ButtonMUL.Click += Operation_Click;
            ButtonDIV.Click += Operation_Click;

            ButtonDOT.Click += Dot_Click;
            ButtonEQ.Click += Equals_Click;
            ButtonAC.Click += Clear_Click;
            ButtonER.Click += Backspace_Click;
            ButtonSQRT.Click += Sqrt_Click;

            TextBoxResult.Text = "0";
        }

        private decimal GetNumber()
        {
            string text = TextBoxResult.Text.Replace(',', '.');

            if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal number))
            {
                return number;
            }

            return 0;
        }

        private void ShowNumber(decimal number)
        {
            TextBoxResult.Text = number.ToString("0.########", CultureInfo.CurrentCulture);
        }

        private void TextBoxResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            FitText();
        }

        private void TextBoxResult_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            FitText();
        }

        private void FitText()
        {
            if (TextBoxResult == null || !TextBoxResult.IsLoaded)
                return;

            double maxFontSize = 32;
            double minFontSize = 6;

            double availableWidth = TextBoxResult.ActualWidth - TextBoxResult.Padding.Left - TextBoxResult.Padding.Right - 12;

            if (availableWidth <= 0)
                return;

            double fontSize = maxFontSize;

            while (fontSize > minFontSize)
            {
                FormattedText formattedText = new FormattedText(
                    TextBoxResult.Text ?? "",
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(TextBoxResult.FontFamily, TextBoxResult.FontStyle, TextBoxResult.FontWeight, TextBoxResult.FontStretch), fontSize, TextBoxResult.Foreground, VisualTreeHelper.GetDpi(TextBoxResult).PixelsPerDip);

                if (formattedText.WidthIncludingTrailingWhitespace <= availableWidth)
                    break;

                fontSize -= 1;
            }

            TextBoxResult.FontSize = fontSize;
        }

        private void Calculate()
        {
            decimal secondNumber = GetNumber();
            decimal result = 0;

            switch (operation)
            {
                case "+":
                    result = firstNumber + secondNumber;
                    break;

                case "-":
                    result = firstNumber - secondNumber;
                    break;

                case "*":
                    result = firstNumber * secondNumber;
                    break;

                case "/":
                    if (secondNumber == 0)
                    {
                        MessageBox.Show(
                            "Nie można dzielić przez zero!",
                            "Błąd",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        operation = "";
                        newNumber = true;
                        return;
                    }

                    result = firstNumber / secondNumber;
                    break;
            }

            ShowNumber(result);
            firstNumber = result;
            operation = "";
            newNumber = true;
        }

        private void Number_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string digit = button.Content.ToString();

            if (newNumber || TextBoxResult.Text == "0")
            {
                TextBoxResult.Text = digit;
                newNumber = false;
            }
            else
            {
                TextBoxResult.Text += digit;
            }
        }

        private void Dot_Click(object sender, RoutedEventArgs e)
        {
            if (newNumber)
            {
                TextBoxResult.Text = "0" +
                    CultureInfo.CurrentCulture.NumberFormat
                    .NumberDecimalSeparator;

                newNumber = false;
            }
            else
            {
                string separator =
                    CultureInfo.CurrentCulture.NumberFormat
                    .NumberDecimalSeparator;

                if (!TextBoxResult.Text.Contains(separator)
                    && !TextBoxResult.Text.Contains(".")
                    && !TextBoxResult.Text.Contains(","))
                {
                    TextBoxResult.Text += separator;
                }
            }
        }

        private void Operation_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;

            if (operation != "" && !newNumber)
            {
                Calculate();
            }

            firstNumber = GetNumber();
            operation = button.Content.ToString();
            newNumber = true;
        }

        private void Equals_Click(object sender, RoutedEventArgs e)
        {
            if (operation != "")
            {
                try
                {
                    Calculate();
                }
                catch (OverflowException)
                {
                    MessageBox.Show(
                        "Wynik jest zbyt duży lub zbyt mały!",
                        "Błąd przepełnienia",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );

                    TextBoxResult.Text = "Błąd";
                    operation = "";
                    newNumber = true;
                }
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            TextBoxResult.Text = "0";
            firstNumber = 0;
            operation = "";
            newNumber = true;
        }

        private void Backspace_Click(object sender, RoutedEventArgs e)
        {
            if (newNumber)
            {
                return;
            }

            if (TextBoxResult.Text.Length > 1)
            {
                TextBoxResult.Text =
                    TextBoxResult.Text.Substring(
                        0, TextBoxResult.Text.Length - 1);
            }
            else
            {
                TextBoxResult.Text = "0";
                newNumber = true;
            }
        }

        private void Sqrt_Click(object sender, RoutedEventArgs e)
        {
            decimal number = GetNumber();

            if (number < 0)
            {
                MessageBox.Show(
                    "Nie można obliczyć pierwiastka z liczby ujemnej!",
                    "Błąd",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            decimal result = (decimal)Math.Sqrt((double)number);

            ShowNumber(result);
            firstNumber = result;
            operation = "";
            newNumber = true;
        }
    }
}

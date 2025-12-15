using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Vakhitova41
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        private string captchaText = "";
        private bool isCaptchaShown = false; // показывалась ли капча
        private bool isBlocked = false; // заблокирована ли кнопка
        private DispatcherTimer blockTimer;
        private int secondsLeft = 0;
        private Random random = new Random();
        private bool firstFailedAttempt = true; // Флаг для первой неудачной попытки

        public AuthPage()
        {
            InitializeComponent();
        }

        // Метод для генерации капчи
        private void GenerateCaptcha()
        {
            // Очищаем Canvas
            CanvasCaptcha.Children.Clear();

            // Генерируем случайную строку из 5 символов
            string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            captchaText = "";

            for (int i = 0; i < 5; i++)
            {
                captchaText += chars[random.Next(chars.Length)];
            }

            // Рисуем капчу на Canvas
            DrawCaptcha();
        }

        private void DrawCaptcha()
        {
            // Добавляем фоновый шум
            for (int i = 0; i < 15; i++)
            {
                Ellipse noise = new Ellipse
                {
                    Width = random.Next(1, 3),
                    Height = random.Next(1, 3),
                    Fill = Brushes.DarkGray
                };
                Canvas.SetLeft(noise, random.Next(5, 205));
                Canvas.SetTop(noise, random.Next(5, 50)); 
                CanvasCaptcha.Children.Add(noise);
            }

            for (int i = 0; i < 2; i++)
            {
                Line line = new Line
                {
                    X1 = random.Next(10, 80),
                    Y1 = random.Next(10, 50), 
                    X2 = random.Next(150, 210),
                    Y2 = random.Next(10, 50), 
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 1
                };
                CanvasCaptcha.Children.Add(line);
            }

            double x = 25;
            for (int i = 0; i < captchaText.Length; i++)
            {
                TextBlock charBlock = new TextBlock
                {
                    Text = captchaText[i].ToString(),
                    FontSize = 26, 
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DarkBlue
                };

                double y = 20 + random.Next(-3, 4); 

                Canvas.SetLeft(charBlock, x);
                Canvas.SetTop(charBlock, y);
                CanvasCaptcha.Children.Add(charBlock);

                Line strike = new Line
                {
                    X1 = x - 5,
                    Y1 = y + 13,
                    X2 = x + 25,
                    Y2 = y + 13,
                    Stroke = Brushes.Red,
                    StrokeThickness = 2,
                    Opacity = 0.8
                };
                CanvasCaptcha.Children.Add(strike);

                x += 35; 
            }
        }

        // Метод для показа капчи
        private void ShowCaptcha()
        {
            if (!isCaptchaShown)
            {
                isCaptchaShown = true;
                GenerateCaptcha();
                BorderCaptcha.Visibility = Visibility.Visible;
                TextBlockCaptchaInput.Visibility = Visibility.Visible;
                TBoxCaptcha.Visibility = Visibility.Visible;
                TBoxCaptcha.Text = "";
                TBoxCaptcha.Focus();
            }
        }

        private void BtnAuthGuest_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ProductPage());
        }

        private void BtnAuth_Click(object sender, RoutedEventArgs e)
        {
            if (isBlocked)
            {
                MessageBox.Show("Кнопка заблокирована. Подождите " + secondsLeft + " секунд.");
                return;
            }

            string login = TBoxLogin.Text;
            string password = TBoxPassword.Text;

            if (login == "" || password == "")
            {
                MessageBox.Show("Есть пустые поля");
                return;
            }

            // Если капча показана
            if (isCaptchaShown)
            {
                string captchaInput = TBoxCaptcha.Text;
                if (captchaInput == "")
                {
                    MessageBox.Show("Введите капчу");
                    return;
                }

                // Проверяем пользователя и капчу
                User user = Vakhitova41Entities.GetContext().User.ToList()
                    .Find(p => p.UserLogin == login && p.UserPassword == password);

                if (user != null && captchaInput == captchaText)
                {
                    // Успешный вход
                    Manager.MainFrame.Navigate(new ProductPage(user));
                    TBoxLogin.Text = "";
                    TBoxPassword.Text = "";
                    TBoxCaptcha.Text = "";

                    HideCaptcha();
                    isCaptchaShown = false;
                }
                else
                {
                    // Неверные данные или капча
                    if (captchaInput != captchaText)
                    {
                        MessageBox.Show("Неверная капча!");
                    }
                    else
                    {
                        MessageBox.Show("Введены неверные данные");
                    }

                    // Генерируем новую капчу
                    GenerateCaptcha();
                    TBoxCaptcha.Text = "";

                    BlockButtonFor10Seconds();
                }
            }
            else
            {
                // Первая попытка (капча не показана)
                User user = Vakhitova41Entities.GetContext().User.ToList()
                    .Find(p => p.UserLogin == login && p.UserPassword == password);

                if (user != null)
                {
                    // Успешный вход
                    Manager.MainFrame.Navigate(new ProductPage(user));
                    TBoxLogin.Text = "";
                    TBoxPassword.Text = "";
                }
                else
                {
                    // Первая неудачная попытка - показываем капчу
                    ShowCaptcha();
                    MessageBox.Show("Введены неверные данные");
                }
            }
        }

        private void HideCaptcha()
        {
            BorderCaptcha.Visibility = Visibility.Hidden;
            TextBlockCaptchaInput.Visibility = Visibility.Hidden;
            TBoxCaptcha.Visibility = Visibility.Hidden;
        }


        // Метод для блокировки кнопки на 10 секунд
        private void BlockButtonFor10Seconds()
        {
            isBlocked = true;
            secondsLeft = 10;
            BtnAuth.IsEnabled = false;
            BtnAuth.Content = $"Заблокировано ({secondsLeft} сек)";

            // сохраняем, какое поле было в фокусе перед блокировкой
            var focusedControl = FocusManager.GetFocusedElement(this);

            // таймер создается и запускается
            blockTimer = new DispatcherTimer();
            blockTimer.Interval = TimeSpan.FromSeconds(1);
            blockTimer.Tick += (s, args) => BlockTimer_Tick(s, args, focusedControl);
            blockTimer.Start();
        }


        // Обработчик таймера
        private void BlockTimer_Tick(object sender, EventArgs e, IInputElement previouslyFocusedControl = null)
        {
            secondsLeft--;

            if (secondsLeft > 0)
            {
                BtnAuth.Content = $"Заблокировано ({secondsLeft} сек)";
            }
            else
            {
                // остановка таймера и разблокировка
                blockTimer.Stop();
                isBlocked = false;
                BtnAuth.IsEnabled = true;
                BtnAuth.Content = "Войти";

                //  НЕ очищаем поле капчи после разблокировки
                // Восстанавливаем фокус на том элементе, который был в фокусе до блокировки
                if (previouslyFocusedControl != null)
                {
                    // Используем Dispatcher для установки фокуса после разблокировки
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        previouslyFocusedControl.Focus();
                    }), DispatcherPriority.Background);
                }
            }
        }
    }
}

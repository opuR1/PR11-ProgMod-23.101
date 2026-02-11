using sommer_al.Models;
using sommer_al.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace sommer_al.Pages
{
    public partial class ChangePass : Page
    {
        private readonly EmailService _emailService;
        private readonly PasswordRecoveryService _recoveryService;
        private readonly TwoFactorService _twoFactorService;
        private string _userEmail;
        private int _currentStep = 1;
        private DispatcherTimer _resendTimer;
        private int _resendCooldown = 60;

        public ChangePass()
        {
            InitializeComponent();
            _emailService = new EmailService(
                "smtp.yandex.ru",
                 587,
                "sommeraleksey@yandex.ru",
                "emkhjurdwzxsibvo"
            );
            _recoveryService = new PasswordRecoveryService();
            _twoFactorService = new TwoFactorService();

            UpdateStep();
            InitializeResendTimer();
        }

        private void InitializeResendTimer()
        {
            _resendTimer = new DispatcherTimer();
            _resendTimer.Interval = TimeSpan.FromSeconds(1);
            _resendTimer.Tick += ResendTimer_Tick;
        }

        private void ResendTimer_Tick(object sender, EventArgs e)
        {
            _resendCooldown--;

            if (_resendCooldown <= 0)
            {
                _resendTimer.Stop();
                btnResendCode.IsEnabled = true;
                btnResendCode.Content = "Отправить код повторно";
            }
            else
            {
                btnResendCode.Content = $"Повторно через {_resendCooldown} сек.";
            }
        }

        private void UpdateStep()
        {
            step1Border.Visibility = _currentStep == 1 ? Visibility.Visible : Visibility.Collapsed;
            step2Border.Visibility = _currentStep == 2 ? Visibility.Visible : Visibility.Collapsed;
            step3Border.Visibility = _currentStep == 3 ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void btnSendCode_Click(object sender, RoutedEventArgs e)
        {
            string loginOrEmail = tbLogin.Text.Trim();

            if (string.IsNullOrEmpty(loginOrEmail))
            {
                ShowStep1Error("Пожалуйста, введите email");
                return;
            }

            if (!loginOrEmail.Contains("@"))
            {
                ShowStep1Error("Пожалуйста, введите корректный email адрес");
                return;
            }

            btnSendCode.IsEnabled = false;
            btnSendCode.Content = "Отправка...";

            try
            {
                if (_recoveryService.StartPasswordRecovery(loginOrEmail, out _userEmail))
                {
                    string recoveryCode = _recoveryService.GenerateRecoveryCode();
                    await _emailService.SendVerificationCodeAsync(_userEmail, recoveryCode, false);

                    _currentStep = 2;
                    UpdateStep();
                    tbCodeInfo.Text = $"Код подтверждения отправлен на email: {_userEmail}";
                    ClearErrors();

                    btnResendCode.IsEnabled = false;
                    _resendCooldown = 60;
                    _resendTimer.Start();
                }
                else
                {
                    ShowStep1Error("Пользователь с таким email не найден");
                }
            }
            catch (Exception ex)
            {
                ShowStep1Error($"Ошибка отправки кода: {ex.Message}");
            }
            finally
            {
                btnSendCode.IsEnabled = true;
                btnSendCode.Content = "Отправить код";
            }
        }

        private async void btnResendCode_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_userEmail))
            {
                ShowStep2Error("Ошибка: email не указан");
                return;
            }

            btnResendCode.IsEnabled = false;

            try
            {
                string newCode = _recoveryService.GenerateRecoveryCode();

                await _emailService.SendVerificationCodeAsync(_userEmail, newCode, false);

                ShowStep2Success("Новый код отправлен!");

                _resendCooldown = 60;
                _resendTimer.Start();
            }
            catch (Exception ex)
            {
                ShowStep2Error($"Ошибка отправки кода: {ex.Message}");
                btnResendCode.IsEnabled = true;
            }
        }

        private void btnVerifyCode_Click(object sender, RoutedEventArgs e)
        {
            string code = tbVerificationCode.Text.Trim();

            if (string.IsNullOrEmpty(code) || code.Length != 4)
            {
                ShowStep2Error("Введите 4-значный код подтверждения");
                return;
            }

            if (!code.All(char.IsDigit))
            {
                ShowStep2Error("Код должен содержать только цифры");
                return;
            }

            if (_recoveryService.VerifyRecoveryCode(_userEmail, code))
            {
                _currentStep = 3;
                UpdateStep();
                ClearErrors();
                _resendTimer.Stop();
            }
            else
            {
                ShowStep2Error("Неверный код или срок его действия истек");
            }
        }

        private void btnSavePassword_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = tbNewPassword.Password;
            string confirmPassword = tbConfirmPassword.Password;

            if (string.IsNullOrEmpty(newPassword))
            {
                ShowStep3Error("Введите новый пароль");
                return;
            }

            if (newPassword.Length < 6)
            {
                ShowStep3Error("Пароль должен содержать не менее 6 символов");
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowStep3Error("Пароли не совпадают");
                return;
            }

            if (_recoveryService.ChangePassword(_userEmail, newPassword))
            {
                ShowStep3Success("Пароль успешно изменен!");
                tbStep3Error.Visibility = Visibility.Collapsed;
                btnSavePassword.IsEnabled = false;
                btnCancel.Content = "Вернуться к авторизации";

                var timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(3);
                timer.Tick += (s, args) =>
                {
                    timer.Stop();
                    NavigationService.GoBack();
                };
                timer.Start();
            }
            else
            {
                ShowStep3Error("Ошибка при изменении пароля");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void ShowStep1Error(string message)
        {
            tbStep1Error.Text = message;
            tbStep1Error.Visibility = Visibility.Visible;
            tbStep2Error.Visibility = Visibility.Collapsed;
            tbStep3Error.Visibility = Visibility.Collapsed;
        }

        private void ShowStep2Error(string message)
        {
            tbStep2Error.Text = message;
            tbStep2Error.Visibility = Visibility.Visible;
            tbStep1Error.Visibility = Visibility.Collapsed;
            tbStep3Error.Visibility = Visibility.Collapsed;
        }

        private void ShowStep2Success(string message)
        {
            tbStep2Error.Text = message;
            tbStep2Error.Foreground = new SolidColorBrush(Colors.Green);
            tbStep2Error.Visibility = Visibility.Visible;

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                tbStep2Error.Foreground = new SolidColorBrush(Colors.Red);
                tbStep2Error.Visibility = Visibility.Collapsed;
            };
            timer.Start();
        }

        private void ShowStep3Error(string message)
        {
            tbStep3Error.Text = message;
            tbStep3Error.Visibility = Visibility.Visible;
            tbStep3Success.Visibility = Visibility.Collapsed;
            tbStep1Error.Visibility = Visibility.Collapsed;
            tbStep2Error.Visibility = Visibility.Collapsed;
        }

        private void ShowStep3Success(string message)
        {
            tbStep3Success.Text = message;
            tbStep3Success.Visibility = Visibility.Visible;
            tbStep3Error.Visibility = Visibility.Collapsed;
        }

        private void ClearErrors()
        {
            tbStep1Error.Visibility = Visibility.Collapsed;
            tbStep2Error.Visibility = Visibility.Collapsed;
            tbStep3Error.Visibility = Visibility.Collapsed;
            tbStep3Success.Visibility = Visibility.Collapsed;
        }
    }
}
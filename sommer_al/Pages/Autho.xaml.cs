using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using sommer_al.Models;
using sommer_al.Services;

namespace sommer_al.Pages
{
    public partial class Page1 : Page
    {
        int click;
        int failedAttempts;
        DispatcherTimer blockTimer;
        int blockTimeRemaining;

        private EmailService _emailService;
        private TwoFactorService _twoFactorService;

        private Users _pendingUser;
        private string _pendingLoginEmail;
        private string _pendingHashedPassword;
        private bool _pendingIsEmployee;
        private int _pendingRoleType;

        public Page1()
        {
            InitializeComponent();
            click = 0;
            failedAttempts = 0;
            tbCaptcha.Visibility = Visibility.Collapsed;
            tblCaptcha.Visibility = Visibility.Collapsed;
            borderTwoFactor.Visibility = Visibility.Collapsed;

            _emailService = new EmailService(
                "smtp.yandex.ru",
                 587,
                "sommeraleksey@yandex.ru",
                "emkhjurdwzxsibvo"
            );
            _twoFactorService = new TwoFactorService();

            blockTimer = new DispatcherTimer();
            blockTimer.Interval = TimeSpan.FromSeconds(1);
            blockTimer.Tick += BlockTimer_Tick;
        }

        private void BlockTimer_Tick(object sender, EventArgs e)
        {
            blockTimeRemaining--;
            tbTimer.Text = $"Система заблокирована. Осталось: {blockTimeRemaining} сек.";

            if (blockTimeRemaining <= 0)
            {
                blockTimer.Stop();
                UnlockInterface();
                failedAttempts = 0;
            }
        }

        private void LockInterface()
        {
            tbLogin.IsEnabled = false;
            tbPassword.IsEnabled = false;
            tbCaptcha.IsEnabled = false;
            chkIsEmployee.IsEnabled = false;
            btnEnter.IsEnabled = false;
            btnRegister.IsEnabled = false;
            ChangePassword.IsEnabled = false;

            tbTimer.Visibility = Visibility.Visible;
            blockTimeRemaining = 10;
            tbTimer.Text = $"Система заблокирована. Осталось: {blockTimeRemaining} сек.";
        }

        private void UnlockInterface()
        {
            tbLogin.IsEnabled = true;
            tbPassword.IsEnabled = true;
            tbCaptcha.IsEnabled = true;
            chkIsEmployee.IsEnabled = true;
            btnEnter.IsEnabled = true;
            btnRegister.IsEnabled = true;
            ChangePassword.IsEnabled = true;

            tbTimer.Visibility = Visibility.Collapsed;
            ClearFields();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Registration());
        }

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ChangePass());
        }

        private void GenerateCaptcha()
        {
            tbCaptcha.Visibility = Visibility.Visible;
            tblCaptcha.Visibility = Visibility.Visible;
            string captchaText = CaptchaGenerator.GenerateCaptchaText(6);
            tblCaptcha.Text = captchaText;
            tblCaptcha.TextDecorations = TextDecorations.Strikethrough;
        }

        private async void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            if (!btnEnter.IsEnabled)
                return;

            click += 1;
            string login = tbLogin.Text.Trim();
            string password = tbPassword.Password.Trim();
            bool isEmployee = chkIsEmployee.IsChecked ?? false;
            int RoleType = isEmployee ? 1 : 2;
            string HashedPassword = Hash.HashPassword(password);

            using (var db = ProgramModPR5Entities3.GetContext())
            {
                var user = db.Users.FirstOrDefault(x =>
                    x.Email == login && x.Password == HashedPassword && RoleType == x.RoleID);

                if (isEmployee && !TimeServices.isWorkHours())
                {
                    MessageBox.Show("Доступ запрещен! Рабочее время с 10:00 до 19:00");
                    failedAttempts++;
                    CheckFailedAttempts();
                    return;
                }

                if (click == 1)
                {
                    if (user != null)
                    {
                        if (_twoFactorService.IsTwoFactorEnabled(user.UserID))
                        {
                            _pendingUser = user;
                            _pendingLoginEmail = login;
                            _pendingHashedPassword = HashedPassword;
                            _pendingIsEmployee = isEmployee;
                            _pendingRoleType = RoleType;

                            tbLogin.IsEnabled = false;
                            tbPassword.IsEnabled = false;
                            chkIsEmployee.IsEnabled = false;
                            btnEnter.IsEnabled = false;

                            string verificationCode = _twoFactorService.GenerateVerificationCode();
                            _twoFactorService.SaveVerificationCode(login, verificationCode, user.UserID);

                            try
                            {
                                await _emailService.SendVerificationCodeAsync(login, verificationCode, true);

                                borderTwoFactor.Visibility = Visibility.Visible;
                                tbTwoFactorInfo.Text = $"На email {login} отправлен 4-значный код подтверждения";

                                MessageBox.Show("Код подтверждения отправлен на ваш email",
                                    "Двухфакторная аутентификация",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Не удалось отправить код: {ex.Message}",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                                tbLogin.IsEnabled = true;
                                tbPassword.IsEnabled = true;
                                chkIsEmployee.IsEnabled = true;
                                btnEnter.IsEnabled = true;
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Вы вошли под: {user.Roles.Role}", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            failedAttempts = 0;
                            LoadPage(user.Roles.Role.ToString(), user);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Неверный email или пароль!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        failedAttempts++;
                        GenerateCaptcha();
                        ClearFields();
                        CheckFailedAttempts();
                    }
                }
                else if (click > 1)
                {
                    if (user != null && tbCaptcha.Text == tblCaptcha.Text)
                    {
                        if (_twoFactorService.IsTwoFactorEnabled(user.UserID))
                        {
                            _pendingUser = user;
                            _pendingLoginEmail = login;
                            _pendingHashedPassword = HashedPassword;
                            _pendingIsEmployee = isEmployee;
                            _pendingRoleType = RoleType;

                            string verificationCode = _twoFactorService.GenerateVerificationCode();
                            _twoFactorService.SaveVerificationCode(login, verificationCode, user.UserID);

                            try
                            {
                                await _emailService.SendVerificationCodeAsync(login, verificationCode, true);

                                tbLogin.IsEnabled = false;
                                tbPassword.IsEnabled = false;
                                chkIsEmployee.IsEnabled = false;
                                btnEnter.IsEnabled = false;

                                borderTwoFactor.Visibility = Visibility.Visible;
                                tbTwoFactorInfo.Text = $"На email {login} отправлен 4-значный код подтверждения";

                                MessageBox.Show("Код подтверждения отправлен на ваш email",
                                    "Двухфакторная аутентификация",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Не удалось отправить код: {ex.Message}",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                                tbLogin.IsEnabled = true;
                                tbPassword.IsEnabled = true;
                                chkIsEmployee.IsEnabled = true;
                                btnEnter.IsEnabled = true;
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Вы вошли под: {user.Roles.Role}", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            failedAttempts = 0;
                            LoadPage(user.Roles.Role.ToString(), user);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Неверные данные или капча!", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        failedAttempts++;
                        ClearFields();
                        CheckFailedAttempts();
                    }
                }
            }
        }

        private void btnVerifyTwoFactor_Click(object sender, RoutedEventArgs e)
        {
            string inputCode = tbTwoFactorCode.Text.Trim();

            if (string.IsNullOrEmpty(inputCode) || inputCode.Length != 4)
            {
                MessageBox.Show("Введите 4-значный код", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!inputCode.All(char.IsDigit))
            {
                MessageBox.Show("Код должен содержать только цифры", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_twoFactorService.VerifyCode(_pendingLoginEmail, inputCode, out int userId))
            {
                if (_pendingUser != null && _pendingUser.UserID == userId)
                {
                    MessageBox.Show("Двухфакторная аутентификация пройдена успешно!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    borderTwoFactor.Visibility = Visibility.Collapsed;
                    failedAttempts = 0;
                    LoadPage(_pendingUser.Roles.Role.ToString(), _pendingUser);
                }
                else
                {
                    MessageBox.Show("Ошибка проверки пользователя", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Неверный код или срок его действия истек", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                tbTwoFactorCode.Clear();
                tbTwoFactorCode.Focus();
            }
        }

        private void btnCancelTwoFactor_Click(object sender, RoutedEventArgs e)
        {
            borderTwoFactor.Visibility = Visibility.Collapsed;
            tbTwoFactorCode.Clear();

            tbLogin.IsEnabled = true;
            tbPassword.IsEnabled = true;
            chkIsEmployee.IsEnabled = true;
            btnEnter.IsEnabled = true;

            ClearFields();
        }

        private void CheckFailedAttempts()
        {
            if (failedAttempts >= 3)
            {
                LockInterface();
                blockTimer.Start();
            }
        }

        private void LoadPage(string _role, Users user)
        {
            click = 0;
            switch (_role)
            {
                case "Клиент":
                    NavigationService.Navigate(new Client(user, _role));
                    break;
                case "Сотрудник":
                    NavigationService.Navigate(new Employee(user, _role));
                    break;
            }
        }

        private void ClearFields()
        {
            tbPassword.Clear();
            tbCaptcha.Clear();
            tbCaptcha.Visibility = Visibility.Collapsed;
            tblCaptcha.Visibility = Visibility.Collapsed;

            if (failedAttempts > 0)
            {
                GenerateCaptcha();
            }
        }
    }
}
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

        private async void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            string login = tbLogin.Text;
            string password = tbPassword.Password;
            bool isEmployee = chkIsEmployee.IsChecked == true;
            int roleType = isEmployee ? 1 : 2;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (failedAttempts > 0 && tbCaptcha.Text != tblCaptcha.Text)
            {
                MessageBox.Show("Неверная капча!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                GenerateCaptcha();
                return;
            }

            string hashedPassword = Hash.HashPassword(password);

            using (var db = new ProgramModPR5Entities3())
            {
                var user = db.Users.FirstOrDefault(u => u.Email == login && u.Password == hashedPassword && u.RoleID == roleType);

                if (user != null)
                {
                    if (user.IsTwoFactorEnabled == 1)
                    {
                        _pendingUser = user;
                        string code = _twoFactorService.GenerateVerificationCode();
                        _twoFactorService.SaveVerificationCode(user.Email, code, user.UserID);

                        try
                        {
                            await _emailService.SendVerificationCodeAsync(user.Email, code, true);

                            borderTwoFactor.Visibility = Visibility.Visible;
                            tbLogin.IsEnabled = false;
                            tbPassword.IsEnabled = false;
                            chkIsEmployee.IsEnabled = false;
                            btnEnter.IsEnabled = false;

                            tbTwoFactorCode.Focus();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка отправки кода: " + ex.Message);
                        }
                    }
                    else
                    {
                        failedAttempts = 0;
                        LoadPage(isEmployee ? "Сотрудник" : "Клиент", user);
                    }
                }
                else
                {
                    failedAttempts++;
                    MessageBox.Show($"Неверный логин или пароль! Попыток: {failedAttempts}");
                    CheckFailedAttempts();
                    GenerateCaptcha();
                }
            }
        }

        private void btnVerifyTwoFactor_Click(object sender, RoutedEventArgs e)
        {
            if (_pendingUser == null) return;

            int userId;
            if (_twoFactorService.VerifyCode(_pendingUser.Email, tbTwoFactorCode.Text, out userId))
            {
                string role = chkIsEmployee.IsChecked == true ? "Сотрудник" : "Клиент";
                LoadPage(role, _pendingUser);
            }
            else
            {
                MessageBox.Show("Неверный или просроченный код подтверждения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                tbTwoFactorCode.Clear();
            }
        }

        private void btnCancelTwoFactor_Click(object sender, RoutedEventArgs e)
        {
            borderTwoFactor.Visibility = Visibility.Collapsed;
            tbTwoFactorCode.Clear();
            _pendingUser = null;

            tbLogin.IsEnabled = true;
            tbPassword.IsEnabled = true;
            chkIsEmployee.IsEnabled = true;
            btnEnter.IsEnabled = true;
        }

        private void GenerateCaptcha()
        {
            string allowchar = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
            allowchar += "a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,y,z";
            allowchar += "1,2,3,4,5,6,7,8,9,0";
            char[] a = { ',' };
            string[] ar = allowchar.Split(a);
            string pwd = "";
            Random r = new Random();
            for (int i = 0; i < 6; i++)
            {
                pwd += ar[r.Next(0, ar.Length)];
            }
            tblCaptcha.Text = pwd;
            tblCaptcha.Visibility = Visibility.Visible;
            tbCaptcha.Visibility = Visibility.Visible;
        }

        private void CheckFailedAttempts()
        {
            if (failedAttempts >= 3)
            {
                LockInterface();
                blockTimeRemaining = 10;
                blockTimer.Start();
            }
        }

        private void LockInterface()
        {
            btnEnter.IsEnabled = false;
            tbLogin.IsEnabled = false;
            tbPassword.IsEnabled = false;
            tbCaptcha.IsEnabled = false;
        }

        private void UnlockInterface()
        {
            btnEnter.IsEnabled = true;
            tbLogin.IsEnabled = true;
            tbPassword.IsEnabled = true;
            tbCaptcha.IsEnabled = true;
            tbTimer.Visibility = Visibility.Collapsed;
        }

        private void BlockTimer_Tick(object sender, EventArgs e)
        {
            if (blockTimeRemaining > 0)
            {
                tbTimer.Visibility = Visibility.Visible;
                tbTimer.Text = $"Вход заблокирован: {blockTimeRemaining} сек.";
                blockTimeRemaining--;
            }
            else
            {
                blockTimer.Stop();
                UnlockInterface();
            }
        }

        private void LoadPage(string _role, Users user)
        {
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

        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ChangePass());
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Registration());
        }
    }
}
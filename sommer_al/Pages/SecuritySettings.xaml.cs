using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using sommer_al.Models;
using sommer_al.Services;

namespace sommer_al.Pages
{
    public partial class SecuritySettings : Page
    {
        private readonly Users _currentUser;
        private readonly TwoFactorService _twoFactorService;

        public SecuritySettings(Users user)
        {
            InitializeComponent();
            _currentUser = user;
            _twoFactorService = new TwoFactorService();

            LoadSettings();
        }

        private void LoadSettings()
        {
            tbCurrentEmail.Text = $"Текущий email: {_currentUser.Email}";
            bool isTwoFactorEnabled = _twoFactorService.IsTwoFactorEnabled(_currentUser.UserID);
            cbEnableTwoFactor.IsChecked = isTwoFactorEnabled;

            UpdateStatusText(isTwoFactorEnabled);
        }

        private void UpdateStatusText(bool isEnabled)
        {
            if (isEnabled)
            {
                tbStatus.Text = "Статус: Двухфакторная аутентификация ВКЛЮЧЕНА";
                tbStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(46, 125, 50));
            }
            else
            {
                tbStatus.Text = "Статус: Двухфакторная аутентификация ОТКЛЮЧЕНА";
                tbStatus.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(211, 47, 47));
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            bool enableTwoFactor = cbEnableTwoFactor.IsChecked ?? false;

            if (enableTwoFactor)
                _twoFactorService.EnableTwoFactorAuth(_currentUser.UserID);
            else
                _twoFactorService.DisableTwoFactorAuth(_currentUser.UserID);

            MessageBox.Show("Настройки безопасности сохранены в базе данных.", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);

            UpdateStatusText(enableTwoFactor);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
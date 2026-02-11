using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using sommer_al.Models;
using sommer_al.Services;
using System.Data.Entity;

namespace sommer_al.Pages
{
    public partial class Profile : Page
    {
        private readonly Users _currentUser;
        private Models.Clients _clientInfo;
        private Models.Employees _employeeInfo;
        private string _userRole;

        public Profile(Users user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadUserData();
            DisplayUserInfo();
        }

        private void LoadUserData()
        {
            using (var db = ProgramModPR5Entities3.GetContext())
            {
                var role = db.Roles.FirstOrDefault(r => r.RoleID == _currentUser.RoleID);
                _userRole = role?.Role ?? "Неизвестная роль";

                if (_userRole == "Клиент")
                {
                    _clientInfo = db.Clients.FirstOrDefault(c => c.UserID == _currentUser.UserID);
                    panelClientInfo.Visibility = Visibility.Visible;
                    panelEmployeeInfo.Visibility = Visibility.Collapsed;
                    tbAdditionalInfoTitle.Text = "Информация о клиенте";
                }
                else if (_userRole == "Сотрудник")
                {
                    _employeeInfo = db.Employees
                        .Include("Positions")
                        .FirstOrDefault(e => e.UserID == _currentUser.UserID);
                    panelClientInfo.Visibility = Visibility.Collapsed;
                    panelEmployeeInfo.Visibility = Visibility.Visible;
                    tbAdditionalInfoTitle.Text = "Информация о сотруднике";
                }
            }
        }

        private void DisplayUserInfo()
        {
            tbLogin.Text = _currentUser.Email;
            tbRole.Text = _userRole;
            tbRoleDetail.Text = _userRole;
            tbUserId.Text = _currentUser.UserID.ToString();
            tbEmail.Text = _currentUser.Email;

            string fullName = "";
            string initials = "";

            if (_userRole == "Клиент" && _clientInfo != null)
            {
                fullName = $"{_clientInfo.LastName} {_clientInfo.FirstName}";
                if (!string.IsNullOrEmpty(_clientInfo.SurName))
                {
                    fullName += $" {_clientInfo.SurName}";
                }

                initials = GetInitials(_clientInfo.FirstName, _clientInfo.LastName);

                tbClientLastName.Text = _clientInfo.LastName ?? "Не указано";
                tbClientFirstName.Text = _clientInfo.FirstName ?? "Не указано";
                tbClientSurName.Text = _clientInfo.SurName ?? "Не указано";

                tbFullName.Text = fullName;
            }
            else if (_userRole == "Сотрудник" && _employeeInfo != null)
            {
                fullName = $"{_employeeInfo.LastName} {_employeeInfo.FirstName}";
                if (!string.IsNullOrEmpty(_employeeInfo.SurName))
                {
                    fullName += $" {_employeeInfo.SurName}";
                }

                initials = GetInitials(_employeeInfo.FirstName, _employeeInfo.LastName);

                tbEmployeeLastName.Text = _employeeInfo.LastName ?? "Не указано";
                tbEmployeeFirstName.Text = _employeeInfo.FirstName ?? "Не указано";
                tbEmployeeSurName.Text = _employeeInfo.SurName ?? "Не указано";
                tbEmployeePhone.Text = _employeeInfo.Phone ?? "Не указано";
                tbEmployeePosition.Text = _employeeInfo.Positions?.PositionName ?? "Не указано";

                tbFullName.Text = fullName;
            }
            else
            {
                tbFullName.Text = "Пользователь";
                initials = "П";
            }

            var avatarTextBlock = FindName("UserInitials") as TextBlock;
            if (avatarTextBlock != null)
            {
                avatarTextBlock.Text = initials;
            }

            var avatarBorder = FindName("avatarBorder") as Border;
            if (avatarBorder != null)
            {
                if (_userRole == "Клиент")
                {
                    avatarBorder.Background = new SolidColorBrush(Color.FromRgb(33, 150, 243));
                }
                else if (_userRole == "Сотрудник")
                {
                    avatarBorder.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80));
                }
            }
        }

        private string GetInitials(string firstName, string lastName)
        {
            string initials = "";

            if (!string.IsNullOrEmpty(firstName) && firstName.Length > 0)
                initials += firstName[0];

            if (!string.IsNullOrEmpty(lastName) && lastName.Length > 0)
                initials += lastName[0];

            return initials.ToUpper();
        }

        private void btnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция смены пароля будет реализована позже.", "В разработке",
                MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void btnSecuritySettings_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SecuritySettings(_currentUser));
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole == "Клиент")
            {
                NavigationService.Navigate(new Client(_currentUser, _userRole));
            }
            else if (_userRole == "Сотрудник")
            {
                NavigationService.Navigate(new Employee(_currentUser, _userRole));
            }
            else
            {
                NavigationService.GoBack();
            }
        }
    }
}
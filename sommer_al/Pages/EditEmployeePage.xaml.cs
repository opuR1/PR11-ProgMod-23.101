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
using sommer_al.Models;
using sommer_al.Services;

namespace sommer_al.Pages
{
    /// <summary>
    /// Логика взаимодействия для EditEmployeePage.xaml
    /// </summary>
    public partial class EditEmployeePage : Page
    {
        private Employees _currentEmployee;
        private ProgramModPR5Entities3 db = ProgramModPR5Entities3.GetContext();
        private bool isNewEmployee;
        public EditEmployeePage(Employees employee)
        {
            InitializeComponent();
            _currentEmployee = employee;
            isNewEmployee = employee == null;

            LoadPositions();

            if (isNewEmployee)
            {
                tblMode.Text = "Добавление нового сотрудника";
                _currentEmployee = new Employees();
            }
            else
            {
                tblMode.Text = $"Редактирование: {_currentEmployee.LastName} {_currentEmployee.FirstName}";
                LoadEmployeeData();
                tbEmail.IsEnabled = false;
                pnlNewUserFields.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadPositions()
        {
            try
            {
                cmbPosition.ItemsSource = db.Positions.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки должностей: {ex.Message}");
            }
        }

        private void LoadEmployeeData()
        {
            tbFirstName.Text = _currentEmployee.FirstName;
            tbLastName.Text = _currentEmployee.LastName;
            tbSurName.Text = _currentEmployee.SurName;
            tbEmail.Text = _currentEmployee.Email;
            tbPhone.Text = _currentEmployee.Phone;
            cmbPosition.SelectedValue = _currentEmployee.PositionID;
        }

        private bool ValidateFields()
        {
            
            var tempEmployee = new Employees
            {
                FirstName = tbFirstName.Text?.Trim() ?? "",
                LastName = tbLastName.Text?.Trim() ?? "",
                SurName = string.IsNullOrWhiteSpace(tbSurName.Text) ? null : tbSurName.Text.Trim(),
                PositionID = cmbPosition.SelectedValue != null ? (int)cmbPosition.SelectedValue : 0,
                Phone = string.IsNullOrWhiteSpace(tbPhone.Text) ? "" : tbPhone.Text.Trim(),
                Email = tbEmail.Text?.Trim() ?? "",
                UserID = _currentEmployee?.UserID ?? 0
            };

            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(tempEmployee);
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(tempEmployee, validationContext, results, true);

            if (!isValid)
            {
                tblError.Text = string.Join("\n", results.Select(r => r.ErrorMessage));
                return false;
            }
            

            if (isNewEmployee)
            {
                if (pbPassword.Password.Length < 6)
                {
                    tblError.Text = "Пароль должен содержать минимум 6 символов.\n";
                    return false;
                }
                else if (pbPassword.Password != pbConfirmPassword.Password)
                {
                    tblError.Text = "Пароли не совпадают.\n";
                    return false;
                }
            }

            tblError.Text = "";
            return true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
                return;

            try
            {
                if (isNewEmployee)
                {
                    if (db.Users.Any(u => u.Email == tbEmail.Text.Trim()))
                    {
                        tblError.Text = "Пользователь с таким Email уже существует.";
                        return;
                    }

                    var newUser = new Users
                    {
                        Email = tbEmail.Text.Trim(),
                        Password = Hash.HashPassword(pbPassword.Password),
                        RoleID = 1
                    };
                    db.Users.Add(newUser);
                    db.SaveChanges();

                    var newEmployee = new Employees
                    {
                        UserID = newUser.UserID,
                        Email = tbEmail.Text.Trim(),
                        FirstName = tbFirstName.Text.Trim(),
                        LastName = tbLastName.Text.Trim(),
                        SurName = string.IsNullOrWhiteSpace(tbSurName.Text) ? null : tbSurName.Text.Trim(),
                        Phone = string.IsNullOrWhiteSpace(tbPhone.Text) ? null : tbPhone.Text.Trim(),
                        PositionID = (int)cmbPosition.SelectedValue
                    };

                    var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(newEmployee);
                    var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                    bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(newEmployee, validationContext, validationResults, true);

                    if (!isValid)
                    {
                        tblError.Text = string.Join("\n", validationResults.Select(r => r.ErrorMessage));
                        return;
                    }

                    db.Employees.Add(newEmployee);
                    db.SaveChanges();

                    MessageBox.Show("Сотрудник успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _currentEmployee.FirstName = tbFirstName.Text.Trim();
                    _currentEmployee.LastName = tbLastName.Text.Trim();
                    _currentEmployee.SurName = string.IsNullOrWhiteSpace(tbSurName.Text) ? null : tbSurName.Text.Trim();
                    _currentEmployee.Phone = string.IsNullOrWhiteSpace(tbPhone.Text) ? null : tbPhone.Text.Trim();
                    _currentEmployee.PositionID = (int)cmbPosition.SelectedValue;

                    var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(_currentEmployee);
                    var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                    bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(_currentEmployee, validationContext, results, true);

                    if (!isValid)
                    {
                        tblError.Text = string.Join("\n", results.Select(r => r.ErrorMessage));
                        return;
                    }

                    db.SaveChanges();
                    MessageBox.Show("Данные сотрудника успешно обновлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
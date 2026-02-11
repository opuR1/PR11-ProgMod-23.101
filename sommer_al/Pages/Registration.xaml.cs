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
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class Registration : Page
    {
        int attempts = 0;

        public Registration()
        {
            InitializeComponent();
            attempts = 0;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            attempts++;

            
            if (string.IsNullOrWhiteSpace(tbFirstName.Text) ||
                string.IsNullOrWhiteSpace(tbLastName.Text) ||
                string.IsNullOrWhiteSpace(tbEmail.Text) ||
                tbPassword.Password.Length == 0 ||
                tbConfirmPassword.Password.Length == 0)
            {
                ShowError("Заполните все обязательные поля (отмечены *)");
                return;
            }

            
            if (tbPassword.Password != tbConfirmPassword.Password)
            {
                ShowError("Пароли не совпадают!");
                return;
            }

           
            if (tbPassword.Password.Length < 6)
            {
                ShowError("Пароль должен содержать минимум 6 символов!");
                return;
            }

            
            if (attempts > 2)
            {
                if (string.IsNullOrWhiteSpace(tbCaptcha.Text) || tbCaptcha.Text != tblCaptcha.Text)
                {
                    ShowError("Неверная капча!");
                    GenerateCaptcha();
                    return;
                }
            }

            try
            {
                
                string hashedPassword = Hash.HashPassword(tbPassword.Password);
                bool isEmployee = chkIsEmployee.IsChecked ?? false;

                using (var context = ProgramModPR5Entities3.GetContext())
                {
                    
                    if (context.Users.Any(u => u.Email == tbEmail.Text.Trim()))
                    {
                        ShowError("Пользователь с таким email уже существует!");
                        return;
                    }

                    
                    var newUser = new Users
                    {
                        Email = tbEmail.Text.Trim(),
                        Password = hashedPassword,
                        RoleID = isEmployee ? 1 : 2 
                    };

                    context.Users.Add(newUser);
                    context.SaveChanges();

                    
                    int newUserId = newUser.UserID;

                    if (isEmployee)
                    {
                        
                        var newEmployee = new Employees
                        {
                            FirstName = tbFirstName.Text.Trim(),
                            LastName = tbLastName.Text.Trim(),
                            SurName = string.IsNullOrWhiteSpace(tbSurName.Text) ? null : tbSurName.Text.Trim(),
                            PositionID = 1, 
                            Phone = string.IsNullOrWhiteSpace(tbPhone.Text) ? null : tbPhone.Text.Trim(),
                            Email = tbEmail.Text.Trim(),
                            UserID = newUserId
                        };

                        var contextValidation = new System.ComponentModel.DataAnnotations.ValidationContext(newEmployee);
                        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                        bool isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(newEmployee, contextValidation, results, true);

                        if (!isValid)
                        {
                            ShowError(string.Join("\n", results.Select(r => r.ErrorMessage)));
                            return;
                        }
                        

                        context.Employees.Add(newEmployee);
                    }
                    else
                    {
                        
                        var newClient = new Clients
                        {
                            FirstName = tbFirstName.Text.Trim(),
                            LastName = tbLastName.Text.Trim(),
                            SurName = string.IsNullOrWhiteSpace(tbSurName.Text) ? null : tbSurName.Text.Trim(),
                            Email = tbEmail.Text.Trim(),
                            UserID = newUserId
                        };
                        context.Clients.Add(newClient);
                    }

                    context.SaveChanges();

                    MessageBox.Show("Регистрация прошла успешно! Теперь вы можете войти в систему.",
                                  "Успешная регистрация",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);

                    
                    NavigationService.GoBack();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка регистрации: {ex.Message}");
                if (attempts > 1)
                {
                    GenerateCaptcha();
                }
            }
        }

        private void GenerateCaptcha()
        {
            tblCaptcha.Visibility = Visibility.Visible;
            tbCaptcha.Visibility = Visibility.Visible;

            string captchaText = CaptchaGenerator.GenerateCaptchaText(6);
            tblCaptcha.Text = captchaText;
            tblCaptcha.TextDecorations = TextDecorations.Strikethrough;

            tbCaptcha.Clear();
        }

        private void ShowError(string message)
        {
            tblError.Text = message;

            
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += (s, args) =>
            {
                tblError.Text = "";
                timer.Stop();
            };
            timer.Start();

            
            if (attempts >= 2 && tblCaptcha.Visibility != Visibility.Visible)
            {
                GenerateCaptcha();
            }
        }
    }
}
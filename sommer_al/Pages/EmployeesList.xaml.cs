using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace sommer_al.Pages
{
    /// <summary>
    /// Логика взаимодействия для EmployeesList.xaml
    /// </summary>
    public partial class EmployeesList : Page
    {
        private Users currentUser;
        private ProgramModPR5Entities3 db;
        private List<Employees> allEmployees;
        public EmployeesList(Users user)
        {
            InitializeComponent();
            currentUser = user;
            db = ProgramModPR5Entities3.GetContext();
            LoadEmployees();
        }
        private void LoadEmployees()
        {
            try
            {
                allEmployees = db.Employees.Include("Positions").ToList();
                FilterEmployees();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сотрудников: {ex.Message}");
            }
        }
        private void FilterEmployees()
        {
            string searchText = tbSearch.Text.ToLower().Trim();
            var filteredList = allEmployees.Where(e =>
                e.LastName.ToLower().Contains(searchText) ||
                e.FirstName.ToLower().Contains(searchText) ||
                (e.SurName != null && e.SurName.ToLower().Contains(searchText)) ||
                e.Email.ToLower().Contains(searchText) ||
                (e.Positions != null && e.Positions.PositionName.ToLower().Contains(searchText))
            ).ToList();

            dgEmployees.ItemsSource = new ObservableCollection<Employees>(filteredList);
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditEmployeePage(null));
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedEmployee = dgEmployees.SelectedItem as Employees;
            if (selectedEmployee == null)
            {
                MessageBox.Show("Выберите сотрудника для удаления.");
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить сотрудника {selectedEmployee.LastName} {selectedEmployee.FirstName}?",
                                         "Подтверждение удаления",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var userToDelete = db.Users.FirstOrDefault(u => u.UserID == selectedEmployee.UserID);
                    if (userToDelete != null)
                    {
                        db.Employees.Remove(selectedEmployee);
                        db.SaveChanges(); 

                        db.Users.Remove(userToDelete);
                        db.SaveChanges(); 

                        MessageBox.Show("Сотрудник и его учетная запись успешно удалены.");
                        LoadEmployees();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось найти связанную учетную запись пользователя.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}");
                }
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selectedEmployee = (sender as Button)?.DataContext as Employees;
            if (selectedEmployee != null)
            {
                
                NavigationService.Navigate(new EditEmployeePage(selectedEmployee));
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для редактирования.");
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadEmployees();
        }

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterEmployees();
        }
    }
}

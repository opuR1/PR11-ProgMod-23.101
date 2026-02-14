using sommer_al.Models;
using sommer_al.Services;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace sommer_al.Pages
{
    /// <summary>
    /// Логика взаимодействия для Employee.xaml
    /// </summary>
    public partial class Employee : Page
    {
        private Users currentUser;
        public Employee(Users user, string role)
        {
            InitializeComponent();
            currentUser = user;
            DisplayUserInfo();

        }
        private void DisplayUserInfo()
        {
            string greeting = TimeServices.GetGreeting();
            tbGreeting.Text = greeting + "!";
            ProgramModPR5Entities3 db = ProgramModPR5Entities3  .GetContext();

            var Employeer = db.Employees.Where(e => e.UserID == currentUser.UserID && e.Email == currentUser.Email).FirstOrDefault();

            string FirstName = Employeer.FirstName;
            string LastName = Employeer.LastName;
            string FullName = $"{LastName} {FirstName}";
            if (Employeer.SurName != null)
            {
                string SurName = Employeer.SurName;
                FullName = $"{LastName} {FirstName} {SurName}";
            }
            tbUserName.Text = FullName;
            
           
        }

        private void ButtonEmployees_Click(object sender, RoutedEventArgs e)
        {
            Users user = currentUser;
            NavigationService.Navigate(new EmployeesList(user));
        }

        private void ButtonShoes_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProductsList(currentUser));
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Profile(currentUser));
        }
    }
}

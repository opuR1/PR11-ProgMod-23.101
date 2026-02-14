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
    /// Логика взаимодействия для Client.xaml
    /// </summary>
    public partial class Client : Page
    {
        private Users currentUser;
        public Client(Users user, string role)
        {
            currentUser = user;
            InitializeComponent();
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

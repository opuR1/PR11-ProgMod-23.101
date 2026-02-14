using sommer_al.Pages;
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

namespace sommer_al
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            FrmMain.Navigate(new Page1());
        }

        private void FrmMain_ContentRendered(object sender, EventArgs e)
        {
            if (FrmMain.CanGoBack)
            {
                btn_back.Visibility = Visibility.Visible;
            }
            else
            {
                btn_back.Visibility = Visibility.Hidden;
            }
            if (FrmMain.Content is Page1 || FrmMain.Content is Registration)
            {
                btn_back.Visibility = Visibility.Collapsed;
            }
        }

        private void btn_back_Click(object sender, RoutedEventArgs e)
        {
            if (FrmMain.Content is Client || FrmMain.Content is Employee)
            {
                FrmMain.Navigate(new Page1());
            }
            else
            {
                FrmMain.GoBack();
            }

        }
    }
}

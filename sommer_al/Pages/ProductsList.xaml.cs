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

namespace sommer_al.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProductsList.xaml
    /// </summary>
    public class ProductDisplayModel : ShoeModels
    {
        public string SeasonName { get; set; }
    }
    public partial class ProductsList : Page
    {
        private ProgramModPR5Entities3 db = ProgramModPR5Entities3.GetContext();
        private List<ProductDisplayModel> _allProducts;
        private Users _currentUser;

        private Dictionary<int, string> _seasonMap = new Dictionary<int, string>
        {
            { 1, "Зима" },
            { 2, "Весна" },
            { 3, "Лето" },
            { 4, "Осень" }
        };
        public ProductsList(Users user)
        {
            InitializeComponent();
            _currentUser = user;
            cmbSeasonSort.SelectedIndex = 0;
            if (_currentUser != null && _currentUser.RoleID == 1)
            {
                btnAdd.Visibility = Visibility.Visible;
                btnEdit.Visibility = Visibility.Visible;
                btnDelete.Visibility = Visibility.Visible;
            }
        }

        private string GetSeasonName(int seasonId)
        {
            return _seasonMap.ContainsKey(seasonId) ? _seasonMap[seasonId] : "Неизвестно";
        }

        private void LoadProducts()
        {
            try
            {
                _allProducts = db.ShoeModels.ToList().Select(p => new ProductDisplayModel
                {
                    ModelID = p.ModelID,
                    ShoeModelName = p.ShoeModelName,
                    Article = p.Article,
                    SeasonID = p.SeasonID,
                    Price = p.Price,
                    CostPrice = p.CostPrice,
                    ModelDescription = p.ModelDescription,
                    SeasonName = GetSeasonName(p.SeasonID)
                }).ToList();

                ApplyFiltersAndSort();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}");
            }
        }
        private void ApplyFiltersAndSort()
        {
            if (_allProducts == null) return;

            IEnumerable<ProductDisplayModel> filteredProducts = _allProducts;
            string searchText = tbSearch.Text.ToLower().Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                filteredProducts = filteredProducts.Where(p =>
                    p.ShoeModelName.ToLower().Contains(searchText) ||
                    p.Article.ToLower().Contains(searchText)
                );
            }
            if (cmbSeasonSort.SelectedIndex > 0)
            {
                int selectedSeasonId = cmbSeasonSort.SelectedIndex;
                filteredProducts = filteredProducts.Where(p => p.SeasonID == selectedSeasonId);
            }
            lvProducts.ItemsSource = filteredProducts.ToList();
            tblItemCount.Text = $"Найдено: {filteredProducts.Count()}";
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        private void TbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void CmbSeasonSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lvProducts.SelectedItem is ProductDisplayModel selectedDisplayModel)
            {
                var productToEdit = db.ShoeModels.Find(selectedDisplayModel.ModelID);

                if (productToEdit != null)
                {
                    NavigationService.Navigate(new ProductEdit(productToEdit));
                }
            }
            else
            {
                MessageBox.Show("Выберите товар для изменения.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProductEdit(null));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lvProducts.SelectedItem is ProductDisplayModel selectedDisplayModel)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить товар '{selectedDisplayModel.ShoeModelName}' (Артикул: {selectedDisplayModel.Article})?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var productToDelete = db.ShoeModels.Find(selectedDisplayModel.ModelID);

                        if (productToDelete != null)
                        {
                            db.ShoeModels.Remove(productToDelete);
                            db.SaveChanges();
                            MessageBox.Show("Товар успешно удален.");
                            LoadProducts();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении товара: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите товар для удаления.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

}

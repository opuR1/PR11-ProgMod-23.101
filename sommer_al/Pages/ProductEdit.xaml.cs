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
    /// Логика взаимодействия для ProductEdit.xaml
    /// </summary>
    public partial class ProductEdit : Page
    {
        private ShoeModels _currentProduct;
        private ProgramModPR5Entities3 db = ProgramModPR5Entities3.GetContext();
        private bool isNewProduct;
        private Dictionary<int, string> _seasonMap = new Dictionary<int, string>
        {
            { 1, "Зима" },
            { 2, "Весна" },
            { 3, "Лето" },
            { 4, "Осень" }
        };
        public ProductEdit(ShoeModels product)
        {
            InitializeComponent();
            _currentProduct = product;
            isNewProduct = product == null;

            LoadSeasons();

            if (isNewProduct)
            {
                tblMode.Text = "Добавление нового товара";
                _currentProduct = new ShoeModels();
            }
            else
            {
                tblMode.Text = $"Редактирование: {_currentProduct.ShoeModelName}";
                LoadProductData();
            }
        }
        private void LoadSeasons()
        {
            cmbSeason.ItemsSource = _seasonMap;
        }
        private void LoadProductData()
        {
            tbName.Text = _currentProduct.ShoeModelName;
            tbArticle.Text = _currentProduct.Article;
            tbPrice.Text = _currentProduct.Price.ToString();
            tbCostPrice.Text = _currentProduct.CostPrice.ToString();
            tbDescription.Text = _currentProduct.ModelDescription;
            cmbSeason.SelectedValue = _currentProduct.SeasonID;
        }
        private bool ValidateFields(out decimal price, out decimal costPrice)
        {
            string error = "";
            price = 0;
            costPrice = 0;

            if (string.IsNullOrWhiteSpace(tbName.Text))
                error += "Введите название товара.\n";

            if (string.IsNullOrWhiteSpace(tbArticle.Text))
                error += "Введите артикул.\n";

            if (cmbSeason.SelectedValue == null)
                error += "Выберите сезон.\n";

            if (!decimal.TryParse(tbPrice.Text, out price) || price <= 0)
                error += "Введите корректную цену (продажа) > 0.\n";

            if (!decimal.TryParse(tbCostPrice.Text, out costPrice) || costPrice <= 0)
                error += "Введите корректную себестоимость > 0.\n";

            if (!string.IsNullOrWhiteSpace(error))
            {
                tblError.Text = error;
                return false;
            }

            tblError.Text = "";
            return true;
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            decimal price, costPrice;
            if (!ValidateFields(out price, out costPrice))
                return;

            try
            {
                _currentProduct.ShoeModelName = tbName.Text.Trim();
                _currentProduct.Article = tbArticle.Text.Trim();
                _currentProduct.SeasonID = (int)cmbSeason.SelectedValue;
                _currentProduct.Price = price;
                _currentProduct.CostPrice = costPrice;
                _currentProduct.ModelDescription = string.IsNullOrWhiteSpace(tbDescription.Text) ? null : tbDescription.Text.Trim();

                if (isNewProduct)
                {
                    if (db.ShoeModels.Any(p => p.Article == _currentProduct.Article))
                    {
                        tblError.Text = "Товар с таким артикулом уже существует.";
                        return;
                    }

                    db.ShoeModels.Add(_currentProduct);
                    MessageBox.Show("Товар успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else 
                {
                    if (db.ShoeModels.Any(p => p.Article == _currentProduct.Article && p.ModelID != _currentProduct.ModelID))
                    {
                        tblError.Text = "Товар с таким артикулом уже существует.";
                        return;
                    }
                    MessageBox.Show("Данные товара успешно обновлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                db.SaveChanges();
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

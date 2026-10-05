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

namespace Mufteev_Shoe
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        public ProductPage()
        {
            InitializeComponent();

            // 1. Загружаем список производителей для выпадающего списка фильтрации
            var allManufacturers = MufteevShoeDBEntities.GetContext().manufacturers.ToList();

            // Добавляем в начало списка строку "Все производители", чтобы можно было сбросить фильтр
            allManufacturers.Insert(0, new manufacturers { manufacturer_name = "Все производители" });
            ComboFilter.ItemsSource = allManufacturers;

            // Устанавливаем начальные значения для комбобоксов
            ComboSort.SelectedIndex = 0;
            ComboFilter.SelectedIndex = 0;

            // 2. Вызываем метод первоначального отображения товаров
            UpdateProducts();

            // Принудительно делаем кнопку добавления видимой, чтобы отладить лабу
            BtnAddProduct.Visibility = System.Windows.Visibility.Visible;

            if (Manager.CurrentUser == null)
            {
                // Если вошел Гость — полностью скрываем кнопку перехода в корзину в шапке
                BtnGoToCart.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Если вошел авторизованный клиент/сотрудник — кнопка корзины видна
                BtnGoToCart.Visibility = Visibility.Visible;
            }
        }

        private void UpdateProducts()
        {
            // Получаем актуальный список всех товаров из БД
            var currentProducts = MufteevShoeDBEntities.GetContext().products.ToList();

            // --- 1. ФИЛЬТРАЦИЯ ---
            if (ComboFilter.SelectedIndex > 0)
            {
                var selectedManufacturer = ComboFilter.SelectedItem as manufacturers;
                if (selectedManufacturer != null)
                {
                    currentProducts = currentProducts.Where(p => p.manufacturer_id == selectedManufacturer.id).ToList();
                }
            }

            // --- 2. ПОИСК ---
            if (!string.IsNullOrWhiteSpace(TBoxSearch.Text))
            {
                string searchText = TBoxSearch.Text.ToLower();
                currentProducts = currentProducts.Where(p => p.title.ToLower().Contains(searchText)).ToList();
            }

            // --- 3. СОРТИРОВКА ---
            if (ComboSort.SelectedIndex == 1)
            {
                currentProducts = currentProducts.OrderBy(p => p.price).ToList();
            }
            else if (ComboSort.SelectedIndex == 2)
            {
                currentProducts = currentProducts.OrderByDescending(p => p.price).ToList();
            }

            // Сбрасываем старую привязку и принудительно перерисовываем актуальные остатки на экране
            ProductListView.ItemsSource = null;
            ProductListView.ItemsSource = currentProducts;
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void ComboFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }

        // Кнопка добавления нового товара
        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage(null));
        }

        // Кнопка редактирования существующего товара
        private void BtnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            var currentProduct = (sender as Button).DataContext as products;
            Manager.MainFrame.Navigate(new AddEditPage(currentProduct));
        }

        // Автоматическое обновление списка при возврате на эту страницу
        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                MufteevShoeDBEntities.GetContext().ChangeTracker.Entries().ToList().ForEach(p => p.Reload());
                UpdateProducts();
            }
        }

        // НОВОЕ: Добавление выбранного размера обуви в корзину памяти приложения
        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var currentProduct = button.DataContext as products;

            var parentStackPanel = button.Parent as StackPanel;
            var comboSizes = parentStackPanel.Children.OfType<ComboBox>().FirstOrDefault();

            if (comboSizes == null || comboSizes.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите размер обуви перед добавлением в корзину!",
                                "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedStock = comboSizes.SelectedItem as stock_items;
            var targetSize = selectedStock.sizes;

            var existingItem = Manager.SelectedItemsInCart.FirstOrDefault(item =>
                item.Product.id == currentProduct.id && item.SelectedSize.id == targetSize.id);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                Manager.SelectedItemsInCart.Add(new Manager.CartItem
                {
                    Product = currentProduct,
                    SelectedSize = targetSize,
                    Quantity = 1
                });
            }

            MessageBox.Show($"Товар «{currentProduct.title}» ({targetSize.size_value} размер) добавлен в корзину!",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // НОВОЕ: Кнопка перехода на новую страницу корзины
        private void BtnGoToCart_Click(object sender, RoutedEventArgs e)
        {
            if (Manager.SelectedItemsInCart.Count == 0)
            {
                MessageBox.Show("Ваша корзина пуста! Добавьте товар из каталога.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Manager.MainFrame.Navigate(new CartPage());
        }

        private void BtnAddToCart_Loaded(object sender, RoutedEventArgs e)
        {
            Button btnCart = sender as Button;
            // Если в системе Гость — прячем кнопки добавления в корзину во всем каталоге
            if (Manager.CurrentUser == null)
            {
                btnCart.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnCart.Visibility = Visibility.Visible;
            }
        }
    }
}

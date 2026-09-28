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
        }

        private void UpdateProducts()
        {
            // Получаем актуальный список всех товаров из БД
            var currentProducts = MufteevShoeDBEntities.GetContext().products.ToList();

            // --- 1. ФИЛЬТРАЦИЯ ---
            if (ComboFilter.SelectedIndex > 0)
            {
                // Приводим выбранный элемент к типу нашей таблицы производителей
                var selectedManufacturer = ComboFilter.SelectedItem as manufacturers;
                if (selectedManufacturer != null)
                {
                    // Оставляем только те товары, у которых ID производителя совпадает с выбранным
                    currentProducts = currentProducts.Where(p => p.manufacturer_id == selectedManufacturer.id).ToList();
                }
            }

            // --- 2. ПОИСК ---
            if (!string.IsNullOrWhiteSpace(TBoxSearch.Text))
            {
                // Переводим текст в нижний регистр для поиска без учета регистра букв
                string searchText = TBoxSearch.Text.ToLower();
                currentProducts = currentProducts.Where(p => p.title.ToLower().Contains(searchText)).ToList();
            }

            // --- 3. СОРТИРОВКА ---
            if (ComboSort.SelectedIndex == 1)
            {
                // По возрастанию цены
                currentProducts = currentProducts.OrderBy(p => p.price).ToList();
            }
            else if (ComboSort.SelectedIndex == 2)
            {
                // По убыванию цены
                currentProducts = currentProducts.OrderByDescending(p => p.price).ToList();
            }

            // Отдаем отфильтрованный и отсортированный список в ListView
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
    }
}

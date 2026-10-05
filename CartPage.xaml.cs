using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Mufteev_Shoe
{
    public partial class CartPage : Page
    {
        public CartPage()
        {
            InitializeComponent();
        }

        // Метод для расчета стоимости корзины и обновления таблицы
        private void UpdateCartInfo()
        {
            DGCart.ItemsSource = null;
            DGCart.ItemsSource = Manager.SelectedItemsInCart;

            decimal total = Manager.SelectedItemsInCart.Sum(item => item.Product.price * item.Quantity);
            TBlockTotalSum.Text = $"Итого к оплате: {total:N0} руб.";
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                UpdateCartInfo();
            }
        }

        // Фиксация покупки в таблицах СУБД (orders, order_details) и вычитание остатков из stock_items
        private void BtnConfirmOrder_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            // 1. Проверка лимитов наличия пар на складе магазина перед оформлением чека
            foreach (var item in Manager.SelectedItemsInCart)
            {
                if (item.Quantity <= 0)
                {
                    errors.AppendLine($"Количество для товара «{item.Product.title}» должно быть больше 0!");
                    continue;
                }

                var stock = MufteevShoeDBEntities.GetContext().stock_items.FirstOrDefault(s =>
                    s.product_id == item.Product.id && s.size_id == item.SelectedSize.id);

                if (stock == null || stock.quantity < item.Quantity)
                {
                    int available = stock != null ? stock.quantity : 0;
                    errors.AppendLine($"Недостаточно товара «{item.Product.title}» ({item.SelectedSize.size_value} размер)! В наличии: {available} шт.");
                }
            }

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка оформления", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Создание записи в главной таблице orders
            orders newOrder = new orders();
            newOrder.order_date = DateTime.Now;
            newOrder.total_sum = Manager.SelectedItemsInCart.Sum(item => item.Product.price * item.Quantity);
            newOrder.user_id = Manager.CurrentUser.id;

            MufteevShoeDBEntities.GetContext().orders.Add(newOrder);

            // 3. Создание строк состава чека в order_details и автоматическое минусование остатков склада
            try
            {
                MufteevShoeDBEntities.GetContext().SaveChanges();

                foreach (var item in Manager.SelectedItemsInCart)
                {
                    var stock = MufteevShoeDBEntities.GetContext().stock_items.FirstOrDefault(s =>
                        s.product_id == item.Product.id && s.size_id == item.SelectedSize.id);

                    order_details detail = new order_details();
                    detail.order_id = newOrder.id;
                    detail.product_item_id = stock.id;
                    detail.quantity = item.Quantity;

                    MufteevShoeDBEntities.GetContext().order_details.Add(detail);

                    // МИНУСУЕМ остаток со склада базы данных
                    stock.quantity -= item.Quantity;
                }

                MufteevShoeDBEntities.GetContext().SaveChanges();

                MessageBox.Show($"Заказ №{newOrder.id} успешно оформлен! Складские остатки обновлены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                Manager.SelectedItemsInCart.Clear();
                Manager.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка СУБД: {ex.Message}", "Ошибка записи заказа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

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
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        public AuthPage()
        {
            InitializeComponent();
        }

        private void BtnGuest_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Вы вошли в систему как неавторизованный пользователь. Возможности оформления заказов будут ограничены.", "Вход гостя", MessageBoxButton.OK, MessageBoxImage.Information);

            // Переходим в каталог товаров
            Manager.MainFrame.Navigate(new ProductPage());
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string inputLogin = TBoxLogin.Text.Trim();

            if (string.IsNullOrEmpty(inputLogin))
            {
                MessageBox.Show("Пожалуйста, введите логин для входа!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Ищем пользователя в базе данных по введенному логину
            // (В зависимости от генерации модели поле может называться login или латиницей, укажите ваше)
            var user = MufteevShoeDBEntities.GetContext().users.FirstOrDefault(u => u.login == inputLogin);

            if (user != null)
            {
                MessageBox.Show($"Добро пожаловать, {user.first_name} {user.last_name}!", "Успешный вход", MessageBoxButton.OK, MessageBoxImage.Information);

                // Переходим на страницу каталога товаров
                Manager.MainFrame.Navigate(new ProductPage());
            }
            else
            {
                MessageBox.Show("Пользователь с таким логином не найден в системе!", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

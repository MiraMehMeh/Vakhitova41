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

namespace Vakhitova41
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        public ProductPage()
        {
            InitializeComponent();
            FIOTB.Text = "Гость";
            RoleTB.Text = "Гость";

            UpdateProducts();
        }
        public ProductPage(User user)
        {
            InitializeComponent();

            // добавляем строки
                // загрузить в список из бд
            var currentProducts = Vakhitova41Entities.GetContext().Product.ToList();
            // связать с листвью
            ProductListView.ItemsSource = currentProducts;
            // добавили строки

            ComboType.SelectedIndex = 0;

            // вызываем метод
            UpdateProducts();

            // FIOTB - текстбокс для отображ ФИО
            FIOTB.Text = user.UserSurname + " " + user.UserName + " " + user.UserPatronymic;

            switch (user.UserRole)
            {
                case 1:
                    // RoleTB - текстбокс для отобр роли
                    RoleTB.Text = "Администратор"; break;

                case 2:
                    RoleTB.Text = "Менеджер"; break;

                case 3:
                    RoleTB.Text = "Клиент"; break;
            }
        }

        private void UpdateItemsCount()
        {
            try
            {
                int displayedCount = ProductListView.Items.Count;
                int totalCount = Vakhitova41Entities.GetContext().Product.Count();

                TBlockCount.Text = $"кол-во {displayedCount} из {totalCount}";
            }
            catch (Exception ex)
            {
                TBlockCount.Text = "кол-во 0 из 0";
            }
        }

        private void UpdateProducts()
        {
            // берем из бд данные таблицы Продукт
            var currentProducts = Vakhitova41Entities.GetContext().Product.ToList();

            // прописываем фильтрацию по условию задания
            if (ComboType.SelectedIndex == 0)
            {
                currentProducts = currentProducts.Where(p => (Convert.ToDouble(p.ProductDiscountAmount) >= 0 && Convert.ToDouble(p.ProductDiscountAmount) <= 100)).ToList();
            }

            if (ComboType.SelectedIndex == 1)
            {
                currentProducts = currentProducts.Where(p => (Convert.ToDouble(p.ProductDiscountAmount) >= 0 && Convert.ToDouble(p.ProductDiscountAmount) <= 9.99)).ToList();
            }

            if (ComboType.SelectedIndex == 2)
            {
                currentProducts = currentProducts.Where(p => (Convert.ToDouble(p.ProductDiscountAmount) >= 10 && Convert.ToDouble(p.ProductDiscountAmount) <= 14.99)).ToList();
            }

            if (ComboType.SelectedIndex == 3)
            {
                currentProducts = currentProducts.Where(p => (Convert.ToDouble(p.ProductDiscountAmount) >= 15 && Convert.ToDouble(p.ProductDiscountAmount) <= 100)).ToList();
            }


            // реализуем поиск данных в листвью при вводе текста в окно поиска
            currentProducts = currentProducts.Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

            // для отображения итогов фильтра и поиска в листвью
            ProductListView.ItemsSource = currentProducts.ToList();

            if (RButtonDown.IsChecked.Value)
            {
                // для отображения итогов фильтра и поиска по убыванию
                ProductListView.ItemsSource = currentProducts.OrderByDescending(p => p.ProductCost).ToList();
            }

            if (RButtonUp.IsChecked.Value)
            {
                // для отображения итогов фильтра и поиска по возрастанию
                ProductListView.ItemsSource = currentProducts.OrderBy(p => p.ProductCost).ToList();
            }

            UpdateItemsCount();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void RButtonUp_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProducts();
        }

        private void RButtonDown_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProducts();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }
    }
}

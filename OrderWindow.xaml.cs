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
using System.Windows.Shapes;

namespace Vakhitova41
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        private List<OrderProduct> selectedOrderProducts;
        private List<Product> selectedProducts;
        private User currentUser;

        public OrderWindow(List<OrderProduct> selectedOrderProducts,
                          List<Product> selectedProducts,
                          User user)
        {
            InitializeComponent();

            this.selectedOrderProducts = selectedOrderProducts;
            this.selectedProducts = selectedProducts;
            this.currentUser = user;

            InitializeWindow();
        }

        private void InitializeWindow()
        {
            // Устанавливаем ФИО клиента
            if (currentUser != null)
            {
                ClientTB.Text = $"{currentUser.UserSurname} {currentUser.UserName} {currentUser.UserPatronymic}";
            }
            else
            {
                ClientTB.Text = "Гость";
            }

            // Устанавливаем дату заказа (сегодняшний день)
            OrderDatePicker.SelectedDate = DateTime.Now;
            OrderDatePicker.Opacity = 1;
            OrderDatePicker.Foreground = Brushes.Black;


            // УСТАНАВЛИВАЕМ НОМЕР ЗАКАЗА (MAX(OrderID) + 1)
            OrderIDTB.Text = GetNextOrderID().ToString();

            // Загружаем пункты выдачи
            var pickupPoints = Vakhitova41Entities.GetContext().PickupPoint.ToList();
            PickupCombo.ItemsSource = pickupPoints;

            // Для каждого продукта устанавливаем временное свойство Quantity
            // на основе Amount из OrderProduct
            foreach (Product p in selectedProducts)
            {
                var orderProduct = selectedOrderProducts
                    .FirstOrDefault(op => op.ProductArticleNumber == p.ProductArticleNumber);

                if (orderProduct != null)
                {
                    p.Quantity = orderProduct.Amount; // Временное свойство
                }
                else
                {
                    p.Quantity = 1; // По умолчанию
                }
            }

            // Устанавливаем список товаров
            ShoeListView.ItemsSource = selectedProducts;

            // Рассчитываем дату доставки
            CalculateDeliveryDate();

            // Рассчитываем суммы
            CalculateTotals();
        }

        private void CalculateDeliveryDate()
        {
            if (selectedProducts == null || selectedProducts.Count == 0)
            {
                DeliveryDatePicker.Text = "";
                return;
            }

            // Проверяем, достаточно ли товара на складе для каждого заказанного товара
            bool hasEnoughStock = true;

            foreach (var product in selectedProducts)
            {
                // Получаем количество заказанных единиц
                var orderProduct = selectedOrderProducts
                    .FirstOrDefault(op => op.ProductArticleNumber == product.ProductArticleNumber);

                int orderedQuantity = orderProduct?.Amount ?? product.Quantity;

                // Если на складе меньше, чем заказано, ИЛИ меньше 3 единиц
                if (product.ProductQuantityInStock < orderedQuantity || product.ProductQuantityInStock < 3)
                {
                    hasEnoughStock = false;
                    break;
                }
            }

            int deliveryDays;

            if (hasEnoughStock)
            {
                deliveryDays = 3; // Все товары в достаточном количестве (≥3 и ≥заказанного)
            }
            else
            {
                deliveryDays = 6; // Не хватает товара или менее 3 единиц на складе
            }

            DateTime deliveryDate = DateTime.Now.AddDays(deliveryDays);

            // Устанавливаем дату доставки
            DeliveryDatePicker.Text = deliveryDate.ToString("dd.MM.yyyy");
            DeliveryDatePicker.Opacity = 1;
            DeliveryDatePicker.Foreground = Brushes.Black;
        }

        private int GetNextOrderCode()
        {
            try
            {
                var context = Vakhitova41Entities.GetContext();

                // Получаем максимальный OrderCode из БД
                var maxOrderCode = context.Order.Max(o => (int?)o.OrderCode) ?? 0;

                // Прибавляем 1 к существующему максимальному
                return maxOrderCode + 1;
            }
            catch (Exception)
            {
                // Если ошибка (например, таблица пустая), начинаем с 1
                return 1;
            }
        }

        private int GetNextOrderID()
        {
            try
            {
                var context = Vakhitova41Entities.GetContext();

                // Получаем максимальный OrderID из БД
                var maxOrderID = context.Order.Max(o => (int?)o.OrderID) ?? 0;

                // Прибавляем 1 к существующему максимальному
                return maxOrderID + 1;
            }
            catch (Exception)
            {
                // Если ошибка (например, таблица пустая), начинаем с 1
                return 1;
            }
        }

        private void CalculateTotals()
        {
            decimal totalSum = 0;
            decimal discountSum = 0;

            foreach (var product in selectedProducts)
            {
                // Убираем оператор ?? для не-nullable типов
                decimal price = product.ProductCost; // Просто берем значение
                decimal discountPercent = product.ProductDiscountAmount; // Просто берем значение
                int quantity = product.Quantity; // Временное свойство

                decimal itemTotal = price * quantity;
                decimal itemDiscount = itemTotal * discountPercent / 100;

                totalSum += itemTotal;
                discountSum += itemDiscount;
            }

            decimal finalSum = totalSum - discountSum;

            // Обновляем текстовые поля (если они есть в XAML)
            if (TotalSumText != null)
                TotalSumText.Text = $"Сумма: {totalSum:F2}₽";

            if (DiscountText != null)
                DiscountText.Text = $"Скидка: {discountSum:F2}₽";

            if (FinalSumText != null)
                FinalSumText.Text = $"Итого: {finalSum:F2}₽";

            ShoeListView.Items.Refresh();
        }

        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            var prod = (sender as Button).DataContext as Product;
            prod.Quantity++; // Временное свойство

            // Обновляем Amount в OrderProduct
            var selectedOP = selectedOrderProducts.FirstOrDefault(p =>
                p.ProductArticleNumber == prod.ProductArticleNumber);

            if (selectedOP != null)
            {
                selectedOP.Amount = prod.Quantity;
            }

            CalculateTotals();

            // ПЕРЕСЧИТЫВАЕМ дату доставки при изменении количества
            // (потому что могло превысить доступное количество на складе)
            CalculateDeliveryDate();
        }

        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            var prod = (sender as Button).DataContext as Product;

            if (prod.Quantity > 1)
            {
                prod.Quantity--;

                var selectedOP = selectedOrderProducts.FirstOrDefault(p =>
                    p.ProductArticleNumber == prod.ProductArticleNumber);

                if (selectedOP != null)
                {
                    selectedOP.Amount = prod.Quantity;
                }

                CalculateTotals();

                // ПЕРЕСЧИТЫВАЕМ дату доставки при изменении количества
                // (потому что могло превысить доступное количество на складе)
                CalculateDeliveryDate();
            }
            else
            {
                // Если количество становится 0, удаляем товар
                DeleteBtn_Click(sender, e);
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var prod = (sender as Button).DataContext as Product;

            // Удаляем из списков
            var productToRemove = selectedProducts.FirstOrDefault(p =>
                p.ProductArticleNumber == prod.ProductArticleNumber);
            if (productToRemove != null)
                selectedProducts.Remove(productToRemove);

            var orderProductToRemove = selectedOrderProducts.FirstOrDefault(p =>
                p.ProductArticleNumber == prod.ProductArticleNumber);
            if (orderProductToRemove != null)
                selectedOrderProducts.Remove(orderProductToRemove);

            // Обновляем
            ShoeListView.ItemsSource = null;
            ShoeListView.ItemsSource = selectedProducts;
            CalculateTotals();
            // Пересчитываем дату доставки при изменении состава заказа
            CalculateDeliveryDate();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка выбора пункта выдачи
                if (PickupCombo.SelectedItem == null)
                {
                    MessageBox.Show("Выберите пункт выдачи!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var context = Vakhitova41Entities.GetContext();
                var selectedPickup = PickupCombo.SelectedItem as PickupPoint;

                // Вычисляем следующий OrderCode
                int nextOrderCode = GetNextOrderCode();

                // Создаем новый заказ
                var newOrder = new Order
                {
                    OrderDate = OrderDatePicker.SelectedDate ?? DateTime.Now,
                    OrderDeliveryDate = DateTime.ParseExact(DeliveryDatePicker.Text, "dd.MM.yyyy", null),
                    OrderCode = nextOrderCode,
                    OrderPickupPoint = selectedPickup.PickupPointId,
                    OrderStatus = "Новый"
                };

                // Если пользователь авторизован
                if (currentUser != null)
                {
                    newOrder.OrderClient = currentUser.UserID;
                }

                context.Order.Add(newOrder);
                context.SaveChanges();

                // Сохраняем товары заказа
                foreach (var orderProduct in selectedOrderProducts)
                {
                    if (orderProduct.Amount > 0) // Сохраняем только если количество > 0
                    {
                        orderProduct.OrderID = newOrder.OrderID;
                        context.OrderProduct.Add(orderProduct);
                    }
                }

                context.SaveChanges();

                MessageBox.Show($"Заказ №{newOrder.OrderID} успешно сохранен!\n" +
                              $"Статус: {newOrder.OrderStatus}\n" +
                              $"Дата доставки: {newOrder.OrderDeliveryDate:dd.MM.yyyy}",
                              "Заказ оформлен",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения заказа: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }
    }
}

using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace BookstoreWPF
{
    public partial class CartWindow : Window
    {
        private MainWindow mainWindow;

        public CartWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            LoadCartItems();
        }

        private void LoadCartItems()
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = @"
                    SELECT b.title AS 'Название', b.price AS 'Цена', ci.quantity AS 'Количество'
                    FROM cart c
                    JOIN cart_items ci ON c.cart_id = ci.cart_id
                    JOIN books b ON ci.book_id = b.book_id
                    WHERE c.user_id = @userId";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@userId", MainWindow.CurrentUserId);
                DataTable dt = new DataTable();
                da.Fill(dt);
                CartItemsGrid.ItemsSource = dt.DefaultView;

                // Подсчет общей суммы
                decimal totalAmount = 0;
                foreach (DataRow row in dt.Rows)
                {
                    totalAmount += (decimal)row["Цена"] * (int)row["Количество"];
                }
                TotalAmountText.Text = $"Общая сумма: {totalAmount:F2} руб.";
            }
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string cartIdQuery = "SELECT cart_id FROM cart WHERE user_id = @userId";
                SqlCommand cartIdCmd = new SqlCommand(cartIdQuery, conn);
                cartIdCmd.Parameters.AddWithValue("@userId", MainWindow.CurrentUserId);
                int cartId = (int)cartIdCmd.ExecuteScalar();

                // Подсчет общей суммы
                string totalQuery = "SELECT SUM(b.price * ci.quantity) FROM cart_items ci JOIN books b ON ci.book_id = b.book_id WHERE ci.cart_id = @cartId";
                SqlCommand totalCmd = new SqlCommand(totalQuery, conn);
                totalCmd.Parameters.AddWithValue("@cartId", cartId);
                object totalResult = totalCmd.ExecuteScalar();
                decimal totalAmount = totalResult != DBNull.Value ? (decimal)totalResult : 0;

                // Создание заказа
                string orderQuery = "INSERT INTO orders (user_id, total_amount, order_status, created_at) VALUES (@userId, @totalAmount, 'Pending', GETDATE())";
                SqlCommand orderCmd = new SqlCommand(orderQuery, conn);
                orderCmd.Parameters.AddWithValue("@userId", MainWindow.CurrentUserId);
                orderCmd.Parameters.AddWithValue("@totalAmount", totalAmount);
                orderCmd.ExecuteNonQuery();

                // Очистка корзины
                string clearQuery = "DELETE FROM cart_items WHERE cart_id = @cartId";
                SqlCommand clearCmd = new SqlCommand(clearQuery, conn);
                clearCmd.Parameters.AddWithValue("@cartId", cartId);
                clearCmd.ExecuteNonQuery();

                MessageBox.Show("Заказ успешно оформлен!");
                mainWindow.RefreshOrders(); // Обновляем список заказов в MainWindow
                LoadCartItems(); // Обновляем содержимое корзины в окне
                Close();
            }
        }
    }
}
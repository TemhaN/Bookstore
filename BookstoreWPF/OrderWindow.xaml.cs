using System.Data.SqlClient;
using System.Windows;

namespace BookstoreWPF
{
    public partial class OrderWindow : Window
    {
        public OrderWindow()
        {
            InitializeComponent();
        }

        private void PlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    string totalQuery = @"SELECT SUM(b.price * ci.quantity) 
                                        FROM cart_items ci 
                                        JOIN books b ON ci.book_id = b.book_id 
                                        JOIN cart c ON ci.cart_id = c.cart_id 
                                        WHERE c.user_id = @userId";
                    SqlCommand totalCmd = new SqlCommand(totalQuery, conn, transaction);
                    totalCmd.Parameters.AddWithValue("@userId", MainWindow.CurrentUserId);
                    decimal total = (decimal)totalCmd.ExecuteScalar();

                    string orderQuery = "INSERT INTO orders (user_id, total_amount, order_status) VALUES (@userId, @total, 'pending'); SELECT SCOPE_IDENTITY();";
                    SqlCommand orderCmd = new SqlCommand(orderQuery, conn, transaction);
                    orderCmd.Parameters.AddWithValue("@userId", MainWindow.CurrentUserId);
                    orderCmd.Parameters.AddWithValue("@total", total);
                    int orderId = (int)(decimal)orderCmd.ExecuteScalar();

                    string itemsQuery = @"INSERT INTO order_items (order_id, book_id, quantity, price_at_time)
                                        SELECT @orderId, ci.book_id, ci.quantity, b.price 
                                        FROM cart_items ci 
                                        JOIN books b ON ci.book_id = b.book_id 
                                        JOIN cart c ON ci.cart_id = c.cart_id 
                                        WHERE c.user_id = @userId";
                    SqlCommand itemsCmd = new SqlCommand(itemsQuery, conn, transaction);
                    itemsCmd.Parameters.AddWithValue("@orderId", orderId);
                    itemsCmd.Parameters.AddWithValue("@userId", MainWindow.CurrentUserId);
                    itemsCmd.ExecuteNonQuery();

                    string clearCartQuery = @"DELETE FROM cart_items WHERE cart_id IN (SELECT cart_id FROM cart WHERE user_id = @userId)";
                    SqlCommand clearCmd = new SqlCommand(clearCartQuery, conn, transaction);
                    clearCmd.Parameters.AddWithValue("@userId", MainWindow.CurrentUserId);
                    clearCmd.ExecuteNonQuery();

                    transaction.Commit();
                    MessageBox.Show("Заказ успешно оформлен!");
                    Close();
                }
                catch
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка при оформлении заказа.");
                }
            }
        }
    }
}
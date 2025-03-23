using System.Data.SqlClient;
using System.Windows;

namespace BookstoreWPF
{
    public partial class BookDetailsWindow : Window
    {
        private int bookId;
        private MainWindow mainWindow;

        public BookDetailsWindow(int bookId, MainWindow mainWindow)
        {
            InitializeComponent();
            this.bookId = bookId;
            this.mainWindow = mainWindow;
            LoadBookDetails();
        }

        private void LoadBookDetails()
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = "SELECT title, author, price, description, short_info, stock_quantity FROM books WHERE book_id = @bookId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@bookId", bookId);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    TitleText.Text = reader["title"].ToString();
                    AuthorText.Text = $"Автор: {reader["author"]}";
                    PriceText.Text = $"Цена: {reader["price"]:F2} руб.";
                    DescriptionText.Text = $"Описание: {reader["description"]}";
                    ShortInfoText.Text = $"Краткая информация: {reader["short_info"]}";
                    StockText.Text = $"В наличии: {reader["stock_quantity"]} шт.";
                }
            }
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.CurrentUserId == -1)
            {
                MessageBox.Show("Пожалуйста, войдите в систему.");
                return;
            }

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string cartQuery = "INSERT INTO cart (user_id) SELECT @userId WHERE NOT EXISTS (SELECT 1 FROM cart WHERE user_id = @userId)";
                SqlCommand cartCmd = new SqlCommand(cartQuery, conn);
                cartCmd.Parameters.AddWithValue("@userId", MainWindow.CurrentUserId);
                cartCmd.ExecuteNonQuery();

                string cartIdQuery = "SELECT cart_id FROM cart WHERE user_id = @userId";
                SqlCommand cartIdCmd = new SqlCommand(cartIdQuery, conn);
                cartIdCmd.Parameters.AddWithValue("@userId", MainWindow.CurrentUserId);
                int cartId = (int)cartIdCmd.ExecuteScalar();

                string itemQuery = "INSERT INTO cart_items (cart_id, book_id, quantity) VALUES (@cartId, @bookId, 1)";
                SqlCommand itemCmd = new SqlCommand(itemQuery, conn);
                itemCmd.Parameters.AddWithValue("@cartId", cartId);
                itemCmd.Parameters.AddWithValue("@bookId", bookId);
                itemCmd.ExecuteNonQuery();

                MessageBox.Show("Книга добавлена в корзину!");

                // Обновляем корзину в главном окне
                if (mainWindow != null)
                {
                    mainWindow.RefreshCart();
                }
                else
                {
                    MessageBox.Show("Ошибка: не удалось обновить корзину, ссылка на главное окно отсутствует.");
                }
            }
        }
    }
}
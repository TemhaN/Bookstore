using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace BookstoreWPF
{
    public partial class BooksWindow : Window
    {
        private int selectedBookId;

        public BooksWindow()
        {
            InitializeComponent();
            LoadBooks();
        }

        private void LoadBooks()
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter("SELECT book_id, title, author, price, short_info FROM books", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                BooksDataGrid.ItemsSource = dt.DefaultView;
            }
        }

        private void BooksDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (BooksDataGrid.SelectedItem != null && BooksDataGrid.SelectedItem is DataRowView row)
            {
                selectedBookId = (int)row["book_id"];
            }
            else
            {
                selectedBookId = -1;
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
                itemCmd.Parameters.AddWithValue("@bookId", selectedBookId);
                itemCmd.ExecuteNonQuery();

                MessageBox.Show("Книга добавлена в корзину!");
            }
        }
    }
}
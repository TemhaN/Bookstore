using System.Data;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using System.Data.SqlClient;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using Microsoft.Win32;

namespace BookstoreWPF
{
    public partial class MainWindow : Window
    {
        public static int CurrentUserId = -1;

        public MainWindow()
        {
            InitializeComponent();
            LoadBooks();
            LoadOrders();
            LoadCart();
            UpdateAccountInfo();
        }

        public void RefreshCart()
        {
            LoadCart();
        }

        private void UpdateAccountInfo()
        {
            if (CurrentUserId != -1)
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT username FROM users WHERE user_id = @userId", conn);
                    cmd.Parameters.AddWithValue("@userId", CurrentUserId);
                    AccountText.Text = $"Добро пожаловать, {cmd.ExecuteScalar()?.ToString()}";
                }
            }
        }

        private void LoadBooks(string search = "")
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string query = "SELECT book_id, title AS 'Название', author AS 'Автор', price AS 'Цена', short_info AS 'Описание' FROM books";
                if (!string.IsNullOrEmpty(search))
                    query += " WHERE title LIKE @search OR author LIKE @search";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                if (!string.IsNullOrEmpty(search))
                    da.SelectCommand.Parameters.AddWithValue("@search", $"%{search}%");
                DataTable dt = new DataTable();
                da.Fill(dt);
                BooksItemsControl.ItemsSource = dt.DefaultView;
            }
        }

        private void LoadOrders()
        {
            if (CurrentUserId != -1)
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter("SELECT order_id AS 'Номер', total_amount AS 'Сумма', order_status AS 'Статус', created_at AS 'Дата' FROM orders WHERE user_id = @userId", conn);
                    da.SelectCommand.Parameters.AddWithValue("@userId", CurrentUserId);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    OrdersItemsControl.ItemsSource = dt.DefaultView; // Используем ItemsControl
                }
            }
            else
            {
                OrdersItemsControl.ItemsSource = null;
            }
        }

        private void LoadCart()
        {
            if (CurrentUserId != -1)
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string query = @"
                        SELECT b.book_id, b.title AS 'Название', b.price AS 'Цена'
                        FROM cart c
                        JOIN cart_items ci ON c.cart_id = ci.cart_id
                        JOIN books b ON ci.book_id = b.book_id
                        WHERE c.user_id = @userId";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.SelectCommand.Parameters.AddWithValue("@userId", CurrentUserId);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    CartItemsControl.ItemsSource = dt.DefaultView; // Используем ItemsControl
                }
            }
            else
            {
                CartItemsControl.ItemsSource = null;
            }
        }

        private void Cart_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUserId == -1)
            {
                MessageBox.Show("Пожалуйста, войдите в систему.");
                return;
            }
            new CartWindow(this).ShowDialog();
            LoadOrders();
            LoadCart();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadBooks(SearchTextBox.Text);
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUserId = -1;
            new LoginRegisterWindow().Show();
            Close();
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUserId == -1)
            {
                MessageBox.Show("Пожалуйста, войдите в систему.");
                return;
            }

            Button button = sender as Button;
            int bookId = (int)button.Tag;

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string cartQuery = "INSERT INTO cart (user_id) SELECT @userId WHERE NOT EXISTS (SELECT 1 FROM cart WHERE user_id = @userId)";
                SqlCommand cartCmd = new SqlCommand(cartQuery, conn);
                cartCmd.Parameters.AddWithValue("@userId", CurrentUserId);
                cartCmd.ExecuteNonQuery();

                string cartIdQuery = "SELECT cart_id FROM cart WHERE user_id = @userId";
                SqlCommand cartIdCmd = new SqlCommand(cartIdQuery, conn);
                cartIdCmd.Parameters.AddWithValue("@userId", CurrentUserId);
                int cartId = (int)cartIdCmd.ExecuteScalar();

                string itemQuery = "INSERT INTO cart_items (cart_id, book_id, quantity) VALUES (@cartId, @bookId, 1)";
                SqlCommand itemCmd = new SqlCommand(itemQuery, conn);
                itemCmd.Parameters.AddWithValue("@cartId", cartId);
                itemCmd.Parameters.AddWithValue("@bookId", bookId);
                itemCmd.ExecuteNonQuery();

                MessageBox.Show("Книга добавлена в корзину!");
                LoadCart();
            }
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUserId == -1)
            {
                MessageBox.Show("Пожалуйста, войдите в систему.");
                return;
            }

            Button button = sender as Button;
            int bookId = (int)button.Tag;

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string cartIdQuery = "SELECT cart_id FROM cart WHERE user_id = @userId";
                SqlCommand cartIdCmd = new SqlCommand(cartIdQuery, conn);
                cartIdCmd.Parameters.AddWithValue("@userId", CurrentUserId);
                int cartId = (int)cartIdCmd.ExecuteScalar();

                string deleteQuery = "DELETE FROM cart_items WHERE cart_id = @cartId AND book_id = @bookId";
                SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn);
                deleteCmd.Parameters.AddWithValue("@cartId", cartId);
                deleteCmd.Parameters.AddWithValue("@bookId", bookId);
                deleteCmd.ExecuteNonQuery();

                MessageBox.Show("Книга удалена из корзины!");
                LoadCart();
            }
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUserId == -1)
            {
                MessageBox.Show("Пожалуйста, войдите в систему.");
                return;
            }

            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                string cartIdQuery = "SELECT cart_id FROM cart WHERE user_id = @userId";
                SqlCommand cartIdCmd = new SqlCommand(cartIdQuery, conn);
                cartIdCmd.Parameters.AddWithValue("@userId", CurrentUserId);
                object cartIdResult = cartIdCmd.ExecuteScalar();
                if (cartIdResult == null)
                {
                    MessageBox.Show("Корзина пуста! Добавьте товары перед оформлением заказа.");
                    return;
                }
                int cartId = (int)cartIdResult;

                string totalQuery = "SELECT SUM(b.price * ci.quantity) FROM cart_items ci JOIN books b ON ci.book_id = b.book_id WHERE ci.cart_id = @cartId";
                SqlCommand totalCmd = new SqlCommand(totalQuery, conn);
                totalCmd.Parameters.AddWithValue("@cartId", cartId);
                object totalResult = totalCmd.ExecuteScalar();
                if (totalResult == DBNull.Value || (decimal)totalResult == 0)
                {
                    MessageBox.Show("Корзина пуста! Добавьте товары перед оформлением заказа.");
                    return;
                }
                decimal totalAmount = (decimal)totalResult;

                string orderQuery = "INSERT INTO orders (user_id, total_amount, order_status, created_at) VALUES (@userId, @totalAmount, 'Pending', GETDATE())";
                SqlCommand orderCmd = new SqlCommand(orderQuery, conn);
                orderCmd.Parameters.AddWithValue("@userId", CurrentUserId);
                orderCmd.Parameters.AddWithValue("@totalAmount", totalAmount);
                orderCmd.ExecuteNonQuery();

                string clearQuery = "DELETE FROM cart_items WHERE cart_id = @cartId";
                SqlCommand clearCmd = new SqlCommand(clearQuery, conn);
                clearCmd.Parameters.AddWithValue("@cartId", cartId);
                clearCmd.ExecuteNonQuery();

                MessageBox.Show("Заказ успешно оформлен!");
                LoadOrders();
                LoadCart();
            }
        }

        private void BooksItemsControl_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (BooksItemsControl.SelectedItem is DataRowView row)
            {
                int bookId = (int)row["book_id"];
                new BookDetailsWindow(bookId, this).ShowDialog();
            }
        }

        private void ExportOrdersToExcel(object sender, RoutedEventArgs e)
        {
            if (CurrentUserId == -1)
            {
                MessageBox.Show("Пожалуйста, войдите в систему.");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                DefaultExt = "xlsx",
                FileName = "MyOrders.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter("SELECT o.order_id AS 'Номер заказа', u.username AS 'Пользователь', o.total_amount AS 'Сумма', o.order_status AS 'Статус', o.created_at AS 'Дата' FROM orders o JOIN users u ON o.user_id = u.user_id WHERE o.user_id = @userId", conn);
                    da.SelectCommand.Parameters.AddWithValue("@userId", CurrentUserId);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("My Orders");
                        var table = worksheet.Cell(2, 1).InsertTable(dt);
                        worksheet.Row(1).Height = 30;
                        worksheet.Cell(1, 1).Value = "Мои заказы";
                        worksheet.Cell(1, 1).Style.Font.Bold = true;
                        worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#D9C2A7");
                        worksheet.Range(1, 1, 1, dt.Columns.Count).Merge();

                        table.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        table.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        table.ShowAutoFilter = true;
                        worksheet.Columns().AdjustToContents();

                        workbook.SaveAs(saveFileDialog.FileName);
                    }
                }
                MessageBox.Show($"Ваши заказы экспортированы в {saveFileDialog.FileName}");
            }
        }

        private void ExportOrdersToWord(object sender, RoutedEventArgs e)
        {
            if (CurrentUserId == -1)
            {
                MessageBox.Show("Пожалуйста, войдите в систему.");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Word files (*.docx)|*.docx|All files (*.*)|*.*",
                DefaultExt = "docx",
                FileName = "MyOrders.docx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter("SELECT o.order_id, u.username, o.total_amount, o.order_status, o.created_at FROM orders o JOIN users u ON o.user_id = u.user_id WHERE o.user_id = @userId", conn);
                    da.SelectCommand.Parameters.AddWithValue("@userId", CurrentUserId);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(saveFileDialog.FileName, WordprocessingDocumentType.Document))
                    {
                        MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                        mainPart.Document = new Document();
                        Body body = mainPart.Document.AppendChild(new Body());

                        Paragraph header = body.AppendChild(new Paragraph(new ParagraphProperties(
                            new Justification() { Val = JustificationValues.Center }
                        )));
                        Run headerRun = header.AppendChild(new Run());
                        headerRun.AppendChild(new Text("Мои заказы"));
                        headerRun.RunProperties = new RunProperties(new Bold(), new FontSize() { Val = "40" }, new RunFonts() { Ascii = "Georgia" }, new Color() { Val = "805A3B" });

                        foreach (DataRow row in dt.Rows)
                        {
                            Paragraph para = body.AppendChild(new Paragraph(new ParagraphProperties(
                                new SpacingBetweenLines() { After = "200" }
                            )));
                            Run run = para.AppendChild(new Run());
                            run.AppendChild(new Text($"Заказ #{row["order_id"]} - Пользователь: {row["username"]}, Сумма: {row["total_amount"]} руб., Статус: {row["order_status"]}, Дата: {row["created_at"]}"));
                            run.RunProperties = new RunProperties(new FontSize() { Val = "24" }, new RunFonts() { Ascii = "Georgia" });
                        }
                    }
                }
                MessageBox.Show($"Ваши заказы экспортированы в {saveFileDialog.FileName}");
            }
        }

        private void ExportBooksToExcel(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                DefaultExt = "xlsx",
                FileName = "BooksList.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter("SELECT book_id AS 'ID', title AS 'Название', author AS 'Автор', price AS 'Цена', short_info AS 'Описание', stock_quantity AS 'Количество' FROM books", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Books List");
                        var table = worksheet.Cell(2, 1).InsertTable(dt);
                        worksheet.Row(1).Height = 30;
                        worksheet.Cell(1, 1).Value = "Список книг";
                        worksheet.Cell(1, 1).Style.Font.Bold = true;
                        worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#D9C2A7");
                        worksheet.Range(1, 1, 1, dt.Columns.Count).Merge();

                        table.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        table.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        table.ShowAutoFilter = true;
                        worksheet.Columns().AdjustToContents();

                        workbook.SaveAs(saveFileDialog.FileName);
                    }
                }
                MessageBox.Show($"Список книг экспортирован в {saveFileDialog.FileName}");
            }
        }

        private void ExportBooksToWord(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Word files (*.docx)|*.docx|All files (*.*)|*.*",
                DefaultExt = "docx",
                FileName = "BooksList.docx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                using (SqlConnection conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter("SELECT book_id, title, author, price, short_info, stock_quantity FROM books", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(saveFileDialog.FileName, WordprocessingDocumentType.Document))
                    {
                        MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                        mainPart.Document = new Document();
                        Body body = mainPart.Document.AppendChild(new Body());

                        Paragraph header = body.AppendChild(new Paragraph(new ParagraphProperties(
                            new Justification() { Val = JustificationValues.Center }
                        )));
                        Run headerRun = header.AppendChild(new Run());
                        headerRun.AppendChild(new Text("Список книг"));
                        headerRun.RunProperties = new RunProperties(new Bold(), new FontSize() { Val = "40" }, new RunFonts() { Ascii = "Georgia" }, new Color() { Val = "805A3B" });

                        foreach (DataRow row in dt.Rows)
                        {
                            Paragraph para = body.AppendChild(new Paragraph(new ParagraphProperties(
                                new SpacingBetweenLines() { After = "200" }
                            )));
                            Run run = para.AppendChild(new Run());
                            run.AppendChild(new Text($"Книга #{row["book_id"]} - Название: {row["title"]}, Автор: {row["author"]}, Цена: {row["price"]} руб., Описание: {row["short_info"]}, Количество: {row["stock_quantity"]}"));
                            run.RunProperties = new RunProperties(new FontSize() { Val = "24" }, new RunFonts() { Ascii = "Georgia" });
                        }
                    }
                }
                MessageBox.Show($"Список книг экспортирован в {saveFileDialog.FileName}");
            }
        }

        public void RefreshOrders()
        {
            LoadOrders();
        }
    }
}
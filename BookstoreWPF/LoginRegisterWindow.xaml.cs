using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace BookstoreWPF
{
    public partial class LoginRegisterWindow : Window
    {
        public LoginRegisterWindow()
        {
            InitializeComponent();
        }

        private void LoginRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (ActionButton != null)
                ActionButton.Content = "Войти";
            if (EmailLabel != null)
                EmailLabel.Visibility = Visibility.Collapsed;
            if (EmailTextBox != null)
                EmailTextBox.Visibility = Visibility.Collapsed;
        }

        private void RegisterRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (ActionButton != null)
                ActionButton.Content = "Зарегистрироваться";
            if (EmailLabel != null)
                EmailLabel.Visibility = Visibility.Visible;
            if (EmailTextBox != null)
                EmailTextBox.Visibility = Visibility.Visible;
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            using (SqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                if (LoginRadio?.IsChecked == true)
                {
                    string query = "SELECT user_id FROM users WHERE username = @username AND password = @password";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", UsernameTextBox.Text);
                    cmd.Parameters.AddWithValue("@password", PasswordBox.Password);
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        MainWindow.CurrentUserId = (int)result;
                        new MainWindow().Show();
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Неверные данные.");
                    }
                }
                else
                {
                    string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username OR email = @email";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@username", UsernameTextBox.Text);
                    checkCmd.Parameters.AddWithValue("@email", EmailTextBox.Text);
                    if ((int)checkCmd.ExecuteScalar() > 0)
                    {
                        MessageBox.Show("Пользователь с таким именем или email уже существует!");
                        return;
                    }

                    string insertQuery = "INSERT INTO users (username, password, email) VALUES (@username, @password, @email)";
                    SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                    insertCmd.Parameters.AddWithValue("@username", UsernameTextBox.Text);
                    insertCmd.Parameters.AddWithValue("@password", PasswordBox.Password);
                    insertCmd.Parameters.AddWithValue("@email", EmailTextBox.Text);
                    insertCmd.ExecuteNonQuery();

                    MessageBox.Show("Регистрация успешна!");
                    LoginRadio.IsChecked = true;
                }
            }
        }
    }
}
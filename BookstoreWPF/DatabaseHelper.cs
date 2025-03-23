using System.Data.SqlClient;

public class DatabaseHelper
{
    private static string connectionString = "Server=TEMHANLAPTOP\\TDG2022;Database=bookstore;Trusted_Connection=True;";

    public static SqlConnection GetConnection()
    {
        return new SqlConnection(connectionString);
    }
}
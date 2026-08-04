/*
using System;
using System.Data.SQLite;

class Program
{
    static void Main()
    {
        string connStr = @"Data Source=d:\quanlyresort-main\quanlyresort-main\QuanLyResort\ResortDev.db;Version=3;";
        using (var conn = new SQLiteConnection(connStr))
        {
            conn.Open();
            using (var cmd = new SQLiteCommand("SELECT CustomerId, FullName, AvatarUrl FROM Customers", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine("Customer: " + reader["CustomerId"] + " - " + reader["FullName"] + " - Avatar: " + reader["AvatarUrl"]);
                }
            }
            using (var cmd = new SQLiteCommand("SELECT ReviewId, CustomerId, Comment FROM Reviews", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine("Review: " + reader["ReviewId"] + " - CustomerId: " + reader["CustomerId"]);
                }
            }
        }
    }
}
*/


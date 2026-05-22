using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using CertificateApp.Models;

namespace CertificateApp.Services
{
    public class DatabaseService
    {
        private const string ConnectionString = "Data Source=certificates.db";

        public static void InitializeDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            
            // 1. جدول الشهادات والمستفيدين
            var createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Certificates (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Witness1Name TEXT, Witness1Birth TEXT, Witness1Address TEXT, Witness1Card TEXT, Witness1CardDate TEXT,
                    Witness2Name TEXT, Witness2Birth TEXT, Witness2Address TEXT, Witness2Card TEXT, Witness2CardDate TEXT,
                    TargetName TEXT, TargetBirth TEXT, TargetCard TEXT, TargetCardDate TEXT, LatinName TEXT,
                    IssueDate TEXT
                );";
            using var command1 = new SqliteCommand(createTableQuery, connection);
            command1.ExecuteNonQuery();

            // 2. جدول حفظ إعدادات مراجع جهة الإصدار
            var createSettingsTable = @"
                CREATE TABLE IF NOT EXISTS Settings (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Wilaya TEXT, Daira TEXT, Baladia TEXT
                );";
            using var command2 = new SqliteCommand(createSettingsTable, connection);
            command2.ExecuteNonQuery();
        }

        // دالة حفظ وتحديث الإعدادات العامة للبرنامج
        public static void SaveSettings(string wilaya, string daira, string baladia)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var deleteQuery = "DELETE FROM Settings;";
            using (var delCmd = new SqliteCommand(deleteQuery, connection)) delCmd.ExecuteNonQuery();

            var insertQuery = "INSERT INTO Settings (Wilaya, Daira, Baladia) VALUES (@Wilaya, @Daira, @Baladia);";
            using var command = new SqliteCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@Wilaya", wilaya);
            command.Parameters.AddWithValue("@Daira", daira);
            command.Parameters.AddWithValue("@Baladia", baladia);
            command.ExecuteNonQuery();
        }

        // دالة جلب الإعدادات الحالية للواجهة والطباعة
        public static Tuple<string, string, string> LoadSettings()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var query = "SELECT Wilaya, Daira, Baladia FROM Settings LIMIT 1;";
            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();
            
            if (reader.Read())
            {
                return new Tuple<string, string, string>(
                    reader.IsDBNull(0) ? "" : reader.GetString(0),
                    reader.IsDBNull(1) ? "" : reader.GetString(1),
                    reader.IsDBNull(2) ? "" : reader.GetString(2)
                );
            }
            return new Tuple<string, string, string>("", "", "");
        }

        // دالة حفظ شهادة جديدة في قاعدة البيانات (تم تصحيح الحقول هنا لـ .NET 8)
        public static void SaveCertificate(CertificateRecord record)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var insertQuery = @"
                INSERT INTO Certificates (
                    Witness1Name, Witness1Birth, Witness1Address, Witness1Card, Witness1CardDate,
                    Witness2Name, Witness2Birth, Witness2Address, Witness2Card, Witness2CardDate,
                    TargetName, TargetBirth, TargetCard, TargetCardDate, LatinName, IssueDate
                ) VALUES (
                    @W1Name, @W1Birth, @W1Address, @W1Card, @W1CardDate,
                    @W2Name, @W2Birth, @W2Address, @W2Card, @W2CardDate,
                    @TName, @TBirth, @TCard, @TCardDate, @Latin, @IDate
                );";

            using var command = new SqliteCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@W1Name", record.Witness1Name);
            command.Parameters.AddWithValue("@W1Birth", record.Witness1Birth); 
            command.Parameters.AddWithValue("@W1Address", record.Witness1Address);
            command.Parameters.AddWithValue("@W1Card", record.Witness1Card); 
            command.Parameters.AddWithValue("@W1CardDate", record.Witness1CardDate);
            
            command.Parameters.AddWithValue("@W2Name", record.Witness2Name);
            command.Parameters.AddWithValue("@W2Birth", record.Witness2Birth); 
            command.Parameters.AddWithValue("@W2Address", record.Witness2Address);
            command.Parameters.AddWithValue("@W2Card", record.Witness2Card); 
            command.Parameters.AddWithValue("@W2CardDate", record.Witness2CardDate);

            command.Parameters.AddWithValue("@TName", record.TargetName);
            command.Parameters.AddWithValue("@TBirth", record.TargetBirth); 
            command.Parameters.AddWithValue("@TCard", record.TargetCard); 
            command.Parameters.AddWithValue("@TCardDate", record.TargetCardDate);
            command.Parameters.AddWithValue("@Latin", record.LatinName);
            command.Parameters.AddWithValue("@IDate", record.IssueDate);

            command.ExecuteNonQuery();
        }

        // دالة البحث في السجلات
        public static List<CertificateRecord> SearchCertificates(string keyword)
        {
            var list = new List<CertificateRecord>();
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var query = "SELECT * FROM Certificates WHERE TargetName LIKE @Key OR TargetCard LIKE @Key";
            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@Key", $"%{keyword}%");

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new CertificateRecord
                {
                    Id = reader.GetInt32(0),
                    Witness1Name = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Witness1Birth = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Witness1Address = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Witness1Card = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Witness1CardDate = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    Witness2Name = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    Witness2Birth = reader.IsDBNull(7) ? "" : reader.GetString(7),
                    Witness2Address = reader.IsDBNull(8) ? "" : reader.GetString(8),
                    Witness2Card = reader.IsDBNull(9) ? "" : reader.GetString(9),
                    Witness2CardDate = reader.IsDBNull(10) ? "" : reader.GetString(10),
                    TargetName = reader.IsDBNull(11) ? "" : reader.GetString(11),
                    TargetBirth = reader.IsDBNull(12) ? "" : reader.GetString(12),
                    TargetCard = reader.IsDBNull(13) ? "" : reader.GetString(13),
                    TargetCardDate = reader.IsDBNull(14) ? "" : reader.GetString(14),
                    LatinName = reader.IsDBNull(15) ? "" : reader.GetString(15),
                    IssueDate = reader.IsDBNull(16) ? "" : reader.GetString(16)
                });
            }
            return list;
        }
    }
}

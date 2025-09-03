using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Collections.Generic;

namespace RM
{
    public static class DatabaseHelper
    {
        private static string connectionString = @"Data Source=DESKTOP-F4BK7ES\HUZAIFA;Initial Catalog=RM;Integrated Security=True";
        
        public static bool TestConnection()
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    Console.WriteLine($"[DATABASE] Successfully connected to {conn.Database} on {conn.DataSource}");
                    
                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Staff'", conn))
                    {
                        int tableCount = (int)cmd.ExecuteScalar();
                        Console.WriteLine($"[DATABASE] Staff table exists: {tableCount > 0}");
                        
                        if (tableCount == 0)
                        {
                            Console.WriteLine("[DATABASE] Creating Staff table...");
                            string createTable = @"
                                CREATE TABLE Staff (
                                    StaffID INT IDENTITY(1,1) PRIMARY KEY,
                                    Name NVARCHAR(100) NOT NULL,
                                    Username NVARCHAR(50) UNIQUE NOT NULL,
                                    Password NVARCHAR(100) NOT NULL,
                                    Role NVARCHAR(50) NOT NULL
                                )";
                                
                            using (var createCmd = new SqlCommand(createTable, conn))
                            {
                                createCmd.ExecuteNonQuery();
                                Console.WriteLine("[DATABASE] Staff table created successfully");
                                
                                string addAdmin = @"
                                    INSERT INTO Staff (Name, Username, Password, Role)
                                    VALUES ('Admin', 'admin', 'admin123', 'Admin')";
                                    
                                using (var insertCmd = new SqlCommand(addAdmin, conn))
                                {
                                    insertCmd.ExecuteNonQuery();
                                    Console.WriteLine("[DATABASE] Default admin user created");
                                }
                            }
                        }
                    }
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Database connection failed: {ex.Message}";
                Console.WriteLine($"[ERROR] {errorMsg}");
                MessageBox.Show(errorMsg, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool ValidateUser(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND PasswordHash = @Password";
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);
                        
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error validating user: " + ex.Message);
                    return false;
                }
            }
        }

        public static DataTable GetStaff(string search = "")
        {
            DataTable dt = new DataTable();
            try
            {
                var safeConnString = new SqlConnectionStringBuilder(connectionString);
                Console.WriteLine($"[DIAGNOSTIC] Connecting to: {safeConnString.DataSource}/{safeConnString.InitialCatalog}");

                using (SqlConnection testConn = new SqlConnection(connectionString))
                {
                    try
                    {
                        testConn.Open();
                        Console.WriteLine("[DEBUG] Successfully connected to database");

                        string checkTable = @"IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                                       WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Staff')
                                    SELECT 1 ELSE SELECT 0";
                        using (SqlCommand cmd = new SqlCommand(checkTable, testConn))
                        {
                            cmd.CommandTimeout = 30;
                            bool tableExists = (int)cmd.ExecuteScalar() == 1;
                            Console.WriteLine($"[DEBUG] Staff table exists: {tableExists}");

                            if (!tableExists)
                            {
                                string errorMsg = "[ERROR] Staff table does not exist in the database. Please run the database setup script.";
                                Console.WriteLine(errorMsg);
                                MessageBox.Show(errorMsg, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return dt;
                            }
                        }

                        Console.WriteLine("[DEBUG] Staff table structure:");
                        string getColumns = @"SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
                                   FROM INFORMATION_SCHEMA.COLUMNS 
                                   WHERE TABLE_NAME = 'Staff'
                                   ORDER BY ORDINAL_POSITION";
                        using (SqlCommand cmd = new SqlCommand(getColumns, testConn))
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Console.WriteLine($"- {reader["COLUMN_NAME"]} ({reader["DATA_TYPE"]}) Nullable: {reader["IS_NULLABLE"]}");
                            }
                        }

                        string countQuery = "SELECT COUNT(*) FROM Staff";
                        using (SqlCommand cmd = new SqlCommand(countQuery, testConn))
                        {
                            int rowCount = (int)cmd.ExecuteScalar();
                            Console.WriteLine($"[DEBUG] Staff table has {rowCount} rows");
                        }

                        if (!string.IsNullOrEmpty(search))
                        {
                            Console.WriteLine($"[DEBUG] Searching for: {search}");
                            string searchQuery = @"SELECT TOP 5 StaffID AS ID, Name, Username, Role 
                                       FROM Staff 
                                       WHERE Name LIKE @Search 
                                       OR Username LIKE @Search 
                                       OR Role LIKE @Search";
                            Console.WriteLine($"[DEBUG] Executing search query: {searchQuery}");

                            using (SqlCommand cmd = new SqlCommand(searchQuery, testConn))
                            {
                                cmd.Parameters.AddWithValue("@Search", $"%{search}%");
                                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                                {
                                    da.Fill(dt);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("[DEBUG] Fetching all staff records");
                            string query = "SELECT StaffID AS ID, Name, Username, Role FROM Staff ORDER BY Name";
                            Console.WriteLine($"[DEBUG] Executing query: {query}");
                            using (SqlCommand cmd = new SqlCommand(query, testConn))
                            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            {
                                da.Fill(dt);
                            }
                        }

                        Console.WriteLine($"[DEBUG] Retrieved {dt.Rows.Count} rows");
                        Console.WriteLine($"[DIAGNOSTIC] Found {dt.Rows.Count} staff records");

                        if (dt.Rows.Count > 0)
                        {
                            Console.WriteLine("[DIAGNOSTIC] Sample data from Staff table:");
                            for (int i = 0; i < Math.Min(3, dt.Rows.Count); i++)
                            {
                                var row = dt.Rows[i];
                                Console.WriteLine($"  - ID={row["ID"]}, " +
                                                $"Name='{row["Name"]}', " +
                                                $"Username='{row["Username"]}', " +
                                                $"Role='{row["Role"]}'");
                            }
                        }
                        else
                        {
                            Console.WriteLine("[DIAGNOSTIC] No records found in Staff table");

                            Console.WriteLine("[DIAGNOSTIC] Attempting to insert test data...");
                            try
                            {
                                string insertTest = @"
                                IF NOT EXISTS (SELECT 1 FROM Staff WHERE Username = 'testadmin')
                                BEGIN
                                    INSERT INTO Staff (Name, Username, Password, Role)
                                    VALUES ('Test Admin', 'testadmin', 'test123', 'Admin');
                                    PRINT 'Test admin user added';
                                END";

                                using (SqlCommand cmd = new SqlCommand(insertTest, testConn))
                                {
                                    cmd.ExecuteNonQuery();
                                    Console.WriteLine("[DIAGNOSTIC] Test data inserted");

                                    using (SqlDataAdapter da = new SqlDataAdapter("SELECT StaffID, Name, Username, Role FROM Staff", testConn))
                                    {
                                        dt.Clear();
                                        da.Fill(dt);
                                        Console.WriteLine($"[DIAGNOSTIC] After insert: {dt.Rows.Count} records found");
                                    }
                                }
                            }
                            
                            catch (Exception insertEx)
                            {
                                Console.WriteLine($"[DIAGNOSTIC] Error inserting test data: {insertEx.Message}");
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        string errorMsg = $"SQL Error in GetStaff: {sqlEx.Message}\n\nError Number: {sqlEx.Number}\n\nStack Trace:\n{sqlEx.StackTrace}";
                        Console.WriteLine(errorMsg);
                        MessageBox.Show(errorMsg, "Database SQL Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception connEx)
                    {
                        string errorMsg = $"Connection Error in GetStaff: {connEx.Message}\n\nStack Trace:\n{connEx.StackTrace}";
                        Console.WriteLine(errorMsg);
                        MessageBox.Show(errorMsg, "Database Connection Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"Unexpected Error in GetStaff: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
                Console.WriteLine(errorMsg);
                MessageBox.Show(errorMsg, "Unexpected Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dt;
        }
        

        public static bool AddStaff(string name, string username, string password, string role)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("AddStaff", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);
                        cmd.Parameters.AddWithValue("@Role", role);
                        
                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding staff: {ex.Message}");
                    return false;
                }
            }
        }

        public static bool UpdateStaff(int staffId, string name, string username, string password, string role)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"UPDATE Staff 
                                    SET Name = @Name, 
                                        Username = @Username, " +
                                        (string.IsNullOrEmpty(password) ? "" : "Password = @Password, ") + 
                                    @" Role = @Role 
                                    WHERE StaffID = @StaffID";
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StaffID", staffId);
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Username", username);
                        if (!string.IsNullOrEmpty(password))
                        {
                            cmd.Parameters.AddWithValue("@Password", password);
                        }
                        cmd.Parameters.AddWithValue("@Role", role);
                        
                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error updating staff: {ex.Message}");
                    return false;
                }
            }
        }

        public static bool RegisterUser(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("RegisterUser", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@Password", password);
                        
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
                catch (SqlException ex) when (ex.Number == 50000) 
                {
                    MessageBox.Show("Registration failed: " + ex.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error registering user: " + ex.Message);
                    return false;
                }
            }
        }

        public static bool AddCategory(string name, string description = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("AddCategory", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@CategoryName", name);
                        cmd.Parameters.AddWithValue("@Description", (object)description ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
                catch (SqlException ex) when (ex.Number == 50000) 
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error adding category: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        public static bool UpdateCategory(int id, string name, string description = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE Categories SET CategoryName = @Name, Description = @Description WHERE CategoryID = @ID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Description", (object)description ?? DBNull.Value);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating category: " + ex.Message);
                    return false;
                }
            }
        }

        public static bool DeleteCategory(int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "DELETE FROM Categories WHERE CategoryID = @ID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting category: " + ex.Message);
                    return false;
                }
            }
        }

        public static DataTable GetCategories(string search = "")
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                DataTable dt = new DataTable();
                try
                {
                    conn.Open();
                    string query = "SELECT CategoryID, CategoryName, Description, IsActive, CreatedAt FROM Categories " +
                                  "WHERE CategoryName LIKE @Search OR Description LIKE @Search";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Search", $"%{search}%");
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading categories: " + ex.Message);
                }
                return dt;
            }
        }

        public static bool AddTable(string tableNumber, int capacity, string status = "Available")
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("AddTable", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@TableNumber", tableNumber);
                        cmd.Parameters.AddWithValue("@Capacity", capacity);
                        cmd.Parameters.AddWithValue("@Status", status);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
                catch (SqlException ex) when (ex.Number == 50000)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error adding table: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        public static bool UpdateTable(int id, string tableNumber, int capacity, string status)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE RestaurantTables SET TableNumber = @TableNumber, Capacity = @Capacity, Status = @Status WHERE TableID = @ID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);
                        cmd.Parameters.AddWithValue("@TableNumber", tableNumber);
                        cmd.Parameters.AddWithValue("@Capacity", capacity);
                        cmd.Parameters.AddWithValue("@Status", status);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating table: " + ex.Message);
                    return false;
                }
            }
        }

        public static bool DeleteTable(int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "DELETE FROM RestaurantTables WHERE TableID = @ID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting table: " + ex.Message);
                    return false;
                }
            }
        }

        public static DataTable GetTables(string search = "")
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                DataTable dt = new DataTable();
                try
                {
                    conn.Open();
                    string query = "SELECT TableID, TableNumber, Capacity, Status, CreatedAt FROM RestaurantTables " +
                                  "WHERE TableNumber LIKE @Search OR Status LIKE @Search";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Search", $"%{search}%");
                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading tables: " + ex.Message);
                }
                return dt;
            }
        }

        public static bool DeleteStaff(int id)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "DELETE FROM Staff WHERE StaffID = @ID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", id);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting staff: " + ex.Message);
                    return false;
                }
            }
        }

    }
}

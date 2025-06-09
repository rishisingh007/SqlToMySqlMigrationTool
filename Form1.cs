using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;


namespace SqlToMySqlMigrator
{
    public class Form1 : Form
    {
        private MenuStrip menuStrip;
        private ToolStripMenuItem helpMenu;
        private Label lblSqlDbName;
        private TextBox txtSqlDbName;
        private Label lblSqlInstance;
        private TextBox txtSqlInstance;
        private Label lblSqlUser;
        private TextBox txtSqlUser;
        private Label lblSqlPassword;
        private TextBox txtSqlPassword;
        private Button btnTestSql;

        private Label lblTo;

        private Label lblMySqlDbName;
        private TextBox txtMySqlDbName;
        private Label lblMySqlHost;
        private TextBox txtMySqlHost;
        private Label lblMySqlUser;
        private TextBox txtMySqlUser;
        private Label lblMySqlPassword;
        private TextBox txtMySqlPassword;
        private Button btnTestMySql;

        private Button btnMigrate;
        private Button btnMigrateData;

        public Form1()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "MS SQL to MySQL Migrator";
            this.ClientSize = new System.Drawing.Size(400, 500);

            menuStrip = new MenuStrip();
            helpMenu = new ToolStripMenuItem("Help");
            helpMenu.Click += HelpMenu_Click;
            menuStrip.Items.Add(helpMenu);
            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // SQL Server Section
            lblSqlInstance = new Label { Text = "SQL Server Instance:", Left = 30, Top = 50, Width = 130 };
            txtSqlInstance = new TextBox { Left = 170, Top = 50, Width = 180 };

            lblSqlDbName = new Label { Text = "SQL Server Database:", Left = 30, Top = 85, Width = 130 };
            txtSqlDbName = new TextBox { Left = 170, Top = 85, Width = 180 };

            lblSqlUser = new Label { Text = "SQL Server User:", Left = 30, Top = 120, Width = 130 };
            txtSqlUser = new TextBox { Left = 170, Top = 120, Width = 180 };

            lblSqlPassword = new Label { Text = "SQL Server Password:", Left = 30, Top = 155, Width = 130 };
            txtSqlPassword = new TextBox { Left = 170, Top = 155, Width = 180, UseSystemPasswordChar = true };

            btnTestSql = new Button { Text = "Test SQL Connection", Left = 30, Top = 190, Width = 160 };
            btnTestSql.Click += BtnTestSql_Click;

            // TO label
            lblTo = new Label { Text = "↓ To ↓", Left = 170, Top = 225, Width = 60, TextAlign = ContentAlignment.MiddleCenter };

            // MySQL Section
            lblMySqlHost = new Label { Text = "MySQL Host:", Left = 30, Top = 260, Width = 130 };
            txtMySqlHost = new TextBox { Left = 170, Top = 260, Width = 180 };

            lblMySqlDbName = new Label { Text = "MySQL Database:", Left = 30, Top = 295, Width = 130 };
            txtMySqlDbName = new TextBox { Left = 170, Top = 295, Width = 180 };

            lblMySqlUser = new Label { Text = "MySQL User:", Left = 30, Top = 330, Width = 130 };
            txtMySqlUser = new TextBox { Left = 170, Top = 330, Width = 180 };

            lblMySqlPassword = new Label { Text = "MySQL Password:", Left = 30, Top = 365, Width = 130 };
            txtMySqlPassword = new TextBox { Left = 170, Top = 365, Width = 180, UseSystemPasswordChar = true };

            btnTestMySql = new Button { Text = "Test MySQL Connection", Left = 30, Top = 400, Width = 160 };
            btnTestMySql.Click += BtnTestMySql_Click;

            // Final Migrate Buttons
            btnMigrate = new Button { Text = "Migrate Schema", Left = 250, Top = 435, Width = 100, ForeColor = System.Drawing.Color.Blue };
            btnMigrate.Click += BtnMigrate_Click;

            btnMigrateData = new Button { Text = "Migrate Data", Left = 250, Top = 470, Width = 100, ForeColor = System.Drawing.Color.Green };
            btnMigrateData.Click += BtnMigrateData_Click;

            this.Controls.Add(btnMigrateData);            

            // Add controls
            this.Controls.Add(lblSqlInstance);
            this.Controls.Add(txtSqlInstance);
            this.Controls.Add(lblSqlDbName);
            this.Controls.Add(txtSqlDbName);
            this.Controls.Add(lblSqlUser);
            this.Controls.Add(txtSqlUser);
            this.Controls.Add(lblSqlPassword);
            this.Controls.Add(txtSqlPassword);
            this.Controls.Add(btnTestSql);

            this.Controls.Add(lblTo);

            
            this.Controls.Add(lblMySqlHost);
            this.Controls.Add(txtMySqlHost);
            this.Controls.Add(lblMySqlDbName);
            this.Controls.Add(txtMySqlDbName);
            this.Controls.Add(lblMySqlUser);
            this.Controls.Add(txtMySqlUser);
            this.Controls.Add(lblMySqlPassword);
            this.Controls.Add(txtMySqlPassword);
            this.Controls.Add(btnTestMySql);

            this.Controls.Add(btnMigrate);
        }

        private void BtnTestSql_Click(object sender, EventArgs e)
        {
            string connStr = $"Server={txtSqlInstance.Text};Initial Catalog={txtSqlDbName.Text};User Id={txtSqlUser.Text};Password={txtSqlPassword.Text};";
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    MessageBox.Show("SQL Server connection successful.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("SQL Server connection failed:\n" + ex.Message);
                }
            }
        }

        private void BtnTestMySql_Click(object sender, EventArgs e)
        {
            string connStr = $"Server={txtMySqlHost.Text};Database={txtMySqlDbName.Text};Uid={txtMySqlUser.Text};Pwd={txtMySqlPassword.Text};";
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                try
                {
                    conn.Open();
                    MessageBox.Show("MySQL connection successful.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("MySQL connection failed:\n" + ex.Message);
                }
            }
        }

        private void BtnMigrate_Click(object sender, EventArgs e)
        {
            string mysqlConnStr = $"Server={txtMySqlHost.Text};Uid={txtMySqlUser.Text};Pwd={txtMySqlPassword.Text};";
            string mysqlDb = txtMySqlDbName.Text.Trim();

            string sqlConnStr = $"Server={txtSqlInstance.Text};Database={txtSqlDbName.Text};User Id={txtSqlUser.Text};Password={txtSqlPassword.Text};";

            try
            {
                using (var mysqlConn = new MySqlConnection(mysqlConnStr))
                {
                    mysqlConn.Open();

                    // Create DB if not exists
                    using (var cmd = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{mysqlDb}`;", mysqlConn))
                        cmd.ExecuteNonQuery();

                    // Select the new DB
                    mysqlConn.ChangeDatabase(mysqlDb);

                    using (var sqlConn = new SqlConnection(sqlConnStr))
                    {
                        sqlConn.Open();

                        // Get all table names
                        var sqlTables = new List<string>();
                        using (var cmd = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_CATALOG=@db", sqlConn))
                        {
                            cmd.Parameters.AddWithValue("@db", txtSqlDbName.Text.Trim());
                            using (var reader = cmd.ExecuteReader())
                                while (reader.Read())
                                    sqlTables.Add(reader.GetString(0));
                        }

                        foreach (string tableName in sqlTables)
                        {
                            string mysqlTable = ToSnakeCase(tableName);
                            List<string> columnDefs = new();

                            using (var cmd = new SqlCommand(
                                @"SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE 
                          FROM INFORMATION_SCHEMA.COLUMNS 
                          WHERE TABLE_NAME = @table AND TABLE_CATALOG = @db", sqlConn))
                            {
                                cmd.Parameters.AddWithValue("@table", tableName);
                                cmd.Parameters.AddWithValue("@db", txtSqlDbName.Text.Trim());

                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        string col = ToSnakeCase(reader.GetString(0));
                                        string type = reader.GetString(1);
                                        string nullable = reader.GetString(3);
                                        string sqlType = MapSqlTypeToMySql(type, reader["CHARACTER_MAXIMUM_LENGTH"]);

                                        columnDefs.Add($"`{col}` {sqlType} {(nullable == "YES" ? "" : "NOT NULL")}");
                                    }
                                }
                            }

                            string createTable = $"CREATE TABLE `{mysqlTable}` (\n  {string.Join(",\n  ", columnDefs)}\n);";

                            using (var cmd = new MySqlCommand(createTable, mysqlConn))
                                cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("✅ Table migration completed (schema only).");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Migration failed:\n" + ex.Message);
            }
        }

        private void BtnMigrateData_Click(object sender, EventArgs e)
        {
            string sqlConnStr = $"Server={txtSqlInstance.Text};Database={txtSqlDbName.Text};User Id={txtSqlUser.Text};Password={txtSqlPassword.Text};";
            string mysqlDbName = txtMySqlDbName.Text.Trim();
            string mysqlConnStr = $"Server={txtMySqlHost.Text};Database={mysqlDbName};Uid={txtMySqlUser.Text};Pwd={txtMySqlPassword.Text};";

            try
            {
                using var sqlConn = new SqlConnection(sqlConnStr);
                sqlConn.Open();

                using var mysqlConn = new MySqlConnection(mysqlConnStr);
                mysqlConn.Open();

                // ✅ Step 1: Check if MySQL database exists
                using (var checkDbCmd = new MySqlCommand($"SHOW DATABASES LIKE '{mysqlDbName}'", mysqlConn))
                using (var reader = checkDbCmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        MessageBox.Show($"❌ MySQL database '{mysqlDbName}' does not exist.\nPlease run schema migration first.");
                        return;
                    }
                }

                mysqlConn.ChangeDatabase(mysqlDbName);

                // ✅ Step 2: Get all table names from SQL Server
                var sqlTables = new List<string>();
                using (var cmd = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_CATALOG=@db", sqlConn))
                {
                    cmd.Parameters.AddWithValue("@db", txtSqlDbName.Text.Trim());
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                        sqlTables.Add(reader.GetString(0));
                }

                // ✅ Step 3: Check if all expected MySQL tables exist
                foreach (string table in sqlTables)
                {
                    string mysqlTable = ToSnakeCase(table);
                    using var checkTableCmd = new MySqlCommand($"SHOW TABLES LIKE '{mysqlTable}'", mysqlConn);
                    var result = checkTableCmd.ExecuteScalar();
                    if (result == null)
                    {
                        MessageBox.Show($"❌ MySQL table '{mysqlTable}' not found.\nPlease run schema migration first.");
                        return;
                    }
                }
        

                foreach (string table in sqlTables)
                {
                    string mysqlTable = ToSnakeCase(table);

                    // Get all columns
                    List<string> columns = new();
                    using (var cmd = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @table AND TABLE_CATALOG = @db", sqlConn))
                    {
                        cmd.Parameters.AddWithValue("@table", table);
                        cmd.Parameters.AddWithValue("@db", txtSqlDbName.Text.Trim());
                        using var reader = cmd.ExecuteReader();
                        while (reader.Read())
                            columns.Add(reader.GetString(0));
                    }

                    // Fetch data from SQL Server
                    using var fetchCmd = new SqlCommand($"SELECT * FROM [{table}]", sqlConn);
                    using var fetchReader = fetchCmd.ExecuteReader();

                    while (fetchReader.Read())
                    {
                        List<string> colNames = new();
                        List<string> colValues = new();

                        for (int i = 0; i < fetchReader.FieldCount; i++)
                        {
                            string col = ToSnakeCase(fetchReader.GetName(i));
                            object val = fetchReader.GetValue(i);
                            colNames.Add($"`{col}`");

                            if (val == DBNull.Value)
                                colValues.Add("NULL");
                            else if (val is string || val is DateTime)
                                colValues.Add($"'{MySqlHelper.EscapeString(val.ToString())}'");
                            else
                                colValues.Add(val.ToString());
                        }

                        string insertQuery = $"INSERT INTO `{mysqlTable}` ({string.Join(",", colNames)}) VALUES ({string.Join(",", colValues)});";
                        using var insertCmd = new MySqlCommand(insertQuery, mysqlConn);
                        insertCmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("✅ Data migration completed successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("❌ Data migration failed:\n" + ex.Message);
            }
        }


        private string ToSnakeCase(string name)
        {
            if (name == "ID") return "id"; // Special case

            var result = Regex.Replace(name, @"(?<!^)(?=[A-Z])", "_").ToLower();
            result = Regex.Replace(result, @"_d$", "d"); // fix for ID becoming _d
            return result;
        }

        private string MapSqlTypeToMySql(string sqlType, object maxLength)
        {
            return sqlType switch
            {
                "int" => "INT",
                "bigint" => "BIGINT",
                "nvarchar" => maxLength != DBNull.Value ? $"VARCHAR({maxLength})" : "TEXT",
                "varchar" => maxLength != DBNull.Value ? $"VARCHAR({maxLength})" : "TEXT",
                "datetime" => "DATETIME",
                "bit" => "BOOLEAN",
                "float" => "FLOAT",
                "decimal" => "DECIMAL(18,2)",
                _ => "TEXT" // fallback
            };
        }

        private void HelpMenu_Click(object sender, EventArgs e)
        {
            Form helpForm = new Form
            {
                Text = "About This Tool",
                Size = new Size(400, 300),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label lblInfo = new Label
            {
                Text = "📘 MS SQL to MySQL Migration Tool\n\n" +
                       "🔹 Developed by: Rishi Pal Singh\n\n" +
                       "🔹 This tool helps migrate schema and data\n" +
                       "    from Microsoft SQL Server to MySQL.\n\n" +
                       "🔹 Step 1: Migrate Schema — creates tables in MySQL.\n" +
                       "🔹 Step 2: Migrate Data — moves all data.\n\n" +
                       "🔹 All table and column names are converted to\n" +
                       "    lowercase snake_case for compatibility.\n",
                AutoSize = false,
                Width = 360,
                Height = 220,
                Top = 20,
                Left = 20
            };

            Button btnClose = new Button
            {
                Text = "Close",
                Width = 80,
                Height = 30,
                Top = 200,
                Left = 150
            };
            btnClose.Click += (s, ev) => helpForm.Close();

            helpForm.Controls.Add(lblInfo);
            helpForm.Controls.Add(btnClose);
            helpForm.ShowDialog();
        }


        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new Form1());
        }

    }
}

using Konscious.Security.Cryptography;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CoffeeAdminHelper
{
    public static class ElementHelper
    {
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.RegisterAttached(
                "CornerRadius",
                typeof(CornerRadius),
                typeof(ElementHelper),
                new FrameworkPropertyMetadata(new CornerRadius(5), FrameworkPropertyMetadataOptions.Inherits));

        public static void SetCornerRadius(UIElement element, CornerRadius value) =>
            element.SetValue(CornerRadiusProperty, value);

        public static CornerRadius GetCornerRadius(UIElement element) =>
            (CornerRadius)element.GetValue(CornerRadiusProperty);
    }

    public partial class MainWindow : Window
    {
        private static readonly string database_command = @"server=localhost;userid=root;password=;";
        private readonly string cs = database_command + @"database=coffeeshop_database";

        public MainWindow()
        {
            InitializeComponent();
            using (var connect = new MySqlConnection(database_command))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("CREATE DATABASE IF NOT EXISTS coffeeshop_database", connect))
                {
                    command.ExecuteNonQuery();
                    connect.Close();
                    connect.ConnectionString = cs;
                    connect.Open(); 
                    command.CommandText = "CREATE TABLE IF NOT EXISTS admins(admin_username VARCHAR(100) COLLATE utf8mb4_bin NOT NULL, salt VARCHAR(150) NOT NULL, hash VARCHAR(150) NOT NULL, can_add_products BOOLEAN NOT NULL DEFAULT FALSE, can_remove_products BOOLEAN NOT NULL DEFAULT FALSE, can_update_products BOOLEAN NOT NULL DEFAULT FALSE, can_add_menus BOOLEAN NOT NULL DEFAULT FALSE, can_remove_menus BOOLEAN NOT NULL DEFAULT FALSE, can_update_menus BOOLEAN NOT NULL DEFAULT FALSE, can_add_menucontents BOOLEAN NOT NULL DEFAULT FALSE, can_remove_menucontents BOOLEAN NOT NULL DEFAULT FALSE, can_add_members BOOLEAN NOT NULL DEFAULT FALSE, can_remove_members BOOLEAN NOT NULL DEFAULT FALSE, can_update_members BOOLEAN NOT NULL DEFAULT FALSE, can_view_history BOOLEAN NOT NULL DEFAULT FALSE, can_modify_rates BOOLEAN NOT NULL DEFAULT FALSE, can_modify_disc BOOLEAN NOT NULL DEFAULT FALSE, can_modify_levelup BOOLEAN NOT NULL DEFAULT FALSE, can_modify_order BOOLEAN NOT NULL DEFAULT FALSE, superadmin BOOLEAN NOT NULL DEFAULT FALSE, PRIMARY KEY (admin_username))";
                    command.ExecuteNonQuery();
                }
            }
            CheckSuperadmin();
        }

        private (string Hash, string Salt) HashPassword(string password)
        {
            // Generate a random salt
            if (string.IsNullOrEmpty(password)) return ("", "");
            byte[] salt = new byte[16];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            // Hash the password using Argon2id
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = salt;
                argon2.MemorySize = 65536; // Example memory cost in KB
                argon2.Iterations = 8; // Number of iterations
                argon2.DegreeOfParallelism = 2; // Degree of parallelism

                byte[] hash = argon2.GetBytes(32); // Output hash size
                return (Convert.ToBase64String(hash), Convert.ToBase64String(salt)); // Store both
            }
        }

        private bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            byte[] salt = Convert.FromBase64String(storedSalt);
            byte[] storedHashBytes = Convert.FromBase64String(storedHash);

            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = salt;
                argon2.MemorySize = 65536; // Same memory cost
                argon2.Iterations = 8; // Same number of iterations
                argon2.DegreeOfParallelism = 2; // Same degree of parallelism

                byte[] computedHash = argon2.GetBytes(32); // Hash length

                return Convert.ToBase64String(computedHash) == storedHash; // Compare hashes
            }
        }

        private void ShowNewPassword_Click(object sender, RoutedEventArgs e)
        {
            if (ShowNewPassword.IsChecked == true)
            {
                ShownNewAdminPassword.Text = HiddenNewAdminPassword.Password;
                HiddenNewAdminPassword.Visibility = Visibility.Collapsed;
                ShownNewAdminPassword.Visibility = Visibility.Visible;
            }
            else
            {
                HiddenNewAdminPassword.Password = ShownNewAdminPassword.Text;
                HiddenNewAdminPassword.Visibility = Visibility.Visible;
                ShownNewAdminPassword.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowNewSuperadminPassword_Click(object sender, RoutedEventArgs e)
        {
            if (ShowNewSuperadminPassword.IsChecked == true)
            {
                ShownNewSuperadminPassword.Text = HiddenNewSuperadminPassword.Password;
                HiddenNewSuperadminPassword.Visibility = Visibility.Collapsed;
                ShownNewSuperadminPassword.Visibility = Visibility.Visible;
            }
            else 
            {
                HiddenNewSuperadminPassword.Password = ShownNewSuperadminPassword.Text;
                HiddenNewSuperadminPassword.Visibility = Visibility.Visible;
                ShownNewSuperadminPassword.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowCurrentSuperadminPassword_Click(object sender, RoutedEventArgs e)
        {
            if (ShowCurrentSuperadminPassword.IsChecked == true)
            {
                ShownCurrentSuperadminPassword.Text = HiddenCurrentSuperadminPassword.Password;
                HiddenCurrentSuperadminPassword.Visibility = Visibility.Collapsed;
                ShownCurrentSuperadminPassword.Visibility = Visibility.Visible;
            }
            else
            {
                HiddenCurrentSuperadminPassword.Password = ShownCurrentSuperadminPassword.Text;
                HiddenCurrentSuperadminPassword.Visibility = Visibility.Visible;
                ShownCurrentSuperadminPassword.Visibility = Visibility.Collapsed;
            }
        }

        private void ClearNewSuperadminTextboxes_Click(object sender, RoutedEventArgs e)
        {
            NewSuperadminUsername.Text = "";
            ShownNewSuperadminPassword.Text = "";
            HiddenNewSuperadminPassword.Password = "";
        }

        private void ClearCurrentSuperadminTextboxes_Click(object sender, RoutedEventArgs e)
        {
            CurrentSuperadminUsername.Text = "";
            ShownCurrentSuperadminPassword.Text = "";
            HiddenCurrentSuperadminPassword.Password = "";
        }

        private void CheckSuperadmin()
        {
            int condition = 0;
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT EXISTS(SELECT 1 FROM ADMINS LIMIT 1)", connect))
                {
                    int result =  Convert.ToInt32(command.ExecuteScalar());
                    if (result == 0) ++condition;
                    command.CommandText = "SELECT EXISTS(SELECT 1 FROM ADMINS WHERE superadmin = 1 LIMIT 1)";
                    result = Convert.ToInt32(command.ExecuteScalar());
                    if (result == 0) ++condition;
                }
            }
            if (condition >= 1)
            {
                AdminDashboard.Visibility = Visibility.Collapsed;
                LoginSuperadmin.Visibility = Visibility.Collapsed;
                CreateSuperadmin.Visibility= Visibility.Visible;
            }
        }

        private void CreateSuperadminAccountButton_Click(object sender, RoutedEventArgs e)
        {
            string curpass = "";
            if (ShownNewSuperadminPassword.Visibility == Visibility.Visible) curpass = ShownNewSuperadminPassword.Text;
            else if (HiddenNewSuperadminPassword.Visibility == Visibility.Visible) curpass = HiddenNewSuperadminPassword.Password;
            if (string.IsNullOrWhiteSpace(NewSuperadminUsername.Text) || string.IsNullOrWhiteSpace(curpass))
            {
                MessageBox.Show("Please ensure that the username and password are valid.", "Account creation unsuccessful", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand($"SELECT EXISTS(SELECT 1 FROM admins WHERE admin_username = @username)", connect))
                {
                    command.Parameters.AddWithValue("@username",  NewSuperadminUsername.Text);
                    int result = Convert.ToInt32(command.ExecuteScalar());
                    if (result > 0)
                    {
                        MessageBox.Show("Please use a different username.", "Username unavailable", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    command.CommandText = "INSERT INTO admins VALUES(@username, @new_salt, @new_hash, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1)";
                    var salt_hash = HashPassword(curpass);
                    command.Parameters.AddWithValue("@new_salt", salt_hash.Salt);
                    command.Parameters.AddWithValue("@new_hash", salt_hash.Hash);
                    command.ExecuteNonQuery();
                    MessageBox.Show("The superadmin account has been successfully created", "Account creation successful.", MessageBoxButton.OK, MessageBoxImage.Information);
                    CreateSuperadmin.Visibility = Visibility.Collapsed;
                    LoginSuperadmin.Visibility = Visibility.Visible;
                    NewSuperadminUsername.Text = "";
                    ShownNewAdminPassword.Text = "";
                    HiddenNewAdminPassword.Password = "";
                }
            }
        }

        private void LoginSuperadminAccountButton_Click(object sender, RoutedEventArgs e)
        {
            string curpass = "";
            if (ShownCurrentSuperadminPassword.Visibility == Visibility.Visible) curpass = ShownCurrentSuperadminPassword.Text;
            else if (HiddenCurrentSuperadminPassword.Visibility == Visibility.Visible) curpass = HiddenCurrentSuperadminPassword.Password;
            if (string.IsNullOrWhiteSpace(CurrentSuperadminUsername.Text) || string.IsNullOrWhiteSpace(curpass))
            {
                MessageBox.Show("Please ensure that the username and password are valid.", "Login unsuccessful", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            using (var connect = new MySqlConnection(cs))
            {
                string cur_hash = "", cur_salt = "";
                connect.Open();
                using (MySqlCommand command = new MySqlCommand($"SELECT salt, hash FROM admins WHERE admin_username = @username AND superadmin = 1 LIMIT 1", connect))
                {
                    command.Parameters.AddWithValue("@username", CurrentSuperadminUsername.Text);
                    bool found = false;
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cur_hash = reader[1]?.ToString();
                            cur_salt = reader[0]?.ToString();
                            if (!string.IsNullOrEmpty(cur_hash) && !string.IsNullOrEmpty(cur_salt))
                            {
                                if (VerifyPassword(curpass, cur_hash, cur_salt))
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                        if (found)
                        {
                            CurrentSuperadminUsername.Text = "";
                            ShownCurrentSuperadminPassword.Text = "";
                            HiddenCurrentSuperadminPassword.Password = "";
                            LoginSuperadmin.Visibility = Visibility.Collapsed;
                            LoadAdmins("");
                            AdminDashboard.Visibility = Visibility.Visible;
                            MessageBox.Show("You have successfully logged into the system.", "Superadmin login successful", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                        MessageBox.Show("Please ensure that the superadmin credentials are valid.", "Superadmin login unsuccessful", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void LoadAdmins(string target)
        {
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand($"SELECT admin_username AS 'Admin Username', can_add_products AS 'Can Add Products', can_remove_products AS 'Can Remove Products', can_update_products AS 'Can Update Products', can_add_menus AS 'Can Add Menus', can_remove_menus AS 'Can Remove Menus', can_update_menus AS 'Can Update Menus', can_add_menucontents AS 'Can Add Menu Contents', can_remove_menucontents AS 'Can Remove Menu Contents', can_add_members AS 'Can Add Members', can_remove_members AS 'Can Remove Members', can_update_members AS 'Can Update Members', can_view_history AS 'Can View Transactions', can_modify_rates AS 'Can Modify Rates', can_modify_disc AS 'Can Modify Discounts', can_modify_levelup AS 'Can Modify Level-Up', can_modify_order AS 'Can Modify Order Number' FROM admins WHERE superadmin = 0 AND admin_username LIKE @search", connect))
                {
                    if (target.Contains("\\") || target.Contains("/") || string.IsNullOrWhiteSpace(target)) { target = "%"; }
                    command.Parameters.AddWithValue("@search", target);
                    DataTable table = new DataTable();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command)) adapter.Fill(table);
                    foreach (DataRow row in table.Rows)
                    {
                        row["Can Add Products"] = Convert.ToBoolean(row["Can Add Products"]);
                        row["Can Remove Products"] = Convert.ToBoolean(row["Can Remove Products"]);
                        row["Can Update Products"] = Convert.ToBoolean(row["Can Update Products"]);
                        row["Can Add Menus"] = Convert.ToBoolean(row["Can Add Menus"]);
                        row["Can Remove Menus"] = Convert.ToBoolean(row["Can Remove Menus"]);
                        row["Can Update Menus"] = Convert.ToBoolean(row["Can Update Menus"]);
                        row["Can Add Menu Contents"] = Convert.ToBoolean(row["Can Add Menu Contents"]);
                        row["Can Remove Menu Contents"] = Convert.ToBoolean(row["Can Remove Menu Contents"]);
                        row["Can Add Members"] = Convert.ToBoolean(row["Can Add Members"]);
                        row["Can Remove Members"] = Convert.ToBoolean(row["Can Remove Members"]);
                        row["Can View Transactions"] = Convert.ToBoolean(row["Can View Transactions"]);
                        row["Can Modify Rates"] = Convert.ToBoolean(row["Can Modify Rates"]);
                        row["Can Modify Discounts"] = Convert.ToBoolean(row["Can Modify Discounts"]);
                        row["Can Modify Level-Up"] = Convert.ToBoolean(row["Can Modify Level-Up"]);
                    }
                    AdminTable.ItemsSource = table.DefaultView;
                }
            }
        }

        private void LogoutAdmin_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to log out?", "Confirm logout", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes)
            {
                AdminDashboard.Visibility = Visibility.Collapsed;
                LoginSuperadmin.Visibility = Visibility.Visible;
                CheckSuperadmin();
                MessageBox.Show("You have successfully logged out.", "Logged out successfully", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SearchAccountButton_Click(object sender, RoutedEventArgs e) 
        {
            if (CurrentAdminUsername.Text.Contains("\\") || CurrentAdminUsername.Text.Contains("/")) return;
            LoadAdmins(CurrentAdminUsername.Text); 
        }

        private void AddAccountButton_Click(object sender, RoutedEventArgs e)
        {
            string curpass = "";
            if (ShownNewAdminPassword.Visibility == Visibility.Visible) curpass = ShownNewAdminPassword.Text;
            else if (HiddenNewAdminPassword.Visibility == Visibility.Visible) curpass = HiddenNewAdminPassword.Password;
            if (string.IsNullOrWhiteSpace(NewAdminUsername.Text) || string.IsNullOrWhiteSpace(curpass))
            {
                MessageBox.Show("Please ensure that the username and password are valid.", "Account creation unsuccessful", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand($"SELECT EXISTS(SELECT 1 FROM admins WHERE admin_username = @username)", connect))
                {
                    command.Parameters.AddWithValue("@username", NewAdminUsername.Text);
                    int result = Convert.ToInt32(command.ExecuteScalar());
                    if (result > 0)
                    {
                        MessageBox.Show("This username is unavailable, please use a different username", "Username not available", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    command.CommandText = "INSERT INTO admins VALUES(@username, @salt, @hash, @can_add_prod, @can_remove_prod, @can_update_prod, @can_add_menu, @can_remove_menu, @can_update_menu, @can_add_contents, @can_remove_contents, @can_add_member, @can_remove_member, @can_update_member, @can_view_hist, @can_mod_rate, @can_mod_disc, @can_mod_level, @can_mod_ord, 0)";
                    var hash_salt = HashPassword(curpass);
                    command.Parameters.AddWithValue("@salt", hash_salt.Salt);
                    command.Parameters.AddWithValue("@hash", hash_salt.Hash);
                    command.Parameters.AddWithValue("@can_add_prod", CanAddProducts.IsChecked);
                    command.Parameters.AddWithValue("@can_remove_prod", CanRemoveProducts.IsChecked);
                    command.Parameters.AddWithValue("@can_update_prod", CanUpdateProducts.IsChecked);
                    command.Parameters.AddWithValue("@can_add_menu", CanAddMenus.IsChecked);
                    command.Parameters.AddWithValue("@can_remove_menu", CanRemoveMenus.IsChecked);
                    command.Parameters.AddWithValue("@can_update_menu", CanUpdateMenus.IsChecked);
                    command.Parameters.AddWithValue("@can_add_contents", CanAddProductsInMenus.IsChecked);
                    command.Parameters.AddWithValue("@can_remove_contents", CanRemoveProductsInMenus.IsChecked);
                    command.Parameters.AddWithValue("@can_add_member", CanAddMembers.IsChecked);
                    command.Parameters.AddWithValue("@can_remove_member", CanRemoveMembers.IsChecked);
                    command.Parameters.AddWithValue("@can_update_member", CanUpdateMembers.IsChecked);
                    command.Parameters.AddWithValue("@can_view_hist", CanViewHistory.IsChecked);
                    command.Parameters.AddWithValue("@can_mod_rate", CanModifyRates.IsChecked);
                    command.Parameters.AddWithValue("@can_mod_disc", CanModifyDiscount.IsChecked);
                    command.Parameters.AddWithValue("@can_mod_level", CanModifyLevelUp.IsChecked);
                    command.Parameters.AddWithValue("@can_mod_ord", CanModifyOrderNumber.IsChecked);
                    command.ExecuteNonQuery();
                    LoadAdmins("");
                    ClearAllInputs();
                    MessageBox.Show("The administrator account has been successfully created.", "Account creation successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void AdminTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AdminTable.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)AdminTable.SelectedItem;
                string menuName = selectedRow["Admin Username"].ToString();
                CurrentAdminUsername.Text = menuName;

                bool curPerm = Convert.ToBoolean(selectedRow["Can Add Products"]);
                if (curPerm) CanAddProducts.IsChecked = true;
                else CanAddProducts.IsChecked = false;
                curPerm = Convert.ToBoolean(selectedRow["Can Remove Products"]);
                if (curPerm) CanRemoveProducts.IsChecked = true;
                else CanRemoveProducts.IsChecked = false;
                curPerm = Convert.ToBoolean(selectedRow["Can Update Products"]);
                if (curPerm) CanUpdateProducts.IsChecked = true;
                else CanUpdateProducts.IsChecked = false;
                curPerm = Convert.ToBoolean(selectedRow["Can Add Menus"]);
                if (curPerm) CanAddMenus.IsChecked = true;
                else CanAddMenus.IsChecked = false;
                curPerm = Convert.ToBoolean(selectedRow["Can Remove Menus"]);
                if (curPerm) CanRemoveMenus.IsChecked = true;
                else CanRemoveMenus.IsChecked = false;
                curPerm = Convert.ToBoolean(selectedRow["Can Update Menus"]);
                if (curPerm) CanUpdateMenus.IsChecked = true;
                else CanUpdateMenus.IsChecked = false;
                curPerm = Convert.ToBoolean(selectedRow["Can Add Menu Contents"]);
                if (curPerm) CanAddProductsInMenus.IsChecked = true;
                else CanAddProductsInMenus.IsChecked = false;
                curPerm = Convert.ToBoolean(selectedRow["Can Remove Menu Contents"]);
                if (curPerm) CanRemoveProductsInMenus.IsChecked = true;
                else CanRemoveProductsInMenus.IsChecked = false;
                curPerm = Convert.ToBoolean(selectedRow["Can Add Members"]);
                if (curPerm) CanAddMembers.IsChecked = true;
                else CanAddMembers.IsChecked = false;
                curPerm = Convert.ToBoolean(selectedRow["Can Remove Members"]);
                if (curPerm) CanRemoveMembers.IsChecked = true;
                else CanRemoveMembers.IsChecked = false;
                curPerm = Convert.ToBoolean(selectedRow["Can Update Members"]);
                if (curPerm) CanUpdateMembers.IsChecked = true;
                else CanUpdateMembers.IsChecked = false;
                curPerm = Convert.ToBoolean(selectedRow["Can View Transactions"]);
                if (curPerm) CanViewHistory.IsChecked = true;
                else CanViewHistory.IsChecked = false;
                curPerm = Convert.ToBoolean(selectedRow["Can Modify Rates"]);
                if (curPerm) CanModifyRates.IsChecked = true;
                else CanModifyRates.IsChecked = false;
                curPerm = Convert.ToBoolean(selectedRow["Can Modify Discounts"]);
                if (curPerm) CanModifyDiscount.IsChecked = true;
                else CanModifyDiscount.IsChecked = false;
                curPerm = Convert.ToBoolean(selectedRow["Can Modify Level-Up"]);
                if (curPerm) CanModifyLevelUp.IsChecked = true;
                else CanModifyLevelUp.IsChecked = false;
                curPerm = Convert.ToBoolean(selectedRow["Can Modify Order Number"]);
                if (curPerm) CanModifyOrderNumber.IsChecked = true;
                else CanModifyOrderNumber.IsChecked = false;
            }
        }

        private void RemoveAccountButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentAdminUsername.Text))
            {
                MessageBox.Show("Please ensure that the current admin username is not blank.", "Missing input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    // Lock the entire row first using SELECT * FOR UPDATE to properly apply the lock
                    // Make sure that the entire operation happens inside a transaction, or they will be no effect
                    // Make sure to rollback the transaction when the operation fails to unlock the locked row
                    using (MySqlCommand command = new MySqlCommand($"SELECT superadmin FROM admins WHERE admin_username = @username", connect, transaction))
                    {
                        try
                        {
                            command.Parameters.AddWithValue("@username", CurrentAdminUsername.Text);
                            int is_super = Convert.ToInt32(command.ExecuteScalar());
                            command.CommandText = "SELECT EXISTS(SELECT 1 FROM admins WHERE admin_username = @username)";
                            int found = Convert.ToInt32(command.ExecuteScalar());
                            if (is_super > 0 || found == 0)
                            {
                                MessageBox.Show("Please check if the administrator exists in the system.", "Remove operation failed", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }
                            command.CommandText = "SELECT * FROM admins WHERE admin_username = @username FOR UPDATE";
                            // Lock the row
                            command.ExecuteNonQuery();
                            command.CommandText = "DELETE FROM admins WHERE admin_username = @username";
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            LoadAdmins("");
                            ClearAllInputs();
                            MessageBox.Show("The specified admin account has been deleted.", "Admin account successfully removed", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            MessageBox.Show("The specified admin account has not been deleted, please try again.", "Admin account removal failed", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
        }

        private void ClearFieldsButton_Click(object sender, RoutedEventArgs e) { ClearAllInputs(); }

        private void ClearAllInputs()
        {
            CurrentAdminUsername.Text = "";
            NewAdminUsername.Text = "";
            ShownNewAdminPassword.Text = "";
            HiddenNewAdminPassword.Password = "";
            CanAddProducts.IsChecked = false;
            CanRemoveProducts.IsChecked = false;
            CanUpdateProducts.IsChecked = false;
            CanAddMenus.IsChecked = false;
            CanRemoveMenus.IsChecked = false;
            CanUpdateMenus.IsChecked = false;
            CanAddProductsInMenus.IsChecked = false;
            CanRemoveProductsInMenus.IsChecked = false;
            CanAddMembers.IsChecked = false;
            CanRemoveMembers.IsChecked = false;
            CanUpdateMembers.IsChecked = false;
            CanModifyRates.IsChecked = false;
            CanViewHistory.IsChecked = false;
            CanModifyDiscount.IsChecked = false;
            CanModifyLevelUp.IsChecked = false;
            CanModifyOrderNumber.IsChecked = false;
        }

        private void UpdateAccountButton_Click(object sender, RoutedEventArgs e)
        {
            string new_pass = "";
            if (HiddenNewAdminPassword.Visibility == Visibility.Visible) new_pass = HiddenNewAdminPassword.Password;
            else if (ShownNewAdminPassword.Visibility == Visibility.Visible) new_pass = ShownNewAdminPassword.Text;
            if (NewAdminUsername.Text.Length == 0 && new_pass.Length == 0 && AllowPermissionUpdate.IsChecked == false)
            {
                MessageBox.Show("Please indicate a detail to update.", "Missing input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (CurrentAdminUsername.Text.Length == 0)
            {
                MessageBox.Show("Please ensure that the current admin username is not blank.", "Missing input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {                        
                    // Check for existence
                    using (MySqlCommand command = new MySqlCommand($"SELECT EXISTS(SELECT 1 FROM admins WHERE admin_username = @username AND superadmin = 0 LIMIT 1)", connect, transaction))
                    {
                        try
                        {
                            StringBuilder successful_ops = new StringBuilder();
                            command.Parameters.AddWithValue("@username", CurrentAdminUsername.Text);
                            int result = Convert.ToInt32(command.ExecuteScalar());
                            if (result == 0)
                            {
                                MessageBox.Show("This administrator does not exist, please try again", "Non-existent admin username", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            command.CommandText = "SELECT * FROM admins WHERE admin_username = @username FOR UPDATE";
                            // When existence is confirmed, lock the row
                            command.ExecuteNonQuery();
                            if (!string.IsNullOrWhiteSpace(new_pass))
                            {
                                var hash_salt = HashPassword(new_pass);
                                command.CommandText = "UPDATE admins SET salt = @n_salt, hash = @n_hash WHERE admin_username = @username";
                                command.Parameters.AddWithValue("@n_salt", hash_salt.Salt);
                                command.Parameters.AddWithValue("@n_hash", hash_salt.Hash);
                                command.ExecuteNonQuery();
                                CurrentAdminUsername.Text = "";
                                ShownNewAdminPassword.Text = "";
                                HiddenNewAdminPassword.Password = "";
                                if (successful_ops.Length > 0) successful_ops.Append(", Password");
                                else successful_ops.Append("Password");
                            }
                            if (!string.IsNullOrWhiteSpace(NewAdminUsername.Text))
                            {
                                command.CommandText = "UPDATE admins SET admin_username = @new_user WHERE admin_username = @username";
                                command.Parameters.AddWithValue("@new_user", NewAdminUsername.Text);
                                command.ExecuteNonQuery();
                                CurrentAdminUsername.Text = NewAdminUsername.Text;
                                NewAdminUsername.Text = "";
                                if (successful_ops.Length > 0) successful_ops.Append(", Username");
                                else successful_ops.Append("Username");
                            }
                            if (AllowPermissionUpdate.IsChecked == true)
                            {
                                command.CommandText = "UPDATE admins SET can_add_products = @can_add_prod, can_remove_products = @can_remove_prod, can_update_products = @can_update_prod, can_add_menus = @can_add_menu, can_remove_menus = @can_remove_menu, can_update_menus = @can_update_menu, can_add_menucontents = @can_add_contents, can_remove_menucontents = @can_remove_contents, can_add_members = @can_add_member, can_remove_members = @can_remove_member, can_update_members = @can_update_member, can_view_history = @can_view_hist, can_modify_rates = @can_mod_rate, can_modify_disc = @can_mod_disc, can_modify_levelup = @can_mod_level, can_modify_order = @can_mod_ord WHERE admin_username = @username";
                                command.Parameters.AddWithValue("@can_add_prod", CanAddProducts.IsChecked);
                                command.Parameters.AddWithValue("@can_remove_prod", CanRemoveProducts.IsChecked);
                                command.Parameters.AddWithValue("@can_update_prod", CanUpdateProducts.IsChecked);
                                command.Parameters.AddWithValue("@can_add_menu", CanAddMenus.IsChecked);
                                command.Parameters.AddWithValue("@can_remove_menu", CanRemoveMenus.IsChecked);
                                command.Parameters.AddWithValue("@can_update_menu", CanUpdateMenus.IsChecked);
                                command.Parameters.AddWithValue("@can_add_contents", CanAddProductsInMenus.IsChecked);
                                command.Parameters.AddWithValue("@can_remove_contents", CanRemoveProductsInMenus.IsChecked);
                                command.Parameters.AddWithValue("@can_add_member", CanAddMembers.IsChecked);
                                command.Parameters.AddWithValue("@can_remove_member", CanRemoveMembers.IsChecked);
                                command.Parameters.AddWithValue("@can_update_member", CanUpdateMembers.IsChecked);
                                command.Parameters.AddWithValue("@can_view_hist", CanViewHistory.IsChecked);
                                command.Parameters.AddWithValue("@can_mod_rate", CanModifyRates.IsChecked);
                                command.Parameters.AddWithValue("@can_mod_disc", CanModifyDiscount.IsChecked);
                                command.Parameters.AddWithValue("@can_mod_level", CanModifyLevelUp.IsChecked);
                                command.Parameters.AddWithValue("@can_mod_ord", CanModifyOrderNumber.IsChecked);
                                command.ExecuteNonQuery();
                                CanAddProducts.IsChecked = false;
                                CanRemoveProducts.IsChecked = false;
                                CanUpdateProducts.IsChecked = false;
                                CanAddMenus.IsChecked = false;
                                CanRemoveMenus.IsChecked = false;
                                CanUpdateMenus.IsChecked = false;
                                CanAddProductsInMenus.IsChecked = false;
                                CanRemoveProductsInMenus.IsChecked = false;
                                CanAddMembers.IsChecked = false;
                                CanRemoveMembers.IsChecked = false;
                                CanUpdateMembers.IsChecked = false;
                                AllowPermissionUpdate.IsChecked = false;
                                CanViewHistory.IsChecked = false;
                                CanModifyRates.IsChecked = false;
                                CanModifyDiscount.IsChecked = false;
                                CanModifyLevelUp.IsChecked = false;
                                CanModifyOrderNumber.IsChecked = false;
                                if (successful_ops.Length > 0) successful_ops.Append(", Permissions");
                                else successful_ops.Append("Permissions");
                            }
                            CurrentAdminUsername.Text = "";
                            transaction.Commit();
                            LoadAdmins("");
                            MessageBox.Show($"The following details have been altered: {successful_ops}", "Update operation successful", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (MySqlException ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"The selected administrator has not been updated, please try again {ex}.", "Update operation unsuccessful", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
        }
    }
}

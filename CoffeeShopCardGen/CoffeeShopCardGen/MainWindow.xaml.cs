using Konscious.Security.Cryptography;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
namespace CoffeeShopCardGen
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
        private bool PanelRevealed = true;
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
                    command.CommandText = "CREATE TABLE IF NOT EXISTS members(member_id VARCHAR(20) COLLATE utf8mb4_bin NOT NULL, member_name VARCHAR(150) COLLATE utf8mb4_bin NOT NULL, member_birthdate DATE NOT NULL, gender VARCHAR(7) NOT NULL, member_nationality VARCHAR(50) NOT NULL, join_date DATE NOT NULL, reward_type VARCHAR(30) NOT NULL, member_tier VARCHAR(30) NOT NULL, mobile_number VARCHAR(13) DEFAULT 'N/A', email VARCHAR(100) DEFAULT 'N/A', total_purchase INT NOT NULL DEFAULT 0, current_purchase INT NOT NULL DEFAULT 0, points INT NOT NULL DEFAULT 0, PRIMARY KEY (member_id))";
                    command.ExecuteNonQuery();
                    command.CommandText = "CREATE TABLE IF NOT EXISTS admins(admin_username VARCHAR(100) COLLATE utf8mb4_bin NOT NULL, salt VARCHAR(150) NOT NULL, hash VARCHAR(150) NOT NULL, can_add_products BOOLEAN NOT NULL DEFAULT FALSE, can_remove_products BOOLEAN NOT NULL DEFAULT FALSE, can_update_products BOOLEAN NOT NULL DEFAULT FALSE, can_add_menus BOOLEAN NOT NULL DEFAULT FALSE, can_remove_menus BOOLEAN NOT NULL DEFAULT FALSE, can_update_menus BOOLEAN NOT NULL DEFAULT FALSE, can_add_menucontents BOOLEAN NOT NULL DEFAULT FALSE, can_remove_menucontents BOOLEAN NOT NULL DEFAULT FALSE, can_add_members BOOLEAN NOT NULL DEFAULT FALSE, can_remove_members BOOLEAN NOT NULL DEFAULT FALSE, can_update_members BOOLEAN NOT NULL DEFAULT FALSE, can_view_history BOOLEAN NOT NULL DEFAULT FALSE, can_modify_rates BOOLEAN NOT NULL DEFAULT FALSE, can_modify_disc BOOLEAN NOT NULL DEFAULT FALSE, can_modify_levelup BOOLEAN NOT NULL DEFAULT FALSE, can_modify_order BOOLEAN NOT NULL DEFAULT FALSE, superadmin BOOLEAN NOT NULL DEFAULT FALSE, PRIMARY KEY (admin_username))";
                    command.ExecuteNonQuery();
                    command.CommandText = "CREATE TABLE IF NOT EXISTS loyalty_values(required_stamps INT NOT NULL DEFAULT 0, stamps_reward DOUBLE NOT NULL DEFAULT 0.0, required_points DOUBLE NOT NULL DEFAULT 0.0, points_reward DOUBLE NOT NULL DEFAULT 0.0)";
                    command.ExecuteNonQuery();
                    InitializeRewardValues();
                    command.CommandText = "CREATE TABLE IF NOT EXISTS levelup_values(silver INT NOT NULL DEFAULT 0, gold INT NOT NULL DEFAULT 0, platinum INT NOT NULL DEFAULT 0)";
                    command.ExecuteNonQuery();
                    InitializeLevelUpValues();
                    command.CommandText = "CREATE TABLE IF NOT EXISTS disc_values(bronze INT NOT NULL DEFAULT 0, silver INT NOT NULL DEFAULT 0, gold INT NOT NULL DEFAULT 0, platinum INT NOT NULL DEFAULT 0)";
                    command.ExecuteNonQuery();
                    InitializeDiscountValues();
                }
                LoadDefaultLoyaltyValues();
                LoadDefaultDiscounts();
                LoadDefaultLevelUpValues();
            }
        }

        private void ShowAdminPassword_Click(object sender, RoutedEventArgs e)
        {
            if (ShowAdminPassword.IsChecked == true)
            {
                ShownAdminPassword.Text = HiddenAdminPassword.Password;
                HiddenAdminPassword.Visibility = Visibility.Collapsed;
                ShownAdminPassword.Visibility = Visibility.Visible;
            }
            else
            {
                HiddenAdminPassword.Password = ShownAdminPassword.Text;
                HiddenAdminPassword.Visibility = Visibility.Visible;
                ShownAdminPassword.Visibility = Visibility.Collapsed;
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

        private void ClearInputButton_Click(object sender, RoutedEventArgs e)
        {
            AdminUsername.Text = "";
            HiddenAdminPassword.Password = "";
            ShownAdminPassword.Text = "";
        }

        private void LoginAdminButton_Click(object sender, RoutedEventArgs e)
        {
            string curpass = "";
            if (ShownAdminPassword.Visibility == Visibility.Visible) curpass = ShownAdminPassword.Text;
            else if (HiddenAdminPassword.Visibility == Visibility.Visible) curpass = HiddenAdminPassword.Password;
            if (string.IsNullOrEmpty(curpass) || string.IsNullOrEmpty(AdminUsername.Text))
            {
                MessageBox.Show("Please ensure that the input fields are not blank.", "Login unsuccessful", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand($"SELECT salt, hash, can_add_members, can_remove_members, can_update_members, can_modify_rates, can_modify_disc, can_modify_levelup FROM admins WHERE admin_username = @username", connect))
                {
                    command.Parameters.AddWithValue("@username", AdminUsername.Text);
                    bool found = false;
                    string cur_salt = "", cur_hash = "";
                    int can_add = 0, can_remove = 0, can_update = 0, can_modify_rate = 0, can_modify_discount = 0, can_mod_lvl = 0;
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cur_salt = reader[0]?.ToString();
                            cur_hash = reader[1]?.ToString();
                            can_add = Convert.ToInt32(reader[2]);
                            can_remove = Convert.ToInt32(reader[3]);
                            can_update = Convert.ToInt32(reader[4]);
                            can_modify_rate = Convert.ToInt32(reader[5]);
                            can_modify_discount = Convert.ToInt32(reader[6]);
                            can_mod_lvl = Convert.ToInt32(reader[7]);
                            if (string.IsNullOrEmpty(cur_salt) || string.IsNullOrEmpty(cur_hash)) { continue; }
                            if (VerifyPassword(curpass, cur_hash, cur_salt))
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                    if (found)
                    {
                        if (can_add == 0)
                        {
                            AddMemberButton.IsEnabled = false;
                            AddMemberButton.Background = new SolidColorBrush(Colors.Gray);
                        }    
                        else
                        {
                            AddMemberButton.IsEnabled = true;
                            AddMemberButton.Background = new SolidColorBrush(Colors.Transparent);
                        }
                        if (can_remove == 0)
                        {
                            RemoveMemberButton.IsEnabled = false;
                            RemoveMemberButton.Background = new SolidColorBrush(Colors.Gray);
                        }
                        else
                        {
                            RemoveMemberButton.IsEnabled = true;
                            RemoveMemberButton.Background = new SolidColorBrush(Colors.Transparent);
                        }
                        if (can_update == 0)
                        {
                            UpdateMemberButton.IsEnabled = false;
                            UpdateMemberButton.Background = new SolidColorBrush(Colors.Gray);
                        }
                        else
                        {
                            UpdateMemberButton.IsEnabled = true;
                            UpdateMemberButton.Background = new SolidColorBrush(Colors.Transparent);
                        }
                        if (can_modify_rate == 0)
                        {
                            UpdatePointsReward.IsEnabled = false;
                            UpdatePointsReward.Background = new SolidColorBrush(Colors.Gray);
                            UpdateRequiredPoints.IsEnabled = false;
                            UpdateRequiredPoints.Background = new SolidColorBrush(Colors.Gray);
                            UpdateStampsReward.IsEnabled = false;
                            UpdateStampsReward.Background = new SolidColorBrush(Colors.Gray);
                            UpdateRequiredStamps.IsEnabled = false;
                            UpdateRequiredStamps.Background = new SolidColorBrush(Colors.Gray);
                        }
                        else
                        {
                            UpdatePointsReward.IsEnabled = true;
                            UpdatePointsReward.Background = new SolidColorBrush(Colors.Transparent);
                            UpdateRequiredPoints.IsEnabled = true;
                            UpdateRequiredPoints.Background = new SolidColorBrush(Colors.Transparent);
                            UpdateStampsReward.IsEnabled = true;
                            UpdateStampsReward.Background = new SolidColorBrush(Colors.Transparent);
                            UpdateRequiredStamps.IsEnabled = true;
                            UpdateRequiredStamps.Background = new SolidColorBrush(Colors.Transparent);
                        }
                        if (can_modify_discount == 0)
                        {
                            UpdateBronzeDiscount.IsEnabled = false;
                            UpdateBronzeDiscount.Background = new SolidColorBrush(Colors.Gray);
                            UpdateSilverDiscount.IsEnabled = false;
                            UpdateSilverDiscount.Background = new SolidColorBrush(Colors.Gray);
                            UpdateGoldDiscount.IsEnabled = false;
                            UpdateGoldDiscount.Background = new SolidColorBrush(Colors.Gray);
                            UpdatePlatinumDiscount.IsEnabled = false;
                            UpdatePlatinumDiscount.Background = new SolidColorBrush(Colors.Gray);
                        }
                        else
                        {
                            UpdateBronzeDiscount.IsEnabled = true;
                            UpdateBronzeDiscount.Background = new SolidColorBrush(Colors.Transparent);
                            UpdateSilverDiscount.IsEnabled = true;
                            UpdateSilverDiscount.Background = new SolidColorBrush(Colors.Transparent);
                            UpdateGoldDiscount.IsEnabled = true;
                            UpdateGoldDiscount.Background = new SolidColorBrush(Colors.Transparent);
                            UpdatePlatinumDiscount.IsEnabled = true;
                            UpdatePlatinumDiscount.Background = new SolidColorBrush(Colors.Transparent);
                        }
                        if (can_mod_lvl == 0) 
                        {
                            UpdateSilverLevelUp.IsEnabled = false;
                            UpdateSilverLevelUp.Background = new SolidColorBrush(Colors.Gray);
                            UpdateGoldLevelUp.IsEnabled = false;
                            UpdateGoldLevelUp.Background= new SolidColorBrush(Colors.Gray);
                            UpdatePlatinumLevelUp.IsEnabled = false;
                            UpdatePlatinumLevelUp.Background = new SolidColorBrush(Colors.Gray);
                        }
                        else
                        {
                            UpdateSilverLevelUp.IsEnabled = true;
                            UpdateSilverLevelUp.Background = new SolidColorBrush(Colors.Transparent);
                            UpdateGoldLevelUp.IsEnabled = true;
                            UpdateGoldLevelUp.Background = new SolidColorBrush(Colors.Transparent);
                            UpdatePlatinumLevelUp.IsEnabled = true;
                            UpdatePlatinumLevelUp.Background = new SolidColorBrush(Colors.Transparent);
                        }
                        LoginAdmin.Visibility = Visibility.Collapsed;
                        MemberDashboard.Visibility = Visibility.Visible;
                        LoadMembers("");
                        MessageBox.Show("You have successfully logged into the system.", "Login successful", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    MessageBox.Show("Please ensure that the credentials are valid.", "Login unsuccessful", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to log out?", "Confirm logout", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                MemberDashboard.Visibility = Visibility.Collapsed;
                LoginAdmin.Visibility = Visibility.Visible;
                MessageBox.Show("You have successfully logged out.", "Logged out successfully", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ClearInputButton2_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => ClearMemberInputs(false);

        private void ClearInputButton2_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
            ClearMemberInputs(true);
        }

        private void ClearMemberInputs(bool setToSentinel)
        {
            MemberID.Text = "";
            FullName.Text = "";
            Birthdate.SelectedDate = null;
            Nationality.Text = "";
            MobileNumber.Text = "";
            EmailAddress.Text = "";
            if (setToSentinel)
            {
                Gender.SelectedIndex = 0;
                MemberTier.SelectedIndex = 0;
                RewardType.SelectedIndex = 0;
            }
            else
            {
                Gender.SelectedIndex = 1;
                MemberTier.SelectedIndex = 1;
                RewardType.SelectedIndex = 1;
            }
        }

        private void LoadMembers(string target)
        {
            if (target.Contains("\\") || target.Contains("/") || string.IsNullOrWhiteSpace(target)) { target = "%"; }
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand($"SELECT member_id AS 'Member ID', member_name AS 'Name', member_birthdate AS 'Birthdate', gender AS 'Gender', member_nationality AS 'Nationality', join_date AS 'Join Date', reward_type AS 'Reward Type', member_tier AS 'Member Tier', mobile_number AS 'Mobile Number', email AS 'E-mail Address', total_purchase AS 'Total Purchases', current_purchase AS 'Current Purchases', points AS 'Points' FROM members WHERE member_id LIKE @search OR member_name LIKE @search", connect))
                {
                    command.Parameters.AddWithValue("@search", target);
                    DataTable table = new DataTable();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command)) adapter.Fill(table);
                    MemberGrid.ItemsSource = table.DefaultView;
                }
            }
        }

        private void MemberGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MemberGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)MemberGrid.SelectedItem;
                MemberID.Text = selectedRow["Member ID"].ToString();
                FullName.Text = selectedRow["Name"].ToString();
            }
        }

        private void LoadMemberButton_Click(object sender, RoutedEventArgs e)
        {
            if (MemberGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)MemberGrid.SelectedItem;
                MemberID.Text = selectedRow["Member ID"].ToString();
                FullName.Text = selectedRow["Name"].ToString();
                Birthdate.SelectedDate = (DateTime)selectedRow["Birthdate"];
                Nationality.Text = selectedRow["Nationality"].ToString();
                Gender.Text = selectedRow["Gender"].ToString();
                RewardType.Text = selectedRow["Reward Type"].ToString();
                MemberTier.Text = selectedRow["Member Tier"].ToString();
                MobileNumber.Text = selectedRow["Mobile Number"].ToString();
                EmailAddress.Text = selectedRow["E-mail Address"].ToString();
            }
        }

        private void SearchMemberButton_Click(object sender, RoutedEventArgs e)
        {
            if (UseID.IsChecked == true) LoadMembers(MemberID.Text);
            else LoadMembers(FullName.Text);
        }

        private bool ValidMobileNumber(string number)
        {
            foreach (char c in number)
            {
                if (!char.IsDigit(c)) return false;
            }
            return true;
        }

        private void AddMemberButton_Click(object sender, RoutedEventArgs e)
        {
            Random rand = new Random();
            StringBuilder id = new StringBuilder();
            if (string.IsNullOrWhiteSpace(FullName.Text) || FullName.Text.Contains('-') || string.IsNullOrWhiteSpace(Nationality.Text) || Birthdate.SelectedDate == null)
            {
                MessageBox.Show("Please ensure that all the required inputs are valid.", "Member registration unsuccessful", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!string.IsNullOrWhiteSpace(MobileNumber.Text) && (!MobileNumber.Text.StartsWith("639") || MobileNumber.Text.Length != 12 || !ValidMobileNumber(MobileNumber.Text)))
            {
                MessageBox.Show("Please ensure that the mobile number contains +639, and the mobile number contains 12 numbers only.", "Invalid mobile number", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!string.IsNullOrWhiteSpace(EmailAddress.Text) && (!EmailAddress.Text.Contains("@") || !EmailAddress.Text.EndsWith(".com")))
            {
                MessageBox.Show("Please ensure that the e-mail address contains @, and ends with .com.", "Invalid e-mail address", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string gender = Gender.SelectedIndex == 0 ? "Male" : Gender.Text;
            string tier = MemberTier.SelectedIndex == 0 ? "Bronze" : MemberTier.Text;
            string reward = RewardType.SelectedIndex == 0 ? "Points" : RewardType.Text;
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT EXISTS(SELECT 1 FROM members WHERE member_name = @name LIMIT 1)", connect))
                {
                    command.Parameters.AddWithValue("@name", FullName.Text);
                    int res = Convert.ToInt32(command.ExecuteScalar());
                    if (res > 0)
                    {
                        MessageBox.Show("This person has already been registered into the system.", "Member registration unsuccessful", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    DateTime now = DateTime.Now;
                    string temp = "";
                    int p1 = rand.Next(100000), p2 = rand.Next(100000), p3 = rand.Next(100000);
                    temp = p1.ToString();
                    switch (temp.Length)
                    {
                        case 1: id.Append("0000"); break;
                        case 2: id.Append("000"); break;
                        case 3: id.Append("00"); break;
                        case 4: id.Append("0"); break;
                    }
                    id.Append(temp).Append('-');
                    temp = p2.ToString();
                    switch (temp.Length)
                    {
                        case 1: id.Append("0000"); break;
                        case 2: id.Append("000"); break;
                        case 3: id.Append("00"); break;
                        case 4: id.Append("0"); break;
                    }
                    id.Append(temp).Append('-');
                    temp = p3.ToString();
                    switch (temp.Length)
                    {
                        case 1: id.Append("0000"); break;
                        case 2: id.Append("000"); break;
                        case 3: id.Append("00"); break;
                        case 4: id.Append("0"); break;
                    }
                    id.Append(temp);
                    command.CommandText = "INSERT into members VALUES(@id, @name, @birthdate, @gen, @national, @join, @reward, @tier, @number, @n_email, 0, 0, 0)";
                    command.Parameters.AddWithValue("@id", id.ToString());
                    command.Parameters.AddWithValue("@birthdate", Birthdate.SelectedDate.Value);
                    command.Parameters.AddWithValue("@gen", gender);
                    command.Parameters.AddWithValue("@national", Nationality.Text);
                    command.Parameters.AddWithValue("@join", now);
                    command.Parameters.AddWithValue("@reward", reward);
                    command.Parameters.AddWithValue("@tier", tier);
                    command.Parameters.AddWithValue("@number", !string.IsNullOrWhiteSpace(MobileNumber.Text) ? MobileNumber.Text : "N/A");
                    command.Parameters.AddWithValue("@n_email", !string.IsNullOrWhiteSpace(EmailAddress.Text) ? EmailAddress.Text : "N/A");
                    command.ExecuteNonQuery();
                    LoadMembers("");
                    MemberID.Text = "";
                    FullName.Text = "";
                    Birthdate.SelectedDate = null;
                    Nationality.Text = "";
                    MobileNumber.Text = "";
                    EmailAddress.Text = "";
                    MessageBox.Show("This person has been successfully registered into the system.", "Member registration successful", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
        }

        private void RemoveMemberButton_Click(object sender, RoutedEventArgs e)
        {
            string target = "";
            if (UseName.IsChecked == true) target = FullName.Text;
            else target = MemberID.Text;
            if (string.IsNullOrEmpty(target))
            {
                MessageBox.Show("Please ensure that the member ID or member name is not blank.", "Missing input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand($"SELECT EXISTS(SELECT 1 FROM members WHERE member_id = @target OR member_name = @target LIMIT 1)", connect, transaction))
                    {
                        try
                        {
                            command.Parameters.AddWithValue("@target", target);
                            int res = Convert.ToInt32(command.ExecuteScalar());
                            if (res == 0)
                            {
                                MessageBox.Show("Please check if the indicated member exists in the system", "Remove operation failed", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }
                            command.CommandText = "SELECT * FROM members WHERE member_id = @target OR member_name LIMIT 1 FOR UPDATE";
                            command.ExecuteNonQuery();
                            command.CommandText = "DELETE FROM members WHERE member_id = @target OR member_name = @target LIMIT 1";
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            LoadMembers("");
                            MemberID.Text = "";
                            FullName.Text = "";
                            MessageBox.Show("The specified member has been deleted from the system.", "Member removed successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            MessageBox.Show("The specified member has not been deleted, please try again.", "Member remove failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
        }

        private void UpdateMemberButton_Click(object sender, RoutedEventArgs e)
        {
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand($"SELECT EXISTS(SELECT 1 FROM members WHERE member_id = @id)", connect, transaction))
                    {
                        try
                        {
                            command.Parameters.AddWithValue("@id", MemberID.Text);
                            StringBuilder successful_ops = new StringBuilder();
                            int res = Convert.ToInt32(command.ExecuteScalar());
                            if (res == 0)
                            {
                                MessageBox.Show("Please check if the indicated member exists in the system", "Update operation failed", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }
                            command.CommandText = "SELECT * FROM members WHERE member_id = @id FOR UPDATE";
                            // Lock the row
                            command.ExecuteNonQuery();
                            if (!string.IsNullOrWhiteSpace(FullName.Text))
                            {
                                command.CommandText = "UPDATE members SET member_name = @new_user WHERE member_id = @id";
                                command.Parameters.AddWithValue("@new_user", FullName.Text);
                                command.ExecuteNonQuery();
                                FullName.Text = "";
                                if (successful_ops.Length > 0) successful_ops.Append(", Username");
                                else successful_ops.Append("Username");
                            }
                            if (Birthdate.SelectedDate != null)
                            {
                                command.CommandText = "UPDATE members SET member_birthdate = @new_date WHERE member_id = @id";
                                command.Parameters.AddWithValue("@new_date", Birthdate.SelectedDate.Value);
                                command.ExecuteNonQuery();
                                Birthdate.SelectedDate = null;
                                if (successful_ops.Length > 0) successful_ops.Append(", Birthdate");
                                else successful_ops.Append("Birthdate");
                            }
                            if (!string.IsNullOrWhiteSpace(Nationality.Text))
                            {
                                command.CommandText = "UPDATE members SET member_nationality = @new_nat WHERE member_id = @id";
                                command.Parameters.AddWithValue("@new_nat", Nationality.Text);
                                command.ExecuteNonQuery();
                                Nationality.Text = "";
                                if (successful_ops.Length > 0) successful_ops.Append(", Nationality");
                                else successful_ops.Append("Nationality");
                            }
                            if (!string.IsNullOrWhiteSpace(MobileNumber.Text))
                            {
                                if (!MobileNumber.Text.StartsWith("639") || MobileNumber.Text.Length != 12 || !ValidMobileNumber(MobileNumber.Text))
                                {
                                    MessageBox.Show("Please ensure that the mobile number contains +639, and the mobile number contains 12 numbers only.", "Invalid mobile number", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    return;
                                }
                                command.CommandText = "UPDATE members SET mobile_number = @new_mobile WHERE member_id = @id";
                                command.Parameters.AddWithValue("@new_mobile", MobileNumber.Text);
                                command.ExecuteNonQuery();
                                MobileNumber.Text = "";
                                if (successful_ops.Length > 0) successful_ops.Append(", Mobile Number");
                                else successful_ops.Append("Mobile Number");
                            }
                            if (!string.IsNullOrWhiteSpace(EmailAddress.Text))
                            {
                                if (!EmailAddress.Text.Contains("@") || !EmailAddress.Text.EndsWith(".com"))
                                {
                                    MessageBox.Show("Please ensure that the e-mail address contains @, and ends with .com.", "Invalid e-mail address", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    return;
                                }
                                command.CommandText = "UPDATE members SET email = @new_email WHERE member_id = @id";
                                command.Parameters.AddWithValue("@new_email", EmailAddress.Text);
                                command.ExecuteNonQuery();
                                EmailAddress.Text = "";
                                if (successful_ops.Length > 0) successful_ops.Append(", E-mail Address");
                                else successful_ops.Append("E-mail Address");
                            }
                            if (Gender.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE members SET gender = @new_gen WHERE member_id = @id";
                                command.Parameters.AddWithValue("@new_gen", Gender.SelectedItem);
                                command.ExecuteNonQuery();
                                Gender.SelectedIndex = 0;
                                if (successful_ops.Length > 0) successful_ops.Append(", Gender");
                                else successful_ops.Append("Gender");
                            }
                            if (RewardType.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE members SET reward_type = @new_type WHERE member_id = @id";
                                command.Parameters.AddWithValue("@new_type", RewardType.SelectedItem);
                                command.ExecuteNonQuery();
                                RewardType.SelectedIndex = 0;
                                if (successful_ops.Length > 0) successful_ops.Append(", Reward Type");
                                else successful_ops.Append("Reward Type");
                            }
                            if (MemberTier.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE members SET member_tier = @new_tier WHERE member_id = @id";
                                command.Parameters.AddWithValue("@new_tier", MemberTier.SelectedItem);
                                command.ExecuteNonQuery();
                                MemberTier.SelectedIndex = 0;
                                if (successful_ops.Length > 0) successful_ops.Append(", Member Tier");
                                else successful_ops.Append("Member Tier");
                            }
                            transaction.Commit();
                            LoadMembers("");
                            MemberID.Text = "";
                            MessageBox.Show($"The following details have been altered: {successful_ops}", "Update operation successful", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"The selected member has not been updated, please try again.", "Update operation failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
        }

        private void UseID_Click(object sender, RoutedEventArgs e) => UseName.IsChecked = !UseID.IsChecked;

        private void UseName_Click(object sender, RoutedEventArgs e) => UseID.IsChecked = !UseName.IsChecked;

        private void InitializeRewardValues()
        {
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    // When locking a table, it is golden standard to provide a timeout for automatic unlocking
                    // This will help avoid deadlocks
                    // To lock a table, use GET_LOCK('table_name', seconds)
                    // Table-locking is NOT tied to the transaction, it needs explicit unlocking
                    using (MySqlCommand command = new MySqlCommand("SELECT GET_LOCK('loyalty values', 15)", connect, transaction))
                    {
                        try
                        {
                            // Lock the entire table for 10 seconds, when 10 seconds has passed, unlock automatically
                            command.ExecuteNonQuery();
                            command.CommandText = "SELECT COUNT(*) FROM loyalty_values LIMIT 2";
                            int rowCount = Convert.ToInt32(command.ExecuteScalar());
                            if (rowCount == 0)
                            {
                                command.CommandText = "INSERT INTO loyalty_values VALUES(0, 0.0, 0, 0.0)";
                                command.ExecuteNonQuery();
                            }
                            else if (rowCount > 1)
                            {
                                command.CommandText = "DELETE FROM loyalty_values";
                                command.ExecuteNonQuery();
                                command.CommandText = "INSERT INTO loyalty_values VALUES(0, 0.0, 0, 0.0)";
                                command.ExecuteNonQuery();
                            }
                            transaction.Commit();
                            // When the operation is done, immediately unlock the table
                            command.CommandText = "SELECT RELEASE_LOCK('loyalty values')";
                            command.ExecuteNonQuery();
                        }
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            command.CommandText = "SELECT RELEASE_LOCK('loyalty values')";
                            command.ExecuteNonQuery();                         
                        }
                    }
                }
            }
        }

        private void LoadDefaultLoyaltyValues()
        {
            InitializeRewardValues();
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT required_stamps, required_points, stamps_reward, points_reward FROM loyalty_values LIMIT 1", connect))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string required_s = reader[0]?.ToString();
                            CurrentStampsRequired.Text = $"{required_s} stamps";
                            CurrentPointsRequired.Text = $"{reader[1]?.ToString()} points";
                            CurrentStampsReward.Text = $"{reader[2]?.ToString()} PHP";
                            CurrentPointsReward.Text = $"{reader[3]?.ToString()} PHP";
                            StampRewardText.Text = $"PHP per {required_s} stamps";
                        }
                    }
                }
            }
        }

        private void UpdatePointsReward_Click(object sender, RoutedEventArgs e)
        {
            double p_reward = 0.0;
            if (!double.TryParse(NewPointsReward.Text, out p_reward) || p_reward < 0.0)
            {
                MessageBox.Show("Please ensure that the value for points reward is valid.", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            InitializeRewardValues();
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT GET_LOCK('loyalty_values', 10)", connect, transaction))
                    {
                        try
                        {
                            // Lock the table for 10 seconds
                            command.ExecuteNonQuery();
                            command.CommandText = "UPDATE loyalty_values SET points_reward = @new_p_reward LIMIT 1";
                            command.Parameters.AddWithValue("@new_p_reward", p_reward);
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            command.CommandText = "SELECT RELEASE_LOCK('loyalty_values')";
                            // Unlock the table
                            command.ExecuteNonQuery();
                            CurrentPointsReward.Text = $"{NewPointsReward.Text} PHP";
                            NewPointsReward.Text = "";
                        }
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            command.CommandText = "SELECT RELEASE_LOCK('loyalty_values')";
                            command.ExecuteNonQuery();                          
                        }

                    }
                }
            }
        }

        private void UpdateStampsReward_Click(object sender, RoutedEventArgs e)
        {
            double s_reward = 0.0;
            if (!double.TryParse(NewStampsReward.Text, out s_reward) || s_reward < 0.0)
            {
                MessageBox.Show("Please ensure that the value for stamps reward is valid.", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            InitializeRewardValues();
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT GET_LOCK('loyalty_values', 10)", connect, transaction))
                    {
                        try
                        {
                            // Lock the table for 10 seconds
                            command.ExecuteNonQuery();
                            command.CommandText = "UPDATE loyalty_values SET stamps_reward = @new_s_reward LIMIT 1";
                            command.Parameters.AddWithValue("@new_s_reward", s_reward);
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            command.CommandText = "SELECT RELEASE_LOCK('loyalty_values')";
                            // Unlock the table
                            command.ExecuteNonQuery();
                            CurrentStampsReward.Text = $"{NewStampsReward.Text} PHP";
                            NewStampsReward.Text = "";
                        }
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            command.CommandText = "SELECT RELEASE_LOCK('loyalty_values')";
                            // Unlock the table
                            command.ExecuteNonQuery();                        
                        }
                    }
                }
            }
        }

        private void UpdateRequiredPoints_Click(object sender, RoutedEventArgs e)
        {
            double p_req = 0.0;
            if (!double.TryParse(NewPointsRequired.Text, out p_req) || p_req < 0.0)
            {
                MessageBox.Show("Please ensure that the value for required points is valid.", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            InitializeRewardValues();
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT GET_LOCK('loyalty_values', 10)", connect, transaction))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                            command.CommandText = "UPDATE loyalty_values SET required_points = @new_p_req LIMIT 1";
                            command.Parameters.AddWithValue("@new_p_req", p_req);
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            command.CommandText = "SELECT RELEASE_LOCK('loyalty_values')";
                            command.ExecuteNonQuery();
                            CurrentPointsRequired.Text = $"{NewPointsRequired.Text} points";
                            NewPointsRequired.Text = "";
                        }
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            command.CommandText = "SELECT RELEASE_LOCK('loyalty_values')";
                            command.ExecuteNonQuery();                      
                        }
                    }
                }
            }
        }

        private void UpdateRequiredStamps_Click(object sender, RoutedEventArgs e)
        {
            int s_req = 0;
            if (!int.TryParse(NewStampsRequired.Text, out s_req) || s_req < 0)
            {
                MessageBox.Show("Please ensure that the value for required stamps is valid.", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            InitializeRewardValues();
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT GET_LOCK('loyalty_values', 10)", connect, transaction))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                            command.CommandText = "UPDATE loyalty_values SET required_stamps = @new_s_req LIMIT 1";
                            command.Parameters.AddWithValue("@new_s_req", s_req);
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            command.CommandText = "SELECT RELEASE_LOCK('loyalty_values')";
                            command.ExecuteNonQuery();
                            CurrentStampsRequired.Text = $"{NewStampsRequired.Text} stamps";
                            StampRewardText.Text = $"PHP per {s_req} stamps";
                            NewStampsRequired.Text = "";
                        }
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            command.CommandText = "SELECT RELEASE_LOCK('loyalty_values')";
                            command.ExecuteNonQuery();                       
                        }
                    }
                }
            }
        }

        private void InitializeDiscountValues()
        {
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT GET_LOCK('disc_values', 10)", connect, transaction))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                            command.CommandText = "SELECT COUNT(*) FROM disc_values LIMIT 2";
                            int rowCount = Convert.ToInt32(command.ExecuteScalar());
                            if (rowCount == 0)
                            {
                                command.CommandText = "INSERT INTO disc_values VALUES(0, 0, 0, 0)";
                                command.ExecuteNonQuery();
                            }
                            else if (rowCount > 1)
                            {
                                command.CommandText = "DELETE FROM disc_values";
                                command.ExecuteNonQuery();
                                command.CommandText = "INSERT INTO disc_values VALUES(0, 0, 0, 0)";
                                command.ExecuteNonQuery();
                            }
                            transaction.Commit();
                            command.CommandText = "SELECT RELEASE_LOCK('disc_values')";
                            command.ExecuteNonQuery();
                        }
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            command.CommandText = "SELECT RELEASE_LOCK('disc_values')";
                            command.ExecuteNonQuery();                          
                        }
                    }
                }
            }
        }

        private void LoadDefaultDiscounts()
        {
            InitializeDiscountValues();
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT bronze, silver, gold, platinum FROM disc_values LIMIT 1", connect))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CurrentBronzeDiscount.Text = $"{reader[0]?.ToString()}%";
                            CurrentSilverDiscount.Text = $"{reader[1]?.ToString()}%";
                            CurrentGoldDiscount.Text = $"{reader[2]?.ToString()}%";
                            CurrentPlatinumDiscount.Text = $"{reader[3]?.ToString()}%";
                        }
                    }
                }
            }
        }

        private void UpdateBronzeDiscount_Click(object sender, RoutedEventArgs e)
        {
            InitializeDiscountValues();
            int newBronzeDisc = 0;
            if (!int.TryParse(NewBronzeDiscount.Text, out newBronzeDisc) || newBronzeDisc < 0 || newBronzeDisc > 100)
            {
                MessageBox.Show("Please use a valid number for the bronze discount value.", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT GET_LOCK('disc_values', 10)", connect, transaction))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                            command.CommandText = "UPDATE disc_values SET bronze = @newBronze LIMIT 1";
                            command.Parameters.AddWithValue("@newBronze", newBronzeDisc);
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            command.CommandText = "SELECT RELEASE_LOCK('disc_values')";
                            command.ExecuteNonQuery();
                            CurrentBronzeDiscount.Text = $"{newBronzeDisc}%";
                            NewBronzeDiscount.Text = "";
                            
                        } 
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            command.CommandText = "SELECT RELEASE_LOCK('disc_values')";
                            command.ExecuteNonQuery();                         
                        }
                    }
                }
            }
        }

        private void UpdateSilverDiscount_Click(object sender, RoutedEventArgs e)
        {
            InitializeDiscountValues();
            int newSilverDisc = 0;
            if (!int.TryParse(NewSilverDiscount.Text, out newSilverDisc) || newSilverDisc < 0 || newSilverDisc > 100)
            {
                MessageBox.Show("Please use a valid number for the silver discount value.", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT GET_LOCK('disc_values', 10)", connect, transaction))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                            command.CommandText = "UPDATE disc_values SET silver = @newSilver LIMIT 1";
                            command.Parameters.AddWithValue("@newSilver", newSilverDisc);
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            command.CommandText = "SELECT RELEASE_LOCK('disc_values')";
                            command.ExecuteNonQuery();
                            CurrentSilverDiscount.Text = $"{newSilverDisc}%";
                            NewSilverDiscount.Text = "";
                        }
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            command.CommandText = "SELECT RELEASE_LOCK('disc_values')";
                            command.ExecuteNonQuery();                            
                        }
                    }
                }
            }
        }

        private void UpdateGoldDiscount_Click(object sender, RoutedEventArgs e)
        {
            InitializeDiscountValues();
            int newGoldDisc = 0;
            if (!int.TryParse(NewGoldDiscount.Text, out newGoldDisc) || newGoldDisc < 0 || newGoldDisc > 100)
            {
                MessageBox.Show("Please use a valid number for the gold discount value.", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT GET_LOCK('disc_values', 10)", connect, transaction))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                            command.CommandText = "UPDATE disc_values SET gold = @newGold LIMIT 1";
                            command.Parameters.AddWithValue("@newGold", newGoldDisc);
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            command.CommandText = "SELECT RELEASE_LOCK('disc_values')";
                            command.ExecuteNonQuery();
                            CurrentGoldDiscount.Text = $"{newGoldDisc}%";
                            NewGoldDiscount.Text = "";
                        }
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            command.CommandText = "SELECT RELEASE_LOCK('disc_values')";
                            command.ExecuteNonQuery();                        
                        }
                    }
                }
            }
        }

        private void UpdatePlatinumDiscount_Click(object sender, RoutedEventArgs e)
        {
            InitializeDiscountValues();
            int newPlatinumDisc = 0;
            if (!int.TryParse(NewPlatinumDiscount.Text, out newPlatinumDisc) || newPlatinumDisc < 0 || newPlatinumDisc > 100)
            {
                MessageBox.Show("Please use a valid number for the platinum discount value.", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT GET_LOCK('disc_values', 10)", connect, transaction))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                            command.CommandText = "UPDATE disc_values SET platinum = @newPlat LIMIT 1";
                            command.Parameters.AddWithValue("@newPlat", newPlatinumDisc);
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            command.CommandText = "SELECT RELEASE_LOCK('disc_values')";
                            command.ExecuteNonQuery();
                            CurrentPlatinumDiscount.Text = $"{newPlatinumDisc}%";
                            NewPlatinumDiscount.Text = "";
                        }
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            command.CommandText = "SELECT RELEASE_LOCK('disc_values')";
                            command.ExecuteNonQuery();                          
                        }
                    }
                }
            }
        }

        private void InitializeLevelUpValues()
        {
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT GET_LOCK('levelup_values', 10)", connect, transaction))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                            command.CommandText = "SELECT COUNT(*) FROM levelup_values LIMIT 2";
                            int rowCount = Convert.ToInt32(command.ExecuteScalar());
                            if (rowCount == 0)
                            {
                                command.CommandText = "INSERT INTO levelup_values VALUES(0, 0, 0)";
                                command.ExecuteNonQuery();
                            }
                            else if (rowCount > 1)
                            {
                                command.CommandText = "DELETE FROM levelup_values";
                                command.ExecuteNonQuery();
                                command.CommandText = "INSERT INTO levelup_values VALUES(0, 0, 0)";
                                command.ExecuteNonQuery();
                            }
                            transaction.Commit();
                            command.CommandText = "SELECT RELEASE_LOCK('levelup_values')";
                            command.ExecuteNonQuery();
                        }
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            command.CommandText = "SELECT RELEASE_LOCK('levelup_values')";
                            command.ExecuteNonQuery();                        
                        }
                    }
                }
            }
        }

        private void LoadDefaultLevelUpValues()
        {
            InitializeLevelUpValues();
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT silver, gold, platinum FROM levelup_values LIMIT 1", connect))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CurrentSilverLevelUp.Text = $"{reader[0]?.ToString()} total purchases";
                            CurrentGoldLevelUp.Text = $"{reader[1]?.ToString()} total purchases";
                            CurrentPlatinumLevelUp.Text = $"{reader[2]?.ToString()} total purchases";
                        }
                    }
                }
            }
        }

        private void UpdateSilverLevelUp_Click(object sender, RoutedEventArgs e)
        {
            InitializeLevelUpValues();
            int newSilverLvl = 0;
            if (!int.TryParse(NewSilverLevelUp.Text, out newSilverLvl) || newSilverLvl < 0)
            {
                MessageBox.Show("Please use a valid number for the silver level-up value.", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT GET_LOCK('levelup_values', 10)", connect, transaction))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                            command.CommandText = "UPDATE levelup_values SET silver = @newSilver LIMIT 1";
                            command.Parameters.AddWithValue("@newSilver", newSilverLvl);
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            command.CommandText = "SELECT RELEASE_LOCK('levelup_values')";
                            command.ExecuteNonQuery();
                            CurrentSilverLevelUp.Text = $"{newSilverLvl} total purchases";
                            NewSilverLevelUp.Text = "";
                        }
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            command.CommandText = "SELECT RELEASE_LOCK('levelup_values')";
                            command.ExecuteNonQuery();                        
                        }
                    }
                }
            }
        }

        private void UpdateGoldLevelUp_Click(object sender, RoutedEventArgs e)
        {
            InitializeLevelUpValues();
            int newGoldLvl = 0;
            if (!int.TryParse(NewGoldLevelUp.Text, out newGoldLvl) || newGoldLvl < 0)
            {
                MessageBox.Show("Please use a valid number for the gold level-up value.", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT GET_LOCK('levelup_values', 10)", connect, transaction))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                            command.CommandText = "UPDATE levelup_values SET gold = @newGold LIMIT 1";
                            command.Parameters.AddWithValue("@newGold", newGoldLvl);
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            command.CommandText = "SELECT RELEASE_LOCK('levelup_values')";
                            command.ExecuteNonQuery();
                            CurrentGoldLevelUp.Text = $"{newGoldLvl} total purchases";
                            NewGoldLevelUp.Text = "";
                        }
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            command.CommandText = "SELECT RELEASE_LOCK('levelup_values')";
                            command.ExecuteNonQuery();                          
                        }
                    }
                }
            }
        }

        private void UpdatePlatinumLevelUp_Click(object sender, RoutedEventArgs e)
        {
            InitializeLevelUpValues();
            int newPlatinumLvl = 0;
            if (!int.TryParse(NewPlatinumLevelUp.Text, out newPlatinumLvl) || newPlatinumLvl < 0)
            {
                MessageBox.Show("Please use a valid number for the platinum level-up value.", "Invalid value", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT GET_LOCK('levelup_values', 10)", connect, transaction))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                            command.CommandText = "UPDATE levelup_values SET platinum = @newPlat LIMIT 1";
                            command.Parameters.AddWithValue("@newPlat", newPlatinumLvl);
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            command.CommandText = "SELECT RELEASE_LOCK('levelup_values')";
                            command.ExecuteNonQuery();
                            CurrentPlatinumLevelUp.Text = $"{newPlatinumLvl} total purchases";
                            NewPlatinumLevelUp.Text = "";
                        }
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            command.CommandText = "SELECT RELEASE_LOCK('levelup_values')";
                            command.ExecuteNonQuery();                       
                        }
                    }
                }
            }
        }

        private void ShowAdminPanel_Click(object sender, RoutedEventArgs e)
        {
            if (PanelRevealed)
            {
                // AdminFunctions.Opacity = 0 is better if the element must be invisible, but can still respond to mouse events
                AdminFunctions.Visibility = Visibility.Hidden;
                PanelRevealed = false;
            }
            else
            {
                AdminFunctions.Visibility = Visibility.Visible;
                PanelRevealed = true;
            }
        }

        private void ManageMembersButton_Click(object sender, RoutedEventArgs e)
        {
            ManageMembersPanel.Visibility = Visibility.Visible;
            MembershipValuesPanel.Visibility = Visibility.Hidden;
        }

        private void MemberValuesButton_Click(object sender, RoutedEventArgs e)
        {
            ManageMembersPanel.Visibility = Visibility.Hidden;
            MembershipValuesPanel.Visibility = Visibility.Visible;
        }

        private void ClearMemberInputValues_Click(object sender, RoutedEventArgs e)
        {
            NewPointsRequired.Text = "";
            NewPointsReward.Text = "";
            NewStampsRequired.Text = "";
            NewStampsReward.Text = "";
            NewBronzeDiscount.Text = "";
            NewSilverDiscount.Text = "";
            NewGoldDiscount.Text = "";
            NewPlatinumDiscount.Text = "";
            NewSilverLevelUp.Text = "";
            NewGoldLevelUp.Text = "";
            NewPlatinumLevelUp.Text = "";
        }
    }
}

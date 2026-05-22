using Konscious.Security.Cryptography;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CoffeeCatalogManager
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

    // Simple byte[] to Image converter
    public class ByteArrayToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte[] bytes = value as byte[];
            if (bytes == null || bytes.Length == 0) return null;
            BitmapImage bmp = new BitmapImage();
            using (var ms = new MemoryStream(bytes))
            {
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = ms;
                bmp.EndInit();
            }
            return bmp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MainWindow : Window
    {
        private static readonly string database_command = @"server=localhost;userid=root;password=;";
        private readonly string cs = database_command + @"database=coffeeshop_database";
        private byte[] selectedProductImage;
        private byte[] selectedMenuImage;
        private byte[] selectedFoodPlaceholder;
        private byte[] selectedMenuPlaceholder;
        private byte[] selectedOrderImage;
        private bool PanelRevealed = true;

        private int selectedMilkMenu = int.MaxValue;
        private int selectedAddonMenu = int.MaxValue;
        private int selectedSyrupMenu = int.MaxValue;
        private int selectedSweetMenu = int.MaxValue;
        private int selectedBaseMenu = int.MaxValue;
        private int selectedVariantMenu = int.MaxValue;

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

                    command.CommandText = "CREATE TABLE IF NOT EXISTS espresso_shot(price DOUBLE NOT NULL DEFAULT 0.0)";
                    command.ExecuteNonQuery();
                    InitializeShotValue();
                    command.CommandText = "CREATE TABLE IF NOT EXISTS products(product_name VARCHAR(150) COLLATE utf8mb4_bin NOT NULL DEFAULT 'None', order_mode INT NOT NULL DEFAULT 3, product_price DOUBLE NOT NULL DEFAULT 0.0, price_scale INT NOT NULL DEFAULT 0, product_image MEDIUMBLOB, product_quantity INT NOT NULL DEFAULT 0, has_quantity BOOLEAN NOT NULL DEFAULT TRUE, has_discount BOOLEAN NOT NULL DEFAULT TRUE, variants_page VARCHAR(150) NOT NULL DEFAULT 'N/A', variants_id VARCHAR(13) NOT NULL DEFAULT 'N/A', milk_page VARCHAR(150) NOT NULL DEFAULT 'N/A', milk_id VARCHAR(13) NOT NULL DEFAULT 'N/A', milk_scale_value INT NOT NULL DEFAULT 0, max_milk INT NOT NULL DEFAULT 0, milk_required INT NOT NULL DEFAULT 0, addons_page VARCHAR(150) NOT NULL DEFAULT 'N/A', addons_id VARCHAR(13) NOT NULL DEFAULT 'N/A', addon_scale_value INT NOT NULL DEFAULT 0, max_addon INT NOT NULL DEFAULT 0, addon_required INT NOT NULL DEFAULT 0, syrup_page VARCHAR(150) NOT NULL DEFAULT 'N/A', syrup_id VARCHAR(13) NOT NULL DEFAULT 'N/A', syrup_scale_value INT NOT NULL DEFAULT 0, max_syrup INT NOT NULL DEFAULT 0, syrup_required INT NOT NULL DEFAULT 0, sweetener_page VARCHAR(150) NOT NULL DEFAULT 'N/A', sweet_id VARCHAR(13) NOT NULL DEFAULT 'N/A', sweetener_scale_value INT NOT NULL DEFAULT 0, max_sweetener INT NOT NULL DEFAULT 0, sweet_required INT NOT NULL DEFAULT 0, base_page VARCHAR(150) NOT NULL DEFAULT 'N/A', base_id VARCHAR(13) NOT NULL DEFAULT 'N/A', max_base INT NOT NULL DEFAULT 0, base_required INT NOT NULL DEFAULT 0, default_espresso_shot INT NOT NULL DEFAULT 0, espresso_scale_value INT NOT NULL DEFAULT 0, max_espresso_shot INT NOT NULL DEFAULT 0, has_espresso_shot BOOLEAN NOT NULL DEFAULT FALSE, can_serve_warm BOOLEAN NOT NULL DEFAULT FALSE, temp_mode INT NOT NULL DEFAULT 1, size_mode INT NOT NULL DEFAULT 0, start_date DATE, end_date DATE, start_time TIME, end_time TIME, points_value INT NOT NULL DEFAULT 0, stamps_value INT NOT NULL DEFAULT 0, PRIMARY KEY (product_name))";
                    command.ExecuteNonQuery();
                    command.CommandText = "CREATE TABLE IF NOT EXISTS menus(menu_name VARCHAR(150) COLLATE utf8mb4_bin NOT NULL DEFAULT 'None', menu_id VARCHAR(13) NOT NULL DEFAULT 'None', order_mode INT NOT NULL DEFAULT 3, menu_image MEDIUMBLOB, is_hidden BOOLEAN NOT NULL DEFAULT FALSE, start_date DATE, end_date DATE, start_time TIME, end_time TIME)";
                    command.ExecuteNonQuery();
                    command.CommandText = "SELECT EXISTS(SELECT 1 FROM menus WHERE menu_name = 'Home' LIMIT 1)";
                    int rowCount = Convert.ToInt32(command.ExecuteScalar());
                    if (rowCount == 0)
                    {
                        command.CommandText = "INSERT INTO menus VALUES('Home', 'menu_14185951', NULL, false, NULL, NULL, NULL, NULL)";
                        command.ExecuteNonQuery();
                        command.CommandText = "CREATE TABLE menu_14185951(prod_name VARCHAR(100))";
                        command.ExecuteNonQuery();
                    }
                    command.CommandText = "CREATE TABLE IF NOT EXISTS ordering_images(image_name VARCHAR(16) PRIMARY KEY, image MEDIUMBLOB)";
                    command.ExecuteNonQuery();
                    command.CommandText = "CREATE TABLE IF NOT EXISTS transactions(receipt_number VARCHAR(23) NOT NULL, receipt_date DATETIME DEFAULT NULL, order_type VARCHAR(50) NOT NULL DEFAULT 'N/A', pwd_senior_id VARCHAR(75) NOT NULL DEFAULT 'N/A', pwd_senior_name VARCHAR(150) NOT NULL DEFAULT 'N/A', total_price DOUBLE NOT NULL DEFAULT 0.0, total_quantity INT NOT NULL DEFAULT 0, member_id VARCHAR(20) NOT NULL DEFAULT 'N/A', points_earned DOUBLE NOT NULL DEFAULT 0.0, stamps_earned INT NOT NULL DEFAULT 0, pwd_senior_disc DOUBLE NOT NULL DEFAULT 0.0, member_disc DOUBLE NOT NULL DEFAULT 0.0, total_disc DOUBLE NOT NULL DEFAULT 0.0, payment_method VARCHAR(50) NOT NULL DEFAULT 'N/A', transaction TEXT NOT NULL, PRIMARY KEY(receipt_number))";
                    command.ExecuteNonQuery();
                    command.CommandText = "CREATE TABLE IF NOT EXISTS admins(admin_username VARCHAR(100) COLLATE utf8mb4_bin NOT NULL, salt VARCHAR(150) NOT NULL, hash VARCHAR(150) NOT NULL, can_add_products BOOLEAN NOT NULL DEFAULT FALSE, can_remove_products BOOLEAN NOT NULL DEFAULT FALSE, can_update_products BOOLEAN NOT NULL DEFAULT FALSE, can_add_menus BOOLEAN NOT NULL DEFAULT FALSE, can_remove_menus BOOLEAN NOT NULL DEFAULT FALSE, can_update_menus BOOLEAN NOT NULL DEFAULT FALSE, can_add_menucontents BOOLEAN NOT NULL DEFAULT FALSE, can_remove_menucontents BOOLEAN NOT NULL DEFAULT FALSE, can_add_members BOOLEAN NOT NULL DEFAULT FALSE, can_remove_members BOOLEAN NOT NULL DEFAULT FALSE, can_update_members BOOLEAN NOT NULL DEFAULT FALSE, can_view_history BOOLEAN NOT NULL DEFAULT FALSE, can_modify_rates BOOLEAN NOT NULL DEFAULT FALSE, can_modify_disc BOOLEAN NOT NULL DEFAULT FALSE, can_modify_levelup BOOLEAN NOT NULL DEFAULT FALSE, can_modify_order BOOLEAN NOT NULL DEFAULT FALSE, superadmin BOOLEAN NOT NULL DEFAULT FALSE, PRIMARY KEY (admin_username))";
                    command.ExecuteNonQuery();
                }
            }
            PopulateTimes();
            PopulateNumberComboboxes();
            LoadMenus("");
        }

        private string RetrieveMenuID(string name)
        {
            string menuID = "";
            if (name == "N/A") return "N/A";
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT menu_id FROM menus WHERE menu_name = @mName", connect))
                {
                    command.Parameters.AddWithValue("@mName", name);
                    var tempID = command.ExecuteScalar();
                    if (tempID != DBNull.Value && tempID != null) menuID = tempID.ToString();
                }
            }
            return menuID;
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

        private void ClearAdminInputs_Click(object sender, RoutedEventArgs e)
        {
            AdminUsername.Text = "";
            ShownAdminPassword.Text = "";
            HiddenAdminPassword.Password = "";
        }

        private void LoginAdminButton_Click(object sender, RoutedEventArgs e)
        {
            string curpass = "";
            if (ShownAdminPassword.Visibility == Visibility.Visible) curpass = ShownAdminPassword.Text;
            else if (HiddenAdminPassword.Visibility == Visibility.Visible) curpass = HiddenAdminPassword.Password;
            int can_add_prod = 0, can_remove_prod = 0, can_update_prod = 0;
            int can_add_menu = 0, can_remove_menu = 0, can_update_menu = 0;
            int can_add_contents = 0, can_remove_contents = 0;

            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand($"SELECT salt, hash, can_add_products, can_remove_products, can_update_products, can_add_menus, can_remove_menus, can_update_menus, can_add_menucontents, can_remove_menucontents FROM admins WHERE admin_username = @username", connect))
                {
                    bool found = false;
                    command.Parameters.AddWithValue("@username", AdminUsername.Text);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        string hash = "", salt = "";
                        while (reader.Read())
                        {
                            hash = reader[1]?.ToString();
                            salt = reader[0]?.ToString();
                            can_add_prod = Convert.ToInt32(reader[2]);
                            can_remove_prod = Convert.ToInt32(reader[3]);
                            can_update_prod = Convert.ToInt32(reader[4]);
                            can_add_menu = Convert.ToInt32(reader[5]);
                            can_remove_menu = Convert.ToInt32(reader[6]);
                            can_update_menu = Convert.ToInt32(reader[7]);
                            can_add_contents = Convert.ToInt32(reader[8]);
                            can_remove_contents = Convert.ToInt32(reader[9]);
                            if (!string.IsNullOrEmpty(hash) && !string.IsNullOrEmpty(salt))
                            {
                                if (VerifyPassword(curpass, hash, salt))
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                        if (found)
                        {
                            AdminUsername.Text = "";
                            ShownAdminPassword.Text = "";
                            HiddenAdminPassword.Password = "";
                            if (can_add_prod == 1)
                            {
                                AddProduct.IsEnabled = true;
                                AddProduct.Background = new SolidColorBrush(Colors.Transparent);
                            }
                            else
                            {
                                AddProduct.IsEnabled = false;
                                AddProduct.Background = new SolidColorBrush(Colors.Gray);
                            }
                            if (can_remove_prod == 1)
                            {
                                RemoveProduct.IsEnabled = true;
                                RemoveProduct.Background = new SolidColorBrush(Colors.Transparent);
                            }
                            else
                            {
                                RemoveProduct.IsEnabled = false;
                                RemoveProduct.Background = new SolidColorBrush(Colors.Gray);
                            }
                            if (can_update_prod == 1)
                            {
                                UpdateProduct.IsEnabled = true;
                                UpdateProduct.Background = new SolidColorBrush(Colors.Transparent);
                            }
                            else
                            {
                                UpdateProduct.IsEnabled = false;
                                UpdateProduct.Background = new SolidColorBrush(Colors.Gray);
                            }
                            if (can_add_menu == 1)
                            {
                                AddMenu.IsEnabled = true;
                                AddMenu.Background = new SolidColorBrush(Colors.Transparent);
                            }
                            else
                            {
                                AddMenu.IsEnabled = false;
                                AddMenu.Background = new SolidColorBrush(Colors.Gray);
                            }
                            if (can_remove_menu == 1)
                            {
                                RemoveMenu.IsEnabled = true;
                                RemoveMenu.Background = new SolidColorBrush(Colors.Transparent);
                            }
                            else
                            {
                                RemoveMenu.IsEnabled = false;
                                RemoveMenu.Background = new SolidColorBrush(Colors.Gray);
                            }
                            if (can_update_menu == 1)
                            {
                                UpdateMenu.IsEnabled = true;
                                UpdateMenu.Background = new SolidColorBrush(Colors.Transparent);
                            }
                            else
                            {
                                UpdateMenu.IsEnabled = false;
                                UpdateMenu.Background = new SolidColorBrush(Colors.Gray);
                            }
                            if (can_add_contents == 1)
                            {
                                AddProductToMenu.IsEnabled = true;
                                AddProductToMenu.Background = new SolidColorBrush(Colors.Transparent);
                            }
                            else
                            {
                                AddProductToMenu.IsEnabled = false;
                                AddProductToMenu.Background = new SolidColorBrush(Colors.Gray);
                            }
                            if (can_remove_contents == 1)
                            {
                                RemoveProductOnMenu.IsEnabled = true;
                                RemoveProductOnMenu.Background = new SolidColorBrush(Colors.Transparent);
                                RemoveProductAllMenus.IsEnabled = true;
                                RemoveProductAllMenus.Background = new SolidColorBrush(Colors.Transparent);
                                ClearMenuContentsButton.IsEnabled = true;
                                ClearMenuContentsButton.Background = new SolidColorBrush(Colors.Transparent);
                            }
                            else
                            {
                                RemoveProductOnMenu.IsEnabled = false;
                                RemoveProductOnMenu.Background = new SolidColorBrush(Colors.Gray);
                                RemoveProductAllMenus.IsEnabled = false;
                                RemoveProductAllMenus.Background = new SolidColorBrush(Colors.Gray);
                                ClearMenuContentsButton.IsEnabled = false;
                                ClearMenuContentsButton.Background = new SolidColorBrush(Colors.Gray);
                            }
                            CatalogDashboard.Visibility = Visibility.Visible;
                            AdminLogin.Visibility = Visibility.Collapsed;
                            MessageBox.Show("You have successfully logged into the program.", "Login successful", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                        MessageBox.Show("The admin login credentials are invalid, please try again.", "Login unsuccessful", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to log out?", "Confirm logout", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                CatalogDashboard.Visibility = Visibility.Collapsed;
                AdminLogin.Visibility = Visibility.Visible;
                MessageBox.Show("You have successfully logged out.", "Logged out successfully", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void PopulateTimes()
        {
            DateTime time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day); // Start at midnight (00:00)
            DateTime endTime = time.AddDays(1); // End at next midnight

            while (time < endTime)
            {
                string item = time.ToString("hh:mm tt");
                ProductStartTime.Items.Add(item); // 12-hour format with AM/PM
                ProductEndTime.Items.Add(item);
                MenuStartTime.Items.Add(item);
                MenuEndTime.Items.Add(item);
                time = time.AddMinutes(1); // Add 1 minute
            }
            ProductStartTime.Items.Add("Anytime");
            ProductStartTime.Items.Add("None");
            ProductEndTime.Items.Add("Anytime");
            ProductEndTime.Items.Add("None");
            MenuStartTime.Items.Add("Anytime");
            MenuStartTime.Items.Add("None");
            MenuEndTime.Items.Add("Anytime");
            MenuEndTime.Items.Add("None");
            ProductStartTime.SelectedIndex = ProductStartTime.Items.Count - 1; // Default selection
            ProductEndTime.SelectedIndex = ProductEndTime.Items.Count - 1;
            MenuStartTime.SelectedIndex = MenuStartTime.Items.Count - 1;
            MenuEndTime.SelectedIndex = MenuEndTime.Items.Count - 1;
        }

        private void PopulateNumberComboboxes()
        {
            List<int> choices = new List<int>() { -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99 };
            MaxMilk.ItemsSource = choices;
            MaxMilk.SelectedIndex = 0;
            MilkScale.ItemsSource = choices;
            MilkScale.SelectedIndex = 0;
            MaxAddon.ItemsSource = choices;
            MaxAddon.SelectedIndex = 0;
            AddonScale.ItemsSource = choices;
            AddonScale.SelectedIndex = 0;
            MaxSyrup.ItemsSource = choices;
            MaxSyrup.SelectedIndex = 0;
            SyrupScale.ItemsSource = choices;
            SyrupScale.SelectedIndex = 0;
            MaxSweetener.ItemsSource = choices;
            MaxSweetener.SelectedIndex = 0;
            SweetenerScale.ItemsSource = choices;
            SweetenerScale.SelectedIndex = 0;
            MaxBase.ItemsSource = choices;
            MaxBase.SelectedIndex = 0;
            DefaultEspresso.ItemsSource = choices;
            DefaultEspresso.SelectedIndex = 0;
            MaxEspresso.ItemsSource = choices;
            MaxEspresso.SelectedIndex = 0;
            EspressoScale.ItemsSource = choices;
            EspressoScale.SelectedIndex = 0;
            PriceScale.ItemsSource = choices;
            PriceScale.SelectedIndex = 0;
            PointsValue.ItemsSource = choices;
            PointsValue.SelectedIndex = 0;
            StampValue.ItemsSource = choices;
            StampValue.SelectedIndex = 0;   
        }

        private void LoadMenuComboboxes(int mode)
        {
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT menu_name FROM menus", connect))
                {
                    DataTable table = new DataTable();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command)) adapter.Fill(table);
                    if (mode == 1)
                    {
                        List<string> menuNames2 = table.AsEnumerable().Select(row => row["menu_name"].ToString()).ToList();
                        SelectedMenuSwap.ItemsSource = menuNames2;
                        SelectedMenuSwap.SelectedIndex = menuNames2.Count > 0 ? 0 : 1;
                        for (int i = 0; i < menuNames2.Count; ++i)
                        {
                            if (menuNames2[i] == "Home")
                            {
                                string temp = menuNames2[menuNames2.Count - 1];
                                menuNames2[menuNames2.Count - 1] = "Home";
                                menuNames2[i] = temp;
                                menuNames2.RemoveAt(menuNames2.Count - 1);
                                break;
                            }
                        }
                        SelectedMenuA.ItemsSource = menuNames2;
                        SelectedMenuA.SelectedIndex = menuNames2.Count > 0 ? 0 : 1;
                        SelectedMenuB.ItemsSource = menuNames2;
                        SelectedMenuB.SelectedIndex = menuNames2.Count > 0 ? 0 : 1;
                    }
                    else if (mode == 2)
                    {
                        List<string> menuNames = table.AsEnumerable().Select(row => row["menu_name"].ToString()).ToList();
                        for (int i = 0; i < menuNames.Count; ++i)
                        {
                            if (menuNames[i] == "Home")
                            {
                                string temp = menuNames[menuNames.Count - 1];
                                menuNames[menuNames.Count - 1] = "Home";
                                menuNames[i] = temp;
                                menuNames.RemoveAt(menuNames.Count - 1);
                                break;
                            }
                        }
                        menuNames.Add("N/A");
                        menuNames.Add("Do not update");
                        int last = menuNames.Count - 1;
                        MilkPage.ItemsSource = menuNames;
                        MilkPage.SelectedIndex = selectedMilkMenu < MilkPage.Items.Count ? selectedMilkMenu : last;
                        AddonPage.ItemsSource = menuNames;
                        AddonPage.SelectedIndex = selectedAddonMenu < AddonPage.Items.Count ? selectedAddonMenu : last;
                        SyrupPage.ItemsSource = menuNames;
                        SyrupPage.SelectedIndex = selectedSyrupMenu < SyrupPage.Items.Count ? selectedSyrupMenu : last;
                        SweetenerPage.ItemsSource = menuNames;
                        SweetenerPage.SelectedIndex = selectedSweetMenu < SweetenerPage.Items.Count ? selectedSweetMenu : last;
                        BasePage.ItemsSource = menuNames;
                        BasePage.SelectedIndex = selectedBaseMenu < BasePage.Items.Count ? selectedBaseMenu : last;
                        VariantPage.ItemsSource = menuNames;
                        VariantPage.SelectedIndex = selectedVariantMenu < VariantPage.Items.Count ? selectedVariantMenu : last;
                    }
                }
            }
        }

        private void LoadProductImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp" };
            dlg.Multiselect = false;

            if (dlg.ShowDialog() == true)
            {
                BitmapImage bitmap = new BitmapImage(new Uri(dlg.FileName));
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.Freeze();

                int width = bitmap.PixelWidth;
                int height = bitmap.PixelHeight;
                if (width < 64 || height < 64)
                {
                    MessageBox.Show("The selected image has too low resolution, please select another image.", "Too low image resolution", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                ProductImage.Source = bitmap;

                using (var ms = new MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(ms);
                    selectedProductImage = ms.ToArray();
                }
            }
        }

        private void RemoveProductImage_Click(object sender, RoutedEventArgs e)
        {
            ProductImage.Source = null;
            selectedProductImage = null;
        }

        private void LoadProducts(string target)
        {
            if (target.Contains("\\") || target.Contains("/") || string.IsNullOrWhiteSpace(target)) { target = "%"; }

            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT product_name AS 'Product Name', order_mode AS 'Order Mode', points_value AS 'Points Value', stamps_value AS 'Stamps Value', product_price AS 'Product Price', price_scale AS 'Price Scale', product_quantity AS 'Product Quantity', has_quantity AS 'Has Quantity', has_discount AS 'Has Discount', variants_page AS 'Variants Page', milk_page AS 'Product Milk Page', milk_scale_value AS 'Milk Scale Value', max_milk AS 'Max Milk Amount', milk_required AS 'Milk Required', addons_page AS 'Product Add-ons Page', addon_scale_value AS 'Add-ons Scale Value', max_addon AS 'Max Add-ons Amount', addon_required AS 'Add-on Required', syrup_page AS 'Product Syrup Page', syrup_scale_value AS 'Syrup Scale Value', max_syrup AS 'Max Syrup Amount', syrup_required AS 'Syrup Required', sweetener_page AS 'Product Sweetener Page', sweetener_scale_value AS 'Sweetener Scale Value', max_sweetener AS 'Max Sweetener Amount', sweet_required AS 'Sweetener Required', base_page AS 'Product Base Page', max_base AS 'Max Base Amount', base_required AS 'Base Required', default_espresso_shot AS 'Default Espresso Shot', espresso_scale_value AS 'Espresso Shot Scale Value', max_espresso_shot AS 'Max Espresso Shot', has_espresso_shot AS 'Has Espresso Shot', can_serve_warm AS 'Can Serve Warm', temp_mode AS 'Temperature Mode', size_mode AS 'Size Mode', start_date AS 'Product Start Date', end_date AS 'Product End Date', start_time AS 'Product Start Time', end_time AS 'Product End Time', product_image FROM products WHERE product_name LIKE @prod_target", connect))
                {
                    command.Parameters.AddWithValue("@prod_target", target);
                    DataTable table = new DataTable();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command)) adapter.Fill(table);

                    // Template Column for Image
                    DataGridTemplateColumn imageCol = new DataGridTemplateColumn { Header = "Product Image" };

                    // Define the Image element in code
                    FrameworkElementFactory imageFactory = new FrameworkElementFactory(typeof(Image));
                    Binding imageBinding = new Binding("product_image")
                    {
                        Converter = new ByteArrayToImageConverter()
                    };
                    imageFactory.SetBinding(Image.SourceProperty, imageBinding);
                    imageFactory.SetValue(Image.WidthProperty, 100.0); // Width = 80
                    imageFactory.SetValue(Image.HeightProperty, 100.0); // Heoght = 80

                    // Create DataTemplate
                    DataTemplate cellTemplate = new DataTemplate
                    {
                        VisualTree = imageFactory
                    };
                    imageCol.CellTemplate = cellTemplate;

                    ProductsTable.ItemsSource = table.DefaultView;
                    if (ProductsTable.Columns.Count > 38 && ProductsTable.Columns[38] != null) { ProductsTable.Columns.RemoveAt(38); }
                    ProductsTable.Columns.Add(imageCol);
                }
            }
        }

        private void SearchProduct_Click(object sender, RoutedEventArgs e) => LoadProducts(ProductName.Text);

        private void ProductsTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsTable.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)ProductsTable.SelectedItem;
                string prodName = selectedRow["Product Name"].ToString();
                ProductName.Text = prodName;
            }
        }

        private void LoadProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsTable.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)ProductsTable.SelectedItem;
                string prodName = selectedRow["Product Name"].ToString();
                ProductName.Text = prodName;
                ProductPrice.Text = selectedRow["Product Price"].ToString();
                PriceScale.SelectedValue = Convert.ToInt32(selectedRow["Price Scale"]);
                ProductQuantity.Text = selectedRow["Product Quantity"].ToString();
                HasQuantity.SelectedValue = Convert.ToBoolean(selectedRow["Has Quantity"]);
                HasDiscount.SelectedValue = Convert.ToBoolean(selectedRow["Has Discount"]);
                VariantPage.SelectedValue = selectedRow["Variants Page"].ToString();
                PointsValue.SelectedValue = Convert.ToInt32(selectedRow["Points Value"]);
                StampValue.SelectedValue = Convert.ToInt32(selectedRow["Stamps Value"]);
                ProductOrderMode.SelectedValue = Convert.ToInt32(selectedRow["Order Mode"]);

                MilkPage.SelectedValue = selectedRow["Product Milk Page"].ToString();
                MaxMilk.SelectedValue = Convert.ToInt32(selectedRow["Max Milk Amount"]);
                MilkScale.SelectedValue = Convert.ToInt32(selectedRow["Milk Scale Value"]);
                MilkRequired.SelectedValue = Convert.ToInt32(selectedRow["Milk Required"]);

                AddonPage.SelectedValue = selectedRow["Product Add-ons Page"].ToString();
                MaxAddon.SelectedValue = Convert.ToInt32(selectedRow["Max Add-ons Amount"]);
                AddonScale.SelectedValue = Convert.ToInt32(selectedRow["Add-ons Scale Value"]);
                AddonRequired.SelectedValue = Convert.ToInt32(selectedRow["Add-on Required"]);

                SyrupPage.SelectedValue = selectedRow["Product Syrup Page"].ToString();
                MaxSyrup.SelectedValue = Convert.ToInt32(selectedRow["Max Syrup Amount"]);
                SyrupScale.SelectedValue = Convert.ToInt32(selectedRow["Syrup Scale Value"]);
                SyrupRequired.SelectedValue = Convert.ToInt32(selectedRow["Syrup Required"]);

                SweetenerPage.SelectedValue = selectedRow["Product Sweetener Page"].ToString();
                MaxSweetener.SelectedValue = Convert.ToInt32(selectedRow["Max Sweetener Amount"]);
                SweetenerScale.SelectedValue = Convert.ToInt32(selectedRow["Syrup Scale Value"]);
                SweetRequired.SelectedValue = Convert.ToInt32(selectedRow["Sweetener Required"]);

                BasePage.SelectedValue = selectedRow["Product Base Page"].ToString();
                MaxBase.SelectedValue = Convert.ToInt32(selectedRow["Max Base Amount"]);
                BaseRequired.SelectedValue = Convert.ToInt32(selectedRow["Base Required"]);

                DefaultEspresso.SelectedValue = Convert.ToInt32(selectedRow["Default Espresso Shot"]);
                EspressoScale.SelectedValue = Convert.ToInt32(selectedRow["Espresso Shot Scale Value"]);
                MaxEspresso.SelectedValue = Convert.ToInt32(selectedRow["Max Espresso Shot"]);
                HasEspShot.SelectedValue = Convert.ToBoolean(selectedRow["Has Espresso Shot"]);

                CanServeWarm.SelectedValue = Convert.ToBoolean(selectedRow["Can Serve Warm"]);
                TempMode.SelectedValue = Convert.ToInt32(selectedRow["Temperature Mode"]);
                SizeMode.SelectedValue = Convert.ToInt32(selectedRow["Size Mode"]);

                if (selectedRow["Product Start Date"] != DBNull.Value) ProductStartDate.SelectedDate = (DateTime)selectedRow["Product Start Date"];
                else ProductStartDate.SelectedDate = null;
                if (selectedRow["Product End Date"] != DBNull.Value) ProductEndDate.SelectedDate = (DateTime)selectedRow["Product End Date"];
                else ProductEndDate.SelectedDate = null;

                if (selectedRow["Product Start Time"] != DBNull.Value)
                {
                    TimeSpan startSpan = (TimeSpan)selectedRow["Product Start Time"];
                    DateTime dateTime = DateTime.Today.Add(startSpan);
                    ProductStartTime.SelectedValue = dateTime.ToString("hh:mm tt"); // Output format like "12:00 PM"
                }
                else ProductStartTime.SelectedValue = "Anytime";

                if (selectedRow["Product End Time"] != DBNull.Value)
                {
                    TimeSpan endSpan = (TimeSpan)selectedRow["Product End Time"];
                    DateTime dateTime = DateTime.Today.Add(endSpan);
                    ProductEndTime.SelectedValue = dateTime.ToString("hh:mm tt");
                }
                else ProductEndTime.SelectedValue = "Anytime";

                byte[] imageBytes = selectedRow["product_image"] as byte[];
                if (imageBytes != null)
                {
                    selectedProductImage = imageBytes;
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = ms;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;  // Forces the loaded image to appear 
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                        ProductImage.Source = bitmapImage;
                    }
                }
                else
                {
                    selectedProductImage = null;
                    ProductImage.Source = null;
                }
            }
        }

        private void ClearProductInput_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => ClearProductInputs(false);

        private void ClearProductInput_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;  // Ignores left mouse button event
            ClearProductInputs(true);
        }

        private void ClearProductInputs(bool setToSentinel)
        {
            ProductName.Text = "";
            NewProductName.Text = "";
            ProductPrice.Text = "";
            ProductQuantity.Text = "";
            ProductImage.Source = null;
            selectedProductImage = null;
            ProductStartDate.SelectedDate = null;
            ProductEndDate.SelectedDate = null;
            if (setToSentinel)
            {
                PriceScale.SelectedIndex = 0;
                VariantPage.SelectedIndex = VariantPage.Items.Count - 1;
                ProductStartTime.SelectedIndex = ProductStartTime.Items.Count - 1;
                ProductEndTime.SelectedIndex = ProductEndTime.Items.Count - 1;
                MilkPage.SelectedIndex = MilkPage.Items.Count - 1;
                MaxMilk.SelectedIndex = 0;
                MilkScale.SelectedIndex = 0;
                MilkRequired.SelectedIndex = 0;
                AddonPage.SelectedIndex = AddonPage.Items.Count - 1;
                AddonScale.SelectedIndex = 0;
                MaxAddon.SelectedIndex = 0;
                AddonRequired.SelectedIndex = 0;
                SyrupPage.SelectedIndex = SyrupPage.Items.Count - 1;
                SyrupScale.SelectedIndex = 0;
                MaxSyrup.SelectedIndex = 0;
                SyrupRequired.SelectedIndex = 0;
                SweetenerPage.SelectedIndex = SweetenerPage.Items.Count - 1;
                SweetenerScale.SelectedIndex = 0;
                MaxSweetener.SelectedIndex = 0;
                SweetRequired.SelectedIndex = 0;
                BasePage.SelectedIndex = BasePage.Items.Count - 1;
                MaxBase.SelectedIndex = 0;
                BaseRequired.SelectedIndex = 0;
                DefaultEspresso.SelectedIndex = 0;
                MaxEspresso.SelectedIndex = 0;
                EspressoScale.SelectedIndex = 0;
                HasEspShot.SelectedIndex = 0;
                HasQuantity.SelectedIndex = 0;
                HasDiscount.SelectedIndex = 0;
                CanServeWarm.SelectedIndex = 0;
                TempMode.SelectedIndex = 0;
                SizeMode.SelectedIndex = 0;
                StampValue.SelectedIndex = 0;
                PointsValue.SelectedIndex = 0;
                ProductOrderMode.SelectedIndex = 0;
            }
            else
            {
                PriceScale.SelectedIndex = 1;
                VariantPage.SelectedIndex = VariantPage.Items.Count - 2;
                ProductStartTime.SelectedIndex = ProductStartTime.Items.Count - 2;
                ProductEndTime.SelectedIndex = ProductEndTime.Items.Count - 2;
                MilkPage.SelectedIndex = MilkPage.Items.Count - 2;
                MaxMilk.SelectedIndex = 1;
                MilkScale.SelectedIndex = 1;
                MilkRequired.SelectedIndex = 1;
                AddonPage.SelectedIndex = AddonPage.Items.Count - 2;
                AddonScale.SelectedIndex = 1;
                MaxAddon.SelectedIndex = 1;
                AddonRequired.SelectedIndex = 1;
                SyrupPage.SelectedIndex = SyrupPage.Items.Count - 2;
                SyrupScale.SelectedIndex = 1;
                MaxSyrup.SelectedIndex = 1;
                SyrupRequired.SelectedIndex = 1;
                SweetenerPage.SelectedIndex = SweetenerPage.Items.Count - 2;
                SweetenerScale.SelectedIndex = 1;
                MaxSweetener.SelectedIndex = 1;
                SweetRequired.SelectedIndex = 1;
                BasePage.SelectedIndex = BasePage.Items.Count - 2;
                MaxBase.SelectedIndex = 1;
                BaseRequired.SelectedIndex = 1;
                DefaultEspresso.SelectedIndex = 1;
                MaxEspresso.SelectedIndex = 1;
                EspressoScale.SelectedIndex = 1;
                HasEspShot.SelectedIndex = 1;
                HasQuantity.SelectedIndex = 1;
                HasDiscount.SelectedIndex = 1;
                CanServeWarm.SelectedIndex = 1;
                TempMode.SelectedIndex = 1;
                SizeMode.SelectedIndex = 1;
                StampValue.SelectedIndex = 1;
                PointsValue.SelectedIndex = 1;
                ProductOrderMode.SelectedIndex = 3;
            }
        }

        private void RemoveProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductName.Text.Length == 0)
            {
                MessageBox.Show("Please provide a valid current product name.", "Missing current product name", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand($"SELECT EXISTS(SELECT 1 FROM products WHERE product_name = @p_name)", connect, transaction))
                    {
                        try
                        {
                            command.Parameters.AddWithValue("@p_name", ProductName.Text);
                            int result = Convert.ToInt32(command.ExecuteScalar());
                            if (result == 0)
                            {
                                MessageBox.Show("The specified product does not exist in the system.", "Product does not exists in the system", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            command.CommandText = "SELECT * FROM products WHERE product_name = @p_name FOR UPDATE";
                            // Lock the row
                            command.ExecuteNonQuery();
                            command.CommandText = "DELETE FROM products WHERE product_name = @p_name";
                            command.ExecuteNonQuery();
                            // Needed since products in the menus table are not set as foreign key
                            string menuID = "";
                            using (MySqlConnection connect2 = new MySqlConnection(cs))
                            {
                                connect2.Open();
                                using (MySqlCommand command2 = new MySqlCommand("SELECT menu_id FROM menus", connect2))
                                {
                                    using (MySqlDataReader reader2 = command2.ExecuteReader())
                                    {
                                        while (reader2.Read())
                                        {
                                            menuID = reader2[0]?.ToString();
                                            command.CommandText = $"SELECT * {menuID} WHERE prod_name = @p_name LIMIT 1 FOR UPDATE";
                                            command.ExecuteNonQuery();
                                            command.CommandText = $"DELETE FROM {menuID} WHERE prod_name = @p_name";
                                            command.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }

                            transaction.Commit();
                            LoadProducts("");
                            ProductName.Text = "";
                            MessageBox.Show("The selected product has been removed from the system.", "Product successfully removed", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            MessageBox.Show("The selected product has not been removed, please try again.", "Product remove failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewProductName.Text))
            {
                MessageBox.Show("Please ensure that the product name is valid.", "Invalid product name", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int maxMilk = MaxMilk.SelectedIndex == 0 ? 0 : (int)MaxMilk.SelectedItem;
            int milkScale = MilkScale.SelectedIndex == 0 ? 0 : (int)MilkScale.SelectedItem;
            int milkRequired = MilkRequired.SelectedIndex == 0 ? 1 : (int)MilkRequired.SelectedItem;
            int maxAddon = MaxAddon.SelectedIndex == 0 ? 0 : (int)MaxAddon.SelectedItem;
            int addonScale = AddonScale.SelectedIndex == 0 ? 0 : (int)AddonScale.SelectedItem;
            int addonRequired = AddonRequired.SelectedIndex == 0 ? 1 : (int)AddonRequired.SelectedItem;
            int maxSyrup = MaxSyrup.SelectedIndex == 0 ? 0 : (int)MaxSyrup.SelectedItem;
            int syrupScale = SyrupScale.SelectedIndex == 0 ? 0 : (int)SyrupScale.SelectedItem;
            int syrupRequired = SyrupRequired.SelectedIndex == 0 ? 1 : (int)SyrupRequired.SelectedItem;
            int maxSweet = MaxSweetener.SelectedIndex == 0 ? 0 : (int)MaxSweetener.SelectedItem;
            int sweetScale = SweetenerScale.SelectedIndex == 0 ? 0 : (int)SweetenerScale.SelectedItem;
            int sweetRequired = SweetRequired.SelectedIndex == 0 ? 1 : (int)SweetRequired.SelectedItem;
            int maxBase = MaxBase.SelectedIndex == 0 ? 0 : (int)MaxBase.SelectedItem;
            int baseRequired = BaseRequired.SelectedIndex == 0 ? 1 : (int)BaseRequired.SelectedItem;
            int priceScale = PriceScale.SelectedIndex == 0 ? 0 : (int)PriceScale.SelectedItem;
            int tempMode = TempMode.SelectedIndex == 0 ? 1 : (int)TempMode.SelectedItem;
            int sizeMode = SizeMode.SelectedIndex == 0 ? 1 : (int)SizeMode.SelectedItem;
            int maxShot = MaxEspresso.SelectedIndex == 0 ? 0 : (int)MaxEspresso.SelectedItem;
            int defShot = DefaultEspresso.SelectedIndex == 0 ? 0 : (int)DefaultEspresso.SelectedItem;
            int shotScale = EspressoScale.SelectedIndex == 0 ? 0 : (int)EspressoScale.SelectedItem;
            int pointsVal = PointsValue.SelectedIndex == 0 ? 0 : (int)PointsValue.SelectedItem;
            int stampsVal = StampValue.SelectedIndex == 0 ? 0 : (int)StampValue.SelectedItem;
            int orderMode = ProductOrderMode.SelectedIndex == 0 ? 3 : (int)ProductOrderMode.SelectedItem;
            bool hasEsp = HasEspShot.SelectedIndex == 0 ? false : (bool)HasEspShot.SelectedItem;
            bool serveWarm = CanServeWarm.SelectedIndex == 0 ? false : (bool)CanServeWarm.SelectedItem;
            bool hasQuantity = HasQuantity.SelectedIndex == 0 ? false : (bool)HasQuantity.SelectedItem;
            bool hasDisc = HasDiscount.SelectedIndex == 0 ? false : (bool)HasDiscount.SelectedItem;
            int quantity = 0;
            if (!int.TryParse(ProductQuantity.Text, out quantity) || quantity < 0)
            {
                MessageBox.Show("Please make sure that the product quantity is valid.", "Invalid product quantity", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            double price = 0.0;
            if (!double.TryParse(ProductPrice.Text, out price) || price < 0.0)
            {
                MessageBox.Show("Please make sure that the product price is valid.", "Invalid product price", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string variantPage = VariantPage.SelectedIndex == VariantPage.Items.Count - 1 ? "N/A" : VariantPage.Text;
            if (!VerifyMenuName(variantPage))
            {
                MessageBox.Show("The selected variant page does not exist in the database.\nThe menu boxes are now refreshed, please select a different menu", "Non-existent menu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }           
            string milkPage = MilkPage.SelectedIndex == MilkPage.Items.Count - 1 ? "N/A" : MilkPage.Text;
            if (!VerifyMenuName(milkPage))
            {
                MessageBox.Show("The selected milk page does not exist in the database.\nThe menu boxes are now refreshed, please select a different menu", "Non-existent menu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string addonPage = AddonPage.SelectedIndex == AddonPage.Items.Count - 1 ? "N/A" : AddonPage.Text;
            if (!VerifyMenuName(addonPage))
            {
                MessageBox.Show("The selected add-ons page does not exist in the database.\nThe menu boxes are now refreshed, please select a different menu", "Non-existent menu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string syrupPage = SyrupPage.SelectedIndex == SyrupPage.Items.Count - 1 ? "N/A" : SyrupPage.Text;
            if (!VerifyMenuName(syrupPage))
            {
                MessageBox.Show("The selected syrup page does not exist in the database.\nThe menu boxes are now refreshed, please select a different menu", "Non-existent menu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string sweetPage = SweetenerPage.SelectedIndex == SweetenerPage.Items.Count - 1 ? "N/A" : SweetenerPage.Text;
            if (!VerifyMenuName(sweetPage))
            {
                MessageBox.Show("The selected sweetener page does not exist in the database.\nThe menu boxes are now refreshed, please select a different menu", "Non-existent menu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string basePage = BasePage.SelectedIndex == BasePage.Items.Count - 1 ? "N/A" : BasePage.Text;
            if (!VerifyMenuName(basePage))
            {
                MessageBox.Show("The selected base page does not exist in the database.\nThe menu boxes are now refreshed, please select a different menu", "Non-existent menu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT EXISTS(SELECT 1 FROM products WHERE product_name = @p_name)", connect))
                {
                    command.Parameters.AddWithValue("@p_name", NewProductName.Text);
                    int res = Convert.ToInt32(command.ExecuteScalar());
                    if (res > 0)
                    {
                        MessageBox.Show("A product with the same name already exists in the database.", "Product already exists", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    string variantID = RetrieveMenuID(variantPage);
                    string milkID = RetrieveMenuID(milkPage);
                    string addonID = RetrieveMenuID(addonPage);
                    string syrupID = RetrieveMenuID(syrupPage);
                    string sweetID = RetrieveMenuID(sweetPage);
                    string baseID = RetrieveMenuID(basePage);
                    command.CommandText = "INSERT INTO products VALUES(@p_name, @orderMode, @p_price, @p_scale, @p_image, @quantity, @has_q, @has_disc, @variantpage, @variant_id, @milkpage, @milk_id, @milkscale, @milkmax, @milkreq, @addonpage, @addon_id, @addonscale, @addonmax, @addonreq, @syruppage, @syrup_id, @syrupscale, @syrupmax, @syrupreq, @sweetpage, @sweet_id, @sweetmax, @sweetscale, @sweetreq, @basepage, @base_id, @basemax, @basereq, @espdef, @espscale, @espmax, @hasesp, @canservewarm, @tempmode, @sizemode, @startDate, @endDate, @startTime, @endTime, @points, @stamps)";
                    command.Parameters.AddWithValue("@p_image", selectedProductImage);
                    command.Parameters.AddWithValue("@orderMode", orderMode);
                    command.Parameters.AddWithValue("@p_price", price);
                    command.Parameters.AddWithValue("@p_scale", priceScale);
                    command.Parameters.AddWithValue("@quantity", quantity);
                    command.Parameters.AddWithValue("@has_q", hasQuantity);
                    command.Parameters.AddWithValue("@has_disc", hasDisc);
                    command.Parameters.AddWithValue("@variantpage", variantPage);
                    command.Parameters.AddWithValue("@milkpage", milkPage);
                    command.Parameters.AddWithValue("@milkscale", milkScale);
                    command.Parameters.AddWithValue("@milkmax", maxMilk);
                    command.Parameters.AddWithValue("@milkreq", milkRequired);
                    command.Parameters.AddWithValue("@addonpage", addonPage);
                    command.Parameters.AddWithValue("@addonscale", addonScale);
                    command.Parameters.AddWithValue("@addonmax", maxAddon);
                    command.Parameters.AddWithValue("@addonreq", addonRequired);
                    command.Parameters.AddWithValue("@syruppage", syrupPage);
                    command.Parameters.AddWithValue("@syrupscale", syrupScale);
                    command.Parameters.AddWithValue("@syrupmax", maxSyrup);
                    command.Parameters.AddWithValue("@syrupreq", syrupRequired);
                    command.Parameters.AddWithValue("@sweetpage", sweetPage);
                    command.Parameters.AddWithValue("@sweetmax", maxSweet);
                    command.Parameters.AddWithValue("@sweetscale", sweetScale);
                    command.Parameters.AddWithValue("@sweetreq", sweetRequired);
                    command.Parameters.AddWithValue("@basepage", basePage);
                    command.Parameters.AddWithValue("@basemax", maxBase);
                    command.Parameters.AddWithValue("@basereq", baseRequired);
                    command.Parameters.AddWithValue("@espdef", defShot);
                    command.Parameters.AddWithValue("@espscale", shotScale);
                    command.Parameters.AddWithValue("@espmax", maxShot);
                    command.Parameters.AddWithValue("@hasesp", hasEsp);
                    command.Parameters.AddWithValue("@canservewarm", serveWarm);
                    command.Parameters.AddWithValue("@tempmode", tempMode);
                    command.Parameters.AddWithValue("@sizemode", sizeMode); 
                    command.Parameters.AddWithValue("@points", pointsVal);
                    command.Parameters.AddWithValue("@stamps", stampsVal);
                    command.Parameters.AddWithValue("@variant_id", variantID);
                    command.Parameters.AddWithValue("@milk_id", milkID);
                    command.Parameters.AddWithValue("@addon_id", addonID);
                    command.Parameters.AddWithValue("@syrup_id", syrupID);
                    command.Parameters.AddWithValue("@sweet_id", sweetID);
                    command.Parameters.AddWithValue("@base_id", baseID);
                    if (ProductStartTime.SelectedItem != null)
                    {
                        string selectedTime = ProductStartTime.SelectedItem.ToString();
                        DateTime parsedTime;
                        if (DateTime.TryParseExact(selectedTime, "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTime))
                        {
                            string startTimeSQL = parsedTime.ToString("HH:mm:ss");
                            command.Parameters.AddWithValue("@startTime", startTimeSQL);
                        }
                        else command.Parameters.AddWithValue("@startTime", DBNull.Value);
                    }
                    else command.Parameters.AddWithValue("@startTime", DBNull.Value);
                    if (ProductEndTime.SelectedItem != null)
                    {
                        string selectedTime = ProductEndTime.SelectedItem.ToString();
                        DateTime parsedTime;
                        if (DateTime.TryParseExact(selectedTime, "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTime))
                        {
                            string endTimeSQL = parsedTime.ToString("HH:mm:ss");
                            command.Parameters.AddWithValue("@endTime", endTimeSQL);
                        }
                        else command.Parameters.AddWithValue("@endTime", DBNull.Value);
                    }
                    else command.Parameters.AddWithValue("@endTime", DBNull.Value);
                    if (ProductStartDate.SelectedDate.HasValue) command.Parameters.AddWithValue("@startDate", ProductStartDate.SelectedDate.Value);
                    else command.Parameters.AddWithValue("@startDate", DBNull.Value);
                    if (ProductEndDate.SelectedDate.HasValue) command.Parameters.AddWithValue("@endDate", ProductEndDate.SelectedDate.Value);
                    else command.Parameters.AddWithValue("@endDate", DBNull.Value);
                    command.ExecuteNonQuery();
                    LoadProducts("");
                    NewProductName.Text = "";
                    ProductPrice.Text = "";
                    ProductQuantity.Text = "";
                    ProductImage.Source = null;
                    selectedProductImage = null;
                    ProductStartDate.SelectedDate = null;
                    ProductEndDate.SelectedDate = null;
                    ProductStartTime.SelectedIndex = ProductStartTime.Items.Count - 1;
                    ProductEndTime.SelectedIndex = ProductEndTime.Items.Count - 1;
                    MessageBox.Show("The product has been successfully added into the systen.", "Product added", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void UpdateProduct_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ProductName.Text))
            {
                MessageBox.Show("Please ensure that the product name is valid.", "Invalid product name", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand($"SELECT EXISTS(SELECT 1 FROM products WHERE product_name = @p_name LIMIT 1)", connect, transaction))
                    {
                        try
                        {
                            command.Parameters.AddWithValue("@p_name", ProductName.Text);
                            StringBuilder ops = new StringBuilder();
                            int res = Convert.ToInt32(command.ExecuteScalar());
                            if (res == 0)
                            {
                                MessageBox.Show("Please ensure that the product exists in the system.", "Non-existent product", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                            command.CommandText = "SELECT * FROM products WHERE product_name = @p_name FOR UPDATE";
                            command.ExecuteNonQuery();
                            if (!string.IsNullOrWhiteSpace(NewProductName.Text))
                            {
                                command.CommandText = "SELECT EXISTS(SELECT 1 FROM products WHERE product_name = @n_name)";
                                command.Parameters.AddWithValue("@n_name", NewProductName.Text);
                                int rowCount = Convert.ToInt32(command.ExecuteScalar());
                                if (rowCount > 0)
                                {
                                    MessageBox.Show("Please ensure that the new product name is unused.", "Invalid new product name", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    return;
                                }

                                string menuID = "";
                                using (MySqlConnection connect2 = new MySqlConnection(cs))
                                {
                                    connect2.Open();
                                    using (MySqlCommand command2 = new MySqlCommand("SELECT menu_id FROM menus", connect2))
                                    {
                                        using (MySqlDataReader reader = command2.ExecuteReader())
                                        {
                                            try
                                            {
                                                while (reader.Read())
                                                {
                                                    menuID = reader[0]?.ToString();
                                                    command.CommandText = $"SELECT * FROM {menuID} WHERE prod_name = @p_name LIMIT 1 FOR UPDATE";
                                                    command.ExecuteNonQuery();
                                                    command.CommandText = $"UPDATE {menuID} SET prod_name = @n_name WHERE prod_name = @p_name LIMIT 1";
                                                    command.ExecuteNonQuery();
                                                }
                                            }
                                            catch (MySqlException)
                                            {
                                                transaction.Rollback();
                                                MessageBox.Show($"The selected product has not been updated, please try again.", "Product update unsuccessful", MessageBoxButton.OK, MessageBoxImage.Information);
                                                return;
                                            }
                                        }
                                    }
                                }

                                command.CommandText = "UPDATE products SET product_name = @n_name WHERE product_name = @p_name";
                                command.ExecuteNonQuery();
                                ProductName.Text = NewProductName.Text;
                                NewProductName.Text = "";
                                if (ops.Length > 0) ops.Append(", Product Name");
                                else ops.Append("Product Name");
                            }
                            if (UpdateProductImage.IsChecked == true)
                            {
                                command.CommandText = "UPDATE products SET product_image = @p_img WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@p_img", selectedProductImage);
                                command.ExecuteNonQuery();
                                ProductImage.Source = null;
                                selectedProductImage = null;
                                UpdateProductImage.IsChecked = false;
                                if (ops.Length > 0) ops.Append(", Product Image");
                                else ops.Append("Product Image");
                            }
                            if (ProductOrderMode.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET order_mode = @orderMode WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@orderMode", ProductOrderMode.SelectedItem);
                                command.ExecuteNonQuery();
                                ProductOrderMode.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Product Order Mode");
                                else ops.Append("Product Order Mode");
                            }
                            if (VariantPage.SelectedIndex != VariantPage.Items.Count - 1)
                            {
                                if (!VerifyMenuName(VariantPage.SelectedItem.ToString()))
                                {
                                    MessageBox.Show("The selected variants page does not exist in the database.\nThe menu boxes are now refreshed, please select a different menu", "Non-existent menu", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                                command.CommandText = "UPDATE products SET variants_page = @variantpage, variants_id = @variantID WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@variantpage", VariantPage.SelectedItem);
                                string varID = RetrieveMenuID(VariantPage.Text);
                                command.Parameters.AddWithValue("@variantID", varID);
                                command.ExecuteNonQuery();
                                VariantPage.SelectedIndex = VariantPage.Items.Count - 1;
                                if (ops.Length > 0) ops.Append(", Variant Menu");
                                else ops.Append("Variant Page");
                            }
                            if (MilkPage.SelectedIndex != MilkPage.Items.Count - 1)
                            {
                                if (!VerifyMenuName(MilkPage.SelectedItem.ToString()))
                                {
                                    MessageBox.Show("The selected milk page does not exist in the database.\nThe menu boxes are now refreshed, please select a different menu", "Non-existent menu", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                                command.CommandText = "UPDATE products SET milk_page = @milkpage, milk_id = @milkID WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@milkpage", MilkPage.SelectedItem);
                                string milkID = RetrieveMenuID(MilkPage.Text);
                                command.Parameters.AddWithValue("@milkID", milkID);
                                command.ExecuteNonQuery();
                                MilkPage.SelectedIndex = MilkPage.Items.Count - 1;
                                if (ops.Length > 0) ops.Append(", Milk Menu");
                                else ops.Append("Milk Page");
                            }
                            if (MilkScale.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET milk_scale_value = @milkscale WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@milkscale", MilkScale.SelectedItem);
                                command.ExecuteNonQuery();
                                MilkScale.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Milk Scale Value");
                                else ops.Append("Milk Scale Value");
                            }
                            if (MaxMilk.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET max_milk = @milkmax WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@milkmax", MaxMilk.SelectedItem);
                                command.ExecuteNonQuery();
                                MaxMilk.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Milk Max Amount");
                                else ops.Append("Milk Max Amount");
                            }
                            if (MilkRequired.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET milk_required = @milkreq WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@milkreq", MilkRequired.SelectedItem);
                                command.ExecuteNonQuery();
                                MilkRequired.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Milk Required");
                                else ops.Append("Milk Required");
                            }
                            if (AddonPage.SelectedIndex != AddonPage.Items.Count - 1)
                            {
                                if (!VerifyMenuName(AddonPage.SelectedItem.ToString()))
                                {
                                    MessageBox.Show("The selected add-ons page does not exist in the database.\nThe menu boxes are now refreshed, please select a different menu", "Non-existent menu", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                                command.CommandText = "UPDATE products SET addons_page = @addonpage, addons_id = @addonID WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@addonpage", AddonPage.SelectedItem);
                                string addonID = RetrieveMenuID(AddonPage.Text);
                                command.Parameters.AddWithValue("@addonID", addonID);
                                command.ExecuteNonQuery();
                                AddonPage.SelectedIndex = AddonPage.Items.Count - 1;
                                if (ops.Length > 0) ops.Append(", Add-on Menu");
                                else ops.Append("Add-on Menu");
                            }
                            if (AddonScale.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET addon_scale_value = @addonscale WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@addonscale", AddonScale.SelectedItem);
                                command.ExecuteNonQuery();
                                AddonScale.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Add-on Scale Value");
                                else ops.Append("Add-on Scale Value");
                            }
                            if (MaxAddon.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET max_addon = @addonmax WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@addonmax", MaxAddon.SelectedItem);
                                command.ExecuteNonQuery();
                                MaxAddon.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Add-on Max Amount");
                                else ops.Append("Add-on Max Amount");
                            }
                            if (AddonRequired.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET addon_required = @addonreq WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@addonreq", AddonRequired.SelectedItem);
                                command.ExecuteNonQuery();
                                AddonRequired.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Add-on Required");
                                else ops.Append("Add-on Required");
                            }
                            if (SyrupPage.SelectedIndex != SyrupPage.Items.Count - 1)
                            {
                                if (!VerifyMenuName(SyrupPage.SelectedItem.ToString()))
                                {
                                    MessageBox.Show("The selected syrup page does not exist in the database.\nThe menu boxes are now refreshed, please select a different menu", "Non-existent menu", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                                command.CommandText = "UPDATE products SET syrup_page = @syruppage, syrup_id = @syrupID WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@syruppage", SyrupPage.SelectedItem);
                                string syrupID = RetrieveMenuID(SyrupPage.Text);
                                command.Parameters.AddWithValue("@syrupID", syrupID);
                                command.ExecuteNonQuery();
                                SyrupPage.SelectedIndex = SyrupPage.Items.Count - 1;
                                if (ops.Length > 0) ops.Append(", Syrup Menu");
                                else ops.Append("Syrup Menu");
                            }
                            if (SyrupScale.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET syrup_scale_value = @syrupscale WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@syrupscale", SyrupScale.SelectedItem);
                                command.ExecuteNonQuery();
                                SyrupScale.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Syrup Scale Value");
                                else ops.Append("Syrup Scale Value");
                            }
                            if (MaxSyrup.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET max_syrup = @syrupmax WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@syrupmax", MaxSyrup.SelectedItem);
                                command.ExecuteNonQuery();
                                MaxSyrup.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Syrup Max Amount");
                                else ops.Append("Syrup Max Amount");
                            }
                            if (SyrupRequired.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET syrup_required = @syrupreq WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@syrupreq", SyrupRequired.SelectedItem);
                                command.ExecuteNonQuery();
                                SyrupRequired.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Syrup Required");
                                else ops.Append("Syrup Required");
                            }
                            if (SweetenerPage.SelectedIndex != SweetenerPage.Items.Count - 1)
                            {
                                if (!VerifyMenuName(SweetenerPage.SelectedItem.ToString()))
                                {
                                    MessageBox.Show("The selected sweetener page does not exist in the database.\nThe menu boxes are now refreshed, please select a different menu", "Non-existent menu", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                                command.CommandText = "UPDATE products SET sweetener_page = @sweetpage, sweet_id = @sweetID WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@sweetpage", SweetenerPage.SelectedItem);
                                string sweetID = RetrieveMenuID(SweetenerPage.Text);
                                command.Parameters.AddWithValue("@sweetID", sweetID);
                                command.ExecuteNonQuery();
                                SweetenerPage.SelectedIndex = SweetenerPage.Items.Count - 1;
                                if (ops.Length > 0) ops.Append(", Sweetener Menu");
                                else ops.Append("Sweetener Menu");
                            }
                            if (SweetenerScale.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET sweetener_scale_value = @sweetscale WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@sweetscale", SweetenerScale.SelectedItem);
                                command.ExecuteNonQuery();
                                SweetenerScale.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Sweetener Scale Value");
                                else ops.Append("Sweetener Scale Value");
                            }
                            if (MaxSweetener.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET max_sweetener = @sweetmax WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@sweetmax", MaxSweetener.SelectedItem);
                                command.ExecuteNonQuery();
                                MaxSweetener.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Sweetener Max Amount");
                                else ops.Append("Sweetener Max Amount");
                            }
                            if (SweetRequired.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET sweet_required = @sweetreq WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@sweetreq", SweetRequired.SelectedItem);
                                command.ExecuteNonQuery();
                                SweetRequired.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Sweet Required");
                                else ops.Append("Sweet Required");
                            }
                            if (BasePage.SelectedIndex != BasePage.Items.Count - 1)
                            {
                                if (!VerifyMenuName(BasePage.SelectedItem.ToString()))
                                {
                                    MessageBox.Show("The selected base page does not exist in the database.\nThe menu boxes are now refreshed, please select a different menu", "Non-existent menu", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                                command.CommandText = "UPDATE products SET base_page = @basepage, base_id = @baseID WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@basepage", BasePage.SelectedItem);
                                string baseID = RetrieveMenuID(BasePage.Text);
                                command.Parameters.AddWithValue("@baseID", baseID);
                                command.ExecuteNonQuery();
                                BasePage.SelectedIndex = BasePage.Items.Count - 1;
                                if (ops.Length > 0) ops.Append(", Base Menu");
                                else ops.Append("Base Menu");
                            }
                            if (MaxBase.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET max_base = @basemax WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@basemax", MaxBase.SelectedItem);
                                command.ExecuteNonQuery();
                                MaxBase.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Base Max Amount");
                                else ops.Append("Base Max Amount");
                            }
                            if (BaseRequired.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET base_required = @basereq WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@basereq", BaseRequired.SelectedItem);
                                command.ExecuteNonQuery();
                                BaseRequired.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Base Required");
                                else ops.Append("Base Required");
                            }
                            if (DefaultEspresso.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET default_espresso_shot = @defesp WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@defesp", DefaultEspresso.SelectedItem);
                                command.ExecuteNonQuery();
                                DefaultEspresso.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Default Espresso Shot");
                                else ops.Append("Default Espresso Shot");
                            }
                            if (EspressoScale.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET espresso_scale_value = @espscale WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@espscale", EspressoScale.SelectedItem);
                                command.ExecuteNonQuery();
                                EspressoScale.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Espresso Shot Scale Value");
                                else ops.Append("Espresso Shot Scale Value");
                            }
                            if (MaxEspresso.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET max_espresso_shot = @maxesp WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@maxesp", MaxEspresso.SelectedItem);
                                command.ExecuteNonQuery();
                                MaxEspresso.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Espresso Shot Max Value");
                                else ops.Append("Espresso Shot Max Value");
                            }
                            if (HasEspShot.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET has_espresso_shot = @hasesp WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@hasesp", HasEspShot.SelectedItem);
                                command.ExecuteNonQuery();
                                HasEspShot.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Has Espresso Shot");
                                else ops.Append("Has Espresso Shot");
                            }
                            if (TempMode.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET temp_mode = @tmpmode WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@tmpmode", TempMode.SelectedItem);
                                command.ExecuteNonQuery();
                                TempMode.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Temperature Mode");
                                else ops.Append("Temperature Mode");
                            }
                            if (SizeMode.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET size_mode = @sizemode WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@sizemode", SizeMode.SelectedItem);
                                command.ExecuteNonQuery();
                                SizeMode.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Size Mode");
                                else ops.Append("Size Mode");
                            }
                            int quantity = 0;
                            if (!string.IsNullOrWhiteSpace(ProductQuantity.Text) && int.TryParse(ProductQuantity.Text, out quantity) && quantity >= 0)
                            {
                                command.CommandText = "UPDATE products SET product_quantity = @quan WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@quan", quantity);
                                command.ExecuteNonQuery();
                                ProductQuantity.Text = "";
                                if (ops.Length > 0) ops.Append(", Product Quantity");
                                else ops.Append("Product Quantity");
                            }
                            double price = 0.0;
                            if (!string.IsNullOrWhiteSpace(ProductPrice.Text) && double.TryParse(ProductPrice.Text, out price) && price >= 0.0)
                            {
                                command.CommandText = "UPDATE products SET product_price = @price WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@price", price);
                                command.ExecuteNonQuery();
                                ProductPrice.Text = "";
                                if (ops.Length > 0) ops.Append(", Product Price");
                                else ops.Append("Product Price");
                            }
                            if (PriceScale.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET price_scale = @p_scale WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@p_scale", PriceScale.SelectedItem);
                                command.ExecuteNonQuery();
                                PriceScale.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Price Scale");
                                else ops.Append("Price Scale");
                            }
                            if (PointsValue.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET points_value = @points WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@points", PointsValue.SelectedItem);
                                command.ExecuteNonQuery();
                                PointsValue.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Points Value");
                                else ops.Append("Points Value");
                            }
                            if (StampValue.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET stamps_value = @stamps WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@stamps", StampValue.SelectedItem);
                                command.ExecuteNonQuery();
                                StampValue.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Stamp Value");
                                else ops.Append("Stamp Value");
                            }
                            if (HasQuantity.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET has_quantity = @has_q WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@has_q", HasQuantity.SelectedItem);
                                command.ExecuteNonQuery();
                                HasQuantity.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Has Quantity");
                                else ops.Append("Has Quantity");
                            }
                            if (HasDiscount.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE products SET has_discount = @has_d WHERE product_name = @p_name";
                                command.Parameters.AddWithValue("@has_d", HasDiscount.SelectedItem);
                                command.ExecuteNonQuery();
                                HasDiscount.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Has PWD/Senior Discount");
                                else ops.Append("Has PWD/Senior Discount");
                            }
                            if (ProductStartTime.SelectedIndex != ProductStartTime.Items.Count - 1)
                            {
                                command.CommandText = "UPDATE products SET start_time = @startTime WHERE product_name = @p_name";
                                string selectedTime = ProductStartTime.SelectedItem.ToString();
                                DateTime parsedTime;
                                if (DateTime.TryParseExact(selectedTime, "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTime))
                                {
                                    string startTimeSQL = parsedTime.ToString("HH:mm:ss");
                                    command.Parameters.AddWithValue("@startTime", startTimeSQL);
                                }
                                else command.Parameters.AddWithValue("@startTime", DBNull.Value);
                                command.ExecuteNonQuery();
                                ProductStartTime.SelectedIndex = ProductStartTime.Items.Count - 1;                                
                                if (ops.Length > 0) ops.Append(", Product Start Time");
                                else ops.Append("Product Start Time");
                            }
                            if (ProductEndTime.SelectedIndex != ProductEndTime.Items.Count - 1)
                            {
                                command.CommandText = "UPDATE products SET end_time = @endTime WHERE product_name = @p_name";
                                string selectedTime = ProductEndTime.SelectedItem.ToString();
                                DateTime parsedTime;
                                if (DateTime.TryParseExact(selectedTime, "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTime))
                                {
                                    string startTimeSQL = parsedTime.ToString("HH:mm:ss");
                                    command.Parameters.AddWithValue("@endTime", startTimeSQL);
                                }
                                else command.Parameters.AddWithValue("@endTime", DBNull.Value);
                                command.ExecuteNonQuery();
                                ProductEndTime.SelectedIndex = ProductEndTime.Items.Count - 1;                                
                                if (ops.Length > 0) ops.Append(", Product End Time");
                                else ops.Append("Product End Time");
                            }
                            if (UpdateProductStartDate.IsChecked == true)
                            {
                                command.CommandText = "UPDATE products SET start_date = @startDate WHERE product_name = @p_name";
                                if (ProductStartDate.SelectedDate.HasValue) command.Parameters.AddWithValue("@startDate", ProductStartDate.SelectedDate.Value);
                                else command.Parameters.AddWithValue("@startDate", DBNull.Value);
                                command.ExecuteNonQuery();
                                ProductStartDate.SelectedDate = null;
                                UpdateProductStartDate.IsChecked = false;
                                if (ops.Length > 0) ops.Append(", Product Start Date");
                                else ops.Append("Product Start Date");
                            }
                            if (UpdateProductEndDate.IsChecked == true)
                            {
                                command.CommandText = "UPDATE products SET end_date = @endDate WHERE product_name = @p_name";
                                if (ProductEndDate.SelectedDate.HasValue) command.Parameters.AddWithValue("@endDate", ProductEndDate.SelectedDate.Value);
                                else command.Parameters.AddWithValue("@endDate", DBNull.Value);
                                command.ExecuteNonQuery();
                                ProductEndDate.SelectedDate = null;
                                UpdateProductEndDate.IsChecked = false;
                                if (ops.Length > 0) ops.Append(", Product End Date");
                                else ops.Append("Product End Date");
                            }
                            transaction.Commit();
                            LoadProducts("");
                            MessageBox.Show($"The following details have been updated: {ops.ToString()}", "Product update successful", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"The selected product has not been updated, please try again.", "Product update unsuccessful", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
        }

        private void SelectMenuImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp" };
            dlg.Multiselect = false;

            if (dlg.ShowDialog() == true)
            {
                BitmapImage bitmap = new BitmapImage(new Uri(dlg.FileName));
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.Freeze();

                int width = bitmap.PixelWidth;
                int height = bitmap.PixelHeight;
                if (width < 64 || height < 64)
                {
                    MessageBox.Show("The selected image has too low resolution, please select another image.", "Too low image resolution", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MenuImage.Source = bitmap;

                using (var ms = new MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(ms);
                    selectedMenuImage = ms.ToArray();
                }
            }
        }

        private void RemoveMenuImage_Click(object sender, RoutedEventArgs e)
        {
            MenuImage.Source = null;
            selectedMenuImage = null;
        }

        private void LoadMenus(string target)
        {
            if (target.Contains("/") || target.Contains("\\") || string.IsNullOrWhiteSpace(target)) { target = "%"; }
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand($"SELECT menu_name AS 'Menu Name', order_mode AS 'Order Mode', is_hidden AS 'Is Menu Hidden', start_date AS 'Menu Start Date', end_date AS 'Menu End Date', start_time AS 'Menu Start Time', end_time AS 'Menu End Time', menu_image FROM menus WHERE menu_name LIKE @m_target", connect))
                {
                    command.Parameters.AddWithValue("@m_target", target);
                    DataTable table = new DataTable();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command)) adapter.Fill(table);

                    for (int i = 0; i < table.Rows.Count; ++i)
                    {
                        if (table.Rows[i]["Menu Name"].ToString() == "Home")
                        {
                            table.Rows.RemoveAt(i);
                            break;
                        }
                    }

                    // Template Column for Image
                    DataGridTemplateColumn imageCol = new DataGridTemplateColumn { Header = "Menu Image" };

                    // Define the Image element in code
                    FrameworkElementFactory imageFactory = new FrameworkElementFactory(typeof(Image));
                    Binding imageBinding = new Binding("menu_image")
                    {
                        Converter = new ByteArrayToImageConverter()
                    };
                    imageFactory.SetBinding(Image.SourceProperty, imageBinding);
                    imageFactory.SetValue(Image.WidthProperty, 90.0); // Width = 90
                    imageFactory.SetValue(Image.HeightProperty, 90.0); // Heoght = 90

                    // Create DataTemplate
                    DataTemplate cellTemplate = new DataTemplate
                    {
                        VisualTree = imageFactory
                    };
                    imageCol.CellTemplate = cellTemplate;

                    MenuGrid.ItemsSource = table.DefaultView;
                    if (MenuGrid.Columns.Count > 6 && MenuGrid.Columns[6] != null) { MenuGrid.Columns.RemoveAt(6); }
                    MenuGrid.Columns.Add(imageCol);
                }
            }
        }

        private void ClearMenuInputs2(bool setToSentinel)
        {
            NewMenuName.Text = "";
            MenuName.Text = "";    
            selectedMenuImage = null;
            MenuImage.Source = null;
            MenuStartDate.SelectedDate = null;
            MenuEndDate.SelectedDate = null;
            if (setToSentinel)
            {
                MenuHidden.SelectedIndex = 0;
                MenuStartTime.SelectedIndex = MenuStartTime.Items.Count - 1;
                MenuEndTime.SelectedIndex = MenuEndTime.Items.Count - 1;
                MenuOrderMode.SelectedIndex = 0;
            }
            else
            {
                MenuHidden.SelectedIndex = 1;
                MenuStartTime.SelectedIndex = MenuStartTime.Items.Count - 2;
                MenuEndTime.SelectedIndex = MenuEndTime.Items.Count - 2;
                MenuOrderMode.SelectedIndex = 3;
            }
        }

        private void ClearMenuInputs_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => ClearMenuInputs2(false);

        private void ClearMenuInputs_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
            ClearMenuInputs2(true);
        }

        private void AddMenu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewMenuName.Text) || NewMenuName.Text == "None" || NewMenuName.Text == "N/A" || NewMenuName.Text == "Home")
            {
                MessageBox.Show("Please ensure that the menu name is valid.", "Menu add unsuccessful", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            bool isHidden = MenuHidden.SelectedIndex == 0 ? false : (bool)MenuHidden.SelectedItem;
            int orderMode = MenuOrderMode.SelectedIndex == 0 ? 3 : (int)MenuOrderMode.SelectedItem;
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand($"SELECT EXISTS(SELECT 1 FROM menus WHERE menu_name = @m_name)", connect))
                {
                    command.Parameters.AddWithValue("@m_name", NewMenuName.Text);
                    int res = Convert.ToInt32(command.ExecuteScalar());
                    if (res > 0)
                    {
                        MessageBox.Show("This menu name already exists in the system.", "Menu add unsuccessful", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    StringBuilder sb = new StringBuilder();
                    Random rand = new Random();
                    int id = rand.Next(100000000);
                    string temp = id.ToString();
                    sb.Append("menu_");
                    switch (temp.Length)
                    {
                        case 7: sb.Append("0000000"); break;
                        case 6: sb.Append("000000"); break;
                        case 5: sb.Append("00000"); break;
                        case 4: sb.Append("0000"); break;
                        case 3: sb.Append("000"); break;
                        case 2: sb.Append("00"); break;
                        case 1: sb.Append("0"); break;
                    }
                    sb.Append(temp);
                    command.CommandText = "INSERT INTO menus VALUES(@m_name, @m_id, @orderMode, @m_img, @is_hidden, @startDate, @endDate, @startTime, @endTime)";
                    command.Parameters.AddWithValue("@m_id", sb.ToString());
                    command.Parameters.AddWithValue("@m_img", selectedMenuImage);
                    command.Parameters.AddWithValue("@is_hidden", isHidden);
                    command.Parameters.AddWithValue("@orderMode", orderMode);
                    if (MenuStartTime.SelectedItem != null)
                    {
                        string selectedTime = MenuStartTime.SelectedItem.ToString();
                        DateTime parsedTime;
                        if (DateTime.TryParseExact(selectedTime, "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTime))
                        {
                            string startTimeSQL = parsedTime.ToString("HH:mm:ss");
                            command.Parameters.AddWithValue("@startTime", startTimeSQL);
                        }
                        else command.Parameters.AddWithValue("@startTime", DBNull.Value);
                    }
                    else command.Parameters.AddWithValue("@startTime", DBNull.Value);
                    if (MenuEndTime.SelectedItem != null)
                    {
                        string selectedTime = MenuEndTime.SelectedItem.ToString();
                        DateTime parsedTime;
                        if (DateTime.TryParseExact(selectedTime, "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTime))
                        {
                            string endTimeSQL = parsedTime.ToString("HH:mm:ss");
                            command.Parameters.AddWithValue("@endTime", endTimeSQL);
                        }
                        else command.Parameters.AddWithValue("@endTime", DBNull.Value);
                    }
                    else command.Parameters.AddWithValue("@endTime", DBNull.Value);
                    if (MenuStartDate.SelectedDate.HasValue) command.Parameters.AddWithValue("@startDate", MenuStartDate.SelectedDate.Value);
                    else command.Parameters.AddWithValue("@startDate", DBNull.Value);
                    if (MenuEndDate.SelectedDate.HasValue) command.Parameters.AddWithValue("@endDate", MenuEndDate.SelectedDate.Value);
                    else command.Parameters.AddWithValue("@endDate", DBNull.Value);
                    command.ExecuteNonQuery();
                    command.CommandText = $"CREATE TABLE {sb.ToString()} (prod_name VARCHAR(150) COLLATE utf8mb4_bin)";
                    command.ExecuteNonQuery();
                    LoadMenus("");
                    NewMenuName.Text = "";
                    MenuImage.Source = null;
                    selectedMenuImage = null;
                    MenuStartDate.SelectedDate = null;
                    MenuEndDate.SelectedDate = null;
                    MenuStartTime.SelectedIndex = MenuStartTime.Items.Count - 1;
                    MenuEndTime.SelectedIndex = MenuEndTime.Items.Count - 1;
                    MessageBox.Show("The menu has been successfully added into the system.", "Menu add successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void SearchMenu_Click(object sender, RoutedEventArgs e) => LoadMenus(MenuName.Text);

        private void RemoveMenu_Click(object sender, RoutedEventArgs e)
        {
            if (MenuName.Text.Length == 0 || MenuName.Text == "Home")
            {
                MessageBox.Show("Please ensure that you have provided a valid current menu name.", "Missing current menu name", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand($"SELECT EXISTS(SELECT 1 FROM menus WHERE menu_name = @m_name LIMIT 1)", connect, transaction))
                    {
                        string id = "";
                        try
                        {
                            command.Parameters.AddWithValue("@m_name", MenuName.Text);
                            int result = Convert.ToInt32(command.ExecuteScalar());
                            if (result == 0)
                            {
                                MessageBox.Show("The menu name does not exist, please provide a valid menu name.", "Non-existent current menu name", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            MessageBoxResult answer = MessageBox.Show("Are you sure you want to delete the specified menu?\nThe system will unlink all products from this menu, which will take some time.", "Confirm menu removal", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                            if (answer == MessageBoxResult.Yes)
                            {
                                command.CommandText = "SELECT * FROM menus WHERE menu_name = @m_name LIMIT 1 FOR UPDATE";
                                command.ExecuteNonQuery();
                                command.CommandText = "SELECT menu_id FROM menus WHERE menu_name = @m_name";
                                id = Convert.ToString(command.ExecuteScalar());
                                command.CommandText = $"DROP TABLE {id}";
                                command.ExecuteNonQuery();
                                command.CommandText = "DELETE FROM menus WHERE menu_name = @m_name";
                                command.ExecuteNonQuery();
                                command.CommandText = "SELECT * FROM products WHERE milk_page = @m_name FOR UPDATE";
                                command.ExecuteNonQuery();
                                command.CommandText = "UPDATE products SET milk_page = 'N/A', milk_id = 'N/A' WHERE milk_page = @m_name";
                                command.ExecuteNonQuery();
                                command.CommandText = "SELECT * FROM products WHERE addons_page = @m_name FOR UPDATE";
                                command.ExecuteNonQuery();
                                command.CommandText = "UPDATE products SET addons_page = 'N/A', addons_id = 'N/A' WHERE addons_page = @m_name";
                                command.ExecuteNonQuery();
                                command.CommandText = "SELECT * FROM products WHERE syrup_page = @m_name FOR UPDATE";
                                command.ExecuteNonQuery();
                                command.CommandText = "UPDATE products SET syrup_page = 'N/A', syrup_id = 'N/A' WHERE syrup_page = @m_name";
                                command.ExecuteNonQuery();
                                command.CommandText = "SELECT * FROM products WHERE sweetener_page = @m_name FOR UPDATE";
                                command.ExecuteNonQuery();
                                command.CommandText = "UPDATE products SET sweetener_page = 'N/A', sweet_id = 'N/A' WHERE sweetener_page = @m_name";
                                command.ExecuteNonQuery();
                                command.CommandText = "SELECT * FROM products WHERE base_page = @m_name FOR UPDATE";
                                command.ExecuteNonQuery();
                                command.CommandText = "UPDATE products SET base_page = 'N/A', base_id = 'N/A' WHERE base_page = @m_name";
                                command.ExecuteNonQuery();
                                command.CommandText = "SELECT * FROM products WHERE variants_page = @m_name FOR UPDATE";
                                command.ExecuteNonQuery();
                                command.CommandText = "UPDATE products SET variants_page = 'N/A', variants_id = 'N/A' WHERE variants_page = @m_name";
                                command.ExecuteNonQuery();
                                transaction.Commit();
                                LoadMenus("");
                                MenuName.Text = "";
                                MessageBox.Show("The menu has been removed from the system.", "Menu remove successful", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        catch (MySqlException)
                        {
                            if (!string.IsNullOrWhiteSpace(id))
                            {
                                command.CommandText = $"CREATE TABLE {id} (prod_name VARCHAR(150))";
                                command.ExecuteNonQuery();
                            }
                            transaction.Rollback();
                            MessageBox.Show("The menu has not been removed, please try again.", "Menu remove unsuccessful", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
        }

        private void ManageProductsButton_Click(object sender, RoutedEventArgs e)
        {
            ProductPanel.Visibility = Visibility.Visible;
            MenuPanel.Visibility = Visibility.Collapsed;
            MenuContentsPanel.Visibility = Visibility.Collapsed;
            ReorderCatalogsPanel.Visibility = Visibility.Collapsed;
            ChangeImagePanel.Visibility = Visibility.Collapsed;
            LoadProducts("");
            LoadMenuComboboxes(2);
            
        }

        private void ManageMenusButton_Click(object sender, RoutedEventArgs e)
        {
            ProductPanel.Visibility = Visibility.Collapsed;
            MenuPanel.Visibility = Visibility.Visible;
            MenuContentsPanel.Visibility = Visibility.Collapsed;
            ReorderCatalogsPanel.Visibility = Visibility.Collapsed;
            ChangeImagePanel.Visibility = Visibility.Collapsed;
            LoadMenus("");
        }

        private void ChangeImageButton_Click(object sender, RoutedEventArgs e)
        {
            ProductPanel.Visibility = Visibility.Collapsed;
            MenuPanel.Visibility = Visibility.Collapsed;
            MenuContentsPanel.Visibility = Visibility.Collapsed;
            ReorderCatalogsPanel.Visibility = Visibility.Collapsed;
            ChangeImagePanel.Visibility = Visibility.Visible;
        }

        private void ReorderCatalogsButton_Click(object sender, RoutedEventArgs e)
        {
            ProductPanel.Visibility = Visibility.Collapsed;
            MenuPanel.Visibility = Visibility.Collapsed;
            MenuContentsPanel.Visibility = Visibility.Collapsed;
            ReorderCatalogsPanel.Visibility = Visibility.Visible;
            ChangeImagePanel.Visibility = Visibility.Collapsed;
            LoadMenuComboboxes(1);
        }

        private void MenuGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MenuGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)MenuGrid.SelectedItem;
                string menuName = selectedRow["Menu Name"].ToString();
                MenuName.Text = menuName;
            }
        }

        private void LoadMenu_Click(object sender, RoutedEventArgs e)
        {
            if (MenuGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)MenuGrid.SelectedItem;
                string menuName = selectedRow["Menu Name"].ToString();
                MenuName.Text = menuName;
                MenuHidden.SelectedValue = Convert.ToBoolean(selectedRow["Is Menu Hidden"]);
                MenuOrderMode.SelectedValue = Convert.ToInt32(selectedRow["Order Mode"]);

                if (selectedRow["Menu Start Date"] != DBNull.Value) MenuStartDate.SelectedDate = (DateTime)selectedRow["Menu Start Date"];
                else MenuStartDate.SelectedDate = null;
                if (selectedRow["Menu End Date"] != DBNull.Value) MenuEndDate.SelectedDate = (DateTime)selectedRow["Menu End Date"];
                else MenuEndDate.SelectedDate = null;

                if (selectedRow["Menu Start Time"] != DBNull.Value)
                {
                    TimeSpan startSpan = (TimeSpan)selectedRow["Menu Start Time"];
                    DateTime dateTime = DateTime.Today.Add(startSpan);
                    MenuStartTime.SelectedValue = dateTime.ToString("hh:mm tt"); // Output format like "12:00 PM"
                }
                else MenuStartTime.SelectedValue = "Anytime";

                if (selectedRow["Menu End Time"] != DBNull.Value)
                {
                    TimeSpan endSpan = (TimeSpan)selectedRow["Menu End Time"];
                    DateTime dateTime = DateTime.Today.Add(endSpan);
                    MenuEndTime.SelectedValue = dateTime.ToString("hh:mm tt");
                }
                else MenuEndTime.SelectedValue = "Anytime";

                byte[] imageBytes = selectedRow["menu_image"] as byte[];
                if (imageBytes != null)
                {
                    selectedMenuImage = imageBytes;
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = ms;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;  // Forces the loaded image to appear 
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                        MenuImage.Source = bitmapImage;
                    }
                }
                else
                {
                    selectedMenuImage = null;
                    MenuImage.Source = null;
                }
            }
        }

        private bool VerifyMenuName(string menuname)
        {
            if (menuname == "N/A") { return true; }
            else if (string.IsNullOrEmpty(menuname)) { return false; }
            bool result = false;
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand($"SELECT EXISTS(SELECT 1 from menus WHERE menu_name = @m_name)", connect))
                {
                    command.Parameters.AddWithValue("@m_name", menuname);
                    int res = Convert.ToInt32(command.ExecuteScalar());
                    if (res > 0) result = true;
                }
            }
            if (result == false) LoadMenuComboboxes(2);
            return result;
        }

        private void UpdateMenu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MenuName.Text) || MenuName.Text == "Home")
            {
                MessageBox.Show("Please ensure that you have provided a valid current menu name.", "Missing current menu name", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand($"SELECT EXISTS(SELECT 1 FROM menus WHERE menu_name = @m_name)", connect, transaction))
                    {
                        try
                        {
                            StringBuilder ops = new StringBuilder();
                            command.Parameters.AddWithValue("@m_name", MenuName.Text);
                            int res = Convert.ToInt32(command.ExecuteScalar());
                            if (res == 0)
                            {
                                MessageBox.Show("The indicated menu does not exist in the system.", "Non-existent menu", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            command.CommandText = "SELECT * FROM menus WHERE menu_name = @m_name LIMIT 1 FOR UPDATE";
                            command.ExecuteNonQuery();
                            if (!string.IsNullOrWhiteSpace(NewMenuName.Text) && NewMenuName.Text != "None" && NewMenuName.Text != "Home" && NewMenuName.Text != "N/A")
                            {
                                if (NewMenuName.Text.Length > 48)
                                {
                                    MessageBox.Show("Please ensure that the provided menu name is not too long (max is 48 characters).", "Menu name too long", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                                MessageBoxResult answer = MessageBox.Show("Are you sure you want to rename the specified menu?\nThe system will relink all products to the new menu name, which will take some time.", "Confirm menu removal", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                                if (answer == MessageBoxResult.Yes)
                                {
                                    command.Parameters.AddWithValue("@n_name", NewMenuName.Text);

                                    command.CommandText = "SELECT * FROM products WHERE milk_page = @m_name FOR UPDATE";
                                    command.ExecuteNonQuery();
                                    command.CommandText = "UPDATE products SET milk_page = @n_name WHERE milk_page = @m_name";
                                    command.ExecuteNonQuery();
                                    command.CommandText = "SELECT * FROM products WHERE addons_page = @m_name FOR UPDATE";
                                    command.ExecuteNonQuery();
                                    command.CommandText = "UPDATE products SET addons_page = @n_name WHERE addons_page = @m_name";
                                    command.ExecuteNonQuery();
                                    command.CommandText = "SELECT * FROM products WHERE syrup_page = @m_name FOR UPDATE";
                                    command.ExecuteNonQuery();
                                    command.CommandText = "UPDATE products SET syrup_page = @n_name WHERE syrup_page = @m_name";
                                    command.ExecuteNonQuery();
                                    command.CommandText = "SELECT * FROM products WHERE sweetener_page = @m_name FOR UPDATE";
                                    command.ExecuteNonQuery();
                                    command.CommandText = "UPDATE products SET sweetener_page = @n_name WHERE sweetener_page = @m_name";
                                    command.ExecuteNonQuery();
                                    command.CommandText = "SELECT * FROM products WHERE base_page = @m_name FOR UPDATE";
                                    command.ExecuteNonQuery();
                                    command.CommandText = "UPDATE products SET base_page = @n_name WHERE base_page = @m_name";
                                    command.ExecuteNonQuery();
                                    command.CommandText = "SELECT * FROM products WHERE variants_page = @m_name FOR UPDATE";
                                    command.ExecuteNonQuery();
                                    command.CommandText = "UPDATE products SET variants_page = @n_name WHERE base_page = @m_name";
                                    command.ExecuteNonQuery();
                                    command.CommandText = "UPDATE menus SET menu_name = @n_name WHERE menu_name = @m_name";
                                    command.ExecuteNonQuery();
                                    if (ops.Length > 0) ops.Append(", Menu Name");
                                    else ops.Append("Menu Name");
                                }
                            }
                            if (UpdateMenuImage.IsChecked == true)
                            {
                                command.CommandText = "UPDATE menus SET menu_image = @m_img WHERE menu_name = @m_name LIMIT 1";
                                command.Parameters.AddWithValue("@m_img", selectedMenuImage);
                                command.ExecuteNonQuery();
                                MenuImage.Source = null;
                                selectedMenuImage = null;
                                UpdateMenuImage.IsChecked = false;
                                if (ops.Length > 0) ops.Append(", Menu Image");
                                else ops.Append("Menu Image");
                            }
                            if (MenuHidden.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE menus SET is_hidden = @ishid WHERE menu_name = @m_name LIMIT 1";
                                command.Parameters.AddWithValue("@ishid", MenuHidden.SelectedItem);
                                command.ExecuteNonQuery();
                                MenuHidden.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Is Menu Hidden");
                                else ops.Append("Is Menu Hidden");
                            }
                            if (MenuOrderMode.SelectedIndex != 0)
                            {
                                command.CommandText = "UPDATE menus SET order_mode = @orderMode WHERE menu_name = @m_name LIMIT 1";
                                command.Parameters.AddWithValue("@orderMode", MenuOrderMode.SelectedItem);
                                command.ExecuteNonQuery();
                                MenuOrderMode.SelectedIndex = 0;
                                if (ops.Length > 0) ops.Append(", Menu Order Mode");
                                else ops.Append("Menu Order Mode");
                            }
                            if (MenuStartTime.SelectedIndex != MenuStartTime.Items.Count - 1)
                            {
                                command.CommandText = "UPDATE menus SET start_time = @startTime WHERE menu_name = @m_name LIMIT 1";
                                string selectedTime = MenuStartTime.SelectedItem.ToString();
                                DateTime parsedTime;
                                if (DateTime.TryParseExact(selectedTime, "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTime))
                                {
                                    string startTimeSQL = parsedTime.ToString("HH:mm:ss");
                                    command.Parameters.AddWithValue("@startTime", startTimeSQL);
                                }
                                else command.Parameters.AddWithValue("@startTime", DBNull.Value);
                                command.ExecuteNonQuery();
                                MenuStartTime.SelectedIndex = MenuStartTime.Items.Count - 1;
                                if (ops.Length > 0) ops.Append(", Menu Start Time");
                                else ops.Append("Menu Start Time");
                            }
                            if (MenuEndTime.SelectedIndex != MenuEndTime.Items.Count - 1)
                            {
                                command.CommandText = "UPDATE menus SET end_time = @endTime WHERE menu_name = @m_name LIMIT 1";
                                string selectedTime = MenuEndTime.SelectedItem.ToString();
                                DateTime parsedTime;
                                if (DateTime.TryParseExact(selectedTime, "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedTime))
                                {
                                    string startTimeSQL = parsedTime.ToString("HH:mm:ss");
                                    command.Parameters.AddWithValue("@endTime", startTimeSQL);
                                }
                                else command.Parameters.AddWithValue("@endTime", DBNull.Value);
                                command.ExecuteNonQuery();
                                MenuEndTime.SelectedIndex = MenuEndTime.Items.Count - 1;
                                if (ops.Length > 0) ops.Append(", Menu End Time");
                                else ops.Append("Menu End Time");
                            }
                            if (UpdateMenuStartDate.IsChecked == true)
                            {
                                command.CommandText = "UPDATE menus SET start_date = @startDate WHERE menu_name = @m_name LIMIT 1";
                                if (MenuStartDate.SelectedDate.HasValue) command.Parameters.AddWithValue("@startDate", MenuStartDate.SelectedDate.Value);
                                else command.Parameters.AddWithValue("@startDate", DBNull.Value);
                                command.ExecuteNonQuery();
                                MenuStartDate.SelectedDate = null;
                                UpdateMenuStartDate.IsChecked = false;
                                if (ops.Length > 0) ops.Append(", Menu Start Date");
                                else ops.Append("Menu Start Date");
                            }
                            if (UpdateMenuEndDate.IsChecked == true)
                            {
                                command.CommandText = "UPDATE menus SET end_date = @endDate WHERE menu_name = @m_name LIMIT 1";
                                if (MenuEndDate.SelectedDate.HasValue) command.Parameters.AddWithValue("@endDate", MenuEndDate.SelectedDate.Value);
                                else command.Parameters.AddWithValue("@endDate", DBNull.Value);
                                command.ExecuteNonQuery();
                                MenuEndDate.SelectedDate = null;
                                UpdateMenuEndDate.IsChecked = false;
                                if (ops.Length > 0) ops.Append(", Menu End Date");
                                else ops.Append("Menu End Date");
                            }
                            transaction.Commit();
                            LoadMenus("");
                            MessageBox.Show($"The following details have been modified: {ops.ToString()}", "Menu update successful", MessageBoxButton.OK, MessageBoxImage.Information);                        
                        }
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"The selected menu has not been updated, please try again.", "Menu update unsuccessful", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
        }

        private void LoadMenuContentComboboxes(int mode)
        {
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT menu_name FROM menus", connect))
                {
                    if (mode != 3)
                    {
                        DataTable table = new DataTable();
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            adapter.Fill(table);
                            var menuNames = table.AsEnumerable().Select(row => row["menu_name"]).ToList();
                            if (menuNames.Count == 0) menuNames.Add("N/A");
                            TargetMenu.SelectedIndex = 0;
                            TargetMenu.ItemsSource = menuNames;

                        }
                    }
                    if (mode == 1) { return; }
                    command.CommandText = "SELECT product_name FROM products";
                    DataTable table2 = new DataTable();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                    {
                        adapter.Fill(table2);
                        var prodNames = table2.AsEnumerable().Select(row => row["product_name"]).ToList();
                        if (prodNames.Count == 0) prodNames.Add("N/A");
                        TargetProduct.SelectedIndex = 0;
                        TargetProduct.ItemsSource = prodNames;

                    }
                }
            }
        }

        private void LoadMenuContent(string target)
        {
            if (string.IsNullOrWhiteSpace(target)) return;
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT EXISTS(SELECT 1 FROM menus WHERE menu_name = @m_name)", connect))
                {
                    command.Parameters.AddWithValue("@m_name", target);
                    int res = Convert.ToInt32(command.ExecuteScalar());
                    if (res == 0)
                    {
                        LoadMenuContentComboboxes(1);
                        return;
                    }

                    command.CommandText = "SELECT menu_id FROM menus WHERE menu_name = @m_name";
                    string menuID = "";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read()) menuID = reader[0]?.ToString();
                    }

                    command.CommandText = $"SELECT prod_name AS 'Product Name' FROM {menuID}";
                    DataTable table = new DataTable();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command)) adapter.Fill(table);
                    SimpleMenuGrid.ItemsSource = table.DefaultView;
                    string menuName = target;
                    SelectedMenu.Text = "Selected menu: " + menuName;

                    if (ReorderCatalogsPanel.Visibility == Visibility.Visible)
                    {
                        MenuContentsTable.Columns[0].Visibility = Visibility.Visible;
                        MenuContentsTable.Columns[1].Visibility = Visibility.Collapsed;
                        table.Columns.Add("Appearance Arrangement");
                        int order = 1;
                        foreach (DataRow row in table.Rows)
                        {
                            row["Appearance Arrangement"] = order;
                            order += 1;
                        }
                        MenuContentsTable.ItemsSource = table.DefaultView;
                    }
                }
            }
        }

        private void ViewMenuContents_Click(object sender, RoutedEventArgs e)
        {
            if (TargetMenu.SelectedItem != null) LoadMenuContent(TargetMenu.SelectedItem.ToString());
        }

        private (string, bool) VerifyMenuContentExistence(bool removeOp)
        {
            string menuID = "";
            if (TargetMenu.SelectedIndex == -1 || TargetProduct.SelectedIndex == -1) return ("", false);
            string selectedMenu = TargetMenu.SelectedItem != null ? TargetMenu.SelectedItem.ToString() : "N/A";
            string selectedProduct = TargetProduct.SelectedItem != null ? TargetProduct.SelectedItem.ToString() : "N/A";
            if (selectedMenu == "N/A" || selectedProduct == "N/A") return ("", false);
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT EXISTS(SELECT 1 FROM menus WHERE menu_name = @m_name)", connect))
                {
                    command.Parameters.AddWithValue("@m_name", TargetMenu.SelectedItem);
                    int res = Convert.ToInt32(command.ExecuteScalar());
                    if (res == 0)
                    {
                        LoadMenuContentComboboxes(1);
                        MessageBox.Show("The selected menu name does not exist in the system, please try again.", "Menu does not exist", MessageBoxButton.OK, MessageBoxImage.Information);
                        return ("", false);
                    }
                    command.CommandText = "SELECT EXISTS(SELECT 1 FROM products WHERE product_name = @p_name)";
                    command.Parameters.AddWithValue("@p_name", TargetProduct.SelectedItem);
                    res = Convert.ToInt32(command.ExecuteScalar());
                    if (res == 0)
                    {
                        LoadMenuContentComboboxes(3);
                        MessageBox.Show("The selected product name does not exist in the system, please try again.", "Product does not exist", MessageBoxButton.OK, MessageBoxImage.Information);
                        return ("", false);
                    }
                    command.CommandText = "SELECT menu_id FROM menus WHERE menu_name = @m_name LIMIT 1";                  
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read()) menuID = reader[0]?.ToString();
                    }
                    command.CommandText = $"SELECT EXISTS(SELECT 1 FROM {menuID} WHERE prod_name = @p_name)";
                    res = Convert.ToInt32(command.ExecuteScalar());
                    if (!removeOp)
                    {
                        if (res > 0)
                        {
                            MessageBox.Show("The selected product already exists in this menu.", "Product already exists in menu", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return ("", false);
                        }
                    }
                    else
                    {
                        if (res == 0)
                        {
                            MessageBox.Show("The selected product does not exist in this menu.", "Product does not exist in menu", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return ("", false);
                        }
                    }
                }
            }
            return (menuID, true);
        }

        private void AddProductToMenu_Click(object sender, RoutedEventArgs e)
        {
            var result = VerifyMenuContentExistence(false);
            if (result.Item2 == false) return;
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand($"INSERT INTO {result.Item1} VALUES(@p_name)", connect))
                {
                    command.Parameters.AddWithValue("@p_name", TargetProduct.Text);
                    command.ExecuteNonQuery();
                    LoadMenuContent(TargetMenu.SelectedItem.ToString());
                    MessageBox.Show("The selected product has been added to the menu.", "Add menu content successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void RemoveProductOnMenu_Click(object sender, RoutedEventArgs e)
        {
            var result = VerifyMenuContentExistence(true);
            if (result.Item2 == false) return;
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand($"SELECT * FROM {result.Item1} WHERE prod_name = @p_name LIMIT 1 FOR UPDATE", connect, transaction))
                    {
                        try
                        {
                            command.Parameters.AddWithValue("@p_name", TargetProduct.Text);
                            // Lock the target row
                            command.ExecuteNonQuery();
                            command.CommandText = $"DELETE FROM {result.Item1} WHERE prod_name = @p_name";
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            LoadMenuContent(TargetMenu.SelectedItem.ToString());
                            MessageBox.Show("The selected product has been removed from the menu.", "Remove menu content successful", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (MySqlException) 
                        {
                            MessageBox.Show("The selected product has not been removed from the menu, please try again.", "Remove menu content unsuccessful", MessageBoxButton.OK, MessageBoxImage.Warning);
                            transaction.Rollback();
                        }
                    }
                }
            }
        }

        private string ReturnMenuID(string menuName)
        {
            string mID = "";
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT menu_id FROM menus WHERE menu_name = @mName", connect))
                {
                    command.Parameters.AddWithValue("@mName", menuName);
                    var menuID = command.ExecuteScalar();
                    if (menuID == DBNull.Value) return "";
                    mID = menuID.ToString();
                }
            }
            return mID;
        }

        private void ClearMenuContentsButton_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (TargetMenu.SelectedIndex == -1) return;
            if (SimpleMenuGrid.SelectedItems.Count == 0) return;
            var player = new SoundPlayer("notif.wav");
            player.Play();
            MessageBoxResult confirm = MessageBox.Show("Are you sure you want to remove the selected items of this menu?", "Remove menu items confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.No) return;
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transact = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand("", connect, transact))
                    {
                        string mID = "";
                        try
                        {
                            mID = ReturnMenuID(TargetMenu.Text);
                            if (string.IsNullOrWhiteSpace(mID)) return;

                            command.CommandText = $"SELECT EXISTS(SELECT 1 FROM {mID} LIMIT 1)";
                            int result = Convert.ToInt32(command.ExecuteScalar());
                            if (result == 0)
                            {
                                MessageBox.Show("The selected menu is empty, please select another menu.", "Menu is empty", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            command.Parameters.AddWithValue("@pName", "");
                            command.CommandText = $"SELECT GET_LOCK('{mID}', 15)";
                            command.ExecuteNonQuery();

                            // O(n^2)!
                            // Preparing the statement helps reduce the overhead slightly
                            command.CommandText = $"DELETE FROM {mID} WHERE prod_name = @pName";  
                            command.Prepare();
                            foreach (DataRowView item in SimpleMenuGrid.SelectedItems)
                            {
                                string productName = item["Product Name"].ToString();
                                command.Parameters["@pName"].Value = productName;
                                command.ExecuteNonQuery();
                            }
                            transact.Commit();
                            command.CommandText = $"SELECT RELEASE_LOCK('{mID}')";
                            command.ExecuteNonQuery();
                           
                            LoadMenuContent(TargetMenu.SelectedItem.ToString());
                            MessageBox.Show("The selected items has been removed from this menu.", "Selected items removal successful", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (MySqlException)
                        {
                            transact.Rollback();
                            if (!string.IsNullOrWhiteSpace(mID))
                            {
                                command.CommandText = $"SELECT RELEASE_LOCK('{mID}')";
                                command.ExecuteNonQuery();
                            }
                            MessageBox.Show("The selected items has not been removed from this menu.", "Selected items removal unsuccessful", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        finally { SimpleMenuGrid.SelectedItems.Clear(); }
                    }
                }

            }
        }

        private void ClearMenuContentsButton_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (TargetMenu.SelectedIndex == -1) return;
            MessageBoxResult confirm = MessageBox.Show("Are you sure you want to remove all the items of the selected menu?", "Clear menu items confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm == MessageBoxResult.No) return;
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transact = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand("", connect, transact))
                    {
                        string mID = "";
                        try
                        {
                            mID = ReturnMenuID(TargetMenu.Text);
                            if (string.IsNullOrWhiteSpace(mID)) return;

                            command.CommandText = $"SELECT EXISTS(SELECT 1 FROM {mID} LIMIT 1)";
                            int result = Convert.ToInt32(command.ExecuteScalar());
                            if (result == 0)
                            {
                                MessageBox.Show("The selected menu is empty, please select another menu.", "Menu is empty", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            command.CommandText = $"SELECT GET_LOCK('{mID}', 15)";
                            command.ExecuteNonQuery();

                            command.CommandText = $"DELETE from {mID}";
                            command.ExecuteNonQuery();
                            transact.Commit();
                                                    
                            command.CommandText = $"SELECT RELEASE_LOCK('{mID}')";
                            command.ExecuteNonQuery();

                            LoadMenuContent(TargetMenu.SelectedItem.ToString());
                            MessageBox.Show("The selected menu has been cleared.", "Clear menu successful", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (MySqlException)
                        {
                            transact.Rollback();
                            if (!string.IsNullOrWhiteSpace(mID))
                            {
                                command.CommandText = $"SELECT RELEASE_LOCK('{mID}')";
                                command.ExecuteNonQuery();
                            }
                            MessageBox.Show("The selected menu has not been cleared, please try again.", "Clear menu unsuccessful", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
        }

        private void SimpleMenuGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SimpleMenuGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)SimpleMenuGrid.SelectedItem;
                string prodName = selectedRow["Product Name"].ToString();
                TargetProduct.SelectedItem = prodName;
            }
        }

        private void RemoveProductAllMenus_Click(object sender, RoutedEventArgs e)
        {
            if (TargetProduct.SelectedIndex == -1) return;
            string selectedProduct = TargetProduct.SelectedItem != null ? TargetProduct.SelectedItem.ToString() : "N/A";
            if (selectedProduct == "N/A") return;
            MessageBoxResult answer = MessageBox.Show("Are you sure you want to delete this product on all menus?", "Confirm removal", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer == MessageBoxResult.No) return;
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT EXISTS(SELECT 1 FROM products WHERE product_name = @p_name)", connect))
                {
                    command.Parameters.AddWithValue("@p_name", TargetProduct.SelectedItem);
                    int res = Convert.ToInt32(command.ExecuteScalar());
                    if (res == 0)
                    {
                        LoadMenuContentComboboxes(3);
                        MessageBox.Show("The selected product name does not exist in the system, please try again.", "Remove menu content failed", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    command.CommandText = "SELECT menu_id FROM menus";
                    string menuID = "";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        using (MySqlConnection connect2 = new MySqlConnection(cs))
                        {
                            connect2.Open();
                            using (MySqlTransaction transaction = connect2.BeginTransaction())
                            {
                                using (MySqlCommand command2 = new MySqlCommand("", connect2, transaction))
                                {
                                    try
                                    {
                                        command2.Parameters.AddWithValue("@p_name", TargetProduct.SelectedItem);
                                        while (reader.Read())
                                        {
                                            menuID = reader[0]?.ToString();
                                            command2.CommandText = $"SELECT * FROM {menuID} WHERE prod_name = @p_name";
                                            command2.ExecuteNonQuery();
                                            command2.CommandText = $"DELETE FROM {menuID} WHERE prod_name = @p_name";
                                            command2.ExecuteNonQuery();
                                        }
                                        transaction.Commit();
                                    }
                                    catch (MySqlException)
                                    {
                                        transaction.Rollback();
                                        MessageBox.Show("The selected product has not been removed from all menus, please try again.", "Remove menu content unsuccessful", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    if (TargetMenu.SelectedItem != null) LoadMenuContent(TargetMenu.SelectedItem.ToString());
                    MessageBox.Show("The selected product has been removed from all menus.", "Remove menu content successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ManageMenuContentsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadMenuContentComboboxes(2);

            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT price FROM espresso_shot", connect))
                {
                    double price = 0.0;
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read()) price = Convert.ToDouble(reader[0]);
                    }
                    CurrentShotPrice.Text = price.ToString("F2");
                }
            }

            ProductPanel.Visibility = Visibility.Collapsed;
            MenuPanel.Visibility = Visibility.Collapsed;
            MenuContentsPanel.Visibility = Visibility.Visible;
            ReorderCatalogsPanel.Visibility = Visibility.Collapsed;
            ChangeImagePanel.Visibility = Visibility.Collapsed;
        }

        private void LoadProductsToSwap()
        {
            if (string.IsNullOrEmpty(SelectedMenuSwap.Text)) return;
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT EXISTS(SELECT 1 FROM menus WHERE menu_name = @menuName)", connect))
                {
                    command.Parameters.AddWithValue("@menuName", SelectedMenuSwap.SelectedItem);
                    int result = Convert.ToInt32(command.ExecuteScalar());
                    if (result == 0)
                    {
                        MessageBox.Show("The selected menu does not exist, please try again.", "Non-existent menu", MessageBoxButton.OK, MessageBoxImage.Warning);
                        LoadMenuComboboxes(1);
                        return;
                    }
                    command.CommandText = "SELECT menu_id FROM menus WHERE menu_name = @menuName";
                    DataTable table = new DataTable();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command)) adapter.Fill(table);
                    string menuID = table.Rows[0]["menu_id"].ToString();

                    command.CommandText = $"SELECT prod_name FROM {menuID}";
                    table.Rows.Clear();
                    table.Columns.Clear();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command)) adapter.Fill(table);
                    var prodNames = table.AsEnumerable().Select(row => row["prod_name"]).ToList();
                    SelectedProductA.ItemsSource = prodNames;
                    SelectedProductA.SelectedIndex = 0;
                    SelectedProductB.ItemsSource = prodNames;
                    SelectedProductB.SelectedIndex = 0;
                }
            }
        }

        private bool CheckProductOnMenu(string prodName, string menuName)
        {
            if (string.IsNullOrEmpty(prodName) || string.IsNullOrEmpty(menuName)) return false;
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT EXISTS(SELECT 1 FROM menus WHERE menu_name = @menuName)", connect))
                {
                    command.Parameters.AddWithValue("@menuName", menuName);
                    int result = Convert.ToInt32(command.ExecuteScalar());
                    if (result == 0)
                    {
                        LoadMenuComboboxes(1);
                        return false;
                    }
                    command.CommandText = "SELECT menu_id FROM menus WHERE menu_name = @menuName";
                    DataTable table = new DataTable();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command)) adapter.Fill(table);
                    string menuID = table.Rows[0]["menu_id"].ToString();

                    command.CommandText = $"SELECT EXISTS(SELECT 1 FROM {menuID} WHERE prod_name = @productName)";
                    command.Parameters.AddWithValue("@productName", prodName);
                    result = Convert.ToInt32(command.ExecuteScalar());
                    if (result == 0)
                    {
                        LoadProductsToSwap();
                        return false;
                    }
                }
            }
            return true;
        }

        private void SelectedMenuSwap_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // SelectionChanged calls the method below before TargetMenu.Text changes
            // This results in LoadProductssToSwap() to load the previously selected menu, instead of the currently selected one
            // The code below attempts to fix this issue
            Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadProductsToSwap();
            }), DispatcherPriority.Background);
        }

        private void ViewMenusButton_Click(object sender, RoutedEventArgs e) => LoadMenuArrangement();

        private void ViewMenuContentsSwap_Click(object sender, RoutedEventArgs e) => LoadMenuContent(SelectedMenuSwap.Text);

        private void LoadMenuArrangement()
        {
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand($"SELECT menu_name as 'Menu Name' FROM menus", connect))
                {
                    DataTable table = new DataTable();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command)) adapter.Fill(table);
                    for (int i = 0; i < table.Rows.Count; ++i)
                    {
                        if (table.Rows[i]["Menu Name"].ToString() == "Home")
                        {
                            table.Rows.RemoveAt(i);
                            break;
                        }
                    }
                    MenuContentsTable.Columns[0].Visibility = Visibility.Collapsed;
                    MenuContentsTable.Columns[1].Visibility = Visibility.Visible;
                    table.Columns.Add("Appearance Arrangement");
                    int order = 1;
                    foreach (DataRow row in table.Rows)
                    {
                        row["Appearance Arrangement"] = order;
                        order += 1;
                    }
                    MenuContentsTable.ItemsSource = table.DefaultView;
                }
            }
        }

        private void ViewMenuArrangement_Click(object sender, RoutedEventArgs e) => LoadMenuArrangement();

        private void SwapProductsButton_Click(object sender, RoutedEventArgs e)
        {
            // No FK and PK column should be created to allow this operation to work
            if (SelectedProductA.Text == SelectedProductB.Text) return;
            if (string.IsNullOrEmpty(SelectedMenuSwap.Text) || string.IsNullOrEmpty(SelectedProductA.Text) || string.IsNullOrEmpty(SelectedProductB.Text)) return;
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand($"SELECT EXISTS(SELECT 1 from menus WHERE menu_name = @m_name LIMIT 1)", connect, transaction))
                    {
                        string menuID = "";
                        try
                        {
                            command.Parameters.AddWithValue("@m_name", SelectedMenuSwap.Text);
                            int res = Convert.ToInt32(command.ExecuteScalar());
                            if (res == 0)
                            {
                                MessageBox.Show("The selected menu does not exist in the database, please try again.", "Target menu does not exist", MessageBoxButton.OK, MessageBoxImage.Error);
                                LoadMenuComboboxes(1);
                                return;
                            }
                            if (CheckProductOnMenu(SelectedProductA.Text, SelectedMenuSwap.Text) == false)
                            {
                                MessageBox.Show("Product A does not exist in the target menu, or the target menu is non-existent, please try again.", "Product A does not exist on selected menu", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            if (CheckProductOnMenu(SelectedProductB.Text, SelectedMenuSwap.Text) == false)
                            {
                                MessageBox.Show("Product B does not exist in the target menu, or the target menu is non-existent, please try again.", "Product B does not exist on selected menu", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            // Retrieve menu ID
                            command.CommandText = "SELECT menu_id FROM menus WHERE menu_name = @m_name LIMIT 1";
                            DataTable menuData = new DataTable();
                            using (MySqlDataAdapter adapter = new MySqlDataAdapter(command)) adapter.Fill(menuData);
                            menuID = menuData.Rows[0]["menu_id"].ToString();

                            // Temporarily lock the database to prevent swaps under the same table
                            command.CommandText = $"SELECT GET_LOCK('{menuID}', 20)";
                            command.ExecuteNonQuery();

                            string origProductAName = SelectedProductA.Text;
                            string origProductBName = SelectedProductB.Text;

                            // Change name of product A and B to placeholder values to avoid primary key issue
                            command.Parameters.AddWithValue("@prodAName", origProductAName);
                            command.Parameters.AddWithValue("@prodBName", origProductBName);

                            command.CommandText = $"UPDATE {menuID} SET prod_name = 'ToSwapA' WHERE prod_name = @prodAName LIMIT 1";
                            command.ExecuteNonQuery();

                            command.CommandText = $"UPDATE {menuID} SET prod_name = 'ToSwapB' WHERE prod_name = @prodBName LIMIT 1";
                            command.ExecuteNonQuery();

                            // Swap name of product A and product B, since there is only 1 column
                            command.CommandText = $"UPDATE {menuID} SET prod_name = @prodBName WHERE prod_name = 'ToSwapA' LIMIT 1";
                            command.ExecuteNonQuery();

                            command.CommandText = $"UPDATE {menuID} SET prod_name = @prodAName WHERE prod_name = 'ToSwapB' LIMIT 1";
                            command.ExecuteNonQuery();

                            command.CommandText = $"SELECT RELEASE_LOCK('{menuID}')";
                            command.ExecuteNonQuery();

                            transaction.Commit();
                            LoadProductsToSwap();
                            LoadMenuContent(SelectedMenuSwap.Text);
                            MessageBox.Show("The selected products have been swapped successfully.", "Swap successful", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (MySqlException)
                        {
                            if (!string.IsNullOrWhiteSpace(menuID))
                            {
                                command.CommandText = $"SELECT RELEASE_LOCK('{menuID}')";
                                command.ExecuteNonQuery();
                            }
                            transaction.Rollback();
                            MessageBox.Show("The selected products have not been swapped, please try again.", "Swap unsuccessful", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
        }

        private void SwapMenusButton_Click(object sender, RoutedEventArgs e)
        {
            // No FK and PK column should be created to allow this operation to work
            if (SelectedMenuA.Text == SelectedMenuB.Text) return;
            if (string.IsNullOrEmpty(SelectedMenuA.Text) || string.IsNullOrEmpty(SelectedMenuB.Text)) return;
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand($"SELECT EXISTS(SELECT 1 from menus WHERE menu_name = @menuAName LIMIT 1)", connect, transaction))
                    {
                        try
                        {
                            command.Parameters.AddWithValue("@menuAName", SelectedMenuA.Text);
                            int res = Convert.ToInt32(command.ExecuteScalar());
                            if (res == 0)
                            {
                                MessageBox.Show("Menu A does not exist in the database, please try again.", "Menu A does not exist", MessageBoxButton.OK, MessageBoxImage.Error);
                                LoadMenuComboboxes(1);
                                return;
                            }
                            command.Parameters["@menuAName"].Value = SelectedMenuB.Text;
                            res = Convert.ToInt32(command.ExecuteScalar());
                            if (res == 0)
                            {
                                MessageBox.Show("Menu B does not exist in the database, please try again.", "Menu B does not exist", MessageBoxButton.OK, MessageBoxImage.Error);
                                LoadMenuComboboxes(1);
                                return;
                            }

                            // Lock the entire menus table to ensure thread safety
                            command.CommandText = "SELECT GET_LOCK('menus', 20)";
                            command.ExecuteNonQuery();

                            // Cache original name
                            string origMenuAName = SelectedMenuA.Text;
                            string origMenuBName = SelectedMenuB.Text;

                            // Cache rows of menu A and B, define parameters once
                            command.Parameters["@menuAName"].Value = origMenuAName;
                            command.Parameters.AddWithValue("@menuBName", origMenuBName);

                            DataTable menuAData = new DataTable();
                            command.CommandText = "SELECT menu_id, is_hidden, menu_image, start_time, end_time, start_date, end_date FROM menus WHERE menu_name = @menuAName LIMIT 1";
                            using (MySqlDataAdapter adapter = new MySqlDataAdapter(command)) adapter.Fill(menuAData);

                            DataTable menuBData = new DataTable();
                            command.CommandText = "SELECT menu_id, is_hidden, menu_image, start_time, end_time, start_date, end_date FROM menus WHERE menu_name = @menuBName LIMIT 1";
                            using (MySqlDataAdapter adapter = new MySqlDataAdapter(command)) adapter.Fill(menuBData);

                            // Prepare menu A and menu B for swapping by renaming
                            command.CommandText = "UPDATE menus SET menu_name = 'ToSwapA' WHERE menu_name = @menuAName LIMIT 1";
                            command.ExecuteNonQuery();

                            command.CommandText = "UPDATE menus SET menu_name = 'ToSwapB' WHERE menu_name = @menuBName LIMIT 1";
                            command.ExecuteNonQuery();

                            // Swap values of menu A and menu B
                            // Define the parameters once, to avoid clearing and readding
                            command.Parameters.AddWithValue("@menuID", menuBData.Rows[0]["menu_id"]);
                            command.Parameters.AddWithValue("@isHidden", menuBData.Rows[0]["is_hidden"]);
                            command.Parameters.AddWithValue("@menuImage", menuBData.Rows[0]["menu_image"]);
                            command.Parameters.AddWithValue("@startTime", menuBData.Rows[0]["start_time"]);
                            command.Parameters.AddWithValue("@endTime", menuBData.Rows[0]["end_time"]);
                            command.Parameters.AddWithValue("@startDate", menuBData.Rows[0]["start_date"]);
                            command.Parameters.AddWithValue("@endDate", menuBData.Rows[0]["end_date"]);

                            command.CommandText = "UPDATE menus SET menu_id = @menuID, is_hidden = @isHidden, menu_image = @menuImage, start_time = @startTime, end_time = @endTime, start_date = @startDate, end_date = @endDate WHERE menu_name = 'ToSwapA' LIMIT 1";
                            command.ExecuteNonQuery();

                            command.CommandText = "UPDATE menus SET menu_name = @menuBName WHERE menu_name = 'ToSwapA' LIMIT 1";
                            command.ExecuteNonQuery();

                            command.CommandText = "UPDATE menus SET menu_id = @menuID, is_hidden = @isHidden, menu_image = @menuImage, start_time = @startTime, end_time = @endTime, start_date = @startDate, end_date = @endDate WHERE menu_name = 'ToSwapB' LIMIT 1";
                            // Since the parameters do not change, only the values, reuse the parameters
                            command.Parameters["@menuID"].Value = menuAData.Rows[0]["menu_id"];
                            command.Parameters["@isHidden"].Value = menuAData.Rows[0]["is_hidden"];
                            command.Parameters["@menuImage"].Value = menuAData.Rows[0]["menu_image"];
                            command.Parameters["@startTime"].Value = menuAData.Rows[0]["start_time"];
                            command.Parameters["@endTime"].Value = menuAData.Rows[0]["end_time"];
                            command.Parameters["@startDate"].Value = menuAData.Rows[0]["start_date"];
                            command.Parameters["@endDate"].Value = menuAData.Rows[0]["end_date"];
                            command.ExecuteNonQuery();

                            command.CommandText = "UPDATE menus SET menu_name = @menuAName WHERE menu_name = 'ToSwapB' LIMIT 1";
                            command.ExecuteNonQuery();

                            // Unlock the table when done
                            command.CommandText = "SELECT RELEASE_LOCK('menus')";
                            command.ExecuteNonQuery();

                            transaction.Commit();
                            LoadMenuArrangement();
                            LoadMenuComboboxes(1);
                            MessageBox.Show("The selected menus have been swapped successfully.", "Swap successful", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (MySqlException)
                        {
                            command.CommandText = "SELECT RELEASE_LOCK('menus')";
                            command.ExecuteNonQuery();
                            transaction.Rollback();
                            MessageBox.Show("The selected menus have not been swapped successfully, please try again.", "Menu swap unsuccessful", MessageBoxButton.OK, MessageBoxImage.Error);
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
                AdminFunctions.Visibility= Visibility.Visible;
                PanelRevealed = true;
            }
        }

        private void InitializeShotValue()
        {
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT GET_LOCK('espresso_shot', 10)", connect, transaction))
                    {
                        try
                        {
                            // Lock the table
                            command.ExecuteNonQuery();
                            command.CommandText = "SELECT COUNT(*) FROM espresso_shot LIMIT 2";
                            int rowCount = Convert.ToInt32(command.ExecuteScalar());
                            if (rowCount == 0)
                            {
                                command.CommandText = "INSERT INTO espresso_shot VALUES(40.00)";
                                command.ExecuteNonQuery();
                            }
                            else if (rowCount > 1)
                            {
                                command.CommandText = "DELETE FROM espresso_shot";
                                command.ExecuteNonQuery();
                                command.CommandText = "INSERT INTO espresso_shot VALUES(40.00)";
                                command.ExecuteNonQuery();
                            }
                            transaction.Commit();
                            command.CommandText = "SELECT RELEASE_LOCK('espresso_shot')";
                            command.ExecuteNonQuery();
                        }
                        catch (MySqlException)
                        {
                            transaction.Rollback();
                            command.CommandText = "SELECT RELEASE_LOCK('espresso_shot')";
                            command.ExecuteNonQuery();                           
                        }
                    }
                }
            }
        }

        private void UpdateShotPrice_Click(object sender, RoutedEventArgs e)
        {
            double newPrice = 0.0;
            if (!Double.TryParse(NewShotPrice.Text, out newPrice) || newPrice >= 999999 || newPrice < 0.0)
            {
                MessageBox.Show("Please ensure that the price is valid.", "Invalid shot price", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            InitializeShotValue();
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT GET_LOCK('espresso_shot', 10)", connect, transaction))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                            command.CommandText = "UPDATE espresso_shot SET price = @n_price LIMIT 1";
                            command.Parameters.AddWithValue("@n_price", newPrice);
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            command.CommandText = "SELECT RELEASE_LOCK('espresso_shot')";
                            command.ExecuteNonQuery();
                            CurrentShotPrice.Text = newPrice.ToString("F2");
                        }
                        catch (MySqlException)
                        {
                            command.CommandText = "SELECT RELEASE_LOCK('espresso_shot')";
                            command.ExecuteNonQuery();
                            transaction.Rollback();
                        }
                    }
                }
            }
        }

        private void LoadFoodPlaceholder_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp" };
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == true)
            {
                var bitmap = new BitmapImage(new Uri(dlg.FileName));
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.Freeze();

                int width = bitmap.PixelWidth;
                int height = bitmap.PixelHeight;
                if (width < 100 || height < 100)
                {
                    MessageBox.Show("The selected image has too low resolution, please select another image.", "Too low image resolution", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                FoodPlaceholderImage.Source = bitmap;

                using (var ms = new MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(ms);
                    selectedFoodPlaceholder = ms.ToArray();
                }
            }
        }

        private void RemoveFoodPlaceholder_Click(object sender, RoutedEventArgs e)
        {
            FoodPlaceholderImage.Source = null;
            selectedFoodPlaceholder = null;
        }

        private void ApplyFoodPlaceholder_Click(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand($"SELECT COUNT(*) from ordering_images WHERE image_name = @imgName", connect, transaction))
                    {
                        try
                        {
                            command.Parameters.AddWithValue("@imgName", "food_placeholder");
                            int result = Convert.ToInt32(command.ExecuteScalar());
                            if (result == 0) command.CommandText = $"INSERT INTO ordering_images VALUES(@imgName, @img)";
                            else
                            {
                                command.CommandText = "SELECT * FROM ordering_images WHERE image_name = @imgName FOR UPDATE";
                                command.ExecuteNonQuery();
                                command.CommandText = $"UPDATE ordering_images SET image = @img WHERE image_name = @imgName";
                            }
                            command.Parameters.AddWithValue("@img", selectedFoodPlaceholder);
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            selectedFoodPlaceholder = null;
                            FoodPlaceholderImage.Source = null;
                            MessageBox.Show("The food placeholder image has been uploaded successfully.", "Food placeholder image applied", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (MySqlException)
                        {
                            MessageBox.Show("The food placeholder image has not been uploaded successfully, please try again.", "Food placeholder image not applied", MessageBoxButton.OK, MessageBoxImage.Warning);
                            transaction.Rollback();
                        }
                    }
                }
            }
        }

        private void LoadMenuPlaceholder_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp" };
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == true)
            {
                var bitmap = new BitmapImage(new Uri(dlg.FileName));
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.Freeze();

                int width = bitmap.PixelWidth;
                int height = bitmap.PixelHeight;
                if (width < 50 || height < 50)
                {
                    MessageBox.Show("The selected image has too low resolution, please select another image.", "Too low image resolution", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MenuPlaceholderImage.Source = bitmap;

                using (var ms = new MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(ms);
                    selectedMenuPlaceholder = ms.ToArray();
                }
            }
        }

        private void RemoveMenuPlaceholder_Click(object sender, RoutedEventArgs e)
        {
            MenuPlaceholderImage.Source = null;
            selectedMenuPlaceholder = null;
        }

        private void ApplyMenuPlaceholder_Click(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand($"SELECT COUNT(*) from ordering_images WHERE image_name = @imgName", connect, transaction))
                    {
                        try
                        {
                            command.Parameters.AddWithValue("@imgName", "menu_placeholder");
                            int result = Convert.ToInt32(command.ExecuteScalar());
                            if (result == 0) command.CommandText = $"INSERT INTO ordering_images VALUES(@imgName, @img)";
                            else
                            {
                                command.CommandText = "SELECT * FROM ordering_images WHERE image_name = @imgName FOR UPDATE";
                                command.ExecuteNonQuery();
                                command.CommandText = $"UPDATE ordering_images SET image = @img WHERE image_name = @imgName";
                            }
                            command.Parameters.AddWithValue("@img", selectedMenuPlaceholder);
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            selectedMenuPlaceholder = null;
                            MenuPlaceholderImage.Source = null;
                            MessageBox.Show("The menu placeholder image has been uploaded successfully.", "Menu placeholder image applied", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (MySqlException)
                        {
                            MessageBox.Show("The menu placeholder image has not been uploaded successfully, please try again.", "Menu placeholder image not applied", MessageBoxButton.OK, MessageBoxImage.Warning);
                            transaction.Rollback();
                        }
                    }
                }
            }
        }

        private void LoadOrderImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp" };
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == true)
            {
                var bitmap = new BitmapImage(new Uri(dlg.FileName));
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.Freeze();

                int width = bitmap.PixelWidth;
                int height = bitmap.PixelHeight;
                if (width < 140 || height < 140)
                {
                    MessageBox.Show("The selected image has too low resolution, please select another image.", "Too low image resolution", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                OrderMenuImage.Source = bitmap;

                using (var ms = new MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(ms);
                    selectedOrderImage = ms.ToArray();
                }
            }
        }

        private void RemoveOrderImage_Click(object sender, RoutedEventArgs e)
        {
            OrderMenuImage.Source = null;
            selectedOrderImage = null;
        }

        private void ApplyInterfaceImage_Click(object sender, RoutedEventArgs e)
        {
            if (selectedOrderImage == null)
            {
                MessageBoxResult result = MessageBox.Show("This operation will remove the ordering interface image, do you want to continue?", "Remove irdering interface image confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No) return;
            }
            using (MySqlConnection connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand($"SELECT COUNT(*) from ordering_images WHERE image_name = @imgName", connect, transaction))
                    {
                        try
                        {
                            command.Parameters.AddWithValue("@imgName", "interface_image");
                            int result = Convert.ToInt32(command.ExecuteScalar());
                            if (result == 0) command.CommandText = $"INSERT INTO ordering_images VALUES(@imgName, @img)";
                            else
                            {
                                command.CommandText = "SELECT * FROM ordering_images WHERE image_name = @imgName FOR UPDATE";
                                command.ExecuteNonQuery();
                                command.CommandText = $"UPDATE ordering_images SET image = @img WHERE image_name = @imgName";
                            }
                            command.Parameters.AddWithValue("@img", selectedOrderImage);
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            selectedOrderImage = null;
                            OrderMenuImage.Source = null;
                            MessageBox.Show("The ordering image has been uploaded successfully.", "Ordering image applied", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (MySqlException)
                        {
                            MessageBox.Show("The ordering image has not been uploaded successfully, please try again.", "Ordering image not applied", MessageBoxButton.OK, MessageBoxImage.Warning);
                            transaction.Rollback();
                        }
                    }
                }
            }
        }

        private void PreviewImagesButton_Click(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT * FROM ordering_images", connect))
                {
                    DataTable table = new DataTable();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command)) adapter.Fill(table);

                    foreach (DataRow row in table.Rows)
                    {
                        byte[] imageBytes = row["image"] as byte[];
                        if (imageBytes == null || imageBytes.Length == 0) continue;
                        BitmapImage picture = new BitmapImage();
                        using (MemoryStream ms = new MemoryStream(imageBytes))
                        {
                            ms.Position = 0;
                            picture.BeginInit();
                            picture.CacheOption = BitmapCacheOption.OnLoad;
                            picture.StreamSource = ms;
                            picture.EndInit();
                            picture.Freeze(); // Makes it cross-thread accessible
                        }
                        string name = row["image_name"].ToString();
                        switch (name)
                        {
                            case "interface_image":
                                OrderMenuImage.Source = picture;
                                selectedOrderImage = imageBytes;
                                break;
                            case "menu_placeholder":
                                MenuPlaceholderImage.Source = picture;
                                selectedMenuPlaceholder = imageBytes;
                                break;
                            case "food_placeholder":
                                FoodPlaceholderImage.Source = picture;
                                selectedFoodPlaceholder = imageBytes;
                                break;
                        }
                    }
                }
            }
        }

        private void MilkPage_SelectionChanged(object sender, SelectionChangedEventArgs e) => selectedMilkMenu = MilkPage.SelectedIndex;

        private void AddonPage_SelectionChanged(object sender, SelectionChangedEventArgs e) => selectedAddonMenu = AddonPage.SelectedIndex;

        private void SyrupPage_SelectionChanged(object sender, SelectionChangedEventArgs e) => selectedSyrupMenu = SyrupPage.SelectedIndex;

        private void SweetenerPage_SelectionChanged(object sender, SelectionChangedEventArgs e) => selectedSweetMenu = SweetenerPage.SelectedIndex;

        private void BasePage_SelectionChanged(object sender, SelectionChangedEventArgs e) => selectedBaseMenu = BasePage.SelectedIndex;

        private void VariantPage_SelectionChanged(object sender, SelectionChangedEventArgs e) => selectedVariantMenu = VariantPage.SelectedIndex;

    }
}

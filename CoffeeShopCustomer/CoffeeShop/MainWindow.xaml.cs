using Konscious.Security.Cryptography;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace CoffeeShopCustomer
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

    public class Base
    {
        public string BaseName { get; set; }
        public int BaseQuantity { get; set; }
        public double BasePrice { get; set; }

        [JsonConstructor]
        public Base(string BaseName, int BaseQuantity, double BasePrice)
        {
            this.BaseName = BaseName;
            this.BaseQuantity = BaseQuantity;
            this.BasePrice = BasePrice;
        }

        public Base(string baseName)
        {
            this.BaseName = baseName;
            this.BaseQuantity = 0;
            this.BasePrice = 0.0;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + (BaseName?.GetHashCode() ?? 0);
            return hash;
        }

        public override bool Equals(object obj)
        {
            Base other = obj as Base;
            if (other == null) return false;
            return this.BaseName == other.BaseName;
        }
    }

    public class Milk
    {
        public string MilkName { get; set; }
        public int MilkQuantity { get; set; }
        public double MilkPrice { get; set; }

        [JsonConstructor]
        public Milk(string MilkName, int MilkQuantity, double MilkPrice)
        {
            this.MilkName = MilkName;
            this.MilkQuantity = MilkQuantity;
            this.MilkPrice = MilkPrice;
        }
        public Milk(string milkName)
        {
            this.MilkName = milkName;
            this.MilkQuantity = 0;
            this.MilkPrice = 0.0;
        }

        public override int GetHashCode()
        {
            int hash = 16;
            hash = hash * 25 + (MilkName?.GetHashCode() ?? 0);
            return hash;
        }

        public override bool Equals(object obj)
        {
            Milk other = obj as Milk;
            if (other == null) return false;
            return this.MilkName == other.MilkName;
        }
    }

    public class Addon
    {
        public string AddonName { get; set; }
        public int AddonQuantity { get; set; }
        public double AddonPrice { get; set; }

        [JsonConstructor]
        public Addon(string AddonName, int AddonQuantity, double AddonPrice)
        {
            this.AddonName = AddonName;
            this.AddonQuantity = AddonQuantity;
            this.AddonPrice = AddonPrice;
        }

        public Addon(string addonName)
        {
            this.AddonName = addonName;
            this.AddonQuantity = 0;
            this.AddonPrice = 0.0;
        }

        public override int GetHashCode()
        {
            int hash = 15;
            hash = hash * 21 + (AddonName?.GetHashCode() ?? 0);
            return hash;
        }

        public override bool Equals(object obj)
        {
            Addon other = obj as Addon;
            if (other == null) return false;
            return this.AddonName == other.AddonName;
        }
    }

    public class Syrup
    {
        public string SyrupName { get; set; }
        public int SyrupQuantity { get; set; }
        public double SyrupPrice { get; set; }

        [JsonConstructor]
        public Syrup(string SyrupName, int SyrupQuantity, double SyrupPrice)
        {
            this.SyrupName = SyrupName;
            this.SyrupQuantity = SyrupQuantity;
            this.SyrupPrice = SyrupPrice;
        }


        public Syrup(string syrupName)
        {
            this.SyrupName = syrupName;
            this.SyrupQuantity = 0;
            this.SyrupPrice = 0.0;
        }
        public override int GetHashCode()
        {
            int hash = 13;
            hash = hash * 22 + (SyrupName?.GetHashCode() ?? 0);
            return hash;
        }

        public override bool Equals(object obj)
        {
            Syrup other = obj as Syrup;
            if (other == null) return false;
            return this.SyrupName == other.SyrupName;
        }
    }

    public class Sweetener
    {
        public string SweetenerName { get; set; }
        public int SweetenerQuantity { get; set; }
        public double SweetenerPrice { get; set; }

        [JsonConstructor]
        public Sweetener(string SweetenerName, int SweetenerQuantity, double SweetenerPrice)
        {
            this.SweetenerName = SweetenerName;
            this.SweetenerQuantity = SweetenerQuantity;
            this.SweetenerPrice = SweetenerPrice;
        }
        public Sweetener(string sweetName)
        {
            this.SweetenerName = sweetName;
            this.SweetenerQuantity = 0;
            this.SweetenerPrice = 0.0;
        }

        public override int GetHashCode()
        {
            int hash = 19;
            hash = hash * 27 + (SweetenerName?.GetHashCode() ?? 0);
            return hash;
        }

        public override bool Equals(object obj)
        {
            Sweetener other = obj as Sweetener;
            if (other == null) return false;
            return this.SweetenerName == other.SweetenerName;
        }
    }

    public class Product
    {
        public string ProductName { get; set; }
        public double BasePrice { get; set; }
        public int ProductQuantity { get; set; }
        public double ProductPrice { get; set; }
        public int ProductSize { get; set; }
        public int ProductTemperature { get; set; }
        public int EspressoShotAmount { get; set; }
        public double EspressoShotPrice { get; set; }
        public int IceLevel { get; set; }
        public int ReheatProduct { get; set; }
		public int Stamps { get; set; }
        public double Points { get; set; }
        public HashSet<Milk> ProductMilk { get; set; } = new HashSet<Milk>();
        public HashSet<Addon> ProductAddon { get; set; } = new HashSet<Addon>();
        public HashSet<Syrup> ProductSyrup { get; set; } = new HashSet<Syrup>();
        public HashSet<Sweetener> ProductSweetener { get; set; } = new HashSet<Sweetener>();
        public HashSet<Base> ProductBase { get; set; } = new HashSet<Base>();
        public byte[] ProductImage { get; set; }

        [JsonConstructor]
        public Product(string ProductName, double BasePrice, int ProductQuantity, double ProductPrice, int ProductSize, int ProductTemperature, int EspressoShotAmount, double EspressoShotPrice, int ReheatProduct, int IceLevel, int Stamps, double Points, HashSet<Milk> ProductMilk, HashSet<Addon> ProductAddon, HashSet<Syrup> ProductSyrup, HashSet<Sweetener> ProductSweetener, HashSet<Base> ProductBase, byte[] ProductImage)
        {
            this.ProductName = ProductName;
            this.BasePrice = BasePrice;
            this.ProductQuantity = ProductQuantity;
            this.ProductPrice = ProductPrice;
            this.ProductSize = ProductSize;
            this.ProductTemperature = ProductTemperature;
            this.EspressoShotAmount = EspressoShotAmount;
            this.EspressoShotPrice = EspressoShotPrice;
            this.ReheatProduct = ReheatProduct;
            this.IceLevel = IceLevel;
			this.Stamps = Stamps;
            this.Points = Points;
            this.ProductMilk = ProductMilk;
            this.ProductAddon = ProductAddon;
            this.ProductSweetener = ProductSweetener;
            this.ProductSyrup = ProductSyrup;
            this.ProductBase = ProductBase;
            this.ProductImage = ProductImage;
        }

        public Product(string ProductName)
        {
            this.ProductName = ProductName;
            BasePrice = 0;
            ProductImage = null;
            ProductQuantity = 0;
            ProductPrice = 0;
            ProductSize = 0;
            ProductTemperature = 0;
            EspressoShotAmount = 0;
            EspressoShotPrice = 0.0;
            ReheatProduct = 0;
            IceLevel = 0;
        }

        public void AddBase(Base newBase) => this.ProductBase.Add(newBase);
        public void AddMilk(Milk newMilk) => this.ProductMilk.Add(newMilk);
        public void AddSyrup(Syrup newSyrup) => this.ProductSyrup.Add(newSyrup);
        public void AddSweetener(Sweetener newSweetener) => this.ProductSweetener.Add(newSweetener);
        public void AddAddon(Addon newAddon) => this.ProductAddon.Add(newAddon);

        public void RemoveBase(string baseName)
        {
            Base curBase = new Base(baseName, 0, 0.0);
            ProductBase.Remove(curBase);
        }

        public void RemoveMilk(string milkName)
        {
            Milk curMilk = new Milk(milkName, 0, 0.0);
            ProductMilk.Remove(curMilk);
        }

        public void RemoveSyrup(string syrupName)
        {
            Syrup curSyrup = new Syrup(syrupName, 0, 0.0);
            ProductSyrup.Remove(curSyrup);
        }

        public void RemoveSweet(string sweetName)
        {
            Sweetener curSweet = new Sweetener(sweetName, 0, 0.0);
            ProductSweetener.Remove(curSweet);
        }

        public void RemoveAddon(string addonName)
        {
            Addon curAddon = new Addon(addonName, 0, 0.0);
            ProductAddon.Remove(curAddon);
        }

        public bool SearchBase(string baseName)
        {
            Base curBase = new Base(baseName, 0, 0.0);
            return ProductBase.Contains(curBase);
        }

        public bool SearchMilk(string milkName)
        {
            Milk curMilk = new Milk(milkName, 0, 0.0);
            return ProductMilk.Contains(curMilk);
        }

        public bool SearchSyrup(string syrupName)
        {
            Syrup curSyrup = new Syrup(syrupName, 0, 0.0);
            return ProductSyrup.Contains(curSyrup);
        }

        public bool SearchSweet(string sweetName)
        {
            Sweetener curSweet = new Sweetener(sweetName, 0, 0.0);
            return ProductSweetener.Contains(curSweet);
        }

        public bool SearchAddon(string addonName)
        {
            Addon curAddon = new Addon(addonName, 0, 0.0);
            return ProductAddon.Contains(curAddon);
        }
    }

    public class Order
    {
        public int OrderNumber { get; set; }
        public int OrderMode { get; set; }
        public int ClaimMode { get; set; }
        public int TableNumber { get; set; }
        public List<Product> CurrentFoodCart { get; set; } = new List<Product>();

        // Make sure to use [JsonConstructor], and the parameter names in the constructor matches the class names, case-sensitive
        [JsonConstructor]
        public Order(int OrderNumber, int OrderMode, int ClaimMode, int TableNumber, List<Product> CurrentFoodCart)
        {
            this.OrderNumber = OrderNumber;
            this.OrderMode = OrderMode;
            this.ClaimMode = ClaimMode;
            this.TableNumber = TableNumber;
            this.CurrentFoodCart = CurrentFoodCart;
        }
    }

    public partial class MainWindow : Window
    {
        private static readonly string database_command = @"server=localhost;userid=root;password=;";
        private static readonly string cs = database_command + @"database=coffeeshop_database";
        private static int CurrentOrderMode = 0;
        private static int iceMode = 0; 
        private static int tempMode = 0;  // 1 = hot, 2 = cold
        private static int sizeMode = 0; // 1 = small, 2 = medium, 3 = large
        private static int serveWarm = 0;  // 0 = None, 1 = true, 2 = false
        private static int ProductCartLocation = -1;
        private static int CurrentClaimMode = 1;
        private string CurrentOrderFolderPath = @"C:\Users\louwe\Desktop\Orders";
        private string CurrentPromotionFileLocation = @"C:\Users\louwe\Desktop\videoplayback.mp4";
        private string CurrentBackgroundLocation = "";

        private static List<Product> ProductCart = new List<Product>();
        private static HashSet<Base> CurrentBase = new HashSet<Base>();
        private static HashSet<Milk> CurrentMilk = new HashSet<Milk>();
        private static HashSet<Addon> CurrentAddon = new HashSet<Addon>();
        private static HashSet<Syrup> CurrentSyrup = new HashSet<Syrup>();
        private static HashSet<Sweetener> CurrentSweet = new HashSet<Sweetener>();
        private static byte[] CurrentProductImage;
        private static bool EditMode = false;
        private static bool VariantMode = false;
        private bool AdminPanelShown = true;

        BitmapImage menuPlaceholder = new BitmapImage();
        BitmapImage productPlaceholder = new BitmapImage();
        BitmapImage interfaceLogo = new BitmapImage();
     
        public MainWindow()
        {
            InitializeComponent();           
            using (var connect = new MySqlConnection(database_command))
            {
                connect.Open();
                using (var command = new MySqlCommand("CREATE DATABASE IF NOT EXISTS coffeeshop_database", connect))
                {
                    command.ExecuteNonQuery();
                    connect.Close();
                    connect.ConnectionString = cs;
                    connect.Open();

                    command.CommandText = "CREATE TABLE IF NOT EXISTS espresso_shot(price DOUBLE NOT NULL DEFAULT 0.0)";
                    command.ExecuteNonQuery();
                    command.CommandText = "SELECT EXISTS(SELECT 1 FROM espresso_shot LIMIT 1)";
                    int rowCount = Convert.ToInt32(command.ExecuteScalar());
                    if (rowCount == 0)
                    {
                        command.CommandText = "INSERT INTO espresso_shot VALUES(40.00)";
                        command.ExecuteNonQuery();
                    }
                    command.CommandText = "CREATE TABLE IF NOT EXISTS espresso_shot(price DOUBLE NOT NULL DEFAULT 40.0)";
                    command.ExecuteNonQuery();
                    command.CommandText = "CREATE TABLE IF NOT EXISTS products(product_name VARCHAR(150) COLLATE utf8mb4_bin NOT NULL DEFAULT 'None', order_mode INT NOT NULL DEFAULT 3, product_price DOUBLE NOT NULL DEFAULT 0.0, price_scale INT NOT NULL DEFAULT 0, product_image MEDIUMBLOB, product_quantity INT NOT NULL DEFAULT 0, has_quantity BOOLEAN NOT NULL DEFAULT TRUE, has_discount BOOLEAN NOT NULL DEFAULT TRUE, variants_page VARCHAR(150) NOT NULL DEFAULT 'N/A', variants_id VARCHAR(13) NOT NULL DEFAULT 'N/A', milk_page VARCHAR(150) NOT NULL DEFAULT 'N/A', milk_id VARCHAR(13) NOT NULL DEFAULT 'N/A', milk_scale_value INT NOT NULL DEFAULT 0, max_milk INT NOT NULL DEFAULT 0, milk_required INT NOT NULL DEFAULT 0, addons_page VARCHAR(150) NOT NULL DEFAULT 'N/A', addons_id VARCHAR(13) NOT NULL DEFAULT 'N/A', addon_scale_value INT NOT NULL DEFAULT 0, max_addon INT NOT NULL DEFAULT 0, addon_required INT NOT NULL DEFAULT 0, syrup_page VARCHAR(150) NOT NULL DEFAULT 'N/A', syrup_id VARCHAR(13) NOT NULL DEFAULT 'N/A', syrup_scale_value INT NOT NULL DEFAULT 0, max_syrup INT NOT NULL DEFAULT 0, syrup_required INT NOT NULL DEFAULT 0, sweetener_page VARCHAR(150) NOT NULL DEFAULT 'N/A', sweet_id VARCHAR(13) NOT NULL DEFAULT 'N/A', sweetener_scale_value INT NOT NULL DEFAULT 0, max_sweetener INT NOT NULL DEFAULT 0, sweet_required INT NOT NULL DEFAULT 0, base_page VARCHAR(150) NOT NULL DEFAULT 'N/A', base_id VARCHAR(13) NOT NULL DEFAULT 'N/A', max_base INT NOT NULL DEFAULT 0, base_required INT NOT NULL DEFAULT 0, default_espresso_shot INT NOT NULL DEFAULT 0, espresso_scale_value INT NOT NULL DEFAULT 0, max_espresso_shot INT NOT NULL DEFAULT 0, has_espresso_shot BOOLEAN NOT NULL DEFAULT FALSE, can_serve_warm BOOLEAN NOT NULL DEFAULT FALSE, temp_mode INT NOT NULL DEFAULT 1, size_mode INT NOT NULL DEFAULT 0, start_date DATE, end_date DATE, start_time TIME, end_time TIME, points_value INT NOT NULL DEFAULT 0, stamps_value INT NOT NULL DEFAULT 0, PRIMARY KEY (product_name))";
                    command.ExecuteNonQuery();
                    command.CommandText = "CREATE TABLE IF NOT EXISTS menus(menu_name VARCHAR(150) COLLATE utf8mb4_bin NOT NULL DEFAULT 'None', menu_id VARCHAR(13) NOT NULL DEFAULT 'None', order_mode INT NOT NULL DEFAULT 3, menu_image MEDIUMBLOB, is_hidden BOOLEAN NOT NULL DEFAULT FALSE, start_date DATE, end_date DATE, start_time TIME, end_time TIME)";
                    command.ExecuteNonQuery();
                    command.CommandText = "SELECT EXISTS(SELECT 1 FROM menus WHERE menu_name = 'Home' LIMIT 1)";
                    rowCount = Convert.ToInt32(command.ExecuteScalar());
                    if (rowCount == 0)
                    {
                        command.CommandText = "INSERT INTO menus VALUES('Home', 'menu_14185951', NULL, false, NULL, NULL, NULL, NULL)";
                        command.ExecuteNonQuery();
                        command.CommandText = "CREATE TABLE menu_14185951(prod_name VARCHAR(100))";
                        command.ExecuteNonQuery();
                    }
                    command.CommandText = "CREATE TABLE IF NOT EXISTS ordering_images(image_name VARCHAR(16) PRIMARY KEY, image BLOB)";
                    command.ExecuteNonQuery();
                    command.CommandText = "CREATE TABLE IF NOT EXISTS transactions(receipt_number VARCHAR(23) NOT NULL, receipt_date DATETIME DEFAULT NULL, order_type VARCHAR(50) NOT NULL DEFAULT 'N/A', pwd_senior_id VARCHAR(75) NOT NULL DEFAULT 'N/A', pwd_senior_name VARCHAR(150) NOT NULL DEFAULT 'N/A', total_price DOUBLE NOT NULL DEFAULT 0.0, total_quantity INT NOT NULL DEFAULT 0, member_id VARCHAR(20) NOT NULL DEFAULT 'N/A', points_earned DOUBLE NOT NULL DEFAULT 0.0, stamps_earned INT NOT NULL DEFAULT 0, pwd_senior_disc DOUBLE NOT NULL DEFAULT 0.0, member_disc DOUBLE NOT NULL DEFAULT 0.0, total_disc DOUBLE NOT NULL DEFAULT 0.0, payment_method VARCHAR(50) NOT NULL DEFAULT 'N/A', transaction TEXT NOT NULL, PRIMARY KEY(receipt_number))";
                    command.ExecuteNonQuery();
                    command.CommandText = "CREATE TABLE IF NOT EXISTS order_number(value INT NOT NULL DEFAULT 1)";
                    command.ExecuteNonQuery();
                    InitializeOrderNumber();
                }
            }
            ReloadPromotionDisplay(CurrentPromotionFileLocation);
            LoadOrderingImages();

            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = System.IO.Path.Combine(basePath, "orderingfolder_location.json");
            if (File.Exists(filePath))
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        ReferenceHandler = ReferenceHandler.IgnoreCycles
                    };

                    string json = File.ReadAllText(filePath);
                    CurrentOrderFolderPath = JsonSerializer.Deserialize<string>(json, options);
                    OrderFolderLocation.Text = CurrentOrderFolderPath;
                }
                catch (JsonException) { SaveOrderFolderPath(); }
            }
            else SaveOrderFolderPath();

            filePath = Path.Combine(basePath, "promotionalfile_location.json");
            if (File.Exists(filePath))
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        ReferenceHandler = ReferenceHandler.IgnoreCycles
                    };

                    string json = File.ReadAllText(filePath);
                    CurrentPromotionFileLocation = JsonSerializer.Deserialize<string>(json, options);
                    PromotionFileLocation.Text = CurrentPromotionFileLocation;
                    ReloadPromotionDisplay(CurrentPromotionFileLocation);
                }
                catch (JsonException) { SavePromotionFilePath(); }
            }
            else SavePromotionFilePath();

            filePath = Path.Combine(basePath, "background_location.json");
            if (File.Exists(filePath))
            {
                try
                {
                    var options = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        ReferenceHandler = ReferenceHandler.IgnoreCycles
                    };

                    string json = File.ReadAllText(filePath);
                    CurrentBackgroundLocation = JsonSerializer.Deserialize<string>(json, options);
                    ApplyBackground();
                }
                catch (JsonException) { SaveBackgroundLocation(); }
            }
            else SaveBackgroundLocation();
        }

        public void LoadOrderingImages()
        {
            menuPlaceholder = new BitmapImage(new Uri("pack://application:,,,/forward.png"));
            productPlaceholder = new BitmapImage(new Uri("pack://application:,,,/CoffeeCup.png"));
            interfaceLogo = new BitmapImage(new Uri("pack://application:,,,/restaurant.png"));
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT image_name, image FROM ordering_images", connect))
                {
                    byte[] newMenuPlaceholder = null;
                    byte[] newProductPlaceholder = null;
                    byte[] newInterfaceLogo = null;

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string image_name = reader[0]?.ToString();
                            byte[] imageByte = (byte[])reader[1];
                            switch (image_name)
                            {
                                case "food_placeholder":
                                    newProductPlaceholder = imageByte;
                                    break;
                                case "interface_image":
                                    newInterfaceLogo = imageByte;
                                    break;
                                case "menu_placeholder":
                                    newMenuPlaceholder = imageByte;
                                    break;
                            }
                        }
                    }

                    if (newMenuPlaceholder != null && newMenuPlaceholder.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(newMenuPlaceholder))
                        {
                            ms.Position = 0;
                            menuPlaceholder = new BitmapImage();
                            menuPlaceholder.BeginInit();
                            menuPlaceholder.CacheOption = BitmapCacheOption.OnLoad;
                            menuPlaceholder.StreamSource = ms;
                            menuPlaceholder.EndInit();
                            menuPlaceholder.Freeze(); // Makes it cross-thread accessible
                        }
                    }

                    if (newProductPlaceholder != null && newProductPlaceholder.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(newProductPlaceholder))
                        {
                            ms.Position = 0;
                            productPlaceholder = new BitmapImage();
                            productPlaceholder.BeginInit();
                            productPlaceholder.CacheOption = BitmapCacheOption.OnLoad;
                            productPlaceholder.StreamSource = ms;
                            productPlaceholder.EndInit();
                            productPlaceholder.Freeze(); // Makes it cross-thread accessible
                        }
                    }

                    if (newInterfaceLogo != null && newInterfaceLogo.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(newInterfaceLogo))
                        {
                            ms.Position = 0;
                            interfaceLogo = new BitmapImage();
                            interfaceLogo.BeginInit();
                            interfaceLogo.CacheOption = BitmapCacheOption.OnLoad;
                            interfaceLogo.StreamSource = ms;
                            interfaceLogo.EndInit();
                            interfaceLogo.Freeze(); // Makes it cross-thread accessible
                            CompanyLogo.Source = interfaceLogo;
                            CartLogo.Source = interfaceLogo;
                        }
                    }
                }
            }
        }

        public void FillMenus()
        {
            MenusPanel.Children.Clear();
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT menu_name, menu_image, menu_id, is_hidden, order_mode FROM menus", connect))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            bool isHidden = (bool)reader[3];
                            int orderMode = (int)reader[4];
                            if (isHidden) continue;
                            if (orderMode != 3 && orderMode != CurrentOrderMode) continue;
                            string name = reader[0]?.ToString();
                            if (name == "Home") continue;
                            byte[] img = null;
                            if (reader[1] != DBNull.Value) img = (byte[])reader[1];
                            string id = reader[2]?.ToString();
                            if (!string.IsNullOrEmpty(name))
                            {
                                if (IsTargetVisible(name, true) == false) continue;
                                StackPanel menuPanel = new StackPanel();
                                menuPanel.Orientation = Orientation.Horizontal;
                                menuPanel.Background = new SolidColorBrush(Colors.Transparent);
                                menuPanel.Width = 150;
                                menuPanel.Height = 60;
                                menuPanel.Margin = new Thickness(0, 5, 0, 1);

                                Image menuImg = new Image();
                                menuImg.Width = 50;
                                menuImg.Height = 50;
                                if (img != null)
                                {
                                    using (MemoryStream ms = new MemoryStream(img))
                                    {
                                        BitmapImage bitmapImage = new BitmapImage();
                                        bitmapImage.BeginInit();
                                        bitmapImage.StreamSource = ms;
                                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;  // Forces the loaded image to appear 
                                        bitmapImage.EndInit();
                                        bitmapImage.Freeze();
                                        menuImg.Source = bitmapImage;
                                    }
                                }
                                else menuImg.Source = menuPlaceholder;
                                menuPanel.Children.Add(menuImg);

                                TextBlock menuID = new TextBlock();
                                menuID.Text = id;
                                menuID.Width = 0;
                                menuID.Height = 0;
                                
                                TextBlock menuName = new TextBlock();
                                menuName.Text = name;
                                if (name.Length <= 16) menuName.Height = 16;
                                else if (name.Length > 16 && name.Length <= 32) menuName.Height = 33;
                                else if (name.Length > 32 && name.Length <= 48) menuName.Height = 50;
                                menuName.FontFamily = new FontFamily("Cascadia Mono");
                                menuName.Width = 100;
                                menuName.Margin = new Thickness(22, 0, 0, 0);
                                menuName.TextWrapping = TextWrapping.Wrap;
                                menuName.TextAlignment = TextAlignment.Left;

                                menuPanel.Children.Add(menuName);
                                menuPanel.Children.Add(menuID);

                                Button button = new Button();
                                button.FocusVisualStyle = null;
                                button.Content = menuPanel;
                                button.BorderThickness = new Thickness(0);
                                button.BorderBrush = new SolidColorBrush(Colors.Transparent);
                                button.Background = new SolidColorBrush(Colors.Transparent);
                                button.Click += (s, e) =>
                                {
                                    DisplayMenuName.Text = menuName.Text;
                                    FillProducts(menuID.Text, false);
                                };

                                // disable the default border of the button
                                button.Template = new ControlTemplate(typeof(Button))
                                {
                                    VisualTree = new FrameworkElementFactory(typeof(ContentPresenter))
                                };

                                Border buttonBorder = new Border();
                                buttonBorder.Margin = new Thickness(0, 0, 0, 1);
                                buttonBorder.BorderBrush = new SolidColorBrush(Colors.Black);
                                buttonBorder.BorderThickness = new Thickness(0,0,0,1);
                                buttonBorder.Background = new SolidColorBrush(Colors.Transparent);

                                buttonBorder.Child = button;

                                MenusPanel.Children.Add(buttonBorder);
                            }
                        }
                    }
                }
            }
        }

        private void FillProducts(string id, bool variantMenu)
        {
            if (!variantMenu) ProductsPanel.Children.Clear();
            ProductVariants.Children.Clear();
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (var connect2 = new MySqlConnection(cs))
                {
                    connect2.Open();
                    using (MySqlCommand command = new MySqlCommand($"SELECT prod_name FROM {id}", connect))
                    {
                        using (MySqlCommand command2 = new MySqlCommand($"SELECT product_name, product_price, product_image, product_quantity, has_quantity, variants_id, order_mode FROM products WHERE product_name = @productName", connect2))
                        {
                            command2.Parameters.AddWithValue("@productName", "");
							command2.Prepare();
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string pName = reader[0]?.ToString();
                                    if (!string.IsNullOrEmpty(pName))
                                    {
                                        if (IsTargetVisible(pName, false) == false) continue;
                                        command2.Parameters["@productName"].Value = pName;

                                        DataTable table = new DataTable();
                                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command2))
                                        { adapter.Fill(table); }

                                        int quantity = (int)table.Rows[0]["product_quantity"];
                                        bool has_quantity = Convert.ToBoolean(table.Rows[0]["has_quantity"]);
                                        int orderMode = Convert.ToInt32(table.Rows[0]["order_mode"]);
                                        if (has_quantity == true && quantity == 0) continue;
                                        if (orderMode != 3 && orderMode != CurrentOrderMode) continue;

                                        Border productBorder = new Border();
                                        productBorder.Width = 172;
                                        productBorder.Height = 207;
                                        productBorder.CornerRadius = new CornerRadius(5);
                                        productBorder.BorderThickness = new Thickness(1);
                                        productBorder.Margin = new Thickness(0, 0, 10, 10);
                                        productBorder.BorderBrush = new SolidColorBrush(Colors.Black);
                                        productBorder.Background = new SolidColorBrush(Colors.Transparent);
                                        productBorder.MouseEnter += Product_MouseEnter;
                                        productBorder.MouseLeave += Product_MouseLeave;

                                        StackPanel productPanel = new StackPanel();
                                        productPanel.Width = 172;
                                        productPanel.Orientation = Orientation.Vertical;

                                        string vName = (string)table.Rows[0]["variants_id"];
                                        TextBlock variantName = new TextBlock();
                                        variantName.Width = 0;
                                        variantName.Height = 0;
                                        if (!string.IsNullOrEmpty(vName)) variantName.Text = vName;
                                        else variantName.Text = "N/A";
                                        productPanel.Children.Add(variantName);

                                        Image productImg = new Image();
                                        byte[] img = null;
                                        if (table.Rows[0]["product_image"] != DBNull.Value) img = (byte[])table.Rows[0]["product_image"];
                                        productImg.Width = 120;
                                        productImg.Height = 120;
                                        if (img != null)
                                        {
                                            using (MemoryStream ms = new MemoryStream(img))
                                            {
                                                BitmapImage bitmapImage = new BitmapImage();
                                                bitmapImage.BeginInit();
                                                bitmapImage.StreamSource = ms;
                                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;  // Forces the loaded image to appear 
                                                bitmapImage.EndInit();
                                                bitmapImage.Freeze();
                                                productImg.Source = bitmapImage;
                                            }
                                        }
                                        else productImg.Source = productPlaceholder;
                                        productPanel.Children.Add(productImg);

                                        string productName = table.Rows[0]["product_name"].ToString();
                                        TextBlock nameBlock = new TextBlock();
                                        nameBlock.FontWeight = FontWeights.SemiBold;
                                        nameBlock.Text = productName;
                                        nameBlock.TextWrapping = TextWrapping.Wrap;
                                        nameBlock.Width = 160;
                                        if (productName.Length <= 24)
                                        {
                                            nameBlock.Height = 17;
                                            nameBlock.Margin = new Thickness(5, 29, 0, 5);
                                        }
                                        else if (productName.Length > 24 && productName.Length <= 41)
                                        {
                                            nameBlock.Height = 34;
                                            nameBlock.Margin = new Thickness(5, 13, 0, 5);
                                        }
                                        else if (productName.Length > 41 && productName.Length <= 69)
                                        {
                                            nameBlock.Height = 51;
                                            nameBlock.Margin = new Thickness(5, 0, 0, 5);
                                        }
                                        nameBlock.FontSize = 13;
                                        productPanel.Children.Add(nameBlock);

                                        StackPanel productPriceArea = new StackPanel();
                                        productPriceArea.Width = 154;
                                        productPriceArea.Orientation = Orientation.Horizontal;

                                        TextBlock phpTxt = new TextBlock();
                                        phpTxt.Text = "PHP ";
                                        phpTxt.FontSize = 13;
                                        productPriceArea.Children.Add(phpTxt);

                                        TextBlock priceTxt = new TextBlock();
                                        priceTxt.Text = table.Rows[0]["product_price"].ToString();
                                        priceTxt.FontSize = 13;
                                        productPriceArea.Children.Add(priceTxt);

                                        productPanel.Children.Add(productPriceArea);
                                        productBorder.Child = productPanel;
                                        productBorder.MouseDown += (s, e) =>
                                        {
                                            if (variantName.Text == "N/A") LoadCustomizeMenu(nameBlock.Text, false);
                                            else
                                            {
                                                ProductIcon.Source = productImg.Source;
                                                MainProductName.Text = pName;
                                                OrderingInterface.Visibility = Visibility.Hidden;
                                                LowerCartInterface.Visibility = Visibility.Hidden;
                                                ProductVariantsPanel.Visibility = Visibility.Visible;
                                                VariantMode = true;
                                                FillProducts(variantName.Text, true);
                                            }
                                        };

                                        if (!variantMenu) ProductsPanel.Children.Add(productBorder);
                                        else ProductVariants.Children.Add(productBorder);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // Removes jittering when resizing to add the scrollbar
            if (ProductsPanel.Children.Count <= 5) ProductsPanel.Width = 977;
            else ProductsPanel.Width = 960;
            if (ProductVariants.Children.Count <= 5) ProductVariants.Width = 977;
            else ProductVariants.Width = 960;
        }

        private void LoadCustomizeMenu(string productName, bool editMode)
        {
            ProductVariantsPanel.Visibility = Visibility.Hidden;
            ProductCustScroll.ScrollToVerticalOffset(0);
            BaseScroll.ScrollToHorizontalOffset(0);
            MilkScroll.ScrollToHorizontalOffset(0);
            SyrupScroll.ScrollToHorizontalOffset(0);
            SweetScroll.ScrollToHorizontalOffset(0);
            AddonScroll.ScrollToHorizontalOffset(0);
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT milk_id, addons_id, syrup_id, sweet_id, base_id, max_milk, milk_scale_value, max_addon, addon_scale_value, max_syrup, syrup_scale_value, max_sweetener, sweetener_scale_value, max_base, has_espresso_shot, can_serve_warm, temp_mode, size_mode, default_espresso_shot, max_espresso_shot, espresso_scale_value, milk_scale_value, addon_scale_value, syrup_scale_value, sweetener_scale_value, milk_required, addon_required, syrup_required, sweet_required, base_required, product_image, product_price, price_scale FROM products WHERE product_name = @prodName", connect))
                {
                    bool isFound = false;
                    command.Parameters.AddWithValue("@prodName", productName);
                    string milkPage = "", addonsPage = "", syrupPage = "", sweetPage = "", basePage = "";
                    bool hasEspresso = false, servedWarm = false;
                    int maxMilk = 0, milkScale = 0, maxAddon = 0, addonScale = 0, maxSyrup = 0, syrupScale = 0, maxSweet = 0, sweetScale = 0, maxBase = 0, ctempMode = 0, csizeMode = 0;
                    int defShot = 0, maxShot = 0, shotScale = 0, milkReq = 0, addonReq = 0, syrupReq = 0, sweetReq = 0, baseReq = 0, step = 0, priceScale = 0;
                    double productPrice = 0.0;
                    byte[] productImg = null;
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            milkPage = reader[0]?.ToString();
                            addonsPage = reader[1]?.ToString();
                            syrupPage = reader[2]?.ToString();
                            sweetPage = reader[3]?.ToString();
                            basePage = reader[4]?.ToString();
                            maxMilk = (int)reader[5];
                            milkScale = (int)reader[6];
                            maxAddon = (int)reader[7];
                            addonScale = (int)reader[8];
                            maxSyrup = (int)reader[9];
                            syrupScale = (int)reader[10];
                            maxSweet = (int)reader[11];
                            sweetScale = (int)reader[12];
                            maxBase = (int)reader[13];
                            hasEspresso = Convert.ToBoolean(reader[14]);                            
                            servedWarm = Convert.ToBoolean(reader[15]);
                            ctempMode = (int)reader[16];
                            csizeMode = (int)reader[17];
                            defShot = (int)reader[18];
                            maxShot = (int)reader[19];
                            shotScale = (int)reader[20];
                            milkScale = (int)reader[21];
                            addonScale = (int)reader[22];
                            syrupScale = (int)reader[23];
                            sweetScale = (int)reader[24];
                            milkReq = (int)reader[25];
                            addonReq = (int)reader[26];
                            syrupReq = (int)reader[27];
                            sweetReq = (int)reader[28];
                            baseReq = (int)reader[29];
                            if (reader[30] != DBNull.Value) productImg = (byte[])reader[30];
                            productPrice = (double)reader[31];
                            priceScale = (int)reader[32];
                            isFound = true;
                        }
                    }
                    if (isFound == false) return;

                    Product product = new Product("");
                    if (editMode) product = ProductCart[ProductCartLocation];

                    string priceStr = productPrice.ToString("F2");
                    ProductName.Text = productName;
                    SelectedProductPrice.Text = priceStr;
                    BasePrice.Text = priceStr;
                    PriceScale.Text = priceScale.ToString();
                    if (editMode)
                    {
                        double basePrice = product.BasePrice * product.ProductQuantity;
                        SelectedProductPrice.Text = basePrice.ToString("F2");
                        SelectedProductQuantity.Text = product.ProductQuantity.ToString();
                    }

                    command.CommandText = "SELECT price FROM espresso_shot LIMIT 1";
                    double shotPrice = 0.0;
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read()) shotPrice = Convert.ToDouble(reader[0]);
                        HiddenShotPrice.Text = shotPrice.ToString("F2");
                    }

                    if (productImg != null)
                    {
                        using (MemoryStream ms = new MemoryStream(productImg))
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
					else ProductImage.Source = productPlaceholder;
                    CurrentProductImage = productImg;

                    switch (csizeMode)
                    {
                        case 1: ProductSize.Visibility = Visibility.Collapsed; sizeMode = 0; break;
                        case 2: ProductSize.Visibility = Visibility.Collapsed; sizeMode = 1; break;
                        case 3:
                            sizeMode = 0;
                            ProductSize.Visibility = Visibility.Visible;
                            SmallBorder.Visibility = Visibility.Visible;
                            MediumBorder.Visibility = Visibility.Visible;
                            LargeBorder.Visibility = Visibility.Collapsed;
                            ++step;
                            break;
                        case 4:
                            sizeMode = 0;
                            ProductSize.Visibility = Visibility.Visible;
                            SmallBorder.Visibility = Visibility.Visible;
                            MediumBorder.Visibility = Visibility.Collapsed;
                            LargeBorder.Visibility = Visibility.Visible;
                            ++step;
                            break;
                        case 5: ProductSize.Visibility = Visibility.Collapsed; sizeMode = 2; break;
                        case 6:
                            sizeMode = 0;
                            ProductSize.Visibility = Visibility.Visible;
                            SmallBorder.Visibility = Visibility.Collapsed;
                            MediumBorder.Visibility = Visibility.Visible;
                            LargeBorder.Visibility = Visibility.Visible;
                            ++step;
                            break;
                        case 7: ProductSize.Visibility = Visibility.Collapsed; sizeMode = 3; break;
                        case 8:
                            sizeMode = 0;
                            ProductSize.Visibility = Visibility.Visible;
                            SmallBorder.Visibility = Visibility.Visible;
                            MediumBorder.Visibility = Visibility.Visible;
                            LargeBorder.Visibility = Visibility.Visible;
                            ++step;
                            break;
                    }
                    if (ProductSize.Visibility == Visibility.Visible) SizeStep.Text = step.ToString();
                    if (editMode)
                    {
                        switch (product.ProductSize)
                        {
                            case 1: SmallBorder.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue); sizeMode = 1; break;
                            case 2: MediumBorder.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue); sizeMode = 2; break;
                            case 3: LargeBorder.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue); sizeMode = 3; break;
                        }
                    }

                    iceMode = 0;
                    switch (ctempMode)
                    {
                        case 1: 
                            TempMode.Visibility = Visibility.Collapsed;
							IceLevel.Visibility = Visibility.Collapsed;
                            IceStep.Visibility = Visibility.Collapsed;
                            tempMode = 0;
                            break;
                        case 2: 
                            TempMode.Visibility = Visibility.Collapsed;
							IceLevel.Visibility = Visibility.Collapsed;
                            IceStep.Visibility = Visibility.Collapsed;
                            tempMode = 1;
                            break;
                        case 3: 
							TempMode.Visibility = Visibility.Collapsed;
                            IceLevel.Visibility = Visibility.Visible;
							++step;
                            IceStep.Text = step.ToString();
                            IceStep.Visibility = Visibility.Visible;
                            tempMode = 2; 
                            break;
                        case 4:                           
                            ++step;
                            TempStep.Text = step.ToString();
                            ++step;
                            IceStep.Text = step.ToString();
							IceStep.Visibility = Visibility.Visible;
                            TempMode.Visibility = Visibility.Visible;
                            IceLevel.Visibility = Visibility.Collapsed;
                            tempMode = 0;
                            break; 
                    }
                    if (editMode)
                    {
                        switch (product.ProductTemperature)
                        {
                            case 1: HotProduct.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue); tempMode = 1; break;
                            case 2: 
                                ColdProduct.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue);
                                IceLevel.Visibility = Visibility.Visible;
                                tempMode = 2; 
                                break;
                        }

                        switch (product.IceLevel)
                        {
                            case 1: ColdNoIce.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue); iceMode = 1; break;
                            case 2: LessIce.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue); iceMode = 2; break;
                            case 3: RegIce.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue); iceMode = 3; break;
                            case 4: MoreIce.BorderBrush = new SolidColorBrush (Colors.CornflowerBlue); iceMode = 4; break;
                            case 5: FullIce.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue); iceMode = 5; break;
                        }
                    }

                    if (hasEspresso == false) EspressoShot.Visibility = Visibility.Collapsed;
                    else
                    {
                        ++step;
						string strDefShot = defShot.ToString();
                        EspressoStep.Text = step.ToString();
                        EspressoShot.Visibility = Visibility.Visible;
                        EspressoQuantity.Text = strDefShot;
                        DefaultEspresso.Text = strDefShot;
                        ShotScale.Text = shotScale.ToString();
                        if (maxShot > 0)
                        {
                            string strMaxShot = maxShot.ToString();
							MaxShotQuantity.Text = strMaxShot;
							DefaultMaxShot.Text = strMaxShot;
                            ShotSlash.Visibility = Visibility.Visible;
                            MaxShotQuantity.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            ShotSlash.Visibility = Visibility.Collapsed;
                            MaxShotQuantity.Visibility = Visibility.Collapsed;
                        }
                    }
                    if (editMode)
                    {
                        string converted = product.EspressoShotPrice.ToString("F2");
                        ShotPrice.Text = converted;
                        EspressoQuantity.Text = product.EspressoShotAmount.ToString();
                    }

                    serveWarm = 0;
                    if (servedWarm == false) ServeWarm.Visibility = Visibility.Collapsed;
                    else
                    {
                        ServeWarm.Visibility = Visibility.Visible; 
                        ++step;
                        WarmStep.Text = step.ToString();
                    }
                    if (editMode)
                    {
                        switch (product.ReheatProduct)
                        {
                            case 1: AllowReheat.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue); serveWarm = 1; break;
                            case 2: NoReheat.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue); serveWarm = 2; break;
                        }
                    }
					string strMaxMilk = maxMilk.ToString();
                    MaxMilk.Text = strMaxMilk;
                    DefaultMaxMilk.Text = strMaxMilk;
					MilkScale.Text = milkScale.ToString();
					
					string strMaxAdd = maxAddon.ToString();
					MaxAddon.Text = strMaxAdd;
					DefaultMaxAddon.Text = strMaxAdd;
                    AddonScale.Text = addonScale.ToString();                    
					
					string strMaxSyrup = maxSyrup.ToString();
					MaxSyrup.Text = strMaxSyrup;
					DefaultMaxSyrup.Text = strMaxSyrup;
                    SyrupScale.Text = syrupScale.ToString();
                    					
					string strMaxSweet = maxSweet.ToString();
					MaxSweet.Text = strMaxSweet;
					DefaultMaxSweet.Text = strMaxSweet;
                    SweetScale.Text = sweetScale.ToString();
                    					
                    MaxBase.Text = maxBase.ToString();

                    if (maxMilk == 0)
                    {
                        MaxMilk.Visibility = Visibility.Collapsed;
                        MilkSlash.Visibility = Visibility.Collapsed;
                        MinMilk.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MaxMilk.Visibility = Visibility.Visible;
                        MilkSlash.Visibility = Visibility.Visible;
                        MinMilk.Visibility = Visibility.Visible;
                    }
                    if (maxAddon == 0)
                    {
                        MaxAddon.Visibility = Visibility.Collapsed;
                        AddonSlash.Visibility = Visibility.Collapsed;
                        MinAddon.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MaxAddon.Visibility = Visibility.Visible;
                        AddonSlash.Visibility = Visibility.Visible;
                        MinAddon.Visibility = Visibility.Visible;
                    }
                    if (maxSyrup == 0)
                    {
                        MaxSyrup.Visibility = Visibility.Collapsed;
                        SyrupSlash.Visibility = Visibility.Collapsed;
                        MinSyrup.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MaxSyrup.Visibility = Visibility.Visible;
                        SyrupSlash.Visibility = Visibility.Visible;
                        MinSyrup.Visibility = Visibility.Visible;
                    }
                    if (maxSweet == 0)
                    {
                        MaxSweet.Visibility = Visibility.Collapsed;
                        SweetSlash.Visibility = Visibility.Collapsed;
                        MinSweet.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MaxSweet.Visibility = Visibility.Visible;
                        SweetSlash.Visibility = Visibility.Visible;
                        MinSweet.Visibility = Visibility.Visible;
                    }
                    if (maxBase == 0)
                    {
                        MaxBase.Visibility = Visibility.Collapsed;
                        BaseSlash.Visibility = Visibility.Collapsed;
                        MinBase.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MaxBase.Visibility = Visibility.Visible;
                        BaseSlash.Visibility = Visibility.Visible;
                        MinBase.Visibility = Visibility.Visible;
                    }

                    switch (milkReq)
                    {
                        case 1:
                            MilkRequired.Text = "1";
                            MilkRequiredText.Text = "(Optional)";
                            break;
                        case 2:
                            MilkRequired.Text = "2";
                            MilkRequiredText.Text = "(Choose at least 1)";
                            break;
                        case 3:
                            MilkRequired.Text = "3";
                            MilkRequiredText.Text = "(Required)";
                            break;
                    }

                    switch (addonReq)
                    {
                        case 1:
                            AddonRequired.Text = "1";
                            AddonRequiredText.Text = "(Optional)";
                            break;
                        case 2:
                            AddonRequired.Text = "2";
                            AddonRequiredText.Text = "(Choose at least 1)";
                            break;
                        case 3:
                            AddonRequired.Text = "3";
                            AddonRequiredText.Text = "(Required)";
                            break;
                    }

                    switch (syrupReq)
                    {
                        case 1:
                            SyrupRequired.Text = "1";
                            SyrupRequiredText.Text = "(Optional)";
                            break;
                        case 2:
                            SyrupRequired.Text = "2";
                            SyrupRequiredText.Text = "(Choose at least 1)";
                            break;
                        case 3:
                            SyrupRequired.Text = "3";
                            SyrupRequiredText.Text = "(Required)";
                            break;
                    }

                    switch (sweetReq)
                    {
                        case 1:
                            SweetRequired.Text = "1";
                            SweetRequiredText.Text = "(Optional)";
                            break;
                        case 2:
                            SweetRequired.Text = "2";
                            SweetRequiredText.Text = "(Choose at least 1)";
                            break;
                        case 3:
                            SweetRequired.Text = "3";
                            SweetRequiredText.Text = "(Required)";
                            break;
                    }

                    switch (baseReq)
                    {
                        case 1:
                            BaseRequired.Text = "1";
                            BaseRequiredText.Text = "(Optional)";
                            break;
                        case 2:
                            BaseRequired.Text = "2";
                            BaseRequiredText.Text = "(Choose at least 1)";
                            break;
                        case 3:
                            BaseRequired.Text = "3";
                            BaseRequiredText.Text = "(Required)";
                            break;
                    }
					
					MilkOptions.Children.Clear();
                    BaseOptions.Children.Clear();
                    SyrupOptions.Children.Clear();
                    SweetOptions.Children.Clear();
                    AddonOptions.Children.Clear();

                    if (basePage != "N/A")
                    {
						LoadProductOptions(basePage, 5, editMode);
                        ++step;
                        BaseStep.Text = step.ToString();
                        ProductBase.Visibility = Visibility.Visible;
                    }
                    else ProductBase.Visibility = Visibility.Collapsed;
                    if (milkPage != "N/A")
                    {
						LoadProductOptions(milkPage, 1, editMode);
                        ++step;
                        MilkStep.Text = step.ToString();
                        ProductMilk.Visibility = Visibility.Visible;
                    }
                    else ProductMilk.Visibility = Visibility.Collapsed;
                    if (sweetPage != "N/A")
                    {
						LoadProductOptions(sweetPage, 4, editMode);
                        ++step;
                        SweetStep.Text = step.ToString();
                        ProductSweetener.Visibility = Visibility.Visible;
                    }
                    else ProductSweetener.Visibility = Visibility.Collapsed;
                    if (syrupPage != "N/A")
                    {
						LoadProductOptions(syrupPage, 3, editMode);
                        ++step;
                        SyrupStep.Text = step.ToString();
                        ProductSyrup.Visibility = Visibility.Visible;
                    }
                    else ProductSyrup.Visibility = Visibility.Collapsed;
                    if (addonsPage != "N/A")
                    {
						LoadProductOptions(addonsPage, 2, editMode);
                        ++step;
                        AddonStep.Text = step.ToString();
                        ProductAddon.Visibility = Visibility.Visible;
                    }
                    else ProductAddon.Visibility = Visibility.Collapsed;

                    OrderingInterface.Visibility = Visibility.Hidden;
                    LowerCartInterface.Visibility = Visibility.Hidden;
                    ProductCustInterface.Visibility = Visibility.Visible;
                }
            }
        }

        private void LoadProductOptions(string menuID, int target, bool editMode)
        {
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (var connect2 = new MySqlConnection(cs))
                {
                    connect2.Open();
                    using (MySqlCommand command = new MySqlCommand("SELECT product_price, product_image, product_quantity, has_quantity, order_mode FROM products WHERE product_name = @prodName", connect))
                    {
                        command.Parameters.AddWithValue("@prodName", "");
						command.Prepare();
                        using (MySqlCommand command2 = new MySqlCommand($"SELECT prod_name FROM {menuID}", connect2))
                        {
                            using (MySqlDataReader reader = command2.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string pName = reader[0]?.ToString();
                                    if (string.IsNullOrEmpty(pName)) continue;
                                    if (IsTargetVisible(pName, false) == false) continue;
                                    command.Parameters["@prodName"].Value = pName;

                                    double price = 0;
                                    int quantity = 0;
                                    int orderMode = 0;
                                    byte[] img = null;
                                    bool hasQuantity = false;
                                    using (MySqlDataReader reader1 = command.ExecuteReader())
                                    {
                                        while (reader1.Read())
                                        {
                                            price = (double)reader1[0];
                                            if (reader1[1] != DBNull.Value) img = (byte[])reader1[1];
                                            quantity = (int)reader1[2];
                                            hasQuantity = Convert.ToBoolean(reader1[3]);
                                            orderMode = (int)reader1[4];
                                        }
                                    }
                                    if (hasQuantity && quantity == 0) continue;
                                    if (orderMode != 3 && orderMode != CurrentOrderMode) continue;

                                    Milk cmilk = new Milk("");
                                    Addon caddon = new Addon("");
                                    Syrup csyrup = new Syrup("");
                                    Sweetener csweet = new Sweetener("");
                                    Base cbase = new Base("");
                                    bool success = false;
                                    if (editMode)
                                    {
                                        switch (target)
                                        {
                                            case 1: success = CurrentMilk.TryGetValue(new Milk(pName), out cmilk); break;
                                            case 2: success = CurrentAddon.TryGetValue(new Addon(pName), out caddon); break;
                                            case 3: success = CurrentSyrup.TryGetValue(new Syrup(pName), out csyrup); break;
                                            case 4: success = CurrentSweet.TryGetValue(new Sweetener(pName), out csweet); break;
                                            case 5: success = CurrentBase.TryGetValue(new Base(pName), out cbase); break;
                                        }
                                    }

                                    Border prodBorder = new Border();
                                    prodBorder.Width = 200;
                                    prodBorder.Height = 300;
                                    prodBorder.BorderBrush = new SolidColorBrush(Colors.Black);
                                    prodBorder.BorderThickness = new Thickness(1);
                                    prodBorder.Background = new SolidColorBrush(Colors.Transparent);
                                    prodBorder.Margin = new Thickness(0,0,20,0);
                                    prodBorder.CornerRadius = new CornerRadius(15);

                                    StackPanel prodPanel = new StackPanel();
                                    prodPanel.Background = new SolidColorBrush(Colors.Transparent);
                                    prodPanel.Orientation = Orientation.Vertical;

                                    Image prodImage = new Image();
                                    prodImage.Width = 122;
                                    prodImage.Height = 122;
                                    prodImage.Margin = new Thickness(0,7,0,7);
                                    if (img != null)
                                    {
                                        using (MemoryStream ms = new MemoryStream(img))
                                        {
                                            BitmapImage bitmapImage = new BitmapImage();
                                            bitmapImage.BeginInit();
                                            bitmapImage.StreamSource = ms;
                                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;  // Forces the loaded image to appear 
                                            bitmapImage.EndInit();
                                            bitmapImage.Freeze();
                                            prodImage.Source = bitmapImage;
                                        }
                                    }
                                    else prodImage.Source = productPlaceholder;
                                    prodPanel.Children.Add(prodImage);

                                    TextBlock nameBlock = new TextBlock();
                                    nameBlock.Text = pName;
                                    nameBlock.TextWrapping = TextWrapping.Wrap;
                                    nameBlock.Width = 133;
                                    nameBlock.Height = 70;
                                    if (nameBlock.Text.Length <= 16)
                                    {
                                        nameBlock.Height = 18;
                                        nameBlock.Margin = new Thickness(0, 49, 0, 5);
                                    }
                                    else if (nameBlock.Text.Length > 16 && nameBlock.Text.Length <= 32)
                                    {
                                        nameBlock.Height = 36;
                                        nameBlock.Margin = new Thickness(0, 32, 0, 5);
                                    }
                                    else if (nameBlock.Text.Length > 32 && nameBlock.Text.Length <= 48)
                                    {
                                        nameBlock.Height = 54;
                                        nameBlock.Margin = new Thickness(0, 16, 0, 5);
                                    }
                                    else if (nameBlock.Text.Length > 48 && nameBlock.Text.Length <= 64)
                                    {
                                        nameBlock.Height = 72;
                                        nameBlock.Margin = new Thickness(0, 0, 0, 5);
                                    }
                                    nameBlock.FontSize = 14;
                                    nameBlock.FontFamily = new FontFamily("Cascadia Mono");
                                    nameBlock.VerticalAlignment = VerticalAlignment.Center;
                                    nameBlock.HorizontalAlignment = HorizontalAlignment.Center;
                                    prodPanel.Children.Add(nameBlock);

                                    StackPanel pricePanel = new StackPanel();
                                    pricePanel.Width = 135;
                                    pricePanel.Background = new SolidColorBrush(Colors.Transparent);
                                    pricePanel.Orientation = Orientation.Horizontal;
                                    pricePanel.Margin = new Thickness(0, 0, 0, 15);

                                    TextBlock pesoSign = new TextBlock();
                                    pesoSign.Text = "+₱";
                                    pesoSign.FontSize = 16;
                                    pesoSign.Width = 20;

                                    TextBlock pesoValue = new TextBlock();
                                    pesoValue.Text = price.ToString("F2");
                                    pesoValue.Width = 150;
                                    pesoValue.TextAlignment = TextAlignment.Left;
                                    pesoValue.FontSize = 16;

                                    if (editMode && success)
                                    {
                                        switch (target)
                                        {
                                            case 1: pesoValue.Text = cmilk.MilkPrice.ToString("F2"); break;
                                            case 2: pesoValue.Text = caddon.AddonPrice.ToString("F2"); break;
                                            case 3: pesoValue.Text = csyrup.SyrupPrice.ToString("F2"); break;
                                            case 4: pesoValue.Text = csweet.SweetenerPrice.ToString("F2"); break;
                                            case 5: pesoValue.Text = cbase.BasePrice.ToString("F2"); break;
                                        }
                                    }

                                    pricePanel.Children.Add(pesoSign);
                                    pricePanel.Children.Add(pesoValue);
                                    prodPanel.Children.Add(pricePanel);

                                    StackPanel buttonPanel = new StackPanel();
                                    buttonPanel.Width = 87;
                                    buttonPanel.Height = 20;
                                    buttonPanel.Background = new SolidColorBrush(Colors.Transparent);
                                    buttonPanel.Orientation = Orientation.Horizontal;

                                    TextBlock productType = new TextBlock();
                                    productType.Text = "";
                                    productType.Width = 0;
                                    productType.Height = 0;
                                    switch(target)
                                    {
                                        case 1: productType.Text = "M"; break;
                                        case 2: productType.Text = "A"; break;
                                        case 3: productType.Text = "Y"; break;
                                        case 4: productType.Text = "W"; break;
                                        case 5: productType.Text = "B"; break;
                                    }
                                    prodPanel.Children.Add(productType);

                                    TextBlock productQuantity = new TextBlock();
                                    productQuantity.Text = quantity.ToString();
                                    productQuantity.Width = 0;
                                    productQuantity.Height = 0;
                                    prodPanel.Children.Add(productQuantity);

                                    TextBlock currentQuantity = new TextBlock();
                                    currentQuantity.Text = "0";
                                    currentQuantity.FontSize = 16;
                                    currentQuantity.Width = 20;
                                    currentQuantity.FontFamily = new FontFamily("Cascadia Mono");
                                    currentQuantity.TextAlignment = TextAlignment.Center;

                                    if (editMode && success)
                                    {
                                        switch (target)
                                        {
                                            case 1: 
                                                currentQuantity.Text = cmilk.MilkQuantity.ToString();
                                                int currentMinMilk = Convert.ToInt32(MinMilk.Text);
                                                MinMilk.Text = (currentMinMilk + cmilk.MilkQuantity).ToString();
                                                break;
                                            case 2: 
                                                currentQuantity.Text = caddon.AddonQuantity.ToString();
                                                int currentMinAddon = Convert.ToInt32(MinAddon.Text);
                                                MinAddon.Text = (currentMinAddon + caddon.AddonQuantity).ToString();
                                                break;
                                            case 3: 
                                                currentQuantity.Text = csyrup.SyrupQuantity.ToString();
                                                int currentMinSyrup = Convert.ToInt32(MinSyrup.Text);
                                                MinSyrup.Text = (currentMinSyrup + csyrup.SyrupQuantity).ToString();
                                                break;
                                            case 4: 
                                                currentQuantity.Text = csweet.SweetenerQuantity.ToString();
                                                int currentMinSweet = Convert.ToInt32(MinSweet.Text);
                                                MinSweet.Text = (currentMinSweet + csweet.SweetenerQuantity).ToString();
                                                break;
                                            case 5: 
                                                currentQuantity.Text = cbase.BaseQuantity.ToString();
                                                int currentMinBase = Convert.ToInt32(MinBase.Text);
                                                MinBase.Text = (currentMinBase + cbase.BaseQuantity).ToString();
                                                break;
                                        }
                                    }

                                    Style style = (Style)this.Resources["RoundButtonStyle"];
                                    Button btnPlus = new Button();
                                    btnPlus.Style = style;
                                    btnPlus.Background = new SolidColorBrush(Colors.White);
                                    btnPlus.Width = 20;
                                    btnPlus.Height = 20;
                                    btnPlus.Content = "+";
                                    btnPlus.FontFamily = new FontFamily("Cascadia Mono");
                                    btnPlus.FontSize = 15;
                                    btnPlus.Margin = new Thickness(13, 0, 0, 0);
                                    btnPlus.Click += (s, e) =>
                                    {
                                        int curQuantity = Convert.ToInt32(currentQuantity.Text);
                                        int baseQty = Convert.ToInt32(SelectedProductQuantity.Text);
                                        int totalQuantity = (curQuantity + 1) * baseQty;
                                        int prodQuantity = Convert.ToInt32(productQuantity.Text);
                                        if (hasQuantity && totalQuantity > prodQuantity) return;
                                        double curPrice = Convert.ToDouble(pesoValue.Text);
                                        double basePrice = curPrice, newPrice = curPrice;
                                        if (curQuantity >= 1)
                                        {
                                            basePrice = curPrice / curQuantity;
                                            newPrice = basePrice + curPrice;
                                        }
                                        switch (productType.Text)
                                        {
                                            case "M":
                                                int minMilk = Convert.ToInt32(MinMilk.Text);
                                                int maxMilk = Convert.ToInt32(MaxMilk.Text);
                                                if (maxMilk > 0 && minMilk >= maxMilk) return;
                                                MinMilk.Text = (minMilk + 1).ToString();
                                                Milk newMilk = new Milk(nameBlock.Text, curQuantity + 1, newPrice);
                                                CurrentMilk.Remove(newMilk);
                                                CurrentMilk.Add(newMilk);
                                                break;
                                            case "A":
                                                int minAdd = Convert.ToInt32(MinAddon.Text);
                                                int maxAdd = Convert.ToInt32(MaxAddon.Text);
                                                if (maxAdd > 0 && minAdd >= maxAdd) return;
                                                MinAddon.Text = (minAdd + 1).ToString();
                                                Addon newAddon = new Addon(nameBlock.Text, curQuantity + 1, newPrice);
                                                CurrentAddon.Remove(newAddon);
                                                CurrentAddon.Add(newAddon);
                                                break;
                                            case "Y":
                                                int minSyrup = Convert.ToInt32(MinSyrup.Text);
                                                int maxSyrup = Convert.ToInt32(MaxSyrup.Text);
                                                if (maxSyrup > 0 && minSyrup >= maxSyrup) return;
                                                MinSyrup.Text = (minSyrup + 1).ToString();
                                                Syrup newSyrup = new Syrup(nameBlock.Text, curQuantity + 1, newPrice);
                                                CurrentSyrup.Remove(newSyrup);
                                                CurrentSyrup.Add(newSyrup);
                                                break;
                                            case "W":
                                                int minSweet = Convert.ToInt32(MinSweet.Text);
                                                int maxSweet = Convert.ToInt32(MaxSweet.Text);
                                                if (maxSweet > 0 && minSweet >= maxSweet) return;
                                                MinSweet.Text = (minSweet + 1).ToString();
                                                Sweetener newSweet = new Sweetener(nameBlock.Text, curQuantity + 1, newPrice);
                                                CurrentSweet.Remove(newSweet);
                                                CurrentSweet.Add(newSweet);
                                                break;
                                            case "B": 
                                                int minBase = Convert.ToInt32(MinBase.Text);
                                                int maxBase = Convert.ToInt32(MaxBase.Text);
                                                if (maxBase > 0 && minBase >= maxBase) return;
                                                MinBase.Text = (minBase + 1).ToString();
                                                Base newBase = new Base(nameBlock.Text, curQuantity + 1, newPrice);
                                                CurrentBase.Remove(newBase);
                                                CurrentBase.Add(newBase);
                                                break;
                                        }
                                        pesoValue.Text = newPrice.ToString("F2");
                                        currentQuantity.Text = (curQuantity + 1).ToString();
                                    };

                                    Button btnMinus = new Button();
                                    btnMinus.Style = style;
                                    btnMinus.Background = new SolidColorBrush(Colors.White);
                                    btnMinus.Width = 20;
                                    btnMinus.Height = 20;
                                    btnMinus.Content = "-";
                                    btnMinus.FontFamily = new FontFamily("Cascadia Mono");
                                    btnMinus.FontSize = 15;
                                    btnMinus.Margin = new Thickness(0, 0, 13, 0);
                                    btnMinus.Click += (s, e) =>
                                    {
                                        int curQuantity = Convert.ToInt32(currentQuantity.Text);
                                        if (curQuantity == 0) return;

                                        double curPrice = Convert.ToDouble(pesoValue.Text);
                                        double basePrice = curPrice / curQuantity, newPrice = basePrice;
                                        if (curQuantity > 1) newPrice = curPrice - basePrice;
                                        switch (productType.Text)
                                        {
                                            case "M":
                                                int minMilk = Convert.ToInt32(MinMilk.Text);
                                                MinMilk.Text = (minMilk - 1).ToString();                                               
                                                Milk newMilk = new Milk(nameBlock.Text, curQuantity - 1, newPrice);
                                                CurrentMilk.Remove(newMilk);
                                                if (curQuantity - 1 >= 1) CurrentMilk.Add(newMilk);
                                                break;
                                            case "A":
                                                int minAddon = Convert.ToInt32(MinAddon.Text);
                                                MinAddon.Text = (minAddon - 1).ToString();
                                                Addon newAddon = new Addon(nameBlock.Text, curQuantity - 1, newPrice);
                                                CurrentAddon.Remove(newAddon);
                                                if (curQuantity - 1 >= 1) CurrentAddon.Add(newAddon);
                                                break;
                                            case "Y": 
                                                int minSyrup = Convert.ToInt32(MinSyrup.Text);
                                                MinSyrup.Text = (minSyrup - 1).ToString();
                                                Syrup newSyrup = new Syrup(nameBlock.Text, curQuantity - 1, newPrice);
                                                CurrentSyrup.Remove(newSyrup);
                                                if (curQuantity - 1 >= 1) CurrentSyrup.Add(newSyrup);
                                                break;
                                            case "W": 
                                                int minSweet = Convert.ToInt32(MinSweet.Text);
                                                MinSweet.Text = (minSweet - 1).ToString();
                                                Sweetener newSweet = new Sweetener(nameBlock.Text, curQuantity - 1, newPrice);
                                                CurrentSweet.Remove(newSweet);
                                                if (curQuantity - 1 >= 1) CurrentSweet.Add(newSweet);
                                                break;
                                            case "B": 
                                                int minBase = Convert.ToInt32(MinBase.Text);
                                                MinBase.Text = (minBase - 1).ToString();
                                                Base newBase = new Base(nameBlock.Text, curQuantity - 1, newPrice);
                                                CurrentBase.Remove(newBase);
                                                if (curQuantity - 1 >= 1) CurrentBase.Add(newBase);
                                                break;
                                        }
                                        pesoValue.Text = newPrice.ToString("F2");
                                        currentQuantity.Text = (curQuantity - 1).ToString();
                                    };

                                    buttonPanel.Children.Add(btnMinus);
                                    buttonPanel.Children.Add(currentQuantity);
                                    buttonPanel.Children.Add(btnPlus);

                                    // Border with only 1 side visible
                                    // Thickness(0,0,0,0) determines which side is visible
                                    Border buttonBorder = new Border();
                                    buttonBorder.Width = 200;
                                    buttonBorder.Height = 51;
                                    buttonBorder.BorderThickness = new Thickness(0,1,0,0);
                                    buttonBorder.Background = new SolidColorBrush(Colors.Transparent);
                                    buttonBorder.BorderBrush = new SolidColorBrush(Colors.Black);
                                    buttonBorder.Child = buttonPanel;

                                    prodPanel.Children.Add(buttonBorder);

                                    prodBorder.Child = prodPanel;
                                    switch (target)
                                    {
                                        case 1: MilkOptions.Children.Add(prodBorder); break;
                                        case 2: AddonOptions.Children.Add(prodBorder); break;
                                        case 3: SyrupOptions.Children.Add(prodBorder); break;
                                        case 4: SweetOptions.Children.Add(prodBorder); break;
                                        case 5: BaseOptions.Children.Add(prodBorder); break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Product_MouseEnter(object sender, RoutedEventArgs e)
        {
            Border border = sender as Border;
            if (border != null)
            {
                border.BorderBrush = Brushes.CornflowerBlue;
            }
        }

        private void Product_MouseLeave(object sender, RoutedEventArgs e)
        {
            Border border = sender as Border;
            if (border != null)
            {
                border.BorderBrush = Brushes.Black;
            }
        }

        private void DineInButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentOrderMode = 1;
			InitiateOrder();
        }

        private void TakeOutButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentOrderMode = 2;
			InitiateOrder();
        }
		
		private void InitiateOrder()
        {
            OrderingInterface.Visibility = Visibility.Visible;
            LowerCartInterface.Visibility = Visibility.Visible;
            OrderModePage.Visibility = Visibility.Hidden;
            FillMenus();
            FillProducts("menu_14185951", false);
        }

        private void StartOverButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult prompt = MessageBox.Show("Do you want to start over?", "Return to home page", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (prompt == MessageBoxResult.Yes)
            {
                ResetOrderSystemState();
                OrderingInterface.Visibility = Visibility.Hidden;
                CustomerModeLandingPage.Visibility = Visibility.Visible;               
                LowerCartInterface.Visibility = Visibility.Hidden;
                if (PromotionVideo.Source != null) PromotionVideo.Play();
            }
        }

        private void ResetOrderSystemState()
        {
            CartQuantity.Text = "0";
            CartPrice.Text = "0.0";
            SelectedProductQuantity.Text = "1";
            ShotPrice.Text = "0";
            MinMilk.Text = "0";
            MinBase.Text = "0";
            MinAddon.Text = "0";
            MinSweet.Text = "0";
            MinSyrup.Text = "0";
            ProductCart.Clear();
            FoodCartContent.Children.Clear();
            CurrentMilk.Clear();
            CurrentAddon.Clear();
            CurrentBase.Clear();
            CurrentSweet.Clear();
            CurrentSyrup.Clear();
            CustomerTableNumber.Text = "";
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            double price = Convert.ToDouble(SelectedProductPrice.Text);
            int quantity = Convert.ToInt32(SelectedProductQuantity.Text);
            double baseprice = price / quantity;
            price += baseprice;
            quantity += 1;
            SelectedProductPrice.Text = price.ToString("F2");
            SelectedProductQuantity.Text = quantity.ToString();
        }

        private void MinusButton_Click(object sender, RoutedEventArgs e)
        {
            int quantity = Convert.ToInt32(SelectedProductQuantity.Text);
            if (quantity == 1) return;
            double price = Convert.ToDouble(SelectedProductPrice.Text);
            double baseprice = price / quantity;
            price -= baseprice;
            quantity -= 1;
            SelectedProductPrice.Text = price.ToString("F2");
            SelectedProductQuantity.Text = quantity.ToString();
        }

        private void AddEspresso_Click(object sender, RoutedEventArgs e)
        {
            int quantity = Convert.ToInt32(EspressoQuantity.Text);
            int max = Convert.ToInt32(MaxShotQuantity.Text);
            if (quantity >= 99) return;
            else if (max > 0 && quantity >= max) return;
            int defQuantity = Convert.ToInt32(DefaultEspresso.Text);
            if (quantity >= defQuantity)
            {
                if (ShotPrice.Text == "0") ShotPrice.Text = HiddenShotPrice.Text;
                int downscale = quantity - defQuantity;
                if (downscale > 0)
                {
                    double price = Convert.ToDouble(ShotPrice.Text);
                    double basePrice = price / downscale;
                    ShotPrice.Text = (price + basePrice).ToString("F2");
                }
            }
            quantity += 1;
            EspressoQuantity.Text = quantity.ToString();
        }

        private void RemoveEspresso_Click(object sender, RoutedEventArgs e)
        {
            int quantity = Convert.ToInt32(EspressoQuantity.Text);
            int defQuantity = Convert.ToInt32(DefaultEspresso.Text);
            if (quantity == 0) return;
            if (quantity > defQuantity)
            {
                int downscale = quantity - defQuantity;
                double price = Convert.ToDouble(ShotPrice.Text);
                double basePrice = price / downscale;
                double newPrice = price - basePrice;
                if (newPrice > 0) ShotPrice.Text = newPrice.ToString("F2");
                else ShotPrice.Text = newPrice.ToString();
            }
            else ShotPrice.Text = "0";
            quantity -= 1;
            EspressoQuantity.Text = quantity.ToString();
        }

        private void ColdNoIce_Click(object sender, RoutedEventArgs e)
        {
            ColdNoIce.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue);
            LessIce.BorderBrush = new SolidColorBrush(Colors.Black);
            RegIce.BorderBrush = new SolidColorBrush(Colors.Black);
            MoreIce.BorderBrush = new SolidColorBrush(Colors.Black);
            FullIce.BorderBrush = new SolidColorBrush(Colors.Black);
            iceMode = 1;
        }

        private void LessIce_Click(object sender, RoutedEventArgs e)
        {
            ColdNoIce.BorderBrush = new SolidColorBrush(Colors.Black);
            LessIce.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue);
            RegIce.BorderBrush = new SolidColorBrush(Colors.Black);
            MoreIce.BorderBrush = new SolidColorBrush(Colors.Black);
            FullIce.BorderBrush = new SolidColorBrush(Colors.Black);
            iceMode = 2;
        }

        private void RegIce_Click(object sender, RoutedEventArgs e)
        {
            ColdNoIce.BorderBrush = new SolidColorBrush(Colors.Black);
            LessIce.BorderBrush = new SolidColorBrush(Colors.Black);
            RegIce.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue);
            MoreIce.BorderBrush = new SolidColorBrush(Colors.Black);
            FullIce.BorderBrush = new SolidColorBrush(Colors.Black);
            iceMode = 3;
        }

        private void MoreIce_Click(object sender, RoutedEventArgs e)
        {
            ColdNoIce.BorderBrush = new SolidColorBrush(Colors.Black);
            LessIce.BorderBrush = new SolidColorBrush(Colors.Black);
            RegIce.BorderBrush = new SolidColorBrush(Colors.Black);
            MoreIce.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue);
            FullIce.BorderBrush = new SolidColorBrush(Colors.Black);
            iceMode = 4;
        }

        private void FullIce_Click(object sender, RoutedEventArgs e)
        {
            ColdNoIce.BorderBrush = new SolidColorBrush(Colors.Black);
            LessIce.BorderBrush = new SolidColorBrush(Colors.Black);
            RegIce.BorderBrush = new SolidColorBrush(Colors.Black);
            MoreIce.BorderBrush = new SolidColorBrush(Colors.Black);
            FullIce.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue);
            iceMode = 5;
        }

        private void HotProduct_Click(object sender, RoutedEventArgs e)
        {
            HotProduct.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue);
            ColdProduct.BorderBrush= new SolidColorBrush(Colors.Black);
            IceLevel.Visibility = Visibility.Collapsed;
            tempMode = 1;
        }

        private void ColdProduct_Click(object sender, RoutedEventArgs e)
        {
            HotProduct.BorderBrush = new SolidColorBrush(Colors.Black);
            ColdProduct.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue);
            IceLevel.Visibility = Visibility.Visible;
            tempMode = 2;
        }

        private void SmallProduct_Click(object sender, RoutedEventArgs e)
        {
            SmallBorder.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue);
            MediumBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            LargeBorder.BorderBrush= new SolidColorBrush(Colors.Black);
            sizeMode = 1;

            MaxMilk.Text = DefaultMaxMilk.Text;
            MaxAddon.Text = DefaultMaxAddon.Text;
            MaxSyrup.Text = DefaultMaxSyrup.Text;
            MaxSweet.Text = DefaultMaxSweet.Text;
			MaxShotQuantity.Text = DefaultMaxShot.Text;
            double basePrice = Convert.ToDouble(BasePrice.Text);
            int quantity = Convert.ToInt32(SelectedProductQuantity.Text);
            double newPrice = basePrice * quantity;
            SelectedProductPrice.Text = newPrice.ToString("F2");
        }

        private void MediumProduct_Click(object sender, RoutedEventArgs e)
        {
            SmallBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            MediumBorder.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue);
            LargeBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            sizeMode = 2;

            if (SmallBorder.Visibility == Visibility.Collapsed)
            {
                MaxMilk.Text = DefaultMaxMilk.Text;
                MaxAddon.Text = DefaultMaxAddon.Text;
                MaxSyrup.Text = DefaultMaxSyrup.Text;
                MaxSweet.Text = DefaultMaxSweet.Text;
				MaxShotQuantity.Text = DefaultMaxShot.Text;
                return;
            }

            int newMilkMax = Convert.ToInt32(DefaultMaxMilk.Text);
            if (newMilkMax > 0)
            {
                int milkScale = Convert.ToInt32(MilkScale.Text);
                MaxMilk.Text = (newMilkMax + milkScale).ToString();
            }

            int newSyrupMax = Convert.ToInt32(DefaultMaxSyrup.Text);
            if (newSyrupMax > 0)
            {
                int syrupScale = Convert.ToInt32(SyrupScale.Text);
                MaxSyrup.Text = (newSyrupMax + syrupScale).ToString();
            }

            int newSweetMax = Convert.ToInt32(DefaultMaxSweet.Text);
            if (newSweetMax > 0)
            {
                int sweetScale = Convert.ToInt32(SweetScale.Text);
                MaxSweet.Text = (newSweetMax + sweetScale).ToString();
            }

            int newAddonMax = Convert.ToInt32(DefaultMaxAddon.Text);
            if (newAddonMax > 0)
            {
                int addonScale = Convert.ToInt32(AddonScale.Text);
                MaxAddon.Text = (newAddonMax + addonScale).ToString();
            }
			
			int newShotMax = Convert.ToInt32(DefaultMaxShot.Text);
            if (newShotMax > 0)
            {
                int shotScale = Convert.ToInt32(ShotScale.Text);
                MaxShotQuantity.Text = (newShotMax + shotScale).ToString();
            }

            double basePrice = Convert.ToDouble(BasePrice.Text);
            int priceScale = Convert.ToInt32(PriceScale.Text);
            basePrice += priceScale;
            int quantity = Convert.ToInt32(SelectedProductQuantity.Text);
            double newPrice = basePrice * quantity;
            SelectedProductPrice.Text = newPrice.ToString("F2");
        }

        private void LargeProduct_Click(object sender, RoutedEventArgs e)
        {
            SmallBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            MediumBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            LargeBorder.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue);
            sizeMode = 3;

            int newMilkMax = Convert.ToInt32(DefaultMaxMilk.Text);
            int milkScale = Convert.ToInt32(MilkScale.Text);          
            int newSyrupMax = Convert.ToInt32(DefaultMaxSyrup.Text);
            int syrupScale = Convert.ToInt32(SyrupScale.Text);
            int newSweetMax = Convert.ToInt32(DefaultMaxSweet.Text);
            int sweetScale = Convert.ToInt32(SweetScale.Text);
            int newAddonMax = Convert.ToInt32(DefaultMaxAddon.Text);
            int addonScale = Convert.ToInt32(AddonScale.Text);
			int newShotMax = Convert.ToInt32(DefaultMaxShot.Text);
            int shotScale = Convert.ToInt32(ShotScale.Text);

            double basePrice = Convert.ToDouble(BasePrice.Text);
            int priceScale = Convert.ToInt32(PriceScale.Text);
           
            if (SmallBorder.Visibility == Visibility.Visible)
            {
                newMilkMax = newMilkMax > 0 ? newMilkMax + milkScale : 0;
                newSweetMax = newSweetMax > 0 ? newSweetMax + sweetScale : 0;
                newSyrupMax = newSyrupMax > 0 ? newSyrupMax + syrupScale : 0;
                newAddonMax = newAddonMax > 0 ? newAddonMax + addonScale : 0;
				newShotMax = newShotMax > 0 ? newShotMax + shotScale : 0;
                basePrice += priceScale;
            }
            if (MediumBorder.Visibility == Visibility.Visible)
            {
                newMilkMax = newMilkMax > 0 ? newMilkMax + milkScale : 0;
                newSweetMax = newSweetMax > 0 ? newSweetMax + sweetScale : 0;
                newSyrupMax = newSyrupMax > 0 ? newSyrupMax + syrupScale : 0;
                newAddonMax = newAddonMax > 0 ? newAddonMax + addonScale : 0;
				newShotMax = newShotMax > 0 ? newShotMax + shotScale : 0;
                basePrice += priceScale;
            }

            MaxSweet.Text = newSweetMax.ToString();
            MaxAddon.Text = newAddonMax.ToString();
            MaxSyrup.Text = newSyrupMax.ToString();
            MaxMilk.Text = newMilkMax.ToString();
			MaxShotQuantity.Text = newShotMax.ToString();

            int quantity = Convert.ToInt32(SelectedProductQuantity.Text);
            double newPrice = basePrice * quantity;
            SelectedProductPrice.Text = newPrice.ToString("F2");
        }

        private void AllowReheat_Click(object sender, RoutedEventArgs e)
        {
            AllowReheat.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue);
            NoReheat.BorderBrush = new SolidColorBrush (Colors.Black);
            serveWarm = 1;
        }

        private void NoReheat_Click(object sender, RoutedEventArgs e)
        {
            AllowReheat.BorderBrush = new SolidColorBrush(Colors.Black);
            NoReheat.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue);
            serveWarm = 2;
        }

        private bool IsTargetVisible(string targetName, bool isMenu)
        {
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("", connect))
                {
                    if (isMenu)
                    {
                        command.CommandText = "SELECT start_time, end_time, start_date, end_date FROM menus WHERE menu_name = @m_name";
                        command.Parameters.AddWithValue("@m_name", targetName);
                    }
                    else
                    {
                        command.CommandText = "SELECT start_time, end_time, start_date, end_date FROM products WHERE product_name = @p_name";
                        command.Parameters.AddWithValue("@p_name", targetName);
                    }
                    DataTable table = new DataTable();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command)) adapter.Fill(table);
                    DateTime today = DateTime.Now;
                    // TimeSpan? does not know how to handle DBNull, resulting in a crash
                    if (table.Rows[0]["start_time"] != DBNull.Value)
                    {
                        TimeSpan startSpan = (TimeSpan)table.Rows[0]["start_time"];
                        DateTime startTime = DateTime.Today.Add(startSpan);
                        if (today.TimeOfDay < startTime.TimeOfDay) return false;
                    }
                    if (table.Rows[0]["end_time"] != DBNull.Value)
                    {
                        TimeSpan endSpan = (TimeSpan)table.Rows[0]["end_time"];
                        DateTime endTime = DateTime.Today.Add(endSpan);
                        if (today.TimeOfDay > endTime.TimeOfDay) return false;
                    }
                    if (table.Rows[0]["start_date"] != DBNull.Value)
                    {
                        DateTime startDate = (DateTime)table.Rows[0]["start_date"];
                        if (today.Date < startDate.Date) return false;
                    }
                    if (table.Rows[0]["end_date"] != DBNull.Value)
                    {
                        DateTime endDate = (DateTime)table.Rows[0]["end_date"];
                        if (today.Date > endDate.Date) return false;
                    }
                }
            }
            return true;
        }

        private void HomeMenu_Click(object sender, RoutedEventArgs e)
        {
            DisplayMenuName.Text = "Home";
            FillProducts("menu_14185951", false);
        }

        private void AddProductToCart_Click(object sender, RoutedEventArgs e)
        {
            if (EditMode == true) CreateProduct(true);
            else CreateProduct(false);
        }

        private void CloseCustButton_Click(object sender, RoutedEventArgs e)
        {
            ProductCustInterface.Visibility = Visibility.Hidden;
            LowerCartInterface.Visibility = Visibility.Visible;
            if (EditMode)
            {
                ProductCartPanel.Visibility = Visibility.Visible;
                EditMode = false;
                AddProductToCart.Content = "Add Product to Cart";
                CloseCustButton.Content = "Return to Menu";
            }
            else if (VariantMode)
            {
                ProductVariantsPanel.Visibility = Visibility.Visible;
                LowerCartInterface.Visibility = Visibility.Hidden;
            }
            else OrderingInterface.Visibility = Visibility.Visible;
            SelectedProductQuantity.Text = "1";
            CurrentMilk.Clear();
            CurrentBase.Clear();
            CurrentAddon.Clear();
            CurrentSweet.Clear();
            CurrentSyrup.Clear();
            BaseOptions.Children.Clear();
            MilkOptions.Children.Clear();
            SyrupOptions.Children.Clear();
            SweetOptions.Children.Clear();
            AddonOptions.Children.Clear();
            serveWarm = 0;
            iceMode = 0;
            tempMode = 0;
            sizeMode = 0;
            ShotPrice.Text = "0";
            MinMilk.Text = "0";
            MinBase.Text = "0";
            MinAddon.Text = "0";
            MinSweet.Text = "0";
            MinSyrup.Text = "0";
            SmallBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            MediumBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            LargeBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            HotProduct.BorderBrush = new SolidColorBrush(Colors.Black);
            ColdProduct.BorderBrush = new SolidColorBrush(Colors.Black);
            ColdNoIce.BorderBrush = new SolidColorBrush(Colors.Black);
            LessIce.BorderBrush = new SolidColorBrush(Colors.Black);
            RegIce.BorderBrush = new SolidColorBrush(Colors.Black);
            MoreIce.BorderBrush = new SolidColorBrush(Colors.Black);
            FullIce.BorderBrush = new SolidColorBrush(Colors.Black);
            AllowReheat.BorderBrush = new SolidColorBrush(Colors.Black);
            NoReheat.BorderBrush = new SolidColorBrush(Colors.Black);
        }

		private (int, double) RewardEquivalent(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName)) return (0, 0);
            int stamps = 0;
            double points = 0;
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT stamps_value, points_value FROM products WHERE product_name = @pName", connect))
                {
                    command.Parameters.AddWithValue("@pName", productName);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stamps = Convert.ToInt32(reader[0]);
                            points = Convert.ToDouble(reader[1]);
                        }
                    }
                }
            }
            return (stamps, points);
        }
		
        private void CreateProduct(bool editMode)
        {
            if (ProductSize.Visibility == Visibility.Visible && sizeMode == 0)
            {
                MessageBox.Show("Please choose the size of the product", "No given product size", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (tempMode == 0 && TempMode.Visibility == Visibility.Visible)
            {
                MessageBox.Show("Please choose the temperature of the product", "No given ice level", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (tempMode == 2 && iceMode == 0)
            {
                MessageBox.Show("Please choose the ice level of the product", "No given ice level", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            int milkRequired = Convert.ToInt32(MilkRequired.Text);
            int minMilk = Convert.ToInt32(MinMilk.Text);
            int maxMilk = Convert.ToInt32(MaxMilk.Text);
            switch (milkRequired)
            {
                case 2:
                    if (minMilk == 0)
                    {
                        MessageBox.Show("Please choose at least 1 milk option", "Pick at least 1 milk", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    break;
                case 3:
                    if (minMilk < maxMilk || minMilk == 0)
                    {
                        MessageBox.Show("Please choose the correct amount of milk options.", "Insufficient milk amount", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    break;
            }

            int addonRequired = Convert.ToInt32(AddonRequired.Text);
            int minAddon = Convert.ToInt32(MinAddon.Text);
            int maxAddon = Convert.ToInt32(MaxAddon.Text);
            switch (addonRequired)
            {
                case 2:
                    if (minAddon == 0)
                    {
                        MessageBox.Show("Please choose at least 1 add-on option", "Pick at least 1 add-on", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    break;
                case 3:
                    if (minAddon < maxAddon || minAddon == 0)
                    {
                        MessageBox.Show("Please choose the correct amount of add-on options.", "Insufficient add-on amount", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    break;
            }

            int syrupRequired = Convert.ToInt32(SyrupRequired.Text);
            int minSyrup = Convert.ToInt32(MinSyrup.Text);
            int maxSyrup = Convert.ToInt32(MaxSyrup.Text);
            switch (syrupRequired)
            {
                case 2:
                    if (minSyrup == 0)
                    {
                        MessageBox.Show("Please choose at least 1 syrup option", "Pick at least 1 syrup", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    break;
                case 3:
                    if (minSyrup < maxSyrup || minSyrup == 0)
                    {
                        MessageBox.Show("Please choose the correct amount of syrup options.", "Insufficient syrup amount", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    break;
            }

            int sweetRequired = Convert.ToInt32(SweetRequired.Text);
            int minSweet = Convert.ToInt32(MinSweet.Text);
            int maxSweet = Convert.ToInt32(MaxSweet.Text);
            switch (sweetRequired)
            {
                case 2:
                    if (minSweet == 0)
                    {
                        MessageBox.Show("Please choose at least 1 sweetener option", "Pick at least 1 sweetener", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    break;
                case 3:
                    if (minSweet < maxSweet || minSweet == 0)
                    {
                        MessageBox.Show("Please choose the correct amount of sweetener options.", "Insufficient sweetener amount", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    break;
            }

            int baseRequired = Convert.ToInt32(BaseRequired.Text);
            int minBase = Convert.ToInt32(MinBase.Text);
            int maxBase = Convert.ToInt32(MaxBase.Text);
            switch (baseRequired)
            {
                case 2:
                    if (minBase == 0)
                    {
                        MessageBox.Show("Please choose at least 1 base option", "Pick at least 1 base", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    break;
                case 3:
                    if (minBase < maxBase || minBase == 0)
                    {
                        MessageBox.Show("Please choose the correct amount of base options.", "Insufficient base amount", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    break;
            }

            double totalPrice = 0;
            double totalOptionsPrice = 0;
            HashSet<Milk> newMilk = new HashSet<Milk>();
            HashSet<Addon> newAddon = new HashSet<Addon>();
            HashSet<Syrup> newSyrup = new HashSet<Syrup>();
            HashSet<Sweetener> newSweet = new HashSet<Sweetener>();
            HashSet<Base> newBase = new HashSet<Base>();
            foreach (Milk milk in CurrentMilk)
            {
                newMilk.Add(milk);
                totalOptionsPrice += milk.MilkPrice;
            }
            foreach (Addon addon in CurrentAddon)
            {
                newAddon.Add(addon);
                totalOptionsPrice += addon.AddonPrice;
            }
            foreach (Syrup syrup in CurrentSyrup)
            {
                newSyrup.Add(syrup);
                totalOptionsPrice += syrup.SyrupPrice;
            }
            foreach (Sweetener sweet in CurrentSweet)
            {
                newSweet.Add(sweet);
                totalOptionsPrice += sweet.SweetenerPrice;
            }
            foreach (Base cBase in CurrentBase)
            {
                newBase.Add(cBase);
                totalOptionsPrice += cBase.BasePrice;
            }
            double productPrice = Convert.ToDouble(SelectedProductPrice.Text);
            double espressoShotPrice = Convert.ToDouble(ShotPrice.Text);
            int espressoShotQuantity = Convert.ToInt32(EspressoQuantity.Text);
            int quantity = Convert.ToInt32(SelectedProductQuantity.Text);
            double basePrice = productPrice / quantity;
            totalPrice += productPrice;
            totalPrice += (totalOptionsPrice * quantity);
            totalPrice += (espressoShotPrice * quantity);

            var rewardValue = RewardEquivalent(ProductName.Text);
            Product newProduct = new Product(ProductName.Text, basePrice, quantity, totalPrice, sizeMode, tempMode, espressoShotQuantity, espressoShotPrice, serveWarm, iceMode, rewardValue.Item1, rewardValue.Item2, newMilk, newAddon, newSyrup, newSweet, newBase, CurrentProductImage);
            ProductCustInterface.Visibility = Visibility.Hidden;
            LowerCartInterface.Visibility = Visibility.Visible;
            if (!editMode)
            {
                ProductCart.Add(newProduct);
                LoadProductCart(ProductCart.Count - 1);
                OrderingInterface.Visibility = Visibility.Visible;
            }
            else
            {
                Product targetProduct = ProductCart[ProductCartLocation];
                double curCartPrice = Convert.ToDouble(CartPrice.Text);
                int curCartQuantity = Convert.ToInt32(CartQuantity.Text);
                curCartPrice -= targetProduct.ProductPrice;
                curCartQuantity -= targetProduct.ProductQuantity;
                CartPrice.Text = curCartPrice.ToString();
                CartQuantity.Text = curCartQuantity.ToString();

                ProductCart[ProductCartLocation] = newProduct;
                ModifyQuantityInCart(ProductCartLocation, quantity, totalPrice);
                ProductCartPanel.Visibility = Visibility.Visible;
                EditMode = false;
                AddProductToCart.Content = "Add Product to Cart";
                CloseCustButton.Content = "Return to Menu";
            }

            int currentCartQuantity = Convert.ToInt32(CartQuantity.Text);
            double currentCartTotal = Convert.ToDouble(CartPrice.Text);
            CartQuantity.Text = (quantity + currentCartQuantity).ToString();
            CartPrice.Text = (currentCartTotal + totalPrice).ToString("F2");
            SelectedProductQuantity.Text = "1";
            ShotPrice.Text = "0";
            MinMilk.Text = "0";
            MinBase.Text = "0";
            MinAddon.Text = "0";
            MinSweet.Text = "0";
            MinSyrup.Text = "0";
            CurrentMilk.Clear();
            CurrentAddon.Clear();
            CurrentSyrup.Clear();
            CurrentSweet.Clear();
            CurrentBase.Clear();
            CurrentProductImage = null;
            serveWarm = 0;
            iceMode = 0;
            tempMode = 0;
            sizeMode = 0;
            SmallBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            MediumBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            LargeBorder.BorderBrush = new SolidColorBrush(Colors.Black);
            HotProduct.BorderBrush = new SolidColorBrush(Colors.Black);
            ColdProduct.BorderBrush = new SolidColorBrush(Colors.Black);
            ColdNoIce.BorderBrush = new SolidColorBrush(Colors.Black);
            LessIce.BorderBrush = new SolidColorBrush(Colors.Black);
            RegIce.BorderBrush = new SolidColorBrush(Colors.Black);
            MoreIce.BorderBrush = new SolidColorBrush(Colors.Black);
            FullIce.BorderBrush = new SolidColorBrush(Colors.Black);
            AllowReheat.BorderBrush = new SolidColorBrush(Colors.Black);
            NoReheat.BorderBrush = new SolidColorBrush(Colors.Black);
        }

        private int QuantityInvalid(string prodName, int quantity)
        {
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlCommand command = new MySqlCommand("SELECT product_quantity, has_quantity FROM products WHERE product_name = @prodName", connect))
                {
                    command.Parameters.AddWithValue("@prodName", prodName);
                    DataTable table = new DataTable();
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(command)) adapter.Fill(table);
                    if (table.Rows.Count == 0) return 2; // Product is invalid

                    int hasQuantity = Convert.ToInt32(table.Rows[0]["has_quantity"]);
                    if (hasQuantity == 1)
                    {
                        int curQuantity = Convert.ToInt32(table.Rows[0]["product_quantity"]);
                        if (quantity > curQuantity) return 1; // Current quantity is bad
                    }
                }
            }
            return 0; // Product is okay
        }

        private void ModifyQuantityInCart(int location, int newQuantity, double newPrice)
        {
            Border target = (Border)FoodCartContent.Children[location];
            StackPanel panel = (StackPanel)target.Child;
            // Price
            StackPanel firstPanel = (StackPanel)panel.Children[2];
            StackPanel innerFirst = (StackPanel)firstPanel.Children[2];
            TextBlock targetPrice = (TextBlock)innerFirst.Children[1];
            targetPrice.Text = newPrice.ToString("F2");

            // Quantity
            StackPanel thirdPanel = (StackPanel)panel.Children[4];
            StackPanel innerThird = (StackPanel)thirdPanel.Children[3];
            TextBlock targetQty = (TextBlock)innerThird.Children[1];
            targetQty.Text = newQuantity.ToString();
        }

        private void ShiftCartIndex(int start)
        {
            for (int i = start; i < ProductCart.Count; i++)
            {
                Border border = (Border)FoodCartContent.Children[i];
                StackPanel panel = (StackPanel)border.Child;
                TextBlock index = (TextBlock)panel.Children[1];
                index.Text = i.ToString();
            }
        }

        private void LoadProductCart(int startingPoint)
        {
            int index = 0;
            if (ProductCart.Count == 0) return;
            for (int i = startingPoint; i < ProductCart.Count; ++i)
            {
                Product product = ProductCart[i];
                string name = product.ProductName;

                // 3 children
                StackPanel productPanel = new StackPanel();
                productPanel.Orientation = Orientation.Vertical;

                // Max 45 characters per line
                TextBlock nameBlock = new TextBlock();
                nameBlock.Text = name;
                nameBlock.FontSize = 15;
                nameBlock.Width = 450;
                nameBlock.Height = 20;
                nameBlock.HorizontalAlignment = HorizontalAlignment.Center;
                nameBlock.VerticalAlignment = VerticalAlignment.Center;
                nameBlock.Margin = new Thickness(0, 0, 30, 0);
                if (name.Length <= 45) nameBlock.Height = 20;
                else if (name.Length > 45 && name.Length <= 90) nameBlock.Height = 40;
                else if (name.Length > 90 && name.Length <= 135) nameBlock.Height = 60;

                TextBlock extraPanelHeight = new TextBlock();
                extraPanelHeight.Text = "0";
                extraPanelHeight.Width = 0;
                extraPanelHeight.Height = 0;
                productPanel.Children.Add(extraPanelHeight);

                TextBlock CurrentProductIndex = new TextBlock();
                CurrentProductIndex.Text = i.ToString();
                CurrentProductIndex.Width = 0;
                CurrentProductIndex.Height = 0;
                productPanel.Children.Add(CurrentProductIndex);

                Border prodBorder = new Border();
                prodBorder.Child = productPanel;
                prodBorder.Width = 720;
                prodBorder.Height = 170;
                prodBorder.BorderBrush = new SolidColorBrush(Colors.Black);
                prodBorder.BorderThickness = new Thickness(1);
                prodBorder.Margin = new Thickness(10, 0, 0, 5);
                prodBorder.Background = new SolidColorBrush(Colors.White);  // Fixes MouseDown responsiveness
                prodBorder.CornerRadius = new CornerRadius(10);

                // 1st child
                StackPanel productDetail = new StackPanel();
                productDetail.Orientation = Orientation.Horizontal;
                productDetail.Height = 100;

                Image prodImage = new Image();
                prodImage.Margin = new Thickness(20, 0, 15, 0);
                // prodImage.VerticalAlignment = VerticalAlignment.Top;
                BitmapImage placeholder = new BitmapImage(new Uri("pack://application:,,,/CoffeeCup.png"));
                byte[] imageBytes = product.ProductImage;
                if (imageBytes != null)
                {
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = ms;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // Force image to load
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();

                        prodImage.Source = bitmapImage;
                        prodImage.Width = 90;
                        prodImage.Height = 90;
                        prodImage.VerticalAlignment = VerticalAlignment.Center;
                        // prodImage.Margin = new Thickness(5, 4, 0, 10);
                    }
                }
                else prodImage.Source = placeholder;
                productDetail.Children.Add(prodImage);
                productDetail.Children.Add(nameBlock);

                Image arrowImg = new Image();
                arrowImg.Height = 40;
                arrowImg.Width = 40;
                BitmapImage arrowDown = new BitmapImage(new Uri("pack://application:,,,/down.png"));
                arrowDown.CacheOption = BitmapCacheOption.OnLoad; // Force image to load
                BitmapImage arrowUp = new BitmapImage(new Uri("pack://application:,,,/up.png"));
                arrowDown.CacheOption = BitmapCacheOption.OnLoad; // Force image to load
                arrowImg.Margin = new Thickness(150, 0, 130, 0);
                arrowImg.Source = arrowDown;

                StackPanel productPricePanel = new StackPanel();
                productPricePanel.Width = 120;
                productPricePanel.Height = 20;
                productPricePanel.Orientation = Orientation.Horizontal;

                TextBlock pesosSign = new TextBlock();
                pesosSign.Text = "₱";
                pesosSign.FontSize = 15;
                pesosSign.Width = 10;
                pesosSign.Height = 20;

                TextBlock productPrice = new TextBlock();
                productPrice.Text = product.ProductPrice.ToString("F2");
                productPrice.FontSize = 15;
                productPrice.Width = 100;
                productPrice.Height = 20;

                productPricePanel.Children.Add(pesosSign);
                productPricePanel.Children.Add(productPrice);
                productDetail.Children.Add(productPricePanel);
                productPanel.Children.Add(productDetail);

                // 2nd child
                StackPanel productSummaryPanel = new StackPanel();
                productSummaryPanel.Orientation = Orientation.Horizontal;
                productSummaryPanel.HorizontalAlignment = HorizontalAlignment.Left;
                productSummaryPanel.Margin = new Thickness(25, 0, 0, 0);

                StackPanel productNames = new StackPanel();
                productNames.Orientation = Orientation.Vertical;
                productNames.HorizontalAlignment = HorizontalAlignment.Left;

                StackPanel productPrices = new StackPanel();
                productPrices.Orientation = Orientation.Vertical;
                productPrices.Width = 72;

                productSummaryPanel.Visibility = Visibility.Collapsed;
                productSummaryPanel.Children.Add(productNames);
                productSummaryPanel.Children.Add(productPrices);
                productPanel.Children.Add(productSummaryPanel); 

                // Collapsed is needed here because the StackPanel for product viewing must not taking any additional space when hidden
                // Hidden only hides the element, but the space it takes is still there
                prodBorder.MouseDown += (s, e) =>
                {
                    int extraHeight = 0;
                    if (productSummaryPanel.Visibility == Visibility.Collapsed)
                    {
                        productSummaryPanel.Visibility = Visibility.Visible;
                        arrowImg.Source = arrowUp;
                        prodBorder.Height += Convert.ToInt32(extraPanelHeight.Text);
                        double total = 0;
                        if (productNames.Children.Count == 0 || productPrices.Children.Count == 0)
                        {
                            prodBorder.Height = 170;
                            int target = Convert.ToInt32(CurrentProductIndex.Text);
                            Product prod = ProductCart[target];

                            TextBlock text = new TextBlock();
                            text.Width = 590;
                            text.Height = 22;
                            text.FontSize = 14;
                            text.TextWrapping = TextWrapping.Wrap;
                            text.Text = prod.ProductName + " (Base)";
                            text.HorizontalAlignment = HorizontalAlignment.Left;
                            text.TextTrimming = TextTrimming.CharacterEllipsis;
                            text.Margin = new Thickness(0, 0, 12, 0);

                            productNames.Children.Add(text);

                            TextBlock price = new TextBlock();
                            price.Text = prod.BasePrice.ToString();
                            total += prod.BasePrice;
                            price.Width = 72;
                            price.FontSize = 14;
                            price.Height = 22;
                            price.HorizontalAlignment = HorizontalAlignment.Left;

                            productPrices.Children.Add(price);
                            prodBorder.Height += 22;
                            extraHeight += 22;

                            if (prod.ProductSize > 0)
                            {
                                TextBlock pSize = new TextBlock();
                                pSize.HorizontalAlignment = HorizontalAlignment.Left;
                                pSize.Width = 580;
                                pSize.Height = 22;
                                pSize.TextWrapping = TextWrapping.Wrap;
                                pSize.Text = "Product Size:";
                                pSize.FontSize = 14;
                                pSize.FontWeight = FontWeights.Bold;
                                pSize.TextTrimming = TextTrimming.CharacterEllipsis;
                                productNames.Children.Add(pSize);

                                string tempSize = "";
                                switch (prod.ProductSize)
                                {
                                    case 1: tempSize = "Small"; break;
                                    case 2: tempSize = "Medium"; break;
                                    case 3: tempSize = "Large"; break;
                                }
                                TextBlock prodSize = new TextBlock();
                                prodSize.Text = tempSize;
                                prodSize.Width = 72;
                                prodSize.Height = 22;
                                prodSize.FontSize = 14;
                                productPrices.Children.Add(prodSize);

                                prodBorder.Height += 22;
                                extraHeight += 22;
                            }

                            if (prod.ProductTemperature > 0)
                            {
                                TextBlock pTemp = new TextBlock();
                                pTemp.HorizontalAlignment = HorizontalAlignment.Left;
                                pTemp.Width = 580;
                                pTemp.Height = 22;
                                pTemp.FontSize = 14;
                                pTemp.TextWrapping = TextWrapping.Wrap;
                                pTemp.Text = "Product Temperature: ";
                                pTemp.FontWeight = FontWeights.Bold;
                                pTemp.TextTrimming = TextTrimming.CharacterEllipsis;
                                productNames.Children.Add(pTemp);

                                string tempTemperature = "";
                                switch (prod.ProductTemperature)
                                {
                                    case 1: tempTemperature = "Hot"; break;
                                    case 2: tempTemperature = "Cold"; break;
                                }
                                TextBlock prodTemp = new TextBlock();
                                prodTemp.Text = tempTemperature;
                                prodTemp.Width = 72;
                                prodTemp.Height = 22;
                                prodTemp.FontSize = 14;
                                productPrices.Children.Add(prodTemp);

                                prodBorder.Height += 22;
                                extraHeight += 22;
                            }

                            if (prod.IceLevel > 0)
                            {
                                TextBlock pIce = new TextBlock();
                                pIce.HorizontalAlignment = HorizontalAlignment.Left;
                                pIce.Width = 580;
                                pIce.Height = 22;
                                pIce.TextWrapping = TextWrapping.Wrap;
                                pIce.Text = "Ice Level ";
                                pIce.FontSize = 14;
                                pIce.FontWeight = FontWeights.Bold;
                                pIce.TextTrimming = TextTrimming.CharacterEllipsis;
                                productNames.Children.Add(pIce);

                                string tempIce = "";
                                switch (prod.IceLevel)
                                {
                                    case 1: tempIce = "No Ice"; break;
                                    case 2: tempIce = "Less Ice"; break;
                                    case 3: tempIce = "Regular Ice"; break;
                                    case 4: tempIce = "More Ice"; break;
                                    case 5: tempIce = "Full Ice"; break;
                                }
                                TextBlock prodIce = new TextBlock();
                                prodIce.Text = tempIce;
                                prodIce.Width = 72;
                                prodIce.FontSize = 14;
                                prodIce.Height = 22;
                                productPrices.Children.Add(prodIce);

                                prodBorder.Height += 22;
                                extraHeight += 22;
                            }

                            if (prod.ReheatProduct > 0)
                            {
                                TextBlock pReheat = new TextBlock();
                                pReheat.HorizontalAlignment = HorizontalAlignment.Left;
                                pReheat.Width = 580;
                                pReheat.Height = 22;
                                pReheat.TextWrapping = TextWrapping.Wrap;
                                pReheat.Text = "Reheat Product: ";
                                pReheat.FontSize = 14;
                                pReheat.FontWeight = FontWeights.Bold;
                                pReheat.TextTrimming = TextTrimming.CharacterEllipsis;
                                productNames.Children.Add(pReheat);

                                string tempHeat = "";
                                switch (prod.IceLevel)
                                {
                                    case 1: tempHeat = "Yes"; break;
                                    case 2: tempHeat = "No"; break;
                                }
                                TextBlock prodHeat = new TextBlock();
                                prodHeat.Text = tempHeat;
                                prodHeat.Width = 72;
                                prodHeat.FontSize = 14;
                                prodHeat.Height = 22;
                                productPrices.Children.Add(prodHeat);

                                prodBorder.Height += 22;
                                extraHeight += 22;
                            }

                            if (prod.EspressoShotPrice > 0)
                            {
                                TextBlock totalShots = new TextBlock();
                                totalShots.HorizontalAlignment = HorizontalAlignment.Left;
                                totalShots.Width = 580;
                                totalShots.Height = 22;
                                totalShots.FontSize = 14;
                                totalShots.TextWrapping = TextWrapping.Wrap;
                                totalShots.Text = "Espresso Shot:";
                                totalShots.FontWeight = FontWeights.Bold;
                                totalShots.TextTrimming = TextTrimming.CharacterEllipsis;
                                productNames.Children.Add(totalShots);

                                TextBlock cost1 = new TextBlock();
                                cost1.Text = prod.EspressoShotPrice.ToString();
                                cost1.Width = 72;
                                cost1.FontSize = 14;
                                cost1.Height = 22;
                                productPrices.Children.Add(cost1);
                                total += prod.EspressoShotPrice;

                                prodBorder.Height += 22;
                                extraHeight += 22;
                            }

                            if (prod.ProductMilk.Count > 0)
                            {
                                TextBlock header = new TextBlock();
                                header.HorizontalAlignment = HorizontalAlignment.Left;
                                header.Width = 580;
                                header.Height = 22;
                                header.FontSize = 14;
                                header.Text = "Selected Milk";
                                header.FontWeight = FontWeights.Bold;
                                productNames.Children.Add(header);

                                TextBlock cost1 = new TextBlock();
                                cost1.Text = "";
                                cost1.Width = 72;
                                cost1.FontSize = 14;
                                cost1.Height = 22;
                                productPrices.Children.Add(cost1);

                                prodBorder.Height += 22;
                                extraHeight += 22;
                                foreach (Milk milk in prod.ProductMilk)
                                {
                                    TextBlock title = new TextBlock();
                                    title.HorizontalAlignment = HorizontalAlignment.Left;
                                    title.Width = 580;
                                    title.Height = 22;
                                    title.TextWrapping = TextWrapping.Wrap;
                                    title.Text = milk.MilkName;
                                    title.FontSize = 14;
                                    title.TextTrimming = TextTrimming.CharacterEllipsis;
                                    productNames.Children.Add(title);

                                    TextBlock cost3 = new TextBlock();
                                    cost3.Text = milk.MilkPrice.ToString();
                                    total += milk.MilkPrice;
                                    cost3.Width = 72;
                                    cost3.Height = 22;
                                    cost3.FontSize = 14;
                                    cost3.HorizontalAlignment = HorizontalAlignment.Left;
                                    productPrices.Children.Add(cost3);

                                    prodBorder.Height += 22;
                                    extraHeight += 22;
                                }
                            }

                            if (prod.ProductAddon.Count > 0)
                            {
                                TextBlock header = new TextBlock();
                                header.HorizontalAlignment = HorizontalAlignment.Left;
                                header.Width = 580;
                                header.Height = 22;
                                header.FontSize = 14;
                                header.Text = "Selected Add-ons";
                                header.FontWeight = FontWeights.Bold;
                                productNames.Children.Add(header);

                                TextBlock cost1 = new TextBlock();
                                cost1.Text = "";
                                cost1.Width = 72;
                                cost1.FontSize = 14;
                                cost1.Height = 22;
                                productPrices.Children.Add(cost1);

                                prodBorder.Height += 22;
                                extraHeight += 22;
                                foreach (Addon addon in prod.ProductAddon)
                                {
                                    TextBlock title = new TextBlock();
                                    title.HorizontalAlignment = HorizontalAlignment.Left;
                                    title.Width = 580;
                                    title.Height = 22;
                                    title.TextWrapping = TextWrapping.Wrap;
                                    title.Text = addon.AddonName;
                                    title.FontSize = 14;
                                    title.TextTrimming = TextTrimming.CharacterEllipsis;
                                    productNames.Children.Add(title);

                                    TextBlock cost4 = new TextBlock();
                                    cost4.Text = addon.AddonPrice.ToString();
                                    total += addon.AddonPrice;
                                    cost4.Width = 72;
                                    cost4.Height = 22;
                                    cost4.FontSize = 14;
                                    productPrices.Children.Add(cost4);

                                    prodBorder.Height += 22;
                                    extraHeight += 22;
                                }
                            }

                            if (prod.ProductSyrup.Count > 0)
                            {
                                TextBlock header = new TextBlock();
                                header.HorizontalAlignment = HorizontalAlignment.Left;
                                header.Width = 580;
                                header.Height = 22;
                                header.FontSize = 14;
                                header.Text = "Selected Syrup";
                                header.FontWeight = FontWeights.Bold;
                                productNames.Children.Add(header);

                                TextBlock cost1 = new TextBlock();
                                cost1.Text = "";
                                cost1.Width = 72;
                                cost1.FontSize = 14;
                                cost1.Height = 22;
                                productPrices.Children.Add(cost1);

                                prodBorder.Height += 22;
                                extraHeight += 22;
                                foreach (Syrup syrup in prod.ProductSyrup)
                                {
                                    TextBlock title = new TextBlock();
                                    title.HorizontalAlignment = HorizontalAlignment.Left;
                                    title.Width = 580;
                                    title.Height = 22;
                                    title.TextWrapping = TextWrapping.Wrap;
                                    title.Text = syrup.SyrupName;
                                    title.TextTrimming = TextTrimming.CharacterEllipsis;
                                    title.FontSize = 14;
                                    productNames.Children.Add(title);

                                    TextBlock cost5 = new TextBlock();
                                    cost5.Text = syrup.SyrupPrice.ToString();
                                    total += syrup.SyrupPrice;
                                    cost5.Width = 72;
                                    cost5.Height = 22;
                                    cost5.FontSize = 14;
                                    productPrices.Children.Add(cost5);

                                    prodBorder.Height += 22;
                                    extraHeight += 22;
                                }
                            }

                            if (prod.ProductSweetener.Count > 0)
                            {
                                TextBlock header = new TextBlock();
                                header.HorizontalAlignment = HorizontalAlignment.Left;
                                header.Width = 580;
                                header.Height = 22;
                                header.FontSize = 14;
                                header.Text = "Selected Sweetener";
                                header.FontWeight = FontWeights.Bold;
                                productNames.Children.Add(header);

                                TextBlock cost1 = new TextBlock();
                                cost1.Text = "";
                                cost1.Width = 72;
                                cost1.FontSize = 14;
                                cost1.Height = 22;
                                productPrices.Children.Add(cost1);

                                prodBorder.Height += 22;
                                extraHeight += 22;
                                foreach (Sweetener sweet in prod.ProductSweetener)
                                {
                                    TextBlock title = new TextBlock();
                                    title.HorizontalAlignment = HorizontalAlignment.Left;
                                    title.Width = 580;
                                    title.Height = 22;
                                    title.TextWrapping = TextWrapping.Wrap;
                                    title.Text = sweet.SweetenerName;
                                    title.TextTrimming = TextTrimming.CharacterEllipsis;
                                    title.FontSize = 14;
                                    productNames.Children.Add(title);

                                    TextBlock cost5 = new TextBlock();
                                    cost5.Text = sweet.SweetenerPrice.ToString();
                                    total += sweet.SweetenerPrice;
                                    cost5.Width = 72;
                                    cost5.Height = 22;
                                    cost5.FontSize = 14;
                                    productPrices.Children.Add(cost5);

                                    prodBorder.Height += 22;
                                    extraHeight += 22;
                                }
                            }

                            if (prod.ProductBase.Count > 0)
                            {
                                TextBlock header = new TextBlock();
                                header.HorizontalAlignment = HorizontalAlignment.Left;
                                header.Width = 580;
                                header.Height = 22;
                                header.FontSize = 14;
                                header.Text = "Selected Base";
                                header.FontWeight = FontWeights.Bold;
                                productNames.Children.Add(header);

                                TextBlock cost1 = new TextBlock();
                                cost1.Text = "";
                                cost1.Width = 72;
                                cost1.FontSize = 14;
                                cost1.Height = 22;
                                productPrices.Children.Add(cost1);

                                prodBorder.Height += 22;
                                extraHeight += 22;
                                foreach (Base cBase in prod.ProductBase)
                                {
                                    TextBlock title = new TextBlock();
                                    title.HorizontalAlignment = HorizontalAlignment.Left;
                                    title.Width = 580;
                                    title.Height = 22;
                                    title.TextWrapping = TextWrapping.Wrap;
                                    title.Text = cBase.BaseName;
                                    title.FontSize = 14;
                                    title.TextTrimming = TextTrimming.CharacterEllipsis;
                                    productNames.Children.Add(title);

                                    TextBlock cost5 = new TextBlock();
                                    cost5.Text = cBase.BasePrice.ToString();
                                    total += cBase.BasePrice;
                                    cost5.Width = 72;
                                    cost5.Height = 22;
                                    cost5.FontSize = 14;
                                    productPrices.Children.Add(cost5);

                                    prodBorder.Height += 22;
                                    extraHeight += 22;
                                }
                            }

                            TextBlock compHeader = new TextBlock();
                            compHeader.HorizontalAlignment = HorizontalAlignment.Left;
                            compHeader.Width = 580;
                            compHeader.Height = 22;
                            compHeader.FontSize = 14;
                            compHeader.TextWrapping = TextWrapping.Wrap;
                            compHeader.Text = "Price Computation";
                            compHeader.FontWeight = FontWeights.Bold;
                            compHeader.TextTrimming = TextTrimming.CharacterEllipsis;
                            productNames.Children.Add(compHeader);

                            TextBlock space1 = new TextBlock();
                            space1.Text = "";
                            space1.Width = 72;
                            space1.FontSize = 14;
                            space1.Height = 22;
                            productPrices.Children.Add(space1);

                            prodBorder.Height += 22;
                            extraHeight += 22;

                            TextBlock totalC = new TextBlock();
                            totalC.HorizontalAlignment = HorizontalAlignment.Left;
                            totalC.Width = 580;
                            totalC.Height = 22;
                            totalC.FontSize = 14;
                            totalC.TextWrapping = TextWrapping.Wrap;
                            totalC.Text = "Base Total:";
                            totalC.FontWeight = FontWeights.Bold;
                            totalC.TextTrimming = TextTrimming.CharacterEllipsis;
                            productNames.Children.Add(totalC);

                            TextBlock cost = new TextBlock();
                            cost.Text = total.ToString();
                            cost.Width = 72;
                            cost.FontSize = 14;
                            cost.Height = 22;
                            productPrices.Children.Add(cost);

                            prodBorder.Height += 22;
                            extraHeight += 22;

                            TextBlock quan = new TextBlock();
                            quan.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                            quan.Width = 580;
                            quan.Height = 22;
                            quan.FontSize = 14;
                            quan.TextWrapping = TextWrapping.Wrap;
                            quan.Text = "Product Quantity:";
                            quan.FontWeight = FontWeights.Bold;
                            quan.TextTrimming = TextTrimming.CharacterEllipsis;
                            productNames.Children.Add(quan);

                            TextBlock quanNum = new TextBlock();
                            quanNum.Text = prod.ProductQuantity.ToString();
                            quanNum.Width = 72;
                            quanNum.Height = 22;
                            quanNum.FontSize = 14;
                            productPrices.Children.Add(quanNum);

                            prodBorder.Height += 22;
                            extraHeight += 22;

                            TextBlock totalUnit = new TextBlock();
                            totalUnit.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                            totalUnit.Width = 580;
                            totalUnit.Height = 22;
                            totalUnit.FontSize = 14;
                            totalUnit.TextWrapping = TextWrapping.Wrap;
                            totalUnit.Text = "Base Total x Quantity:";
                            totalUnit.FontWeight = FontWeights.Bold;
                            totalUnit.TextTrimming = TextTrimming.CharacterEllipsis;
                            productNames.Children.Add(totalUnit);

                            TextBlock totalNum = new TextBlock();
                            totalNum.Text = (total * prod.ProductQuantity).ToString();
                            totalNum.Width = 72;
                            totalNum.Height = 22;
                            totalNum.FontSize = 14;
                            totalNum.FontWeight = FontWeights.Bold;
                            productPrices.Children.Add(totalNum);

                            prodBorder.Height += 22;
                            extraHeight += 22;

                            extraPanelHeight.Text = extraHeight.ToString();
                        }
                    }
                    else if (productSummaryPanel.Visibility == Visibility.Visible)
                    {
                        productSummaryPanel.Visibility = Visibility.Collapsed;
                        arrowImg.Source = arrowDown;
                        prodBorder.Height = 170;
                    }
                };

                // 3rd child
                StackPanel buttonsPanel = new StackPanel();
                buttonsPanel.Height = 50;
                buttonsPanel.Orientation = Orientation.Horizontal;
                buttonsPanel.Margin = new Thickness(0, 10, 0, 0);

                Style style = (Style)this.Resources["RoundButtonStyle"];
                Button editButton = new Button();
                editButton.Content = "Edit Product";
                editButton.Width = 100;
                editButton.Height = 40;
                editButton.FontSize = 15;
                editButton.Style = style;
                editButton.Background = new SolidColorBrush(Colors.White);
                editButton.Margin = new Thickness(20, 0, 20, 0);
                editButton.Click += (s, e) =>
                {
                    EditMode = true;
                    
                    int start = Convert.ToInt32(CurrentProductIndex.Text);
                    Product targetProduct = ProductCart[start];
                    ProductCartLocation = start;

                    productNames.Children.Clear();
                    productPrices.Children.Clear();
                    productSummaryPanel.Visibility = Visibility.Collapsed;
                    arrowImg.Source = arrowDown;
                    prodBorder.Height = 170;
                    CurrentMilk.Clear();
                    foreach (Milk milk in targetProduct.ProductMilk) CurrentMilk.Add(milk);
                    CurrentAddon.Clear();
                    foreach (Addon add in targetProduct.ProductAddon) CurrentAddon.Add(add);
                    CurrentSyrup.Clear();
                    foreach (Syrup syrup in targetProduct.ProductSyrup) CurrentSyrup.Add(syrup);
                    CurrentSweet.Clear();
                    foreach (Sweetener sweet in targetProduct.ProductSweetener) CurrentSweet.Add(sweet);
                    CurrentBase.Clear();
                    foreach (Base cBase in targetProduct.ProductBase) CurrentBase.Add(cBase);

                    LoadCustomizeMenu(nameBlock.Text, true);
                    CloseCustButton.Content = "Return to Cart";
                    AddProductToCart.Content = "Apply changes";
                    ProductCartPanel.Visibility = Visibility.Hidden;
                    LowerCartInterface.Visibility = Visibility.Hidden;
                    ProductCustInterface.Visibility = Visibility.Visible;
                };
                buttonsPanel.Children.Add(editButton);

                TextBlock quantityDisplay = new TextBlock();
                quantityDisplay.Text = product.ProductQuantity.ToString();
                quantityDisplay.FontSize = 15;
                quantityDisplay.Width = 25;
                quantityDisplay.Height = 25;
                quantityDisplay.TextAlignment = TextAlignment.Center;
                quantityDisplay.Margin = new Thickness(10, 0, 10, 0);

                Button removeButton = new Button();
                removeButton.Content = "Remove Product";
                removeButton.Width = 125;
                removeButton.Height = 40;
                removeButton.FontSize = 15;
                removeButton.Style = style;
                removeButton.Background = new SolidColorBrush(Colors.White);
                removeButton.Margin = new Thickness(0, 0, 0, 0);
                removeButton.Click += (s, e) =>
                {
                    int quantity = Convert.ToInt32(quantityDisplay.Text);
                    int curQuantity = Convert.ToInt32(CartQuantity.Text);
                    CartQuantity.Text = (curQuantity - quantity).ToString();

                    double price = Convert.ToDouble(productPrice.Text);
                    double curPrice = Convert.ToDouble(CartPrice.Text);
                    CartPrice.Text = (curPrice - price).ToString("F2");

                    int loc = Convert.ToInt32(CurrentProductIndex.Text);
                    ProductCart.RemoveAt(loc);  // O(n)
                    FoodCartContent.Children.RemoveAt(loc);  // O(n)

                    ShiftCartIndex(loc);  // O(n)
                    prodBorder.Visibility = Visibility.Hidden;
                    prodBorder = null;
                };
                buttonsPanel.Children.Add(removeButton);

                buttonsPanel.Children.Add(arrowImg);

                StackPanel quantityPanel = new StackPanel();
                quantityPanel.Width = 100;
                quantityPanel.Orientation = Orientation.Horizontal;

                Button btnPlus = new Button();
                btnPlus.Content = "+";
                btnPlus.Width = 25;
                btnPlus.Height = 25;
                btnPlus.FontSize = 15;
                btnPlus.Style = style;
                btnPlus.Background = new SolidColorBrush(Colors.White);
                btnPlus.FontFamily = new FontFamily("Cascadia Mono");
                btnPlus.Click += (s, e) =>
                {
                    productSummaryPanel.Visibility = Visibility.Collapsed;
                    prodBorder.Height = 170;
                    productNames.Children.Clear();
                    productPrices.Children.Clear();
                    arrowImg.Source = arrowDown;
                    int qty = int.Parse(quantityDisplay.Text);
                    double newPrice = Convert.ToDouble(productPrice.Text);
                    int checkQuantity = QuantityInvalid(nameBlock.Text, qty + 1);
                    double newTotalPrice = Convert.ToDouble(CartPrice.Text);
                    newTotalPrice -= newPrice;
                    if (checkQuantity == 1)
                    {
                        MessageBox.Show("The quantity of the product cannot go past this point.", "Max quantity reached", MessageBoxButton.OK, MessageBoxImage.Hand);
                        return;
                    }
                    if (checkQuantity == 2)
                    {
                        FillMenus();
                        OrderingInterface.Visibility = Visibility.Visible;
                        ProductCartPanel.Visibility = Visibility.Hidden;
                        ProductCart.Remove(new Product(nameBlock.Text));
                        MessageBox.Show("This product does not exist in the database, you will be returned to the menu.", "Invalid product", MessageBoxButton.OK, MessageBoxImage.Hand);
                        return;
                    }
                    if (qty >= 99) return;
                    int prodIndex = Convert.ToInt32(CurrentProductIndex.Text);
                    Product prod = ProductCart[prodIndex];
                    productPrice.Text = ((newPrice / qty) * (qty + 1)).ToString("F2");
                    newPrice = Convert.ToDouble(productPrice.Text);
                    prod.ProductPrice = newPrice;
                    CartPrice.Text = (newTotalPrice + newPrice).ToString("F2");
                    quantityDisplay.Text = (qty + 1).ToString();
                    prod.ProductQuantity = qty + 1;
                    ProductCart[prodIndex] = prod;
                    int curQuantity = Convert.ToInt32(CartQuantity.Text);
                    CartQuantity.Text = (curQuantity + 1).ToString();
                };

                Button btnMinus = new Button();
                btnMinus.Content = "-";
                btnMinus.Width = 25;
                btnMinus.Height = 25;
                btnMinus.FontSize = 15;
                btnMinus.Style = style;
                btnMinus.Background = new SolidColorBrush(Colors.White);
                btnMinus.FontFamily = new FontFamily("Cascadia Mono");
                btnMinus.Click += (s, e) =>
                {
                    productSummaryPanel.Visibility = Visibility.Collapsed;
                    prodBorder.Height = 170;
                    productNames.Children.Clear();
                    productPrices.Children.Clear();
                    arrowImg.Source = arrowDown;
                    int qty = int.Parse(quantityDisplay.Text);
                    double newPrice = 0;

                    if (qty == 1) return;
                    int checkQuantity = QuantityInvalid(nameBlock.Text, qty - 1);
                    if (checkQuantity == 1)
                    {
                        MessageBox.Show("The current quantity of the product is larger than the available product quantity.", "Max quantity reached", MessageBoxButton.OK, MessageBoxImage.Hand);
                        quantityDisplay.Text = "0";
                        return;
                    }
                    if (checkQuantity == 2)
                    {
                        FillMenus();
                        ProductCartPanel.Visibility = Visibility.Hidden;
                        OrderingInterface.Visibility = Visibility.Visible;
                        ProductCart.Remove(new Product(nameBlock.Text));
                        MessageBox.Show("This product does not exist in the database, you will be returned to the menu.", "Invalid product", MessageBoxButton.OK, MessageBoxImage.Hand);
                        return;
                    }
                    double newTotalPrice = Convert.ToDouble(CartPrice.Text);
                    newPrice = Convert.ToDouble(productPrice.Text);
                    newTotalPrice -= newPrice;
                    productPrice.Text = ((newPrice / qty) * (qty - 1)).ToString("F2");
                    newPrice = Convert.ToDouble(productPrice.Text);
                    int prodIndex = Convert.ToInt32(CurrentProductIndex.Text);
                    Product prod = ProductCart[prodIndex];
                    prod.ProductPrice = newPrice;
                    quantityDisplay.Text = (qty - 1).ToString();
                    prod.ProductQuantity = qty - 1;
                    ProductCart[prodIndex] = prod;
                    int curQuantity = Convert.ToInt32(CartQuantity.Text);
                    CartQuantity.Text = (curQuantity - 1).ToString();
                    CartPrice.Text = (newTotalPrice + newPrice).ToString("F2");
                };
                quantityPanel.Children.Add(btnMinus);
                quantityPanel.Children.Add(quantityDisplay);
                quantityPanel.Children.Add(btnPlus);
                buttonsPanel.Children.Add(quantityPanel);
                productPanel.Children.Add(buttonsPanel);
                FoodCartContent.Children.Add(prodBorder);
                ++index;
            }
        }

        private void ViewCartButton_Click(object sender, RoutedEventArgs e)
        {
            FoodCartScroll.ScrollToVerticalOffset(0);
            if (CurrentOrderMode == 1) CurrentOrderMethod.Text = "Dine-in";
            else if (CurrentOrderMode == 2) CurrentOrderMethod.Text = "Take-out";
            OrderingInterface.Visibility = Visibility.Hidden;
            ProductCartPanel.Visibility = Visibility.Visible;
        }

        private void ReturnToMenuButton_Click(object sender, RoutedEventArgs e)
        {
            OrderingInterface.Visibility = Visibility.Visible;
            ProductCartPanel.Visibility = Visibility.Hidden;
        }

        private void ServedAtCounterButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentClaimMode = 1;
            ProcessCustomerOrder();
        }

        private void ServedAtTableButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CustomerTableNumber.Text))
            {
                MessageBox.Show("Please provide a table number if it is available.", "No table number", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            int tableNum = 0;
            if (int.TryParse(CustomerTableNumber.Text, out tableNum) == false)
            {
                MessageBox.Show("Please provide a valid table number, only digits are allowed.", "Invalid table number", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            CurrentClaimMode = 2;
            ProcessCustomerOrder();
        }

        private void CancelCustomerOrder_Click(object sender, RoutedEventArgs e)
        {
            ClaimModePanel.Visibility = Visibility.Collapsed;
            ProductCartPanel.Visibility = Visibility.Visible;
            LowerCartInterface.Visibility = Visibility.Visible;
            CustomerTableNumber.Text = "";
        }

        private void ConfirmOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductCart.Count == 0)
            {
                MessageBox.Show("Please select a product to order before proceeding with the check-out.", "Product cart is empty", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            ProductCartPanel.Visibility = Visibility.Collapsed;
            LowerCartInterface.Visibility = Visibility.Collapsed;
            ClaimModePanel.Visibility = Visibility.Visible;
        }

        private void ProcessCustomerOrder()
        {
            CreateReceipt();                  
            ClaimModePanel.Visibility = Visibility.Collapsed;

            if (CurrentOrderMode == 1) CustOrderMode.Text = "Dine-in";
            else if (CurrentOrderMode == 2) CustOrderMode.Text = "Take-out";
            ResetOrderSystemState();
            OrderEndingPage.Visibility = Visibility.Visible;
        }

        public string PaddedReceiptLine(string name, string quantity, string price)
        {
            if (name.Length > 52)
            {
                StringBuilder formatted = new StringBuilder();
                for (int i = 0; i < 49; ++i) formatted.Append(name[i]);
                for (int i = 0; i < 3; ++i) formatted.Append(".");
                name = formatted.ToString();
            }
            string nameSpace = "", priceSpace = "", quantitySpace = "";
            switch (name.Length)
            {
                case 51: nameSpace = " "; break;
                case 50: nameSpace = "  "; break;
                case 49: nameSpace = "   "; break;
                case 48: nameSpace = "    "; break;
                case 47: nameSpace = "     "; break;
                case 46: nameSpace = "      "; break;
                case 45: nameSpace = "       "; break;
                case 44: nameSpace = "        "; break;
                case 43: nameSpace = "         "; break;
                case 42: nameSpace = "          "; break;
                case 41: nameSpace = "           "; break;
                case 40: nameSpace = "            "; break;
                case 39: nameSpace = "             "; break;
                case 38: nameSpace = "              "; break;
                case 37: nameSpace = "               "; break;
                case 36: nameSpace = "                "; break;
                case 35: nameSpace = "                 "; break;
                case 34: nameSpace = "                  "; break;
                case 33: nameSpace = "                   "; break;
                case 32: nameSpace = "                    "; break;
                case 31: nameSpace = "                     "; break;
                case 30: nameSpace = "                      "; break;
                case 29: nameSpace = "                       "; break;
                case 28: nameSpace = "                        "; break;
                case 27: nameSpace = "                         "; break;
                case 26: nameSpace = "                          "; break;
                case 25: nameSpace = "                           "; break;
                case 24: nameSpace = "                            "; break;
                case 23: nameSpace = "                             "; break;
                case 22: nameSpace = "                              "; break;
                case 21: nameSpace = "                               "; break;
                case 20: nameSpace = "                                "; break;
                case 19: nameSpace = "                                 "; break;
                case 18: nameSpace = "                                  "; break;
                case 17: nameSpace = "                                   "; break;
                case 16: nameSpace = "                                    "; break;
                case 15: nameSpace = "                                     "; break;
                case 14: nameSpace = "                                      "; break;
                case 13: nameSpace = "                                       "; break;
                case 12: nameSpace = "                                        "; break;
                case 11: nameSpace = "                                         "; break;
                case 10: nameSpace = "                                          "; break;
                case 9: nameSpace = "                                           "; break;
                case 8: nameSpace = "                                            "; break;
                case 7: nameSpace = "                                             "; break;
                case 6: nameSpace = "                                              "; break;
                case 5: nameSpace = "                                               "; break;
                case 4: nameSpace = "                                                "; break;
                case 3: nameSpace = "                                                 "; break;
                case 2: nameSpace = "                                                  "; break;
                case 1: nameSpace = "                                                   "; break;
            }

            switch (price.Length)
            {
                case 12: priceSpace = " "; break;
                case 11: priceSpace = "  "; break;
                case 10: priceSpace = "   "; break;
                case 9: priceSpace = "    "; break;
                case 8: priceSpace = "     "; break;
                case 7: priceSpace = "      "; break;
                case 6: priceSpace = "       "; break;
                case 5: priceSpace = "        "; break;
                case 4: priceSpace = "         "; break;
                case 3: priceSpace = "          "; break;
                case 2: priceSpace = "           "; break;
                case 1: priceSpace = "            "; break;
            }

            switch (quantity.Length)
            {
                case 6: quantitySpace = " "; break;
                case 5: quantitySpace = "  "; break;
                case 4: quantitySpace = "   "; break;
                case 3: quantitySpace = "    "; break;
                case 2: quantitySpace = "     "; break;
                case 1: quantitySpace = "      "; break;
                default: quantitySpace = "       "; break;
            }
            return $"| {quantitySpace}{quantity} | {name}{nameSpace} | {priceSpace}₱{price} |\n";
        }

        private void CreateReceipt()
        {
            StringBuilder receipt = new StringBuilder();
            receipt.Append("+---------------------------------------------------------------------------------+\n");
            receipt.Append("|                              Your Order Receipt                                 |\n");
            receipt.Append("+---------+------------------------------------------------------+----------------+\n");
            receipt.Append("|   Qty   |                       Product Name                   |      Total     |\n");
            receipt.Append("+---------+------------------------------------------------------+----------------+\n");
            // Max 7, 52, 13 characters
            foreach (Product product in ProductCart)
            {
                string name = product.ProductName;
                string price = string.Format("{0:F2}", product.BasePrice * product.ProductQuantity);
                string quantity = product.ProductQuantity.ToString();
                receipt.Append(PaddedReceiptLine(name, quantity, price));

                if (product.EspressoShotAmount > 0)
                {
                    price = string.Format("{0:F2}", product.EspressoShotPrice);
                    quantity = product.EspressoShotAmount.ToString();
                    receipt.Append(PaddedReceiptLine("Espresso Shot", quantity, price));
                }

                foreach (Milk milk in product.ProductMilk)
                {
                    name = milk.MilkName;
                    price = string.Format("{0:F2}", milk.MilkPrice);
                    quantity = milk.MilkQuantity.ToString();
                    receipt.Append(PaddedReceiptLine(name, quantity, price));
                }

                foreach (Addon addon in product.ProductAddon)
                {
                    name = addon.AddonName;
                    price = string.Format("{0:F2}", addon.AddonPrice);
                    quantity = addon.AddonQuantity.ToString();
                    receipt.Append(PaddedReceiptLine(name, quantity, price));
                }

                foreach (Syrup syrup in product.ProductSyrup)
                {
                    name = syrup.SyrupName;
                    price = string.Format("{0:F2}", syrup.SyrupPrice);
                    quantity = syrup.SyrupQuantity.ToString();
                    receipt.Append(PaddedReceiptLine(name, quantity, price));
                }

                foreach (Sweetener sweet in product.ProductSweetener)
                {
                    name = sweet.SweetenerName;
                    price = string.Format("{0:F2}", sweet.SweetenerPrice);
                    quantity = sweet.SweetenerQuantity.ToString();
                    receipt.Append(PaddedReceiptLine(name, quantity, price));
                }

                foreach (Base cBase in product.ProductBase)
                {
                    name = cBase.BaseName;
                    price = string.Format("{0:F2}", cBase.BasePrice);
                    quantity = cBase.BaseQuantity.ToString();
                    receipt.Append(PaddedReceiptLine(name, quantity, price));
                }

                receipt.Append(PaddedReceiptLine("Total", "", product.ProductPrice.ToString("F2")));
                receipt.Append("+---------+------------------------------------------------------+----------------+\n");
            }

            int orderNumber = RetrieveIncrementOrderNumber();
            string numberSpace = "";
            string orderNum = orderNumber.ToString();
            
            StringBuilder tempNum = new StringBuilder();
            string convertedNum = orderNum;
            switch (convertedNum.Length)
            {
                case 1: tempNum.Append("0000"); break;
                case 2: tempNum.Append("000"); break;
                case 3: tempNum.Append("00"); break;
                case 4: tempNum.Append("0"); break;
            }
            tempNum.Append(convertedNum);
            CustOrderNumber.Text = tempNum.ToString();

            switch (orderNum.Length)
            {
                case 5: numberSpace = " "; break;
                case 4: numberSpace = "  "; break;
                case 3: numberSpace = "   "; break;
                case 2: numberSpace = "    "; break;
                case 1: numberSpace = "     "; break;
            }
            receipt.Append($"| Your order number is:                                                    {numberSpace}{orderNum} |\n");

            string modeSpace = "";
            string orderMode = "";
            switch (CurrentOrderMode)
            {
                case 1: orderMode = "Dine-in"; break;
                case 2: orderMode = "Take-out"; break;
                default: orderMode = "Dine-in"; break;
            }
            switch (orderMode.Length)
            {
                case 7: modeSpace = " "; break;
                case 6: modeSpace = "  "; break;
            }
            receipt.Append($"| Order Type:                                                            {modeSpace}{orderMode} |\n");

            string tableNumSpace = "";
            string tableNum = "";
            if (!string.IsNullOrWhiteSpace(CustomerTableNumber.Text)) tableNum = CustomerTableNumber.Text;
            if (tableNum != "")
            {
                switch (tableNum.Length)
                {
                    case 7: tableNumSpace = " "; break;
                    case 6: tableNumSpace = "  "; break;
                    case 5: tableNumSpace = "   "; break;
                    case 4: tableNumSpace = "    "; break;
                    case 3: tableNumSpace = "     "; break;
                    case 2: tableNumSpace = "      "; break;
                    case 1: tableNumSpace = "       "; break;
                }
                receipt.Append($"| Table Number:                                                          {tableNumSpace}{tableNum} |\n");
            }

            string number = "";
            receipt.Append("+---------------------------------------------------------------------------------+\n");
            Random rand = new Random();
            int first = rand.Next(1, 100000), second = rand.Next(1, 100000), third = rand.Next(1, 100000);
            string firstPart = first.ToString(), secondPart = second.ToString(), thirdPart = third.ToString();
            StringBuilder id = new StringBuilder();
            switch (firstPart.Length)
            {
                case 4: id.Append("0"); break;
                case 3: id.Append("00"); break;
                case 2: id.Append("000"); break;
                case 1: id.Append("0000"); break;
            }
            id.Append(firstPart);
            id.Append("-");
            switch (secondPart.Length)
            {
                case 4: id.Append("0"); break;
                case 3: id.Append("00"); break;
                case 2: id.Append("000"); break;
                case 1: id.Append("0000"); break;
            }
            id.Append(secondPart);
            id.Append("-");
            switch (thirdPart.Length)
            {
                case 4: id.Append("0"); break;
                case 3: id.Append("00"); break;
                case 2: id.Append("000"); break;
                case 1: id.Append("0000"); break;
            }
            id.Append(thirdPart);
            number = id.ToString();
            receipt.Append($"| Receipt Number:                                               {number} |\n");

            // Max -> 19
            DateTime date = DateTime.Now;
            string formattedDate = date.ToString("MM/dd/yyyy hh:mm tt");
            receipt.Append($"| Transaction Date:                                           {formattedDate} |\n");

            receipt.Append("+---------------------------------------------------------------------------------+");
            SaveOrder(orderNumber); // SaveOrder must be called here since the old orderNumber before increment is still kept 
            ReceiptPrint(receipt, number);
        }

        private void ReceiptPrint(StringBuilder target, string number)
        {
            string receipt = target.ToString();
            // Wrapping the OrderReceipt in single quotation mark avoids d and t to be mutated to a different data
            // Since d and t are special formatting characters on DateTime
            string receiptname = DateTime.Now.ToString("'OrderReceipt'_yyyyMMdd_HHmmss_fffffff");
            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            string fileName = string.Format("{0}.xml", receiptname);
            XmlSerializer processReceipt = new XmlSerializer(typeof(string));
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            XmlWriter receiptFile = XmlWriter.Create(fileName, settings);
            processReceipt.Serialize(receiptFile, receipt, emptyNamespaces);
            receiptFile.Close(); return;
        }

        private void ReorderButton_Click(object sender, RoutedEventArgs e)
        {
            OrderEndingPage.Visibility = Visibility.Hidden;
            CustomerModeLandingPage.Visibility = Visibility.Visible;
            if (PromotionVideo.Source != null) PromotionVideo.Play();
        }

        private void ReloadPromotionDisplay(string location)
        {
            PromotionImage.Source = null;
            PromotionVideo.Source = null;
            if (File.Exists(location))
            {
                BitmapImage bitmap = null;
                bool first_fail = false, second_fail = false;
                BlankMediaMessage.Visibility = Visibility.Collapsed;
                try
                {
                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(location);
                    bitmap.EndInit();
                    bitmap.Freeze();
                    PromotionVideo.Visibility = Visibility.Collapsed;
                    PromotionImage.Visibility = Visibility.Visible;
                    PromotionImage.Source = bitmap;
                }
                catch (NotSupportedException) { first_fail = true; }
                catch (FileFormatException) { first_fail = true; }
                catch (UriFormatException) { first_fail = true; }
                try
                {
                    PromotionVideo.Source = new Uri(location);
                    PromotionVideo.Visibility = Visibility.Visible;
                    PromotionImage.Visibility = Visibility.Collapsed;
                    PromotionVideo.IsMuted = true;
                    PromotionVideo.Play();
                }
                catch (NotSupportedException) { second_fail = true; }
                catch (FileFormatException) { second_fail = true; }
                catch (UriFormatException) { second_fail = true; }
                if (first_fail && second_fail)
                {
                    BlankMediaMessage.Visibility = Visibility.Visible;
                    PromotionVideo.Visibility = Visibility.Collapsed;
                    PromotionImage.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void PromotionVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            PromotionVideo.Position = TimeSpan.Zero;
            PromotionVideo.Play();
        }

        private void StartOrderButton_Click(object sender, RoutedEventArgs e)
        {
            CustomerModeLandingPage.Visibility = Visibility.Hidden;
            OrderModePage.Visibility = Visibility.Visible;
            if (PromotionVideo.Source != null) PromotionVideo.Pause();
        }

        private void CloseVariantButton_Click(object sender, RoutedEventArgs e)
        {
            ProductVariantsPanel.Visibility = Visibility.Hidden;
            OrderingInterface.Visibility = Visibility.Visible;
            LowerCartInterface.Visibility = Visibility.Visible;
            VariantMode = false;
        }

        private void SaveOrder(int orderNumber)
        {
            string tableNumber = "-1";
            if (!string.IsNullOrEmpty(CustomerTableNumber.Text)) tableNumber = CustomerTableNumber.Text;
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string filename = DateTime.Now.ToString("yyyyMMdd_HHmmss_fffffff");
            string folderPath = System.IO.Path.Combine(basePath, "Orders");
            string filePath = System.IO.Path.Combine(folderPath, string.Format($"Order{orderNumber}_{filename}.json"));

            if (!string.IsNullOrEmpty(CurrentOrderFolderPath))
            {
                basePath = CurrentOrderFolderPath; // From the folder browser
                filePath = System.IO.Path.Combine(basePath, string.Format($"Order{orderNumber}_{filename}.json"));
            }

            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
            Order newOrder = new Order(orderNumber, CurrentOrderMode, CurrentClaimMode, Convert.ToInt32(tableNumber), ProductCart);
            string json = JsonSerializer.Serialize(newOrder, options);
            File.WriteAllText(filePath, json);
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
                using (MySqlCommand command = new MySqlCommand($"SELECT salt, hash FROM admins WHERE admin_username = @username", connect))
                {
                    command.Parameters.AddWithValue("@username", AdminUsername.Text);
                    bool found = false;
                    string cur_salt = "", cur_hash = "";
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cur_salt = reader[0]?.ToString();
                            cur_hash = reader[1]?.ToString();
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
                        LoginAdmin.Visibility = Visibility.Hidden;
                        AdminDashboard.Visibility = Visibility.Visible;
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
                AdminDashboard.Visibility = Visibility.Hidden;
                OrderModePage.Visibility = Visibility.Visible;
                MessageBox.Show("You have successfully logged out.", "Logged out successfully", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            OrderModePage.Visibility = Visibility.Hidden;
            LoginAdmin.Visibility = Visibility.Visible;
        }

        private void CloseLoginButton_Click(object sender, RoutedEventArgs e)
        {
            OrderModePage.Visibility = Visibility.Visible;
            LoginAdmin.Visibility = Visibility.Hidden;
        }

        private void ShowAdminPanel_Click(object sender, RoutedEventArgs e)
        {
            if (AdminPanelShown)
            {
                AdminFunctions.Visibility = Visibility.Hidden;
                AdminPanelShown = false;
            }
            else
            {
                AdminFunctions.Visibility = Visibility.Visible;
                AdminPanelShown = true;
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

        private void LoadBackground_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp" };
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == true)
            {
                var bitmap = new BitmapImage(new Uri(dlg.FileName));
                CurrentBackgroundLocation = dlg.FileName;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.Freeze();

                int width = bitmap.PixelWidth;
                int height = bitmap.PixelHeight;
                if (width < 768 || height < 1024)
                {
                    MessageBox.Show("The selected image has too low resolution, please select another image.", "Too low image resolution", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                BackgroundImage.Source = bitmap;

                using (var ms = new MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    encoder.Save(ms);
                }
            }
        }

        private void RemoveBackground_Click(object sender, RoutedEventArgs e)
        {
            BackgroundImage.Source = null;
            CurrentBackgroundLocation = "";
        }

        private void ApplyBackgroundImage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CurrentBackgroundLocation)) return;
            MessageBoxResult result = MessageBox.Show("This operation will alter the background image, do you want to continue?", "Alter background image confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.No) return;
            SaveBackgroundLocation();
            ApplyBackground();
            BackgroundImage.Source = null;
        }

        private void SaveBackgroundLocation()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(basePath, "background_location.json");
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            string json = JsonSerializer.Serialize(CurrentBackgroundLocation, options);
            File.WriteAllText(filePath, json);
        }

        private void ApplyBackground()
        {
            if (File.Exists(CurrentBackgroundLocation))
            {
                try
                {
                    BitmapImage bg = new BitmapImage();
                    bg.BeginInit();
                    bg.CacheOption = BitmapCacheOption.OnLoad;
                    bg.UriSource = new Uri(Path.GetFullPath(CurrentBackgroundLocation));
                    bg.EndInit();
                    bg.Freeze();
                    WindowBackground.ImageSource = bg;
                }
                catch (NotSupportedException)
                {
                    WindowBackground.ImageSource = null;
                    OrderingWindow.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                }
                catch (COMException)
                {
                    WindowBackground.ImageSource = null;
                    OrderingWindow.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                }
            }
            else
            {
                WindowBackground.ImageSource = null;
                OrderingWindow.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            }
        }

        private void ChangePromotionButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Image and Video Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.mp4;*.wmv;*.avi;*.mov|" + "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|" + "Video Files|*.mp4;*.wmv;*.avi;*.mov" };
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == true)
            {
                CurrentPromotionFileLocation = dlg.FileName;
                PromotionFileLocation.Text = CurrentPromotionFileLocation;
            }
        }

        private void SavePromotionButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CurrentPromotionFileLocation)) return;
            SavePromotionFilePath();
            ReloadPromotionDisplay(CurrentPromotionFileLocation);
            MessageBox.Show("The selected promotional file has been saved.", "Promotional file saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SavePromotionFilePath()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = Path.Combine(basePath, "promotionalfile_location.json");
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            string json = JsonSerializer.Serialize(CurrentPromotionFileLocation, options);
            File.WriteAllText(filePath, json);
        }

        private void ChangeFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "Select Order Folder"
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string selectedPath = dialog.FileName;
                OrderFolderLocation.Text = selectedPath;
                CurrentOrderFolderPath = selectedPath;
            }
        }

        private void SaveFolderButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CurrentOrderFolderPath)) return;
            SaveOrderFolderPath();
            MessageBox.Show("The folder location has been saved.", "Saved folder location", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SaveOrderFolderPath()
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string filePath = System.IO.Path.Combine(basePath, "orderingfolder_location.json");
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            string json = JsonSerializer.Serialize(CurrentOrderFolderPath, options);
            File.WriteAllText(filePath, json);
        }

        private void PreviewImagesButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(CurrentBackgroundLocation))
            {
				try 
				{
					BitmapImage bg = new BitmapImage();
					bg.BeginInit();
					bg.CacheOption = BitmapCacheOption.OnLoad;
					bg.UriSource = new Uri(Path.GetFullPath(CurrentBackgroundLocation));
					bg.EndInit();
					bg.Freeze();
					BackgroundImage.Source = bg;
				}
				catch (NotSupportedException)
                {
					BackgroundImage.Source = null;
                }
                catch (COMException)
                {
                    BackgroundImage.Source = null;
                }
            }
        }

        private void InitializeOrderNumber()
        {
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT GET_LOCK('order_number', 15)", connect, transaction))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                            command.CommandText = "SELECT COUNT(*) FROM order_number LIMIT 2";
                            int result = Convert.ToInt32(command.ExecuteScalar());
                            if (result == 0)
                            {
                                command.CommandText = "INSERT INTO order_number VALUES(1)";
                                command.ExecuteNonQuery();
                            }
                            else if (result > 1)
                            {
                                command.CommandText = "DELETE FROM order_number";
                                command.ExecuteNonQuery();
                                command.CommandText = "INSERT INTO order_number VALUES(1)";
                                command.ExecuteNonQuery();
                            }
                            command.CommandText = "SELECT RELEASE_LOCK('order_number')";
                            command.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        catch (MySqlException)
                        {
                            command.CommandText = "SELECT RELEASE_LOCK('order_number')";
                            command.ExecuteNonQuery();
                            transaction.Rollback();
                        }
                    }
                }
            }
        }

        private int RetrieveIncrementOrderNumber()
        {
            int newOrder = 0, oldOrder = 0;
            using (var connect = new MySqlConnection(cs))
            {
                connect.Open();
                using (MySqlTransaction transaction = connect.BeginTransaction())
                {
                    using (MySqlCommand command = new MySqlCommand("SELECT GET_LOCK('order_number', 15)", connect, transaction))
                    {
                        try
                        {
                            command.ExecuteNonQuery();
                            command.CommandText = "SELECT value FROM order_number LIMIT 1";
                            newOrder = Convert.ToInt32(command.ExecuteScalar());
                            oldOrder = newOrder;
                            newOrder++;
                            command.CommandText = "UPDATE order_number SET value = @newVal LIMIT 1";
                            command.Parameters.AddWithValue("@newVal", newOrder);
                            command.ExecuteNonQuery();

                            command.CommandText = "SELECT RELEASE_LOCK('order_number')";
                            command.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        catch (MySqlException)
                        {
                            command.CommandText = "SELECT RELEASE_LOCK('order_number')";
                            command.ExecuteNonQuery();
                            transaction.Rollback();
                        }
                    }
                }
            }
            return oldOrder;
        }
    }
}

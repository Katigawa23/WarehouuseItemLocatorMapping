using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;


namespace WarehouuseItemLocatorMapping
{
    public partial class Form2 : Form
    {
        private Dictionary<string, Button> locationBtn = new Dictionary<string, Button>();
        private Dictionary<string, WareHouseItem> storedItems = new Dictionary<string, WareHouseItem>();
        private string selectedLocationCode = null;
        string connectionString = "server=localhost;userid=root;password=;database=warehousedb;";

        public Form2()
        {
            InitializeComponent();
            GenerateGrid();
            
            itemNameSearch.TextChanged += searchBox_TextChanged;
            searchBtn.Click += searchBtn_Click;
            this.Click += Form_ClickOutside;
            panelGrid.Click += Form_ClickOutside;
            this.Load += Form2_Load;

        }

        private void Form_ClickOutside(object sender, EventArgs e)
        {
            if (selectedLocationCode != null && storedItems.ContainsKey(selectedLocationCode))
            {
                locationBtn[selectedLocationCode].BackColor = Color.Green;
            }

            selectedLocationCode = null;
        }

        private void GenerateGrid()
        {
            int rows = 10;
            int cols = 10;
            int cellSize = 60;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    string locCode = $"{(char)('A' + row)}{col + 1}";

                    Button btn = new Button
                    {
                        Width = cellSize,
                        Height = cellSize,
                        Left = col * (cellSize + 5),
                        Top = row * (cellSize + 5),
                        Text = locCode,
                        BackColor = Color.LightGray,
                        Font = new Font("Segoe UI", 8, FontStyle.Regular)
                    };


                    panelGrid.Controls.Add(btn);
                    locationBtn[locCode] = btn;



                }
            }
        }
        private void PopulateLocationBox()
        {
            locationBox.Items.Clear();
            foreach (string code in locationBtn.Keys)
            {
                locationBox.Items.Add(code);
            }

            if (locationBox.Items.Count > 0)
                locationBox.SelectedIndex = 0;
        }



        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        public class WareHouseItem
        {
            private string itemName;
            private string locationCode;

            public string ItemName
            {
                get { return itemName; }
                set
                {
                    if (!string.IsNullOrWhiteSpace(value))
                        itemName = value;
                    else
                        throw new ArgumentException("Item name cannot be empty");
                }
            }

            public string LocationCode
            {
                get { return locationCode; }
                set
                {
                    if (!string.IsNullOrWhiteSpace(value))
                        locationCode = value.ToUpper();
                    else
                        throw new ArgumentException("Location code cannot be empty");
                }
            }

            public WareHouseItem(string itemName, string locationCode)
            {
                ItemName = itemName;
                LocationCode = locationCode;

            }

            public string GetDetails()
            {
                return $"Item: {ItemName} is located at {LocationCode}";
            }
        }

        private void addItemBtn_Click(object sender, EventArgs e)
        {
            try
            {
                string name = itemNameAdd.Text.Trim();
                string loc = locationBox.SelectedItem?.ToString();

                if (storedItems.ContainsKey(loc))
                {
                    MessageBox.Show("This storage location already have an item.");
                    return;
                }

                bool itemExists = storedItems.Values.Any(i => i.ItemName.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (itemExists)
                {
                    MessageBox.Show("This item name already exist");
                    return;
                }


                WareHouseItem item = new WareHouseItem(name, loc);
                MessageBox.Show(item.GetDetails());

                if (locationBtn.ContainsKey(item.LocationCode))
                {
                    Button locBtn = locationBtn[item.LocationCode];
                    locBtn.BackColor = Color.Green;
                    locBtn.Text = item.LocationCode + "\n" + item.ItemName;
                    locBtn.Font = new Font("Segoe UI", 7);
                    locBtn.Click -= Cell_Click;
                    locBtn.Click += Cell_Click;

                    storedItems[item.LocationCode] = item;

                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string insertQuery = "INSERT INTO WarehouseItems (ItemName, LocationCode) VALUES (@itemName, @locationCode)";
                        using (MySqlCommand cmd = new MySqlCommand(insertQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@itemName", name);
                            cmd.Parameters.AddWithValue("@locationCode", loc);
                            cmd.ExecuteNonQuery();
                        }
                    }

                }

                itemNameAdd.Clear();
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error:" + ex.Message);
            }
        }

        private void Cell_Click(object sender, EventArgs e)
        {
            Button clickedBtn = sender as Button;
            string locCode = clickedBtn.Text.Split('\n')[0];


            foreach (var kvp in storedItems)
            {
                if (locationBtn.ContainsKey(kvp.Key))
                {
                    locationBtn[kvp.Key].BackColor = Color.Green;
                }
            }

            selectedLocationCode = locCode;
            clickedBtn.BackColor = Color.Orange;
        }


        private void RemoveItemFromCell(string locationCode)
        {
            if (locationBtn.ContainsKey(locationCode))
            {
                Button btn = locationBtn[locationCode];
                btn.Text = locationCode;
                btn.BackColor = Color.LightGray;
                btn.Font = new Font("Segoe UI", 8, FontStyle.Regular);

                btn.Click -= Cell_Click;
                storedItems.Remove(locationCode);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(selectedLocationCode))
            {
                if (storedItems.ContainsKey(selectedLocationCode))
                {
                    string itemName = storedItems[selectedLocationCode].ItemName;

                    DialogResult result = MessageBox.Show(
                        $"Are you sure you want to delete the item \"{itemName}\" from location {selectedLocationCode}?",
                        "Confirm Deletion",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (result == DialogResult.Yes)
                    {
                        using (MySqlConnection conn = new MySqlConnection(connectionString))
                        {
                            conn.Open();
                            string deleteQuery = "DELETE FROM WarehouseItems WHERE LocationCode = @locationCode AND ItemName = @itemName";
                            using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@locationCode", selectedLocationCode);
                                cmd.Parameters.AddWithValue("@itemName", itemName);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        RemoveItemFromCell(selectedLocationCode);
                        selectedLocationCode = null;
                    }
                }
            }
            else
            {
                MessageBox.Show("Please click a cell first.");
            }
        }

        private void searchBtn_Click(object sender, EventArgs e)
        {
            string query = itemNameSearch.Text.Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(query))
            {
                MessageBox.Show("Please enter item name to search.");


                foreach (var kvp in locationBtn)
                {
                    if (storedItems.ContainsKey(kvp.Key))
                    {
                        kvp.Value.BackColor = Color.Green;
                    }
                    else
                    {
                        kvp.Value.BackColor = Color.LightGray;
                    }
                }

                return;
            }

            bool found = false;

            foreach (var kvp in locationBtn)
            {
                string code = kvp.Key;
                Button btn = kvp.Value;

                if (storedItems.ContainsKey(code) && storedItems[code].ItemName.ToUpper().Contains(query))
                {
                    btn.BackColor = Color.Yellow;
                    panelGrid.ScrollControlIntoView(btn);
                    found = true;
                }
                else if (storedItems.ContainsKey(code))
                {
                    btn.BackColor = Color.Green;
                }
                else
                {
                    btn.BackColor = Color.LightGray;
                }
            }

            if (!found)
            {
                MessageBox.Show("Item not found.");
            }
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            string query = itemNameSearch.Text.Trim().ToUpper();

            foreach (var kvp in locationBtn)
            {
                string code = kvp.Key;
                Button btn = kvp.Value;

                if (string.IsNullOrWhiteSpace(query))
                {

                    if (storedItems.ContainsKey(code))
                    {
                        btn.BackColor = Color.Green;
                    }
                    else
                    {
                        btn.BackColor = Color.LightGray;
                    }
                }
                else
                {

                    if (storedItems.ContainsKey(code) &&
                        storedItems[code].ItemName.ToUpper().Contains(query))
                    {
                        btn.BackColor = Color.Yellow;
                    }
                    else if (storedItems.ContainsKey(code))
                    {
                        btn.BackColor = Color.Green;
                    }
                    else
                    {
                        btn.BackColor = Color.LightGray;
                    }
                }
            }
        }
        private void LoadItemsFromDatabase()
        {
            string query = "SELECT * FROM WarehouseItems";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string itemName = reader["ItemName"].ToString();
                    string locationCode = reader["LocationCode"].ToString();

                    // Create a new WarehouseItem from the database data
                    WareHouseItem item = new WareHouseItem(itemName, locationCode);

                    if (locationBtn.ContainsKey(item.LocationCode))
                    {
                        Button locBtn = locationBtn[item.LocationCode];
                        locBtn.BackColor = Color.Green;
                        locBtn.Text = item.LocationCode + "\n" + item.ItemName;
                        locBtn.Font = new Font("Segoe UI", 7);

                        locBtn.Click -= Cell_Click;
                        locBtn.Click += Cell_Click;

                        storedItems[item.LocationCode] = item;
                    }
                }
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
           
            PopulateLocationBox();
            LoadItemsFromDatabase(); 

        }

        private void logoutBtn_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to logout?",
       "Confirm Logout",
       MessageBoxButtons.YesNo,
       MessageBoxIcon.Question
   );

            if (result == DialogResult.Yes)
            {
                Form1 f1 = new Form1();
                f1.Show();
                this.Close();
            }
        }

        private void historyBtn_Click(object sender, EventArgs e)
        {
            Form3 f3 = new Form3();
            f3.ShowDialog();
        }
    }
}

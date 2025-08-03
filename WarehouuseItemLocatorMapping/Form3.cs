using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

namespace WarehouuseItemLocatorMapping
{
    public partial class Form3 : Form
    {
        private string connectionString = "server=localhost;userid=root;password=;database=warehousedb;";

        public Form3()
        {
            InitializeComponent();
            LoadHistory();
        }

        private void LoadHistory()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                string query = "SELECT ItemName, LocationCode FROM WarehouseItems";

                try
                {
                    conn.Open();
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    historyGrid.DataSource = table;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading history: " + ex.Message);
                }
            }
        }

        private void closeBtn_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2();
            f2.Show();
            this.Hide();
        }


    }
}

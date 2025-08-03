using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WarehouuseItemLocatorMapping
{
    public partial class Form1: Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string enteredUsername = userName.Text.Trim();
            string enteredPassword = password.Text;

            if (enteredUsername == "admin" && enteredPassword == "admin123")
            {
                // Login successful - open Form2 (main application)
                Form2 f2 = new Form2();
                f2.Show();
                this.Hide(); // or use this.Close(); if you want to close the login form
            }
            else
            {
                // Login failed - show error 
                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void password_TextChanged(object sender, EventArgs e)
        {
            password.PasswordChar = '*';

        }
    }
}

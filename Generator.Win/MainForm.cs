using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Generator.Win
{
    public partial class MainForm : Form
    {
        private int _ErrorId = 0;

        #region "Events"
            private void ConnectionButton_Click(object sender, EventArgs e)
            {
                ConnectDatabase();
            }

            private void DisconnectButton_Click(object sender, EventArgs e)
            {
                DisconnectDatabase();
            }

            private void FileExitMenu_Click(object sender, EventArgs e)
            {
                Application.Exit();
            }

            public MainForm()
            {
                InitializeComponent();
            }
        #endregion

        #region "Methods"
            private void ConnectDatabase()
            {
                OpenConnection();
                DisableControls();
            }

            private void DisableControls()
            {
                ServerBox.Enabled = false;
                UserBox.Enabled = false;
                PasswordBox.Enabled = false;
                ConnectButton.Enabled = false;
                DisconnectButton.Enabled = true;
                DatabaseCombo.Enabled = true;
            }

            private void DisconnectDatabase()
            {
                ServerBox.Enabled = true;
                UserBox.Enabled = true;
                PasswordBox.Enabled = true;
                ConnectButton.Enabled = true;
                DisconnectButton.Enabled = false;
                DatabaseCombo.Enabled = false;
            }

            public void GetTables(string Provider)
            {
                DataTable Tables;

                using (SqlConnection SqlServer = new SqlConnection(Provider))
                {
                    //SqlServer.Open();

                    //Tables = SqlServer.GetSchema("Tables");

                    //SqlServer.Close();

                    //if (Tables.Rows.Count > 0)
                    //{
                    //    // Muestra las tablas de la base de datos seleccionada en el combo
                    //    Tables.DataSource = Tables;
                    //    Tables.DisplayMember = "TABLE_NAME";
                    //    Tables.ValueMember = "TABLE_NAME";
                    //}
                    //else
                    //{
                    //    Tables.DataSource = null;
                    //}
                }
            }

            private void OpenConnection()
            {
                string Provider = "";
                DataTable Databases;

                Provider = "Integrated Security=False; Persist Security Info=False; Server=" + ServerBox.Text.Trim() + "; ";
                Provider += "User=" + UserBox.Text.Trim() + "; ";
                Provider += "Password=" + PasswordBox.Text.Trim() + "; ";

                using (SqlConnection SqlServer = new SqlConnection(Provider))
                {
                    SqlServer.Open();

                    Databases = SqlServer.GetSchema("Databases");

                    SqlServer.Close();

                    if (Databases.Rows.Count > 0)
                    {
                        // Muestra las bases de datos del servidor en un comobobox
                        DatabaseCombo.DataSource = Databases;
                        DatabaseCombo.DisplayMember = "database_name";
                        DatabaseCombo.ValueMember = "database_name";

                        Provider = Provider + "Initial Catalog=" + Databases.Rows[0]["database_name"].ToString() + "; ";

                        GetTables(Provider);
                    }
                    else
                    {
                        DatabaseCombo.DataSource = null;
                    }
                }
            }
        #endregion
    }
}

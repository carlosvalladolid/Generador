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

            private void CloseButton_Click(object sender, EventArgs e)
            {
                Application.Exit();
            }

            private void CreateButton_Click(object sender, EventArgs e)
            {
                CreateProcedures();
            }

            private void DatabaseCombo_SelectedIndexChanged(object sender, EventArgs e)
            {
                DatabaseComboSelectedIndexChanged();
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

            private void CreateSelect()
            {
                StringBuilder Query = new StringBuilder();
                DataTable dtSchema;
                SqlDataReader drColumns;
                SqlConnection SqlServer = new SqlConnection(GetProviderString());
                SqlCommand Command = new SqlCommand("", SqlServer);

                SqlServer.Open();

                drColumns = Command.ExecuteReader(CommandBehavior.CloseConnection);

                dtSchema = drColumns.GetSchemaTable();

                SqlServer.Close();

                drColumns.Close();

                foreach (DataRow dr in dtSchema.Rows)
                {
                    MessageBox.Show(dr["DataType"].ToString());
                }

                Query.Append("");
            }

            private void CreateProcedures()
            {
                DataGridViewCheckBoxCell CheckColumn;

                // Se barre el grid para tomar las tablas que se utilizarán para generar los SPs
                foreach (DataGridViewRow Row in TableGrid.Rows)
                {
                    CheckColumn = (DataGridViewCheckBoxCell)Row.Cells["CheckColumn"];

                    //if (CheckColumn.Selected)
                    //    MessageBox.Show("Bingo!");

                    // Generar el script para la instrucción SELECT
                    //if (SelectCheck.Checked)
                    //    CreateSelect();
                }
            }

            private void DatabaseComboSelectedIndexChanged()
            {
                GetTables(GetProviderString());
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

            private string GetProviderString()
            {
                string ProviderString = string.Empty;

                ProviderString = "Integrated Security=False; Persist Security Info=False; Server=" + ServerBox.Text.Trim() + "; ";
                ProviderString += "User=" + UserBox.Text.Trim() + "; ";
                ProviderString += "Password=" + PasswordBox.Text.Trim() + "; ";

                if (DatabaseCombo.SelectedIndex > 0)
                    ProviderString += "Initial Catalog=" + DatabaseCombo.SelectedValue.ToString() + "; ";

                return ProviderString;
            }

            private void GetTables(string ProviderString)
            {
                DataTable Tables;

                using (SqlConnection SqlServer = new SqlConnection(ProviderString))
                {
                    SqlServer.Open();

                    Tables = SqlServer.GetSchema("Tables");

                    SqlServer.Close();

                    if (Tables.Rows.Count > 0)
                    {
                        // Muestra las tablas de la base de datos seleccionada en el grid
                        TableGrid.DataSource = Tables;
                    }
                    else
                    {
                        TableGrid.DataSource = null;
                    }
                }
            }

            private void OpenConnection()
            {
                string ProviderString = "";
                DataTable Databases;

                ProviderString = GetProviderString();

                using (SqlConnection SqlServer = new SqlConnection(ProviderString))
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

                        ProviderString = ProviderString + "Initial Catalog=" + Databases.Rows[0]["database_name"].ToString() + "; ";

                        GetTables(ProviderString);
                    }
                    else
                        DatabaseCombo.DataSource = null;
                }
            }
        #endregion
    }
}

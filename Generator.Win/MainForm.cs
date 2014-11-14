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

            private void CreateDeleteProcedure(string TableName)
            {
                // Nothing yet
            }

            private void CreateInsertProcedure(string TableName)
            {
                // Nothing yet
            }

            private void CreateProcedures()
            {
                // Se barre el grid para tomar las tablas que se utilizarán para generar los SPs
                foreach (DataGridViewRow Row in TableGrid.Rows)
                {
                    // Si la celda trae null, nos pasamos al siguiente renglón
                    if (Row.Cells["CheckColumn"].Value == null)
                        continue;

                    // Si no está seleccionada la casilla, nos pasamos al siguiente renglón
                    if (!(bool)Row.Cells["CheckColumn"].Value)
                        continue;

                    if(SelectCheck.Checked)
                        CreateSelectProcedure(Row.Cells["TableColumn"].Value.ToString());

                    if(InsertCheck.Checked)
                        CreateInsertProcedure(Row.Cells["TableColumn"].Value.ToString());

                    if(UpdateCheck.Checked)
                        CreateUpdateProcedure(Row.Cells["TableColumn"].Value.ToString());

                    if(DeleteCheck.Checked)
                        CreateDeleteProcedure(Row.Cells["TableColumn"].Value.ToString());
                }
            }

            private void CreateSelectProcedure(string TableName)
            {
                bool IsFirstParameter = true;
                //string[] KeyArray = new string[];
                StringBuilder ScriptQuery = new StringBuilder();
                DataTable dtSchema;
                SqlDataReader drColumns;
                SqlConnection SqlServer = new SqlConnection(GetProviderString());
                SqlCommand Command = new SqlCommand("SELECT * FROM " + TableName, SqlServer);

                SqlServer.Open();

                drColumns = Command.ExecuteReader(CommandBehavior.KeyInfo);

                dtSchema = drColumns.GetSchemaTable();

                SqlServer.Close();
                drColumns.Close();

                ScriptQuery.Append("/*******************************************************************************************");
                ScriptQuery.Append("* NOMBRE			    Select");
                ScriptQuery.Append(TableName);
                ScriptQuery.Append("\r");
                ScriptQuery.Append("* AUTOR			    Code Generator Beta 1.0.0");
                ScriptQuery.Append("\r");
                ScriptQuery.Append("* DESCRIPCIÓN 		Busca información de la tabla ");
                ScriptQuery.Append(TableName);
                ScriptQuery.Append("\r");
                ScriptQuery.Append("*");
                ScriptQuery.Append("PARÁMETROS            {0}");
                ScriptQuery.Append("\r");
                ScriptQuery.Append("*");
                ScriptQuery.Append("*********************************************************************************************/");
                ScriptQuery.Append("\r");
                ScriptQuery.Append("CREATE PROCEDURE [dbo].Select");
                ScriptQuery.Append(TableName);
                ScriptQuery.Append("\r");
                ScriptQuery.Append("(");
                ScriptQuery.Append("\r");

                // Se extraen los parámetros
                foreach (DataRow dr in dtSchema.Rows)
                {
                    if ((bool)dr["IsKey"])
                    {
                        if (!IsFirstParameter)
                            ScriptQuery.Append(",\r");
                        else
                            IsFirstParameter = false;

                        ScriptQuery.Append("@");
                        ScriptQuery.Append(dr["ColumnName"]);
                        ScriptQuery.Append(" ");
                        ScriptQuery.Append(dr["DataTypeName"]);
                    }
                }

                ScriptQuery.Append("\r");
                ScriptQuery.Append(")");
                ScriptQuery.Append("\r\r");
                ScriptQuery.Append("AS");
                ScriptQuery.Append("\r\r");
                ScriptQuery.Append("SET NOCOUNT ON");
                ScriptQuery.Append("\r\r");

                ScriptQuery.Append("SELECT ");

                IsFirstParameter = true;

                // Se arma la consulta
                foreach (DataRow dr in dtSchema.Rows)
                {
                    if (!IsFirstParameter)
                        ScriptQuery.Append(", ");
                    else
                        IsFirstParameter = false;

                    ScriptQuery.Append(dr["ColumnName"]);
                }

                ScriptQuery.Append("\r");
                ScriptQuery.Append("FROM ");
                ScriptQuery.Append(TableName);
                ScriptQuery.Append("\r");

                // Cláusula WHERE
                ScriptQuery.Append("WHERE (");



                ScriptQuery.Append("\r\r");
                ScriptQuery.Append("SET NOCOUNT OFF");

                MessageBox.Show(ScriptQuery.ToString());
            }

            private void CreateUpdateProcedure(string TableName)
            {
                // Nothing yet
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

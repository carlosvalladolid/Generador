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

using Generator.Win.Class;

namespace Generator.Win
{
    public partial class MainForm : Form
    {
        private int _ErrorId = 0;
        private string _ErrorDescription = string.Empty;

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

            private void HelpAboutMenu_Click(object sender, EventArgs e)
            {
                MessageBox.Show("Generador v1.0", "Acerca de Generador");
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
                bool IsFirstParameter = true;
                int Count = 0;
                List<string> Parameter = new List<string>();
                StringBuilder ScriptQuery = new StringBuilder();
                DataTable dtSchema;
                SqlDataReader drColumns;
                SqlConnection SqlServer = new SqlConnection(GetProviderString());
                SqlCommand Command = new SqlCommand("SELECT * FROM [" + TableName + "]", SqlServer);

                SqlServer.Open();

                drColumns = Command.ExecuteReader(CommandBehavior.KeyInfo);

                dtSchema = drColumns.GetSchemaTable();

                SqlServer.Close();
                drColumns.Close();

                ScriptQuery.Append("/*******************************************************************************************");
                ScriptQuery.Append("\r");
                ScriptQuery.Append("* NOMBRE			    Delete");
                ScriptQuery.Append(TableName);
                ScriptQuery.Append("\r");
                ScriptQuery.Append("* AUTOR			        Code Generator Beta 1.0.0");
                ScriptQuery.Append("\r");
                ScriptQuery.Append("* DESCRIPCIÓN 		    Borra información de la tabla ");
                ScriptQuery.Append(TableName);
                ScriptQuery.Append("\r");
                ScriptQuery.Append("*");
                ScriptQuery.Append(" PARÁMETROS             ");
                ScriptQuery.Append("\r");
                ScriptQuery.Append("*");
                ScriptQuery.Append("*********************************************************************************************/");
                ScriptQuery.Append("\r");
                ScriptQuery.Append("CREATE PROCEDURE [dbo].Delete");
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

                        ScriptQuery.Append("    @");
                        ScriptQuery.Append(dr["ColumnName"].ToString());
                        ScriptQuery.Append(" ");
                        ScriptQuery.Append(dr["DataTypeName"].ToString());

                        Parameter.Add(dr["ColumnName"].ToString());
                    }
                }

                ScriptQuery.Append("\r");
                ScriptQuery.Append(")");
                ScriptQuery.Append("\r\r");
                ScriptQuery.Append("AS");
                ScriptQuery.Append("\r\r");
                ScriptQuery.Append("SET NOCOUNT ON");
                ScriptQuery.Append("\r\r");

                ScriptQuery.Append("    DELETE ");
                ScriptQuery.Append("[");
                ScriptQuery.Append(TableName);
                ScriptQuery.Append("]");
                ScriptQuery.Append("\r");

                // Cláusula WHERE
                ScriptQuery.Append("        WHERE (");

                IsFirstParameter = true;

                for (Count = 0; Count < Parameter.Count; Count++)
                {
                    if (IsFirstParameter)
                        IsFirstParameter = false;
                    else
                    {
                        ScriptQuery.Append("\r");
                        ScriptQuery.Append("AND (");
                    }

                    ScriptQuery.Append("[");
                    ScriptQuery.Append(Parameter[Count]);
                    ScriptQuery.Append("]");
                    ScriptQuery.Append(" = @");
                    ScriptQuery.Append(Parameter[Count]);
                    ScriptQuery.Append(")");

                    // ToDo: Poner la condición del parámetro vacío
                    //ScriptQuery.Append("OR @");
                    //ScriptQuery.Append(Parameter[Count]);
                }

                ScriptQuery.Append("\r\r");
                ScriptQuery.Append("SET NOCOUNT OFF");
                ScriptQuery.Append("\r");

                ExecuteQuery(ScriptQuery.ToString());
            }

            private void CreateInsertProcedure(string TableName)
            {
                bool IsFirstParameter = true;
                StringBuilder ScriptQuery = new StringBuilder();
                DataTable dtSchema;
                SqlDataReader drColumns;
                SqlConnection SqlServer = new SqlConnection(GetProviderString());
                SqlCommand Command = new SqlCommand("SELECT * FROM [" + TableName + "]", SqlServer);

                SqlServer.Open();

                drColumns = Command.ExecuteReader(CommandBehavior.KeyInfo);

                dtSchema = drColumns.GetSchemaTable();

                SqlServer.Close();
                drColumns.Close();

                ScriptQuery.Append("/*******************************************************************************************");
                ScriptQuery.Append("\r");
                ScriptQuery.Append("* NOMBRE			    Insert");
                ScriptQuery.Append(TableName);
                ScriptQuery.Append("\r");
                ScriptQuery.Append("* AUTOR			        Code Generator Beta 1.0.0");
                ScriptQuery.Append("\r");
                ScriptQuery.Append("* DESCRIPCIÓN 		    Guarda un registro nuevo en la tabla ");
                ScriptQuery.Append(TableName);
                ScriptQuery.Append(" \r");
                ScriptQuery.Append("*");
                ScriptQuery.Append(" PARÁMETROS             ");
                ScriptQuery.Append("\r");
                ScriptQuery.Append("*");
                ScriptQuery.Append("*********************************************************************************************/");
                ScriptQuery.Append("\r");
                ScriptQuery.Append("CREATE PROCEDURE [dbo].Insert");
                ScriptQuery.Append(TableName);
                ScriptQuery.Append("\r");
                ScriptQuery.Append("(");
                ScriptQuery.Append("\r");

                // Se extraen los parámetros
                foreach (DataRow dr in dtSchema.Rows)
                {
                    if (!IsFirstParameter)
                        ScriptQuery.Append(",\r");
                    else
                        IsFirstParameter = false;

                    ScriptQuery.Append("    @");
                    ScriptQuery.Append(dr["ColumnName"].ToString());
                    ScriptQuery.Append(" ");
                    ScriptQuery.Append(dr["DataTypeName"].ToString());

                    if (dr["DataTypeName"].ToString().ToLower() == "varchar")
                    {
                        ScriptQuery.Append("(");
                        ScriptQuery.Append(dr["ColumnSize"].ToString());
                        ScriptQuery.Append(")");
                    }
                }

                ScriptQuery.Append("\r");
                ScriptQuery.Append(")");
                ScriptQuery.Append("\r\r");
                ScriptQuery.Append("AS");
                ScriptQuery.Append("\r\r");
                ScriptQuery.Append("SET NOCOUNT ON");
                ScriptQuery.Append("\r\r");

                ScriptQuery.Append("    INSERT INTO [");
                ScriptQuery.Append(TableName);
                ScriptQuery.Append("] (");

                IsFirstParameter = true;

                // Campos de la tabla
                foreach (DataRow dr in dtSchema.Rows)
                {
                    if (!IsFirstParameter)
                        ScriptQuery.Append(", ");
                    else
                        IsFirstParameter = false;

                    ScriptQuery.Append("[");
                    ScriptQuery.Append(dr["ColumnName"].ToString());
                    ScriptQuery.Append("]");
                }

                ScriptQuery.Append(")");
                ScriptQuery.Append("\r");
                ScriptQuery.Append("        VALUES(");

                IsFirstParameter = true;

                // Valores de la tabla
                foreach (DataRow dr in dtSchema.Rows)
                {
                    if (!IsFirstParameter)
                        ScriptQuery.Append(", ");
                    else
                        IsFirstParameter = false;

                    ScriptQuery.Append("@");
                    ScriptQuery.Append(dr["ColumnName"].ToString());
                }

                ScriptQuery.Append(")");
                ScriptQuery.Append("\r\r");
                ScriptQuery.Append("SET NOCOUNT OFF");
                ScriptQuery.Append("\r");

                ExecuteQuery(ScriptQuery.ToString());
            }

            private void CreateProcedures()
            {
                CreateButton.Enabled = false;
                StatusBar.Text = Constant.StatusStart;

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

                    if(InsertCheck.Checked && _ErrorId == 0)
                        CreateInsertProcedure(Row.Cells["TableColumn"].Value.ToString());

                    if (UpdateCheck.Checked && _ErrorId == 0)
                        CreateUpdateProcedure(Row.Cells["TableColumn"].Value.ToString());

                    if (DeleteCheck.Checked && _ErrorId == 0)
                        CreateDeleteProcedure(Row.Cells["TableColumn"].Value.ToString());
                }

                if (_ErrorDescription == "")
                    StatusBar.Text = Constant.StatusFinished;
                else
                    MessageBox.Show(_ErrorDescription);

                CreateButton.Enabled = true;
            }

            private void CreateSelectProcedure(string TableName)
            {
                bool IsFirstParameter = true;
                int Count = 0;
                List<string> Parameter = new List<string>();
                StringBuilder ScriptQuery = new StringBuilder();
                DataTable dtSchema;
                SqlDataReader drColumns;
                SqlConnection SqlServer = new SqlConnection(GetProviderString());
                SqlCommand Command = new SqlCommand("SELECT * FROM [" + TableName + "]", SqlServer);

                SqlServer.Open();

                drColumns = Command.ExecuteReader(CommandBehavior.KeyInfo);

                dtSchema = drColumns.GetSchemaTable();

                SqlServer.Close();
                drColumns.Close();

                ScriptQuery.Append("/*******************************************************************************************");
                ScriptQuery.Append("\r");
                ScriptQuery.Append("* NOMBRE			    Select");
                ScriptQuery.Append(TableName);
                ScriptQuery.Append("\r");
                ScriptQuery.Append("* AUTOR			        Code Generator Beta 1.0.0");
                ScriptQuery.Append("\r");
                ScriptQuery.Append("* DESCRIPCIÓN 		    Busca información de la tabla ");
                ScriptQuery.Append(TableName);
                ScriptQuery.Append("\r");
                ScriptQuery.Append("*");
                ScriptQuery.Append(" PARÁMETROS             ");
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

                        ScriptQuery.Append("    @");
                        ScriptQuery.Append(dr["ColumnName"].ToString());
                        ScriptQuery.Append(" ");
                        ScriptQuery.Append(dr["DataTypeName"].ToString());

                        Parameter.Add(dr["ColumnName"].ToString());
                    }
                }

                ScriptQuery.Append("\r");
                ScriptQuery.Append(")");
                ScriptQuery.Append("\r\r");
                ScriptQuery.Append("AS");
                ScriptQuery.Append("\r\r");
                ScriptQuery.Append("SET NOCOUNT ON");
                ScriptQuery.Append("\r\r");

                ScriptQuery.Append("    SELECT ");

                IsFirstParameter = true;

                // Se arma la consulta
                foreach (DataRow dr in dtSchema.Rows)
                {
                    if (!IsFirstParameter)
                        ScriptQuery.Append(", ");
                    else
                        IsFirstParameter = false;

                    ScriptQuery.Append("[");
                    ScriptQuery.Append(dr["ColumnName"].ToString());
                    ScriptQuery.Append("]");
                }

                ScriptQuery.Append("\r");
                ScriptQuery.Append("        FROM ");
                ScriptQuery.Append("[");
                ScriptQuery.Append(TableName);
                ScriptQuery.Append("]");
                ScriptQuery.Append("\r");

                // Cláusula WHERE
                ScriptQuery.Append("        WHERE (");

                IsFirstParameter = true;

                for (Count = 0; Count < Parameter.Count; Count++)
                {
                    if (IsFirstParameter)
                        IsFirstParameter = false;
                    else
                    {
                        ScriptQuery.Append("\r");
                        ScriptQuery.Append("AND (");
                    }

                    ScriptQuery.Append("[");
                    ScriptQuery.Append(Parameter[Count]);
                    ScriptQuery.Append("]");
                    ScriptQuery.Append(" = @");
                    ScriptQuery.Append(Parameter[Count]);
                    ScriptQuery.Append(")");

                    // ToDo: Poner la condición del parámetro vacío
                    //ScriptQuery.Append("OR @");
                    //ScriptQuery.Append(Parameter[Count]);
                }

                ScriptQuery.Append("\r\r");
                ScriptQuery.Append("SET NOCOUNT OFF");
                ScriptQuery.Append("\r");

                ExecuteQuery(ScriptQuery.ToString());
            }

            private void CreateUpdateProcedure(string TableName)
            {
                bool IsFirstParameter = true;
                int Count = 0;
                List<string> Parameter = new List<string>();
                StringBuilder ScriptQuery = new StringBuilder();
                DataTable dtSchema;
                SqlDataReader drColumns;
                SqlConnection SqlServer = new SqlConnection(GetProviderString());
                SqlCommand Command = new SqlCommand("SELECT * FROM [" + TableName + "]", SqlServer);

                SqlServer.Open();

                drColumns = Command.ExecuteReader(CommandBehavior.KeyInfo);

                dtSchema = drColumns.GetSchemaTable();

                SqlServer.Close();
                drColumns.Close();

                ScriptQuery.Append("/*******************************************************************************************");
                ScriptQuery.Append("\r");
                ScriptQuery.Append("* NOMBRE			    Update");
                ScriptQuery.Append(TableName);
                ScriptQuery.Append("\r");
                ScriptQuery.Append("* AUTOR			        Code Generator Beta 1.0.0");
                ScriptQuery.Append("\r");
                ScriptQuery.Append("* DESCRIPCIÓN 		    Actualiza uno o varios registros de la tabla ");
                ScriptQuery.Append(TableName);
                ScriptQuery.Append(" \r");
                ScriptQuery.Append("*");
                ScriptQuery.Append(" PARÁMETROS             ");
                ScriptQuery.Append("\r");
                ScriptQuery.Append("*");
                ScriptQuery.Append("*********************************************************************************************/");
                ScriptQuery.Append("\r");
                ScriptQuery.Append("CREATE PROCEDURE [dbo].Update");
                ScriptQuery.Append(TableName);
                ScriptQuery.Append("\r");
                ScriptQuery.Append("(");
                ScriptQuery.Append("\r");

                // Se extraen los parámetros
                foreach (DataRow dr in dtSchema.Rows)
                {
                    if (!IsFirstParameter)
                        ScriptQuery.Append(",\r");
                    else
                        IsFirstParameter = false;

                    ScriptQuery.Append("    @");
                    ScriptQuery.Append(dr["ColumnName"].ToString());
                    ScriptQuery.Append(" ");
                    ScriptQuery.Append(dr["DataTypeName"].ToString());

                    if (dr["DataTypeName"].ToString().ToLower() == "varchar")
                    {
                        ScriptQuery.Append("(");
                        ScriptQuery.Append(dr["ColumnSize"].ToString());
                        ScriptQuery.Append(")");
                    }

                    if ((bool)dr["IsKey"])
                        Parameter.Add(dr["ColumnName"].ToString());
                }

                ScriptQuery.Append("\r");
                ScriptQuery.Append(")");
                ScriptQuery.Append("\r\r");
                ScriptQuery.Append("AS");
                ScriptQuery.Append("\r\r");
                ScriptQuery.Append("SET NOCOUNT ON");
                ScriptQuery.Append("\r\r");

                ScriptQuery.Append("    UPDATE [");
                ScriptQuery.Append(TableName);
                ScriptQuery.Append("] SET ");

                IsFirstParameter = true;

                // Campos de la tabla
                foreach (DataRow dr in dtSchema.Rows)
                {
                    if (!IsFirstParameter)
                        ScriptQuery.Append(", ");
                    else
                        IsFirstParameter = false;

                    ScriptQuery.Append("[");
                    ScriptQuery.Append(dr["ColumnName"].ToString());
                    ScriptQuery.Append("] = @");
                    ScriptQuery.Append(dr["ColumnName"].ToString());
                }

                ScriptQuery.Append("\r");
                ScriptQuery.Append("        WHERE(");

                IsFirstParameter = true;

                for (Count = 0; Count < Parameter.Count; Count++)
                {
                    if (IsFirstParameter)
                        IsFirstParameter = false;
                    else
                    {
                        ScriptQuery.Append("\r");
                        ScriptQuery.Append("AND (");
                    }

                    ScriptQuery.Append("[");
                    ScriptQuery.Append(Parameter[Count]);
                    ScriptQuery.Append("]");
                    ScriptQuery.Append(" = @");
                    ScriptQuery.Append(Parameter[Count]);
                    ScriptQuery.Append(")");

                    // ToDo: Poner la condición del parámetro vacío
                    //ScriptQuery.Append("OR @");
                    //ScriptQuery.Append(Parameter[Count]);
                }

                ScriptQuery.Append("\r\r");
                ScriptQuery.Append("SET NOCOUNT OFF");
                ScriptQuery.Append("\r");

                ExecuteQuery(ScriptQuery.ToString());
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

            private void ExecuteQuery(string ScriptQuery)
            {
                SqlConnection Connection = new SqlConnection(GetProviderString());
                SqlCommand Command = new SqlCommand(ScriptQuery, Connection);

                try
                {
                    Command.CommandType = CommandType.Text;

                    Connection.Open();
                    Command.ExecuteNonQuery();
                    Connection.Close();
                }
                catch (SqlException Exception)
                {
                    _ErrorId = Exception.Number;
                    _ErrorDescription = Exception.Message;

                    if (Connection.State == ConnectionState.Open)
                        Connection.Close();
                }
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

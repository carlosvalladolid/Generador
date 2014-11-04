using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Generator.Win
{
    public partial class MainForm : Form
    {
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
        #endregion
    }
}

﻿// <copyright file="MainForm.cs" company="PublicDomain.is">
//     CC0 1.0 Universal (CC0 1.0) - Public Domain Dedication
//     https://creativecommons.org/publicdomain/zero/1.0/legalcode
// </copyright>

namespace ClipHoard
{
    // Directives
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Xml.Serialization;
    using Newtonsoft.Json;
    using PublicDomain;

    /// <summary>
    /// Description of MainForm.
    /// </summary>
    public partial class MainForm : Form
    {
        /// <summary>
        /// Gets or sets the associated icon.
        /// </summary>
        /// <value>The associated icon.</value>
        private Icon associatedIcon = null;

        /// <summary>
        /// The settings data.
        /// </summary>
        public SettingsData settingsData = null;

        /// <summary>
        /// The settings data path.
        /// </summary>
        private string settingsDataPath = $"{Application.ProductName}-SettingsData.txt";

        /// <summary>
        /// The data table.
        /// </summary>
        private DataTable dataTable = new DataTable();

        /// <summary>
        /// The popup form.
        /// </summary>
        private PopupForm popupForm = null;

        /// <summary>
        /// Registers the hot key.
        /// </summary>
        /// <returns><c>true</c>, if hot key was registered, <c>false</c> otherwise.</returns>
        /// <param name="hWnd">H window.</param>
        /// <param name="id">Identifier.</param>
        /// <param name="fsModifiers">Fs modifiers.</param>
        /// <param name="vk">Vk.</param>
        [DllImport("User32")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        /// <summary>
        /// Unregisters the hot key.
        /// </summary>
        /// <returns><c>true</c>, if hot key was unregistered, <c>false</c> otherwise.</returns>
        /// <param name="hWnd">H window.</param>
        /// <param name="id">Identifier.</param>
        [DllImport("User32")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        /// <summary>
        /// The mod shift.
        /// </summary>
        private const int MOD_SHIFT = 0x4;

        /// <summary>
        /// The mod control.
        /// </summary>
        private const int MOD_CONTROL = 0x2;

        /// <summary>
        /// The mod alternate.
        /// </summary>
        private const int MOD_ALT = 0x1;

        /// <summary>
        /// The wm hotkey.
        /// </summary>
        private static int WM_HOTKEY = 0x0312;

        /// <summary>
        /// Gets or sets the copied count tool strip status label text.
        /// </summary>
        /// <value>The copied count tool strip status label text.</value>
        public string CopiedCountToolStripStatusLabelText
        {
            get
            {
                return this.copiedCountToolStripStatusLabel.Text;
            }
            set
            {
                this.copiedCountToolStripStatusLabel.Text = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ClipHoard.MainForm"/> class.
        /// </summary>
        public MainForm()
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            this.InitializeComponent();

            // Add keys
            foreach (var key in Enum.GetValues(typeof(Keys)))
            {
                // Add to list box
                this.keyComboBox.Items.Add(key.ToString());
            }

            /* Set icons */

            // Set associated icon from exe file
            this.associatedIcon = Icon.ExtractAssociatedIcon(typeof(MainForm).GetTypeInfo().Assembly.Location);

            // Set public domain is tool strip menu item image
            this.freeReleasesPublicDomainisToolStripMenuItem.Image = this.associatedIcon.ToBitmap();

            /* Process setiings */

            // Check for settings file
            if (!File.Exists(this.settingsDataPath))
            {
                // Create new settings file
                this.SaveSettingsFile(this.settingsDataPath, new SettingsData());
            }

            // Load settings from disk
            this.settingsData = this.LoadSettingsFile(this.settingsDataPath);

            // The title data column
            DataColumn titleDataColumn = new DataColumn
            {
                ColumnName = "Title",
                DataType = typeof(string)
            };

            // The value data column
            DataColumn valueDataColumn = new DataColumn
            {
                ColumnName = "Value",
                DataType = typeof(string)
            };

            // TODO Add colums [Can be added by AddRange]
            this.dataTable.Columns.Add(titleDataColumn);
            this.dataTable.Columns.Add(valueDataColumn);

            // Dynamic row count event handlers
            this.mainDataGridView.RowsAdded += (sender, args) => UpdateRowCount();
            this.mainDataGridView.RowsRemoved += (sender, args) => UpdateRowCount();
        }

        /// <summary>
        /// Updates the row count.
        /// </summary>
        private void UpdateRowCount()
        {
            this.itemsCountToolStripStatusLabel.Text = this.mainDataGridView.RowCount.ToString();
        }

        /// <summary>
        /// Windows the proc.
        /// </summary>
        /// <param name="m">M.</param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_HOTKEY)
            {
                // Close previous
                if (this.popupForm != null)
                {
                    this.popupForm.Close();
                }

                // Set popup form
                this.popupForm = new PopupForm(this.dataTable, this.closePopupAfterSelectionToolStripMenuItem.Checked)
                {
                    // Set properties
                    TopMost = true,
                    Icon = this.Icon
                };

                // Set popup location
                if (this.openPopupOnCursorLocationToolStripMenuItem.Checked)
                {
                    this.popupForm.Location = Cursor.Position;
                }
                else
                {
                    this.popupForm.StartPosition = FormStartPosition.CenterScreen;
                }

                // Show popup
                this.popupForm.Show(this);
            }
        }

        /// <summary>
        /// Handles the hotkey updated event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnHotkeyUpdated(object sender, EventArgs e)
        {
            // Update the hotkey combination
            this.UpdateHotkey();
        }

        /// <summary>
        /// Updates the hotkey.
        /// </summary>
        private void UpdateHotkey()
        {
            // Only update if there's -at least- a valid key
            if ((this.keyComboBox.SelectedIndex > -1 && this.keyComboBox.SelectedItem.ToString().ToLowerInvariant() != "none"))
            {
                // Try to unregister the key
                try
                {
                    // Unregister the hotkey
                    UnregisterHotKey(this.Handle, 0);
                }
                catch
                {
                    // Let it fall through
                }

                // Try to register the key
                try
                {
                    // Register the hotkey
                    RegisterHotKey(this.Handle, 0, (this.controlCheckBox.Checked ? MOD_CONTROL : 0) + (this.altCheckBox.Checked ? MOD_ALT : 0) + (this.shiftCheckBox.Checked ? MOD_SHIFT : 0), Convert.ToInt16((Keys)Enum.Parse(typeof(Keys), this.keyComboBox.SelectedItem.ToString(), true)));
                }
                catch
                {
                    // Let it fall through
                }
            }
        }

        /// <summary>
        /// Handles the new tool strip menu item1 click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnNewToolStripMenuItem1Click(object sender, EventArgs e)
        {
            // TODO New list [Can extend to ask user if a total settings data reset must be performed]

            // Clear data table rows
            this.dataTable.Rows.Clear();
        }

        /// <summary>
        /// Handles the open tool strip menu item click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO Add code
        }

        /// <summary>
        /// Handles the save tool strip menu item click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSaveToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO Add code
        }

        /// <summary>
        /// Handles the options tool strip menu item drop down item clicked.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOptionsToolStripMenuItemDropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            // Set tool strip menu item
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)e.ClickedItem;

            // Toggle checked
            toolStripMenuItem.Checked = !toolStripMenuItem.Checked;

            // Set topmost by check box
            this.TopMost = this.alwaysOnTopToolStripMenuItem.Checked;
        }

        /// <summary>
        /// Handles the free releases public domainis tool strip menu item click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnFreeReleasesPublicDomainisToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Open our website
            Process.Start("https://publicdomain.is");
        }

        /// <summary>
        /// Handles the original thread donation codercom tool strip menu item click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOriginalThreadDonationCodercomToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Open original thread
            Process.Start("https://www.donationcoder.com/forum/index.php?topic=51393.0");
        }

        /// <summary>
        /// Handles the source code githubcom tool strip menu item click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSourceCodeGithubcomToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Open GitHub repository
            Process.Start("https://github.com/publicdomain/cliphoard");
        }

        /// <summary>
        /// Handles the about tool strip menu item click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnAboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            // TODO Add code
        }

        /// <summary>
        /// Handles the main form load.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnMainFormLoad(object sender, EventArgs e)
        {
            // Load settings data to GUI
            this.SettingsDataToGui();

            // Set data grid view data source
            this.mainDataGridView.DataSource = dataTable;

            // Hack Topmost on start [DEBUG]
            this.TopMost = this.settingsData.TopMost;
        }

        /// <summary>
        /// Handles the main form form closing.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnMainFormFormClosing(object sender, FormClosingEventArgs e)
        {
            // Set settings from GUI
            this.GuiToSettingsSata();

            // Save to disk
            this.SaveSettingsFile(this.settingsDataPath, this.settingsData);
        }

        /// <summary>
        /// GUIs to settings sata.
        /// </summary>
        private void GuiToSettingsSata()
        {
            // Options
            this.settingsData.TopMost = this.alwaysOnTopToolStripMenuItem.Checked;
            this.settingsData.OpenPopupOnCursorLocation = this.openPopupOnCursorLocationToolStripMenuItem.Checked;
            this.settingsData.ClosePopupOnSelection = this.closePopupAfterSelectionToolStripMenuItem.Checked;

            // Modifier checkboxes
            this.settingsData.Control = this.controlCheckBox.Checked;
            this.settingsData.Alt = this.altCheckBox.Checked;
            this.settingsData.Shift = this.shiftCheckBox.Checked;

            // Hotkey
            this.settingsData.Hotkey = this.keyComboBox.SelectedItem.ToString();

            // Data table items
            this.settingsData.SavedItems = JsonConvert.SerializeObject(this.dataTable);
        }

        /// <summary>
        /// Settingses the data to GUI.
        /// </summary>
        private void SettingsDataToGui()
        {
            // Options
            this.alwaysOnTopToolStripMenuItem.Checked = this.settingsData.TopMost;
            this.openPopupOnCursorLocationToolStripMenuItem.Checked = this.settingsData.OpenPopupOnCursorLocation;
            this.closePopupAfterSelectionToolStripMenuItem.Checked = this.settingsData.ClosePopupOnSelection;

            // Modifier checkboxes
            this.controlCheckBox.Checked = this.settingsData.Control;
            this.altCheckBox.Checked = this.settingsData.Alt;
            this.shiftCheckBox.Checked = this.settingsData.Shift;

            // Hotkey
            if (this.settingsData.Hotkey.Length > 0)
            {
                this.keyComboBox.SelectedItem = this.settingsData.Hotkey;
            }

            // Data table items
            this.dataTable = JsonConvert.DeserializeObject<DataTable>(this.settingsData.SavedItems); ;
        }

        /// <summary>
        /// Loads the settings file.
        /// </summary>
        /// <returns>The settings file.</returns>
        /// <param name="settingsFilePath">Settings file path.</param>
        private SettingsData LoadSettingsFile(string settingsFilePath)
        {
            // Use file stream
            using (FileStream fileStream = File.OpenRead(settingsFilePath))
            {
                // Set xml serialzer
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(SettingsData));

                // Return populated settings data
                return xmlSerializer.Deserialize(fileStream) as SettingsData;
            }
        }

        /// <summary>
        /// Saves the settings file.
        /// </summary>
        /// <param name="settingsFilePath">Settings file path.</param>
        /// <param name="settingsDataParam">Settings data parameter.</param>
        private void SaveSettingsFile(string settingsFilePath, SettingsData settingsDataParam)
        {
            try
            {
                // Use stream writer
                using (StreamWriter streamWriter = new StreamWriter(settingsFilePath, false))
                {
                    // Set xml serialzer
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(SettingsData));

                    // Serialize settings data
                    xmlSerializer.Serialize(streamWriter, settingsDataParam);
                }
            }
            catch (Exception exception)
            {
                // Advise user
                MessageBox.Show($"Error saving settings file.{Environment.NewLine}{Environment.NewLine}Message:{Environment.NewLine}{exception.Message}", "File error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the exit tool strip menu item1 click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnExitToolStripMenuItem1Click(object sender, EventArgs e)
        {
            // Close program
            this.Close();
        }
    }
}

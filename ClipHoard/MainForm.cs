// <copyright file="MainForm.cs" company="PublicDomain.is">
//     CC0 1.0 Universal (CC0 1.0) - Public Domain Dedication
//     https://creativecommons.org/publicdomain/zero/1.0/legalcode
// </copyright>

namespace ClipHoard
{
    // Directives
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Xml.Serialization;
    using Microsoft.VisualBasic;
    using Microsoft.Win32;
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
        private DataTable dataTable = null;

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
        /// Gets or sets the data table.
        /// </summary>
        /// <value>The data table.</value>
        internal DataTable DataTable { get => dataTable; set => dataTable = value; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:ClipHoard.MainForm"/> close popup after selection.
        /// </summary>
        /// <value><c>true</c> if close popup after selection; otherwise, <c>false</c>.</value>
        internal bool ClosePopupAfterSelection { get => this.closePopupAfterSelectionToolStripMenuItem.Checked; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:ClipHoard.MainForm"/> open popup on cursor location.
        /// </summary>
        /// <value><c>true</c> if open popup on cursor location; otherwise, <c>false</c>.</value>
        internal bool OpenPopupOnCursorLocation { get => this.openPopupOnCursorLocationToolStripMenuItem.Checked; }

        /// <summary>
        /// Gets the auto paste delay.
        /// </summary>
        /// <value>The auto paste delay.</value>
        internal int AutoPasteDelay { get => this.settingsData.AutoPasteDelay; }

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

            /* Process settings */

            // Check for settings file
            if (!File.Exists(this.settingsDataPath))
            {
                // Create new settings file
                this.SaveSettingsFile(this.settingsDataPath, new SettingsData());
            }

            // Load settings from disk
            this.settingsData = this.LoadSettingsFile(this.settingsDataPath);

            /* DataGridview */

            // Dynamic row count event handlers
            this.mainDataGridView.RowsAdded += (sender, args) => UpdateRowCount();
            this.mainDataGridView.RowsRemoved += (sender, args) => UpdateRowCount();

            // Multiline cell
            this.mainDataGridView.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            this.mainDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        }

        /// <summary>
        /// Handles the handle created.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            // TODO Add code
        }

        /// <summary>
        /// Updates the row count.
        /// </summary>
        private void UpdateRowCount()
        {
            this.itemsCountToolStripStatusLabel.Text = this.mainDataGridView.RowCount.ToString();
        }

        /// <summary>
        /// News the data table.
        /// </summary>
        private void ResetDataTable()
        {
            // Remove data source
            this.mainDataGridView.DataSource = null;

            // New data table
            this.DataTable = new DataTable();

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
            this.DataTable.Columns.Add(titleDataColumn);
            this.DataTable.Columns.Add(valueDataColumn);

            // Set data grid view data source
            this.mainDataGridView.DataSource = DataTable;
        }

        /// <summary>
        /// Handles the hotkey updated event.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        internal void OnHotkeyUpdated(object sender, EventArgs e)
        {
            // Only update if there's -at least- a valid key
            if ((this.keyComboBox.SelectedIndex > -1 && this.keyComboBox.SelectedItem.ToString().ToLowerInvariant() != "none"))
            {
                // Update the hotkey combination
                ((HiddenForm)this.Owner).UpdateHotkey(this.controlCheckBox.Checked, this.altCheckBox.Checked, this.shiftCheckBox.Checked, this.keyComboBox.SelectedItem.ToString());
            }
        }

        /// <summary>
        /// Sends the program to the system tray.
        /// </summary>
        internal void SendToSystemTray()
        {
            // Hide main form
            this.Hide();

            // Remove from task bar
            this.ShowInTaskbar = false;
        }

        /// <summary>
        /// Restores the window back from system tray to the foreground.
        /// </summary>
        internal void RestoreFromSystemTray()
        {
            // Make form visible again
            this.Show();

            // Return window back to normal
            this.WindowState = FormWindowState.Normal;

            // Restore in task bar
            this.ShowInTaskbar = true;
        }

        /// <summary>
        /// Handles the new tool strip menu item1 click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnNewToolStripMenuItem1Click(object sender, EventArgs e)
        {
            // New data table
            this.ResetDataTable();
        }

        /// <summary>
        /// Handles the open tool strip menu item click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnOpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Set properties
            this.openFileDialog.FileName = string.Empty;
            this.openFileDialog.DefaultExt = "txt";
            this.openFileDialog.Filter = "TXT Files|*.txt|All files (*.*)|*.*";
            this.openFileDialog.Title = "Open ClipHoard items file";

            // Show open file dialog
            if (this.openFileDialog.ShowDialog() == DialogResult.OK && this.openFileDialog.FileName.Length > 0)
            {
                try
                {
                    // Remove data source
                    this.mainDataGridView.DataSource = null;

                    // Load data table items from disk
                    this.DataTable = JsonConvert.DeserializeObject<DataTable>(File.ReadAllText(this.openFileDialog.FileName));

                    // Set data grid view data source
                    this.mainDataGridView.DataSource = DataTable;
                }
                catch (Exception exception)
                {
                    // Inform user
                    MessageBox.Show($"Error when opening \"{Path.GetFileName(this.openFileDialog.FileName)}\":{Environment.NewLine}{exception.Message}", "Open file error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Handles the save tool strip menu item click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnSaveToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Set t save file dialog properties
            this.saveFileDialog.FileName = string.Empty;
            this.saveFileDialog.DefaultExt = "txt";
            this.saveFileDialog.Filter = "TXT Files|*.txt|All files (*.*)|*.*";
            this.saveFileDialog.Title = "Save ClipHoard items file";

            // Open save file dialog
            if (this.saveFileDialog.ShowDialog() == DialogResult.OK && this.saveFileDialog.FileName.Length > 0)
            {
                try
                {
                    // Save items to disk
                    File.WriteAllText(this.saveFileDialog.FileName, JsonConvert.SerializeObject(this.DataTable, Formatting.Indented));
                }
                catch (Exception exception)
                {
                    // Inform user
                    MessageBox.Show($"Error when saving to \"{Path.GetFileName(this.saveFileDialog.FileName)}\":{Environment.NewLine}{exception.Message}", "Save file error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // Inform user
                MessageBox.Show($"Saved current items data to \"{Path.GetFileName(this.saveFileDialog.FileName)}\"", "Items file saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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

            // Start on logon
            if (toolStripMenuItem.Name == "startAtLogonToolStripMenuItem")
            {
                try
                {
                    // Open registry key
                    using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                    {
                        // Check if must write to registry
                        if (this.startAtLogonToolStripMenuItem.Checked)
                        {
                            // Add app value
                            registryKey.SetValue("ClipHoard", $"\"{Application.ExecutablePath}\" /autostart");
                        }
                        else
                        {
                            // Erase app value
                            registryKey.DeleteValue("ClipHoard", false);
                        }
                    }
                }
                catch
                {
                    // Inform user
                    MessageBox.Show("Error when interacting with the Windows registry.", "Registry error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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
        /// Handles the show tool strip menu item click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnShowToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Restore
            this.RestoreFromSystemTray();
        }

        /// <summary>
        /// Handles the autopaste delay tool strip menu item click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnAutopasteDelayToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Get new delay from user
            string newDelay = Interaction.InputBox("Set auto-paste delay:", "Delay", this.settingsData.AutoPasteDelay.ToString());

            // Try to covert to int

            if (int.TryParse(newDelay, out int delay) && this.settingsData.AutoPasteDelay != delay)
            {
                // Set
                this.settingsData.AutoPasteDelay = delay;
            }
        }

        /// <summary>
        /// Handles the minimize tool strip menu item click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnMinimizeToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Minimize to tray
            this.SendToSystemTray();
        }

        /// <summary>
        /// Handles the about tool strip menu item click.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnAboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            // Set license text
            var licenseText = $"CC0 1.0 Universal (CC0 1.0) - Public Domain Dedication{Environment.NewLine}" +
                $"https://creativecommons.org/publicdomain/zero/1.0/legalcode{Environment.NewLine}{Environment.NewLine}" +
                $"Libraries and icons have separate licenses.{Environment.NewLine}{Environment.NewLine}" +
                $"Newtonsoft.Json library by James Newton-King - MIT License{Environment.NewLine}" +
                $"https://github.com/JamesNK/Newtonsoft.Json{Environment.NewLine}{Environment.NewLine}" +
                $"Clipboard icon by UnboxScience - Pixabay License{Environment.NewLine}" +
                $"https://pixabay.com/vectors/clipboard-data-science-chart-908886/{Environment.NewLine}{Environment.NewLine}" +
                $"Patreon icon used according to published brand guidelines{Environment.NewLine}" +
                $"https://www.patreon.com/brand{Environment.NewLine}{Environment.NewLine}" +
                $"GitHub mark icon used according to published logos and usage guidelines{Environment.NewLine}" +
                $"https://github.com/logos{Environment.NewLine}{Environment.NewLine}" +
                $"DonationCoder icon used with permission{Environment.NewLine}" +
                $"https://www.donationcoder.com/forum/index.php?topic=48718{Environment.NewLine}{Environment.NewLine}" +
                $"PublicDomain icon is based on the following source images:{Environment.NewLine}{Environment.NewLine}" +
                $"Bitcoin by GDJ - Pixabay License{Environment.NewLine}" +
                $"https://pixabay.com/vectors/bitcoin-digital-currency-4130319/{Environment.NewLine}{Environment.NewLine}" +
                $"Letter P by ArtsyBee - Pixabay License{Environment.NewLine}" +
                $"https://pixabay.com/illustrations/p-glamour-gold-lights-2790632/{Environment.NewLine}{Environment.NewLine}" +
                $"Letter D by ArtsyBee - Pixabay License{Environment.NewLine}" +
                $"https://pixabay.com/illustrations/d-glamour-gold-lights-2790573/{Environment.NewLine}{Environment.NewLine}";

            // Prepend sponsors
            licenseText = $"RELEASE SPONSORS:{Environment.NewLine}{Environment.NewLine}* Jesse Reichler{Environment.NewLine}* Max P.{Environment.NewLine}{Environment.NewLine}=========={Environment.NewLine}{Environment.NewLine}" + licenseText;

            // Set title
            string programTitle = typeof(MainForm).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;

            // Set version for generating semantic version 
            Version version = typeof(MainForm).GetTypeInfo().Assembly.GetName().Version;

            // Set about form
            var aboutForm = new AboutForm(
                $"About {programTitle}",
                $"{programTitle} {version.Major}.{version.Minor}.{version.Build}",
                $"Made for: Pareidol{Environment.NewLine}DonationCoder.com{Environment.NewLine}Day #85, Week #12 @ March 26, 2022",
                licenseText,
                this.Icon.ToBitmap())
            {
                // Set about form icon
                Icon = this.associatedIcon,

                // Set always on top
                TopMost = this.TopMost
            };

            // Show about form
            aboutForm.ShowDialog();
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

            // Check for a null data table
            if (this.DataTable == null || this.DataTable.Rows.Count == 0)
            {
                // Reset data table to create a new one
                this.ResetDataTable();
            }
            else
            {
                // Set data grid view data source
                this.mainDataGridView.DataSource = DataTable;
            }

            // Hack Topmost on start [DEBUG]
            this.TopMost = this.settingsData.TopMost;

            // Open registry key
            using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false))
            {
                // Toggle check box by app value presence
                this.startAtLogonToolStripMenuItem.Checked = registryKey.GetValueNames().Contains("ClipHoard");
            }
        }

        /// <summary>
        /// TODO Handles the main form form closing. [May be modified, by FormCLosed]
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnMainFormFormClosing(object sender, FormClosingEventArgs e)
        {
            // Set settings from GUI
            this.GuiToSettingsData();

            // Save to disk
            this.SaveSettingsFile(this.settingsDataPath, this.settingsData);
        }

        /// <summary>
        /// GUIs to settings data.
        /// </summary>
        private void GuiToSettingsData()
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
            this.settingsData.SavedItems = JsonConvert.SerializeObject(this.DataTable, Formatting.Indented);
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
            this.DataTable = JsonConvert.DeserializeObject<DataTable>(this.settingsData.SavedItems);
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
        /// Handles the main form form closed.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnMainFormFormClosed(object sender, FormClosedEventArgs e)
        {
            // Exit the application
            ((HiddenForm)this.Owner).ExitThread();
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

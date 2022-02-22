// <copyright file="PopupForm.cs" company="PublicDomain.is">
//     CC0 1.0 Universal (CC0 1.0) - Public Domain Dedication
//     https://creativecommons.org/publicdomain/zero/1.0/legalcode
// </copyright>

namespace ClipHoard
{
    // Directives
    using System;
    using System.Data;
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// Description of PopupForm.
    /// </summary>
    public partial class PopupForm : Form
    {
        /// <summary>
        /// The data table.
        /// </summary>
        private DataTable dataTable = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ClipHoard.PopupForm"/> class.
        /// </summary>
        /// <param name="hoardDataTable">Hoard data table.</param>
        public PopupForm(DataTable hoardDataTable)
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            this.InitializeComponent();

            // Set datatable
            this.dataTable = hoardDataTable;

            // Populate list
            for (int i = 0; i < hoardDataTable.Rows.Count; i++)
            {
                this.popupListBox.Items.Add(hoardDataTable.Rows[i][0]);
            }
        }

        /// <summary>
        /// Handles the popup list box selected index changed.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
        private void OnPopupListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // TODO Add code
        }
    }
}

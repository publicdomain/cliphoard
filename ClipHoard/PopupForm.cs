// <copyright file="PopupForm.cs" company="PublicDomain.is">
//     CC0 1.0 Universal (CC0 1.0) - Public Domain Dedication
//     https://creativecommons.org/publicdomain/zero/1.0/legalcode
// </copyright>

namespace ClipHoard
{
    // Directives
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// Description of PopupForm.
    /// </summary>
    public partial class PopupForm : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ClipHoard.PopupForm"/> class.
        /// </summary>
        public PopupForm()
        {
            // The InitializeComponent() call is required for Windows Forms designer support.
            this.InitializeComponent();
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

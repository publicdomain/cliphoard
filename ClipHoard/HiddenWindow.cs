namespace ClipHoard
{
    // Directives
    using System;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    /// <summary>
    /// Hidden window.
    /// </summary>
    public class HiddenWindow : NativeWindow
    {
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
        /// The wm destroy.
        /// </summary>
        private const int WM_DESTROY = 0x0002;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Application.HiddenWindow"/> class.
        /// </summary>
        public HiddenWindow()
        {
            // create the handle for the window.
            this.CreateHandle(new CreateParams());
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
    }
}

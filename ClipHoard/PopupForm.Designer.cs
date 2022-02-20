
namespace ClipHoard
{
	partial class PopupForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.popupListBox = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// popupListBox
			// 
			this.popupListBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.popupListBox.FormattingEnabled = true;
			this.popupListBox.Location = new System.Drawing.Point(0, 0);
			this.popupListBox.Name = "popupListBox";
			this.popupListBox.Size = new System.Drawing.Size(227, 262);
			this.popupListBox.TabIndex = 0;
			this.popupListBox.SelectedIndexChanged += new System.EventHandler(this.OnPopupListBoxSelectedIndexChanged);
			// 
			// PopupForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(227, 262);
			this.Controls.Add(this.popupListBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "PopupForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "ClipHoard Popup";
			this.ResumeLayout(false);
		}
		private System.Windows.Forms.ListBox popupListBox;
	}
}

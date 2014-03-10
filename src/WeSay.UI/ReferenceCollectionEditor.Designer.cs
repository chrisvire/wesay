using System.Windows.Forms;
using WeSay.UI.AutoCompleteTextBox;

namespace WeSay.UI
{
	partial class ReferenceCollectionEditor<KV, ValueT, KEY_CONTAINER>
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (Control box in Controls)
				{
					if (_emptyPicker != null)
					{
						_emptyPicker.ValueChanged -= emptyPicker_ValueChanged;
						_emptyPicker = null;

					}
				}
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SuspendLayout();
			//
			// ReferenceCollectionEditor
			//
			this.BackColor = System.Drawing.Color.NavajoWhite;
			this.Name = "ReferenceCollectionEditor";
			this.Size = new System.Drawing.Size(30, 37);
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Margin = new Padding(0);
			this.ResumeLayout(false);
		   // this.PerformLayout();

		}

		#endregion
	}
}

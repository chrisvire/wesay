namespace WeSay.ConfigTool
{
	partial class WelcomeControl
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
			if (disposing && (components != null))
			{
				components.Dispose();
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WelcomeControl));
			this.blueBar = new System.Windows.Forms.Panel();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this._imageList = new System.Windows.Forms.ImageList(this.components);
			this._debounceListIndexChangedEvent = new System.Windows.Forms.Timer(this.components);
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this._templateButton = new System.Windows.Forms.Button();
			this._templateLabel = new System.Windows.Forms.Label();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			//
			// blueBar
			//
			this.blueBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(63)))), ((int)(((byte)(111)))), ((int)(((byte)(167)))));
			this.blueBar.Dock = System.Windows.Forms.DockStyle.Top;
			this.blueBar.Location = new System.Drawing.Point(0, 0);
			this.blueBar.Name = "blueBar";
			this.blueBar.Size = new System.Drawing.Size(587, 45);
			this.blueBar.TabIndex = 3;
			//
			// textBox1
			//
			this.textBox1.BackColor = System.Drawing.Color.White;
			this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.textBox1.Location = new System.Drawing.Point(116, 73);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ReadOnly = true;
			this.textBox1.Size = new System.Drawing.Size(455, 20);
			this.textBox1.TabIndex = 4;
			this.textBox1.TabStop = false;
			this.textBox1.Text = "Use this tool to create and configure WeSay Projects.";
			//
			// pictureBox1
			//
			this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
			this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.pictureBox1.Image = global::WeSay.ConfigTool.Properties.Resources.WelcomeImage;
			this.pictureBox1.InitialImage = null;
			this.pictureBox1.Location = new System.Drawing.Point(27, 21);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(70, 70);
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			//
			// _imageList
			//
			this._imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_imageList.ImageStream")));
			this._imageList.TransparentColor = System.Drawing.Color.Magenta;
			this._imageList.Images.SetKeyName(0, "browse");
			this._imageList.Images.SetKeyName(1, "getFromUsb");
			this._imageList.Images.SetKeyName(2, "wesayProject");
			this._imageList.Images.SetKeyName(3, "getFromInternet");
			this._imageList.Images.SetKeyName(4, "newProject");
			this._imageList.Images.SetKeyName(5, "flex");
			this._imageList.Images.SetKeyName(6, "solid");
			this._imageList.Images.SetKeyName(7, "getFromChorusHub");
			//
			// toolTip1
			//
			this.toolTip1.AutomaticDelay = 300;
			//
			// _templateButton
			//
			this._templateButton.FlatAppearance.BorderSize = 0;
			this._templateButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._templateButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._templateButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this._templateButton.ImageKey = "wesayProject";
			this._templateButton.ImageList = this._imageList;
			this._templateButton.Location = new System.Drawing.Point(133, 48);
			this._templateButton.Name = "_templateButton";
			this._templateButton.Size = new System.Drawing.Size(351, 23);
			this._templateButton.TabIndex = 6;
			this._templateButton.Text = "   templateButton";
			this._templateButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._templateButton.UseVisualStyleBackColor = true;
			this._templateButton.Visible = false;
			//
			// _templateLabel
			//
			this._templateLabel.AutoSize = true;
			this._templateLabel.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._templateLabel.ForeColor = System.Drawing.Color.DarkOliveGreen;
			this._templateLabel.Location = new System.Drawing.Point(313, 48);
			this._templateLabel.Name = "_templateLabel";
			this._templateLabel.Size = new System.Drawing.Size(112, 20);
			this._templateLabel.TabIndex = 7;
			this._templateLabel.Text = "Template Label";
			this._templateLabel.Visible = false;
			//
			// flowLayoutPanel1
			//
			this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.flowLayoutPanel1.Location = new System.Drawing.Point(27, 111);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(557, 227);
			this.flowLayoutPanel1.TabIndex = 8;
			//
			// WelcomeControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.Color.White;
			this.Controls.Add(this.flowLayoutPanel1);
			this.Controls.Add(this._templateLabel);
			this.Controls.Add(this._templateButton);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.blueBar);
			this.Name = "WelcomeControl";
			this.Size = new System.Drawing.Size(587, 338);
			this.Load += new System.EventHandler(this.WelcomeControl_Load);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Panel blueBar;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.ImageList _imageList;
		private System.Windows.Forms.Timer _debounceListIndexChangedEvent;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Button _templateButton;
		private System.Windows.Forms.Label _templateLabel;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;

	}
}

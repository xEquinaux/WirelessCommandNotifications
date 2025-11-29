namespace CommandNotifications.client
{
	partial class Login
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			textbox_user = new TextBox();
			label1 = new Label();
			label2 = new Label();
			textbox_code = new TextBox();
			button1 = new Button();
			SuspendLayout();
			// 
			// textbox_user
			// 
			textbox_user.Location = new Point(53, 12);
			textbox_user.MaxLength = 32;
			textbox_user.Name = "textbox_user";
			textbox_user.Size = new Size(143, 23);
			textbox_user.TabIndex = 0;
			textbox_user.WordWrap = false;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Location = new Point(17, 15);
			label1.Name = "label1";
			label1.Size = new Size(30, 15);
			label1.TabIndex = 1;
			label1.Text = "User";
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Location = new Point(12, 44);
			label2.Name = "label2";
			label2.Size = new Size(35, 15);
			label2.TabIndex = 2;
			label2.Text = "Code";
			// 
			// textbox_code
			// 
			textbox_code.Location = new Point(53, 41);
			textbox_code.MaxLength = 32;
			textbox_code.Name = "textbox_code";
			textbox_code.Size = new Size(143, 23);
			textbox_code.TabIndex = 3;
			textbox_code.WordWrap = false;
			// 
			// button1
			// 
			button1.Location = new Point(202, 12);
			button1.Name = "button1";
			button1.Size = new Size(55, 52);
			button1.TabIndex = 4;
			button1.Text = "Send";
			button1.UseVisualStyleBackColor = true;
			button1.Click += button1_Click;
			// 
			// Form1
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(274, 78);
			Controls.Add(button1);
			Controls.Add(textbox_code);
			Controls.Add(label2);
			Controls.Add(label1);
			Controls.Add(textbox_user);
			Name = "Form1";
			Text = "Enter Details";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private TextBox textbox_user;
		private Label label1;
		private Label label2;
		private TextBox textbox_code;
		private Button button1;
	}
}
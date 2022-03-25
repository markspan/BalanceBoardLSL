namespace HIDlsl
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.LinkButton = new System.Windows.Forms.Button();
            this.NameLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.BoardSelector = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LinkButton
            // 
            this.LinkButton.Location = new System.Drawing.Point(171, 159);
            this.LinkButton.Name = "LinkButton";
            this.LinkButton.Size = new System.Drawing.Size(175, 23);
            this.LinkButton.TabIndex = 0;
            this.LinkButton.Text = "Link";
            this.LinkButton.UseVisualStyleBackColor = true;
            this.LinkButton.Click += new System.EventHandler(this.LinkButton_Click);
            // 
            // NameLabel
            // 
            this.NameLabel.AutoSize = true;
            this.NameLabel.Location = new System.Drawing.Point(124, 9);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(100, 15);
            this.NameLabel.TabIndex = 1;
            this.NameLabel.Text = "BalanceBoardUSB";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(111, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(132, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "University of Groningen";
            // 
            // BoardSelector
            // 
            this.BoardSelector.FormattingEnabled = true;
            this.BoardSelector.Location = new System.Drawing.Point(81, 81);
            this.BoardSelector.Name = "BoardSelector";
            this.BoardSelector.Size = new System.Drawing.Size(265, 23);
            this.BoardSelector.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 84);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "BoardGuid";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(358, 194);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.BoardSelector);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.NameLabel);
            this.Controls.Add(this.LinkButton);
            this.Name = "MainForm";
            this.Text = "HIDBB - lsl";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button LinkButton;
        private Label NameLabel;
        private Label label1;
        private ComboBox BoardSelector;
        private Label label2;
    }
}
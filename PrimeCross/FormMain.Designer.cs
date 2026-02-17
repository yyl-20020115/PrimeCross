namespace PrimeCross
{
    partial class FormMain
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
            PrimesPictureBox = new PictureBox();
            GenerateButton = new Button();
            InfoLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)PrimesPictureBox).BeginInit();
            SuspendLayout();
            // 
            // PrimesPictureBox
            // 
            PrimesPictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PrimesPictureBox.Location = new Point(12, 12);
            PrimesPictureBox.Name = "PrimesPictureBox";
            PrimesPictureBox.Size = new Size(776, 381);
            PrimesPictureBox.TabIndex = 0;
            PrimesPictureBox.TabStop = false;
            PrimesPictureBox.MouseMove += PrimesPictureBox_MouseMove;
            // 
            // GenerateButton
            // 
            GenerateButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            GenerateButton.Location = new Point(676, 404);
            GenerateButton.Name = "GenerateButton";
            GenerateButton.Size = new Size(112, 34);
            GenerateButton.TabIndex = 1;
            GenerateButton.Text = "&Generate";
            GenerateButton.UseVisualStyleBackColor = true;
            GenerateButton.Click += GenerateButton_Click;
            // 
            // InfoLabel
            // 
            InfoLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            InfoLabel.AutoSize = true;
            InfoLabel.Location = new Point(13, 412);
            InfoLabel.Name = "InfoLabel";
            InfoLabel.Size = new Size(0, 24);
            InfoLabel.TabIndex = 2;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 449);
            Controls.Add(InfoLabel);
            Controls.Add(GenerateButton);
            Controls.Add(PrimesPictureBox);
            Name = "FormMain";
            Text = "Primes";
            Load += FormMain_Load;
            Resize += FormMain_Resize;
            ((System.ComponentModel.ISupportInitialize)PrimesPictureBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox PrimesPictureBox;
        private Button GenerateButton;
        private Label InfoLabel;
    }
}

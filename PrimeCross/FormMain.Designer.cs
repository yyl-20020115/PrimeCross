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
            ResetButton = new Button();
            FlipButton = new Button();
            InverseButton = new Button();
            ((System.ComponentModel.ISupportInitialize)PrimesPictureBox).BeginInit();
            SuspendLayout();
            // 
            // PrimesPictureBox
            // 
            PrimesPictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            PrimesPictureBox.Location = new Point(12, 12);
            PrimesPictureBox.Name = "PrimesPictureBox";
            PrimesPictureBox.Size = new Size(720, 720);
            PrimesPictureBox.TabIndex = 0;
            PrimesPictureBox.TabStop = false;
            PrimesPictureBox.MouseMove += PrimesPictureBox_MouseMove;
            // 
            // GenerateButton
            // 
            GenerateButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            GenerateButton.Location = new Point(634, 743);
            GenerateButton.Name = "GenerateButton";
            GenerateButton.Size = new Size(96, 34);
            GenerateButton.TabIndex = 1;
            GenerateButton.Text = "&Generate";
            GenerateButton.UseVisualStyleBackColor = true;
            GenerateButton.Click += GenerateButton_Click;
            // 
            // InfoLabel
            // 
            InfoLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            InfoLabel.AutoSize = true;
            InfoLabel.Location = new Point(13, 751);
            InfoLabel.Name = "InfoLabel";
            InfoLabel.Size = new Size(0, 24);
            InfoLabel.TabIndex = 2;
            // 
            // ResetButton
            // 
            ResetButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            ResetButton.Location = new Point(555, 743);
            ResetButton.Name = "ResetButton";
            ResetButton.Size = new Size(76, 34);
            ResetButton.TabIndex = 3;
            ResetButton.Text = "&Reset";
            ResetButton.UseVisualStyleBackColor = true;
            ResetButton.Click += ResetButton_Click;
            // 
            // FlipButton
            // 
            FlipButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            FlipButton.Location = new Point(476, 743);
            FlipButton.Name = "FlipButton";
            FlipButton.Size = new Size(76, 34);
            FlipButton.TabIndex = 4;
            FlipButton.Text = "&Flip";
            FlipButton.UseVisualStyleBackColor = true;
            FlipButton.Click += RotateButton_Click;
            // 
            // InverseButton
            // 
            InverseButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            InverseButton.Location = new Point(389, 743);
            InverseButton.Name = "InverseButton";
            InverseButton.Size = new Size(84, 34);
            InverseButton.TabIndex = 5;
            InverseButton.Text = "&Inverse";
            InverseButton.UseVisualStyleBackColor = true;
            InverseButton.Click += InverseButton_Click;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(744, 788);
            Controls.Add(InverseButton);
            Controls.Add(FlipButton);
            Controls.Add(ResetButton);
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
        private Button ResetButton;
        private Button FlipButton;
        private Button InverseButton;
    }
}

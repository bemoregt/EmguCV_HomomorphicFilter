namespace winform_Homom
{
    partial class Form1
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

            this.Text = "Homomorphic Filter Demo";
            this.Size = new Size(1024, 820);

            _originalPictureBox = new PictureBox
            {
                Location = new Point(20, 50),
                Size = new Size(450, 450),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };

            _resultPictureBox = new PictureBox
            {
                Location = new Point(500, 50),
                Size = new Size(450, 450),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };

            _loadImageButton = new Button
            {
                Text = "Load Image",
                Location = new Point(150, 520),
                Size = new Size(150, 30)
            };
            _loadImageButton.Click += LoadImageButton_Click;

            _homomorphicFilterButton = new Button
            {
                Text = "Homomorphic Filter",
                Location = new Point(650, 520),
                Size = new Size(150, 30),
                Enabled = false
            };
            _homomorphicFilterButton.Click += HomomorphicFilterButton_Click;

            this.Controls.Add(_originalPictureBox);
            this.Controls.Add(_resultPictureBox);
            this.Controls.Add(_loadImageButton);
            this.Controls.Add(_homomorphicFilterButton);

            Label originalLabel = new Label
            {
                Text = "Original Image",
                Location = new Point(200, 20),
                AutoSize = true
            };

            Label resultLabel = new Label
            {
                Text = "Filtered Image",
                Location = new Point(680, 20),
                AutoSize = true
            };

            this.Controls.Add(originalLabel);
            this.Controls.Add(resultLabel);

            SuspendLayout();
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion
    }
}
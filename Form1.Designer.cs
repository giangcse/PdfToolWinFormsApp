namespace PdfToolWinFormsApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblFolder = new Label();
            txtFolder = new TextBox();
            btnBrowse = new Button();
            cboFeature = new ComboBox();
            btnRun = new Button();
            txtLog = new TextBox();
            progressBar = new ProgressBar();
            SuspendLayout();
            // 
            // lblFolder
            // 
            lblFolder.AutoSize = true;
            lblFolder.Location = new Point(12, 15);
            lblFolder.Name = "lblFolder";
            lblFolder.Size = new Size(87, 15);
            lblFolder.TabIndex = 0;
            lblFolder.Text = "Chọn thư mục:";
            // 
            // txtFolder
            // 
            txtFolder.Location = new Point(105, 12);
            txtFolder.Name = "txtFolder";
            txtFolder.Size = new Size(330, 23);
            txtFolder.TabIndex = 1;
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(441, 10);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(75, 23);
            btnBrowse.TabIndex = 2;
            btnBrowse.Text = "Browse...";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // cboFeature
            // 
            cboFeature.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFeature.FormattingEnabled = true;
            cboFeature.Items.AddRange(new object[] { "Đếm số trang PDF", "Gộp PDF", "Đổi tên sau Ký số" });
            cboFeature.Location = new Point(105, 45);
            cboFeature.Name = "cboFeature";
            cboFeature.Size = new Size(329, 23);
            cboFeature.TabIndex = 3;
            // 
            // btnRun
            // 
            btnRun.Location = new Point(440, 45);
            btnRun.Name = "btnRun";
            btnRun.Size = new Size(75, 23);
            btnRun.TabIndex = 4;
            btnRun.Text = "Chạy";
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += btnRun_Click;
            // 
            // txtLog
            // 
            txtLog.Location = new Point(15, 80);
            txtLog.Multiline = true;
            txtLog.Name = "txtLog";
            txtLog.ScrollBars = ScrollBars.Vertical;
            txtLog.Size = new Size(500, 200);
            txtLog.TabIndex = 5;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(15, 290);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(500, 23);
            progressBar.TabIndex = 6;
            // 
            // Form1
            // 
            ClientSize = new Size(534, 325);
            Controls.Add(progressBar);
            Controls.Add(txtLog);
            Controls.Add(btnRun);
            Controls.Add(cboFeature);
            Controls.Add(btnBrowse);
            Controls.Add(txtFolder);
            Controls.Add(lblFolder);
            Name = "Form1";
            Text = "PDF Tool App";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.ComboBox cboFeature;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.ProgressBar progressBar;
    }
}

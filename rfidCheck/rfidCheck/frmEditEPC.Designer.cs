namespace LogisTechBase.rfidCheck
{
    partial class frmEditEPC
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmEditEPC));
            this.button6 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.lblTip = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnSaveSecret = new System.Windows.Forms.Button();
            this.btnLockTag = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.lblSecretConfirm = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.txtSecretAgain = new System.Windows.Forms.TextBox();
            this.txtSecret = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.statusLabel = new System.Windows.Forms.Label();
            this.btnSerialPortConfig = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(300, 382);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(82, 28);
            this.button6.TabIndex = 42;
            this.button6.Text = "退出(&X)";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button1.Location = new System.Drawing.Point(246, 29);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(108, 28);
            this.button1.TabIndex = 39;
            this.button1.Text = "写入(&W)";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 46;
            this.label1.Text = "EPC：";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(19, 32);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(201, 21);
            this.textBox1.TabIndex = 47;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(246, 91);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(108, 28);
            this.button2.TabIndex = 42;
            this.button2.Text = "读取当前EPC(&R)";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // lblTip
            // 
            this.lblTip.AutoSize = true;
            this.lblTip.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblTip.Location = new System.Drawing.Point(17, 56);
            this.lblTip.Name = "lblTip";
            this.lblTip.Size = new System.Drawing.Size(41, 12);
            this.lblTip.TabIndex = 48;
            this.lblTip.Text = "label2";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.lblTip);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(10, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(370, 137);
            this.groupBox1.TabIndex = 49;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "编辑EPC";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnSaveSecret);
            this.groupBox2.Controls.Add(this.btnLockTag);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.lblSecretConfirm);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.txtSecretAgain);
            this.groupBox2.Controls.Add(this.txtSecret);
            this.groupBox2.Location = new System.Drawing.Point(10, 145);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(370, 151);
            this.groupBox2.TabIndex = 50;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "锁定标签";
            this.groupBox2.Visible = false;
            // 
            // btnSaveSecret
            // 
            this.btnSaveSecret.Location = new System.Drawing.Point(246, 96);
            this.btnSaveSecret.Name = "btnSaveSecret";
            this.btnSaveSecret.Size = new System.Drawing.Size(108, 28);
            this.btnSaveSecret.TabIndex = 40;
            this.btnSaveSecret.Text = "保存密码";
            this.btnSaveSecret.UseVisualStyleBackColor = true;
            this.btnSaveSecret.Click += new System.EventHandler(this.btnSaveSecret_Click);
            // 
            // btnLockTag
            // 
            this.btnLockTag.Location = new System.Drawing.Point(246, 30);
            this.btnLockTag.Name = "btnLockTag";
            this.btnLockTag.Size = new System.Drawing.Size(108, 28);
            this.btnLockTag.TabIndex = 39;
            this.btnLockTag.Text = "锁定标签";
            this.btnLockTag.UseVisualStyleBackColor = true;
            this.btnLockTag.Click += new System.EventHandler(this.btnLockTag_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 77);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 36;
            this.label5.Text = "再输一遍：";
            // 
            // lblSecretConfirm
            // 
            this.lblSecretConfirm.AutoSize = true;
            this.lblSecretConfirm.Location = new System.Drawing.Point(53, 120);
            this.lblSecretConfirm.Name = "lblSecretConfirm";
            this.lblSecretConfirm.Size = new System.Drawing.Size(65, 12);
            this.lblSecretConfirm.TabIndex = 38;
            this.lblSecretConfirm.Text = "密码不一致";
            this.lblSecretConfirm.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(15, 21);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(41, 12);
            this.label12.TabIndex = 34;
            this.label12.Text = "密码：";
            // 
            // txtSecretAgain
            // 
            this.txtSecretAgain.Location = new System.Drawing.Point(17, 96);
            this.txtSecretAgain.Name = "txtSecretAgain";
            this.txtSecretAgain.PasswordChar = '*';
            this.txtSecretAgain.Size = new System.Drawing.Size(101, 21);
            this.txtSecretAgain.TabIndex = 37;
            this.txtSecretAgain.Text = "12345678";
            this.txtSecretAgain.TextChanged += new System.EventHandler(this.txtSecretAgain_TextChanged);
            // 
            // txtSecret
            // 
            this.txtSecret.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtSecret.Location = new System.Drawing.Point(17, 41);
            this.txtSecret.Name = "txtSecret";
            this.txtSecret.PasswordChar = '*';
            this.txtSecret.Size = new System.Drawing.Size(101, 21);
            this.txtSecret.TabIndex = 33;
            this.txtSecret.Text = "12345678";
            this.txtSecret.TextChanged += new System.EventHandler(this.txtSecret_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 362);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(257, 12);
            this.label2.TabIndex = 40;
            this.label2.Text = "注：标签只有在锁定后，才能在考勤系统中使用";
            this.label2.Visible = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.statusLabel);
            this.groupBox3.Location = new System.Drawing.Point(-1, 419);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(430, 100);
            this.groupBox3.TabIndex = 51;
            this.groupBox3.TabStop = false;
            this.groupBox3.Visible = false;
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(6, 17);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(41, 12);
            this.statusLabel.TabIndex = 0;
            this.statusLabel.Text = "label3";
            // 
            // btnSerialPortConfig
            // 
            this.btnSerialPortConfig.Location = new System.Drawing.Point(298, 302);
            this.btnSerialPortConfig.Name = "btnSerialPortConfig";
            this.btnSerialPortConfig.Size = new System.Drawing.Size(82, 28);
            this.btnSerialPortConfig.TabIndex = 52;
            this.btnSerialPortConfig.Text = "串口设置";
            this.btnSerialPortConfig.UseVisualStyleBackColor = true;
            this.btnSerialPortConfig.Visible = false;
            this.btnSerialPortConfig.Click += new System.EventHandler(this.btnSerialPortConfig_Click);
            // 
            // frmEditEPC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 454);
            this.Controls.Add(this.btnSerialPortConfig);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button6);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(900, 1000);
            this.MinimizeBox = false;
            this.Name = "frmEditEPC";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "标签编辑";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label lblTip;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblSecretConfirm;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtSecretAgain;
        private System.Windows.Forms.TextBox txtSecret;
        private System.Windows.Forms.Button btnLockTag;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Button btnSerialPortConfig;
        private System.Windows.Forms.Button btnSaveSecret;
    }
}
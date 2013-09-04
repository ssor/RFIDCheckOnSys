namespace LogisTechBase
{
    partial class frmHFRead
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmHFRead));
            this.btn_opencom = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.button1 = new System.Windows.Forms.Button();
            this.btnSerialPortConfig = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.ckb14443b = new System.Windows.Forms.CheckBox();
            this.ckb14443a = new System.Windows.Forms.CheckBox();
            this.ckb15693 = new System.Windows.Forms.CheckBox();
            this.ckbTagit = new System.Windows.Forms.CheckBox();
            this.ProgressControl1 = new LogisTechBase.MatrixCircularProgressControl();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_opencom
            // 
            this.btn_opencom.Location = new System.Drawing.Point(677, 124);
            this.btn_opencom.Name = "btn_opencom";
            this.btn_opencom.Size = new System.Drawing.Size(85, 29);
            this.btn_opencom.TabIndex = 51;
            this.btn_opencom.Text = "打开串口";
            this.btn_opencom.UseVisualStyleBackColor = true;
            this.btn_opencom.Click += new System.EventHandler(this.btn_opencom_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(684, 500);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(85, 29);
            this.button2.TabIndex = 50;
            this.button2.Text = "退出";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dataGridView1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(640, 464);
            this.groupBox1.TabIndex = 48;
            this.groupBox1.TabStop = false;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(3, 17);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidth = 21;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(634, 444);
            this.dataGridView1.TabIndex = 41;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(677, 25);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(85, 29);
            this.button1.TabIndex = 47;
            this.button1.Text = "开始读取";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnSerialPortConfig
            // 
            this.btnSerialPortConfig.Location = new System.Drawing.Point(677, 169);
            this.btnSerialPortConfig.Name = "btnSerialPortConfig";
            this.btnSerialPortConfig.Size = new System.Drawing.Size(85, 29);
            this.btnSerialPortConfig.TabIndex = 46;
            this.btnSerialPortConfig.Text = "串口设置";
            this.btnSerialPortConfig.UseVisualStyleBackColor = true;
            this.btnSerialPortConfig.Click += new System.EventHandler(this.btnSerialPortConfig_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.ckb14443b);
            this.groupBox3.Controls.Add(this.ckb14443a);
            this.groupBox3.Controls.Add(this.ckb15693);
            this.groupBox3.Controls.Add(this.ckbTagit);
            this.groupBox3.Location = new System.Drawing.Point(677, 204);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(104, 269);
            this.groupBox3.TabIndex = 49;
            this.groupBox3.TabStop = false;
            // 
            // ckb14443b
            // 
            this.ckb14443b.AutoSize = true;
            this.ckb14443b.Checked = true;
            this.ckb14443b.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckb14443b.Location = new System.Drawing.Point(7, 61);
            this.ckb14443b.Name = "ckb14443b";
            this.ckb14443b.Size = new System.Drawing.Size(84, 16);
            this.ckb14443b.TabIndex = 0;
            this.ckb14443b.Text = "14443B协议";
            this.ckb14443b.UseVisualStyleBackColor = true;
            // 
            // ckb14443a
            // 
            this.ckb14443a.AutoSize = true;
            this.ckb14443a.Checked = true;
            this.ckb14443a.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckb14443a.Location = new System.Drawing.Point(7, 39);
            this.ckb14443a.Name = "ckb14443a";
            this.ckb14443a.Size = new System.Drawing.Size(84, 16);
            this.ckb14443a.TabIndex = 0;
            this.ckb14443a.Text = "14443A协议";
            this.ckb14443a.UseVisualStyleBackColor = true;
            // 
            // ckb15693
            // 
            this.ckb15693.AutoSize = true;
            this.ckb15693.Checked = true;
            this.ckb15693.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ckb15693.Location = new System.Drawing.Point(7, 17);
            this.ckb15693.Name = "ckb15693";
            this.ckb15693.Size = new System.Drawing.Size(78, 16);
            this.ckb15693.TabIndex = 0;
            this.ckb15693.Text = "15693协议";
            this.ckb15693.UseVisualStyleBackColor = true;
            // 
            // ckbTagit
            // 
            this.ckbTagit.AutoSize = true;
            this.ckbTagit.Location = new System.Drawing.Point(8, 109);
            this.ckbTagit.Name = "ckbTagit";
            this.ckbTagit.Size = new System.Drawing.Size(84, 16);
            this.ckbTagit.TabIndex = 0;
            this.ckbTagit.Text = "TAG-IT协议";
            this.ckbTagit.UseVisualStyleBackColor = true;
            this.ckbTagit.Visible = false;
            // 
            // ProgressControl1
            // 
            this.ProgressControl1.BackColor = System.Drawing.Color.Transparent;
            this.ProgressControl1.Interval = 60;
            this.ProgressControl1.Location = new System.Drawing.Point(693, 55);
            this.ProgressControl1.MinimumSize = new System.Drawing.Size(28, 28);
            this.ProgressControl1.Name = "ProgressControl1";
            this.ProgressControl1.Rotation = LogisTechBase.MatrixCircularProgressControl.Direction.CLOCKWISE;
            this.ProgressControl1.Size = new System.Drawing.Size(56, 50);
            this.ProgressControl1.StartAngle = 270F;
            this.ProgressControl1.TabIndex = 52;
            this.ProgressControl1.TickColor = System.Drawing.Color.FromArgb(((int)(((byte)(58)))), ((int)(((byte)(58)))), ((int)(((byte)(58)))));
            // 
            // frmHFRead
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(804, 541);
            this.Controls.Add(this.ProgressControl1);
            this.Controls.Add(this.btn_opencom);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnSerialPortConfig);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmHFRead";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "高频RFID标签读取";
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_opencom;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnSerialPortConfig;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox ckbTagit;
        private System.Windows.Forms.CheckBox ckb14443b;
        private System.Windows.Forms.CheckBox ckb14443a;
        private System.Windows.Forms.CheckBox ckb15693;
        private MatrixCircularProgressControl ProgressControl1;
    }
}
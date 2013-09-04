namespace LogisTechBase
{
    partial class FrmRfidCheck_Server
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmRfidCheck_Server));
            this.btn_stopserver = new System.Windows.Forms.Button();
            this.btn_startserver = new System.Windows.Forms.Button();
            this.btnReadRfid = new System.Windows.Forms.Button();
            this.btnStopRfidCheck = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.labelStatus = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_stopserver
            // 
            this.btn_stopserver.Location = new System.Drawing.Point(6, 68);
            this.btn_stopserver.Name = "btn_stopserver";
            this.btn_stopserver.Size = new System.Drawing.Size(121, 34);
            this.btn_stopserver.TabIndex = 11;
            this.btn_stopserver.Text = "停止(&S)";
            this.btn_stopserver.UseVisualStyleBackColor = true;
            this.btn_stopserver.Click += new System.EventHandler(this.btn_stopserver_Click_1);
            // 
            // btn_startserver
            // 
            this.btn_startserver.Location = new System.Drawing.Point(6, 20);
            this.btn_startserver.Name = "btn_startserver";
            this.btn_startserver.Size = new System.Drawing.Size(121, 34);
            this.btn_startserver.TabIndex = 10;
            this.btn_startserver.Text = "启动(&R)";
            this.btn_startserver.UseVisualStyleBackColor = true;
            this.btn_startserver.Click += new System.EventHandler(this.btn_startserver_Click);
            // 
            // btnReadRfid
            // 
            this.btnReadRfid.Location = new System.Drawing.Point(12, 25);
            this.btnReadRfid.Name = "btnReadRfid";
            this.btnReadRfid.Size = new System.Drawing.Size(121, 34);
            this.btnReadRfid.TabIndex = 15;
            this.btnReadRfid.Text = "启动(&L)";
            this.btnReadRfid.UseVisualStyleBackColor = true;
            this.btnReadRfid.Click += new System.EventHandler(this.btnReadRfid_Click);
            // 
            // btnStopRfidCheck
            // 
            this.btnStopRfidCheck.Location = new System.Drawing.Point(12, 73);
            this.btnStopRfidCheck.Name = "btnStopRfidCheck";
            this.btnStopRfidCheck.Size = new System.Drawing.Size(121, 34);
            this.btnStopRfidCheck.TabIndex = 15;
            this.btnStopRfidCheck.Text = "停止(&T)";
            this.btnStopRfidCheck.UseVisualStyleBackColor = true;
            this.btnStopRfidCheck.Click += new System.EventHandler(this.btnStopRfidCheck_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(3, 17);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidth = 20;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(505, 443);
            this.dataGridView1.TabIndex = 38;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labelStatus.Location = new System.Drawing.Point(13, 535);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(71, 12);
            this.labelStatus.TabIndex = 39;
            this.labelStatus.Text = "labelStatus";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(592, 488);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(97, 29);
            this.button1.TabIndex = 40;
            this.button1.Text = "退出(&X)";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btn_stopserver);
            this.groupBox1.Controls.Add(this.btn_startserver);
            this.groupBox1.Location = new System.Drawing.Point(547, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(169, 113);
            this.groupBox1.TabIndex = 41;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "远程考勤服务";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnStopRfidCheck);
            this.groupBox2.Controls.Add(this.btnReadRfid);
            this.groupBox2.Location = new System.Drawing.Point(547, 143);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(169, 113);
            this.groupBox2.TabIndex = 42;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "本地RFID考勤";
            this.groupBox2.Visible = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.dataGridView1);
            this.groupBox3.Location = new System.Drawing.Point(12, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(511, 463);
            this.groupBox3.TabIndex = 43;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "已考勤人列表";
            // 
            // groupBox4
            // 
            this.groupBox4.Location = new System.Drawing.Point(547, 465);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(166, 10);
            this.groupBox4.TabIndex = 44;
            this.groupBox4.TabStop = false;
            // 
            // groupBox5
            // 
            this.groupBox5.Location = new System.Drawing.Point(1, 520);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(728, 10);
            this.groupBox5.TabIndex = 44;
            this.groupBox5.TabStop = false;
            // 
            // FrmRfidCheck_Server
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(722, 553);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmRfidCheck_Server";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "考勤服务端";
            this.Load += new System.EventHandler(this.FrmRfidCheck_Server_Load);
            this.Shown += new System.EventHandler(this.FrmRfidCheck_Server_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }



        #endregion

        private System.Windows.Forms.Button btn_stopserver;
        private System.Windows.Forms.Button btn_startserver;
        private System.Windows.Forms.Button btnReadRfid;
        private System.Windows.Forms.Button btnStopRfidCheck;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox5;
    }
}
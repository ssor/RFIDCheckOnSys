using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LogisTechBase.rfidCheck
{
    public partial class FrmCheckStatistics : Form
    {
        public FrmCheckStatistics()
        {
            InitializeComponent();
            this.Load += new EventHandler(FrmCheckStatistics_Load);
            this.Shown += new EventHandler(FrmCheckStatistics_Shown);
        }

        void FrmCheckStatistics_Shown(object sender, EventArgs e)
        {
            this.RefreshDataView();
        }

        void FrmCheckStatistics_Load(object sender, EventArgs e)
        {
            List<string> njList = rfidCheck_CheckOn.GetnjList();
            for (int i = 0; i < njList.Count; i++)
            {
                this.cmBnj.Items.Add(njList[i]);
            }
            List<string> bjList = rfidCheck_CheckOn.GetbjList();
            for (int i = 0; i < bjList.Count; i++)
            {
                this.cmBbj.Items.Add(bjList[i]);
            }
            this.dTPStart.Value = DateTime.Today.AddDays(-1);
            this.dTPEnd.Value = DateTime.Now;

        }
        void RefreshDataView()
        {
            string start = rfidCheck_CheckOn.GetFormatDateTimeString(this.dTPStart.Value);
            string end = rfidCheck_CheckOn.GetFormatDateTimeString(this.dTPEnd.Value);
            string bj = null;
            if (this.cmBbj.Text != null && this.cmBbj.Text.Trim() != "")
            {
                bj = cmBbj.Text;
            }
            string nj = null;
            if (this.cmBnj.Text != null && this.cmBnj.Text.Trim() != "")
            {
                nj = cmBnj.Text;
            }
            DataSet ds = rfidCheck_CheckOn.GetStatisticCheckInfoDataSet(start, end, nj, bj);

            if (null != ds)
            {
                this.dataGridView1.DataSource = ds.Tables[0];
            }
            this.dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            int headerW = this.dataGridView1.RowHeadersWidth;
            int columnsW = 0;
            DataGridViewColumnCollection columns = this.dataGridView1.Columns;
            for (int i = 0; i < columns.Count; i++)
            {
                columnsW += columns[i].Width;
            }
            if (columnsW + headerW < this.dataGridView1.Width)
            {
                int leftTotalWidht = this.dataGridView1.Width - columnsW - headerW;
                int eachColumnAddedWidth = leftTotalWidht / columns.Count;
                for (int i = 0; i < columns.Count; i++)
                {
                    columns[i].Width += eachColumnAddedWidth;
                }
            }
            /* 
            
            List<CheckPerson> records = rfidCheck_CheckOn.GetCheckedPersonList(this.dateTimePicker1.Value.ToShortDateString());
            //List<CheckPerson> records = rfidCheck_CheckOn.GetCheckedPersonList("2011/5/10 9:00:00");
            DataTable table = new DataTable("Records");
            DataColumn column;
            DataRow row;

            // Create new DataColumn, set DataType, 
            // ColumnName and add to DataTable.    
            column = new DataColumn("学号");
            //column.Unique = true;
            // Add the Column to the DataColumnCollection.
            table.Columns.Add(column);
            column = new DataColumn("姓名");
            // Add the Column to the DataColumnCollection.
            table.Columns.Add(column);
            column = new DataColumn("考勤时间");
            // Add the Column to the DataColumnCollection.
            table.Columns.Add(column);

            for (int i = 0; i < records.Count; i++)
            {
                row = table.NewRow();
                CheckPerson cp = records[i];
                Person p = cp.person;
                row["学号"] = p.id_num;
                row["姓名"] = p.name;
                row["考勤时间"] = cp.checkDate;
                table.Rows.Add(row);
            }
            this.dataGridView1.DataSource = table;

            this.dataGridView1.Columns[0].Width = 80;
            this.dataGridView1.Columns[1].Width = 80;
            this.dataGridView1.Columns[2].Width = 120;
            */

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.RefreshDataView();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            //this.RefreshDataView();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

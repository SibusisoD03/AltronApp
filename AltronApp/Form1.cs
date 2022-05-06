using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Z.Dapper.Plus;

namespace AltronApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string sqlConn = "Server=.;Database=AltronDB;User Id=sa;Password=me.close;";

        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {                
                DapperPlusManager.Entity<Expenditure>().Table("Expenditure");
                List<Expenditure> expenditures = expenditureBindingSource.DataSource as List<Expenditure>;
                if (expenditures != null)
                {
                    using (IDbConnection db = new SqlConnection(sqlConn))
                    {
                        db.BulkInsert(expenditures);
                    }
                }
                MessageBox.Show("Saved to database!!!");
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message,"Message",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'altronDBDataSet.Expenditure' table. You can move, or remove it, as needed.
            this.expenditureTableAdapter.Fill(this.altronDBDataSet.Expenditure);

        }

        private void comboSheet_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dt = tableCollection[comboSheet.SelectedItem.ToString()];
            //dataGridView1.DataSource = dt;
            if (dt != null)
            {
                List<Expenditure> expenditures = new List<Expenditure>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Expenditure expenditure = new Expenditure();
                    expenditure.Month = dt.Rows[i]["Month"].ToString();
                    expenditure.Income = dt.Rows[i]["Income"].ToString();
                    expenditure.Expenses = dt.Rows[i]["Expenses"].ToString();
                    
                    expenditures.Add(expenditure);
                }
                expenditureBindingSource.DataSource = expenditures;
            }
        }

        DataTableCollection tableCollection;

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "Excel 97-2003 Workbook|*.xls|Excel Workbook|*.xlsx" })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtFilename.Text = openFileDialog.FileName;
                    using (var stream = File.Open(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                    {
                        using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
                            {
                                ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                            });
                            tableCollection = result.Tables;
                            comboSheet.Items.Clear();

                            foreach (DataTable table in tableCollection)
                            {
                                comboSheet.Items.Add(table.TableName);
                            }
                        }
                    }
                }
            }
        }

        private void btnGraph_Click(object sender, EventArgs e)
        {
            fillChart();
        }

        private void fillChart()
        {
            SqlConnection con = new SqlConnection(sqlConn);
            DataSet ds = new DataSet();
            con.Open();
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("select month, expenses from expenditure", con);
            sqlDataAdapter.Fill(ds);
            chart1.DataSource = ds;
            chart1.Series["Expenditure"].XValueMember = "month";
            chart1.Series["Expenditure"].YValueMembers = "expenses";
            chart1.Titles.Add("Altron Expenditure");
            con.Close();
        }
    }
}

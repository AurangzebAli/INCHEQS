using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using Npgsql;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private DataSet ds = new DataSet();
        private DataTable dtResult = new DataTable();



        private void button1_Click(object sender, EventArgs e)
        {
            //DataTable dtResult = new DataTable();

            try
            {
                string errorMsg = "";


                using (NpgsqlConnection conn = new NpgsqlConnection())
                {
                    conn.ConnectionString = "Server=127.0.0.1;Port=5432;User Id=incheqsuser;Password=incheqs2008;Database=INCHEQS_ICS";
                    // conn.ConnectionString = Database.Connection.ConnectionString;
                    //ConfigureSetting.GetConnectionString(); // ConfigurationManager.ConnectionStrings["default"].ConnectionString;

                    //string sql = " SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY fldAmount DESC) AS RowNum, fldInwardItemId, fldUIC, fldAccountNumber, fldChequeSerialNo, fldIssueBranchCode, fldPayBranchCode, fldTransCode, fldAmount, fldrejectstatus1, sum(fldAmount) OVER() As GrandTotalAmount, '0' As TotalOutstanding, '0' As TotalImportItem, Count(*) OVER() As TotalRecords, ((COUNT(*)OVER() + 10 - 1) / 10) AS TotalPage FROM View_Verification WHERE fldClearDate =@cleardate AND fldBankCode = @bankcode   ) AS RowConstrainedResult WHERE RowNum > ((0 - 1) * 10) ORDER BY RowNum LIMIT(10) ";
                    ////string sql = " Select * from tblusermaster ";

                    conn.Open();
                   // NpgsqlCommand cmd = conn.CreateCommand();

                    //List<NpgsqlParameter> cmdParms = new List<NpgsqlParameter>();
                    //NpgsqlParameter.
                    //NpgsqlParameter.Parameters.Add(new NpgsqlParameter(":p_vchTaskId", "308110"));
                    //NpgsqlParameter.Add(new NpgsqlParameter(":p_condition", "AND fldcleardate = 28 / Jun / 2018"));
                    //NpgsqlParameter.Add(new NpgsqlParameter(":p_orderBY", ""));
                    //NpgsqlParameter.Add(new NpgsqlParameter(":p_fldClearDate", ""));
                    //NpgsqlParameter.Add(new NpgsqlParameter(":p_currentUserAbbr", ""));
                    //NpgsqlParameter.Add(new NpgsqlParameter(":p_currentUserBankCode", "52"));
                    //NpgsqlParameter.Add(new NpgsqlParameter(":p_branchcode", "5018"));
                    //NpgsqlParameter.Add(new NpgsqlParameter(":p_select", "fldinwarditemid ,fldaccountnumber ,fldchequeserialno ,fldtranscode , fldrejectcodepending, fldrejectstatus1, fldamount, flditemstatuspending, fldlastupdateusernamepending"));
                    //NpgsqlParameter.Add(new NpgsqlParameter(":p_amount", "fldamount"));
                    //NpgsqlParameter.Add(new NpgsqlParameter(":p_page", "0"));

                    NpgsqlDataReader rdr = null;
                    //NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(sql, conn);
                    NpgsqlCommand cmd = new NpgsqlCommand("spcgInwardPendingMaker", conn);
                    NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd);
                    cmd.CommandType = CommandType.StoredProcedure;
                    adapter.SelectCommand.CommandTimeout = 300; //manually set sql execution timeout to 5 minutes
                   // adapter.Fill(ds);
                    //AttachParameters(cmd, cmdParms[]);
                    cmd.Parameters.AddWithValue(":p_vchtaskid", "308110");
                    cmd.Parameters.AddWithValue(":p_condition", "AND fldcleardate = 28 / Jun / 2018");
                    cmd.Parameters.AddWithValue(":p_orderby", "");
                    cmd.Parameters.AddWithValue(":p_fldcleardate", "");
                    cmd.Parameters.AddWithValue(":p_currentuserabbr", "");
                    cmd.Parameters.AddWithValue(":p_currentuserbankcode", "52");
                    cmd.Parameters.AddWithValue(":p_branchcode", "5018");
                    cmd.Parameters.AddWithValue(":p_select", "fldinwarditemid ,fldaccountnumber ,fldchequeserialno ,fldtranscode , fldrejectcodepending, fldrejectstatus1, fldamount, flditemstatuspending, fldlastupdateusernamepending");
                    cmd.Parameters.AddWithValue(":p_amount", "fldamount");
                    cmd.Parameters.AddWithValue(":p_page", "0");
                    rdr = cmd.ExecuteReader();
                    dtResult.Load(rdr);
                    adapter.Fill(ds);
                    dtResult = ds.Tables[0];
                    dataGridView1.DataSource = dtResult;

                    conn.Close();

                }

            }

            catch (Exception ex)
            {
                throw ex;
            }



        }

        private void PrepareCommand(NpgsqlCommand cmd, NpgsqlTransaction trans, CommandType cmdType, string cmdText, NpgsqlParameter[] commandParameters, ref string _ErrorMsg)
        {
            try
            {
                cmd.CommandText = cmdText;
                if (trans != null)
                {
                    cmd.Transaction = trans;
                }
                cmd.CommandType = cmdType;
                //attach the command parameters if they are provided
                if (commandParameters != null)
                {
                    AttachParameters(cmd, commandParameters);
                }

            }
            catch (Exception ex)
            {
                throw ex;
                _ErrorMsg = "[PrepareCommand] (" + cmdText + ")" + ex.Message;
            }





        }
        private void AttachParameters(NpgsqlCommand command, NpgsqlParameter[] commandParameters)
        {
            //command.Parameters.Clear();
            foreach (NpgsqlParameter p in commandParameters)
            {
                Console.Write(p);
                //check for derived output value with no value assigned
                if ((p.Direction == ParameterDirection.InputOutput) && (p.Value == null))
                {
                    p.Value = DBNull.Value;
                }

                command.Parameters.Add(p);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}

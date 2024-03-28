using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Lab2
{
    public partial class Form1 : Form
    {
        private static readonly string Con = ConfigurationManager.ConnectionStrings["cn"].ConnectionString;
        private static readonly string ParentName = ConfigurationManager.AppSettings["ParentTableName"];
        private static readonly string ChildName = ConfigurationManager.AppSettings["ChildTableName"];
        private static readonly string ChildId = ConfigurationManager.AppSettings["ChildID"];
        private static readonly int ChildNumberOfColumns = int.Parse(ConfigurationManager.AppSettings["ChildNumberOfColumns"]);
        private static readonly string InsertQuery = ConfigurationManager.AppSettings["ChildInsertQUERY"];
        private static readonly string DeleteQuery = ConfigurationManager.AppSettings["ChildDeleteQUERY"];
        private static readonly string UpdateQuery = ConfigurationManager.AppSettings["ChildUpdateQUERY"];

        private static readonly string ChildArr = ConfigurationManager.AppSettings["ChildArr"];

        private static readonly string ChildColumnNames = ConfigurationManager.AppSettings["ChildColumnNames"];
        private static readonly string ChildColumnTypes = ConfigurationManager.AppSettings["ChildColumnTypes"];
        private static readonly string ChildToParentId = ConfigurationManager.AppSettings["ChildToParentID"];

        private readonly SqlConnection _cs = new SqlConnection(Con);
        private readonly SqlDataAdapter _dataAdapter = new SqlDataAdapter();
        private readonly BindingSource _bindingSourceParent = new BindingSource();
        private readonly BindingSource _bindingSourceChild = new BindingSource();
        private readonly DataSet _dataSetParent = new DataSet();
        private readonly DataSet _dataSetChild = new DataSet();

        private readonly TextBox[] _textBoxes = new TextBox[ChildNumberOfColumns];
        private readonly Label[] _labels = new Label[ChildNumberOfColumns];
        
        public Form1()
        {
            InitializeComponent();
            Console.WriteLine(_cs.ConnectionString);
            // initializez text box-urile si label-urile pentru fiecare proprietate
            var names = ChildColumnNames.Split(new []{", "}, StringSplitOptions.None);
            for (var i = 0; i < ChildNumberOfColumns; i++)
            {
                _labels[i] = new Label();
                _textBoxes[i] = new TextBox();
            
            
                _labels[i].Text = names[i];
                _labels[i].Location = new Point(50 + i * 120, 370);
                _labels[i].AutoSize = true;
            
                _textBoxes[i].Text = "";
                _textBoxes[i].Location = new Point(50 + i * 120, 385);
            }
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for (var i = 0; i < ChildNumberOfColumns; i++)
            {
                Controls.Add(_labels[i]);
                Controls.Add(_textBoxes[i]);
            }
        }

        // am nevoie de id-ul maxim din tabela copil pentru a putea adauga o noua linie (pk-ul nu e identity)
        private int GetMaxId()
        {
            var highestId = 0;
            var query = $"SELECT MAX({ChildId}) FROM {ChildName}";

            var command = new SqlCommand(query, _cs);
            try
            {
                _cs.Open();
                var result = command.ExecuteScalar();

                if (result != DBNull.Value) // Check if result is not null
                {
                    highestId = Convert.ToInt32(result);
                }
                else
                {
                    Console.WriteLine(@"Table is empty or ID column has NULL values.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Error: " + ex.Message);
            }
            finally
            {
                _cs.Close();
            }
            return highestId;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            _dataAdapter.InsertCommand = new SqlCommand(InsertQuery, _cs);

            var args = ChildArr.Split(new []{", "} , StringSplitOptions.None);
            var types = ChildColumnTypes.Split(new []{", "} , StringSplitOptions.None);

            try
            {
                _dataAdapter.InsertCommand.Parameters.Add(args[0], SqlDbType.Int).Value = GetMaxId() + 1;
                for (var i = 0; i < ChildNumberOfColumns; i++)
                {
                    switch (types[i])
                    {
                        case "string":
                            _dataAdapter.InsertCommand.Parameters.Add(args[i + 1], SqlDbType.VarChar).Value =
                                _textBoxes[i].Text;
                            break;
                        case "int":
                            _dataAdapter.InsertCommand.Parameters.Add(args[i + 1], SqlDbType.Int).Value =
                                int.Parse(_textBoxes[i].Text);
                            break;
                        case "float":
                            _dataAdapter.InsertCommand.Parameters.Add(args[i + 1], SqlDbType.Float).Value =
                                float.Parse(_textBoxes[i].Text);
                            break;
                        default:
                            MessageBox.Show(@"WTF man");
                            break;
                    }
                }
                _cs.Open();
                _dataAdapter.InsertCommand.ExecuteNonQuery();
                _cs.Close();
                _dataSetChild.Clear();
                _dataAdapter.Fill(_dataSetChild);
            }
            catch(Exception ex)
            {
                _cs.Close();
                MessageBox.Show(@"Input gresit! Error: "+ ex.Message);
            }
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (dataGridViewChild.SelectedCells.Count == 0)
            {
                MessageBox.Show(@"O linie trebuie selectata!");
                return;
            }

            if (dataGridViewChild.SelectedCells.Count > 1)
            {
                MessageBox.Show(@"O singura linie trebuie selectata!");
                return;
            }

            _dataAdapter.UpdateCommand = new SqlCommand(UpdateQuery, _cs);
            _dataAdapter.UpdateCommand.Parameters.Add("@id", SqlDbType.Int).Value =
                _dataSetChild.Tables[0].Rows[dataGridViewChild.CurrentCell.RowIndex][0];

            var args = ChildArr.Split(new []{", "} , StringSplitOptions.None);
            var types = ChildColumnTypes.Split(new []{", "} , StringSplitOptions.None);

            try
            {
                for (var i = 0; i < ChildNumberOfColumns; i++)
                {
                    switch (types[i])
                    {
                        case "string":
                            _dataAdapter.UpdateCommand.Parameters.Add(args[i + 1], SqlDbType.VarChar).Value =
                                _textBoxes[i].Text;
                            break;
                        case "int":
                            _dataAdapter.UpdateCommand.Parameters.Add(args[i + 1], SqlDbType.Int).Value =
                                int.Parse(_textBoxes[i].Text);
                            break;
                        case "float":
                            _dataAdapter.UpdateCommand.Parameters.Add(args[i + 1], SqlDbType.Float).Value =
                                float.Parse(_textBoxes[i].Text);
                            break;
                    }
                }

                _cs.Open();
                var x = _dataAdapter.UpdateCommand.ExecuteNonQuery();
                _cs.Close();
                _dataSetChild.Clear();
                _dataAdapter.Fill(_dataSetChild);
                if (x >= 1) MessageBox.Show(@"Updated!");
            }
            catch(Exception ex)
            {
                _cs.Close();
                MessageBox.Show(@"Input gresit! Error: "+ ex.Message);
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewChild.SelectedCells.Count == 0)
            {
                MessageBox.Show(@"Selectati o linie!");
                return;
            }

            if (dataGridViewChild.SelectedCells.Count > 1)
            {
                MessageBox.Show(@"Doar o linie trebuie selectata!");
                return;
            }

            _dataAdapter.DeleteCommand = new SqlCommand(DeleteQuery, _cs);

            _dataAdapter.DeleteCommand.Parameters.Add("@id",
                SqlDbType.Int).Value = _dataSetChild.Tables[0].Rows[dataGridViewChild.CurrentCell.RowIndex][0];

            _cs.Open();
            _dataAdapter.DeleteCommand.ExecuteNonQuery();
            _cs.Close();
            _dataSetChild.Clear();
            _dataAdapter.Fill(_dataSetChild);
        }

        private void buttonConnectDB_Click(object sender, EventArgs e)
        {
            _dataAdapter.SelectCommand = new SqlCommand("SELECT * FROM " + ParentName + ";", _cs);
            _dataSetParent.Clear();
            _dataAdapter.Fill(_dataSetParent);
            dataGridViewParent.DataSource = _dataSetParent.Tables[0];
            _bindingSourceParent.DataSource = _dataSetParent.Tables[0];
            dataGridViewParent.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _bindingSourceParent.MoveLast();
        }

        private void dataGridViewParent_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewParent.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null)
                return;

            var id = dataGridViewParent.Rows[e.RowIndex].Cells[0].Value.ToString();
            
            _dataAdapter.SelectCommand = new SqlCommand(
                $"SELECT * from {ChildName} where {ChildName}.{ChildToParentId} = {id}; ", _cs);
            _dataSetChild.Clear();
            _dataAdapter.Fill(_dataSetChild);
            dataGridViewChild.DataSource = _dataSetChild.Tables[0];
            _bindingSourceChild.DataSource = _dataSetChild.Tables[0];
            dataGridViewChild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
    }
}
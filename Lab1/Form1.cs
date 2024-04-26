using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Lab1
{
    public partial class Form1 : Form
    {
        private readonly SqlConnection _cs = new SqlConnection("Data Source = EMANUEL-LAPTOP\\SQLEXPRESS;" +
                                                               " Initial Catalog = CasaDeDiscuri; Integrated Security = True");

        private readonly SqlDataAdapter _dataAdapter = new SqlDataAdapter();
        private readonly BindingSource _bindingSourceParent = new BindingSource();
        private readonly BindingSource _bindingSourceChild = new BindingSource();
        private readonly DataSet _dataSetParent = new DataSet();
        private readonly DataSet _dataSetChild = new DataSet();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //throw new System.NotImplementedException();
        }

        private int GetMaxId()
        {
            var highestId = 0;
            const string query = "SELECT MAX(MelodieID) FROM Melodii";

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
            
            if (dataGridViewParent.SelectedCells.Count == 0)
            {
                MessageBox.Show(@"Un playlist trebuie selectat!");
                return;
            }

            if(dataGridViewParent.SelectedCells.Count > 1)
            {
                MessageBox.Show(@"Un singur playlist trebuie slectat!");
                return;
            }

            if (!int.TryParse(DurataTextBox.Text, out int durata))
            {
                MessageBox.Show(@"Durata trebuie sa fie un numar intreg!");
                return;
            }

            _dataAdapter.InsertCommand = new
                SqlCommand("INSERT INTO Melodii(MelodieID, Denumire, Durata, AlbumID, GenMuzicalID, PlayListID) VALUES (@id,@D,@T,@A,@G,@PlaylistID);", _cs);
            
            _dataAdapter.InsertCommand.Parameters.Add("@PlaylistID",
                SqlDbType.Int).Value = _dataSetParent.Tables[dataGridViewParent.CurrentCell.ColumnIndex].Rows[dataGridViewParent.CurrentCell.RowIndex][0];

            _dataAdapter.InsertCommand.Parameters.Add("@D",
                SqlDbType.VarChar).Value = DenumireTextBox.Text;

            _dataAdapter.InsertCommand.Parameters.Add("@T",
                SqlDbType.Int).Value = durata;
            
            _dataAdapter.InsertCommand.Parameters.Add("@id",
                SqlDbType.Int).Value = GetMaxId() + 1;

            var random = new Random();

            _dataAdapter.InsertCommand.Parameters.Add("@G",
                SqlDbType.Int).Value = random.Next(1, 3);
            
            _dataAdapter.InsertCommand.Parameters.Add("@A",
                SqlDbType.Int).Value = random.Next(1, 4);

            _cs.Open();
            _dataAdapter.InsertCommand.ExecuteNonQuery();
            _cs.Close();
            _dataSetChild.Clear();
            _dataAdapter.Fill(_dataSetChild);
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (dataGridViewChild.SelectedCells.Count == 0)
            {
                MessageBox.Show(@"O melodie trebuie selectata!");
                return;
            }
            else if (dataGridViewChild.SelectedCells.Count > 1)
            {
                MessageBox.Show(@"O singura melodie trebuie selectata!");
                return;
            }
            
            if (!int.TryParse(DurataTextBox.Text, out int durata))
            {
                MessageBox.Show(@"Durata trebuie sa fie un numar intreg!");
                return;
            }
            
            _dataAdapter.UpdateCommand = new SqlCommand("Update Melodii set Denumire = @D, Durata = @T where MelodieID = @id", _cs);

            _dataAdapter.UpdateCommand.Parameters.Add("@id",
                SqlDbType.Int).Value = _dataSetChild.Tables[0].Rows[dataGridViewChild.CurrentCell.RowIndex][0];

            _dataAdapter.UpdateCommand.Parameters.Add("@D",
                SqlDbType.VarChar).Value = DenumireTextBox.Text;

            _dataAdapter.UpdateCommand.Parameters.Add("@T",
                SqlDbType.Int).Value = durata;


            _cs.Open();
            var x = _dataAdapter.UpdateCommand.ExecuteNonQuery();
            _cs.Close();
            _dataSetChild.Clear();
            _dataAdapter.Fill(_dataSetChild);

            if (x >= 1)
                MessageBox.Show(@"The record has been updated");
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridViewChild.SelectedCells.Count == 0)
            {
                MessageBox.Show(@"Selectati o melodie!");
                return;
            }
            else if (dataGridViewChild.SelectedCells.Count > 1)
            {
                MessageBox.Show(@"Doar o melodie trebuie selectata!");
                return;
            }

            _dataAdapter.DeleteCommand = new SqlCommand("Delete from Melodii where MelodieID = @id;", _cs);

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
            _dataAdapter.SelectCommand = new SqlCommand("SELECT * FROM PlayList", _cs);
            _dataSetParent.Clear();
            _dataAdapter.Fill(_dataSetParent);
            dataGridViewParent.DataSource = _dataSetParent.Tables[0];
            _bindingSourceParent.DataSource = _dataSetParent.Tables[0];
            _bindingSourceParent.MoveLast();
        }

        private void dataGridViewParent_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewParent.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null)
                return;

            var idPlaylist = dataGridViewParent.Rows[e.RowIndex].Cells[0].Value.ToString();


            //Playlist
            _dataAdapter.SelectCommand = new SqlCommand("SELECT * from Melodii " +
                                              "where Melodii.PlayListID = " + idPlaylist + "; ", _cs);
            _dataSetChild.Clear();
            _dataAdapter.Fill(_dataSetChild);
            dataGridViewChild.DataSource = _dataSetChild.Tables[0];
            _bindingSourceChild.DataSource = _dataSetChild.Tables[0];
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using ProgramMethod;
using MySQLOperator;
using System.Data;

namespace SocketLocationApp
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public class PointList : List<string>
    {
        public PointList()
        {
            this.Add("");
            this.Add("IN");
            this.Add("OUT");
        }
    }
    public class LocationList : List<string>
    {
        public LocationList()
        {
            Add("");
            Add("GG");
            Add("YG");
            Add("YG-M");
            Add("YG-E");
            Add("IG");
            Add("IG-M");
            Add("HP");
            Add("QC1");
            Add("QC2");
            Add("QC2-YGIG");
            Add("FQC");
        }
    }
    public partial class MainWindow : Window
    {
        private MySqlConnection Conn;
        private FileMethod PGMethod = new FileMethod();
        private SocketLocationManage SLManage = new SocketLocationManage();
        private DBMethod MySQL = new DBMethod();
        bool isInsert = false;
        bool CanInsert = false;
        ObservableCollection<MyLocation> Socketdata;
        ObservableCollection<MyLeonardo> Leonardodata;
        public MainWindow()
        {
            InitializeComponent();
            
        }
        class MyLocation
        {
            public int id { get; set; }
            public string ip_address { get; set; }
            public string location { get; set; }
            public string readerno { get; set; }
            public string pointname { get; set; }
            public string create_user { get; set; }
            public string update_user { get; set; }
            public DateTime create_dt { get; set; }
            public DateTime update_dt { get; set; }
        }
        class MyLeonardo
        {
            public string ip_address { get; set; }
            public string MachineNumber { get; set; }
            public string Station { get; set; }
            public int Intervals { get; set; }
            public string create_user { get; set; }
            public string update_user { get; set; }
            public DateTime create_dt { get; set; }
            public DateTime update_dt { get; set; }
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayData();
        }
        private void DisplayData()
        {
            DG_socket.ItemsSource = null;
            DG_leonardo.ItemsSource = null;
            Socketdata = new ObservableCollection<MyLocation>();
            Leonardodata = new ObservableCollection<MyLeonardo>();
            Conn = MySQL.getConnect();
            string SelectSQL = @"SELECT id, ip_address, location, readerno, pointname, create_user, update_user, create_dt, update_dt FROM socketlocation ";
            Conn.Open();
            MySqlCommand cmd = new MySqlCommand(SelectSQL, Conn);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                MyLocation Location = new MyLocation();
                Location.id = dataReader.GetInt32(0);
                Location.ip_address = dataReader.GetString(1);
                Location.location = dataReader.GetString(2);
                Location.readerno = dataReader.GetString(3);
                Location.pointname = dataReader.GetString(4);
                Location.create_user = dataReader.GetString(5);
                Location.update_user = dataReader.GetString(6);
                Location.create_dt = dataReader.GetDateTime(7);
                Location.update_dt = dataReader.GetDateTime(8);
                Socketdata.Add(Location);
            }
            dataReader.Close();
            DG_socket.ItemsSource = Socketdata;
            Conn.Close();
            SelectSQL = @"SELECT ip_address, MachineNumber, Station, Intervals, create_user, update_user, create_dt, update_dt FROM ldsetting ";
            Conn.Open();
            cmd = new MySqlCommand(SelectSQL, Conn);
            dataReader = cmd.ExecuteReader();
            while (dataReader.Read())
            {
                MyLeonardo Leonardo = new MyLeonardo();
                Leonardo.ip_address = dataReader.GetString(0);
                Leonardo.MachineNumber = dataReader.GetString(1);
                Leonardo.Station = dataReader.GetString(2);
                Leonardo.Intervals = dataReader.GetInt32(3);
                Leonardo.create_user = dataReader.GetString(4);
                Leonardo.update_user = dataReader.GetString(5);
                Leonardo.create_dt = dataReader.GetDateTime(6);
                Leonardo.update_dt = dataReader.GetDateTime(7);
                Leonardodata.Add(Leonardo);
            }
            dataReader.Close();
            DG_leonardo.ItemsSource = Leonardodata;
            Conn.Close();
        }
        private void DG_socket_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            isInsert = true;
        }
        private void DG_leonardo_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            CanInsert = true;
        }
        private void DG_socket_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            isInsert = false;
        }
        private void DG_leonardo_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            CanInsert = false;
        }
        private void DG_socket_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            MyLocation locationInfo = e.Row.Item as MyLocation;
            if (locationInfo != null)
            {
                if (locationInfo.id > 0)
                {
                    isInsert = false;
                }
            }
            if (!CheckData(locationInfo))
            {
                DisplayData();
                return;
            }
            if (isInsert)
            {
                if (!InsertInfo(locationInfo, Conn))
                {
                    MessageBox.Show("新增失敗", "Add Socket Location", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("新增成功", "Add Socket Location", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                if (!UpdateInfo(locationInfo, Conn))
                {
                    MessageBox.Show("修改失敗", "Update Socket Location", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("修改成功", "Update Socket Location", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            DisplayData();
        }
        private void DG_leonardo_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            MyLeonardo LeonardoInfo = e.Row.Item as MyLeonardo;
            if (LeonardoInfo != null)
            {
                if (LeonardoInfo.ip_address == "")
                {
                    CanInsert = false;
                }
            }
            if (!CheckLeonardoData(LeonardoInfo))
            {
                DisplayData();
                return;
            }
            if (CanInsert)
            {
                if (!InsertLeonardoInfo(LeonardoInfo, Conn))
                {
                    MessageBox.Show("新增失敗", "Add Leonardo Machine", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("新增成功", "Add Leonardo Machine", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                if (!UpdateLeonardoInfo(LeonardoInfo, Conn))
                {
                    MessageBox.Show("修改失敗", "Update Leonardo Machine", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("修改成功", "Update Leonardo Machine", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            DisplayData();
        }
        private void Delete_Socket(object sender, ExecutedRoutedEventArgs e)
        {
            MyLocation DeleteRow = DG_socket.SelectedItem as MyLocation;
            if (DeleteRow != null)
            {
                if (e.Command == DataGrid.DeleteCommand)
                {
                   if (MessageBox.Show("Are you sure want to delete id " + DeleteRow.id + " ?", "Confirm Delete!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        string DeleteSQL = @"DELETE FROM socketlocation WHERE id = @id";
                        try
                        {
                            Conn.Open();
                            MySqlCommand cmd = new MySqlCommand(DeleteSQL, Conn);
                            MySqlTransaction trans = Conn.BeginTransaction();
                            cmd.Transaction = trans;
                            cmd.Parameters.AddWithValue("@id", DeleteRow.id);
                            cmd.ExecuteNonQueryAsync();
                            trans.Commit();
                            cmd.Cancel();
                            Conn.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to delete socketlocation table. " + ex.Message);
                            e.Handled = true;
                        }
                        finally
                        {
                            Conn.Close();
                        }
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }
            }
        }
        private void Delete_Leonardo(object sender, ExecutedRoutedEventArgs e)
        {
            MyLeonardo DeleteRow = DG_leonardo.SelectedItem as MyLeonardo;
            if (DeleteRow != null)
            {
                if (e.Command == DataGrid.DeleteCommand)
                {
                    if (MessageBox.Show("Are you sure want to delete id " + DeleteRow.ip_address + " ?", "Confirm Delete!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        string DeleteSQL = @"DELETE FROM ldsetting WHERE ip_address = @ip_address";
                        try
                        {
                            Conn.Open();
                            MySqlCommand cmd = new MySqlCommand(DeleteSQL, Conn);
                            MySqlTransaction trans = Conn.BeginTransaction();
                            cmd.Transaction = trans;
                            cmd.Parameters.AddWithValue("@ip_address", DeleteRow.ip_address);
                            cmd.ExecuteNonQueryAsync();
                            trans.Commit();
                            cmd.Cancel();
                            Conn.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to delete ldsetting table. " + ex.Message);
                            e.Handled = true;
                        }
                        finally
                        {
                            Conn.Close();
                        }
                    }
                    else
                    {
                        e.Handled = true;
                    }
                }
            }
        }
        private bool InsertInfo(MyLocation locationInfo, MySqlConnection Comm)
        {
            string InsertSQL = @"insert into socketlocation (ip_address, location, readerno, pointname, create_user, update_user, create_dt, update_dt) 
                values (@ip_address, @location, @readerno, @pointname, @create_user, @update_user, @create_dt, @update_dt)";
            try
            {
                Comm.Open();
                MySqlCommand cmd = new MySqlCommand(InsertSQL, Comm);
                MySqlTransaction trans = Comm.BeginTransaction();
                cmd.Transaction = trans;
                cmd.Parameters.AddWithValue("@ip_address", locationInfo.ip_address);
                cmd.Parameters.AddWithValue("@location", locationInfo.location);
                cmd.Parameters.AddWithValue("@readerno", (locationInfo.readerno == null)?"": locationInfo.readerno);
                cmd.Parameters.AddWithValue("@pointname", (locationInfo.pointname == null)?"": locationInfo.pointname);
                cmd.Parameters.AddWithValue("@create_user", "System");
                cmd.Parameters.AddWithValue("@update_user", "System");
                cmd.Parameters.AddWithValue("@create_dt", DateTime.Now);
                cmd.Parameters.AddWithValue("@update_dt", DateTime.Now);
                cmd.ExecuteNonQueryAsync();
                trans.Commit();
                cmd.Cancel();
                Comm.Close();
                return true;
            }
            catch (Exception ex)
            {
                PGMethod.WriteLog("Failed to add socketlocation table. " + ex.Message);
                return false;
            }
        }
        private bool InsertLeonardoInfo(MyLeonardo LeonardoInfo, MySqlConnection Comm)
        {
            string InsertSQL = @"insert into ldsetting (ip_address, MachineNumber, Station, Intervals, create_user, update_user, create_dt, update_dt) 
                values (@ip_address, @MachineNumber, @Station, @Intervals, @create_user, @update_user, @create_dt, @update_dt)";
            try
            {
                Comm.Open();
                MySqlCommand cmd = new MySqlCommand(InsertSQL, Comm);
                MySqlTransaction trans = Comm.BeginTransaction();
                cmd.Transaction = trans;
                cmd.Parameters.AddWithValue("@ip_address", LeonardoInfo.ip_address);
                cmd.Parameters.AddWithValue("@MachineNumber", LeonardoInfo.MachineNumber);
                cmd.Parameters.AddWithValue("@Station", LeonardoInfo.Station);
                cmd.Parameters.AddWithValue("@Intervals", LeonardoInfo.Intervals);
                cmd.Parameters.AddWithValue("@create_user", "System");
                cmd.Parameters.AddWithValue("@update_user", "System");
                cmd.Parameters.AddWithValue("@create_dt", DateTime.Now);
                cmd.Parameters.AddWithValue("@update_dt", DateTime.Now);
                cmd.ExecuteNonQueryAsync();
                trans.Commit();
                cmd.Cancel();
                Comm.Close();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to add ldsetting table. " + ex.Message);
                return false;
            }
        }
        private bool UpdateInfo(MyLocation locationInfo, MySqlConnection Comm)
        {
            string UpdateSQL = @"UPDATE socketlocation SET ip_address = @ip_address, location = @location, readerno = @readerno, pointname = @pointname, 
                update_user = @update_user, update_dt = @update_dt WHERE id = @id";
            try
            {
                Comm.Open();
                MySqlCommand cmd = new MySqlCommand(UpdateSQL, Comm);
                MySqlTransaction trans = Comm.BeginTransaction();
                cmd.Transaction = trans;
                cmd.Parameters.AddWithValue("@ip_address", locationInfo.ip_address);
                cmd.Parameters.AddWithValue("@location", locationInfo.location);
                cmd.Parameters.AddWithValue("@readerno", locationInfo.readerno);
                cmd.Parameters.AddWithValue("@pointname", locationInfo.pointname);
                cmd.Parameters.AddWithValue("@update_user", "MODIFYER");
                cmd.Parameters.AddWithValue("@update_dt", DateTime.Now);
                cmd.Parameters.AddWithValue("@id", locationInfo.id);
                cmd.ExecuteNonQueryAsync();
                trans.Commit();
                cmd.Cancel();
                Comm.Close();
                return true;
            }
            catch (Exception ex)
            {
                PGMethod.WriteLog("Failed to update socketlocation table. " + ex.Message);
                return false;
            }
        }
        private bool UpdateLeonardoInfo(MyLeonardo LeonardoInfo, MySqlConnection Comm)
        {
            string UpdateSQL = @"UPDATE ldsetting SET MachineNumber = @MachineNumber, Station = @Station, Intervals = @Intervals, 
                update_user = @update_user, update_dt = @update_dt WHERE ip_address = @ip_address";
            try
            {
                Comm.Open();
                MySqlCommand cmd = new MySqlCommand(UpdateSQL, Comm);
                MySqlTransaction trans = Comm.BeginTransaction();
                cmd.Transaction = trans;
                cmd.Parameters.AddWithValue("@MachineNumber", LeonardoInfo.MachineNumber);
                cmd.Parameters.AddWithValue("@Station", LeonardoInfo.Station);
                cmd.Parameters.AddWithValue("@Intervals", LeonardoInfo.Intervals);
                cmd.Parameters.AddWithValue("@update_user", "MODIFYER");
                cmd.Parameters.AddWithValue("@update_dt", DateTime.Now);
                cmd.Parameters.AddWithValue("@ip_address", LeonardoInfo.ip_address);
                cmd.ExecuteNonQueryAsync();
                trans.Commit();
                cmd.Cancel();
                Comm.Close();
                return true;
            }
            catch (Exception ex)
            {
                PGMethod.WriteLog("Failed to update ldsetting table. " + ex.Message);
                return false;
            }
        }
        private bool CheckData(MyLocation locationInfo)
        {
            string ErrorMsg = "";
            if (string.IsNullOrEmpty(locationInfo.ip_address)) ErrorMsg += "IP位址必需輸入\n";
            if (string.IsNullOrEmpty(locationInfo.location)) ErrorMsg += "加工站別必需輸入\n";
            if (string.IsNullOrEmpty(locationInfo.readerno)) ErrorMsg += "機台名稱必需輸入\n";
            string SelectSQL = @"SELECT id FROM socketlocation WHERE ip_address = @ip_address";
            Conn.Open();
            MySqlCommand cmd = new MySqlCommand(SelectSQL, Conn);
            cmd.Parameters.AddWithValue("@ip_address", locationInfo.ip_address);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            if (dataReader.HasRows)
            {
                while(dataReader.Read())
                {
                    if (dataReader.GetInt32(0) != locationInfo.id) ErrorMsg += "IP位址已存在\n";
                    break;
                }
            }
            Conn.Close();
            Conn.Open();
            SelectSQL = @"SELECT id FROM socketlocation WHERE location = @location AND readerno = @readerno AND pointname = @pointname ";
            cmd = new MySqlCommand(SelectSQL, Conn);
            cmd.Parameters.AddWithValue("@location", locationInfo.location);
            cmd.Parameters.AddWithValue("@readerno", locationInfo.readerno);
            cmd.Parameters.AddWithValue("@pointname", locationInfo.pointname);
            dataReader = cmd.ExecuteReader();
            if (dataReader.HasRows)
            {
                while(dataReader.Read())
                {
                    if (dataReader.GetInt32(0) != locationInfo.id) ErrorMsg += "加工站別 & 機台名稱 & 出入口定義已存在\n";
                    break;
                }
            }
            Conn.Close();
            if (!string.IsNullOrEmpty(ErrorMsg))
            {
                MessageBox.Show(ErrorMsg, "Check Data", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
        private bool CheckLeonardoData(MyLeonardo LeonardoInfo)
        {
            string ErrorMsg = "";
            if (string.IsNullOrEmpty(LeonardoInfo.ip_address)) ErrorMsg += "IP位址必需輸入\n";
            if (string.IsNullOrEmpty(LeonardoInfo.MachineNumber)) ErrorMsg += "機台名稱必需輸入\n";
            if (string.IsNullOrEmpty(LeonardoInfo.Station)) ErrorMsg += "站別必需輸入\n";
            if (LeonardoInfo.Intervals == 0) ErrorMsg += "間隔時間必需輸入\n";
            string SelectSQL = @"SELECT ip_address FROM socketlocation WHERE ip_address = @ip_address";
            Conn.Open();
            MySqlCommand cmd = new MySqlCommand(SelectSQL, Conn);
            cmd.Parameters.AddWithValue("@ip_address", LeonardoInfo.ip_address);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    if (dataReader.GetString(0) != LeonardoInfo.ip_address) ErrorMsg += "IP位址已存在\n";
                    break;
                }
            }
            Conn.Close();
            Conn.Open();
            SelectSQL = @"SELECT ip_address FROM ldsetting WHERE MachineNumber = @MachineNumber AND Station = @Station ";
            cmd = new MySqlCommand(SelectSQL, Conn);
            cmd.Parameters.AddWithValue("@MachineNumber", LeonardoInfo.MachineNumber);
            cmd.Parameters.AddWithValue("@Station", LeonardoInfo.Station);
            dataReader = cmd.ExecuteReader();
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    if (dataReader.GetString(0) != LeonardoInfo.ip_address) ErrorMsg += "機台名稱 & 站別 已存在\n";
                    break;
                }
            }
            Conn.Close();
            if (!string.IsNullOrEmpty(ErrorMsg))
            {
                MessageBox.Show(ErrorMsg, "Check Data", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}

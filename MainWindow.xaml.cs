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
            Add("YG");
            Add("YG-M");
            Add("IG");
            Add("IG-M");
            Add("HP");
            Add("QC1");
            Add("QC2");
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
        ObservableCollection<MyLocation> data;
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayData();
        }
        private void DisplayData()
        {
            DG_socket.ItemsSource = null;
            data = new ObservableCollection<MyLocation>();
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
                data.Add(Location);
            }
            DG_socket.ItemsSource = data;
            Conn.Close();
        }
        private void DG_socket_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            isInsert = true;
        }
        private void DG_socket_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            isInsert = false;
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
        private void bt_edit_Click(object sender, RoutedEventArgs e)
        {

            MessageBox.Show("Can't uploaded photo, Add the employee first!", "Add Employee", MessageBoxButton.OK, MessageBoxImage.Information);
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
            SelectSQL = @"SELECT id FROM socketlocation WHERE location = @location AND readerno = @readerno ";
            cmd = new MySqlCommand(SelectSQL, Conn);
            cmd.Parameters.AddWithValue("@location", locationInfo.location);
            cmd.Parameters.AddWithValue("@readerno", locationInfo.readerno);
            dataReader = cmd.ExecuteReader();
            if (dataReader.HasRows)
            {
                while(dataReader.Read())
                {
                    if (dataReader.GetInt32(0) != locationInfo.id) ErrorMsg += "加工站別 & 機台名稱已存在\n";
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

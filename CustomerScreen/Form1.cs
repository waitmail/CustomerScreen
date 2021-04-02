using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;
using System.IO;

namespace CustomerScreen
{
    public partial class Form1 : Form
    {
        //public delegate void load_datain_datagrid(DataTable dataTable);
        public delegate void set_data_in_cash_programm(DataTable dataTable);
        private System.Timers.Timer timer = new System.Timers.Timer();
        private List<String> ListPathPictures = new List<string>();
        private int currentIndex = 0;
        Thread receiveThread = null;
        private bool runung = true;
        private UdpClient receiver = null;


        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            this.Shown += Form1_Shown;
            timer.Interval = 30000;
            timer.Elapsed += Timer_Elapsed;

        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (ListPathPictures.Count > 0)
            {
                currentIndex++;
                if (currentIndex > ListPathPictures.Count - 1)
                {
                    currentIndex = 0;
                }
                pictureBox1.Load(ListPathPictures[currentIndex]);
            }
        }

        public class CustomerScreen
        {
            public List<CheckPosition> ListCheckPositions { get; set; }
        }

        public class CheckPosition
        {
            public string NamePosition { get; set; }
            public string Quantity { get; set; }
            public string Price { get; set; }
        }
       

        private void Form1_Shown(object sender, EventArgs e)
        {
            receiveThread = new Thread(new ThreadStart(ReceiveMessage));
            receiveThread.Start();            
            timer.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //pictureBox1.Load("IMG_9198.jpg");
            //MessageBox.Show(Application.StartupPath+"\\"+ "\\Pictures\\706 на 663  сделай весну на максимум.jpg");
            //pictureBox1.Load(Application.StartupPath + "\\" + "\\Pictures\\706 на 663  сделай весну на максимум.jpg");
            DirectoryInfo dir = new DirectoryInfo(Application.StartupPath + "\\" + "\\Pictures");
            foreach (FileInfo file in dir.GetFiles("*.jpg"))
            {
                ListPathPictures.Add(file.FullName);                
            }
            if (ListPathPictures.Count > 0)
            {
                pictureBox1.Load(ListPathPictures[currentIndex]);
            }

        }
        
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                runung = false;
                receiver.Close();                
                timer.Stop();
                timer.Dispose();
                this.Close();
                this.Dispose();                
            }         
        }

        public void set_data_in_cash_programm_in_datagrid(DataTable dataTable)
        {            
            dataGridView1.DefaultCellStyle.Font = new Font("Microsoft Sans Serif", 14);
            dataGridView1.DataSource = dataTable;
            dataGridView1.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridView1.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            

            dataGridView1.Columns[0].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.Columns[0].Width = 450;
            dataGridView1.Columns[1].Width = 70;
            dataGridView1.Columns[2].Width = 100;         
            dataGridView1.Columns[2].DefaultCellStyle.Format = "##.00";

            dataGridView1.ClearSelection();
            decimal to_pay = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                to_pay += (Convert.ToDecimal(row[1].ToString()) * Convert.ToDecimal(row[2].ToString()));
            }
            lbl_sum.Text = to_pay.ToString("### ### ###.##");
        }

        public void load_datain_datagrid(DataTable dataTable)
        {                      
            Invoke(new set_data_in_cash_programm(set_data_in_cash_programm_in_datagrid), new object[] { dataTable });         
        }

        private void ReceiveMessage()
        {
            //UdpClient 
            receiver = new UdpClient(12345); // UdpClient для получения данных
            IPEndPoint remoteIp = null; // адрес входящего подключения
            try
            {
                while (runung)
                {
                    byte[] data = receiver.Receive(ref remoteIp); // получаем данные
                    string message = Encoding.UTF8.GetString(data);
                    //Console.WriteLine("Собеседник: {0}", message);                    
                    CustomerScreen customerScreen = JsonConvert.DeserializeObject<CustomerScreen>(message);
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("Наименование");
                    dataTable.Columns.Add("Количество", new Int32().GetType());
                    dataTable.Columns.Add("Цена", new Decimal().GetType());                    
                    foreach (CheckPosition checkPosition in customerScreen.ListCheckPositions)
                    {
                        DataRow row = dataTable.NewRow();
                        row[0] = checkPosition.NamePosition;
                        row[1] = checkPosition.Quantity;
                        row[2] = checkPosition.Price;                        
                        dataTable.Rows.Add(row);                        
                    }                                        
                    load_datain_datagrid(dataTable);                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                receiver.Close();
            }
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }
    }
}

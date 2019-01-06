using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Web.Script.Serialization;
using System.IO.Ports;
using System.Media;
using System.Net.Http.Headers;
using System.Threading;


namespace it_storage
{

    public partial class Form1 : Form
    {

        static string comPortData;


        public class Employee
        {
            public string fio { get; set; }
            public string barecode { get; set; }
            public string name { get; set; }
            public string rank { get; set; }
            public string phone { get; set; }
        }

        public class Com
        {
            private SerialPort comPort;

            public Com()
            {
                this.comPort = new SerialPort("COM4", 9600, Parity.None, 8, StopBits.One);
                try
                {
                    comPort.Open();
                }
                catch (Exception)
                {
                    throw new Exception();
                }
            }

            public string ComRun()
            {
                while (true)
                {
                    string employeeSn = comPort.ReadExisting();
                    if (employeeSn.Length > 13)
                    {
                        employeeSn = employeeSn.Substring(1, 12);
                        //comPort.Close();
                        if (employeeSn != comPortData)
                        return employeeSn;
                    }
                }
            }
        }
        private static async Task<string> GetPostData(/*string dataComPort*/)
        {   
            using (HttpClient client = new HttpClient())
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                List<Employee> employees = new List<Employee>();
                Employee emp = new Employee();
                employees.Add(new Employee()
                {
                    /*barecode = dataComPort.ToString()*/
                    barecode = "проверка"
                });
                string jsonEmployees = serializer.Serialize(employees);
                HttpResponseMessage result = await client.PostAsync("http://127.0.0.1:8000/api/v1/employee", new StringContent(jsonEmployees, Encoding.UTF8, "application/json"));
                string resultContent = await result.Content.ReadAsStringAsync();
                return resultContent;
            }
          
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Com comPort = new Com();
            TaskScheduler uiContext = TaskScheduler.FromCurrentSynchronizationContext();
           Task task = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Task<string> promise = Task.Run(() => GetPostData());
                    string dataComPort = comPort.ComRun();
                    Task.Factory.StartNew(() =>
                    {
                        SystemSounds.Beep.Play();
                        comPortData = dataComPort;
                    }, CancellationToken.None, TaskCreationOptions.None, uiContext);
                }
            });

        }

        private void button3_Click(object sender, EventArgs e)
        {
            int i = dataGridView1.RowCount - 1;
            if (i >= 0)
                this.dataGridView1.Rows.RemoveAt(i);
        }

        private void SendDataBtn_Click(object sender, EventArgs e)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Employee emp = new Employee();
            string json_employees;
            //формируем массив с сотрудниками для передачи данных на сервер
            List<Employee>  employees= new List<Employee>();

            for (int j = 0; j < this.dataGridView1.RowCount; j++)
            {
                employees.Add(new Employee() { fio = "fio"+j.ToString(), barecode = "barecode" + j.ToString(), name = "name" + j.ToString(),
                                               rank = "rank" + j.ToString(), phone = "phone" + j.ToString() });
            }

           json_employees = serializer.Serialize(employees);
            /*string getResult = "";
            Task<string> promise = Task.Run(() => GetPostData(json_employees));
            getResult = promise.Result;*/
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}

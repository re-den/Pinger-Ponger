using Pinger;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp3
{

    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            //chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Spline;
            timer1_Tick(null, null);
        }


        bool enableTimer = false;


        private void button1_Click(object sender, EventArgs e)
        {
            if (enableTimer)
            {
                timer1.Start();
                chart1.Series[0].Points.Clear();
                chart1.Series[1].Points.Clear();
                maximum = maximum_eff = 0;
                minimum = minimum_eff = 10000;
                average = average_eff = average_ping1 = average_ping2 = 0;
                button1.Text = "Остановить";

                count = count_eff = 0;
                textBox1.Enabled = textBox3.Enabled = false;
            }
            else
            {
                timer1.Stop();
                textBox1.Enabled = textBox3.Enabled = true;
                button1.Text = "Запустить";
            }
            enableTimer = !enableTimer;

        }


        public int count = 0;
        public int count_eff = 0;
        public long average;
        public long minimum = 10000;
        public long maximum;
        public long average_eff;
        public long minimum_eff = 10000;
        public long maximum_eff;
        public long average_ping1 = 0;
        public long average_ping2 = 0;
        public int period;

        private long moving_avg(long ma, long pingResult, int count)
        {
            ma = ma * (count / (count + 1)) + pingResult / (count + 1);
            
            return ma;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            nt first = new nt();
            long Result = first.ping(textBox1.Text);

            if (period != 0)
                if (chart1.Series[0].Points.Count > period/numericUpDown1.Value)
                    chart1.Series[0].Points.RemoveAt(0);    
            
            chart1.Series[0].Points.AddXY(DateTime.Now.ToString("HH:mm:ss"), Result);

            if (Result > maximum)
                maximum = Result;
            if (Result < minimum)
                minimum = Result;
            //average = moving_avg(average,Result,count);
            count++;
            average_ping1 += Result;
            average = average_ping1 / count;


            label3.Text = count.ToString();

            chart1.Series[0].LegendText = textBox1.Text;
            label1.Text = $"Текущее:{Result}  Min:{minimum}  Среднее:{average}  Макс:{maximum}";

            long Result_eff = first.ping(textBox3.Text);

            if (period!=0)
                if (chart1.Series[1].Points.Count > period / numericUpDown1.Value)
                    chart1.Series[1].Points.RemoveAt(0);

            chart1.Series[1].Points.AddXY(DateTime.Now.ToString("HH:mm:ss"), Result_eff);

            if (Result_eff > maximum_eff)
                maximum_eff = Result_eff;
            if (Result_eff < minimum_eff)
                minimum_eff = Result_eff;

            count_eff++;
            average_ping2 += Result_eff;
            average_eff = average_ping2 / count_eff;


            label4.Text = count_eff.ToString();
            //label2.Text = "Всего пакетов " + textBox3.Text;
            chart1.Series[1].LegendText = textBox3.Text;
            label6.Text = $"Текущее:{Result_eff}  Min:{minimum_eff}  Среднее:{average_eff}  Макс:{maximum_eff}";

            if (checkBox1.Checked)
            {
                LogWrite($"{Application.StartupPath}/{DateTime.Now:yyyyMMdd}-{textBox1.Text}.log", $"{count};{textBox1.Text};{Result};{minimum};{maximum}");
                LogWrite($"{Application.StartupPath}/{DateTime.Now:yyyyMMdd}-{textBox3.Text}.log", $"{count_eff};{textBox3.Text};{Result_eff};{minimum_eff};{maximum_eff}");
            }
        }

        public static bool LogWrite(string fileName, string msg)
        {

            try
            {
                File.AppendAllText($"{fileName}", $"{DateTime.Now:HH:mm:ss}: {msg}{Environment.NewLine}");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval = (int)numericUpDown1.Value * 1000;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox2.Clear();
            nt tracer = new nt();
            textBox2.Text = tracer.tracert(textBox1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox2.Clear();
            nt tracer = new nt();
            textBox2.Text = tracer.tracert(textBox3.Text);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        

        private void button4_Click(object sender, EventArgs e)
        {
            //StringBuilder ansv = new StringBuilder();
            ////string name = Dns.GetHostEntry(Dns.GetHostName());
            //IPHostEntry iP = Dns.GetHostEntry(textBox1.Text);
            //IPAddress ip = Dns.GetHostByName(textBox1.Text).AddressList[0];
            ////ansv.AppendLine(ip.ToString());

            //nt mac = new nt();

            ////IPAddress IP = IPAddress.Parse("192.168.1.104");
            //ansv.AppendLine(textBox1.Text + ": " + ip.ToString() + " - " + mac.ConvertIpToMAC(ip));




            //IPAddressCollection iPAddresses = new IPAddressCollection;

            //foreach (var item in collection)
            //{

            //}
            
            //textBox2.AppendText(Environment.NewLine + ansv.ToString());

        }

        public static IPAddress GetSubnetMask(IPAddress address)
        {
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation unicastIPAddressInformation in adapter.GetIPProperties().UnicastAddresses)
                {
                    if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (address.Equals(unicastIPAddressInformation.Address))
                        {
                            return unicastIPAddressInformation.IPv4Mask;
                        }
                    }
                }
            }
            throw new ArgumentException($"Can't find subnetmask for IP address '{address}'");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            
            //textBox2.AppendText(Environment.NewLine + getIpMac());
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            period = (int)numericUpDown2.Value;
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
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
            timer1_Tick(null,null);
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
                average = average_eff = 0;
                count = count_eff = 1;
                textBox1.Enabled = textBox3.Enabled = false;
            }
            else
            {
                timer1.Stop();
                textBox1.Enabled = textBox3.Enabled = true;
            }
            enableTimer = !enableTimer;

        }

        private long Ping_Srv(string address)
        {
            long PingResult;
            try
            {
                Ping ping = new Ping();
                PingReply reply = ping.Send(address, 10000);
                PingResult = reply.RoundtripTime;
            }
            catch (Exception)
            {
                PingResult = -1;
            }
            return PingResult;
        }
        public int count = 1;
        public int count_eff = 1;
        public long average;
        public long minimum=10000;
        public long maximum;
        public long average_eff;
        public long minimum_eff = 10000;
        public long maximum_eff;
        private void timer1_Tick(object sender, EventArgs e)
        {
            long Result = Ping_Srv(textBox1.Text);
            chart1.Series[0].Points.AddXY(DateTime.Now.ToString("HH:mm:ss"), Result);
            if (Result > maximum)
                maximum = Result;
            if (Result < minimum)
                minimum = Result;
            average = (average + Result) / 2;
            count++;
            label3.Text = count.ToString();
            //label2.Text = "Всего пакетов " + textBox1.Text;
            chart1.Series[0].LegendText = textBox1.Text;
            label1.Text = $"Текущее:{Result}  Min:{minimum}  Среднее:{average}  Макс:{maximum}";

            long Result_eff = Ping_Srv(textBox3.Text);
            chart1.Series[1].Points.AddXY(DateTime.Now.ToString("HH:mm:ss"), Result_eff);
            if (Result_eff > maximum_eff)
                maximum_eff = Result_eff;
            if (Result_eff < minimum_eff)
                minimum_eff = Result_eff;
            average = (average_eff + Result_eff) / 2;
            count_eff++;
            label4.Text = count_eff.ToString();
            //label2.Text = "Всего пакетов " + textBox3.Text;
            chart1.Series[1].LegendText = textBox3.Text;
            label6.Text = $"Текущее:{Result_eff}  Min:{minimum_eff}  Среднее:{average_eff}  Макс:{maximum_eff}";

            if (checkBox1.Checked)
            {
                LogWrite($"{Application.StartupPath}/{DateTime.Now:yyyyMMdd}-{textBox1.Text}.log",$"{count};{textBox1.Text};{Result};{minimum};{maximum}");            
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
            textBox2.Text=Traceroute(textBox1.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox2.Clear();
            textBox2.Text = Traceroute(textBox3.Text);
        }

        public string Traceroute(string ipAddressOrHostName)
        {
            Cursor = Cursors.WaitCursor;
            IPAddress ipAddress = Dns.GetHostEntry(ipAddressOrHostName).AddressList[0];
            StringBuilder traceResults = new StringBuilder();
            using (Ping pingSender = new Ping())
            {
                PingOptions pingOptions = new PingOptions();
                Stopwatch stopWatch = new Stopwatch();
                byte[] bytes = new byte[32];
                pingOptions.DontFragment = true;
                pingOptions.Ttl = 1;
                int maxHops = 30;
                traceResults.AppendLine($"Начало трассировки: {DateTime.Now:HH:mm:ss}{Environment.NewLine}Трассировка до: {ipAddressOrHostName}({ipAddress}){Environment.NewLine}Максимальное число прыжков {maxHops}{Environment.NewLine}");
                //traceResults.AppendLine(string.Format(, ipAddress, maxHops));

                traceResults.AppendLine();
                for (int i = 1; i < maxHops + 1; i++)
                {
                    stopWatch.Reset();
                    stopWatch.Start();
                    PingReply pingReply = pingSender.Send(ipAddress, 5000, new byte[32], pingOptions);
                    stopWatch.Stop();
                    traceResults.AppendLine($"{i}\t{stopWatch.ElapsedMilliseconds} ms\t{pingReply.Address}");
                    //traceResults.AppendLine(string.Format(, , , ));

                    if (pingReply.Status == IPStatus.Success)
                    {
                        traceResults.AppendLine();
                        traceResults.AppendLine($"Трассировки завершена: {DateTime.Now:HH:mm:ss}");
                        break;
                    }
                    pingOptions.Ttl++;
                }
            }
            Cursor = Cursors.Default;
            return traceResults.ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

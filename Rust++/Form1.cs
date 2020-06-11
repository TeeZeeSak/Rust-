using System;
using System.Drawing;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Rust__
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);

        public static bool bMaximized = false;

        public static string szUrl = null;
        public static string szConnectUrl = null;

        public static bool bFading = false;

        public static bool bCanChange = true;

        public static bool bGetCommands = false;

        public bool bEnabled = true;
        public Form1()
        {
            InitializeComponent();
        }

        void getData()
        {
            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString("https://api.battlemetrics.com/servers?filter[game]=rust&filter[status]=online&sort=-details.rust_last_wipe&page[size]=1");

                dynamic server = JsonConvert.DeserializeObject(json);

                if (Properties.Settings.Default.lastServer != (string)server.data[0].id && bCanChange)
                {
                    string ip = (string)server.data[0].attributes.ip;
                    string ping = PingTimeAverage(ip, 10);
                    label2.Text = server.data[0].attributes.name;
                    label4.Text = server.data[0].attributes.players + "/" + server.data[0].attributes.maxPlayers;
                    label5.Text = server.data[0].attributes.details.rust_queued_players;
                    label7.Text = server.data[0].attributes.details.map;
                    label9.Text = "#" + server.data[0].attributes.rank;
                    label11.Text = server.data[0].attributes.country;
                    label13.Text = (bool)server.data[0].attributes.details.official ? "YES" : "NO";
                    if(ping != "0")
                    {
                        label15.Text = ping;
                    }
                    else
                    {
                        label15.Text = "N/A";
                    }

                    if ((bool)server.data[0].attributes.details.official)
                    {
                        label13.Text = "Official";
                    }
                    else
                    {
                        if (server.data[0].attributes.details.rust_type == "modded")
                        {
                            label13.Text = "Modded";
                        }
                        else
                        {
                            label13.Text = "Vanilla";
                        }
                    }
                    richTextBox1.Text = server.data[0].attributes.details.rust_description;
                    szUrl = server.data[0].attributes.details.rust_url;
                    szConnectUrl = "steam://connect/" + server.data[0].attributes.ip + ":" + server.data[0].attributes.port;

                    Properties.Settings.Default.lastServer = server.data[0].id;
                    Properties.Settings.Default.Save();

                    timer1.Start();
                    this.Visible = true;
                }
                else
                {
                    wc.Dispose();
                    json = null;
                    server = null;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.DesktopLocation = new Point(1920 - this.Size.Width);
            timer4.Start();
            timer6.Start();
            this.Visible = false;
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void iconButton4_Click(object sender, EventArgs e)
        {
            bMaximized = !bMaximized;

            if (bMaximized)
            {
                this.Size = new Size(303, 428);
                panel2.Visible = false;
                iconButton4.IconChar = FontAwesome.Sharp.IconChar.ChevronUp;
            }
            else
            {
                this.Size = new Size(303, 180);
                panel2.Visible = true;
                iconButton4.IconChar = FontAwesome.Sharp.IconChar.ChevronDown;

            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (bFading)
            {
                timer1.Stop();
                
            }
            if (this.Opacity != 1.0) {
                this.Opacity += 0.02;
            }
            else
            {
                timer1.Stop();
                timer2.Start();
                timer5.Start();
            }
            bCanChange = false;
        }

        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            timer2.Stop();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer3.Start();
        }

        private void timer3_Tick(object sender, EventArgs e)
        {

            if (this.Opacity != 0.0)
            {
                this.Opacity -= 0.02;
                bFading = true;
            }
            else
            {
                timer3.Stop();
                timer2.Stop();
                timer1.Stop();
                this.Visible = false;
                bCanChange = true;
                bGetCommands = false;
                bFading = false;
            }
        }
        public static string PingTimeAverage(string host, int echoNum)
        {
            long totalTime = 0;
            int timeout = 120;
            Ping pingSender = new Ping();

            for (int i = 0; i < echoNum; i++)
            {
                PingReply reply = pingSender.Send(host, timeout);
                if (reply.Status == IPStatus.Success)
                {
                    totalTime += reply.RoundtripTime;
                }
            }
            return (totalTime / echoNum).ToString();
        }
        private void iconButton2_Click(object sender, EventArgs e)
        {
            timer3.Start();
        }

        private void iconButton3_Click(object sender, EventArgs e)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(szUrl, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (result)
            {
                System.Diagnostics.Process.Start(szUrl);
            }
        }

        private void iconButton1_Click(object sender, EventArgs e)
        {
            if(szConnectUrl != null)
                System.Diagnostics.Process.Start(szConnectUrl);
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            getData();
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            if(bGetCommands)
                timer3.Start();
        }

        private void timer5_Tick(object sender, EventArgs e)
        {
            bGetCommands = true;
        }

        private void timer6_Tick(object sender, EventArgs e)
        {
               if(GetAsyncKeyState(Keys.Menu) == -32767)
               {
                   if(GetAsyncKeyState(Keys.X) == -32767)
                   {
                       bEnabled = !bEnabled;
                       timer1.Start();
                       label17.Text = bEnabled ? "Press ALT + X to hide notifications" : "Press ALT + X to show notifications";

                   }
               }
        }

       

    }
}

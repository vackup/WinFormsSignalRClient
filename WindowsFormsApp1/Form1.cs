using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        HubConnection connection;

        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // Read this article to find out more info about Azure Functions and SignalR Service 
            // https://docs.microsoft.com/en-us/azure/azure-signalr/signalr-quickstart-azure-functions-csharp
            var data = await GetAsync("signalRnegociateFunction");

            var dataJson = JsonConvert.DeserializeObject<SignalRConnData>(data);

            connection = new HubConnectionBuilder()
                .WithUrl(dataJson.url, options => {
                    options.AccessTokenProvider = async () => dataJson.accessToken;
                })
                .Build();

            connection.Closed += async (error) =>
            {
                this.Invoke((Action)(() =>
                {
                    this.textBox1.AppendText("Conexion cerrada");
                    this.textBox1.AppendText(Environment.NewLine);
                }));
            };

            connection.On<TempObject>("newMessage", (newMessage) =>
            {
                this.Invoke((Action) (() =>
                {
                    this.textBox1.AppendText($"Message: {newMessage.sender}" +
                                             $"- {newMessage.temperature}" +
                                             $"- {newMessage.humidity}" +
                                             $"- {newMessage.time}");
                    this.textBox1.AppendText(Environment.NewLine);
                }));
            });

            await connection.StartAsync();

            this.textBox1.AppendText("Conexion abierta");
            this.textBox1.AppendText(Environment.NewLine);
        }

        public async Task<string> GetAsync(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}

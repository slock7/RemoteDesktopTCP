using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;

namespace Receiver
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private TcpListener server;
        private NetworkStream mainStream;
        private int port;

        private readonly Thread Listening;
        private readonly Thread GetImage;
        //private BitmapImage _image;

        int bytesReceived, totalReceived = 0;
        byte[] receivedData = new byte[1024 * 400];

        public Form1()
        {
            InitializeComponent();
            port = 123;
            client = new TcpClient();
            Listening = new Thread(StartListening);
            GetImage = new Thread(ReceiveImage);
        }

        private void StartListening()
        {
            while (!client.Connected)
            {
                server.Start();
                client = server.AcceptTcpClient();
            }
            GetImage.Start();
        }

        private void StopListening()
        {
            server.Stop();
            client = null;

            if (Listening.IsAlive)
                Listening.Abort();

            if (GetImage.IsAlive)
                GetImage.Abort();
        }

        private void ReceiveImage()
        {
            BinaryFormatter binFormatter = new BinaryFormatter();

            while (client.Connected)
            {
                mainStream = client.GetStream();
                //pictureBox1.Image = (Image) binFormatter.Deserialize(mainStream);

                do
                {
                    bytesReceived = mainStream.Read(receivedData, 0, receivedData.Length);
                    try
                    {
                        var image = Image.FromStream(new MemoryStream(receivedData));
                        pictureBox1.Image = image;
                    }
                    catch (Exception e)
                    {
                        return;
                    }

                    totalReceived += bytesReceived;
                    //image.Dispose();

                    Console.WriteLine(DateTime.Now + " Bytes received:" + bytesReceived.ToString());
                }
                while (bytesReceived != 0);

                Console.WriteLine("Total bytes read:" + totalReceived.ToString());
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            server = new TcpListener(IPAddress.Any, port);
            Listening.Start();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            StopListening();
        }
    }
}

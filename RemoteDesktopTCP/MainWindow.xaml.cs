using System;
using System.Linq;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Threading;
using Image = System.Drawing.Image;
using MessageBox = System.Windows.MessageBox;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace sender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TcpClient client = new TcpClient();
        private NetworkStream mainStream;
        private int portNumber;
        //private static Image _screenshot;

        int framesSent = 0;
        private static byte[] bytes;


        public MainWindow()
        {
            InitializeComponent();
        }

        private static byte[] GrabDesktop()
        {
            Rectangle bounds = Screen.PrimaryScreen.Bounds;
            var screenshot = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format16bppRgb565);

            using (Graphics graphic = Graphics.FromImage(screenshot))
            {
                graphic.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size, CopyPixelOperation.SourceCopy);
            }

            long imageQuality = 25L;
            var mss = new MemoryStream();

            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, imageQuality);
            //Bitmap bm = new Bitmap(screenshot);
            screenshot.Save(mss, GetEncoder(ImageFormat.Jpeg), encoderParameters);

            bytes = mss.GetBuffer();
            screenshot.Dispose();
            mss.Close();

            return bytes;
            //return Image.FromStream(mss);
        }

        static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            return codecs.Single(codec => codec.FormatID == format.Guid);
        }

        private void SendDesktopImage()
        {
            //BinaryFormatter binaryFormatter = new BinaryFormatter();
            mainStream = client.GetStream();

            framesSent += 1;
            int lenght = GrabDesktop().Length;
            var bytes2 = bytes.Length;

            try
            {
                mainStream.Write(GrabDesktop(), 0, lenght);
                Console.WriteLine("Sender. Bytes sent:" + lenght.ToString());
                mainStream.Flush();
                //Thread.Sleep(1000);
            }
            catch (Exception e)
            {
                return;
            }

            //Change from Bitmap to BitmapImage - BitmapImage can't be serialized!
            //var desktop = GrabDesktop();
            //var bitmapDesktop = BitmapToImageSource(desktop);

            //binaryFormatter.Serialize(mainStream, GrabDesktop());
            //BufferedStream bs = new BufferedStream(mainStream);
        }


        private void ConnectBtn_OnClick(object sender, RoutedEventArgs e)
        {
            portNumber = 123;
            try
            {
                client.Connect("localhost", portNumber);
                MessageBox.Show("Connected");
            }
            catch (Exception)
            {
                MessageBox.Show("failed to connect...");
            }
        }

        private void ShareBtn_OnClick(object sender, RoutedEventArgs e)
        {
            while (client.Connected)
            {
                SendDesktopImage();
            }
        }
    }
}

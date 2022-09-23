using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace DoraRemoteDesktopClient
{
    public unsafe partial class ClientMain : Form
    {
        private System.Net.Sockets.UdpClient udpClient = null;

        private uint[,] serialCodeArray = new uint[10, 10];

        private Bitmap canvas;

        private System.Timers.Timer timer;

        private ClientSubForm sub;

        private byte* canvasScan0;

        public ClientMain()
        {
            InitializeComponent();

            canvas = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var bd = canvas.LockBits(new Rectangle(0, 0, canvas.Width, canvas.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, canvas.PixelFormat);
            canvasScan0 = (byte*)bd.Scan0.ToPointer();
            canvas.UnlockBits(bd);

            sub = new ClientSubForm();
            sub.Show();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (udpClient != null)
            {
                return;
            }

            pictureBox1.Image = new Bitmap(1920, 1080);

            //UdpClientを作成し、指定したポート番号にバインドする
            System.Net.IPEndPoint localEP = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 49513);
            udpClient = new System.Net.Sockets.UdpClient(localEP);
            //非同期的なデータ受信を開始する
            udpClient.BeginReceive(ReceiveCallback, udpClient);

            //var sizeX = 192;
            //var sizeY = 108;

            //var b = new Bitmap(sizeX * 10, sizeY * 10);

            //foreach (var f in Directory.EnumerateFiles(@"D:\Dorashiro\デスクトップ\test"))
            //{
            //    //System.Diagnostics.Debug.WriteLine(Path.GetFileName(f));
            //    var sub = Path.GetFileNameWithoutExtension(f).Substring(2).Split('_');

            //    // 貼り付け先
            //    using (var g = Graphics.FromImage(b))
            //    {
            //        using (var img = Image.FromFile(f))
            //        {
            //            g.DrawImage(img, new Point(sizeX * int.Parse(sub[0]), sizeY * int.Parse(sub[1])));
            //        }
            //    }
            //}

            //pictureBox1.Image = b;

            timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler((ss, ee) =>
            {
                if(InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        pictureBox1.Image = null;
                        pictureBox1.Image = canvas;
                    });
                }
                else
                {
                    pictureBox1.Image = null;
                    pictureBox1.Image = canvas;
                }
            });
            timer.Interval = 32;
            timer.Start();
        }

        //データを受信した時
        private void ReceiveCallback(IAsyncResult ar)
        {
            this.Invoke((MethodInvoker)delegate
            {
                sub.AvailableUpdate(udpClient.Available);
            });

            System.Net.Sockets.UdpClient udp = (System.Net.Sockets.UdpClient)ar.AsyncState;

            //非同期受信を終了する
            System.Net.IPEndPoint remoteEP = null;
            byte[] rcvBytes;
            try
            {
                rcvBytes = udp.EndReceive(ar, ref remoteEP);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Console.WriteLine("受信エラー({0}/{1})", ex.Message, ex.ErrorCode);
                return;
            }
            catch (ObjectDisposedException ex)
            {
                //すでに閉じている時は終了
                Console.WriteLine("Socketは閉じられています。");
                return;
            }

            int x;
            int y;
            uint serialCode;

            // パラメーターは画像の後ろにつく
            using (var param = new MemoryStream(rcvBytes, rcvBytes.Length-12, 12))
            {
                //param.Position=rcvBytes.Length - 12;
                var buffer = new byte[4];
                // x座標を取得
                param.Read(buffer, 0, 4);
                x = BitConverter.ToInt32(buffer, 0);
                // y座標を取得
                param.Read(buffer, 0, 4);
                y = BitConverter.ToInt32(buffer, 0);
                //　シリアルコードを取得
                param.Read(buffer, 0, 4);
                serialCode = BitConverter.ToUInt32(buffer, 0);

                System.Diagnostics.Debug.WriteLine("(" + x + ", " + y + ")");
            }

            var ByteAtPixcel = 3;
            var sendBmpPosition = 0;
            var readStartPosition = (1920 * 108 * y * ByteAtPixcel) + (192 * x * ByteAtPixcel);
            //var readStartPosition = (Screen.PrimaryScreen.Bounds.Width * sizeY * y * ByteAtPixcel) + (sizeX * x * ByteAtPixcel);
            for (int innerY = 0; innerY < 108; innerY++)
            {
                readStartPosition += Screen.PrimaryScreen.Bounds.Width * ByteAtPixcel;
                for (int innerX = 0; innerX < 192 * ByteAtPixcel; innerX++)
                {
                    canvasScan0[readStartPosition + innerX] = rcvBytes[sendBmpPosition];
                    sendBmpPosition++;
                }
            }

            //using (var imgMemoryStream = new MemoryStream(rcvBytes, 0, rcvBytes.Length - 12))
            //{
            //    Bitmap b = new Bitmap(imgMemoryStream);

            //    // 共通な描画処理
            //    Action doDraw = () =>
            //    {
            //        using (Graphics g = Graphics.FromImage(canvas))
            //        {
            //            var sw = new System.Diagnostics.Stopwatch();
            //            sw.Start();
            //            g.DrawImage(b, new Point(x * b.Width, y * b.Height));
            //            g.DrawString(x.ToString() + ", " + y.ToString(), System.Drawing.SystemFonts.DefaultFont, Brushes.Black, x * b.Width, y * b.Height);

            //            //g.DrawImage(b, new Point(x * 192, y * 108));
            //            //g.DrawString(x.ToString() + ", " + y.ToString(), System.Drawing.SystemFonts.DefaultFont, Brushes.Black, x * 192, y * 108);

            //            // 一旦nullを入れてからじゃないとメモリが解放されないらしい
            //            //pictureBox1.Image = null;
            //            //pictureBox1.Image = canvas;

            //            sw.Stop();
            //            //System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);
            //        }
            //    };



            //    メインスレッド以外で実行された時はInvokeする
            //    if (this.InvokeRequired)
            //    {
            //        this.Invoke((MethodInvoker)delegate
            //        {
            //            doDraw();
            //        });
            //    }
            //    // メインスレッドで実行された時はそのまま実行
            //    else
            //    {
            //        doDraw();
            //    }
            //}

            //再びデータ受信を開始する
            udp.BeginReceive(ReceiveCallback, udp);
        }
    }
}

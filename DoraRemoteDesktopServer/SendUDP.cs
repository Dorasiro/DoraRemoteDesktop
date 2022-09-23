using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DoraRemoteDesktopClient
{
    class SendUDP
    {
        private readonly string hostname;
        private readonly int port;

        private UdpClient client;

        public SendUDP(string hostname, int port)
        {
            this.hostname = hostname;
            this.port = port;

            client = new UdpClient();
        }

        private void SendCallback(IAsyncResult result)
        {
            var udp = (UdpClient)result.AsyncState;

            //非同期送信を終了する
            try
            {
                udp.EndSend(result);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Console.WriteLine("送信エラー({0}/{1})",
                    ex.Message, ex.ErrorCode);
            }
            catch (ObjectDisposedException ex)
            {
                //すでに閉じている時は終了
                Console.WriteLine("Socketは閉じられています。");
            }
        }

        public void SendScreenCapture(byte[] sendBytes)
        {
            client.BeginSend(sendBytes, sendBytes.Length, SendCallback, client);
        }

        public void SendScreenCapture(Bitmap sendBitmap)
        {
            var m = new MemoryStream();
            sendBitmap.Save(m, ImageFormat.Bmp);
            var b = m.GetBuffer();

            client.BeginSend(b, b.Length, hostname, port, SendCallback, client);
        }

        /// <summary>
        /// 座標情報を付加した画像を送信
        /// </summary>
        /// <param name="sendBitmap"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SendScreenCapture(Bitmap sendBitmap, int x, int y, uint serialCode)
        {
            var memoryStream = new MemoryStream();
            sendBitmap.Save(memoryStream, ImageFormat.Jpeg);
            //sendBitmap.Save(@"D:\Dorashiro\デスクトップ\test\server2.bmp", ImageFormat.Jpeg);
            //sendBitmap.Dispose();

            // 座標等のパラメータを書き込む
            memoryStream.Capacity += 12;
            var buffer = new byte[3][];
            buffer[0] = BitConverter.GetBytes(x);
            buffer[1] = BitConverter.GetBytes(y);
            buffer[2] = BitConverter.GetBytes(serialCode);
            memoryStream.Write(buffer[0], 0, buffer[0].Length);
            memoryStream.Write(buffer[1], 0, buffer[1].Length);
            memoryStream.Write(buffer[2], 0, buffer[2].Length);

            // memoryStreamからbyte[]を取得して送信
            var sendByte = memoryStream.GetBuffer();
            memoryStream.Dispose();
            client.BeginSend(sendByte, sendByte.Length, hostname, port, SendCallback, client);
        }

        /// <summary>
        /// 座標情報を付加した画像を送信
        /// </summary>
        /// <param name="sendBitmap"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SendScreenCapture(byte[] sendBitmapByte, int x, int y, uint serialCode)
        {
            var sendByte = new byte[sendBitmapByte.Length + 12];
            Array.Copy(sendBitmapByte, sendByte, sendBitmapByte.Length);

            var byteX = BitConverter.GetBytes(x);
            var byteY = BitConverter.GetBytes(y);
            var byteSerialCode = BitConverter.GetBytes(serialCode);

            // 座標等のパラメータを書き込む
            for (int i=0;i<4;i++)
            {
                sendByte[sendBitmapByte.Length + i] = byteX[i];
                sendByte[sendBitmapByte.Length + i +  4] = byteY[i];
                sendByte[sendBitmapByte.Length + i + 8] = byteSerialCode[i];
            }

            client.BeginSend(sendByte, sendByte.Length, hostname, port, SendCallback, client);
        }
    }
}

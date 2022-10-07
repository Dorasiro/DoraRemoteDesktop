using DoraRemoteDesktopClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace DoraRemoteDesktopServer
{
    public partial class SubForm : Form
    {
        [DllImport("ImageProcessor.dll")]
        private static extern int main();

        /// <summary>
        /// 取得したイメージを表示するためのPictureBox
        /// </summary>
        private PictureBox p;
        /// <summary>
        /// 定期的にキャプチャするために使用するタイマー
        /// 非同期で動くやつ
        /// </summary>
        private System.Timers.Timer timer;
        /// <summary>
        /// キャプチャのオンオフを制御するために使用
        /// </summary>
        private bool enable;
        /// <summary>
        /// PictureBoxに表示中のBitmapをDisposeするとダメらしいので交互に切り替えるために使用
        /// いちばん最初は0で以降は1と2を繰り返す
        /// 0,1,2しか使わないのでbyteにしておく
        /// </summary>
        //private byte counter;
        //private Bitmap b1;
        //private Bitmap b2;

        //private string host = "192.168.2.116";
        //private int port = 49513;

        /// <summary>
        /// 非同期のタイマーを使ってるけどキャプチャは時系列順に処理しないと意味がないからそのためにLockする
        /// </summary>
        private object lockobj = new object();

        // ボタン７用
        //private int b7x = 0;
        //private int b7y = 0;

        //private Bitmap[,] old;
        private byte[][] old2;
        private bool nowProgress = false;

        public SubForm(PictureBox p)
        {
            InitializeComponent();

            this.p = p;
        }

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    ScreenCapture s = new ScreenCapture();
        //    timer = new System.Timers.Timer();
        //    timer.Elapsed += new ElapsedEventHandler((ss, ee) =>
        //    {
        //        var sw = new System.Diagnostics.Stopwatch();
        //        sw.Start();
        //        /*
        //         * counter == 0の時はいちばん最初なのでb2をDisposeしない
        //         * 以降は1と2を交互に繰り返す
        //         */
        //        lock (lockobj)
        //        {
        //            if (counter == 0)
        //            {
        //                b1 = s.DoCapture();
        //                p.Image = b1;
        //                counter = 1;

        //                SendUDP send = new SendUDP(host, port);
        //                send.SendScreenCapture(s.DoCapture());
        //            }
        //            else if (counter == 1)
        //            {
        //                b2 = s.DoCapture();
        //                p.Image = b2;
        //                b1.Dispose();
        //                counter = 2;

        //                SendUDP send = new SendUDP(host, port);
        //                send.SendScreenCapture(s.DoCapture());
        //            }
        //            else if (counter == 2)
        //            {
        //                b1 = s.DoCapture();
        //                p.Image = b1;
        //                b2.Dispose();
        //                counter = 1;

        //                SendUDP send = new SendUDP(host, port);
        //                send.SendScreenCapture(s.DoCapture());
        //            }
        //        }

        //        sw.Stop();
        //        System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);
        //    });

        //    timer.Interval = 16;

        //    if(enable)
        //    {
        //        timer.Stop();

        //        enable = false;
        //    }
        //    else
        //    {
        //        timer.Start();

        //        enable = true;
        //    }
        //}

        /// <summary>
        /// 全てのBitmapを解放
        /// </summary>
        public void DisposeBitmap()
        {

            //if (b1 != null)
            //{
            //    b1.Dispose();
            //}

            //if (b2 != null)
            //{
            //    b2.Dispose();
            //}
        }

        //private void button2_Click(object sender, EventArgs e)
        //{
        //    ScreenCapture s = new ScreenCapture();
        //    s.DoCapture(2,3).Save(@"D:\Dorashiro\デスクトップ\ss.bmp");
        //    //s.DoCapture().Save(@"D:\Dorashiro\デスクトップ\ss.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        //}

        //private void button3_Click(object sender, EventArgs e)
        //{
        //    ScreenCapture s = new ScreenCapture();
        //    SendUDP send = new SendUDP("192.168.2.113", 49513);
        //    send.SendScreenCapture(s.DoCapture());
        //}

        //private async void button4_Click(object sender, EventArgs e)
        //{
        //    this.Visible = false;
        //    this.Update();

        //    var sizeX = 192;
        //    var sizeY = 108;

        //    int x = 1;
        //    int y = 1;

        //    // 横のループ
        //    while(Screen.PrimaryScreen.Bounds.Width/ (x * sizeX) != 0)
        //    {
        //        // 縦のループ
        //        while (Screen.PrimaryScreen.Bounds.Height / (y * sizeY) != 0)
        //        {
        //            await System.Threading.Tasks.Task.Run(() =>
        //            {
        //                System.Diagnostics.Debug.WriteLine("(" + x + ", " + y + ")");

        //                var name = "ss" + x.ToString() + "_" + y.ToString() + ".bmp";
        //                ScreenCapture s = new ScreenCapture();
        //                //s.DoCapture(x - 1, y - 1).Save(@"D:\Dorashiro\デスクトップ\test\" + name);
        //                s.DoCapture(sizeX, sizeY, x - 1, y - 1).Save(@"D:\Dorashiro\デスクトップ\test\" + name);
        //                //s.DoCapture(x - 1, y - 1);
        //            });

        //            y++;
        //        }

        //        y = 1;
        //        x++;
        //    }

        //    this.Visible = true;
        //}

        //private void button5_Click(object sender, EventArgs e)
        //{
        //    this.Visible = false;
        //    this.Update();

        //    var s = new ScreenCapture();
        //    var result = s.DoCaptureReturnArray(192, 108);

        //    for(int i=0;i<result.GetLength(0);i++)
        //    {
        //        for(int j=0;j<result.GetLength(1);j++)
        //        {
        //            result[i, j].Save(@"D:\Dorashiro\デスクトップ\test\ss" + i + "_" + j + ".bmp");
        //            result[i, j].Dispose();
        //        }
        //    }

        //    this.Visible = true;
        //}

        //private void button6_Click(object sender, EventArgs e)
        //{
        //    ScreenCapture s = new ScreenCapture();
        //    //SendUDP send = new SendUDP("192.168.2.113", 49513);
        //    SendUDP send = new SendUDP("127.0.0.1", 49513);
        //    send.SendScreenCapture(s.DoCapture(5, 5), 5, 5, 0);
        //}

        //private void button7_Click(object sender, EventArgs e)
        //{
        //    ScreenCapture s = new ScreenCapture();
        //    //SendUDP send = new SendUDP("192.168.2.113", 49513);
        //    SendUDP send = new SendUDP("127.0.0.1", 49513);
        //    while(true)
        //    {
        //        if (b7y < 10)
        //        {
        //            // xが上限以下
        //            if (b7x < 10)
        //            {
        //                send.SendScreenCapture(s.DoCapture(b7x, b7y), b7x, b7y, 0);
        //                b7x++;
        //                break;
        //            }
        //            // xが上限以上の時はxを0に戻してyをあげる
        //            else
        //            {
        //                b7x = 0;
        //                b7y++;
        //            }
        //        }
        //        else
        //        {
        //            // yが上限を超えたら終わり
        //            break;
        //        }
        //    }
        //}

        //private void button8_Click(object sender, EventArgs e)
        //{
        //    ScreenCapture s = new ScreenCapture();
        //    //SendUDP send = new SendUDP("192.168.2.113", 49513);
        //    SendUDP send = new SendUDP("127.0.0.1", 49513);

        //    timer = new System.Timers.Timer();
        //    timer.Elapsed += new ElapsedEventHandler((ss, ee) =>
        //    {
        //        int x = 0;
        //        int y = 0;

        //        while (true)
        //        {
        //            if (y < 10)
        //            {
        //                // xが上限以下
        //                if (x < 10)
        //                {
        //                    Parallel.Invoke(new Action(() =>
        //                    {
        //                        var sw = new System.Diagnostics.Stopwatch();
        //                        sw.Start();
        //                        send.SendScreenCapture(s.DoCapture(192*4, 108*4, x, y), x, y, 0);
        //                        sw.Stop();
        //                        System.Diagnostics.Debug.WriteLine(x+", "+y+"："+sw.ElapsedMilliseconds);
        //                    }));
        //                    x++;
        //                }
        //                // xが上限以上の時はxを0に戻してyをあげる
        //                else
        //                {
        //                    x = 0;
        //                    y++;
        //                }
        //            }
        //            else
        //            {
        //                // yが上限を超えたら終わり
        //                break;
        //            }
        //        }
        //    });

        //    timer.Interval = 16;
        //    //timer.Interval = 2000;

        //    if (enable)
        //    {
        //        timer.Stop();

        //        enable = false;
        //    }
        //    else
        //    {
        //        timer.Start();

        //        enable = true;
        //    }
        //}

        //private void button9_Click(object sender, EventArgs e)
        //{
        //    ScreenCapture s = new ScreenCapture();
        //    SendUDP send = new SendUDP("192.168.2.116", 49513);
        //    //SendUDP send = new SendUDP("127.0.0.1", 49513);

        //    int x = 0;
        //    int y = 0;

        //    while (true)
        //    {
        //        if (y < 10)
        //        {
        //            // xが上限以下
        //            if (x < 10)
        //            {
        //                send.SendScreenCapture(s.DoCapture(x, y), x, y, 0);
        //                x++;
        //            }
        //            // xが上限以上の時はxを0に戻してyをあげる
        //            else
        //            {
        //                x = 0;
        //                y++;
        //            }
        //        }
        //        else
        //        {
        //            // yが上限を超えたら終わり
        //            break;
        //        }
        //    }
        //}

        private void SubForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        //private void button10_Click(object sender, EventArgs e)
        //{
        //    ScreenCapture s = new ScreenCapture();
        //    var r = s.DoCaptureReturnArray(192, 108);
        //    for (int i = 0; i < r.GetLength(0); i++)
        //    {
        //        for (int j = 0; j < r.GetLength(1); j++)
        //        {
        //            r[i, j].Save(@"D:\Dorashiro\デスクトップ\test\ss" + i + "_" + j + ".bmp");
        //            r[i, j].Dispose();
        //        }
        //    }
        //}

        //private void button11_Click(object sender, EventArgs e)
        //{
        //    //SendUDP send = new SendUDP("127.0.0.1", 49513);
        //    SendUDP send = new SendUDP("192.168.2.116", 49513);
        //    ScreenCapture s = new ScreenCapture();

        //    timer = new System.Timers.Timer();
        //    timer.Elapsed += new ElapsedEventHandler(async (ss, ee) =>
        //    {
        //        try
        //        {
        //            if (nowProgress)
        //            {
        //                return;
        //            }

        //            nowProgress = true;

        //            var sw = new System.Diagnostics.Stopwatch();
        //            sw.Start();

        //            var r = s.DoCaptureReturnArray(198, 108);

        //            sw.Stop();
        //            System.Diagnostics.Debug.WriteLine("" + sw.ElapsedMilliseconds);
        //            sw.Start();
        //            if (old == null)
        //            {
        //                for (int x = 0; x < r.GetLength(0); x++)
        //                {
        //                    // このループはコストが低いからParallelの方が遅い
        //                    for (int y = 0; y < r.GetLength(1); y++)
        //                    {
        //                        send.SendScreenCapture(r[x, y], x, y, 0);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                // 画像の重複チェックが重いからParallelの方が早い
        //                Parallel.For(0, r.GetLength(0), x =>
        //                {
        //                    for (int y = 0; y < r.GetLength(1); y++)
        //                    {
        //                        if (!ScreenCapture.CompareImage(r[x, y], old[x, y]))
        //                        {
        //                            send.SendScreenCapture(r[x, y], x, y, 0);
        //                        }
        //                    }
        //                });

        //                //for (int x = 0; x < r.GetLength(0); x++)
        //                //{
        //                //    // 画像の重複チェックが重いからParallelの方が早い
        //                //    Parallel.For(0, r.GetLength(1), y =>
        //                //    {
        //                //        if (ScreenCapture.CompareImage(r[x, y], old[x, y]))
        //                //        {
        //                //            //System.Diagnostics.Debug.WriteLine("Skip!");
        //                //        }
        //                //        else
        //                //        {
        //                //            //System.Diagnostics.Debug.WriteLine("(" + x + ", " + y + ")");
        //                //            send.SendScreenCapture(r[x, y], x, y, 0);
        //                //        }
        //                //    });
        //                //}

        //                //for (int x = 0; x < r.GetLength(0); x++)
        //                //{
        //                //    // 画像の重複チェックが重いからParallelの方が早い
        //                //    for (int y = 0; y < r.GetLength(1); y++)
        //                //    {
        //                //        if (ScreenCapture.CompareImage(r[x, y], old[x, y]))
        //                //        {
        //                //            //System.Diagnostics.Debug.WriteLine("Skip!");
        //                //        }
        //                //        else
        //                //        {
        //                //            //System.Diagnostics.Debug.WriteLine("(" + x + ", " + y + ")");
        //                //            send.SendScreenCapture(r[x, y], x, y, 0);
        //                //        }
        //                //    };
        //                //}
        //            }

        //            // oldの中身を解放
        //            if (old == null)
        //            {
        //                old = new Bitmap[r.GetLength(0), r.GetLength(1)];
        //            }
        //            else
        //            {
        //                for (int x = 0; x < old.GetLength(0); x++)
        //                {
        //                    for (int y = 0; y < old.GetLength(1); y++)
        //                    {
        //                        old[x, y].Dispose();
        //                    }
        //                }
        //            }

        //            // 今使用したBitmap[,] rをoldに入れて次の比較に使う
        //            Array.Copy(r, old, r.Length);

        //            nowProgress = false;

        //            sw.Stop();
        //            //System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);

        //        }
        //        catch(Exception exx)
        //        {

        //        }
        //    });

        //    timer.Interval = 16;

        //    if (enable)
        //    {
        //        timer.Stop();

        //        enable = false;
        //    }
        //    else
        //    {
        //        timer.Start();

        //        enable = true;
        //    }
        //}

        private void button12_Click(object sender, EventArgs e)
        {
            SendUDP send = new SendUDP("127.0.0.1", 49513);
            //SendUDP send = new SendUDP("192.168.2.116", 49513);
            ScreenCapture s = new ScreenCapture(198/2, 108/2);

            timer = new System.Timers.Timer();

            timer.Elapsed += new ElapsedEventHandler((ss, ee) =>
            {
                try
                {
                    if (nowProgress)
                    {
                        //System.Diagnostics.Debug.WriteLine("skip");
                        return;
                    }

                    nowProgress = true;

                    var sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    var r = s.DoCaptureReturnByte();

                    sw.Stop();
                    //System.Diagnostics.Debug.WriteLine("=>" + sw.ElapsedMilliseconds);
                    sw.Start();
                    if (old2 == null)
                    {
                        for (int y = 0; y < s.VerticalCount - 1; y++)
                        {
                            // このループはコストが低いからParallelの方が遅い
                            for (int x = 0; x < s.HorizonCount + 1; x++)
                            {
                                send.SendScreenCapture(r[y * s.VerticalCount + x], x, y, 0);
                            }
                        }

                        old2 = new byte[r.Length][];
                    }
                    else
                    {
                        //// 画像の重複チェックが重いからParallelの方が早い
                        //Parallel.For(0, r.GetLength(0), x =>
                        //{
                        //    for (int y = 0; y < r.GetLength(1); y++)
                        //    {
                        //        if (!ScreenCapture.CompareImage(r[counter], old2[counter]))
                        //        {
                        //            send.SendScreenCapture(r[counter], x, y, 0);
                        //        }

                        //        Interlocked.Increment(ref counter);
                        //    }
                        //});

                        for (int y = 0; y < s.VerticalCount - 1; y++)
                        {
                            for (int x = 0; x < s.HorizonCount + 1; x++)
                            {
                                if (!ScreenCapture.CompareImage(r[y * s.VerticalCount + x], old2[x + y * s.VerticalCount]))
                                {
                                    send.SendScreenCapture(r[y * s.VerticalCount + x], x, y, 0);
                                    System.Diagnostics.Debug.WriteLine("("+x.ToString()+", "+y.ToString()+")");
                                }
                            }
                        }
                    }

                    // 今使用したBitmap[,] rをoldに入れて次の比較に使う
                    Array.Copy(r, old2, r.Length);

                    nowProgress = false;

                    sw.Stop();
                    //System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);

                }
                catch (Exception exx)
                {

                }
            });

            timer.Interval = 32;

            if (enable)
            {
                timer.Stop();

                enable = false;
            }
            else
            {
                timer.Start();

                enable = true;
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            // 比較対象となるbyte[]を用意　今回は両方とも同じもの
            ScreenCapture s = new ScreenCapture(198 * 2, 108 * 2);
            var r = s.DoCaptureReturnByte();
            var r2 = new byte[r.Length][];
            Array.Copy(r, r2, r.Length);

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            for(int i=0;i<100000;i++)
            {
                for (int y = 0; y < s.VerticalCount - 1; y++)
                {
                    for (int x = 0; x < s.HorizonCount + 1; x++)
                    {
                        ScreenCapture.CompareImage(r[y * s.VerticalCount + x], r2[x + y * s.VerticalCount]);
                    }
                }
            }

            sw.Stop();
            System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            var sizeX = 192;
            var sizeY = 108;
            ScreenCapture s = new ScreenCapture(sizeX, sizeY);

            byte[] imgByte;

            using (var fullSizeSS = s.DoCaptureFullSS())
            {
                System.Drawing.Imaging.BitmapData bd = fullSizeSS.LockBits(new Rectangle(0, 0, fullSizeSS.Width, fullSizeSS.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, fullSizeSS.PixelFormat);
                var imgByteSize = bd.Stride * fullSizeSS.Height;
                imgByte = new byte[imgByteSize];
                System.Runtime.InteropServices.Marshal.Copy(bd.Scan0, imgByte, 0, imgByteSize);
                fullSizeSS.UnlockBits(bd);
            }

            // 分割する大きさのループ　192, 108の大きさごとに分けるためのループ
            for (int y = 0; y < s.VerticalCount - 1; y++)
            {
                for (int x = 0; x < s.HorizonCount + 1; x++)
                {
                    var sendBmp = new Bitmap(sizeX, sizeY, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    var bd = sendBmp.LockBits(new Rectangle(0, 0, sendBmp.Width, sendBmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, sendBmp.PixelFormat);

                    unsafe
                    {
                        byte* sendBmpPointer = (byte*)bd.Scan0.ToPointer();

                        // sendBmpPointerにはシーケンシャルに書き込んでいくため、連番を保存するカウンターが必要
                        var sendBmpPosition = 0;
                        var readStartPosition = (1920 * 108 * y*3) + (192 * x*3);
                        for(int innerY = 0;innerY<sendBmp.Height;innerY++)
                        {
                            readStartPosition += 1920*3;
                            for(int innerX = 0;innerX<bd.Stride;innerX++)
                            {
                                sendBmpPointer[sendBmpPosition] = imgByte[readStartPosition+innerX];
                                sendBmpPosition++;
                            }
                        }
                    }
                    sendBmp.UnlockBits(bd);
                    //var name = "ss" + x.ToString() + "_" + y.ToString() + ".bmp";
                    sendBmp.Save(@"D:\Dorashiro\デスクトップ\test\"+x+"_"+y+".bmp");
                    sendBmp.Dispose();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //SendUDP send = new SendUDP("127.0.0.1", 49513);
            //SendUDP send = new SendUDP("192.168.2.116", 49513);
            SendUDP send = new SendUDP("192.168.2.112", 49513);
            var sizeX = 192;
            var sizeY = 108;
            ScreenCapture s = new ScreenCapture(sizeX, sizeY);

            timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler((ss, ee) =>
            {
                if (nowProgress)
                {
                    //System.Diagnostics.Debug.WriteLine("skip");
                    return;
                }

                nowProgress = true;

                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                var capture = s.DoCaptureNoDrawImage();

                sw.Stop();
                System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);

                if (old2 == null)
                {
                    for (int y = 0; y < s.VerticalCount - 1; y++)
                    {
                        // このループはコストが低いからParallelの方が遅い
                        for (int x = 0; x < s.HorizonCount + 1; x++)
                        {
                            send.SendScreenCapture(capture[y * s.VerticalCount + x], x, y, 0);
                        }
                    }

                    

                    old2 = new byte[capture.Length][];
                }
                else
                {
                    //// 画像の重複チェックが重いからParallelの方が早い
                    //Parallel.For(0, r.GetLength(0), x =>
                    //{
                    //    for (int y = 0; y < r.GetLength(1); y++)
                    //    {
                    //        if (!ScreenCapture.CompareImage(r[counter], old2[counter]))
                    //        {
                    //            send.SendScreenCapture(r[counter], x, y, 0);
                    //        }

                    //        Interlocked.Increment(ref counter);
                    //    }
                    //});

                    for (int y = 0; y < s.VerticalCount - 1; y++)
                    {
                        for (int x = 0; x < s.HorizonCount + 1; x++)
                        {
                            if (!ScreenCapture.CompareImage(capture[y * s.VerticalCount + x], old2[x + y * s.VerticalCount]))
                            {
                                send.SendScreenCapture(capture[y * s.VerticalCount + x], x, y, 0);
                                //System.Diagnostics.Debug.WriteLine("(" + x.ToString() + ", " + y.ToString() + ")");
                            }
                        }
                    }
                }

                // 今使用したBitmap[,] rをoldに入れて次の比較に使う
                Array.Copy(capture, old2, capture.Length);

                nowProgress = false;

                //sw.Stop();
                //System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);

            });

            timer.Interval = 32;
            timer.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //SendUDP send = new SendUDP("127.0.0.1", 49513);
            //SendUDP send = new SendUDP("192.168.2.116", 49513);
            ScreenCapture s = new ScreenCapture(198 / 2, 108 / 2);

            try
            {
                if (nowProgress)
                {
                    //System.Diagnostics.Debug.WriteLine("skip");
                    return;
                }

                nowProgress = true;

                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                var r = s.DoCaptureReturnByte();

                sw.Stop();
                //System.Diagnostics.Debug.WriteLine("=>" + sw.ElapsedMilliseconds);
                sw.Start();
                if (old2 == null)
                {
                    for (int y = 0; y < s.VerticalCount - 1; y++)
                    {
                        // このループはコストが低いからParallelの方が遅い
                        for (int x = 0; x < s.HorizonCount + 1; x++)
                        {
                            //send.SendScreenCapture(r[y * s.VerticalCount + x], x, y, 0);
                        }
                    }

                    old2 = new byte[r.Length][];
                }
                else
                {
                    //// 画像の重複チェックが重いからParallelの方が早い
                    //Parallel.For(0, r.GetLength(0), x =>
                    //{
                    //    for (int y = 0; y < r.GetLength(1); y++)
                    //    {
                    //        if (!ScreenCapture.CompareImage(r[counter], old2[counter]))
                    //        {
                    //            send.SendScreenCapture(r[counter], x, y, 0);
                    //        }

                    //        Interlocked.Increment(ref counter);
                    //    }
                    //});

                    for (int y = 0; y < s.VerticalCount - 1; y++)
                    {
                        for (int x = 0; x < s.HorizonCount + 1; x++)
                        {
                            if (!ScreenCapture.CompareImage(r[y * s.VerticalCount + x], old2[x + y * s.VerticalCount]))
                            {
                                //send.SendScreenCapture(r[y * s.VerticalCount + x], x, y, 0);
                                System.Diagnostics.Debug.WriteLine("(" + x.ToString() + ", " + y.ToString() + ")");
                            }
                        }
                    }
                }

                // 今使用したBitmap[,] rをoldに入れて次の比較に使う
                Array.Copy(r, old2, r.Length);

                nowProgress = false;

                sw.Stop();
                //System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);

            }
            catch (Exception exx)
            {

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ScreenCapture s = new ScreenCapture(198 / 2, 108 / 2);
            var r = s.DoCaptureReturnByte();
            if (old2 == null)
            {
                old2 = new byte[r.Length][];
            }
            else
            {
                ScreenCapture.CompareImage(r[0], old2[0]);
            }

            // 今使用したBitmap[,] rをoldに入れて次の比較に使う
            Array.Copy(r, old2, r.Length);
        }
    }
}

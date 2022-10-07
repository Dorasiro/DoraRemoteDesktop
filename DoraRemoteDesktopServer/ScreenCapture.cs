using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoraRemoteDesktopServer
{
    class ScreenCapture
    {
        [DllImport("ImageProcessor.dll")]
        private static extern int main(IntPtr b1Ptr, IntPtr b2Ptr, int arrayLength);

        private int sizeX;
        private int sizeY;

        public int HorizonCount
        {
            private set; get;
        }

        public int VerticalCount
        {
            private set; get;
        }

        private double horizonDouble;
        private double verticalDouble;

        /// <summary>
        /// 1ピクセルあたりのバイトサイズを表す
        /// アルファチャンネルなしのRGBのBMPを使用するため、1ピクセルは24bit=3byteとなる
        /// </summary>
        public const int ByteAtPixcel = 3;

        public ScreenCapture()
        {

        }

        public ScreenCapture(int sizeX, int sizeY)
        {
            this.sizeX = sizeX;
            this.sizeY = sizeY;

            // 最終的な枚数　一番最後のイメージは引数で指定した大きさと同じとは限らない
            horizonDouble = Screen.PrimaryScreen.Bounds.Width / sizeX;
            verticalDouble = Screen.PrimaryScreen.Bounds.Height / sizeY;

            // 上のint変換版　小数点以下は切り上げ
            HorizonCount = (int)Math.Ceiling(horizonDouble);
            VerticalCount = (int)Math.Ceiling(verticalDouble);
        }

        /// <summary>
        /// キャプチャしてそのイメージを返す
        /// </summary>
        /// <returns>キャプチャしたbitmapイメージ</returns>
        public Bitmap DoCapture()
        {
            //bitmap = new Bitmap(width, height);
            //Bitmap bitmap = new Bitmap(width, height);
            Bitmap bitmap = new Bitmap(100, 100);
            Graphics g = Graphics.FromImage(bitmap);

            var p = new Point(0, 0);
            g.CopyFromScreen(p, p, bitmap.Size);

            g.Dispose();
            
            return bitmap;

            //Bitmap bitmap = new Bitmap(width, height);
            //g = Graphics.FromImage(bitmap);
            //g.CopyFromScreen(p, p, bitmap.Size);

            //return bitmap;
        }

        /// <summary>
        /// キャプチャしてそのイメージを返す
        /// 画面内の任意の場所のキャプチャを取得
        /// 1マスを100*100pixとした座標で指定
        /// </summary>
        /// <returns>キャプチャしたbitmapイメージ</returns>
        public Bitmap DoCapture(int x, int y)
        {
            Bitmap bitmap = new Bitmap(100, 100);
            Graphics g = Graphics.FromImage(bitmap);

            var p = new Point(x * 100, y * 100);
            g.CopyFromScreen(p, new Point(0,0), bitmap.Size);

            g.Dispose();

            return bitmap;
        }

        /// <summary>
        /// キャプチャしてそのイメージを返す
        /// 画面内の任意の場所のキャプチャを取得
        /// </summary>
        /// <returns>キャプチャしたbitmapイメージ</returns>
        public Bitmap DoCapture(int sizeX, int sizeY, int x, int y)
        {
            Bitmap bitmap = new Bitmap(sizeX, sizeY);
            Graphics g = Graphics.FromImage(bitmap);

            var p = new Point(x * sizeX, y * sizeY);
            g.CopyFromScreen(p, new Point(0, 0), bitmap.Size);

            g.Dispose();

            return bitmap;
        }

        /// <summary>
        /// スクリーンショットを撮り、指定したサイズの画像の二次元配列にして返す
        /// </summary>
        /// <param name="sizeX">1枚ずつの幅</param>
        /// <param name="sizeY">1枚ずつの長さ</param>
        /// <returns>(x, y)</returns>
        public Bitmap[] DoCaptureReturnArray(int sizeX, int sizeY)
        {
            // 最終的な枚数　一番最後のイメージは引数で指定した大きさと同じとは限らない
            double horizon = Screen.PrimaryScreen.Bounds.Width / sizeX;
            double vertical = Screen.PrimaryScreen.Bounds.Height / sizeY;

            // 上のint変換版　小数点以下は切り上げ
            int horizonCount = (int)Math.Ceiling(horizon);
            int verticalCount = (int)Math.Ceiling(vertical);

            // 戻り値となるBitmap[][]を格納していくための変数
            var result = new Bitmap[horizonCount*verticalCount];

            // スクショを撮影して保存
            var ss = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (var g = Graphics.FromImage(ss))
            {
                g.CopyFromScreen(new Point(0, 0), new Point(0, 0), ss.Size);
            }

            for (int y = 0; y < verticalCount;y++)
            {
                for(int x = 0; x < horizonCount;x++)
                {
                    Bitmap b;

                    // System.Drawing.Imaging.PixelFormat.Format32bppArgbを入れておかないと黒い部分に白いごみが入るらしい
                    // 参考：https://dobon.net/vb/dotnet/graphics/screencapture.html
                    // h、vともに最大なパターン　右下の角部分に相当　両方向に大してサイズが満たせな可能性がある
                    // h、v % 1が0でない場合は割り切れていないため、はみ出ている
                    if (x ==horizonCount && y == verticalCount && horizon % 1 != 0 && vertical % 1 != 0)
                    {
                        //　はみ出している大きさで作成
                        b = new Bitmap(Screen.PrimaryScreen.Bounds.Width - ((horizonCount - 1) * sizeX), Screen.PrimaryScreen.Bounds.Height - ((verticalCount - 1) * sizeY), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    }
                    // hのみ最大なパターン　右端
                    else if (x == horizonCount && horizon % 1 != 0)
                    {
                        b = new Bitmap(Screen.PrimaryScreen.Bounds.Width - ((horizonCount - 1) * sizeX), sizeY, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    }
                    // vのみ最大なパターン　下端
                    else if (y == verticalCount && horizon % 1 != 0)
                    {
                        b = new Bitmap(sizeX, Screen.PrimaryScreen.Bounds.Height - ((verticalCount - 1) * sizeY), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    }
                    // 通常のパターン　h、vともに最大サイズとなる
                    else
                    {
                        b = new Bitmap(sizeX, sizeY, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    }

                    using (var g = Graphics.FromImage(b))
                    {
                        g.DrawImage(ss, new Rectangle(0, 0, b.Width, b.Height), new Rectangle(x*sizeX, y*sizeY, b.Width, b.Height), GraphicsUnit.Pixel);
                    }

                        result[y*verticalCount + x] = b;
                }
            }

            ss.Dispose();
            return result;
        }

        public byte[][] DoCaptureReturnByte()
        {
            var sw = new System.Diagnostics.Stopwatch();

            // 最終的な枚数　一番最後のイメージは引数で指定した大きさと同じとは限らない
            double horizon = Screen.PrimaryScreen.Bounds.Width / sizeX;
            double vertical = Screen.PrimaryScreen.Bounds.Height / sizeY;

            // 戻り値となるBitmap[][]を格納していくための変数
            var result = new byte[HorizonCount*VerticalCount][];

            // スクショを撮影して保存
            var fullSizeSS = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (var g = Graphics.FromImage(fullSizeSS))
            {
                g.CopyFromScreen(new Point(0, 0), new Point(0, 0), fullSizeSS.Size);
            }

            

            var bmpArray = new Bitmap[HorizonCount*VerticalCount];

            //for (int y = 0; y < VerticalCount - 1; y++)
            //{
            //    for (int x = 0; x < HorizonCount + 1; x++)
            //    {
            //        Bitmap b;

            //        // System.Drawing.Imaging.PixelFormat.Format32bppArgbを入れておかないと黒い部分に白いごみが入るらしい
            //        // 参考：https://dobon.net/vb/dotnet/graphics/screencapture.html
            //        // h、vともに最大なパターン　右下の角部分に相当　両方向に大してサイズが満たせな可能性がある
            //        // h、v % 1が0でない場合は割り切れていないため、はみ出ている
            //        if (x == HorizonCount && y == VerticalCount && horizon % 1 != 0 && vertical % 1 != 0)
            //        {
            //            //　はみ出している大きさで作成
            //            b = new Bitmap(Screen.PrimaryScreen.Bounds.Width - ((HorizonCount - 1) * sizeX), Screen.PrimaryScreen.Bounds.Height - ((VerticalCount - 1) * sizeY), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //        }
            //        // hのみ最大なパターン　右端
            //        else if (x == HorizonCount && horizon % 1 != 0)
            //        {
            //            b = new Bitmap(Screen.PrimaryScreen.Bounds.Width - ((HorizonCount - 1) * sizeX), sizeY, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //        }
            //        // vのみ最大なパターン　下端
            //        else if (y == VerticalCount && horizon % 1 != 0)
            //        {
            //            b = new Bitmap(sizeX, Screen.PrimaryScreen.Bounds.Height - ((VerticalCount - 1) * sizeY), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //        }
            //        // 通常のパターン　h、vともに最大サイズとなる
            //        else
            //        {
            //            b = new Bitmap(sizeX, sizeY, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //        }

            //        using (var g = Graphics.FromImage(b))
            //        {
            //            g.DrawImage(fullSizeSS, new Rectangle(0, 0, b.Width, b.Height), new Rectangle(x * sizeX, y * sizeY, b.Width, b.Height), GraphicsUnit.Pixel);
            //        }



            //        bmpArray[y * VerticalCount + x] = b;

            //        // 参考 https://maywork.net/computer/csharp-opencvsharp-commandline/#toc3
            //        // BitmapConverterを使うことでGDI+のBitmapとOpenCvSharpのMat間の相互変換を行えるらしい
            //        // そしてBitmapConverterを使うためには、OpenCvSharpExtensionsをnugetする必要があるしい
            //        //try
            //        //{
            //        //    using (var mat = BitmapConverter.ToMat(fullSizeSS))
            //        //    using (var resized = mat.Clone(new OpenCvSharp.Rect(0, 0, sizeX, sizeY)))
            //        //    {
            //        //        bmpArray[y * VerticalCount + x] = BitmapConverter.ToBitmap(resized);
            //        //    }
            //        //}
            //        //catch(Exception e)
            //        //{
            //        //    System.Diagnostics.Debug.WriteLine(e.StackTrace);
            //        //}
            //    }
            //}

            var fullbmparray = new Bitmap[VerticalCount - 1];
            for (int i = 0; i < fullbmparray.Length; i++)
            {
                fullbmparray[i] = (Bitmap)fullSizeSS.Clone();
            }

            sw.Stop();
            //System.Diagnostics.Debug.WriteLine("全体SS取得:" + sw.ElapsedMilliseconds);
            sw.Restart();

            Parallel.For(0, VerticalCount - 1, y =>
              {
                  for (int x = 0; x < HorizonCount + 1; x++)
                  {
                      Bitmap b;

                    // System.Drawing.Imaging.PixelFormat.Format32bppArgbを入れておかないと黒い部分に白いごみが入るらしい
                    // 参考：https://dobon.net/vb/dotnet/graphics/screencapture.html
                    // h、vともに最大なパターン　右下の角部分に相当　両方向に大してサイズが満たせな可能性がある
                    // h、v % 1が0でない場合は割り切れていないため、はみ出ている
                    if (x == HorizonCount && y == VerticalCount && horizon % 1 != 0 && vertical % 1 != 0)
                      {
                        //　はみ出している大きさで作成
                        b = new Bitmap(Screen.PrimaryScreen.Bounds.Width - ((HorizonCount - 1) * sizeX), Screen.PrimaryScreen.Bounds.Height - ((VerticalCount - 1) * sizeY), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                      }
                    // hのみ最大なパターン　右端
                    else if (x == HorizonCount && horizon % 1 != 0)
                      {
                          b = new Bitmap(Screen.PrimaryScreen.Bounds.Width - ((HorizonCount - 1) * sizeX), sizeY, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                      }
                    // vのみ最大なパターン　下端
                    else if (y == VerticalCount && horizon % 1 != 0)
                      {
                          b = new Bitmap(sizeX, Screen.PrimaryScreen.Bounds.Height - ((VerticalCount - 1) * sizeY), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                      }
                    // 通常のパターン　h、vともに最大サイズとなる
                    else
                      {
                          b = new Bitmap(sizeX, sizeY, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                      }

                      using (var g = Graphics.FromImage(b))
                      {
                          g.DrawImage(fullbmparray[y], new Rectangle(0, 0, b.Width, b.Height), new Rectangle(x * sizeX, y * sizeY, b.Width, b.Height), GraphicsUnit.Pixel);
                      }


                      bmpArray[y * VerticalCount + x] = b;

                    // 参考 https://maywork.net/computer/csharp-opencvsharp-commandline/#toc3
                    // BitmapConverterを使うことでGDI+のBitmapとOpenCvSharpのMat間の相互変換を行えるらしい
                    // そしてBitmapConverterを使うためには、OpenCvSharpExtensionsをnugetする必要があるしい
                    //try
                    //{
                    //    using (var mat = BitmapConverter.ToMat(fullSizeSS))
                    //    using (var resized = mat.Clone(new OpenCvSharp.Rect(0, 0, sizeX, sizeY)))
                    //    {
                    //        bmpArray[y * VerticalCount + x] = BitmapConverter.ToBitmap(resized);
                    //    }
                    //}
                    //catch(Exception e)
                    //{
                    //    System.Diagnostics.Debug.WriteLine(e.StackTrace);
                    //}
                }
              });

            sw.Stop();
            System.Diagnostics.Debug.WriteLine("サイズ縮小:" + sw.ElapsedMilliseconds);
            sw.Restart();

            Parallel.For(0, bmpArray.Length, i =>
            {
                var mem = new System.IO.MemoryStream();
                bmpArray[i].Save(mem, ImageFormat.Jpeg);
                result[i] = mem.GetBuffer();
                bmpArray[i].Dispose();
            });

            //for(int i=0;i<bArray.Length;i++)
            //{
            //    var mem = new System.IO.MemoryStream();
            //    bArray[i].Save(mem, ImageFormat.Jpeg);
            //    result[i] = mem.GetBuffer();
            //    bArray[i].Dispose();
            //}

            sw.Stop();
            System.Diagnostics.Debug.WriteLine("byte[]に加工:" + sw.ElapsedMilliseconds);

            fullSizeSS.Dispose();
            for (int i = 0; i < fullbmparray.Length; i++)
            {
                fullbmparray[i].Dispose();
            }

            return result;
        }

        /// <summary>
        /// ポインタを使って画像を分割して返す
        /// </summary>
        /// <returns></returns>
        public byte[][] DoCaptureNoDrawImage()
        {
            // returnする結果を保存するためのbyte配列
            byte[][] returnByte = new byte[VerticalCount * HorizonCount][];
            // 画面全体のスクショ結果を保存するためのbyte配列
            byte[] imgByte;

            // 画面全体のスクショを取得してimgByteに入れる
            using (var fullSizeSS = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format24bppRgb))
            {
                using (var g = Graphics.FromImage(fullSizeSS))
                {
                    g.CopyFromScreen(new Point(0, 0), new Point(0, 0), fullSizeSS.Size);
                }

                System.Drawing.Imaging.BitmapData bd = fullSizeSS.LockBits(new Rectangle(0, 0, fullSizeSS.Width, fullSizeSS.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, fullSizeSS.PixelFormat);
                var imgByteSize = bd.Stride * fullSizeSS.Height;
                imgByte = new byte[imgByteSize];
                System.Runtime.InteropServices.Marshal.Copy(bd.Scan0, imgByte, 0, imgByteSize);
                fullSizeSS.UnlockBits(bd);
            }

            // そもそもBMPを介さずにbyte[]に書き込んでいくパターン
            // jpegに変換できてないから遅い？
            //Parallel.For(0, VerticalCount - 1, y =>
            //{
            //    for (int x = 0; x < HorizonCount + 1; x++)
            //    {
            //        // 分割後のサイズ(Width=sizeX, Height=sizeY)の画像を保存するためのbyte配列
            //        var splitByte = new byte[sizeX * sizeY * ByteAtPixcel];

            //        // splitByteにはシーケンシャルに書き込んでいくため、連番を保存するカウンターが必要
            //        var sendBmpPosition = 0;
            //        var readStartPosition = (Screen.PrimaryScreen.Bounds.Width * sizeY * y * ByteAtPixcel) + (sizeX * x * ByteAtPixcel);
            //        for (int innerY = 0; innerY < sizeY; innerY++)
            //        {
            //            readStartPosition += Screen.PrimaryScreen.Bounds.Width * ByteAtPixcel;
            //            for (int innerX = 0; innerX < sizeX * ByteAtPixcel; innerX++)
            //            {
            //                splitByte[sendBmpPosition] = imgByte[readStartPosition + innerX];
            //                sendBmpPosition++;
            //            }
            //        }

            //        returnByte[y * VerticalCount + x] = splitByte;
            //    }
            //});

            // 分割する大きさのループ　192, 108の大きさごとに分けるためのループ
            //Parallel.For(0, VerticalCount - 1, y =>
            //  {
            //      for (int x = 0; x < HorizonCount + 1; x++)
            //      {
            //          var sendBmp = new Bitmap(sizeX, sizeY, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //          var bd = sendBmp.LockBits(new Rectangle(0, 0, sendBmp.Width, sendBmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, sendBmp.PixelFormat);

            //          unsafe
            //          {
            //              byte* sendBmpPointer = (byte*)bd.Scan0.ToPointer();

            //            // sendBmpPointerにはシーケンシャルに書き込んでいくため、連番を保存するカウンターが必要
            //            var sendBmpPosition = 0;
            //              var readStartPosition = (Screen.PrimaryScreen.Bounds.Width * sizeY * y * ByteAtPixcel) + (sizeX * x * ByteAtPixcel);
            //              for (int innerY = 0; innerY < sendBmp.Height; innerY++)
            //              {
            //                  readStartPosition += Screen.PrimaryScreen.Bounds.Width * ByteAtPixcel;
            //                  for (int innerX = 0; innerX < bd.Stride; innerX++)
            //                  {
            //                      sendBmpPointer[sendBmpPosition] = imgByte[readStartPosition + innerX];
            //                      sendBmpPosition++;
            //                  }
            //              }
            //          }
            //          sendBmp.UnlockBits(bd);
            //          var mStream = new System.IO.MemoryStream();
            //          sendBmp.Save(mStream, ImageFormat.Jpeg);
            //          sendBmp.Dispose();
            //          returnByte[y * VerticalCount + x] = mStream.ToArray();
            //          mStream.Dispose();
            //      }
            //  });



            return returnByte;
        }

        public Bitmap DoCaptureFullSS()
        {
            // スクショを撮影して保存
            var fullSizeSS = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(fullSizeSS))
            {
                g.CopyFromScreen(new Point(0, 0), new Point(0, 0), fullSizeSS.Size);
            }

            return fullSizeSS;
        }

        /// <summary>
        /// image1とimage2を比較して結果を返す
        /// ↓から借りた
        /// http://tinqwill.blog59.fc2.com/blog-entry-46.html
        /// </summary>
        /// <param name="image1"></param>
        /// <param name="image2"></param>
        /// <returns></returns>
        public static bool CompareImage(Image image1, Image image2)
        {
            Bitmap img1 = (Bitmap)image1;
            Bitmap img2 = (Bitmap)image2;

            //高さが違えばfalse
            if (img1.Width != img2.Width || img1.Height != img2.Height) return false;
            //BitmapData取得
            BitmapData bd1 = img1.LockBits(new Rectangle(0, 0, img1.Width, img1.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, img1.PixelFormat);
            BitmapData bd2 = img2.LockBits(new Rectangle(0, 0, img2.Width, img2.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, img2.PixelFormat);
            //スキャン幅が違う場合はfalse
            if (bd1.Stride != bd2.Stride)
            {
                //ロックを解除
                img1.UnlockBits(bd1);
                img2.UnlockBits(bd2);
                return false;
            }
            int bsize = bd1.Stride * img1.Height;
            byte[] byte1 = new byte[bsize];
            byte[] byte2 = new byte[bsize];
            //バイト配列にコピー
            Marshal.Copy(bd1.Scan0, byte1, 0, bsize);
            Marshal.Copy(bd2.Scan0, byte2, 0, bsize);
            //ロックを解除
            img1.UnlockBits(bd1);
            img2.UnlockBits(bd2);

            //MD5ハッシュを取る
            System.Security.Cryptography.MD5CryptoServiceProvider md5 =
                new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hash1 = md5.ComputeHash(byte1);
            byte[] hash2 = md5.ComputeHash(byte2);

            //ハッシュを比較
            return hash1.SequenceEqual(hash2);
        }

        //public static bool CompareImage(byte[] b1, byte[] b2)
        //{
        //    // MD5を使用するパターンとbyteごとにXORで比較していくパターンを比較したところ
        //    // MD5 ：57345ms
        //    // XOR ：33492ms
        //    // でXORの方が高速っぽい　あとMD5は何故か分からないけど使用メモリがめっちゃ増えていく
        //    // GCが作動しないように作ればMD5の方が早いのかもしれない


        //    // 大きさが違う場合はfalse
        //    if(b1.Length != b2.Length)
        //    {
        //        return false;
        //    }

        //    //System.Security.Cryptography.MD5CryptoServiceProvider md5 =
        //    //    new System.Security.Cryptography.MD5CryptoServiceProvider();

        //    //return md5.ComputeHash(b1).SequenceEqual(md5.ComputeHash(b2));

        //    for (int i = 0; i < b1.Length; i++)
        //    {
        //        if ((b1[i] ^ b2[i]) != 0)
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}

        public static bool CompareImage(byte[] b1, byte[] b2)
        {
            // 大きさが違う場合はfalse
            if (b1.Length != b2.Length)
            {
                return false;
            }

            // 大きさが同じことはチェック済みだからb1だけすれば良い
            // これは要素数(Length)じゃなくてbit単位の大きさ
            // https://shibuya24.info/entry/cs2cpp_array
            int size = Marshal.SizeOf(typeof(byte)) * b1.Length;

            IntPtr b1Ptr = Marshal.AllocCoTaskMem(b1.Length);
            IntPtr b2Ptr = Marshal.AllocCoTaskMem(b2.Length);

            Marshal.Copy(b1, 0, b1Ptr, size);
            Marshal.Copy(b2, 0, b2Ptr, size);

            var result = main(b1Ptr, b2Ptr, b1.Length);

            Marshal.FreeCoTaskMem(b1Ptr);
            Marshal.FreeCoTaskMem(b2Ptr);

            if (result == 0)
            {
                return true;
            }

            return false;
        }
    }
}

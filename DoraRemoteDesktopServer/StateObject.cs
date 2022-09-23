using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DoraRemoteDesktopClient
{
    class StateObject
    {
        // 非同期処理でソケット情報を保持する為のオブジェクト
        // 受信バッファサイズ
        public const int BufferSize = 1024;

        // 受信バッファ
        public byte[] buffer = new byte[BufferSize];

        // 受信データ
        public StringBuilder sb = new StringBuilder();

        // ソケット
        public Socket workSocket = null;
    }
}

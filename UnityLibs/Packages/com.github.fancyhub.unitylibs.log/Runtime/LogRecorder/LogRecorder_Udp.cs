/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/09/05
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;


namespace FH
{
     
     
    public class LogRecorder_Udp : ILogRecorder
    {
        public const int C_UDP_PACKAGE_SIZE = 600;
        public const string C_REMOTE_ADDRESS = "";
        public const int C_REMOTE_PORT = 1000;

        private StringWriter _string_writer;
        public System.Net.Sockets.UdpClient _client;
        public System.Net.IPEndPoint _remote;
        public System.Net.IPAddress[] _remote_address;

        public LogRecorder_Udp()
        {
            var host_entry = Dns.GetHostEntry(C_REMOTE_ADDRESS);
            if (host_entry == null || host_entry.AddressList == null || host_entry.AddressList.Length == 0)
                return;
            _remote_address = host_entry.AddressList;
            _client = new System.Net.Sockets.UdpClient();
            _remote = new IPEndPoint(_remote_address[0], C_REMOTE_PORT);
            _string_writer = new StringWriter(C_UDP_PACKAGE_SIZE, System.Text.Encoding.UTF8);
        }

        public void Record(List<string> msg_list)
        {
            if (_client == null || msg_list == null || msg_list.Count == 0)
                return;

            foreach (var msg in msg_list)
            {
                if (msg.Length == 0)
                    continue;

                int str_index = 0;
                for (; ; )
                {
                    if (_string_writer.Write(msg, ref str_index) == 0)
                    {
                        _client.Send(_string_writer.Buffer, _string_writer.Position, _remote);
                        _string_writer.Clear();
                    }

                    if (str_index >= msg.Length)
                        break;
                }
            }

            if (_string_writer.Position > 0)
            {
                _client.Send(_string_writer.Buffer, _string_writer.Position, _remote);
                _string_writer.Clear();
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
            _client = null;
        }
    }
}

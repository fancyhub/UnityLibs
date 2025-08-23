/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/09/05
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Net;

namespace FH
{

    public class LogRecorder_Udp : ILogRecorder
    {
        public const int C_UDP_PACKAGE_SIZE = 600;
        
        private StringWriter _string_writer;
        public System.Net.Sockets.UdpClient _client;
        public System.Net.IPEndPoint _remote;

        public LogRecorder_Udp(IPEndPoint remote)
        {
            _remote = remote;
            _string_writer = new StringWriter(C_UDP_PACKAGE_SIZE, System.Text.Encoding.UTF8);
        }

        public static IPEndPoint CreateRemoteIP(string address,int port)
        {
            var host_entry = Dns.GetHostEntry(address);
            if (host_entry == null )
                return null;
            System.Net.IPAddress[] address_list = host_entry.AddressList;
            if (address_list == null || address_list.Length == 0)
                return null;
            return new IPEndPoint(address_list[0], port);
        }

        public void Start()
        {
            _client = new System.Net.Sockets.UdpClient();
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

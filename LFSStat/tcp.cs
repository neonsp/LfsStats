/*
    LFSStat, Insim Replay statistics for Live For Speed Game
    Copyright (C) 2008 Jaroslav Èerný alias JackCY, Robert B. alias Gai-Luron and Monkster.
    Jack.SC7@gmail.com, lfsgailuron@free.fr

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
namespace TcpConnection
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.IO;

    public class Connection
    {
        public enum State
        {
            Disconnected,
            Connected
        }
        public delegate void DelegToWrite(string Text);

        private State _StateConn;
        private DelegToWrite _OnTextToWrite;
        private TcpClient _TcpConnect;
        private NetworkStream _Ns;
        private string _HostName;
        private IPAddress _IP;
        private int _Port;
        private string _Sent;
        private string _Received;
        private byte[] _bReceived;
        private int _RecSize;

        public State StateConn
        {
            get { return _StateConn; }
        }

        public DelegToWrite OnTextToWrite
        {
            get { return _OnTextToWrite; }
            set { _OnTextToWrite = value; }
        }

        public string HostName
        {
            get { return _HostName; }
        }

        public IPAddress IP
        {
            get { return _IP; }
        }

        public int Port
        {
            get { return _Port; }
        }

        public string Sent
        {
            get { return _Sent; }
        }

        public string Received
        {
            get { return _Received; }
        }

        public string AsyncReceived
        {
            get { return Encoding.ASCII.GetString(_bReceived, 0, _RecSize); }
        }

        public Connection(string HostName, int Port)
        {
            _StateConn = State.Disconnected;
            _HostName = HostName;
            _Port = Port;
            _bReceived = new byte[1024];

			IPHostEntry HostInfo = Dns.GetHostEntry(HostName);
            //IPHostEntry HostInfo = Dns.Resolve(HostName);
            _IP = HostInfo.AddressList[0];
        }

        public Connection(IPAddress IP, int Port)
        {
            _StateConn = State.Disconnected;
            _HostName = String.Empty;
            _IP = IP;
            _Port = Port;
            _bReceived = new byte[1024];
        }

        public void Connect()
        {
            if (_StateConn == State.Connected)
                return;

            try
            {
                _TcpConnect = new TcpClient();
                _TcpConnect.Connect(new IPEndPoint(_IP, _Port));
                _Ns = _TcpConnect.GetStream();
                _StateConn = State.Connected;
            }
            catch (Exception e)
            {
                _StateConn = State.Disconnected;
                throw e;
            }
        }

        public void Disconnect()
        {
            try
            {
                _Ns.Close();
                _TcpConnect.Close();
            }
            catch
            {
            }
            finally
            {
                _StateConn = State.Disconnected;
            }
        }

        public void SendToServer(byte[] bText, int len )
        {
            if (_StateConn == State.Disconnected)
                return;
            _Sent = bText.ToString();
            try
            {
                _Ns.Write(bText, 0, bText.Length);
            }
            catch
            {
                try
                {
                    _Ns.Write(bText, 0, bText.Length);
                }
                catch (Exception e)
                {
                    _StateConn = State.Disconnected;
                    throw e;
                }
            }
        }
        public void SendToServer(string Text)
        {

            byte[] bText = Encoding.ASCII.GetBytes(Text.ToCharArray());
            SendToServer(bText, bText.Length);
        }

        public byte[] GetPackFromInsimServer()
        {
            _Received = String.Empty;
            try
            {

                int len = _Ns.ReadByte();
                if (len != -1)
                {
                    byte[] buff = new byte[len];
                    _Ns.Read(buff, 1, (int)(len - 1));
                    buff[0] = (byte)len;
                    return buff;
                }
                else
                {
                    System.Environment.Exit(-1);
                    return null;
                }
            }
            catch (Exception e)
            {
                return null;
                throw e;
            }

        }
        public string GetFromServer()
        {
            if (_StateConn == State.Disconnected)
                return String.Empty;

            StreamReader Sr = null;
            _Received = String.Empty;

            try
            {
                Sr = new StreamReader(_Ns);

                while (Sr.Peek() > -1)
                {
                    _Received += Sr.ReadLine() + "\r\n";
                }

                return _Received;
            }
            catch (Exception e)
            {
                _Received = String.Empty;
                throw e;
            }
        }

        public void GetFromServerAsync()
        {
            AsyncCallback Acb = new AsyncCallback(OnReceived);
            _Ns.BeginRead(_bReceived, 0, _bReceived.Length, Acb, _TcpConnect);
        }

        private void OnReceived(IAsyncResult Ar)
        {
            TcpClient Client = (TcpClient)Ar.AsyncState;
            NetworkStream Ns = Client.GetStream();

            int Size = Ns.EndRead(Ar);

            if (Size > 0)
            {
                _RecSize = Size;

                if (_OnTextToWrite != null)
                    _OnTextToWrite(AsyncReceived);

                GetFromServerAsync();
            }
            else
            {
                _StateConn = State.Disconnected;
            }
        }

        public override string ToString()
        {
            return _IP.ToString();
        }
    }
}


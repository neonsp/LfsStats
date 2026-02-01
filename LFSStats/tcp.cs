/*
	LFSStat, Insim Replay statistics for Live For Speed Game
	Copyright (C) 2008 Jaroslav Èerný alias JackCY, Robert B. alias Gai-Luron and Monkster.
	Jack.SC7@gmail.com, lfsgailuron@free.fr

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either Version 3 of the License, or
	(at your option) any later Version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

/*
// Asynchronous TCP connect with timeout
using (TcpClient tcp = new TcpClient())  
{  
	IAsyncResult ar = tcp.BeginConnect("127.0.0.1", 80, null, null);  
	System.Threading.WaitHandle wh = ar.AsyncWaitHandle;  
	try 
	{  
	   if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5), false))  
	   {  
		   tcp.Close();  
		   throw new TimeoutException();  
	   }  
 
		tcp.EndConnect(ar);  
	}  
	finally 
	{  
		wh.Close();  
	}  
} 
 */

namespace TcpConnection
{
	using System;
	using System.Net;
	using System.Net.Sockets;
	using System.Text;
	using System.IO;
	using LFSStatistics;

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
		private TcpClient _TcpClient;
		private NetworkStream _NetworkStream;
		private IPEndPoint ipEndPoint;
		//private string _HostName;
		//private int _Port;
		//private IPAddress _IP;
		//private string _Sent;
		//private string _Received;
		//private byte[] _ReceivedBytes;
		// Preventa attacks that would cause eating the whole memory, Insim has a maximum packet size of 292 it seems from insim.txt
		private static int securityMaxPacketSize = 4000;	// 4kB

		public State StateConn
		{
			get { return _StateConn; }
		}
		public DelegToWrite OnTextToWrite
		{
			get { return _OnTextToWrite; }
			set { _OnTextToWrite = value; }
		}
		public IPAddress IP
		{
			get { return ipEndPoint.Address; }
		}
		public int Port
		{
			get { return ipEndPoint.Port; }
		}
		//public string Sent
		//{
		//    get { return _Sent; }
		//}
		//public string Received
		//{
		//    get { return _Received; }
		//}
		/*
		public string AsyncReceived
		{
			get { return Encoding.ASCII.GetString(_ReceivedBytes, 0, _RecSize); }
		}*/

		public Connection(IPEndPoint ipEndPoint)
		{
			_StateConn = State.Disconnected;
			this.ipEndPoint = ipEndPoint;
			//_HostName = hostName;
			//_Port = port;
			//_ReceivedBytes = new byte[1024];

			//IPHostEntry hostInfo = Dns.GetHostEntry(hostName);
			//if (hostInfo.AddressList.Length > 0)
			//    _IP = hostInfo.AddressList[0];
			//else
			//{
			//    LFSStats.ErrorWriteLine("Host " + hostName + ":" + port + " not found, forced shutdown.");
			//    LFSStats.Exit(1);
			//}
		}

		//public Connection(IPAddress IP, int Port)
		//{
		//    _StateConn = State.Disconnected;
		//    _HostName = String.Empty;
		//    _IP = IP;
		//    _Port = Port;
		//    _ReceivedBytes = new byte[1024];
		//}

		/// <summary>
		/// Connects via TCP to IP:port
		/// </summary>
		/// <returns>True if connected, false otherwise</returns>
		public bool Connect()
		{
			if (_StateConn == State.Connected)
				return true;
			try
			{
				_TcpClient = new TcpClient();
				_TcpClient.Connect(ipEndPoint);

				if (_TcpClient.Connected)
				{
					_NetworkStream = _TcpClient.GetStream();
					//_NetworkStream.ReadTimeout = 100;	// 100ms time out
					//_NetworkStream.WriteTimeout = 100;	// 100ms time out
#if DEBUG
					Console.WriteLine("\nTCP: RTout " + _TcpClient.ReceiveTimeout + " STout " + _TcpClient.SendTimeout);
					Console.WriteLine("NSt: RTout " + _NetworkStream.ReadTimeout + " WTout " + _NetworkStream.WriteTimeout);
#endif
					_StateConn = State.Connected;
					return true;
				}
				else return false;
			}
			catch
			{
				_StateConn = State.Disconnected;
				return false;
			}
		}

		public void Disconnect()
		{
			try
			{
				_NetworkStream.Close();
				_TcpClient.Close();
			}
			finally
			{
				_StateConn = State.Disconnected;
			}
		}

		public void SendToServer(byte[] bText, int len)
		{
			if (_StateConn == State.Disconnected)
				return;
			//_Sent = bText.ToString();

			for (int i = 0; i < 10; i++)
			{
				try
				{
					_NetworkStream.Write(bText, 0, bText.Length);
					return;
				}
				//catch (IOException) { }	// write timeout
				catch (Exception ex)
				{
					if (i < 10)
					{
						LFSStats.ErrorWriteLine("SendToServer: Stream.Write failed, retrying " + i);
					}
					else
					{
						_StateConn = State.Disconnected;
						LFSStats.ErrorWriteException("SendToServer: Stream.Write failed, forced shutdown.", ex);
						LFSStats.Exit(1);
					}
				}
			}
		}

		//public void SendToServer(string Text)
		//{
		//    byte[] bText = Encoding.ASCII.GetBytes(Text.ToCharArray());
		//    SendToServer(bText, bText.Length);
		//}

		public byte[] Receive()
		{
#if true
			int readLength;
			byte[] buff;
			try
			{
				//if (!_TcpClient.Connected) throw new Exception("TCP disconnected.");

				int len = _NetworkStream.ReadByte();	// reads size of packet, should be always first byte in Insim packets

				if (len > securityMaxPacketSize)		// packet is too big, invalid or an attack
				{
					LFSStats.ErrorWriteLine("Receive: received packet is above a security limit of " + securityMaxPacketSize + "B.");
					// empties the TCP network stream from the rubbish received
					buff = new byte[securityMaxPacketSize];
					int lenAboveLimit = len - 1;	// read packet bytes but minus the already acquired first length byte
					// read into buffer while the rubbish is not gone from stream
					while (lenAboveLimit > 0)
					{
						// length to read has to be within the size of passed buffer
						len = (lenAboveLimit > securityMaxPacketSize) ? securityMaxPacketSize : lenAboveLimit;
						readLength = _NetworkStream.Read(buff, 0, len);
						lenAboveLimit -= readLength;
					}
					return null;
				}
				else if (len > 0)
				{
					buff = new byte[len];	// buffer to store the packet of length len
					int idxBuff = 1;		// start storing data at this index
					len -= 1;				// read packet bytes but minus the already acquired first length byte
					// read into buffer while length is not zero
					while (len > 0) // French:"I have read till a complete packet, freeze??, I do not think"
					{
						// reads bytes from stream, but it may not yet have all of them hence it returns how many was acquired
						// so we can collect the whole packet
						readLength = _NetworkStream.Read(buff, idxBuff, len);
						// move the index for writing into our buffer by the number of bytes acquired
						idxBuff += readLength;
						// reduce the remaining length by the number of bytes acquired
						len -= readLength;
					}
					buff[0] = (byte)idxBuff;	// writes the real and hopefully equal size byte at the beginning
					return buff;				// returns the acquired data
				}
				else
				{
					if (!LFSStats.HasShutdownStarted)
					{
						LFSStats.ErrorWriteLine("Receive: TCP stream empty, forced shutdown.");
						LFSStats.Exit(1);
					}
					return null;
				}
			}
			//catch (IOException)		// read timeout
			//{
			//    return null;
			//}
			catch (Exception ex)
			{
				LFSStats.ErrorWriteException("Receive: failed.", ex);
				return null;
			}
#else
			_Received = String.Empty;
			try
			{
				if (!_TcpClient.Connected) throw new Exception("TCP disconnected.");

				int len = _NetworkStream.ReadByte();
				if (len != -1)
				{
					byte[] buff = new byte[len];
					_NetworkStream.Read(buff, 1, len - 1);
					buff[0] = (byte)len;
					return buff;
				}
				else
				{
					//throw new Exception("TCP Stream empty.");
					LFSStats.ErrorWriteLine("Receive: TCP Stream empty, forced shutdown.");
					LFSStats.Exit(1);
					return null;
				}
			}
			catch (Exception ex)
			{
				LFSStats.ErrorWriteException("Receive: failed.", ex);
				return null;
			}
#endif
		}
		// Never used
		/*public string GetFromServer()
		{
			if (_StateConn == State.Disconnected)
				return String.Empty;

			StreamReader Sr = null;
			_Received = String.Empty;

			try
			{
				Sr = new StreamReader(_NetworkStream);

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
			_NetworkStream.BeginRead(_ReceivedBytes, 0, _ReceivedBytes.Length, Acb, _TcpClient);
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
		}*/

		public override string ToString()
		{
			return ipEndPoint.ToString();
		}
	}
}


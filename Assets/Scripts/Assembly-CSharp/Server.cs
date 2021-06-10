﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

// Token: 0x020000A0 RID: 160
public class Server
{
	// Token: 0x17000034 RID: 52
	// (get) Token: 0x060004C0 RID: 1216 RVA: 0x00017D3C File Offset: 0x00015F3C
	// (set) Token: 0x060004C1 RID: 1217 RVA: 0x00017D43 File Offset: 0x00015F43
	public static int MaxPlayers { get; private set; }

	// Token: 0x17000035 RID: 53
	// (get) Token: 0x060004C2 RID: 1218 RVA: 0x00017D4B File Offset: 0x00015F4B
	// (set) Token: 0x060004C3 RID: 1219 RVA: 0x00017D52 File Offset: 0x00015F52
	public static int Port { get; private set; }

	// Token: 0x060004C4 RID: 1220 RVA: 0x00017D5A File Offset: 0x00015F5A
	public static int GetNextId()
	{
		int result = Server.idCounter;
		Server.idCounter++;
		return result;
	}

	// Token: 0x060004C5 RID: 1221 RVA: 0x00017D70 File Offset: 0x00015F70
	public static void Start(int maxPlayers, int port)
	{
		Server.MaxPlayers = maxPlayers;
		Server.Port = port;
		Debug.Log("Starting server.. ver.0.7");
		Server.InitializeServerData();
		Server.tcpListener = new TcpListener(Server.ipAddress, Server.Port);
		Debug.Log(string.Format("TclpListener on IP: {0}.", Server.ipAddress));
		Server.tcpListener.Start();
		Server.tcpListener.BeginAcceptTcpClient(new AsyncCallback(Server.TCPConnectCallback), null);
		Server.udpListener = new UdpClient(Server.Port);
		Server.udpListener.BeginReceive(new AsyncCallback(Server.UDPReceiveCallback), null);
		Debug.Log("Server started on port:" + Server.Port);
		ThreadManagerServer.Instance.ResetGame();
	}

	// Token: 0x060004C6 RID: 1222 RVA: 0x00017E2C File Offset: 0x0001602C
	private static void TCPConnectCallback(IAsyncResult result)
	{
		TcpClient tcpClient = Server.tcpListener.EndAcceptTcpClient(result);
		Server.tcpListener.BeginAcceptTcpClient(new AsyncCallback(Server.TCPConnectCallback), null);
		Debug.Log(string.Format("Incoming connection from {0}...", tcpClient.Client.RemoteEndPoint));
		for (int i = 1; i <= Server.MaxPlayers; i++)
		{
			if (Server.clients[i].tcp.socket == null)
			{
				Server.clients[i].tcp.Connect(tcpClient);
				return;
			}
		}
		Debug.Log(string.Format("{0} failed to connect: Server full! f", tcpClient.Client.RemoteEndPoint));
	}

	// Token: 0x060004C7 RID: 1223 RVA: 0x00017ED0 File Offset: 0x000160D0
	private static void UDPReceiveCallback(IAsyncResult result)
	{
		try
		{
			IPEndPoint ipendPoint = new IPEndPoint(IPAddress.Any, 0);
			byte[] array = Server.udpListener.EndReceive(result, ref ipendPoint);
			Server.udpListener.BeginReceive(new AsyncCallback(Server.UDPReceiveCallback), null);
			if (array.Length >= 4)
			{
				using (Packet packet = new Packet(array))
				{
					int num = packet.ReadInt(true);
					if (num != 0)
					{
						if (Server.clients[num].udp.endPoint == null)
						{
							Server.clients[num].udp.Connect(ipendPoint);
						}
						else if (Server.clients[num].udp.endPoint.ToString() == ipendPoint.ToString())
						{
							Server.clients[num].udp.HandleData(packet);
						}
					}
				}
			}
		}
		catch (Exception arg)
		{
			Debug.Log(string.Format("Catching error receiving UDP data: {0}", arg));
			Debug.Log("This error message can be ignored if just closing server. Server has been closed successfully.");
		}
	}

	// Token: 0x060004C8 RID: 1224 RVA: 0x00017FE8 File Offset: 0x000161E8
	public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet)
	{
		try
		{
			if (clientEndPoint != null)
			{
				Server.udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
			}
		}
		catch (Exception arg)
		{
			Debug.Log(string.Format("Error sending data to {0} via UDP: {1}.", clientEndPoint, arg));
		}
	}

	// Token: 0x060004C9 RID: 1225 RVA: 0x00018038 File Offset: 0x00016238
	public static void InitializeServerData()
	{
		for (int i = 1; i <= Server.MaxPlayers; i++)
		{
			Server.clients.Add(i, new Client(i));
		}
		Server.InitializeServerPackets();
		Debug.Log("Initialized Packets.");
	}

	// Token: 0x060004CA RID: 1226 RVA: 0x00018078 File Offset: 0x00016278
	public static void InitializeServerPackets()
	{
		Server.PacketHandlers = new Dictionary<int, Server.PacketHandler>
		{
			{
				1,
				new Server.PacketHandler(ServerHandle.WelcomeReceived)
			},
			{
				2,
				new Server.PacketHandler(ServerHandle.JoinRequest)
			},
			{
				29,
				new Server.PacketHandler(ServerHandle.PlayerFinishedLoading)
			},
			{
				5,
				new Server.PacketHandler(ServerHandle.PlayerDisconnect)
			},
			{
				3,
				new Server.PacketHandler(ServerHandle.PlayerPosition)
			},
			{
				26,
				new Server.PacketHandler(ServerHandle.PlayerHp)
			},
			{
				27,
				new Server.PacketHandler(ServerHandle.PlayerDied)
			},
			{
				31,
				new Server.PacketHandler(ServerHandle.RevivePlayer)
			},
			{
				4,
				new Server.PacketHandler(ServerHandle.PlayerRotation)
			},
			{
				6,
				new Server.PacketHandler(ServerHandle.PingReceived)
			},
			{
				9,
				new Server.PacketHandler(ServerHandle.PlayerRequestedSpawns)
			},
			{
				8,
				new Server.PacketHandler(ServerHandle.Ready)
			},
			{
				10,
				new Server.PacketHandler(ServerHandle.ItemDropped)
			},
			{
				11,
				new Server.PacketHandler(ServerHandle.ItemDroppedAtPosition)
			},
			{
				12,
				new Server.PacketHandler(ServerHandle.ItemPickedUp)
			},
			{
				13,
				new Server.PacketHandler(ServerHandle.WeaponInHand)
			},
			{
				15,
				new Server.PacketHandler(ServerHandle.AnimationUpdate)
			},
			{
				28,
				new Server.PacketHandler(ServerHandle.ShootArrow)
			},
			{
				14,
				new Server.PacketHandler(ServerHandle.PlayerHitObject)
			},
			{
				30,
				new Server.PacketHandler(ServerHandle.SpawnEffect)
			},
			{
				20,
				new Server.PacketHandler(ServerHandle.PlayerHit)
			},
			{
				16,
				new Server.PacketHandler(ServerHandle.RequestBuild)
			},
			{
				17,
				new Server.PacketHandler(ServerHandle.RequestChest)
			},
			{
				18,
				new Server.PacketHandler(ServerHandle.UpdateChest)
			},
			{
				19,
				new Server.PacketHandler(ServerHandle.ItemInteract)
			},
			{
				21,
				new Server.PacketHandler(ServerHandle.PlayerDamageMob)
			},
			{
				22,
				new Server.PacketHandler(ServerHandle.ShrineCombatStartRequest)
			},
			{
				23,
				new Server.PacketHandler(ServerHandle.ReceiveChatMessage)
			},
			{
				24,
				new Server.PacketHandler(ServerHandle.ReceivePing)
			},
			{
				25,
				new Server.PacketHandler(ServerHandle.ReceiveArmor)
			}
		};
	}

	// Token: 0x060004CB RID: 1227 RVA: 0x000182E0 File Offset: 0x000164E0
	public static void Stop()
	{
		Server.tcpListener.Stop();
		Server.udpListener.Close();
	}

	// Token: 0x04000422 RID: 1058
	public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

	// Token: 0x04000423 RID: 1059
	public static Dictionary<int, Server.PacketHandler> PacketHandlers;

	// Token: 0x04000424 RID: 1060
	public static int idCounter;

	// Token: 0x04000425 RID: 1061
	private static TcpListener tcpListener;

	// Token: 0x04000426 RID: 1062
	private static UdpClient udpListener;

	// Token: 0x04000427 RID: 1063
	public static IPAddress ipAddress = IPAddress.Any;

	// Token: 0x02000128 RID: 296
	// (Invoke) Token: 0x060007CD RID: 1997
	public delegate void PacketHandler(int fromClient, Packet packet);
}

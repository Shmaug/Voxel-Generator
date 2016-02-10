using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using VoxelGenerator.Dynamic;
using VoxelGenerator.Environment;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace VoxelGenerator.Network {
    enum PacketType {

    }
    class Client {
        public int Ping;
        public IPAddress IP;
        public int ID;
        public string Name;

        public Client(IPAddress ip) {
            IP = ip;
        }
    }
    public class Server {
        Process serverProcess;

        private World world;
        private Player[] players;
        public bool Running = false;
        public int Port = 7777;

        Thread worldThread;
        Thread netThread;

        UdpClient UdpClient;
        
        ServerControl Window;

        internal World World {
            get {
                return world;
            }
            private set {
                world = value;
            }
        }
        internal Player[] Players {
            get {
                return players;
            }
            private set {
                players = value;
            }
        }

        public Server(int maxPlayers = 6) {
            World = new World(5, true);
            worldThread = new Thread(WorldLoop);
            worldThread.Name = "World Update";
            netThread = new Thread(NetLoop);
            netThread.Name = "Network Update";

            Players = new Player[maxPlayers];

            serverProcess = Process.GetCurrentProcess();
        }

        public void Start() {
            Running = true;

            Window = new ServerControl(this);

            {
                string local = "";
                foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList) {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        local = ip.ToString();
                }
                string external = new WebClient().DownloadString("http://ipinfo.io/ip");
                external = external.Substring(0, external.Length - 1); // trim off \n

                Window.ipLabel.Text = String.Format("Local IP:      {0}\nExternal IP: {1}:{2}", local, external, Port);
            }


            UdpClient = new UdpClient(Port);

            worldThread.Start();
            netThread.Start();

            Window.ShowDialog();
        }

        void WorldLoop(object ctx) {
            Stopwatch timer = new Stopwatch();

            while (Running) {
                float time = (float)timer.Elapsed.TotalSeconds;
                timer.Restart();

                World.Update(time);

                for (int i = 0; i < Players.Length; i++)
                    if (Players[i] != null)
                        Players[i].update(time); // entity update

                List<Vector3> playerPos = new List<Vector3>();
                playerPos.Add(Vector3.Zero);
                for (int i = 0; i < Players.Length; i++)
                    if (Players[i] != null)
                        playerPos.Add(Players[i].Position);

                bool modified = World.LoadChunks(null, playerPos.ToArray());
                if (modified) {
                    Window.Dispatcher.Invoke(() => {
                        Window.chunkLoadLabel.Content = World.Chunks.Count + " chunks loaded:";
                        Window.redrawMap();
                    });
                }
            }
        }
        
        async void NetLoop(object ctx) {
            while (Running) {
                UdpReceiveResult result = await UdpClient.ReceiveAsync();
                byte[] buffer = result.Buffer;
            }
        }
    }
}

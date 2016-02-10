using System;

namespace VoxelGenerator {
#if WINDOWS || XBOX
    static class Program
    {
        [STAThread]
        static void Main(string[] args) {
            if (args.Length > 0 && args[0] == "server") {
                VoxelGenerator.Network.Server server = new VoxelGenerator.Network.Server();
                server.Start();
            } else {
                using (Main game = new Main()) {
                    game.Run();
                }
            }
        }
    }
#endif
}


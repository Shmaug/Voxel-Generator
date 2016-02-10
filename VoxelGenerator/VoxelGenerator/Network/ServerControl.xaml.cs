using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VoxelGenerator.Network {
    public partial class ServerControl : Window, System.Windows.Markup.IComponentConnector {
        Server Server;

        public ServerControl(Server server) {
            Server = server;

            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e) {
            Server.Running = false;
            base.OnClosing(e);
        }

        private void textBoxFocused(object sender, RoutedEventArgs e) {
            TextBox box = sender as TextBox;
            box.Text = "";
            box.Foreground = Brushes.Black;
            box.GotFocus -= textBoxFocused;
        }

        Point lastPos = new Point(0, 0);
        Point canvasOffset = new Point(20, 20);
        private void mapGrid_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                lastPos = e.GetPosition(mapGrid);
            }
        }
        private void mapCanvas_MouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                canvasOffset.X -= (e.GetPosition(mapGrid).X - lastPos.X) / (float)VoxelGenerator.Environment.Chunk.CHUNK_SIZE;
                canvasOffset.Y -= (e.GetPosition(mapGrid).Y - lastPos.Y) / (float)VoxelGenerator.Environment.Chunk.CHUNK_SIZE;

                lastPos = e.GetPosition(mapGrid);

                redrawMap();
            }
        }
        private void mapCanvas_CenterView(object sender, EventArgs e) {
            canvasOffset.X = mapGrid.Width / 2;
            canvasOffset.Y = mapGrid.Height / 2;
            redrawMap();
        }

        public void redrawMap() {
            mapGrid.Children.Clear();

            int chunkSize = VoxelGenerator.Environment.Chunk.CHUNK_SIZE;
            int offX = (int)canvasOffset.X;
            int offZ = (int)canvasOffset.Y;
            
            foreach (VoxelGenerator.Environment.Chunk c in Server.World.Chunks)
                    mapGrid.Children.Add(new Rectangle() {
                        Fill = Brushes.Green,
                        RenderTransform = new TranslateTransform((c.X - offX) * chunkSize, (c.Z - offZ) * chunkSize),
                        Height = chunkSize,
                        Width = chunkSize
                    });
            foreach (VoxelGenerator.Dynamic.Player p in Server.Players)
                if (p != null)
                    mapGrid.Children.Add(new Rectangle() {
                        Fill = Brushes.Blue,
                        RenderTransform = new TranslateTransform(p.Position.X, p.Position.Z),
                        Height = 3,
                        Width = 3
                    });
        }

    }
}

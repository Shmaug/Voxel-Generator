using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelGenerator.UI
{
    class Screen
    {
        public static Dictionary<string, Screen> Screens = new Dictionary<string, Screen>();
        
        public Dictionary<string, UIElement> Elements;
        public Rectangle Bounds;
        public bool Maximize;
        public string Name;
        public bool Visible;

        public Screen(bool visible, string name, Rectangle bounds, bool maximize = true) {
            Visible = visible;
            Elements = new Dictionary<string, UIElement>();
            Name = name;
            Bounds = bounds;
            Maximize = maximize;
        }

        public static void Update(GameTime time, Point ms, Rectangle screen, bool isclick) {
            foreach (KeyValuePair<string, Screen> s in Screens)
                if (s.Value.Visible)
                    foreach (KeyValuePair<string, UIElement> e in s.Value.Elements)
                        e.Value.Update(time, ms, screen, isclick);
        }

        public static void Draw(SpriteBatch batch, Rectangle screen) {
            foreach (KeyValuePair<string, Screen> s in Screens)
                if (s.Value.Visible)
                    foreach (KeyValuePair<string, UIElement> e in s.Value.Elements)
                        e.Value.Draw(batch, s.Value.Maximize ? screen : s.Value.Bounds);
        }
    }
}

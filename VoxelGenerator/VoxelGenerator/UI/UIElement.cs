using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelGenerator.UI
{
    public class UDim2 {
        public Vector2 Scale;
        public Vector2 Offset;

        public UDim2(float sX, float sY, float oX, float oY) {
            Scale = new Vector2(sX, sY);
            Offset = new Vector2(oX, oY);
        }

        public UDim2(Vector2 scale, Vector2 offset) {
            Scale = scale;
            Offset = offset;
        }
    }
    abstract class UIElement {

        public static SpriteFont[] Fonts;

        public string Name;
        public UDim2 Position;
        public float Scale;

        public UIElement(string name, UDim2 position, float scale) {
            Name = name;
            Position = position;
            Scale = scale;
        }

        public abstract void Update(GameTime time, Point ms, Rectangle screen, bool click);
        public abstract void Draw(SpriteBatch batch, Rectangle screen);
    }
}

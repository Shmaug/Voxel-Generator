using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelGenerator.UI
{
    class ImageLabel : UIElement {
        public Texture2D Image;
        public Vector2 Size;
        public Color Color;
        public byte Font;

        public ImageLabel(string name, UDim2 position, float scale, Texture2D image, Vector2 size, byte font, Color color) : base(name, position, scale) {
            Image = image;
            Size = size;
            Font = font;
            Color = color;
        }

        public override void Update(GameTime time, Point ms, Rectangle screen, bool click) {

        }

        public override void Draw(SpriteBatch batch, Rectangle screen) {
            int px = (int)(screen.Width * Position.Scale.X + Position.Offset.X);
            int py = (int)(screen.Height * Position.Scale.Y + Position.Offset.Y);
            batch.Draw(Image, new Rectangle(px, py, (int)(Size.X * Scale), (int)(Size.Y * Scale)), null, Color, 0f, Vector2.Zero, SpriteEffects.None, 0f);
        }
    }
}

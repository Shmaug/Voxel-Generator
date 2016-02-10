using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelGenerator.UI
{
    class Label : UIElement {
        public string Text;
        public Color Color;
        public byte Font;

        public Label(string name, UDim2 position, float scale, string text, byte font, Color color) : base(name, position, scale) {
            Text = text;
            Font = font;
            Color = color;
        }

        public override void Update(GameTime time, Point ms, Rectangle screen, bool click) {

        }

        public override void Draw(SpriteBatch batch, Rectangle screen) {
            batch.DrawString(Fonts[Font], Text, new Vector2(screen.Width, screen.Height) * Position.Scale + Position.Offset, Color, 0f, Fonts[Font].MeasureString(Text) / 2f, Scale, SpriteEffects.None, 0f);
        }
    }
}

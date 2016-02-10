using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelGenerator.UI
{
    class Button : UIElement
    {
        public string text;
        public byte Font;
        public Color Color1;
        public Color Color2;
        Action action;
        float hoverTime;
        Rectangle bounds;

        public Button(string name, UDim2 position, float scale, string text, byte font, Color color1, Color color2, Action action) : base(name, position, scale) {
            this.text = text;
            this.action = action;
            Font = font;
            Color1 = color1;
            Color2 = color2;
        }

        public override void Update(GameTime time, Point ms, Rectangle screen, bool click) {
            bounds = new Rectangle(0, 0, (int)Fonts[Font].MeasureString(this.text).X, (int)Fonts[Font].MeasureString(text).Y);
            bounds.X = (int)(screen.Width * Position.Scale.X + Position.Offset.X) - bounds.Width / 2;
            bounds.Y = (int)(screen.Height * Position.Scale.Y + Position.Offset.Y) - bounds.Height / 2;
            if (bounds.Contains(ms))
                hoverTime += (float)time.ElapsedGameTime.TotalSeconds;
            else
                hoverTime = 0f;
            if (hoverTime > 0 && click)
                action();
        }

        public override void Draw(SpriteBatch batch, Rectangle screen) {
            float sc = Scale * MathHelper.SmoothStep(1f, .95f, MathHelper.Clamp(hoverTime * 15f, 0, 1));
            Color col = Color.Lerp(Color1, Color2, MathHelper.Clamp(hoverTime * 15f, 0, 1));
            batch.DrawString(Fonts[Font], text, new Vector2(screen.Width, screen.Height) * Position.Scale + Position.Offset, col, 0f, new Vector2(bounds.Width, bounds.Height) / 2, sc, SpriteEffects.None, 0f);
        }
    }
}

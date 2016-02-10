using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelGenerator.Dynamic {
    class Billboard {
        public static Effect billboardEffect;

        VertexPositionColorTexture[] VertexBuffer;
        int[] IndexBuffer;

        public Vector3 Position;
        public float Scale;
        public Texture2D Texture;
        public Color Color;

        public Billboard(Vector3 pos, float scale, Texture2D tex, Color col) {
            Position = pos;
            Scale = scale;
            Texture = tex;
            Color = col;

            VertexBuffer = new VertexPositionColorTexture[4] {
                new VertexPositionColorTexture(new Vector3(-.5f,  .5f, 0), Color.White, Vector2.Zero),
                new VertexPositionColorTexture(new Vector3( .5f,  .5f, 0), Color.White, Vector2.UnitX),
                new VertexPositionColorTexture(new Vector3(-.5f, -.5f, 0), Color.White, Vector2.UnitY),
                new VertexPositionColorTexture(new Vector3( .5f, -.5f, 0), Color.White, Vector2.One)
            };
            IndexBuffer = new int[6] {
                0, 1, 2,
                1, 3, 2
            };
        }

        public void Draw(GraphicsDevice device, Camera camera) {
            BlendState beforeBlend = device.BlendState;
            RasterizerState beforeRasterizer = device.RasterizerState;
            device.RasterizerState = new RasterizerState() { FillMode = beforeRasterizer.FillMode, CullMode = CullMode.None };
            device.BlendState = BlendState.Additive;
            billboardEffect.Parameters["W"].SetValue(Matrix.CreateScale(Scale) * Matrix.CreateWorld(Position, Position - camera.Position, Vector3.Up));
            billboardEffect.Parameters["VP"].SetValue(camera.View * camera.Projection);
            billboardEffect.Parameters["tex"].SetValue(Texture);
            billboardEffect.Parameters["col"].SetValue(Color.ToVector4());

            foreach (EffectPass p in billboardEffect.CurrentTechnique.Passes) {
                p.Apply();
                device.DrawUserIndexedPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, VertexBuffer, 0, 4, IndexBuffer, 0, 2);
            }
            device.RasterizerState = beforeRasterizer;
            device.BlendState = beforeBlend;
        }
    }
}

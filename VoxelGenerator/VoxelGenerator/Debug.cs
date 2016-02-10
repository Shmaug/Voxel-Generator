using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelGenerator {
    class Debug {
        static List<string> logs = new List<string>();
        static Dictionary<int, string> labels = new Dictionary<int, string>();

        static Dictionary<int, Tuple<BoundingBox, Color, float>> boxes = new Dictionary<int, Tuple<BoundingBox, Color, float>>();

        public static void Log(object l) {
            logs.Add(l.ToString());
            if (logs.Count > 10)
                logs.Remove(logs[0]);
        }

        public static void Track(object l, int slot) {
            labels[slot] = l.ToString();
        }

        public static void TrackBox(BoundingBox b, Color c, float scale, int slot) {
            boxes[slot] = new Tuple<BoundingBox, Color, float>(b, c, scale);
        }

        public static void DrawBoxes(GraphicsDevice device, Effect effect) {
            BlendState beforeBlend = device.BlendState;
            device.BlendState = BlendState.AlphaBlend;
            effect.CurrentTechnique = effect.Techniques["DebugBox"];
            foreach (KeyValuePair<int, Tuple<BoundingBox, Color, float>> k in boxes) {
                BoundingBox box = k.Value.Item1;

                effect.Parameters["W"].SetValue(Matrix.CreateScale(box.Max - box.Min) * Matrix.CreateScale(k.Value.Item3) * Matrix.CreateTranslation((box.Max + box.Min) / 2f));
                effect.Parameters["debugColor"].SetValue(boxes[k.Key].Item2.ToVector4());

                foreach (EffectPass p in effect.CurrentTechnique.Passes) {
                    p.Apply();
                    device.DrawUserIndexedPrimitives<VertexPositionNormalColor>(PrimitiveType.TriangleList, Util.CubeVerticies, 0, Util.CubeVerticies.Length, Util.CubeIndicies, 0, Util.CubeIndicies.Length / 3, new VertexPositionNormalColor().VertexDeclaration);
                }
            }
            boxes = new Dictionary<int, Tuple<BoundingBox, Color, float>>();
            device.BlendState = beforeBlend != null ? beforeBlend : BlendState.Opaque;
        }

        public static void DrawText(SpriteBatch batch, SpriteFont font) {
            string str = "";
            foreach (string l in logs)
                str += l + "\n";
            batch.DrawString(font, str, Vector2.One * 10, Color.White);

            string str2 = "";
            foreach (KeyValuePair<int, string> l in labels)
                str2 += l.Value + "\n";
            batch.DrawString(font, str2, Vector2.One * 10 + Vector2.UnitX * 500, Color.White);

            labels = new Dictionary<int, string>();
        }
    }
}

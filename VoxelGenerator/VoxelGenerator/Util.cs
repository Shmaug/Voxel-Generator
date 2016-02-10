using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using VoxelGenerator.Environment;

namespace VoxelGenerator {
    public struct GrassVertex : IVertexType {
        public Vector3 Position;
        public Vector2 TexCoord;
        public Vector3 Normal;
        public Vector4 Animate;

        public GrassVertex(Vector3 position, Vector3 normal, Vector2 texcoord, Vector4 animate) {
            Position = position;
            TexCoord = texcoord;
            Normal = normal;
            Animate = animate;
        }

        public VertexDeclaration VertexDeclaration {
            get {
                return new VertexDeclaration
                (
                    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                    new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
                    new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
                    new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1)
                );
            }
        }
    }
    public struct VertexPositionNormalColor : IVertexType {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;

        public VertexPositionNormalColor(Vector3 position, Color color, Vector3 normal) {
            Position = position;
            Color = color;
            Normal = normal;
        }

        public VertexDeclaration VertexDeclaration {
            get {
                return new VertexDeclaration
                (
                    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                    new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                    new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
                );
            }
        }
    }
    class Util {
        #region dbcube
        public static readonly VertexPositionNormalColor[] CubeVerticies = new VertexPositionNormalColor[] {
            // top
            new VertexPositionNormalColor(new Vector3(-.5f, .5f, -.5f), Color.White, Vector3.Up),
            new VertexPositionNormalColor(new Vector3( .5f, .5f, -.5f), Color.White, Vector3.Up),
            new VertexPositionNormalColor(new Vector3(-.5f, .5f,  .5f), Color.White, Vector3.Up),
            new VertexPositionNormalColor(new Vector3( .5f, .5f,  .5f), Color.White, Vector3.Up),

            // down
            new VertexPositionNormalColor(new Vector3(-.5f, -.5f, -.5f), Color.White, Vector3.Down),
            new VertexPositionNormalColor(new Vector3( .5f, -.5f, -.5f), Color.White, Vector3.Down),
            new VertexPositionNormalColor(new Vector3(-.5f, -.5f,  .5f), Color.White, Vector3.Down),
            new VertexPositionNormalColor(new Vector3( .5f, -.5f,  .5f), Color.White, Vector3.Down),

            // back
            new VertexPositionNormalColor(new Vector3(-.5f,  .5f, .5f), Color.White, Vector3.Backward),
            new VertexPositionNormalColor(new Vector3( .5f,  .5f, .5f), Color.White, Vector3.Backward),
            new VertexPositionNormalColor(new Vector3(-.5f, -.5f, .5f), Color.White, Vector3.Backward),
            new VertexPositionNormalColor(new Vector3( .5f, -.5f, .5f), Color.White, Vector3.Backward),

            // front
            new VertexPositionNormalColor(new Vector3(-.5f,  .5f, -.5f), Color.White, Vector3.Forward),
            new VertexPositionNormalColor(new Vector3( .5f,  .5f, -.5f), Color.White, Vector3.Forward),
            new VertexPositionNormalColor(new Vector3(-.5f, -.5f, -.5f), Color.White, Vector3.Forward),
            new VertexPositionNormalColor(new Vector3( .5f, -.5f, -.5f), Color.White, Vector3.Forward),

            // right
            new VertexPositionNormalColor(new Vector3(.5f,  .5f,  .5f), Color.White, Vector3.Right),
            new VertexPositionNormalColor(new Vector3(.5f,  .5f, -.5f), Color.White, Vector3.Right),
            new VertexPositionNormalColor(new Vector3(.5f, -.5f,  .5f), Color.White, Vector3.Right),
            new VertexPositionNormalColor(new Vector3(.5f, -.5f, -.5f), Color.White, Vector3.Right),

            // left
            new VertexPositionNormalColor(new Vector3(-.5f,  .5f,  .5f), Color.White, Vector3.Right),
            new VertexPositionNormalColor(new Vector3(-.5f,  .5f, -.5f), Color.White, Vector3.Right),
            new VertexPositionNormalColor(new Vector3(-.5f, -.5f,  .5f), Color.White, Vector3.Right),
            new VertexPositionNormalColor(new Vector3(-.5f, -.5f, -.5f), Color.White, Vector3.Right),
        };
        public static readonly int[] CubeIndicies = new int[] {
            // top
            0, 1, 2,
            1, 3, 2,

            // bottom
            4, 6, 5,
            5, 6, 7,

            // back
            8, 9, 10,
            9, 11, 10,

            // front
            12, 14, 13,
            13, 14, 15,

            // right
            16, 17, 18,
            17, 19, 18,

            // left
            20, 22, 21,
            21, 22, 23,
        };
        #endregion
        
        public static int getIndex(int x, int y, int z, int w, int h) {
            return x + w * (y + h * z);
        }

        public static float getXOffset(BoundingBox bbox1, BoundingBox bbox2, float delta) {
            if (bbox1.Min.Y.CompareTo(bbox2.Max.Y) >= 0 || bbox1.Max.Y.CompareTo(bbox2.Min.Y) <= 0 ||
                bbox1.Min.Z.CompareTo(bbox2.Max.Z) >= 0 || bbox1.Max.Z.CompareTo(bbox2.Min.Z) <= 0)
                return delta;
            
            if (delta > 0)
                if (bbox1.Min.X.CompareTo(bbox2.Max.X) > 0)
                    return bbox1.Min.X - bbox2.Max.X;
                else if (bbox1.Min.X.CompareTo(bbox2.Max.X) == 0)
                    return 0;

            if (delta < 0)
                if (bbox2.Min.X.CompareTo(bbox1.Max.X) > 0)
                    return bbox1.Max.X - bbox2.Min.X;
                else if (bbox2.Min.X.CompareTo(bbox1.Max.X) == 0)
                    return 0;

            return delta;
        }

        public static float getYOffset(BoundingBox bbox1, BoundingBox bbox2, float delta) {
            if (bbox1.Min.X.CompareTo(bbox2.Max.X) >= 0 || bbox1.Max.X.CompareTo(bbox2.Min.X) <= 0 ||
                bbox1.Min.Z.CompareTo(bbox2.Max.Z) >= 0 || bbox1.Max.Z.CompareTo(bbox2.Min.Z) <= 0)
                return delta;
            
            if (delta > 0)
                if (bbox1.Min.Y.CompareTo(bbox2.Max.Y) > 0)
                    return bbox1.Min.Y - bbox2.Max.Y;
                else if (bbox1.Min.Y.CompareTo(bbox2.Max.Y) == 0)
                    return 0;

            if (delta < 0)
                if (bbox2.Min.Y.CompareTo(bbox1.Max.Y) > 0)
                    return bbox1.Max.Y - bbox2.Min.Y;
                else if (bbox2.Min.Y.CompareTo(bbox1.Max.Y) == 0)
                    return 0;

            return delta;
        }

        public static float getZOffset(BoundingBox bbox1, BoundingBox bbox2, float delta) {
            if (bbox1.Min.X.CompareTo(bbox2.Max.X) >= 0 || bbox1.Max.X.CompareTo(bbox2.Min.X) <= 0 ||
                bbox1.Min.Y.CompareTo(bbox2.Max.Y) >= 0 || bbox1.Max.Y.CompareTo(bbox2.Min.Y) <= 0)
                return delta;

            if (delta > 0)
                if (bbox1.Min.Z.CompareTo(bbox2.Max.Z) > 0)
                    return bbox1.Min.Z - bbox2.Max.Z;
                else if (bbox1.Min.Z.CompareTo(bbox2.Max.Z) == 0)
                    return 0;

            if (delta < 0)
                if (bbox2.Min.Z.CompareTo(bbox1.Max.Z) > 0)
                    return bbox1.Max.Z - bbox2.Min.Z;
                else if (bbox2.Min.Z.CompareTo(bbox1.Max.Z) == 0)
                    return 0;

            return delta;
        }

        public static BoundingBox Intersect(BoundingBox b1, BoundingBox b2) {
            if (b1.Contains(b2) == ContainmentType.Contains)
                return b2;
            if (b2.Contains(b1) == ContainmentType.Contains)
                return b1;

            return new BoundingBox(
                new Vector3(
                    Math.Max(b1.Min.X, b2.Min.X),
                    Math.Max(b1.Min.Y, b2.Min.Y),
                    Math.Max(b1.Min.Z, b2.Min.Z)
                    ),
                new Vector3(
                    Math.Min(b1.Max.X, b2.Max.X),
                    Math.Min(b1.Max.Y, b2.Max.Y),
                    Math.Min(b1.Max.Z, b2.Max.Z)
                    ));
        }

        public static Vector3 NormalFromFace(BlockFace face) {
            switch (face) {
                case BlockFace.Back:
                    return Vector3.Backward;
                case BlockFace.Front:
                    return Vector3.Forward;
                case BlockFace.Left:
                    return Vector3.Left;
                case BlockFace.Right:
                    return Vector3.Right;
                case BlockFace.Top:
                    return Vector3.Up;
                case BlockFace.Bottom:
                    return Vector3.Down;
            }
            return Vector3.Zero;
        }


        public static void ComputeKernel(int blurRadius, float blurAmount, out float[] kernel) {
            float amount = blurAmount;

            kernel = new float[blurRadius * 2 + 1];
            float sigma = blurRadius / amount;

            float twoSigmaSquare = 2.0f * sigma * sigma;
            float sigmaRoot = (float)Math.Sqrt(twoSigmaSquare * Math.PI);
            float total = 0.0f;
            float distance = 0.0f;
            int index = 0;

            for (int i = -blurRadius; i <= blurRadius; ++i) {
                distance = i * i;
                index = i + blurRadius;
                kernel[index] = (float)Math.Exp(-distance / twoSigmaSquare) / sigmaRoot;
                total += kernel[index];
            }

            for (int i = 0; i < kernel.Length; ++i)
                kernel[i] /= total;
        }

        public static void ComputeOffsets2D(int blurRadius, float textureWidth, float textureHeight, out Vector2[] horiz, out Vector2[] vert) {
            horiz = new Vector2[blurRadius * 2 + 1];
            vert = new Vector2[blurRadius * 2 + 1];

            int index = 0;
            float xOffset = 1f / textureWidth;
            float yOffset = 1f / textureHeight;

            for (int i = -blurRadius; i <= blurRadius; ++i) {
                index = i + blurRadius;
                horiz[index] = new Vector2(i * xOffset, 0f);
                vert[index] = new Vector2(0f, i * yOffset);
            }
        }

        public static void ComputeOffsets3D(int blurRadius, float textureWidth, float textureHeight, float textureDepth, out Vector3[] x, out Vector3[] y, out Vector3[] z) {
            x = new Vector3[blurRadius * 2 + 1];
            y = new Vector3[blurRadius * 2 + 1];
            z = new Vector3[blurRadius * 2 + 1];

            int index = 0;
            float xOffset = 1f / textureWidth;
            float yOffset = 1f / textureHeight;
            float zOffset = 1f / textureDepth;

            for (int i = -blurRadius; i <= blurRadius; ++i) {
                index = i + blurRadius;
                x[index] = new Vector3(i * xOffset, 0, 0);
                y[index] = new Vector3(0, i * yOffset, 0);
                z[index] = new Vector3(0, 0, i * zOffset);
            }
        }
    }
}

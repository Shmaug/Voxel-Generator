using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelGenerator.Environment {
    enum BlockType {
        None,
        Grass,
        Dirt,
        Stone,
        TallGrass,
        Log,
        Leaves,
        Glowy,
        Water
    }
    enum BlockFace {
        None,
        Top,
        Bottom,
        Left,
        Right,
        Front,
        Back
    }
    enum GeometryType {
        Block,
        HalfBlock,
        Cross
    }
    class Block {
        public static Texture2D BlockTexture;
        public static readonly int BlockTextureSize = 64;

        public BlockType Type { get; private set; }
        public Chunk Chunk;
        public bool Collidable;
        public bool Opaque { get; private set; }
        public float LightAttenuation { get; private set; }
        public float LightEmission { get; private set; }
        public GeometryType GeometryType { get; private set; }
        public int TextureId;
        public object Data;

        public Vector3 Position;
        public BoundingBox hitbox {
            get;
            private set;
        }

        public Block(BlockType type) {
            Type = type;

            setDefaultProperties();
        }

        public Block(BlockType type, Vector3 pos, Chunk chunk) {
            Type = type;
            Chunk = chunk;
            Position = pos;
            makeBBox();

            setDefaultProperties();
        }

        void setDefaultProperties() {
            switch (Type) {
                case BlockType.TallGrass:
                    Collidable = false;
                    Opaque = false;
                    GeometryType = GeometryType.Cross;
                    LightAttenuation = 1f;
                    LightEmission = 0f;
                    break;
                case BlockType.Leaves:
                    Opaque = false;
                    Collidable = true;
                    GeometryType = GeometryType.Block;
                    LightAttenuation = .75f;
                    LightEmission = 0f;
                    break;
                case BlockType.Glowy:
                    Opaque = false;
                    Collidable = true;
                    GeometryType = GeometryType.Block;
                    LightAttenuation = 1f;
                    LightEmission = 5f;
                    break;
                case BlockType.Water:
                    Opaque = false;
                    Collidable = false;
                    GeometryType = GeometryType.Block;
                    LightAttenuation = .5f;
                    LightEmission = 0f;
                    break;
                default:
                    LightEmission = 0f;
                    LightAttenuation = 0f;
                    Collidable = true;
                    Opaque = true;
                    GeometryType = GeometryType.Block;
                    break;
            }
        }

        public void makeBBox() {
            hitbox = new BoundingBox(Position, Position + Vector3.One);
        }

        public static Color GetColor(BlockType type) {
            switch (type) {
                case BlockType.None:
                    return Color.Transparent;
                case BlockType.Grass:
                    return Color.DarkGreen;
                case BlockType.Dirt:
                    return Color.SaddleBrown;
                case BlockType.Stone:
                    return Color.LightGray;
                case BlockType.TallGrass:
                    return Color.Green;
                case BlockType.Log:
                    return Color.RosyBrown;
                case BlockType.Leaves:
                    return Color.LightGreen;
                default:
                    return Color.Transparent;

            }
        }
    }
}

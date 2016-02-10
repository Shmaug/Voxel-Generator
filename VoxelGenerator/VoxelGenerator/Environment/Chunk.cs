using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;

namespace VoxelGenerator.Environment {
    enum LightType {
        Sky, Block, Both
    }
    class Chunk {
        public static readonly int CHUNK_SIZE = 16;
        public static readonly int CHUNK_HEIGHT = 128;

        public int X, Z;
        public Vector3 Position {
            get {
                return new Vector3(X * CHUNK_SIZE, 0, Z * CHUNK_SIZE);
            }
        }
        public bool built;
        public BoundingBox bbox;
        public World world;

        double genTime, buildTime, lightTime;
        
        VertexBuffer VertexBuffer;
        IndexBuffer IndexBuffer;
        VertexBuffer GrassVertexBuffer;
        IndexBuffer GrassIndexBuffer;
        bool building;
        
        /// <summary>
        /// Chunk at x + 1
        /// </summary>
        public Chunk chunkXpos;
        /// <summary>
        /// Chunk at x - 1
        /// </summary>
        public Chunk chunkXneg;
        /// <summary>
        /// Chunk at z + 1
        /// </summary>
        public Chunk chunkZpos;
        /// <summary>
        /// Chunk at z - 1
        /// </summary>
        public Chunk chunkZneg;

        public Block[,,] blocks;
        public float[] blockLight;
        public float[] skyLight;
        public Texture3D lightTexture;

        public float percentDone { get; private set; }

        bool geometryDirty = false;
        bool lightDirty = false;

        bool drawing = false;
        bool settingVertexBuffers = false;
        bool settingLightTexture = false;

        public Chunk(int x, int z, World wld) {
            X = x;
            Z = z;
            world = wld;
            blocks = new Block[CHUNK_SIZE, CHUNK_HEIGHT, CHUNK_SIZE];

            blockLight = new float[CHUNK_SIZE * CHUNK_HEIGHT * CHUNK_SIZE];
            skyLight = new float[CHUNK_SIZE * CHUNK_HEIGHT * CHUNK_SIZE];
        }

        /// <summary>
        /// Gets the block at WORLD COORDINATES (x,y,z)
        /// </summary>
        public Block BlockAt(int x, int y, int z) {
            int x2 = x - X * CHUNK_SIZE;
            int z2 = z - Z * CHUNK_SIZE;
            if (x2 < 0)
                if (chunkXneg != null)
                    return chunkXneg.BlockAt(x, y, z);
                else
                    return null;
            if (x2 >= CHUNK_SIZE)
                if (chunkXpos != null)
                    return chunkXpos.BlockAt(x, y, z);
                else
                    return null;
            if (z2 < 0)
                if (chunkZneg != null)
                    return chunkZneg.BlockAt(x, y, z);
                else
                    return null;
            if (z2 >= CHUNK_SIZE)
                if (chunkZpos != null)
                    return chunkZpos.BlockAt(x, y, z);
                else
                    return null;
            if (y < 0 || y >= CHUNK_HEIGHT)
                return null;
            return blocks[x2, y, z2];
        }

        /// <summary>
        /// Sets the block at WORLD COORDINATES (x,y,z)
        /// </summary>
        public bool SetBlock(int x, int y, int z, Block block) {
            int x2 = x - X * CHUNK_SIZE;
            int z2 = z - Z * CHUNK_SIZE;
            if (x2 < 0)
                if (chunkXneg != null)
                    return chunkXneg.SetBlock(x, y, z, block);
                else
                    return false;
            if (x2 >= CHUNK_SIZE)
                if (chunkXpos != null)
                    return chunkXpos.SetBlock(x, y, z, block);
                else
                    return false;
            if (z2 < 0)
                if (chunkZneg != null)
                    return chunkZneg.SetBlock(x, y, z, block);
                else
                    return false;
            if (z2 >= CHUNK_SIZE)
                if (chunkZpos != null)
                    return chunkZpos.SetBlock(x, y, z, block);
                else
                    return false;
            if (y < 0 || y >= CHUNK_HEIGHT)
                return false;
            blocks[x2, y, z2] = block;
            if (block != null) {
                block.Position = new Vector3(x, y, z);
                block.Chunk = this;
                block.makeBBox();
            } else {
                if (y < CHUNK_HEIGHT-2 && blocks[x2, y + 1, z2] != null && blocks[x2, y + 1, z2].Type == BlockType.TallGrass)
                    blocks[x2, y + 1, z2] = null;
            }
            geometryDirty = true;
            generateLight();
            
            if (x2 == 0 && chunkXneg != null)
                chunkXneg.setDirty();
            if (x2 == CHUNK_SIZE-1 && chunkXpos != null)
                chunkXpos.setDirty();

            if (z2 == 0 && chunkZneg != null)
                chunkZneg.setDirty();
            if (z2 == CHUNK_SIZE - 1 && chunkZpos != null)
                chunkZpos.setDirty();

            return true;
        }

        /// <summary>
        /// Mark for rebuilding of verticies
        /// </summary>
        public void setDirty() {
            geometryDirty = true;
        }
        public void setLightDirty() {
            lightDirty = true;
        }

        void buildFace(Block block, BlockFace face, ref List<VertexPositionNormalTexture> verts, ref List<int> inds, Vector3 scale, Vector3 offset) {
            Vector2 sizeUV = new Vector2((float)Block.BlockTextureSize / (float)Block.BlockTexture.Width, (float)Block.BlockTextureSize / (float)Block.BlockTexture.Height);
            
            Vector2 coord;
            int i = verts.Count;
            switch (face) {
                case BlockFace.Top:
                    coord = new Vector2(((int)block.Type - 1) * sizeUV.X, 0);

                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(0, 1, 0) * scale, Vector3.Up, coord));
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(1, 1, 0) * scale, Vector3.Up, coord + Vector2.UnitX * sizeUV));
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(0, 1, 1) * scale, Vector3.Up, coord + Vector2.UnitY * sizeUV));
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(1, 1, 1) * scale, Vector3.Up, coord + Vector2.One * sizeUV));
                    inds.Add(i);
                    inds.Add(i + 1);
                    inds.Add(i + 2);
                    inds.Add(i + 1);
                    inds.Add(i + 3);
                    inds.Add(i + 2);
                    break;
                case BlockFace.Bottom:
                    coord = new Vector2(((int)block.Type - 1) * sizeUV.X, sizeUV.Y);

                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(0, 0, 0) * scale, Vector3.Down, coord));
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(1, 0, 0) * scale, Vector3.Down, coord + Vector2.UnitX * sizeUV));
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(0, 0, 1) * scale, Vector3.Down, coord + Vector2.UnitY * sizeUV));
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(1, 0, 1) * scale, Vector3.Down, coord + Vector2.One * sizeUV));
                    inds.Add(i);
                    inds.Add(i + 2);
                    inds.Add(i + 1);
                    inds.Add(i + 1);
                    inds.Add(i + 2);
                    inds.Add(i + 3);
                    break;
                case BlockFace.Back:
                    coord = new Vector2(((int)block.Type - 1) * sizeUV.X, sizeUV.Y * 2);
                    
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(0, 1, 1) * scale, Vector3.Backward, coord));
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(1, 1, 1) * scale, Vector3.Backward, coord + Vector2.UnitX * sizeUV));
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(0, 0, 1) * scale, Vector3.Backward, coord + Vector2.UnitY * sizeUV));
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(1, 0, 1) * scale, Vector3.Backward, coord + Vector2.One * sizeUV));
                    inds.Add(i);
                    inds.Add(i + 1);
                    inds.Add(i + 2);
                    inds.Add(i + 1);
                    inds.Add(i + 3);
                    inds.Add(i + 2);
                    break;
                case BlockFace.Front:
                    coord = new Vector2(((int)block.Type - 1) * sizeUV.X, sizeUV.Y * 3);
                    
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(0, 1, 0) * scale, Vector3.Forward, coord));
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(1, 1, 0) * scale, Vector3.Forward, coord + Vector2.UnitX * sizeUV));
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(0, 0, 0) * scale, Vector3.Forward, coord + Vector2.UnitY * sizeUV));
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(1, 0, 0) * scale, Vector3.Forward, coord + Vector2.One * sizeUV));
                    inds.Add(i);
                    inds.Add(i + 2);
                    inds.Add(i + 1);
                    inds.Add(i + 1);
                    inds.Add(i + 2);
                    inds.Add(i + 3);
                    break;
                case BlockFace.Right:
                    coord = new Vector2(((int)block.Type - 1) * sizeUV.X, sizeUV.Y * 4);
                    
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(1, 1, 1) * scale, Vector3.Right, coord));
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(1, 1, 0) * scale, Vector3.Right, coord + Vector2.UnitX * sizeUV));
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(1, 0, 1) * scale, Vector3.Right, coord + Vector2.UnitY * sizeUV));
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(1, 0, 0) * scale, Vector3.Right, coord + Vector2.One * sizeUV));
                    inds.Add(i);
                    inds.Add(i + 1);
                    inds.Add(i + 2);
                    inds.Add(i + 1);
                    inds.Add(i + 3);
                    inds.Add(i + 2);
                    break;
                case BlockFace.Left:
                    coord = new Vector2(((int)block.Type - 1) * sizeUV.X, sizeUV.Y * 5);
                    
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(0, 1, 1) * scale, Vector3.Left, coord));
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(0, 1, 0) * scale, Vector3.Left, coord + Vector2.UnitX * sizeUV));
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(0, 0, 1) * scale, Vector3.Left, coord + Vector2.UnitY * sizeUV));
                    verts.Add(new VertexPositionNormalTexture(block.Position + offset + new Vector3(0, 0, 0) * scale, Vector3.Left, coord + Vector2.One * sizeUV));
                    inds.Add(i);
                    inds.Add(i + 2);
                    inds.Add(i + 1);
                    inds.Add(i + 1);
                    inds.Add(i + 2);
                    inds.Add(i + 3);
                    break;
            }
        }

        void buildVoxel(int x, int y, int z, ref List<VertexPositionNormalTexture> verts, ref List<int> inds, ref List<GrassVertex> gverts, ref List<int> ginds) {
            Block block = BlockAt(x, y, z);
            if (block == null)
                return;

            Block blockUp = BlockAt(x, y + 1, z);
            Block blockDown = BlockAt(x, y - 1, z);
            Block blockLeft = BlockAt(x - 1, y, z);
            Block blockRight = BlockAt(x + 1, y, z);
            Block blockForward = BlockAt(x, y, z - 1);
            Block blockBackward = BlockAt(x, y, z + 1);

            switch (block.GeometryType) {
                case GeometryType.Block:
                    if (block.Opaque) {
                        if (blockUp == null || !blockUp.Opaque)
                            buildFace(block, BlockFace.Top, ref verts, ref inds, Vector3.One, Vector3.Zero);

                        if (blockDown == null || !blockDown.Opaque) {
                            buildFace(block, BlockFace.Bottom, ref verts, ref inds, Vector3.One, Vector3.Zero);
                        }

                        if (blockBackward == null || !blockBackward.Opaque) {
                            buildFace(block, BlockFace.Back, ref verts, ref inds, Vector3.One, Vector3.Zero);
                        }
                        if (blockForward == null || !blockForward.Opaque) {
                            buildFace(block, BlockFace.Front, ref verts, ref inds, Vector3.One, Vector3.Zero);
                        }

                        if (blockRight == null || !blockRight.Opaque) {
                            buildFace(block, BlockFace.Right, ref verts, ref inds, Vector3.One, Vector3.Zero);
                        }
                        if (blockLeft == null || !blockLeft.Opaque) {
                            buildFace(block, BlockFace.Left, ref verts, ref inds, Vector3.One, Vector3.Zero);
                        }
                    } else {
                        if (block.Type == BlockType.Water) {
                            if (blockUp == null || blockUp.Type != BlockType.Water)
                                buildFace(block, BlockFace.Top, ref verts, ref inds, Vector3.One, Vector3.Zero);
                            if (blockDown == null || (!blockDown.Opaque && blockDown.Type != BlockType.Water))
                                buildFace(block, BlockFace.Bottom, ref verts, ref inds, Vector3.One, Vector3.Zero);

                            if (blockLeft == null || blockLeft.Type != BlockType.Water)
                                buildFace(block, BlockFace.Left, ref verts, ref inds, Vector3.One, Vector3.Zero);
                            if (blockRight == null || blockRight.Type != BlockType.Water)
                                buildFace(block, BlockFace.Right, ref verts, ref inds, Vector3.One, Vector3.Zero);

                            if (blockForward == null || blockForward.Type != BlockType.Water)
                                buildFace(block, BlockFace.Front, ref verts, ref inds, Vector3.One, Vector3.Zero);
                            if (blockBackward == null || blockBackward.Type != BlockType.Water)
                                buildFace(block, BlockFace.Back, ref verts, ref inds, Vector3.One, Vector3.Zero);

                        } else {
                            buildFace(block, BlockFace.Top, ref verts, ref inds, Vector3.One, Vector3.Zero);
                            buildFace(block, BlockFace.Bottom, ref verts, ref inds, Vector3.One, Vector3.Zero);
                            buildFace(block, BlockFace.Left, ref verts, ref inds, Vector3.One, Vector3.Zero);
                            buildFace(block, BlockFace.Right, ref verts, ref inds, Vector3.One, Vector3.Zero);
                            buildFace(block, BlockFace.Front, ref verts, ref inds, Vector3.One, Vector3.Zero);
                            buildFace(block, BlockFace.Back, ref verts, ref inds, Vector3.One, Vector3.Zero);
                        }
                    }
                    break;
                case GeometryType.HalfBlock:
                    if (block.Opaque) {
                        if (blockUp == null || !blockUp.Opaque)
                            buildFace(block, BlockFace.Top, ref verts, ref inds, new Vector3(1f, .5f, 1f), new Vector3(0, -.5f, 0));
                        if (blockDown == null || !blockDown.Opaque)
                            buildFace(block, BlockFace.Bottom, ref verts, ref inds, new Vector3(1f, .5f, 1f), new Vector3(0, -.5f, 0));

                        if (blockBackward == null || !blockBackward.Opaque)
                            buildFace(block, BlockFace.Back, ref verts, ref inds, new Vector3(1f, .5f, 1f), new Vector3(0, -.5f, 0));
                        if (blockForward == null || !blockForward.Opaque)
                            buildFace(block, BlockFace.Front, ref verts, ref inds, new Vector3(1f, .5f, 1f), new Vector3(0, -.5f, 0));

                        if (blockRight == null || !blockRight.Opaque)
                            buildFace(block, BlockFace.Right, ref verts, ref inds, new Vector3(1f, .5f, 1f), new Vector3(0, -.5f, 0));
                        if (blockLeft == null || !blockLeft.Opaque)
                            buildFace(block, BlockFace.Left, ref verts, ref inds, new Vector3(1f, .5f, 1f), new Vector3(0, -.5f, 0));
                    } else {
                        buildFace(block, BlockFace.Top, ref verts, ref inds, new Vector3(1f, .5f, 1f), new Vector3(0, -.5f, 0));
                        buildFace(block, BlockFace.Bottom, ref verts, ref inds, new Vector3(1f, .5f, 1f), new Vector3(0, -.5f, 0));
                        buildFace(block, BlockFace.Left, ref verts, ref inds, new Vector3(1f, .5f, 1f), new Vector3(0, -.5f, 0));
                        buildFace(block, BlockFace.Right, ref verts, ref inds, new Vector3(1f, .5f, 1f), new Vector3(0, -.5f, 0));
                        buildFace(block, BlockFace.Front, ref verts, ref inds, new Vector3(1f, .5f, 1f), new Vector3(0, -.5f, 0));
                        buildFace(block, BlockFace.Back, ref verts, ref inds, new Vector3(1f, .5f, 1f), new Vector3(0, -.5f, 0));
                    }
                    break;
                case GeometryType.Cross:
                    #region build cross geometry
                    Vector2 sizeUV = new Vector2((float)Block.BlockTextureSize / (float)Block.BlockTexture.Width, (float)Block.BlockTextureSize / (float)Block.BlockTexture.Height);
                    Vector2 coord = new Vector2(((int)block.Type - 1), block.TextureId) * sizeUV;
                    Vector3 offset = new Vector3(x, y, z);
                    float s = 1f;
                    
                    if (block.Type == BlockType.TallGrass) {
                        Vector4 data = (Vector4)block.Data;

                        offset = new Vector3(x, y, z) + new Vector3(1, 0, 1) * (.146446609406f * ((data.X * 4f) - 2f));
                        s = (data.Y + 1f) / 2f;
                    
                        float t = data.Z * 2f; // animate offset
                        float h = data.W + .5f; // height
                    
                        // Grass normals overwritten in world.fx
                        int i = gverts.Count;
                        gverts.Add(new GrassVertex(offset,                        new Vector3(.5f, 0, -.5f), coord + Vector2.UnitY * sizeUV, new Vector4()));
                        gverts.Add(new GrassVertex(offset + new Vector3(s, 0, s), new Vector3(.5f, 0, -.5f), coord + Vector2.One * sizeUV, new Vector4()));
                        gverts.Add(new GrassVertex(offset + new Vector3(0, h, 0), new Vector3(.5f, 0, -.5f), coord, new Vector4(t, 0, 0, 1)));
                        gverts.Add(new GrassVertex(offset + new Vector3(s, h, s), new Vector3(.5f, 0, -.5f), coord + Vector2.UnitX * sizeUV, new Vector4(t, 0, 0, 1)));
                        ginds.Add(i);
                        ginds.Add(i + 1);
                        ginds.Add(i + 2);
                        ginds.Add(i + 1);
                        ginds.Add(i + 3);
                        ginds.Add(i + 2);

                        i = gverts.Count;
                        gverts.Add(new GrassVertex(offset,                        new Vector3(-.5f, 0, .5f), coord + Vector2.UnitY * sizeUV, new Vector4()));
                        gverts.Add(new GrassVertex(offset + new Vector3(s, 0, s), new Vector3(-.5f, 0, .5f), coord + Vector2.One * sizeUV, new Vector4()));
                        gverts.Add(new GrassVertex(offset + new Vector3(0, h, 0), new Vector3(-.5f, 0, .5f), coord, new Vector4(t, 0, 0, 1)));
                        gverts.Add(new GrassVertex(offset + new Vector3(s, h, s), new Vector3(-.5f, 0, .5f), coord + Vector2.UnitX * sizeUV, new Vector4(t, 0, 0, 1)));
                        ginds.Add(i);
                        ginds.Add(i + 2);
                        ginds.Add(i + 1);
                        ginds.Add(i + 1);
                        ginds.Add(i + 2);
                        ginds.Add(i + 3);

                        i = gverts.Count;
                        gverts.Add(new GrassVertex(offset + new Vector3(0, 0, s), new Vector3(-.5f, 0, -.5f), coord + Vector2.UnitY * sizeUV, new Vector4()));
                        gverts.Add(new GrassVertex(offset + new Vector3(s, 0, 0), new Vector3(-.5f, 0, -.5f), coord + Vector2.One * sizeUV, new Vector4()));
                        gverts.Add(new GrassVertex(offset + new Vector3(0, h, s), new Vector3(-.5f, 0, -.5f), coord, new Vector4(t, 0, 0, 1)));
                        gverts.Add(new GrassVertex(offset + new Vector3(s, h, 0), new Vector3(-.5f, 0, -.5f), coord + Vector2.UnitX * sizeUV, new Vector4(t, 0, 0, 1)));
                        ginds.Add(i);
                        ginds.Add(i + 1);
                        ginds.Add(i + 2);
                        ginds.Add(i + 1);
                        ginds.Add(i + 3);
                        ginds.Add(i + 2);

                        i = gverts.Count;
                        gverts.Add(new GrassVertex(offset + new Vector3(0, 0, s), new Vector3(.5f, 0, .5f), coord + Vector2.UnitY * sizeUV, new Vector4()));
                        gverts.Add(new GrassVertex(offset + new Vector3(s, 0, 0), new Vector3(.5f, 0, .5f), coord + Vector2.One * sizeUV, new Vector4()));
                        gverts.Add(new GrassVertex(offset + new Vector3(0, h, s), new Vector3(.5f, 0, .5f), coord, new Vector4(t, 0, 0, 1)));
                        gverts.Add(new GrassVertex(offset + new Vector3(s, h, 0), new Vector3(.5f, 0, .5f), coord + Vector2.UnitX * sizeUV, new Vector4(t, 0, 0, 1)));
                        ginds.Add(i);
                        ginds.Add(i + 2);
                        ginds.Add(i + 1);
                        ginds.Add(i + 1);
                        ginds.Add(i + 2);
                        ginds.Add(i + 3);
                    }
                    #endregion
                    break;
            }
        }

        public void buildGeometry(object dvc) {
            building = true;
            built = false;

            List<VertexPositionNormalTexture> verts = new List<VertexPositionNormalTexture>();
            List<int> inds = new List<int>();

            List<GrassVertex> gverts = new List<GrassVertex>();
            List<int> ginds = new List<int>();

            percentDone = 0f;
            float total = CHUNK_SIZE * CHUNK_SIZE * CHUNK_HEIGHT;
            int highest = 0;
            for (int x = X * CHUNK_SIZE; x < X * CHUNK_SIZE + CHUNK_SIZE; x++)
                for (int y = 0; y < CHUNK_HEIGHT; y++)
                    for (int z = Z * CHUNK_SIZE; z < Z * CHUNK_SIZE + CHUNK_SIZE; z++) {
                        buildVoxel(x, y, z, ref verts, ref inds, ref gverts, ref ginds);
                        percentDone += 1f / total;
                        if (BlockAt(x, y, z) != null && y + 1 > highest)
                            highest = y + 1;
                    }
            
            GraphicsDevice device = dvc as GraphicsDevice;

            while (drawing && this != null) {
                Thread.Sleep(100);
            }
            if (this == null)
                return;

            settingVertexBuffers = true;
            if (verts.Count > 0) {
                VertexBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), verts.Count, BufferUsage.None);
                VertexBuffer.SetData(verts.ToArray());
                IndexBuffer = new IndexBuffer(device, typeof(int), inds.Count, BufferUsage.None);
                IndexBuffer.SetData(inds.ToArray());
            }
            if (gverts.Count > 0) {
                GrassVertexBuffer = new VertexBuffer(device, typeof(GrassVertex), gverts.Count, BufferUsage.None);
                GrassVertexBuffer.SetData(gverts.ToArray());
                GrassIndexBuffer = new IndexBuffer(device, typeof(int), ginds.Count, BufferUsage.None);
                GrassIndexBuffer.SetData(ginds.ToArray());
            }
            settingVertexBuffers = false;

            bbox = new BoundingBox(Position, Position + new Vector3(CHUNK_SIZE, highest, CHUNK_SIZE));

            building = false;
            built = true;
        }

        public void Generate(object device) {
            if (building) return;
            building = true;
            
            Stopwatch timer = Stopwatch.StartNew();

            // Generate blocks
            world.Generator.GenChunk(this);
            genTime = timer.Elapsed.TotalSeconds;

            // Generate geometry
            timer.Restart();
            if (device != null) buildGeometry(device);
            buildTime = timer.Elapsed.TotalSeconds;

            // Generate lights
            timer.Restart();
            generateLight();
            lightTime = timer.Elapsed.TotalSeconds;

            timer.Stop();

            Console.WriteLine("Done building:\n\tBlock: {0}s\n\tGeometry: {1}s\n\tLight: {2}s", genTime, buildTime, lightTime);
        }

        public void buildGeometryAndLight(object device) {
            if (device != null) {
                buildGeometry(device);
            } else
                geometryDirty = true;
            generateLight();
        }

        /// <summary>
        /// Sets the light at WORLD COORDINATES (x,y,z)
        /// </summary>
        public bool SetLight(int x, int y, int z, float l, LightType type) {
            if (l == GetLight(x, y, z, type)) return false;

            int x2 = x - X * CHUNK_SIZE;
            int z2 = z - Z * CHUNK_SIZE;
            
            if (x2 < 0)
                if (chunkXneg != null)
                    return chunkXneg.SetLight(x, y, z, l, type);
                else return false;
            if (z2 < 0)
                if (chunkZneg != null)
                    return chunkZneg.SetLight(x, y, z, l, type);
                else return false;
            if (y < 0 || y >= CHUNK_HEIGHT)
                return false;
            if (x2 >= CHUNK_SIZE)
                if (chunkXpos != null)
                    return chunkXpos.SetLight(x, y, z, l, type);
                else return false;
            if (z2 >= CHUNK_SIZE)
                if (chunkZpos != null)
                    return chunkZpos.SetLight(x, y, z, l, type);
                else return false;
            
            if (type == LightType.Block || type == LightType.Both)
                blockLight[x2 + CHUNK_SIZE * (y + CHUNK_HEIGHT * z2)] = l;
            if (type == LightType.Sky || type == LightType.Both)
                skyLight[x2 + CHUNK_SIZE * (y + CHUNK_HEIGHT * z2)] = l;

            lightDirty = true;

            if (x2 == 0 && chunkXneg != null)
                chunkXneg.setLightDirty();
            if (x2 == CHUNK_SIZE - 1 && chunkXpos != null)
                chunkXpos.setLightDirty();

            if (z2 == 0 && chunkZneg != null)
                chunkZneg.setLightDirty();
            if (z2 == CHUNK_SIZE - 1 && chunkZpos != null)
                chunkZpos.setLightDirty();
            return true;
        }

        /// <summary>
        /// Gets the light at WORLD COORDINATES (x,y,z)
        /// </summary>
        public float GetLight(int x, int y, int z, LightType type) {
            int x2 = x - X * CHUNK_SIZE;
            int z2 = z - Z * CHUNK_SIZE;

            if (x2 < 0)
                if (chunkXneg != null)
                    return chunkXneg.GetLight(x, y, z, type);
                else return 0f;
            if (z2 < 0)
                if (chunkZneg != null)
                    return chunkZneg.GetLight(x, y, z, type);
                else return 0f;
            if (y < 0 || y >= CHUNK_HEIGHT)
                return 0f;
            if (x2 >= CHUNK_SIZE)
                if (chunkXpos != null)
                    return chunkXpos.GetLight(x, y, z, type);
                else return 0f;
            if (z2 >= CHUNK_SIZE)
                if (chunkZpos != null)
                    return chunkZpos.GetLight(x, y, z, type);
                else return 0f;

            if (type == LightType.Sky)
                return skyLight[x2 + CHUNK_SIZE * (y + CHUNK_HEIGHT * z2)];
            else if (type == LightType.Block)
                return blockLight[x2 + CHUNK_SIZE * (y + CHUNK_HEIGHT * z2)];
            
            return blockLight[x2 + CHUNK_SIZE * (y + CHUNK_HEIGHT * z2)] * world.AmbientBrightness + skyLight[x2 + CHUNK_SIZE * (y + CHUNK_HEIGHT * z2)];
        }

        /// <summary>
        /// Generates light values for each light source, then spreads the light out
        /// </summary>
        public void generateLight() {
            int wx = CHUNK_SIZE * X;
            int wz = CHUNK_SIZE * Z;
            List<Tuple<int, int, int>> bSrcs = new List<Tuple<int, int, int>>();
            List<Tuple<int, int, int>> sSrcs = new List<Tuple<int, int, int>>();
            for (int x = wx; x < wx + CHUNK_SIZE; x++) {
                for (int z = wz; z < wz + CHUNK_SIZE; z++) {
                    float l = 1f;
                    for (int y = CHUNK_HEIGHT - 1; y >= 0; y--) {
                        Block b = BlockAt(x, y, z);
                        SetLight(x, y, z, 0, LightType.Both);
                        if (b != null) {
                            if (b.LightEmission > 0) {
                                SetLight(x, y, z, b.LightEmission, LightType.Block);
                                bSrcs.Add(new Tuple<int, int, int>(x, y, z));
                            }
                            l *= b.LightAttenuation;
                        }
                        if (l > 0) {
                            SetLight(x, y, z, l, LightType.Sky);
                            sSrcs.Add(new Tuple<int, int, int>(x, y, z));
                        }
                    }
                }
            }
            spreadLight(bSrcs, LightType.Block);
            spreadLight(sSrcs, LightType.Sky);
        }

        /// <summary>
        /// Tries to spread light around (x,y,z)
        /// </summary>
        void lightSweep(int x, int y, int z, ref List<Tuple<int, int, int>> updatelist, LightType type) {
            lightSweep(x, y, z, Vector3.UnitX, ref updatelist, type);
            lightSweep(x, y, z, Vector3.UnitY, ref updatelist, type);
            lightSweep(x, y, z, Vector3.UnitZ, ref updatelist, type);
        }
        void lightSweep(int x, int y, int z, Vector3 axis, ref List<Tuple<int, int, int>> updatelist, LightType type) {
            float l = GetLight(x, y, z, type);

            for (int i = -1; i <= 1; i++) {
                if (i == 0) i++;
                Vector3 ax = axis * i;
                Block b2 = BlockAt(x + (int)ax.X, y + (int)ax.Y, z + (int)ax.Z);
                float l2 = GetLight(x + (int)ax.X, y + (int)ax.Y, z + (int)ax.Z, type);
                float a = l * (b2 != null ? b2.LightAttenuation : World.LIGHT_AIR_ATTENUATION);
                if ((b2 == null || !b2.Opaque) && l2 < l && l2 != a && a > .1f) {
                    SetLight(x + (int)ax.X, y + (int)ax.Y, z + (int)ax.Z, a, type);
                    updatelist.Add(new Tuple<int, int, int>(x + (int)ax.X, y + (int)ax.Y, z + (int)ax.Z));
                }
            }
        }
        /// <summary>
        /// Spreads all the light fer dayzzz
        /// </summary>
        public void spreadLight(List<Tuple<int, int, int>> updatelist, LightType type) {
            if (type != LightType.Both) {
                while (updatelist.Count > 0) {
                    List<Tuple<int, int, int>> newlist = new List<Tuple<int, int, int>>();
                    for (int i = 0; i < updatelist.Count; i++)
                        lightSweep(updatelist[i].Item1, updatelist[i].Item2, updatelist[i].Item3, ref newlist, type);
                    updatelist = newlist;
                }
                lightDirty = true;
            }
        }

        bool updatingLight;
        /// <summary>
        /// Create the light texture for drawing
        /// red channel is sky light, green channel is block light
        /// </summary>
        void UpdateLightTexture(object ctx = null) {
            updatingLight = true;
            if (lightTexture != null) {
                int wx = X * CHUNK_SIZE;
                int wz = Z * CHUNK_SIZE;
                Color[] colors = new Color[(CHUNK_SIZE + 2) * CHUNK_HEIGHT * (CHUNK_SIZE + 2)];
                for (int x = 0; x < CHUNK_SIZE + 2; x++) {
                    for (int z = 0; z < CHUNK_SIZE + 2; z++) {
                        for (int y = 0; y < CHUNK_HEIGHT; y++) {
                            float bl = GetLight(x - 1 + wx, y, z - 1 + wz, LightType.Block);
                            float sl = GetLight(x - 1 + wx, y, z - 1 + wz, LightType.Sky);
                            int i = x + (CHUNK_SIZE + 2) * (y + CHUNK_HEIGHT * z);
                            colors[i] = new Color(sl, bl, 0);
                        }
                    }
                }
                while (drawing) {
                    Thread.Sleep(10);
                }

                settingLightTexture = true;
                lightTexture.SetData<Color>(colors);
                settingLightTexture = false;
                lightDirty = false;
            }
            updatingLight = false;
        }

        public bool Render(GraphicsDevice device, BoundingFrustum camFrustum, Effect effect, bool drawGrass = true, bool debugDraw = false) {
            if (lightTexture == null) {
                lightTexture = new Texture3D(device, CHUNK_SIZE + 2, CHUNK_HEIGHT, CHUNK_SIZE + 2, false, SurfaceFormat.Color);
                lightDirty = true;
            }

            if (geometryDirty) {
                device.SetVertexBuffer(null);
                device.Indices = null;

                buildGeometry(device);
                geometryDirty = false;
            }

            if (lightDirty && !updatingLight) {
                updatingLight = true;
                drawing = true;
                ThreadPool.QueueUserWorkItem(new WaitCallback(UpdateLightTexture));
            }
            drawing = true;
            
            if (settingLightTexture) {
                for (int i = 0; i < 3; i++)
                    if (device.Textures[i] == lightTexture)
                        device.Textures[i] = null; // unset light texture
            } else
                effect.Parameters["LightTexture"].SetValue(lightTexture);

            effect.Parameters["W"].SetValue(Matrix.Identity);
            effect.Parameters["chunkPos"].SetValue(Position);
            effect.Parameters["lightTexSize"].SetValue(new Vector3(CHUNK_SIZE + 2, CHUNK_HEIGHT, CHUNK_SIZE + 2));

            if (drawGrass && GrassVertexBuffer != null && GrassIndexBuffer != null && !settingVertexBuffers) {
                effect.CurrentTechnique = effect.Techniques["Grass"];// + (depth ? "Depth" : "")];
                device.SetVertexBuffer(GrassVertexBuffer);
                device.Indices = GrassIndexBuffer;
                foreach (EffectPass p in effect.CurrentTechnique.Passes) {
                    p.Apply();
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, GrassVertexBuffer.VertexCount, 0, GrassIndexBuffer.IndexCount / 3);
                }
            }
            if (VertexBuffer != null && IndexBuffer != null && !settingVertexBuffers) {
                effect.CurrentTechnique = effect.Techniques["Block"];// + (depth ? "Depth" : "")];
                device.SetVertexBuffer(VertexBuffer);
                device.Indices = IndexBuffer;
                foreach (EffectPass p in effect.CurrentTechnique.Passes) {
                    p.Apply();
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexBuffer.VertexCount, 0, IndexBuffer.IndexCount / 3);
                }
            }
            device.SetVertexBuffer(null);
            device.Indices = null;
            for (int i = 0; i < 3; i++)
                if (device.Textures[i] == lightTexture)
                    device.Textures[i] = null; // unset light texture

            drawing = false;

            return true;
        }
    }
}

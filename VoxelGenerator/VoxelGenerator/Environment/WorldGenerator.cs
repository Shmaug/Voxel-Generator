using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelGenerator.Environment {
    class WorldGenerator {
        public int Seed = 0;

        PerlinNoise sfcNoise;
        PerlinNoise treeNoise;
        PerlinNoise oceanNoise;

        float caveNoiseIso = 0.5f;
        float oceanNoiseIso = 0f;
        int waterHeight = (int)(Chunk.CHUNK_HEIGHT * .6);

        public WorldGenerator(int seed) {
            Seed = seed;
            sfcNoise = new PerlinNoise(seed, 16, 1, .005, 0.5);
            treeNoise = new PerlinNoise(seed * 10, 8, 1, .1, .5);
            oceanNoise = new PerlinNoise(seed, 16, 1, .001, .5);

            new Random(seed).NextBytes(SimplexNoise.perm);
        }

        float getOceanNiose(int x, int z) {
            return (float)oceanNoise.Generate(x + int.MaxValue / 2, z + int.MaxValue / 2);
        }
        float getTreeNoise(int x, int z) {
            return (float)treeNoise.Generate(x + int.MaxValue / 2, z + int.MaxValue / 2);
        }
        float getSfcNoise(int x, int z) {
            return (float)sfcNoise.Generate(x + int.MaxValue / 2, z + int.MaxValue / 2);
        }
        float getCaveNoise(int x, int y, int z) {
            return -SimplexNoise.Generate(x / 50f, y / 50f, z / 50f);
        }
        int heightAt(int x, int z) {
            return (int)(getSfcNoise(x, z) * (MathHelper.Clamp((1 - getOceanNiose(x, z)) * 2, 0.5f, 1)) * Chunk.CHUNK_HEIGHT * .2f + Chunk.CHUNK_HEIGHT * .6f);
        }

        /// <summary>
        /// Grows tree with a base at (x,y,z)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void GrowTree(int x, int y, int z, int height, Chunk chunk) {
            if (y + height < Chunk.CHUNK_HEIGHT) {
                for (int y2 = y; y2 < y + height; y2++) {
                    chunk.blocks[x, y2, z] = new Block(BlockType.Log, new Vector3(x, y2, z), chunk);
                }
            }
        }

        public void GenChunk(Chunk chunk) {
            Random r = new Random((chunk.X + chunk.Z) * (chunk.X + chunk.Z + 1) / 2 + chunk.Z);

            Vector3 chunkpos = new Vector3(chunk.X * Chunk.CHUNK_SIZE, 0, chunk.Z * Chunk.CHUNK_SIZE);
            #region terrain height, grass blocks
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    int wx = chunk.X * Chunk.CHUNK_SIZE + x;
                    int wz = chunk.Z * Chunk.CHUNK_SIZE + z;

                    int h = heightAt(wx, wz);
                    for (int y = 0; y <= h; y++) {
                        if (y == h) {
                            chunk.blocks[x, y, z] = new Block(BlockType.Grass, chunkpos + new Vector3(x, y, z), chunk);
                        } else if (y > h - 5)
                            chunk.blocks[x, y, z] = new Block(BlockType.Dirt, chunkpos + new Vector3(x, y, z), chunk);
                        else
                            chunk.blocks[x, y, z] = new Block(BlockType.Stone, chunkpos + new Vector3(x, y, z), chunk);
                    }
                }
            }
            #endregion

            #region caves
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    int wx = chunk.X * Chunk.CHUNK_SIZE + x;
                    int wz = chunk.Z * Chunk.CHUNK_SIZE + z;
                    for (int y = 0; y <= heightAt(wx, wz); y++) {
                        if (getCaveNoise(wx, y, wz) > caveNoiseIso) {
                            chunk.blocks[x, y, z] = null;
                        }
                    }
                }
            }
            #endregion

            #region ocean
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    int wx = chunk.X * Chunk.CHUNK_SIZE + x;
                    int wz = chunk.Z * Chunk.CHUNK_SIZE + z;
                    if (getOceanNiose(wx, wz) > oceanNoiseIso) {
                        for (int y = waterHeight; y >= 0; y--) {
                            if (chunk.blocks[x, y, z] != null) {
                                if (chunk.blocks[x, y, z].Type == BlockType.Grass && y != waterHeight)
                                    chunk.blocks[x, y, z] = new Block(BlockType.Dirt, chunkpos + new Vector3(x, y, z), chunk);
                                break;
                            }
                            chunk.blocks[x, y, z] = new Block(BlockType.Water, chunkpos + new Vector3(x, y, z), chunk);
                        }
                    }
                }
            }
            #endregion

            #region world floor
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                    for (int y = 0; y <= 2; y++)
                        chunk.blocks[x, y, z] = new Block(BlockType.Stone, chunkpos + new Vector3(x, y, z), chunk);
            #endregion

            #region tall grass
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++) {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++) {
                    int wx = chunk.X * Chunk.CHUNK_SIZE + x;
                    int wz = chunk.Z * Chunk.CHUNK_SIZE + z;

                    int y = heightAt(wx, wz);
                    if (chunk.blocks[x, y, z] != null && chunk.blocks[x, y, z].Type == BlockType.Grass) {
                        if (r.Next(0, 10) < 3) {
                            chunk.blocks[x, y + 1, z] = new Block(BlockType.TallGrass, chunkpos + new Vector3(x, y + 1, z), chunk);
                            chunk.blocks[x, y + 1, z].TextureId = r.Next(0, 6);
                            chunk.blocks[x, y + 1, z].Data = new Vector4((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble());
                        }
                    }
                }
            }
            #endregion
            
            #region trees
            for (int x = 2; x < Chunk.CHUNK_SIZE - 2;) {
                int xd = 1;
                for (int z = 2; z < Chunk.CHUNK_SIZE - 2;) {
                    float tn = getTreeNoise(chunk.X * Chunk.CHUNK_SIZE + x, chunk.Z * Chunk.CHUNK_SIZE + z);
                    int height = 5;

                    int start = heightAt(chunk.X * Chunk.CHUNK_SIZE + x, chunk.Z * Chunk.CHUNK_SIZE + z);
                    if (tn > 0f && chunk.blocks[x, start, z] != null && chunk.blocks[x, start, z].Type == BlockType.Grass) {
                        chunk.blocks[x, start, z] = new Block(BlockType.Dirt, chunkpos + new Vector3(x, start, z), chunk);

                        // Leaves
                        for (int y = 2; y >= 0; y--) {
                            if (y == 0) {
                                for (int x2 = x - 1; x2 <= x + 1; x2++)
                                    chunk.blocks[x2, start + height, z] = new Block(BlockType.Leaves, chunkpos + new Vector3(x2, start + height, z), chunk);

                                for (int z2 = z - 1; z2 <= z + 1; z2++)
                                    chunk.blocks[x, start + height, z2] = new Block(BlockType.Leaves, chunkpos + new Vector3(x, start + height, z2), chunk);
                            } else {
                                for (int x2 = x - 2; x2 <= x + 2; x2++)
                                    for (int z2 = z - 2; z2 <= z + 2; z2++)
                                        if (!((x2 == x + 2 || x2 == x - 2) && (z2 == z - 2 || z2 == z + 2) && y == 1))
                                            chunk.blocks[x2, start + height - y, z2] = new Block(BlockType.Leaves, chunkpos + new Vector3(x2, start + height - y, z2), chunk);
                            }
                        }
                        // Trunk
                        for (int y = start + 1; y < start + height; y++)
                            chunk.blocks[x, y, z] = new Block(BlockType.Log, chunkpos + new Vector3(x, y, z), chunk);

                        z += r.Next(3, 6);
                        xd = r.Next(3, 6);
                    } else {
                        z++;
                    }
                }
                x += xd;
            }
            #endregion
        }
    }
}

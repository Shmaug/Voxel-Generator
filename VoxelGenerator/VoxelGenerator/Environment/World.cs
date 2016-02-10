using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using VoxelGenerator.Dynamic;

namespace VoxelGenerator.Environment {
    class World {
        public static readonly float LIGHT_AIR_ATTENUATION = .8f;

        public static TextureCube NightSkybox;
        public static Effect skyEffect;
        public static Effect postfxEffect;
        public static Effect terrainEffect;
        public static Effect debugEffect;
        public static Texture2D SunTexture;
        public static Texture2D MoonTexture;

        public List<Chunk> Chunks;

        public int chunkLoadRadius = 5;

        public int ChunksDrawnLastFrame { get; private set; }

        public float Gravity = -12f;

        public WorldGenerator Generator;

        public Billboard Sun;
        public Billboard Moon;
        VertexPositionColor[] SkyboxVerts;
        int[] SkyboxInds;
        public float TimeOfDay;
        public bool IsDayTime {
            get {
                return LightDirection.Y < 0;
            }
        }
        public Vector3 LightDirection {
            get {
                Vector3 vec = new Vector3(
                        (float)Math.Sin((TimeOfDay - 12f) * (MathHelper.TwoPi / 24f)),
                        (float)-Math.Cos((TimeOfDay - 12f) * (MathHelper.TwoPi / 24f)), 0);
                return vec;
            }
        }
        public float AmbientBrightness {
            get {
                float a = MathHelper.Clamp(
                    -LightDirection.Y*5f
                    , 0.05f, 1f);
                return a;
            }
        }

        public bool DrawGrass = true;
        public float totalTime;

        public RenderTarget2D sunRenderTarget;
        public RenderTarget2D postfxRenderTarget1;
        public RenderTarget2D postfxRenderTarget2;
        public RenderTarget2D sceneRenderTarget;
        //public RenderTarget2D depthRenderTarget;

        float[] blurKernel;
        Vector2[] blurXOffsets;
        Vector2[] blurYOffsets;

        public World(int seed, bool server = false) {
            Chunks = new List<Chunk>();
            Generator = new WorldGenerator(seed);
            TimeOfDay = 6;

            if (!server) {
                Sun = new Billboard(Vector3.Zero, .5f, SunTexture, Color.White);
                Moon = new Billboard(Vector3.Zero, .3f, MoonTexture, Color.White);

                SkyboxVerts = new VertexPositionColor[Util.CubeVerticies.Length];
                for (int i = 0; i < Util.CubeVerticies.Length; i++)
                    SkyboxVerts[i] = new VertexPositionColor(Util.CubeVerticies[i].Position, Color.Black);
                SkyboxInds = new int[Util.CubeIndicies.Length];
                for (int i = 0; i < Util.CubeIndicies.Length; i++)
                    SkyboxInds[i] = Util.CubeIndicies[i];
            }
        }

        Matrix getLightProjection(Vector3 center) {
            Matrix v = Matrix.CreateLookAt(center - LightDirection * 50f, center, Vector3.Cross(LightDirection, Vector3.Backward));
            return v * Matrix.CreateOrthographic(100, 100, 1, 100);
                //Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(70f), 1.778f, 1f, 20f);
        }

        public void Update(float seconds) {
            TimeOfDay += seconds * .05f;
            if (TimeOfDay > 24f)
                TimeOfDay -= 24f;
            totalTime += seconds;
        }

        /// <summary>
        /// Gets the block at x, y, z
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public Block BlockAt(int x, int y, int z) {
            foreach (Chunk c in Chunks)
                if (c.X == x / Chunk.CHUNK_SIZE && c.Z == z / Chunk.CHUNK_SIZE)
                    return c.BlockAt(x, y, z);
            return null;
        }

        public Chunk ChunkAt(float x, float y, float z) {
            foreach (Chunk c in Chunks)
                if (c.X == (int)x / Chunk.CHUNK_SIZE && c.Z == (int)z / Chunk.CHUNK_SIZE)
                    return c;
            return null;
        }

        public bool SetBlock(int x, int y, int z, Block block) {
            foreach (Chunk c in Chunks)
                if (c.X == x / Chunk.CHUNK_SIZE && c.Z == z / Chunk.CHUNK_SIZE)
                    return c.SetBlock(x, y, z, block);
            return false;
        }

        public bool SetBlock(int x, int y, int z, BlockType block) {
            foreach (Chunk c in Chunks)
                if (c.X == x / Chunk.CHUNK_SIZE && c.Z == z / Chunk.CHUNK_SIZE)
                    return c.SetBlock(x, y, z, new Block(block, new Vector3(x, y, z), c));
            return false;
        }

        /// <summary>
        /// Load all chunks around the camera and unload chunks away from camera
        /// </summary>
        /// <param name="device">Will not generate geometry if set to null</param>
        /// <returns>Whether or not Chunks was modified</returns>
        public bool LoadChunks(GraphicsDevice device, params Vector3[] Positions) {
            bool mod = false;
            // unload chunks outside
            for (int i = 0; i < Chunks.Count; i++) {
                bool rmv = true;
                for (int j = 0; j < Positions.Length; j++) {
                    Vector3 pos = new Vector3(Positions[j].X, 0, Positions[j].Z);
                    if (Vector3.DistanceSquared(pos, Chunks[i].Position) <= (chunkLoadRadius * Chunk.CHUNK_SIZE) * (chunkLoadRadius * Chunk.CHUNK_SIZE))
                        rmv = false;
                }
                if (rmv) {
                    Chunks.Remove(Chunks[i]);
                    i--;
                    mod = true;
                }
            }

            List<Chunk> toAdd = new List<Chunk>();
            for (int i = 0; i < Positions.Length; i++) {
                Vector3 pos = new Vector3(Positions[i].X, 0, Positions[i].Z);
                int cx = (int)(Positions[i].X / Chunk.CHUNK_SIZE);
                int cz = (int)(Positions[i].Z / Chunk.CHUNK_SIZE);
                // load chunks around camera
                for (int x = cx - chunkLoadRadius; x < cx + chunkLoadRadius; x++) {
                    for (int z = cz - chunkLoadRadius; z < cz + chunkLoadRadius; z++) {
                        bool gen = true;
                        // Don't generate if outside chunk load radius
                        if (Vector3.DistanceSquared(pos, new Vector3(x, 0, z) * Chunk.CHUNK_SIZE) > (chunkLoadRadius * Chunk.CHUNK_SIZE) * (chunkLoadRadius * Chunk.CHUNK_SIZE))
                            gen = false;
                        // Don't generate if already generated
                        foreach (Chunk c in Chunks) {
                            if (c.X == x && c.Z == z) {
                                gen = false;
                                break;
                            }
                        }
                        // Don't generate if going to generate already
                        foreach (Chunk c in toAdd) {
                            if (c.X == x && c.Z == z) {
                                gen = false;
                                break;
                            }
                        }
                        if (gen)
                            toAdd.Add(new Chunk(x, z, this));
                    }
                }
            }
            foreach (Chunk c in toAdd) {
                Chunks.Add(c);
                mod = true;

                if (device != null) {
                    foreach (Chunk c2 in Chunks) {
                        if (c2.X == c.X + 1 && c2.Z == c.Z) {
                            c.chunkXpos = c2;
                            if (c2.chunkXneg == null)
                                ThreadPool.QueueUserWorkItem(new WaitCallback(c2.buildGeometryAndLight), device);
                            c2.chunkXneg = c;
                        } else if (c2.X == c.X - 1 && c2.Z == c.Z) {
                            c.chunkXneg = c2;
                            if (c2.chunkXpos == null)
                                ThreadPool.QueueUserWorkItem(new WaitCallback(c2.buildGeometryAndLight), device);
                            c2.chunkXpos = c;
                        } else if (c2.X == c.X && c2.Z == c.Z + 1) {
                            c.chunkZpos = c2;
                            if (c2.chunkZneg == null)
                                ThreadPool.QueueUserWorkItem(new WaitCallback(c2.buildGeometryAndLight), device);
                            c2.chunkZneg = c;
                        } else if (c2.X == c.X && c2.Z == c.Z - 1) {
                            c.chunkZneg = c2;
                            if (c2.chunkZpos == null)
                                ThreadPool.QueueUserWorkItem(new WaitCallback(c2.buildGeometryAndLight), device);
                            c2.chunkZpos = c;
                        }
                    }
                }
                ThreadPool.QueueUserWorkItem(new WaitCallback(c.Generate), device);
            }
            return mod;
        }

        /// <summary>
        /// Get aabb's of all blocks colliding with "box"
        /// </summary>
        /// <param name="box">Box to test</param>
        /// <returns></returns>
        public List<BoundingBox> getCollidingBoundingBoxes(BoundingBox box) {
            List<BoundingBox> intersections = new List<BoundingBox>();
            for (int y = (int)box.Min.Y - 2; y < (int)box.Max.Y + 2; y++) {
                for (int x = (int)box.Min.X - 2; x < (int)box.Max.X + 2; x++) {
                    for (int z = (int)box.Min.Z - 2; z < (int)box.Max.Z + 2; z++) {
                        Block b = BlockAt(x, y, z);
                        if (b != null && b.Collidable) {
                            if (b.hitbox.Contains(box) == ContainmentType.Intersects) {
                                intersections.Add(b.hitbox);
                            }
                        }
                    }
                }
            }
            return intersections;
        }

        /// <summary>
        /// Traces a ray through the world and returns the first block hit
        /// </summary>
        /// <param name="start">Ray start</param>
        /// <param name="end">Ray end</param>
        /// <returns>block, hit face, hit pos</returns>
        public Tuple<Block, BlockFace, Vector3> rayTraceBlocks(Vector3 start, Vector3 end) {
            int sx = (int)Math.Floor(start.X);
            int sy = (int)Math.Floor(start.Y);
            int sz = (int)Math.Floor(start.Z);
            int ex = (int)Math.Floor(end.X);
            int ey = (int)Math.Floor(end.Y);
            int ez = (int)Math.Floor(end.Z);
            if (ex < sx) {
                int t = ex;
                ex = sx;
                sx = t;
            }
            if (ey < sy) {
                int t = ey;
                ey = sy;
                sy = t;
            }
            if (ez < sz) {
                int t = ez;
                ez = sz;
                sz = t;
            }

            Vector3 dir = end - start;
            dir.Normalize();
            Ray r = new Ray(start, dir);

            float smallest = 0f;
            Block block = null;
            
            for (int x = sx; x < ex; x++) {
                for (int y = sy; y < ey; y++) {
                    for (int z = sz; z < ez; z++) {
                        Block b = BlockAt(x, y, z);
                        if (b != null) {

                            float? d = r.Intersects(b.hitbox);
                            if (d.HasValue) {
                                if (d.Value < smallest || smallest <= 0) {
                                    smallest = d.Value;
                                    block = b;
                                }
                            }
                        }
                    }
                }
            }

            if (block != null) {
                Vector3 pos = r.Position + r.Direction * smallest;

                BlockFace face = BlockFace.None;

                if (pos.X == block.hitbox.Min.X)
                    face = BlockFace.Left;
                else if (pos.X == block.hitbox.Max.X)
                    face = BlockFace.Right;

                if (pos.Z == block.hitbox.Min.Z)
                    face = BlockFace.Front;
                else if (pos.Z == block.hitbox.Max.Z)
                    face = BlockFace.Back;

                if (pos.Y == block.hitbox.Min.Y)
                    face = BlockFace.Bottom;
                else if (pos.Y == block.hitbox.Max.Y)
                    face = BlockFace.Top;

                return new Tuple<Block, BlockFace, Vector3>(block, face, pos);
            }

            return null;
        }

        public void Render(GraphicsDevice device, SpriteBatch sBatch, Camera camera, bool debug = false) {
            BoundingFrustum frustum = camera.Frustum;
            List<Chunk> toDraw = new List<Chunk>();
            foreach (Chunk c in Chunks)
                if (c.bbox.Intersects(frustum))
                    toDraw.Add(c);

            #region effect parameters
            terrainEffect.Parameters["GrassTimer"].SetValue(totalTime);
            terrainEffect.Parameters["BlockTexture"].SetValue(Block.BlockTexture);
            terrainEffect.Parameters["AmbientBrightness"].SetValue(AmbientBrightness);
            terrainEffect.Parameters["LightDirection"].SetValue(LightDirection * (IsDayTime ? 1 : -1));
            terrainEffect.Parameters["CameraPosition"].SetValue(camera.Position);
            terrainEffect.Parameters["VP"].SetValue(camera.View * camera.Projection);
            //terrainEffect.Parameters["LightWVP"].SetValue(getLightProjection(camera.Position));
            skyEffect.Parameters["W"].SetValue(Matrix.CreateTranslation(camera.Position));
            skyEffect.Parameters["VP"].SetValue(camera.View * camera.Projection);
            skyEffect.Parameters["CameraPosition"].SetValue(camera.Position);
            skyEffect.Parameters["SkyTexture"].SetValue(NightSkybox);
            skyEffect.Parameters["SkyAlpha"].SetValue(1f - (AmbientBrightness - .05f) / .95f);

            if (blurKernel == null || blurXOffsets == null || blurYOffsets == null) {
                Util.ComputeKernel(7, .5f, out blurKernel);
                Util.ComputeOffsets2D(7, sceneRenderTarget.Width, sceneRenderTarget.Height, out blurXOffsets, out blurYOffsets);
                postfxEffect.Parameters["weights"].SetValue(blurKernel);
                postfxEffect.Parameters["pixel"].SetValue(Vector2.One / new Vector2(sceneRenderTarget.Width, sceneRenderTarget.Height));
            }
            #endregion

            #region terrain
            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.Default;

            /*RasterizerState b4 = device.RasterizerState;
            device.RasterizerState = RasterizerState.CullClockwise;
            terrainEffect.Parameters["VP"].SetValue(getLightProjection(camera.Position));
            device.SetRenderTarget(depthRenderTarget);
            device.Clear(Color.Transparent);
            
            foreach (Chunk c in toDraw)
                 c.Render(device, frustum, terrainEffect, true, DrawGrass, debug);

            if (!debug) terrainEffect.Parameters["VP"].SetValue(camera.View * camera.Projection);*/
            //device.RasterizerState = b4;
            device.SetRenderTarget(sceneRenderTarget);
            device.Clear(Color.Transparent);

            //terrainEffect.Parameters["DepthTexture"].SetValue(depthRenderTarget);
            
            ChunksDrawnLastFrame = 0;
            foreach (Chunk c in toDraw)
                ChunksDrawnLastFrame += c.Render(device, frustum, terrainEffect, DrawGrass, debug) ? 1 : 0;

            device.SetVertexBuffer(null);
            device.Indices = null;
            #endregion

            Debug.DrawBoxes(device, debugEffect);

            #region sun & moon, sun shafts
            Sun.Position = camera.Position - LightDirection * 2f;
            Moon.Position = camera.Position + LightDirection * 2f;

            device.DepthStencilState = DepthStencilState.None;
            device.BlendState = BlendState.AlphaBlend;

            device.SetRenderTarget(sunRenderTarget);
            device.Clear(Color.Transparent);

            Sun.Draw(device, camera);
            
            postfxEffect.CurrentTechnique = postfxEffect.Techniques["Occuld"];
            sBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, postfxEffect);
            sBatch.Draw(sceneRenderTarget, Vector2.Zero, Color.White);
            sBatch.End();

            // sun shafts
            device.SetRenderTarget(postfxRenderTarget1);
            device.Clear(Color.Transparent);
            Vector3 lp = device.Viewport.Project(Sun.Position, camera.Projection, camera.View, Matrix.Identity);
            postfxEffect.Parameters["lightPosition"].SetValue(new Vector2(lp.X, lp.Y) / new Vector2(sceneRenderTarget.Width, sceneRenderTarget.Height));
            postfxEffect.CurrentTechnique = postfxEffect.Techniques["Scatter"];
            sBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, postfxEffect);
            sBatch.Draw(sunRenderTarget, Vector2.Zero, Color.White);
            sBatch.End();

            // blur x
            device.SetRenderTarget(postfxRenderTarget2);
            device.Clear(Color.Transparent);
            postfxEffect.Parameters["offsets"].SetValue(blurXOffsets);
            postfxEffect.CurrentTechnique = postfxEffect.Techniques["Blur"];
            sBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, postfxEffect);
            sBatch.Draw(postfxRenderTarget1, Vector2.Zero, Color.White);
            sBatch.End();

            // blur y
            device.SetRenderTarget(sunRenderTarget);
            device.Clear(Color.Transparent);
            postfxEffect.Parameters["offsets"].SetValue(blurYOffsets);
            postfxEffect.CurrentTechnique = postfxEffect.Techniques["Blur"];
            sBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, postfxEffect);
            sBatch.Draw(postfxRenderTarget2, Vector2.Zero, Color.White);
            sBatch.End();
            #endregion
            
            #region skybox & moon
            device.SetRenderTarget(null);
            device.Clear(Color.LightSkyBlue);

            device.BlendState = BlendState.AlphaBlend;
            device.DepthStencilState = DepthStencilState.DepthRead;
            device.RasterizerState = new RasterizerState() { FillMode = device.RasterizerState.FillMode, CullMode = CullMode.CullClockwiseFace };
            skyEffect.CurrentTechnique = skyEffect.Techniques["Skybox"];
            foreach (EffectPass p in skyEffect.CurrentTechnique.Passes) {
                p.Apply();
                device.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, SkyboxVerts, 0, SkyboxVerts.Length, SkyboxInds, 0, SkyboxInds.Length / 3);
            }
            Moon.Draw(device, camera);
            device.RasterizerState = new RasterizerState() { FillMode = device.RasterizerState.FillMode, CullMode = CullMode.CullCounterClockwiseFace };
            device.DepthStencilState = DepthStencilState.Default;
            #endregion

            sBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            sBatch.Draw(sceneRenderTarget, Vector2.Zero, Color.White);
            sBatch.Draw(sunRenderTarget, Vector2.Zero, Color.White);
            sBatch.End();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using VoxelGenerator.Environment;

namespace VoxelGenerator.Dynamic {
    class Player : Entity {
        public bool LocalControl;
        public Camera Camera;
        Vector3 headBob;
        Vector3 headBobL;
        float headBobTime;

        public Player(World world) {
            Width = .6f;
            Height = 1.8f;
            this.world = world;
        }

        public void Update(float deltaSeconds, Vector2 mouseDelta, KeyboardState ks) {
            #region camera move
            Camera.Rotation.X += mouseDelta.Y * deltaSeconds * .2f;
            Camera.Rotation.Y += mouseDelta.X * deltaSeconds * .2f;
            Camera.Rotation.X = MathHelper.Clamp(Camera.Rotation.X, -MathHelper.PiOver2, MathHelper.PiOver2);
            Rotation = new Vector3(0, Camera.Rotation.Y, 0);
            #endregion

            /*
            Tuple<Block, BlockFace, Vector3> rayHit = world.rayTraceBlocks(Camera.Position, Camera.Position + Camera.RotationMatrix.Forward * 10);
            if (rayHit != null) {
                Debug.Track("Selected block: " + rayHit.Item1.Type, 0);
                Debug.TrackBox(rayHit.Item1.hitbox, Color.White, 1.01f, 10);
                if (ks.IsKeyDown(Keys.E)) {
                    world.SetBlock((int)Math.Floor(rayHit.Item1.Position.X), (int)Math.Floor(rayHit.Item1.Position.Y), (int)Math.Floor(rayHit.Item1.Position.Z), null);
                } else if (ks.IsKeyDown(Keys.Q)) {
                    Vector3 pos = rayHit.Item1.Position;
                    pos += Util.NormalFromFace(rayHit.Item2);
                    int x = (int)Math.Floor(pos.X);
                    int y = (int)Math.Floor(pos.Y);
                    int z = (int)Math.Floor(pos.Z);
                    world.SetBlock(x, y, z, new Block(BlockType.Dirt));
                }
            }*/

            Vector3 move = Vector3.Zero;
            #region controls
            if (ks.IsKeyDown(Keys.A))
                move += Vector3.Left;
            else if (ks.IsKeyDown(Keys.D))
                move += Vector3.Right;
            if (ks.IsKeyDown(Keys.W))
                move += Vector3.Forward;
            else if (ks.IsKeyDown(Keys.S))
                move += Vector3.Backward;
            if (!Gravity) {
                if (ks.IsKeyDown(Keys.LeftShift))
                    move += Vector3.Down;
                else if (ks.IsKeyDown(Keys.Space))
                    move += Vector3.Up;
            }
            if (move != Vector3.Zero) {
                headBobTime += deltaSeconds;
                move.Normalize();
                move = Vector3.Transform(move, Matrix.CreateRotationY(Rotation.Y));
                move *= 4;
                if ((!Gravity || (Gravity && Grounded)) && ks.IsKeyDown(Keys.LeftControl))
                    move *= 1.8f;

                if ((!Gravity || (Gravity && Grounded)) && ks.IsKeyDown(Keys.Tab))
                    move *= 20;
            }
            float l = 50f;
            if (!Grounded)
                l = 10f;

            if ((move.X > 0 && Velocity.X < move.X) ||
                (move.X < 0 && Velocity.X > move.X))
                Velocity.X = MathHelper.Lerp(Velocity.X, move.X, deltaSeconds * l);
            if ((move.Z > 0 && Velocity.Z < move.Z) ||
                (move.Z < 0 && Velocity.Z > move.Z))
                Velocity.Z = MathHelper.Lerp(Velocity.Z, move.Z, deltaSeconds * l);

            if (!Gravity) Velocity.Y = move.Y;
            if (Gravity && Grounded && ks.IsKeyDown(Keys.Space))
                Velocity.Y = -world.Gravity * .43f;
            #endregion
            Vector3 b4 = Position;
            bool gb4 = Grounded;

            update(deltaSeconds);

            if (Grounded && move != Vector3.Zero)
                headBobTime += Vector3.Distance(b4, Position);

            if (Grounded && (!gb4 || move == Vector3.Zero))
                headBobTime = 0f;
            
            //headBob = new Vector3(0, (float)Math.Sin(headBobTime * 2f) * .075f, 0) + Vector3.Transform(new Vector3((float)Math.Sin(headBobTime) * .1f, 0, 0), Camera.RotationMatrix);
            //headBobL = Vector3.Lerp(headBobL, headBob, (float)gameTime.ElapsedGameTime.TotalSeconds * 50f);

            Camera.Position = Position + new Vector3(0, .75f, 0) + headBobL;
        }
    }
}

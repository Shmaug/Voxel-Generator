using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

using VoxelGenerator.Environment;

namespace VoxelGenerator.Dynamic {
    class Entity {
        public World world;

        public Vector3 Position;
        public Vector3 Velocity;
        public Vector3 Rotation;
        public float Width; // in meters
        public float Height; // in meters
        public BoundingBox bbox {
            get {
                return new BoundingBox(
                    Position - new Vector3(Width / 2f, Height / 2f, Width / 2f),
                    Position + new Vector3(Width / 2f, Height / 2f, Width / 2f));
            }
        }
        public bool Grounded = false;
        public bool Gravity = true;

        public void update(float deltaSeconds) {
            if (Gravity) Velocity += Vector3.Up * world.Gravity * deltaSeconds;
            if (Velocity.LengthSquared() > (20 * 20)) {
                Velocity.Normalize();
                Velocity *= 20f;
            }
            if (Grounded)
                Velocity *= new Vector3(.8f, 1f, .8f);
            else
                Velocity *= new Vector3(.95f, 1f, .95f);

            Vector3 delta = Velocity * deltaSeconds;
            moveEntity(delta);
        }
        
        void moveEntity(Vector3 delta) {
            Vector3 d = delta;
            List<BoundingBox> collisions = world.getCollidingBoundingBoxes(new BoundingBox(bbox.Min + delta, bbox.Max + delta));
            
            foreach (BoundingBox b in collisions)
                delta.Y = Util.getYOffset(b, bbox, delta.Y);
            Position.Y += delta.Y;

            foreach (BoundingBox b in collisions)
                delta.X = Util.getXOffset(b, bbox, delta.X);
            Position.X += delta.X;

            foreach (BoundingBox b in collisions)
                delta.Z = Util.getZOffset(b, bbox, delta.Z);
            Position.Z += delta.Z;

            Grounded = d.Y != delta.Y && d.Y <= 0f;

            if (d.X != delta.X)
                Velocity.X = 0;
            if (d.Y != delta.Y)
                Velocity.Y = 0;
            if (d.Z != delta.Z)
                Velocity.Z = 0;
        }
    }
}

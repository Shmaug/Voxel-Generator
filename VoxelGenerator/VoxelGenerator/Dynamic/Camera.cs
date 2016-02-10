using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VoxelGenerator.Dynamic {
    class Camera {
        public Vector3 Position;
        public Vector3 Rotation;
        
        public Matrix RotationMatrix {
            get {
                return Matrix.CreateRotationX(Rotation.X) * Matrix.CreateRotationY(Rotation.Y);
            }
        }

        public bool Orthographic;
        public Matrix Projection { get; private set; }
        public Matrix View {
            get {
                return Matrix.CreateLookAt(Position, Position + RotationMatrix.Forward, RotationMatrix.Up);
            }
        }

        public BoundingFrustum Frustum {
            get {
                return new BoundingFrustum(View * Projection);
            }
        }
        float aspect;
        public float AspectRatio {
            get {
                return aspect;
            }
            set {
                aspect = value;
                Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FOV), aspect, .01f, 500f);
            }
        }
        float fov;
        public float FOV {
            get {
                return fov;
            }
            set {
                if (Orthographic)
                    Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FOV), aspect, .01f, 500f);
                else
                    Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FOV), aspect, .01f, 500f);
                fov = value;
            }
        }

        public Camera(float aspect, float fov = 70f) {
            Orthographic = false;
            Position = Vector3.Zero;
            Rotation = Vector3.Zero;
            this.aspect = aspect;
            this.fov = fov;
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FOV), aspect, .01f, 500f);
        }
    }
}

using Microsoft.Xna.Framework;

namespace Rune.Monogame.Geometry
{
    public class Ray
    {
        public Vector3 Position { get; }
        public Vector3 Direction { get; }

        public Ray(Vector3 position, Vector3 direction)
        {
            Position = position;
            Direction = direction;
        }
    }
}
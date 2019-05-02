using Microsoft.Xna.Framework;

namespace Rune.Monogame.Geometry
{
    public class Plane
    {
        public Vector3 Normal { get; }
        public float Distance { get; }
        
        public Plane(Vector3 normal, float distance)
        {
            Normal = normal;
            Distance = distance;
        }

        public Plane(Vector3 normal, Vector3 pointOnPlane)
        {
            Normal = normal;
            Distance = Vector3.Dot(normal, pointOnPlane);
        }

        /// <summary>
        /// Gets the distance to the plane with given point
        /// </summary>
        /// <param name="pointOnPlane">distance to this point</param>
        /// <returns>distance to the plane</returns>
        public float DistanceToPlane(Vector3 point)
        {
            return Vector3.Dot(Normal, point) - Distance;
        }
    }
}
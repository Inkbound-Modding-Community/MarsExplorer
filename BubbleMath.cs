using System.Numerics;

namespace MarsExplorer
{
    internal class BubbleMath
    {
        internal static float DistanceSquared(Point c, Point d)
        {
            Vector2 a = new(c.X, c.Y);
            Vector2 b = new(d.X, d.Y);
            Vector2 delta = a - b;
            return delta.LengthSquared();
        }
    }
}
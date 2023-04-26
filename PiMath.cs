using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class PiMath
{
    public static Vector2 Rotate(this Vector2 v, double degrees)
    {
        return new Vector2(
            (float)(v.X * Math.Cos(degrees) - v.Y * Math.Sin(degrees)),
            (float)(v.X * Math.Sin(degrees) + v.Y * Math.Cos(degrees))
        );
    }

    public static float Angle(this Vector2 vector)
    {
        return (float)Math.Atan2(vector.Y, vector.X);
    }

    public static float AngleTo(this Vector2 v, Vector2 other)
    {
        /*float atanA = (float)Math.Atan2(v.X, v.Y);
        float atanB = (float)Math.Atan2(other.X, other.Y);

        return atanA - atanB;*/

        return (float)Math.Atan2(
            other.X * v.Y - other.Y * v.X, 
            other.X * v.X + other.Y * v.Y);

    }
}


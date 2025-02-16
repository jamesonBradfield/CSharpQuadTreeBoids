using System;
public class Vector3i
{
    public int x;
    public int y;
    public int z;

    public Vector3i(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    // Addition operators
    public static Vector3i operator +(Vector3i a, Vector3i b)
        => new Vector3i(a.x + b.x, a.y + b.y, a.z + b.z);

    public static Vector3i operator +(Vector3i a, int scalar)
        => new Vector3i(a.x + scalar, a.y + scalar, a.z + scalar);

    // Subtraction operators
    public static Vector3i operator -(Vector3i a, Vector3i b)
        => new Vector3i(a.x - b.x, a.y - b.y, a.z - b.z);

    public static Vector3i operator -(Vector3i a, int scalar)
        => new Vector3i(a.x - scalar, a.y - scalar, a.z - scalar);

    // Multiplication operators
    public static Vector3i operator *(Vector3i a, Vector3i b)
        => new Vector3i(a.x * b.x, a.y * b.y, a.z * b.z);

    public static Vector3i operator *(Vector3i a, int scalar)
        => new Vector3i(a.x * scalar, a.y * scalar, a.z * scalar);

    public static Vector3i operator *(int scalar, Vector3i a)
        => a * scalar;

    public static Vector3i operator *(Vector3i a,float scalar)
        => new Vector3i((int)(a.x * scalar),(int)(a.y * scalar),(int)(a.z * scalar));

    // Division operators
    public static Vector3i operator /(Vector3i a, int scalar)
        => new Vector3i(a.x / scalar, a.y / scalar, a.z / scalar);

    // Unary negation
    public static Vector3i operator -(Vector3i a)
        => new Vector3i(-a.x, -a.y, -a.z);

    // Equality operators
    public static bool operator ==(Vector3i a, Vector3i b)
        => a.x == b.x && a.y == b.y && a.z == b.z;

    public static bool operator !=(Vector3i a, Vector3i b)
        => !(a == b);

    // Override Object methods
    public override bool Equals(object obj)
    {
        if (obj is Vector3i other)
            return this == other;
        return false;
    }

    public override int GetHashCode()
        => HashCode.Combine(x, y, z);

    public override string ToString()
        => $"({x}, {y}, {z})";

    // Utility methods
    public int LengthSquared()
        => x * x + y * y + z * z;

    public float Length()
        => (float)Math.Sqrt(LengthSquared());

   public Vector3i LimitLength(float maxLength)
    {
        float currentLength = Length();
        if (currentLength <= maxLength || currentLength == 0)
            return this;
            
        float scale = maxLength / currentLength;
        return new Vector3i(
            (int)(x * scale),
            (int)(y * scale),
            (int)(z * scale)
        );
    }

    public Vector3i Zero => new Vector3i(0, 0, 0);

    // Conversion methods
    public static explicit operator Vector3i(Godot.Vector3 v)
        => new Vector3i((int)v.X, (int)v.Y, (int)v.Z);

    public static explicit operator Godot.Vector3(Vector3i v)
        => new Godot.Vector3(v.x, v.y, v.z);
}


namespace DefaultNamespace;

using GdUnit4;
using static GdUnit4.Assertions;
using System;

[TestSuite]
public class Vector3iTest
{
    // Test normal addition
    [TestCase(1, 1, 1, 2, 2, 2, 3, 3, 3)]
    [TestCase(0, 0, 0, 1, 1, 1, 1, 1, 1)]
    public void Test_Addition(int x1, int y1, int z1, int x2, int y2, int z2, int expectedX, int expectedY, int expectedZ)
    {
        Vector3i vec1 = new Vector3i(x1, y1, z1);
        Vector3i vec2 = new Vector3i(x2, y2, z2);
        Vector3i result = vec1 + vec2;
        AssertThat(result).IsEqual(new Vector3i(expectedX, expectedY, expectedZ));
    }

    // Test addition overflow
    [TestCase(int.MaxValue, 0, 0, 1, 0, 0)]
    [TestCase(0, int.MaxValue, 0, 0, 1, 0)]
    [TestCase(0, 0, int.MaxValue, 0, 0, 1)]
    public void Test_Addition_Overflow(int x1, int y1, int z1, int x2, int y2, int z2)
    {
        Vector3i vec1 = new Vector3i(x1, y1, z1);
        Vector3i vec2 = new Vector3i(x2, y2, z2);
        AssertThrown(() => { var result = vec1 + vec2; });
    }

    // Test normal multiplication
    [TestCase(2, 2, 2, 3, 6, 6, 6)]
    [TestCase(1, 1, 1, 2, 2, 2, 2)]
    public void Test_Multiplication(int x, int y, int z, int scalar, int expectedX, int expectedY, int expectedZ)
    {
        Vector3i vec = new Vector3i(x, y, z);
        Vector3i result = vec * scalar;
        AssertThat(result).IsEqual(new Vector3i(expectedX, expectedY, expectedZ));
    }

    // Test multiplication overflow
    [TestCase(int.MaxValue, 0, 0, 2)]
    [TestCase(0, int.MaxValue, 0, 2)]
    [TestCase(0, 0, int.MaxValue, 2)]
    public void Test_Multiplication_Overflow(int x, int y, int z, int scalar)
    {
        Vector3i vec = new Vector3i(x, y, z);
        AssertThrown(() => { var result = vec * scalar; });
    }

    // Test normal LengthSquared
    [TestCase(1, 1, 1, 3)]
    [TestCase(2, 2, 2, 12)]
    public void Test_LengthSquared(int x, int y, int z, int expected)
    {
        Vector3i vec = new Vector3i(x, y, z);
        int result = vec.LengthSquared();
        AssertThat(result).IsEqual(expected);
    }

    // Test LengthSquared overflow
    [TestCase(46341, 46341, 46341)] // sqrt(Int.MaxValue) ≈ 46340.95
    public void Test_LengthSquared_Overflow(int x, int y, int z)
    {
        Vector3i vec = new Vector3i(x, y, z);
        AssertThrown(() => { var result = vec.LengthSquared(); });
    }

    // Test normal Length calculations
    [TestCase(5, 5, 5, 8.66025403784f, 8.66f)]
    [TestCase(4, 4, 4, 6.928203230275509f, 6.93f)]
    public void Test_Length(int x, int y, int z, float actual, float approx)
    {
        Vector3i vec = new Vector3i(x, y, z);
        float result = vec.Length();
        AssertThat(result).IsEqualApprox(actual, approx);
    }

    // Test normal LimitLength
    [TestCase(5, 5, 5, 5.0f, 2, 2, 2)]
    [TestCase(10, 10, 10, 10.0f, 5, 5, 5)]
    public void Test_LimitLength(int x, int y, int z, float maxLength, int expectedX, int expectedY, int expectedZ)
    {
        Vector3i vec = new Vector3i(x, y, z);
        Vector3i result = vec.LimitLength(maxLength);
        AssertThat(result).IsEqual(new Vector3i(expectedX, expectedY, expectedZ));
    }

    // Test LimitLength with invalid input
    [TestCase(5, 5, 5, -1.0f)]
    public void Test_LimitLength_Invalid(int x, int y, int z, float maxLength)
    {
        Vector3i vec = new Vector3i(x, y, z);
        AssertThrown(() => { var result = vec.LimitLength(maxLength); });
    }

    // Test normal Normalized
    [TestCase(5, 5, 5, 577, 577, 577)]
    [TestCase(3, 4, 5, 424, 566, 707)]
    public void Test_Normalized(int x, int y, int z, int expectedX, int expectedY, int expectedZ)
    {
        Vector3i vec = new Vector3i(x, y, z);
        Vector3i result = vec.Normalized();
        AssertThat(result).IsEqual(new Vector3i(expectedX, expectedY, expectedZ));
    }

    // Test Normalized with zero vector
    [TestCase(0, 0, 0, 0, 0, 0)]
    public void Test_Normalized_Zero(int x, int y, int z, int expectedX, int expectedY, int expectedZ)
    {
        Vector3i vec = new Vector3i(x, y, z);
        Vector3i result = vec.Normalized();
        AssertThat(result).IsEqual(new Vector3i(expectedX, expectedY, expectedZ));
    }

    // Test division by zero
    [TestCase(5, 5, 5, 0)]
    public void Test_Division_By_Zero(int x, int y, int z, float divisor)
    {
        Vector3i vec = new Vector3i(x, y, z);
        AssertThrown(() => { var result = vec / divisor; });
    }

    // Test conversion overflow
    [TestCase(float.MaxValue, 0, 0)]
    public void Test_Conversion_Overflow(float x, float y, float z)
    {
        var godotVec = new Godot.Vector3(x, y, z);
        AssertThrown(() => { var result = (Vector3i)godotVec; });
    }
}

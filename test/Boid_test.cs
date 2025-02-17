namespace DefaultNamespace;

using GdUnit4;
using static GdUnit4.Assertions;
using Godot;
using System.Collections.Generic;

[TestSuite]
public class BoidTest
{
    private Boid? _sut;
    private BoidSettings _settings;

    [Before]
    public void Setup()
    {
        _settings = new BoidSettings
        {
            Maxspeed = 2.0f,
            Maxforce = 0.5f,
            Searchradius = 5.0f,
            Separationradius = 2.0f,
            Fieldofview = 120.0f,
            Alignmentweight = 1,
            Cohesionweight = 1,
            Separationweight = 1
        };

        // Use explicit zero velocity for predictable test behavior
        _sut = new Boid(Vector3.Zero, Vector3i.Zero, _settings);
        AssertThat(_sut).IsNotNull();
    }

    // Add a test for random initialization
    [TestCase]
    public void Test_RandomInitialization()
    {
        var randomBoid = new Boid(Vector3.Zero, _settings);
        AssertThat(randomBoid.velocity).IsNotEqual(Vector3i.Zero);

        // Verify velocity is scaled correctly
        float speed = randomBoid.velocity.Length();
        float expectedSpeed = _settings.Maxspeed * QuadTreeConstants.WORLD_TO_QUAD_SCALE;
        AssertThat(speed).IsEqualApprox(expectedSpeed, 1f);
    }
    [TestCase]
    public void Test_Alignment_WithNoNearbyBoids_ReturnsZero()
    {
        var result = _sut.Alignment(new List<Boid>());
        AssertThat(result).IsEqual(Vector3i.Zero);
    }

    [TestCase(100, 0, 0, 50, 0, 0)]  // Straight right
    [TestCase(0, 100, 0, 0, 50, 0)]  // Straight up
    [TestCase(0, 0, 100, 0, 0, 50)]  // Straight forward
    public void Test_Alignment_WithSingleBoid_MatchesVelocity(
        int velocityX, int velocityY, int velocityZ,
        int expectedX, int expectedY, int expectedZ)
    {
        var nearbyBoid = new Boid(Vector3.Zero, new Vector3i(velocityX, velocityY, velocityZ), _settings);
        var nearby = new List<Boid> { nearbyBoid };

        var result = _sut.Alignment(nearby);

        AssertThat(result.x).IsEqual(expectedX);
        AssertThat(result.y).IsEqual(expectedY);
        AssertThat(result.z).IsEqual(expectedZ);
    }

    [TestCase]
    public void Test_Cohesion_WithNoNearbyBoids_ReturnsZero()
    {
        var result = _sut.Cohesion(new List<Boid>());
        AssertThat(result).IsEqual(Vector3i.Zero);
    }


    [TestCase(5, 0, 0, 50, 0, 0, TestName = "Steer Right")]    // Far right, steer right
    [TestCase(0, 5, 0, 0, 50, 0)]    // Far above, steer up
    [TestCase(0, 0, 5, 0, 0, 50)]    // Far ahead, steer forward
    public void Test_Cohesion_WithSingleBoid_SteerTowardsIt(
        float posX, float posY, float posZ,
        int expectedX, int expectedY, int expectedZ)
    {
        var nearbyBoid = new Boid(new Vector3(posX, posY, posZ), Vector3i.Zero, _settings);
        var nearby = new List<Boid> { nearbyBoid };

        var result = _sut.Cohesion(nearby);

        AssertThat(result.x).IsEqual(expectedX);
        AssertThat(result.y).IsEqual(expectedY);
        AssertThat(result.z).IsEqual(expectedZ);
    }

    [TestCase]
    public void Test_Separation_WithNoNearbyBoids_ReturnsZero()
    {
        var result = _sut.Separation(new List<Boid>());
        AssertThat(result).IsEqual(Vector3i.Zero);
    }
    [TestCase]
    public void Test_Separation_Fuzz()
    {
        var random = new Random(42); // Fixed seed for reproducibility
        const int NUM_TESTS = 1000;
        const float TOLERANCE = 5.0f; // Tolerance for floating point comparisons

        for (int i = 0; i < NUM_TESTS; i++)
        {
            // Generate random position between -1 and 1
            Vector3 randomPos = new Vector3(
                (float)(random.NextDouble() * 2 - 1),
                (float)(random.NextDouble() * 2 - 1),
                (float)(random.NextDouble() * 2 - 1)
            ).Normalized(); // Normalize to ensure consistent distance

            var nearbyBoid = new Boid(randomPos, Vector3i.Zero, _settings);
            var nearby = new List<Boid> { nearbyBoid };

            var result = _sut.Separation(nearby);

            // Calculate expected force
            // The separation force should be in the opposite direction of the boid's position
            // and scaled by maxforce
            Vector3 expectedForce = -randomPos * (_settings.Maxforce * QuadTreeConstants.WORLD_TO_QUAD_SCALE);
            Vector3i expectedForceInt = new Vector3i(
                (int)expectedForce.X,
                (int)expectedForce.Y,
                (int)expectedForce.Z
            );

            // Debug output for failures
            if (Math.Abs(result.x - expectedForceInt.x) > TOLERANCE ||
                Math.Abs(result.y - expectedForceInt.y) > TOLERANCE ||
                Math.Abs(result.z - expectedForceInt.z) > TOLERANCE)
            {
                GD.Print($"Test {i} failed:");
                GD.Print($"Random position: {randomPos}");
                GD.Print($"Expected force: {expectedForceInt}");
                GD.Print($"Actual force: {result}");
                GD.Print($"Difference: {result - expectedForceInt}");
            }

            // Verify the result is close to expected
            AssertThat(result.x).IsEqualApprox(expectedForceInt.x, TOLERANCE);
            AssertThat(result.y).IsEqualApprox(expectedForceInt.y, TOLERANCE);
            AssertThat(result.z).IsEqualApprox(expectedForceInt.z, TOLERANCE);

            // Verify magnitude is close to maxforce
            float actualMagnitude = result.Length();
            float expectedMagnitude = _settings.Maxforce * QuadTreeConstants.WORLD_TO_QUAD_SCALE;
            AssertThat(actualMagnitude).IsEqualApprox(expectedMagnitude, TOLERANCE);

            // Verify direction is opposite to input position
            Vector3 resultDir = new Vector3(result.x, result.y, result.z).Normalized();
            Vector3 expectedDir = -randomPos;
            AssertThat(resultDir.X).IsEqualApprox(expectedDir.X, 0.1f);
            AssertThat(resultDir.Y).IsEqualApprox(expectedDir.Y, 0.1f);
            AssertThat(resultDir.Z).IsEqualApprox(expectedDir.Z, 0.1f);
        }
    }
    [TestCase(1, 0, 0, -50, 0, 0, TestName = "Steer Right")]
    [TestCase(1, 1, 0, -35, -35, 0, TestName = "Steer Up Right")]
    [TestCase(1, -1, 0, -35, 35, 0, TestName = "Steer Down Right")]
    [TestCase(-1, 0, 0, 50, 0, 0, TestName = "Steer Left")]
    [TestCase(-1, 1, 0, 35, -35, 0, TestName = "Steer Up Left")]
    [TestCase(-1, -1, 0, 35, 35, 0, TestName = "Steer Down Left")]
    [TestCase(0, 1, 0, 0, -50, 0, TestName = "Steer Up")]
    [TestCase(0, -1, 0, 0, 50, 0, TestName = "Steer Down")]
    [TestCase(0, 0, 1, 0, 0, -50, TestName = "Steer Back")]
    [TestCase(0, 0, -1, 0, 0, 50, TestName = "Steer Front")]
    // XZ plane combinations
    [TestCase(1, 0, 1, -35, 0, -35, TestName = "Steer Right Back")]
    [TestCase(1, 0, -1, -35, 0, 35, TestName = "Steer Right Front")]
    [TestCase(-1, 0, 1, 35, 0, -35, TestName = "Steer Left Back")]
    [TestCase(-1, 0, -1, 35, 0, 35, TestName = "Steer Left Front")]
    // YZ plane combinations
    [TestCase(0, 1, 1, 0, -35, -35, TestName = "Steer Up Back")]
    [TestCase(0, 1, -1, 0, -35, 35, TestName = "Steer Up Front")]
    [TestCase(0, -1, 1, 0, 35, -35, TestName = "Steer Down Back")]
    [TestCase(0, -1, -1, 0, 35, 35, TestName = "Steer Down Front")]
    // Full 3D diagonal combinations
    [TestCase(1, 1, 1, -28, -28, -28, TestName = "Steer Up Right Back")]
    [TestCase(1, 1, -1, -28, -28, 28, TestName = "Steer Up Right Front")]
    [TestCase(1, -1, 1, -28, 28, -28, TestName = "Steer Down Right Back")]
    [TestCase(1, -1, -1, -28, 28, 28, TestName = "Steer Down Right Front")]
    [TestCase(-1, 1, 1, 28, -28, -28, TestName = "Steer Up Left Back")]
    [TestCase(-1, 1, -1, 28, -28, 28, TestName = "Steer Up Left Front")]
    [TestCase(-1, -1, 1, 28, 28, -28, TestName = "Steer Down Left Back")]
    [TestCase(-1, -1, -1, 28, 28, 28, TestName = "Steer Down Left Front")]
    public void Test_Separation_WithTooCloseBoid_SteerAwayFromIt(
            float posX, float posY, float posZ,
            int expectedX, int expectedY, int expectedZ)
    {
        GD.Print("- Next Separation Test");
        var nearbyBoid = new Boid(new Vector3(posX, posY, posZ), Vector3i.Zero, _settings);
        var nearby = new List<Boid> { nearbyBoid };

        // Debug prints
        GD.Print($"Test boid position: {_sut.Position}");
        GD.Print($"Nearby boid position: {nearbyBoid.Position}");
        GD.Print($"Separation radius (scaled): {_settings.Separationradius * QuadTreeConstants.WORLD_TO_QUAD_SCALE}");
        GD.Print($"Distance between boids: {(nearbyBoid.Position - _sut.Position).Length()}");
        GD.Print($"Maxforce (scaled): {_settings.Maxforce * QuadTreeConstants.WORLD_TO_QUAD_SCALE}");

        var result = _sut.Separation(nearby);
        GD.Print($"Separation result: {result}");

        AssertThat(result.x).IsEqual(expectedX);
        AssertThat(result.y).IsEqual(expectedY);
        AssertThat(result.z).IsEqual(expectedZ);
    }

    [TestCase]
    public void Test_IsInFieldOfView_WithStationaryBoid_ReturnsTrue()
    {
        _sut.velocity = Vector3i.Zero;
        var otherPos = new Vector3i(100, 0, 0);

        var result = _sut.IsInFieldOfView(otherPos);
        AssertThat(result).IsTrue();
    }

    [TestCase(100, 0, 0, 50, 0, 20, true, TestName = "inside Cone")]     // Within FOV cone
    [TestCase(100, 0, 0, 0, 0, 100, false, TestName = "outside Cone")]    // Outside FOV cone
    [TestCase(100, 0, 0, -50, 0, 0, false, TestName = "Behind")]    // Behind
    public void Test_IsInFieldOfView_Cases(
        int velX, int velY, int velZ,
        int offsetX, int offsetY, int offsetZ,
        bool expected)
    {
        _sut.velocity = new Vector3i(velX, velY, velZ);
        var otherPos = _sut.Position + new Vector3i(offsetX, offsetY, offsetZ);

        var result = _sut.IsInFieldOfView(otherPos);

        AssertThat(result).IsEqual(expected);
    }
    [TestCase(10.0f, 500, 0, 0, 500, 0, 0, TestName = "Right boundary - no wrap")]    // Right boundary - no wrap
    [TestCase(10.0f, -500, 0, 0, -500, 0, 0, TestName = "Left boundary - no wrap")]   // Left boundary - no wrap
    [TestCase(10.0f, 0, 0, 500, 0, 0, 500, TestName = "Forward boundary - no wrap")]    // Forward boundary - no wrap
    [TestCase(10.0f, 0, 0, -500, 0, 0, -500, TestName = "Back boundary - no wrap")]   // Back boundary - no wrap
    [TestCase(10.0f, 0, 0, 0, 0, 0, 0, TestName = "Center - no wrap")]        // Center - no wrap
    [TestCase(10.0f, 501, 0, 0, -499, 0, 0, TestName = "wrap from right to left")]     // Wrap from right to left
    [TestCase(10.0f, -501, 0, 0, 499, 0, 0, TestName = "wrap from left to right")]     // Wrap from left to right
    [TestCase(10.0f, 0, 0, 501, 0, 0, -499, TestName = "wrap from front to back")]     // Wrap from front to back
    [TestCase(10.0f, 0, 0, -501, 0, 0, 499, TestName = "wrap from back to front")]     // Wrap from back to front
    public void Test_WrapPosition_Cases(
            float worldSize,
            int posX, int posY, int posZ,
            int expectedX, int expectedY, int expectedZ)
    {
        _sut.Position = new Vector3i(posX, posY, posZ);
        int halfSize = (int)(worldSize * QuadTreeConstants.WORLD_TO_QUAD_SCALE / 2);
        GD.Print($"World size: {worldSize}");
        GD.Print($"Half size: {halfSize}");
        GD.Print($"Initial position: {_sut.Position}");

        _sut.WrapPosition(worldSize);
        GD.Print($"Final position: {_sut.Position}");

        AssertThat(_sut.Position.x).IsEqual(expectedX);
        AssertThat(_sut.Position.y).IsEqual(expectedY);
        AssertThat(_sut.Position.z).IsEqual(expectedZ);
    }

    [TestCase(200, 0, 0, 100, 0, 0)]     // Over speed in X
    [TestCase(0, 200, 0, 0, 100, 0)]     // Over speed in Y
    [TestCase(0, 0, 200, 0, 0, 100)]     // Over speed in Z
    public void Test_UpdateMovement_LimitsSpeed_Cases(
        int velX, int velY, int velZ,
        int expectedX, int expectedY, int expectedZ)
    {
        _sut.velocity = new Vector3i(velX, velY, velZ);
        _sut.UpdateMovement();

        AssertThat(_sut.velocity.x).IsEqual(expectedX);
        AssertThat(_sut.velocity.y).IsEqual(expectedY);
        AssertThat(_sut.velocity.z).IsEqual(expectedZ);
    }
}

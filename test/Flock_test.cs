// namespace DefaultNamespace;
//
// using GdUnit4;
// using static GdUnit4.Assertions;
//
// [TestSuite]
// public class FlockTest
// {
//     private Flock? _sut;
//
//
//     [Before]
//     public void Setup()
//     {
//
//         _sut = AutoFree(new Flock());
//         AssertThat(_sut).IsNotNull();
//     }
//
//     [After]
//     public void TearDownSuite()
//     {
//         // Clean up suite-wide resources if needed
//     }
//
//     [BeforeTest]
//     public void SetupTest()
//     {
//         // Set up resources needed for each individual test
//     }
//
//     [AfterTest]
//     public void TearDownTest()
//     {
//         // Clean up after each individual test
//     }
//
//     [TestCase]
//     public void Test__Ready()
//     {
//         // Arrange
//         AssertThat(_sut).IsNotNull();
//
//         // Act
//         _sut._Ready();
//
//         // Assert
//         // TODO: Add specific assertions for _Ready
//     }
//
//     [TestCase]
//     public void Test__Process()
//     {
//         // Arrange
//         AssertThat(_sut).IsNotNull();
//
//         // Act
//         _sut._Process(0.0);
//
//         // Assert
//         // TODO: Add specific assertions for _Process
//     }
//
//     [TestCase]
//     public void Test_UpdateQuadTree()
//     {
//         // Arrange
//         AssertThat(_sut).IsNotNull();
//
//         // Act
//         _sut.UpdateQuadTree();
//
//         // Assert
//         // TODO: Add specific assertions for UpdateQuadTree
//     }
// }

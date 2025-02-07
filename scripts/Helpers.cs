using Godot;
public static class Helpers{
    public static void CreateCameraToViewBounds(Vector2 bounds,Node3D parent)
    {
        Camera3D camera = new Camera3D();
        parent.AddChild(camera);

        // Position camera above the world to see everything
        // Height based on FOV and world size to ensure full visibility
        float fov = 75.0f; // Camera's field of view in degrees
        float height = Mathf.Max(bounds.X, bounds.Y) * 1.2f; // Add 20% margin
        camera.Position = new Vector3(0, height, 0);
        camera.RotationDegrees = new Vector3(-90, 0, 0); // Look straight down
    }
    public static QuadTree CreateQuadTree(Vector2 bounds)
    {
        // Calculate bounds based on current world size
        // We multiply by 1000 since we're using integer space for the quadtree
        int halfWidth = (int)(bounds.X * 1000);
        int halfHeight = (int)(bounds.Y * 1000);

        // Create boundary centered at 0,0 that encompasses our world
        Rectangle boundary = new Rectangle(0, 0, halfWidth, halfHeight);
        return new QuadTree(boundary, 4);
    }
	public static MeshInstance3D CreatePrismMeshAsChild(Node3D parent,Vector3 size){
        // Create mesh
        MeshInstance3D meshInstance = new MeshInstance3D();
        parent.AddChild(meshInstance);
        
        // Create a simple cone/arrow shape for the boid
        var mesh = new PrismMesh();
        mesh.Size = size; // Width, Height, Depth
        meshInstance.Mesh = mesh;
		return meshInstance;
	}

}

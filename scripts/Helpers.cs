using Godot;
public static class Helpers{

    public static void CreateCameraToViewBounds(float bounds,Node3D parent)
    {
        Camera3D camera = new Camera3D();
        parent.AddChild(camera);

        // Position camera above the world to see everything
        // Height based on FOV and world size to ensure full visibility
        float fov = 75.0f; // Camera's field of view in degrees
		camera.Fov = fov;
        camera.Position = new Vector3(0,bounds, 0) * fov * 0.01f;
        camera.RotationDegrees = new Vector3(-90, 0, 0); // Look straight down
    }
	///<Summary>
	/// create a QuadTree with bounds
	/// <Summary>
    public static QuadTree CreateQuadTree(float size)
    {
        // Calculate bounds based on current world size
        // We multiply by QuadTreeConstants.WORLD_TO_QUAD_SCALE since we're using integer space for the quadtree
        int halfSize = (int)(size * QuadTreeConstants.WORLD_TO_QUAD_SCALE);

        // Create boundary centered at 0,0 that encompasses our world
        Square boundary = new Square(0, 0, halfSize);
        return new QuadTree(boundary, 4);
    }
	///<Summary>
	/// create a cone as a mesh of parent
	/// <Summary>
	public static MeshInstance3D CreateConeMeshAsChild(Node3D parent,float topRadius, float bottomRadius, float height){
        // Create mesh
        MeshInstance3D meshInstance = new MeshInstance3D();
        parent.AddChild(meshInstance);
        
        // Create a simple cone/arrow shape for the boid
        CylinderMesh mesh = new CylinderMesh();
		mesh.BottomRadius = bottomRadius;
		mesh.TopRadius = topRadius;
		mesh.Height = height;
        meshInstance.Mesh = mesh;
		return meshInstance;
	}
	///<Summary>
	/// Create a prism as child of parent with size.
	/// <Summary>
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

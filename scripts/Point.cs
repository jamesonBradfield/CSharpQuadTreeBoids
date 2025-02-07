using Godot;
public class Point
{
    private int id;
    private int x;
    private int y;

    public Point(int id, float x, float y)
    {
        this.id = id;
        this.x = (int)(x * QuadTreeConstants.WORLD_TO_QUAD_SCALE);
        this.y = (int)(y * QuadTreeConstants.WORLD_TO_QUAD_SCALE);
    }

    // Add method to update position with scaling
    public void UpdatePosition(float worldX, float worldZ)
    {
        this.x = (int)(worldX * QuadTreeConstants.WORLD_TO_QUAD_SCALE);
        this.y = (int)(worldZ * QuadTreeConstants.WORLD_TO_QUAD_SCALE);
    }

	public Vector3 GetPosition(){
		return new Vector3(x,0,y);
	}

    public int GetID() => id;
    public int GetX() => x;
    public int GetY() => y;
}

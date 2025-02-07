public struct Point
{
	private int x;
	private int y;
	private int ID;
	public Point(int ID,int x, int y){
		this.x = x;
		this.y = y;
		this.ID = ID;
	}
	public void UpdatePosition(int newX, int newY) {
		this.x = newX;
		this.y = newY;
	}
	public int GetID(){
		return ID;
	}
	public int GetX(){
		return x;
	}
	public int GetY(){
		return y;
	}
}


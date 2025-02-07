public struct Rectangle {
	private int x;
	private int y;
	private int s;

	public Rectangle(int x, int y, int s){
		this.x = x;
		this.y = y;
		// we are going to only use one int for side-length
		this.s = s;
	}

	public bool contains(Point point){
		return(point.GetX() >= x - s &&
				point.GetX() <= x + s &&
				point.GetY() >= y - s &&
				point.GetY() <= y + s);
	}

	public bool intersects(Rectangle range){
		return !(range.x - range.s > this.x + this.s ||
		   		 range.x + this.s < this.x - this.s ||
				 range.y - range.s > this.y + this.s ||
				 range.y + this.s < this.y - this.s);
	}

	public int GetX(){
		return this.x;
	}
	public int GetY(){
		return this.y;
	}
	public int GetS(){
		return this.s;
	}
}

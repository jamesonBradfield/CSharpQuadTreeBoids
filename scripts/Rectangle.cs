public struct Rectangle {
	private int x;
	private int y;
	private int w;
	private int h;

	public Rectangle(int x, int y, int w, int h){
		this.x = x;
		this.y = y;
		this.w = w;
		this.h = h;
	}

	public bool contains(Point point){
		return(point.GetX() >= x - w &&
				point.GetX() <= x + w &&
				point.GetY() >= y - h &&
				point.GetY() <= y + h);
	}

	public bool intersects(Rectangle range){
		return !(range.x - range.w > this.x + this.w ||
		   		 range.x + this.w < this.x - this.w ||
				 range.y - range.h > this.y + this.h ||
				 range.y + this.h < this.y - this.h);
	}

	public int GetX(){
		return this.x;
	}
	public int GetY(){
		return this.y;
	}
	public int GetW(){
		return this.w;
	}
	public int GetH(){
		return this.h;
	}
}

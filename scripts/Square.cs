using System;
public struct Square {
    private int x;
    private int y;
    private int s;  // half-width of the square

    public Square(int x, int y, int s) {
        this.x = x;
        this.y = y;
        this.s = s;
    }

    public bool contains(Point point) {
        // Since s is already our half-width, we can compare directly
        int dx = point.GetX() - x;
        int dy = point.GetY() - y;
        return Math.Abs(dx) <= s && Math.Abs(dy) <= s;
    }

    public bool intersects(Square range) {
        // For two squares to intersect, the distance between their centers
        // must be less than the sum of their half-widths in both x and y
        int dx = Math.Abs(range.x - this.x);
        int dy = Math.Abs(range.y - this.y);
        return dx <= (range.s + this.s) && 
               dy <= (range.s + this.s);
    }

    // Getters can stay the same
    public int GetX() => x;
    public int GetY() => y;
    public int GetS() => s;
}

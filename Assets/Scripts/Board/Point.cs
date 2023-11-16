public class Point
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }

    // Constructors
    public Point()
    {
        X = 0;
        Y = 0;
        Z = 0;
    }

    public Point(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    // Optional: Override ToString() for easy debugging or display
    public override string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }
}

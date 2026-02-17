namespace PrimeCross;

public partial class FormMain : Form
{
    public FormMain()
    {
        InitializeComponent();
    }
    public enum Direction : int
    {
        Down = 0,
        Right,
        Up,
        Left
    }
    private static Point LeftOf(Point p, Direction d) => d switch
    {
        Direction.Down => new Point(p.X + 1, p.Y),
        Direction.Right => new Point(p.X, p.Y - 1),
        Direction.Up => new Point(p.X - 1, p.Y),
        Direction.Left => new Point(p.X, p.Y - 1),
        _ => p,
    };
    private static Point RightOf(Point p, Direction d) => d switch
    {
        Direction.Down => new Point(p.X - 1, p.Y),
        Direction.Right => new Point(p.X, p.Y + 1),
        Direction.Up => new Point(p.X + 1, p.Y),
        Direction.Left => new Point(p.X, p.Y - 1),
        _ => p,
    };

    private static Point ForwardOf(Point p, Direction d) => d switch
    {
        Direction.Down => new Point(p.X, p.Y + 1),
        Direction.Right => new Point(p.X + 1, p.Y),
        Direction.Up => new Point(p.X, p.Y - 1),
        Direction.Left => new Point(p.X - 1, p.Y),
        _ => p,
    };
    private static bool IsPrime(int n)
    {
        if (n < 2) return false;
        if (n == 2) return true;
        if (n % 2 == 0) return false;
        for (int i = 3; i <= Math.Sqrt(n); i += 2)
        {
            if (n % i == 0) return false;
        }
        return true;
    }
    private readonly Color Black = Color.Black;
    private readonly Color White = Color.White;
    private readonly Color Red = Color.Red;
    private readonly Color Green = Color.Green;
    private readonly Color Blue = Color.Blue;

    private void GenerateButton_Click(object sender, EventArgs e)
    {
        using var bitmap = new Bitmap(PrimesPictureBox.Width, PrimesPictureBox.Height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Black);
        Point center = new(bitmap.Width >> 1, bitmap.Height >> 1);
        bitmap.SetPixel(center.X, center.Y, White);
        Point dp = new();
        var direction = Direction.Down;
        var n = 0;
        //skip 0
        var cp = new Size(center.X, center.Y);
        Point p = new(center.X - 1, center.Y - 1);
        //skip 1
        n++;
        p.Y++;

        while (n <= PrimesPictureBox.Width * PrimesPictureBox.Height)
        {
            dp = Point.Subtract(
                p = ForwardOf(p, direction), 
                cp);
            if (p.X < 0 || p.X > PrimesPictureBox.Width - 1
                || p.Y < 0 || p.Y > PrimesPictureBox.Height - 1)
                break;

            if (IsPrime(n))
            {
                bitmap.SetPixel(p.X, p.Y, White);
            }

            n++;
            var l = LeftOf(p, direction);
            if (l.X < 0 || l.X > PrimesPictureBox.Width - 1
                || l.Y < 0 || l.Y > PrimesPictureBox.Height - 1)
                break;
            var lc = bitmap.GetPixel(l.X, l.Y);
            dp = Point.Subtract(l, cp);
            if (lc.ToArgb() == Black.ToArgb())
            {
                direction = (Direction)(((int)direction + 1) % 4);
            }

            var pc = bitmap.GetPixel(p.X, p.Y);
            if(pc.ToArgb()!= Black.ToArgb())
            {
                var rp = RightOf(p,direction);
                if (rp.X < 0 || rp.X > PrimesPictureBox.Width - 1
                    || rp.Y < 0 || rp.Y > PrimesPictureBox.Height - 1)
                    break;
                //p = rp;
            }
        }
        bitmap.SetPixel(center.X, center.Y, Black);
        this.PrimesPictureBox.Image = bitmap.Clone() as Bitmap;
    }

    private void FormMain_Resize(object sender, EventArgs e)
    {
        this.GenerateButton_Click(sender, e);
    }
}

using System.Diagnostics;

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

    private static Direction Turn(Direction d, bool left = true) => left
            ? (Direction)(((int)d + 1) % 4)
            : (d == Direction.Down) ? Direction.Left : ((Direction)((int)d) - 1)
        ;
    private static Point LeftOf(Point p, Direction d) => d switch
    {
        Direction.Down => new Point(p.X + 1, p.Y),
        Direction.Right => new Point(p.X, p.Y - 1),
        Direction.Up => new Point(p.X - 1, p.Y),
        Direction.Left => new Point(p.X, p.Y + 1),
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
    private static bool IsPrime(long n)
    {
        if (n < 2) return false;
        if (n == 2) return true;
        if (n % 2 == 0) return false;
        for (long i = 3; i <= Math.Sqrt(n); i += 2)
        {
            if (n % i == 0) return false;
        }
        return true;
    }
    private const uint Black = 0x000000;
    private const uint White = 0xffffff;
    private const uint Red = 0x0000ff;
    private const uint Green = 0x00ff00;
    private const uint Blue = 0xff0000;

    private void GenerateButton_Click(object sender, EventArgs e)
    {
        try
        {
            int length = Math.Max(PrimesPictureBox.Width, PrimesPictureBox.Height);
            var map = new uint[length, length];

            Point center = new(length >> 1, length >> 1);
            Point dp = new();
            var direction = Direction.Down;
            //skip 0
            var cp = new Size(center.X, center.Y);
            long n = 1;
            map[center.X, center.Y] = White;
            //set to -1,-1
            Point p = new(center.X - 1, center.Y - 1);
            //skip 1
            map[p.X, p.Y] = Red;
            //bitmap.SetPixel(p.X, p.Y, Red);
            dp = Point.Subtract(p, cp);
            //Debug.WriteLine($"-({dp.X,4},{dp.Y,4})={n,4},DIRECTION:{direction}");
            n++;
            p.Y++;
            map[p.X, p.Y] = White;
            dp = Point.Subtract(p, cp);
            //Debug.WriteLine($"+({dp.X,4},{dp.Y,4})={n,4},DIRECTION:{direction}");

            while (n <= length * length)
            {
                n++;
                dp = Point.Subtract(
                    p = ForwardOf(p, direction),
                    cp);
                if (p.X < 0 || p.X >= length - 1
                    || p.Y < 0 || p.Y >= length - 1)
                    break;

                if (IsPrime(n))
                {
                    map[p.X, p.Y] = White;
                    //Debug.WriteLine($"+({dp.X,4},{dp.Y,4})={n,4},DIRECTION:{direction}");
                }
                else
                {
                    map[p.X, p.Y] = Red;
                    //Debug.WriteLine($"-({dp.X,4},{dp.Y,4})={n,4},DIRECTION:{direction}");
                }

                var l = LeftOf(p, direction);
                if (l.X < 0 || l.X >= length - 1
                    || l.Y < 0 || l.Y >= length - 1)
                    break;
                var lc = map[l.X, l.Y];
                if (lc == Black)
                {
                    direction = Turn(direction);
                    var xp = Point.Subtract(p, cp);
                    //Debug.WriteLine($"*({dp.X,4},{dp.Y,4})={n,4},TURN:{direction}");
                }
                else
                {
                    var pt = ForwardOf(p, direction);
                    if (pt.X < 0 || pt.X >= length - 1
                        || pt.Y < 0 || pt.Y >= length - 1)
                    {
                        continue;
                    }

                    var px = map[pt.X, pt.Y];

                    if (px != Black)
                    {
                        var rp = RightOf(p, direction);
                        if (rp.X < 0 || rp.X >= length - 1
                            || rp.Y < 0 || rp.Y >= length - 1)
                            break;
                        p = rp;
                        dp = Point.Subtract(p, cp);
                        //Debug.WriteLine($"*({dp.X,4},{dp.Y,4})={n,4},TURN:{direction}");
                    }
                }

            }
            for (int y = 0; y < length; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    var c = map[x, y];
                    if (c == Red)
                    {
                        map[x, y] = Black;
                    }
                }
            }


            var bitmap = new Bitmap(PrimesPictureBox.Width, PrimesPictureBox.Height);

            for (int y = 0; y < PrimesPictureBox.Height; y++)
            {
                for (int x = 0; x < PrimesPictureBox.Width; x++)
                {
                    var px = x + ((length - PrimesPictureBox.Width) >> 1);
                    var py = y + ((length - PrimesPictureBox.Height) >> 1);
                    var c = map[px, py] | 0xff000000;

                    bitmap.SetPixel(x, y, Color.FromArgb((int)c));
                }
            }

            this.PrimesPictureBox.Image?.Dispose();
            this.PrimesPictureBox.Image = bitmap;

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void FormMain_Resize(object sender, EventArgs e)
    {
        this.GenerateButton_Click(sender, e);
    }

    private void FormMain_Load(object sender, EventArgs e)
    {
        this.GenerateButton_Click(sender, e);
    }
}

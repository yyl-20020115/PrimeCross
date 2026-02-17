using System.Diagnostics;

namespace PrimeCross;

public partial class FormMain : Form
{
    public FormMain()
    {
        InitializeComponent();
    }
    enum Direction : int
    {
        Down = 0,
        Right,
        Up,
        Left
    }

    Direction Turn(Direction d, bool left = true) => left
            ? (Direction)(((int)d + 1) % 4)
            : (d == Direction.Down) ? Direction.Left : ((Direction)((int)d) - 1)
        ;
    Point LeftOf(Point p, Direction d) => d switch
    {
        Direction.Down => new Point(p.X + 1, p.Y),
        Direction.Right => new Point(p.X, p.Y - 1),
        Direction.Up => new Point(p.X - 1, p.Y),
        Direction.Left => new Point(p.X, p.Y + 1),
        _ => p,
    };
    Point RightOf(Point p, Direction d) => d switch
    {
        Direction.Down => new Point(p.X - 1, p.Y),
        Direction.Right => new Point(p.X, p.Y + 1),
        Direction.Up => new Point(p.X + 1, p.Y),
        Direction.Left => new Point(p.X, p.Y - 1),
        _ => p,
    };

    Point ForwardOf(Point p, Direction d) => d switch
    {
        Direction.Down => new Point(p.X, p.Y + 1),
        Direction.Right => new Point(p.X + 1, p.Y),
        Direction.Up => new Point(p.X, p.Y - 1),
        Direction.Left => new Point(p.X - 1, p.Y),
        _ => p,
    };
    bool IsPrime(long n)
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
    const int Black = 0x000000;
    const int White = 0xffffff;
    const int Red = 0x0000ff;
    const int Green = 0x00ff00;
    const int Blue = 0xff0000;
    int map_length = 0;
    (int, long, bool)[,]? primes = null;
    (int, long, bool)[,] BuildPrimesMap(int length)
    {
        var map = new (int, long, bool)[length, length];
        Point center = new(length >> 1, length >> 1);
        var direction = Direction.Up;
        long n = 0;
        map[center.X, center.Y] = (White, n++, false);
        Point p = new(center.X + 1, center.Y + 1);
        map[p.X, p.Y] = (Red, n++, false);
        p.Y--;
        map[p.X, p.Y] = (White, n++, true);
        while (n <= length * length)
        {
            n++;
            p = ForwardOf(p, direction);
            if (p.X < 0 || p.X >= length - 1
                || p.Y < 0 || p.Y >= length - 1)
                break;
            var b = IsPrime(n);
            map[p.X, p.Y] = (b ? White : Red, n, b);

            var l = LeftOf(p, direction);
            if (l.X < 0 || l.X >= length - 1
                || l.Y < 0 || l.Y >= length - 1)
                break;
            var lc = map[l.X, l.Y];
            if (lc.Item1 == Black)
            {
                direction = Turn(direction);
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

                if (px.Item1 != Black)
                {
                    var rp = RightOf(p, direction);
                    if (rp.X < 0 || rp.X >= length - 1
                        || rp.Y < 0 || rp.Y >= length - 1)
                        break;
                    p = rp;
                }
            }
        }
        for (int y = 0; y < length; y++)
        {
            for (int x = 0; x < length; x++)
            {
                map[x, y] = map[x, y].Item1 == Red ? (Black, map[x, y].Item2, map[x, y].Item3) : map[x, y];
            }
        }
        return map;
    }
    private void GenerateButton_Click(object sender, EventArgs e)
    {
        this.map_length = Math.Max(PrimesPictureBox.Width, PrimesPictureBox.Height);
        this.primes = BuildPrimesMap(this.map_length);

        var bitmap = new Bitmap(PrimesPictureBox.Width, PrimesPictureBox.Height);

        for (int y = 0; y < PrimesPictureBox.Height; y++)
        {
            for (int x = 0; x < PrimesPictureBox.Width; x++)
            {
                var px = x + ((this.map_length - PrimesPictureBox.Width) >> 1);
                var py = y + ((this.map_length - PrimesPictureBox.Height) >> 1);
                var c = this.primes[px, py];
                bitmap.SetPixel(x, y, c.Item1 == 0 ? Color.Black : Color.White);
            }
        }

        this.PrimesPictureBox.Image?.Dispose();
        this.PrimesPictureBox.Image = bitmap;

    }

    private void FormMain_Resize(object sender, EventArgs e)
    {
        this.GenerateButton_Click(sender, e);
    }

    private void FormMain_Load(object sender, EventArgs e)
    {
        this.GenerateButton_Click(sender, e);
    }

    private void PrimesPictureBox_MouseMove(object sender, MouseEventArgs e)
    {
        if (this.primes != null && PrimesPictureBox.Image != null)
        {
            var x = e.X + ((PrimesPictureBox.Image.Width - PrimesPictureBox.Width) >> 1);
            var y = e.Y + ((PrimesPictureBox.Image.Height - PrimesPictureBox.Height) >> 1);

            if (x >= 0 && x < PrimesPictureBox.Image.Width
                && y >= 0 && y < PrimesPictureBox.Image.Height)
            {
                Point cp = new(PrimesPictureBox.Image.Width >> 1, PrimesPictureBox.Image.Height >> 1);
                Point dp = new(x - cp.X, cp.Y - y);

                Point mp = new(dp.X + (this.map_length >> 1), dp.Y + (this.map_length >> 1));

                long n = this.primes![mp.X, mp.Y].Item2;
                bool b = this.primes[mp.X, mp.Y].Item3;
                var t = Math.Atan2(dp.Y, dp.X) / Math.PI * 180.0;
                this.InfoLabel.Text = $"n={n}, x={dp.X}, y={dp.Y}, d={t:N4}°: {(b ? "Prime" : "Composite")}";
            }
        }
    }
}

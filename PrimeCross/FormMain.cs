using System.Drawing.Imaging;
using System.Numerics;

namespace PrimeCross;

public partial class FormMain : Form
{
    public static Bitmap GetBitmap(int[,] pixels)
    {
        var width = pixels.GetLength(0);
        var height = pixels.GetLength(1);
        var bitmap = new Bitmap(width, height);
        using var g = Graphics.FromImage(bitmap);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bitmap.SetPixel(x, y, Color.FromArgb(
                    pixels[x, y] | unchecked((int)0xff000000)));
            }
        }
        return bitmap;
    }
    public static int[,] GetPixels(Bitmap bitmap)
    {
        var width = bitmap.Width;
        var height = bitmap.Height;
        var pixels = new int[width, height];
        var rect = new Rectangle(0, 0, width, height);

        var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
        var bpp = Image.GetPixelFormatSize(bitmap.PixelFormat) >> 3;
        var bytes = Math.Abs(data.Stride) * height;
        var buffer = new byte[bytes];

        System.Runtime.InteropServices.Marshal.Copy(data.Scan0, buffer, 0, bytes);

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var i = y * data.Stride + x * bpp;
                pixels[x, y]
                    = (buffer[i + 0] << 00)
                    | (buffer[i + 1] << 08)
                    | (buffer[i + 2] << 16)
                    | (buffer[i + 3] << 24)
                    ;
            }
        }

        bitmap.UnlockBits(data);

        return pixels;
    }
    public static void FFT(Complex[] t, Complex[] f, int r)
    {
        var count = 1L << r;
        int i, j, k, p, bsize;

        var W = new Complex[1L << (r - 1)];
        var X1 = new Complex[count];
        var X2 = new Complex[count];
        for (i = 0; i < W.Length; i++)
        {
            var angle = i * Math.PI * 2.0 / count;
            W[i] = new(Math.Cos(angle), -Math.Sin(angle));
        }

        t.CopyTo(X1, 0);

        for (k = 0; k < r; k++)
        {
            for (j = 0; j < 1 << k; j++)
            {
                bsize = 1 << (r - k);
                for (i = 0; i < (bsize >> 1); i++)
                {
                    p = j * bsize;
                    X2[i + p]
                        = X1[i + p] + X1[i + p + (bsize >> 1)]
                        ;
                    X2[i + p + (bsize >> 1)]
                        = (X1[i + p] - X1[i + p + (bsize >> 1)])
                        * W[i * (1 << k)]
                        ;
                }
            }
            (X1, X2) = (X2, X1);
        }

        for (j = 0; j < count; j++)
        {
            p = 0;
            for (i = 0; i < r; i++)
            {
                if ((j & (1 << i)) != 0)
                {
                    p += 1 << (r - i - 1);
                }
            }
            f[j] = X1[p];
        }
    }
    const int _31 = (sizeof(int) << 3) - 1;
    public static Bitmap? Fourier(Bitmap bitmap)
    {
        var width = bitmap.Width;
        var height = bitmap.Height;
        if (width == 0 || height == 0)
            return bitmap.Clone() as Bitmap;
        var lw = 1L;
        var lh = 1L;
        var wp = 0;
        var hp = 0;
        long i, j;
        long n, m;
        var data = GetPixels(bitmap);
        lw = 1L << (wp = _31 - int.LeadingZeroCount(width));
        lh = 1L << (hp = _31 - int.LeadingZeroCount(height));
        var t = new Complex[lw * lh];
        var f = new Complex[lw * lh];
        var tw = new Complex[lw];
        var th = new Complex[lw];
        for (i = 0; i < lh; i++)
        {
            for (j = 0; j < lw; j++)
            {
                t[i * lw + j] = new(data[j, i] == 0 ? 0 : 0xffffff, 0.0);
            }
        }
        for (i = 0; i < lh; i++)
        {
            Array.Copy(t, i * lw, tw, 0, lw);
            Array.Copy(f, i * lw, th, 0, lw);
            FFT(tw, th, wp);
            Array.Copy(th, 0, f, i * lw, lw);
        }

        for (i = 0; i < lh; i++)
        {
            for (j = 0; j < lw; j++)
            {
                t[j * lh + i] = f[i * lw + j];
            }
        }

        var ow = new Complex[lh];
        var oh = new Complex[lh];
        for (i = 0; i < lw; i++)
        {
            Array.Copy(t, i * lh, ow, 0, lh);
            Array.Copy(f, i * lh, oh, 0, lh);
            FFT(ow, oh, hp);
            oh.CopyTo(f, i * lh);
        }

        var max = f.Max(c => (c.Magnitude));
        for (i = 0; i < lh; i++)
        {
            for (j = 0; j < lw; j++)
            {
                var val = f[j * lh + i].Magnitude;
                var kt = (byte)((val / max) * 255.0);
                n = ((height - lh) >> 1) + (i < (lh >> 1) ? i + (lh >> 1) : i - (lh >> 1));
                m = ((width - lw) >> 1) + (j < (lw >> 1) ? j + (lw >> 1) : j - (lw >> 1));
                data[m, n] = Color.FromArgb(
                    kt,
                    kt,
                    kt)
                    .ToArgb();
            }
        }

        return GetBitmap(data);
    }
    public FormMain()
    {
        InitializeComponent();
    }
    private enum Direction : int
    {
        Down = 0,
        Right,
        Up,
        Left
    }

    private static Direction Turn(Direction d, bool left = true) => left
        ? (Direction)(((int)d + 1) % 4)
        : (d == Direction.Down) ? Direction.Left : d - 1
        ;
    private static Point LeftOf(Point p, Direction d) => d switch
    {
        Direction.Down => new(p.X + 1, p.Y),
        Direction.Right => new(p.X, p.Y - 1),
        Direction.Up => new(p.X - 1, p.Y),
        Direction.Left => new(p.X, p.Y + 1),
        _ => p,
    };
    private static Point RightOf(Point p, Direction d) => d switch
    {
        Direction.Down => new(p.X - 1, p.Y),
        Direction.Right => new(p.X, p.Y + 1),
        Direction.Up => new(p.X + 1, p.Y),
        Direction.Left => new(p.X, p.Y - 1),
        _ => p,
    };

    private static Point ForwardOf(Point p, Direction d) => d switch
    {
        Direction.Down => new(p.X, p.Y + 1),
        Direction.Right => new(p.X + 1, p.Y),
        Direction.Up => new(p.X, p.Y - 1),
        Direction.Left => new(p.X - 1, p.Y),
        _ => p,
    };
    private static bool IsPrime(long n)
    {
        switch (n)
        {
            case < 2:
                return false;
            case 2:
                return true;
            default:
                if (n % 2 == 0) 
                    return false;
                var sqrt = (long)(Math.Sqrt(n) + 0.5);
                for (var i = 3L; i <= sqrt; i += 2)
                    if (n % i == 0) 
                        return false;
                return true;
        }
    }
    const int Black = 0x000000;
    const int White = 0xffffff;
    const int Red = 0x0000ff;
    bool flip = false;
    bool inverse = false;
    bool rotate = false;
    int length = 0;
    (int, long, bool)[,]? primes = null;
    private static (int, long, bool)[,] BuildPrimesMap(int length)
    {
        var map = new (int, long, bool)[length, length];
        var center = new Point(length >> 1, length >> 1);
        var direction = Direction.Up;
        var n = 0L;
        map[center.X, center.Y] = (White, n++, false);
        var p = new Point(center.X + 1, center.Y + 1);
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
            (int color, _, _) = map[l.X, l.Y];
            if (color == Black)
            {
                direction = Turn(direction);
            }
            else
            {
                var pt = ForwardOf(p, direction);
                if (pt.X < 0 || pt.X >= length - 1
                    || pt.Y < 0 || pt.Y >= length - 1)
                    continue;
                (int color, long, bool) px = map[pt.X, pt.Y];
                if (px.color != Black)
                {
                    var rp = RightOf(p, direction);
                    if (rp.X < 0 || rp.X >= length - 1
                        || rp.Y < 0 || rp.Y >= length - 1)
                        break;
                    p = rp;
                }
            }
        }
        return map;
    }

    private static Point FlipProject(Size size, Point p, bool flip = false, bool rotate = false)
    {
        var cp = new Point(size.Width >> 1, size.Height >> 1);
        if (!flip)
        {
            return rotate ? new(p.Y, p.X) : p;
        }
        else
        {
            var x = p.X;
            var y = p.Y;
            x = x < cp.X ? cp.X - x : size.Width + (cp.X - x) - 1;
            y = y < cp.Y ? cp.Y - y : size.Height + (cp.Y - y) - 1;
            return rotate ? new(y, x) : new(x, y);
        }
    }
    private void GeneratePrimesMap(bool flip = false, bool inverse = false, bool rotate = false)
    {
        this.primes = BuildPrimesMap(
            this.length = Math.Max(
                PrimesPictureBox.Width,
                PrimesPictureBox.Height)
            );

        var bitmap = new Bitmap(PrimesPictureBox.Width, PrimesPictureBox.Height);
        var size = new Size(PrimesPictureBox.Width, PrimesPictureBox.Height);
        for (int y = 0; y < size.Height; y++)
        {
            for (int x = 0; x < size.Width; x++)
            {
                var px = x + ((this.length - PrimesPictureBox.Width) >> 1);
                var py = y + ((this.length - PrimesPictureBox.Height) >> 1);
                (_, _, var isprime) = this.primes[px, py];
                var fp = FlipProject(size, new Point(x, y), flip, rotate);
                bitmap.SetPixel(fp.X, fp.Y,
                    ((!inverse) ? isprime : !isprime) ? Color.White : Color.Black);
            }
        }

        this.PrimesPictureBox.Image?.Dispose();
        this.PrimesPictureBox.Image = bitmap;
    }
    private void Reset()
    {
        this.GeneratePrimesMap(this.flip = false, this.inverse = false, this.rotate = false);
    }
    private void Flip()
    {
        this.GeneratePrimesMap(this.flip = !this.flip, this.inverse, this.rotate);
    }
    private void Inverse()
    {
        this.GeneratePrimesMap(this.flip, this.inverse = !this.inverse, this.rotate);
    }
    private void Rotate()
    {
        this.GeneratePrimesMap(this.flip, this.inverse, this.rotate = !this.rotate);
    }
    private void GenerateButton_Click(object sender, EventArgs e)
    {
        if (this.PrimesPictureBox.Image is Bitmap bitmap
            && bitmap.Clone() is Bitmap clone)
        {
            bitmap.Dispose();
            this.PrimesPictureBox.Image = null;
            this.PrimesPictureBox.Image = Fourier(clone);
            clone.Dispose();
        }
    }

    private void FormMain_Resize(object sender, EventArgs e)
    {
        this.GeneratePrimesMap(this.flip, this.inverse, this.rotate);
    }

    private void FormMain_Load(object sender, EventArgs e)
    {
        this.GeneratePrimesMap();
    }

    private void PrimesPictureBox_MouseMove(object sender, MouseEventArgs e)
    {
        if (this.primes != null && PrimesPictureBox.Image != null)
        {
            var x = e.X;
            var y = e.Y;
            if (x >= 0 && x < PrimesPictureBox.Image.Width && y >= 0 && y < PrimesPictureBox.Image.Height)
            {
                var cp = new Point(PrimesPictureBox.Image.Width >> 1, PrimesPictureBox.Image.Height >> 1);
                var dp = new Point(x - cp.X, cp.Y - y);
                var mp = new Point(dp.X + (this.length >> 1), dp.Y + (this.length >> 1) - 1);
                if (mp.X > this.length - 1)
                    mp.X = this.length - 1;
                if (mp.Y > this.length - 1)
                    mp.Y = this.length - 1;
                if (mp.X < 0)
                    mp.X = 0;
                if (mp.Y < 0)
                    mp.Y = 0;
                (_, var n, var isprime) = this.primes[mp.X, mp.Y];
                var t = Math.Atan2(dp.Y, dp.X) / Math.PI * 180.0;
                this.InfoLabel.Text = $"{(isprime ? 'P' : 'C')}:{n} ({dp.X},{dp.Y},{t:N2}°)";
            }
        }
    }

    private void ResetButton_Click(object sender, EventArgs e)
    {
        this.Reset();
    }

    private void FlipButton_Click(object sender, EventArgs e)
    {
        this.Flip();
    }

    private void InverseButton_Click(object sender, EventArgs e)
    {
        this.Inverse();
    }

    private void RotateButton_Click(object sender, EventArgs e)
    {
        this.Rotate();
    }
}

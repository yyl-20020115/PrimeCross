using System.Drawing.Imaging;
using System.Numerics;

namespace PrimeCross;

public partial class FormMain : Form
{
    public static Bitmap GetBitmap(int[][] pixelArray)
    {
        var height = pixelArray.GetLength(0);
        var width = pixelArray[0].GetLength(0);
        var bitmap = new Bitmap(width, height);
        using var g = Graphics.FromImage(bitmap);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int pixelValue = pixelArray[y][x];
                pixelValue |= unchecked((int)0xff000000);
                var c = Color.FromArgb(pixelValue);
                bitmap.SetPixel(x, y, c);
            }
        }
        return bitmap;
    }
    public static int[][] GetPixelArray(Bitmap bitmap)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;
        int[][] pixelArray = new int[height][];
        for (int i = 0; i < height; i++)
        {
            pixelArray[i] = new int[width];
        }
        var rect = new Rectangle(0, 0, width, height);
        var data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

        int bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
        int bytes = Math.Abs(data.Stride) * height;
        var rgbValues = new byte[bytes];

        System.Runtime.InteropServices.Marshal.Copy(data.Scan0, rgbValues, 0, bytes);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = y * data.Stride + x * bytesPerPixel; // 计算当前像素的索引位置
                var v = rgbValues[i] | (rgbValues[i + 1] << 8) | (rgbValues[i + 2] << 16) | (rgbValues[i + 3] << 24);
                pixelArray[y][x] = v; // 对于24位图像，直接组合RGB值
            }
        }

        bitmap.UnlockBits(data);

        return pixelArray;
    }
    public static void FFT(Complex[] t, Complex[] f, int r) 
        // t为时域，f为频域 r为2的幂数
    {
        long count;
        int i, j, k, p, bsize;
        Complex[] W;
        Complex[] X1;
        Complex[] X2;
        //Complex[] X;
        Complex comp;
        double angle;  // 计算加权时所需角度
        count = 1 << r;

        W = new Complex[count / 2];
        X1 = new Complex[count];
        X2 = new Complex[count];
        //X = new Complex[count];
        for (i = 0; i < count / 2; i++)
        {
            angle = i * Math.PI * 2 / count;
            W[i] = new (Math.Cos(angle),-Math.Sin(angle));
        }

        t.CopyTo(X1, 0);

        for (k = 0; k < r; k++)
        {
            for (j = 0; j < 1 << k; j++)
            {
                bsize = 1 << (r - k);
                for (i = 0; i < bsize / 2; i++)
                {
                    p = j * bsize;
                    X2[i + p] = X1[i + p] + X1[i + p + bsize / 2];
                    comp = X1[i + p] - X1[i + p + bsize / 2];
                    X2[i + p + bsize / 2] = comp * W[i * (1 << k)];
                }
            }
            //X = X1;
            //X1 = X2;
            //X2 = X;
            (X1,X2) = (X2, X1);
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
    public static Bitmap Fourier(Bitmap bitmap)
    {
        // 原图像的宽与高
        int w = bitmap.Width;
        int h = bitmap.Height;
        // 傅立叶变换的实际宽高
        long lw = 1;
        long lh = 1;
        // 迭代次数
        int wp = 0; int hp = 0;
        long i, j;
        long n, m;
        double kt;
        var data = GetPixelArray(bitmap);

        while (lw * 2 <= w)
        {
            lw *= 2;
            wp++;
        }
        while (lh * 2 <= h)
        {
            lh *= 2;
            hp++;
        }
        var t = new Complex[lw * lh];
        var f = new Complex[lw * lh];
        var tw = new Complex[lw];
        var th = new Complex[lw];
        for (i = 0; i < lh; i++)
        {
            for (j = 0; j < lw; j++)
            {
                t[i * lw + j] = new (data[i][j] == 0 ? 0 : 0xffffff, 0.0);
            }
        }
        for (i = 0; i < lh; i++) // 垂直方向傅立叶变换
        {
            Array.Copy(t, i * lw, tw, 0, lw);
            Array.Copy(f, i * lw, th, 0, lw);
            FFT(tw, th, wp);
            // Array.Copy(tw, 0, t, i * lw, lw);
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
                kt = (val / max) * 255.0;// ( / max * 255.0);
                n = (h - lh) / 2 + (i < lh / 2 ? i + lh / 2 : i - lh / 2);
                m = (w - lw) / 2 + (j < lw / 2 ? j + lw / 2 : j - lw / 2);
                data[n][m] = Color.FromArgb((byte)(kt),
                    (byte)(kt), (byte)(kt)).ToArgb();
            }
        }

        return GetBitmap(data);
    }
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

    private static Direction Turn(Direction d, bool left = true) => left
            ? (Direction)(((int)d + 1) % 4)
            : (d == Direction.Down) ? Direction.Left : ((Direction)((int)d) - 1)
        ;
    private static Point LeftOf(Point p, Direction d) => d switch
    {
        Direction.Down => new (p.X + 1, p.Y),
        Direction.Right => new (p.X, p.Y - 1),
        Direction.Up => new (p.X - 1, p.Y),
        Direction.Left => new (p.X, p.Y + 1),
        _ => p,
    };
    private static Point RightOf(Point p, Direction d) => d switch
    {
        Direction.Down => new (p.X - 1, p.Y),
        Direction.Right => new (p.X, p.Y + 1),
        Direction.Up => new (p.X + 1, p.Y),
        Direction.Left => new (p.X, p.Y - 1),
        _ => p,
    };

    private static Point ForwardOf(Point p, Direction d) => d switch
    {
        Direction.Down => new (p.X, p.Y + 1),
        Direction.Right => new (p.X + 1, p.Y),
        Direction.Up => new (p.X, p.Y - 1),
        Direction.Left => new (p.X - 1, p.Y),
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
    const int Black = 0x000000;
    const int White = 0xffffff;
    const int Red = 0x0000ff;
    bool flip = false;
    bool inverse = false;
    int length = 0;
    (int, long, bool)[,]? primes = null;
    private static (int, long, bool)[,] BuildPrimesMap(int length)
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
        return map;
    }

    private static Point FlipProject(Size size, Point p, bool flip = false)
    {
        var cp = new Point(size.Width >> 1, size.Height >> 1);
        if (!flip)
        {
            return p;
        }
        else
        {
            int x = p.X, y = p.Y;
            x = x < cp.X ? cp.X - x : size.Width + (cp.X - x) - 1;
            y = y < cp.Y ? cp.Y - y : size.Height + (cp.Y - y) - 1;
            return new Point(x, y);
        }
    }
    private void GeneratePrimesMap(bool flip = false, bool inverse = false)
    {
        this.length = Math.Max(PrimesPictureBox.Width, PrimesPictureBox.Height);
        this.primes = BuildPrimesMap(this.length);

        var bitmap = new Bitmap(PrimesPictureBox.Width, PrimesPictureBox.Height);
        Size size = new(PrimesPictureBox.Width, PrimesPictureBox.Height);
        for (int y = 0; y < size.Height; y++)
        {
            for (int x = 0; x < size.Width; x++)
            {
                var px = x + ((this.length - PrimesPictureBox.Width) >> 1);
                var py = y + ((this.length - PrimesPictureBox.Height) >> 1);
                var c = this.primes[px, py];
                var fp = FlipProject(size, new Point(x, y), flip);
                bitmap.SetPixel(fp.X, fp.Y,
                    ((!inverse) ? c.Item3 : !c.Item3) ? Color.White : Color.Black);
            }
        }

        this.PrimesPictureBox.Image?.Dispose();
        this.PrimesPictureBox.Image = bitmap;
    }
    private void Reset()
    {
        this.GeneratePrimesMap(this.flip = false);
    }
    private void Flip()
    {
        this.GeneratePrimesMap(this.flip = !this.flip, this.inverse);
    }
    private void Inverse()
    {
        this.GeneratePrimesMap(this.flip, this.inverse = !this.inverse);
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
        this.GeneratePrimesMap(this.flip);
    }

    private void FormMain_Load(object sender, EventArgs e)
    {
        this.GeneratePrimesMap(this.flip);
    }

    private void PrimesPictureBox_MouseMove(object sender, MouseEventArgs e)
    {
        if (this.primes != null && PrimesPictureBox.Image != null)
        {
            var x = e.X;// + ((PrimesPictureBox.Image.Width - PrimesPictureBox.Width) >> 1);
            var y = e.Y;// + ((PrimesPictureBox.Image.Height - PrimesPictureBox.Height) >> 1);

            if (x >= 0 && x < PrimesPictureBox.Image.Width
                && y >= 0 && y < PrimesPictureBox.Image.Height)
            {
                PointF cp = new(PrimesPictureBox.Image.Width >> 1, PrimesPictureBox.Image.Height >> 1);
                PointF dp = new(x - cp.X, cp.Y - y);

                PointF mp = new(dp.X + (this.length >> 1) - 1, dp.Y + (this.length >> 1) - 1);
                if (mp.X > this.length - 1)
                    mp.X = this.length - 1;
                if (mp.Y > this.length - 1)
                    mp.Y = this.length - 1;
                if (mp.X < 0)
                    mp.X = 0;
                if (mp.Y < 0)
                    mp.Y = 0;

                var p = this.primes![(int)mp.X, (int)mp.Y];
                long n = p.Item2;
                bool b = p.Item3;
                var t = Math.Atan2(dp.Y, dp.X) / Math.PI * 180.0;
                this.InfoLabel.Text = $"n={n}, x={dp.X}, y={dp.Y}, d={t:N4}°: {(b ? "Prime" : "Composite")}";
            }
        }
    }

    private void ResetButton_Click(object sender, EventArgs e)
    {
        this.Reset();
    }

    private void RotateButton_Click(object sender, EventArgs e)
    {
        this.Flip();
    }

    private void InverseButton_Click(object sender, EventArgs e)
    {
        this.Inverse();
    }
}

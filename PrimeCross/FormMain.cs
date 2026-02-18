using System.Drawing.Imaging;
using System.Numerics;

namespace PrimeCross;

public partial class FormMain : Form
{
    public static Bitmap GetBitmap(int[][] pixelArray, Bitmap bitmap)
    {
        // 创建一个新的Bitmap对象来存储修改后的图像
        var newBitmap = new Bitmap(bitmap.Width, bitmap.Height);
        using var g = Graphics.FromImage(newBitmap);
        g.Clear(Color.Blue);
        // 遍历像素数组并设置新Bitmap的像素值
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                int pixelValue = pixelArray[y][x];
                pixelValue |= unchecked((int)0xff000000);
                Color c = Color.FromArgb(pixelValue);
                //Color color = Color.FromArgb((unchecked((int)0xff000000)) | pixelValue);
                newBitmap.SetPixel(x, y, c);
            }
        }
        return newBitmap;
    }
    public static int[][] GetPixelArray(Bitmap bitmap)
    {
        // 创建一个与bitmap大小相同的数组来存储像素值
        int width = bitmap.Width;
        int height = bitmap.Height;
        int[][] pixelArray = new int[height][];
        for (int i = 0; i < height; i++)
        {
            pixelArray[i] = new int[width];
        }
        // 锁定bitmap的位
        var rect = new Rectangle(0, 0, width, height);
        BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

        // 获取每行的字节长度
        int bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
        int bytes = Math.Abs(bmpData.Stride) * height;
        byte[] rgbValues = new byte[bytes];

        // 复制像素数据到数组中
        System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, rgbValues, 0, bytes);

        // 遍历像素数据并填充到数组中
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = y * bmpData.Stride + x * bytesPerPixel; // 计算当前像素的索引位置
                var v = rgbValues[i] | (rgbValues[i + 1] << 8) | (rgbValues[i + 2] << 16) | (rgbValues[i + 3] << 24);
                pixelArray[y][x] = v; // 对于24位图像，直接组合RGB值
            }
        }

        // 解锁bitmap的位
        bitmap.UnlockBits(bmpData);

        return pixelArray;
    }
    //  快速傅立叶变换
    public static void FFT(Complex[] t, Complex[] f, int r)   // t为时域，f为频域 r为2的幂数
    {
        long count;
        int i, j, k, p, bsize;
        Complex[] W;
        Complex[] X1;
        Complex[] X2;
        Complex[] X;
        Complex comp;
        double angle;  // 计算加权时所需角度
        count = 1 << r;

        W = new Complex[count / 2];
        X1 = new Complex[count];
        X2 = new Complex[count];
        X = new Complex[count];
        for (i = 0; i < count / 2; i++)
        {
            angle = i * Math.PI * 2 / count;
            W[i] = new Complex((double)Math.Cos(angle)
            , -(double)Math.Sin(angle));
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
            X = X1;
            X1 = X2;
            X2 = X;
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
    public static Bitmap Fourier(Bitmap tp)
    {
        // 原图像的宽与高
        int w = tp.Width;
        int h = tp.Height;
        // 傅立叶变换的实际宽高
        long lw = 1;
        long lh = 1;
        // 迭代次数
        int wp = 0; int hp = 0;
        long i, j;
        long n, m;
        double temp;
        Complex[] t;
        Complex[] f;
        var ky = GetPixelArray(tp);

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
        t = new Complex[lw * lh];
        f = new Complex[lw * lh];
        Complex[] tw = new Complex[lw];
        Complex[] th = new Complex[lw];
        for (i = 0; i < lh; i++)
        {
            for (j = 0; j < lw; j++)
            {
                t[i * lw + j] = new Complex(ky[i][j] == 0 ? 0 : 0xffffff, 0.0);
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

        Complex[] ow = new Complex[lh];
        Complex[] oh = new Complex[lh];
        for (i = 0; i < lw; i++)
        {
            Array.Copy(t, i * lh, ow, 0, lh);
            Array.Copy(f, i * lh, oh, 0, lh);
            FFT(ow, oh, hp);
            //Array.Copy(ow, 0, t, i * lh, lh);
            oh.CopyTo(f, i * lh);
        }


        var max = f.Max(c => (c.Magnitude));

        for (i = 0; i < lh; i++)
        {
            for (j = 0; j < lw; j++)
            {
                temp = (f[j * lh + i].Magnitude / max) * 255.0;// ( / max * 255.0);
                n = (h - lh) / 2 + (i < lh / 2 ? i + lh / 2 : i - lh / 2);
                m = (w - lw) / 2 + (j < lw / 2 ? j + lw / 2 : j - lw / 2);
                ky[n][m] = Color.FromArgb((byte)(temp),
                    (byte)(temp), (byte)(temp)).ToArgb();
            }
        }

        return GetBitmap(ky, tp);
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
    int length = 0;
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
        return map;
    }
    private void GeneratePrimesMap()
    {
        this.length = Math.Max(PrimesPictureBox.Width, PrimesPictureBox.Height);
        this.primes = BuildPrimesMap(this.length);

        var bitmap = new Bitmap(PrimesPictureBox.Width, PrimesPictureBox.Height);

        for (int y = 0; y < PrimesPictureBox.Height; y++)
        {
            for (int x = 0; x < PrimesPictureBox.Width; x++)
            {
                var px = x + ((this.length - PrimesPictureBox.Width) >> 1);
                var py = y + ((this.length - PrimesPictureBox.Height) >> 1);
                var c = this.primes[px, py];
                bitmap.SetPixel(x, y, c.Item3 ? Color.White : Color.Black);
            }
        }
        this.PrimesPictureBox.Image?.Dispose();
        this.PrimesPictureBox.Image = bitmap;
    }
    private void GenerateButton_Click(object sender, EventArgs e)
    {
        if (this.PrimesPictureBox.Image is Bitmap bitmap)
        {
            if (bitmap.Clone() is Bitmap clone)
            {
                bitmap.Dispose();
                this.PrimesPictureBox.Image = null;
                this.PrimesPictureBox.Image = Fourier(clone);
                clone.Dispose();
            }
        }
    }

    private void FormMain_Resize(object sender, EventArgs e)
    {
        this.GeneratePrimesMap();
    }

    private void FormMain_Load(object sender, EventArgs e)
    {
        this.GeneratePrimesMap();
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
                PointF cp = new(PrimesPictureBox.Image.Width >> 1, PrimesPictureBox.Image.Height >> 1);
                PointF dp = new(x - cp.X, cp.Y - y);

                PointF mp = new(dp.X + (this.length >> 1), dp.Y + (this.length >> 1));
                var p = this.primes![(int)mp.X, (int)mp.Y];
                long n = p.Item2;
                bool b = p.Item3;
                var t = Math.Atan2(dp.Y, dp.X) / Math.PI * 180.0;
                this.InfoLabel.Text = $"n={n}, x={dp.X}, y={dp.Y}, d={t:N4}°: {(b ? "Prime" : "Composite")}";
            }
        }
    }
}

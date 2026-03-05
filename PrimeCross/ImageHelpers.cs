using System.Drawing.Imaging;
using System.Numerics;

namespace PrimeCross;

internal static class ImageHelpers
{
    const int _31 = (sizeof(int) << 3) - 1;
    // 加窗类型枚举
    enum WindowType
    {
        Rectangle,
        Hamming,
        Hanning,
        Blackman
    }

    // 应用加窗函数到二维数据
    static double[,] ApplyWindowing(double[,] data, WindowType windowType)
    {
        int rows = data.GetLength(0);
        int cols = data.GetLength(1);
        double[,] windowed = new double[rows, cols];

        // 生成行窗函数
        double[] rowWindow = GenerateWindow(cols, windowType);
        // 生成列窗函数
        double[] colWindow = GenerateWindow(rows, windowType);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                windowed[i, j] = data[i, j] * rowWindow[j] * colWindow[i];
            }
        }

        return windowed;
    }

    // 生成一维窗函数
    static double[] GenerateWindow(int length, WindowType windowType)
    {
        double[] window = new double[length];

        switch (windowType)
        {
            case WindowType.Rectangle:
                for (int i = 0; i < length; i++)
                    window[i] = 1.0;
                break;
            case WindowType.Hamming:
                double alpha = 0.54;
                double beta = 0.46;
                for (int i = 0; i < length; i++)
                    window[i] = alpha - beta * Math.Cos(2 * Math.PI * i / (length - 1));
                break;
            case WindowType.Hanning:
                double a0 = 0.5;
                double a1 = 0.5;
                for (int i = 0; i < length; i++)
                    window[i] = a0 - a1 * Math.Cos(2 * Math.PI * i / (length - 1));
                break;
            case WindowType.Blackman:
                double a0b = 0.42;
                double a1b = 0.5;
                double a2b = 0.08;
                for (int i = 0; i < length; i++)
                    window[i] = a0b - a1b * Math.Cos(2 * Math.PI * i / (length - 1)) + a2b * Math.Cos(4 * Math.PI * i / (length - 1));
                break;
        }

        return window;
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
}
// Draws the application icon (original artwork: a sun whose centre is a half-filled circle,
// i.e. "brightness" + "contrast") and writes it as a multi-size .ico. Used by tools\make-icon.ps1.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class IconArt
{
    static GraphicsPath RoundRect(float x, float y, float w, float h, float r)
    {
        var p = new GraphicsPath(); float d = r * 2;
        p.AddArc(x, y, d, d, 180, 90); p.AddArc(x + w - d, y, d, d, 270, 90);
        p.AddArc(x + w - d, y + h - d, d, d, 0, 90); p.AddArc(x, y + h - d, d, d, 90, 90);
        p.CloseFigure(); return p;
    }

    // variant "warm": the whole sun is yellow-orange.  variant "cool": warm rays, cyan-violet centre.
    public static Bitmap Draw(string variant, int n)
    {
        var b = new Bitmap(n, n, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(b))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias; g.PixelOffsetMode = PixelOffsetMode.HighQuality; g.Clear(Color.Transparent);
            float cx = n / 2f, cy = n / 2f;
            using (var bg = new LinearGradientBrush(new PointF(0, 0), new PointF(n, n), Color.FromArgb(255, 28, 22, 48), Color.FromArgb(255, 58, 34, 92)))
                g.FillPath(bg, RoundRect(n * 0.04f, n * 0.04f, n * 0.92f, n * 0.92f, n * 0.2f));

            using (var warm = new LinearGradientBrush(new PointF(0, 0), new PointF(n, n), Color.FromArgb(255, 255, 205, 80), Color.FromArgb(255, 255, 140, 60)))
            using (var cool = new LinearGradientBrush(new PointF(0, 0), new PointF(n, n), Color.FromArgb(255, 90, 220, 255), Color.FromArgb(255, 190, 120, 255)))
            {
                bool small = n <= 24;   // thicker strokes so the shape survives at tray size
                using (var pen = new Pen(warm, n * (small ? 0.085f : 0.06f)))
                {
                    pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round;
                    for (int i = 0; i < 8; i++)
                    {
                        double a = i * Math.PI / 4; float inner = n * 0.30f, outer = n * 0.38f;
                        g.DrawLine(pen, (float)(cx + inner * Math.Cos(a)), (float)(cy + inner * Math.Sin(a)),
                                        (float)(cx + outer * Math.Cos(a)), (float)(cy + outer * Math.Sin(a)));
                    }
                }
                Brush core = variant == "cool" ? (Brush)cool : (Brush)warm;
                float ringR = n * 0.2f;
                using (var ring = new Pen(core, n * (small ? 0.075f : 0.05f))) g.DrawEllipse(ring, cx - ringR, cy - ringR, ringR * 2, ringR * 2);
                float fillR = n * (small ? 0.2f : 0.135f);
                g.FillPie(core, cx - fillR, cy - fillR, fillR * 2, fillR * 2, -90, 180);
            }
        }
        return b;
    }

    public static void SaveIco(string variant, string path)
    {
        int[] sizes = { 16, 24, 32, 48, 64, 128, 256 };
        var imgs = new List<byte[]>();
        foreach (int s in sizes)
            using (var bmp = Draw(variant, s)) using (var ms = new MemoryStream()) { bmp.Save(ms, ImageFormat.Png); imgs.Add(ms.ToArray()); }
        using (var fs = new FileStream(path, FileMode.Create)) using (var w = new BinaryWriter(fs))
        {
            w.Write((ushort)0); w.Write((ushort)1); w.Write((ushort)sizes.Length);
            int off = 6 + 16 * sizes.Length;
            for (int i = 0; i < sizes.Length; i++)
            {
                byte dim = (byte)(sizes[i] >= 256 ? 0 : sizes[i]);
                w.Write(dim); w.Write(dim); w.Write((byte)0); w.Write((byte)0); w.Write((ushort)1); w.Write((ushort)32);
                w.Write((uint)imgs[i].Length); w.Write((uint)off); off += imgs[i].Length;
            }
            foreach (var im in imgs) w.Write(im);
        }
    }

    // preview sheet: each variant at 256 / 48 / 32 / 16 px on a light and a dark background
    public static void SavePreview(string[] variants, string path)
    {
        int colW = 340;
        using (var sheet = new Bitmap(20 + colW * variants.Length, 760))
        using (var g = Graphics.FromImage(sheet))
        using (var font = new Font("Segoe UI", 18, FontStyle.Bold))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias; g.Clear(Color.White);
            g.FillRectangle(new SolidBrush(Color.FromArgb(255, 32, 32, 32)), 0, 380, sheet.Width, 380);
            for (int v = 0; v < variants.Length; v++)
                foreach (int row in new[] { 0, 380 })
                {
                    int x = 20 + v * colW;
                    using (var i256 = Draw(variants[v], 256)) g.DrawImage(i256, x, row + 50, 256, 256);
                    using (var i48 = Draw(variants[v], 48)) g.DrawImage(i48, x + 270, row + 60, 48, 48);
                    using (var i32 = Draw(variants[v], 32)) g.DrawImage(i32, x + 270, row + 130, 32, 32);
                    using (var i16 = Draw(variants[v], 16)) g.DrawImage(i16, x + 270, row + 185, 16, 16);
                    g.DrawString(variants[v], font, row == 0 ? Brushes.Black : Brushes.White, x, row + 8);
                }
            sheet.Save(path, ImageFormat.Png);
        }
    }
}

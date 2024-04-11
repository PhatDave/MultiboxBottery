using System.Diagnostics;
using System.Drawing;

namespace DD2Switcher;

public class Pixel {
    public Pixel(int x, int y, int R, int G, int B) {
        this.x = x;
        this.y = y;
        this.R = R;
        this.G = G;
        this.B = B;
    }

    private int x { get; }
    private int y { get; }
    private int R { get; }
    private int G { get; }
    private int B { get; }

    public bool ProcessBitmap(Bitmap bmp) {
        var tempPixel = bmp.GetPixel(x, y);
        return tempPixel.R >= R && tempPixel.B >= B && tempPixel.G >= G;
    }
}
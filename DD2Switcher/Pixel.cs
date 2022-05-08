using System;
using System.Drawing;

namespace DD2Switcher; 

public class Pixel {
    private int x { get; set; }
    private int y { get; set; }
    private int R { get; set; }
    private int G { get; set; }
    private int B { get; set; }

    public Pixel(int x, int y, int R, int G, int B) {
        this.x = x;
        this.y = y;
        this.R = R;
        this.G = G;
        this.B = B;
    }

    public Boolean ProcessBitmap(Bitmap bmp) {
        Color tempPixel = bmp.GetPixel(x, y);
        return tempPixel.R >= R && tempPixel.B >= B && tempPixel.G >= G;
    }
}
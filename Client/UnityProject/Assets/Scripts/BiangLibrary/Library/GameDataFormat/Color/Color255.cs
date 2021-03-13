using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Exactly like Color, but instead of 0-1 values, they're 0-255. */
public struct Color255 {
    public float r,g,b, a;
    public Color255(float r,float g,float b) {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = 255;
    }
    public Color255(float r,float g,float b, float a) {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }
    
    public Color ToColor() { return new Color(r/255f,g/255f,b/255f, a/255f); }
}

using System;

public static class MathUtils_FOP
{
    //This function need to be later changed to me more flexible on min max mapping direction
    //Remap range, with the option of clamping the output
    public static int Remap(int value, int a1, int a2, int b1, int b2, bool clamp = true)
    {
        int ret = (value - a1) / (a2 - a1) * (b2 - b1) + b1;
        if (clamp) { ret = Math.Min(Math.Max(ret, b1), b2); }
        return ret;
    }

    //This function need to be later changed to me more flexible on min max mapping direction
    //Remap range, with the option of clamping the output
    public static float Remap(float value, float a1, float a2, float b1, float b2, bool clamp = true)
    {
        float ret = (value - a1) / (a2 - a1) * (b2 - b1) + b1;
        if (clamp) { ret = Math.Min(Math.Max(ret, b1), b2); }
        return ret;
    }
}
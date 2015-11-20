using System;
using System.Collections.Generic;
using System.Diagnostics;

class Map2d<Type>
{
    public Map2d(uint width, uint length)
    {
        sizeX = width;
        sizeY = length;

        values = new Type[sizeX * sizeY];
    }

    public Type readAt(int x, int y)
    {
        System.Diagnostics.Debug.Assert(x >= 0 && x < sizeX);
        System.Diagnostics.Debug.Assert(y >= 0 && y < sizeY);

        return values[x + y * sizeX];
    }

    public void writeAt(int x, int y, Type value)
    {
        System.Diagnostics.Debug.Assert(x >= 0 && x < sizeX);
        System.Diagnostics.Debug.Assert(y >= 0 && y < sizeY);

        values[x + y * sizeX] = value;
    }

    public uint getWidth()
    {
        return sizeX;
    }

    public uint getLength()
    {
        return sizeY;
    }

    // for fast low level access, use with caution
    public Type[] unsafeGetValues()
    {
        return values;
    }

    public Map2d<Type> clone()
    {
        Map2d<Type> result;

        result = new Map2d<Type>(sizeX, sizeY);
        result.values = (Type[])values.Clone();

        return result;
    }

    private uint sizeX;
    private uint sizeY;
    private Type []values;
}


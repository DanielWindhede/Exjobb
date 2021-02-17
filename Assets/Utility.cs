using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static int Modulo(int x, int m)
    {
        return (x % m + m) % m;
    }
}

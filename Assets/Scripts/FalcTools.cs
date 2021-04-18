using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalcTools
{
    /// <summary>
    /// Dado el valor x, en el intervalo [a;b], 
    /// devuelve la posicion de x en el intervalo [c;d].
    /// Devuelve NaN error cuando a = b.
    /// </summary>
    public static float Remap(float x, float a, float b, float c, float d)
    {
        if(b != a){
            return c + (d-c)*(x-a)/(b-a);
        }
        else
        {
            return float.NaN;
        }
    }

    /// <summary>
    /// Convierte (x,y,z) en (x,0,z).
    /// </summary>
    public static Vector3 NullY(Vector3 value)
    {
        return value - value.y * Vector3.up;
    }

    /// <summary>
    /// Devuelve true cuando gameObject se encuentra en
    /// alguna capa marcada de mask. De no estarlo, 
    /// devuelve false.
    /// </summary>
    public static bool IsGameObjectInLayerMask(GameObject gameObject, LayerMask mask){
        return ((1 << gameObject.layer) & mask) != 0;
    }
}

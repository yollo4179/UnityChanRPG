using UnityEngine;

public static class VectorUtil
{
    public static Vector3 PlatVector(Vector3 vec)
    {
        vec.y = 0;
        return vec; 
    }
    public static Vector3 PlatVector(Vector3 dst, Vector3 src)
    {
        Vector3 vec= dst - src;
        vec.y=0; 
        return vec;
    }


}

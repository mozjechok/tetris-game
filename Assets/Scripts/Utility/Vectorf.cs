
using UnityEngine;

public static class Vectorf
{
    public static Vector2 RoundToInt(Vector2 v) => new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));

    public static Vector3 RoundToInt(Vector3 v) =>
        new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));

    public static Vector3 MultiplyVec3ToMat4(Vector3 v, Matrix4x4 mat)
    {
        var resultVec = mat * new Vector4(v.x, v.y, v.z, 0);
        return new Vector3(resultVec.x, resultVec.y, resultVec.z);
    }
}

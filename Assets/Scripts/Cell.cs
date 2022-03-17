using UnityEngine;

public struct Cell
{
    public enum Type
    {
        Empty,
        Mine,
        Number
    }

    public Type type;
    public int number;
    public Vector3Int position;
    public bool flagged;
    public bool revealed;
    public bool exploded;

}

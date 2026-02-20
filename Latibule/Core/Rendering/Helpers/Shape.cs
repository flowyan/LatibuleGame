using OpenTK.Mathematics;

namespace Latibule.Core.Rendering.Helpers;

public abstract class Shape
{
    private Vector3[] vertices, normals;
    private Vector2[] texcoords;
    private uint[] indices;
    private int[] colors;

    public Vector3[] Vertices
    {
        get { return vertices; }
        protected set { vertices = value; }
    }

    public Vector3[] Normals
    {
        get { return normals; }
        protected set { normals = value; }
    }

    public Vector2[] Texcoords
    {
        get { return texcoords; }
        protected set { texcoords = value; }
    }

    public uint[] Indices
    {
        get { return indices; }
        protected set { indices = value; }
    }

    public int[] Colors
    {
        get { return colors; }
        protected set { colors = value; }
    }
}
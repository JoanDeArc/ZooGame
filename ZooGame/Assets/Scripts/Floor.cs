using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour {

    private List<List<Vector3>> points;
    public List<List<Vector3>> Points { get { return points; } set { points = value; } }

    public List<List<GameObject>> items;

    public Transform grid;

    public float gridSquareLength;

    private bool displayGrid = true;
    public bool DisplayGrid
    { get { return displayGrid; }
      set
        {
            displayGrid = value;
            grid.GetComponent<Renderer>().enabled = displayGrid;
        }
    }

    public void ToggleGrid()
    {
        DisplayGrid = !DisplayGrid;
    }

	// Use this for initialization
	void Start () {
        SortVertices();

        /*
        items = new List<List<GameObject>>();
        foreach (List<Vector3> l in points)
        {
            items.Add(new List<GameObject>());
            foreach (Vector3 v in l)
            {
                items[items.Count - 1].Add(null);
            }
        }*/
    }
	
	// Update is called once per frame
	void Update () {


        if (displayGrid)
            DrawGrid();
    }


    void DrawGrid()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;


        if (Physics.Raycast(ray, out hit, 100))
        {
            grid.GetComponent<Renderer>().material.SetVector("_ObjPos", new Vector4(hit.point.x, hit.point.y, hit.point.z, 0));
        }
    }

    public void DrawGridAt(Vector3 position)
    {
        DisplayGrid = true;
        grid.GetComponent<Renderer>().material.SetVector("_ObjPos", new Vector4(position.x, position.y, position.z, 0));
    }

    public void AddToFloor(int x, int y, GameObject newObject)
    {
        items[x][y] = newObject;
    }

    public bool FloorBusyAt(int x, int y)
    {
        if (items[x][y] == null)
            return false;
        return true;
    }

    public List<int> NearestVertexIndicesTo(Vector3 point, int within)
    {
        List<int> indices = new List<int>();

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        float minDistanceSqr = Mathf.Infinity;
        int soloIndex = 0;
        // scan all vertices to find nearest
        for (int i = 0; i < mesh.vertices.Length; i++)
            {
                if (within == 0)
                {
                    Vector3 diff = point - mesh.vertices[i];
                    float distSqr = diff.sqrMagnitude;
                    if (distSqr < minDistanceSqr)
                    {
                        minDistanceSqr = distSqr;
                        soloIndex = i;
                    }
                }
                else
                {
                    Vector3 diff = point - mesh.vertices[i];
                    float distSqr = diff.sqrMagnitude;
                    if (distSqr < within)
                    {
                        indices.Add(i);
                    }
                }
            }

        if (within == 0)
            indices.Add(soloIndex);

        return indices;
    }

    public Vector3 ClosestIndexPosition(Vector3 hit)
    {
        Vector2 index = NearestWorldPositionIndicicesTo(hit, 0)[0];
        return Points[(int)index.x][(int)index.y];
    }

    public List<Vector2> NearestWorldPositionIndicicesTo(Vector3 point, int within)
    {
        // convert point to local space
        //point = transform.InverseTransformPoint(point);

        List<Vector2> indices = new List<Vector2>();

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        float minDistanceSqr = Mathf.Infinity;
        Vector2 soloIndex = Vector2.zero;
        // scan all vertices to find nearest
        for (int i = 0; i < points.Count; i++)
            for (int j = 0; j < points[i].Count; j++)
            {
                if (within == 0)
                {
                    Vector3 diff = point - points[i][j];
                    float distSqr = diff.sqrMagnitude;
                    if (distSqr < minDistanceSqr)
                    {
                        minDistanceSqr = distSqr;
                        soloIndex = new Vector2(i, j);
                    }
                }
                else
                {
                    Vector3 diff = point - points[i][j];
                    float distSqr = diff.sqrMagnitude;
                    if (distSqr < within)
                    {
                        indices.Add(new Vector2(i, j));
                    }
                }
            }

        if (within == 0)
            indices.Add(soloIndex);

        return indices;
    }

    public Vector3 NearestGridPos(Vector3 point)
    {
        Vector2 index = NearestWorldPositionIndicicesTo(point, 0)[0];

        return points[(int)index.x][(int)index.y];
    }


    bool hasVertexX(ref List<List<Vector3>> list, float x, ref int result)
    {
        if (list.Count == 0)
            return false;
        for (int i = 0; i < list.Count; i++)
        {
            if (System.Math.Round(list[i][0].x, 1) == System.Math.Round(x, 1))
            {
                result = i;
                return true;
            }
        }
        return false;
    }

    public float HeightAt(Vector3 point)
    {
        return ClosestIndexPosition(point).y;
    }

    public void SortVertices()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        List<Vector3> vertices = new List<Vector3>(mesh.vertices);


        float xValue1 = -1, xValue2 = -1;
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = transform.TransformPoint(vertices[i]);

            // Find out size of grid squares
            if (gridSquareLength == 0)
            {
                if (vertices[i].x != xValue1)
                    xValue1 = vertices[i].x;
                if (xValue1 != xValue2)
                    gridSquareLength = Mathf.Abs(xValue1 - xValue2);
                if (vertices[i].x != xValue2)
                    xValue2 = vertices[i].x;
            }
        }

        List<List<Vector3>> floorCoords = new List<List<Vector3>>();

        for (int i = 0; i < vertices.Count; i++)
        {
            int index = 0;

            if (hasVertexX(ref floorCoords, vertices[i].x, ref index))
                floorCoords[index].Add(vertices[i]);
            else
            {
                floorCoords.Add(new List<Vector3>() { vertices[i] });
            }
        }


        floorCoords.Sort((a, b) => a[0].x.CompareTo(b[0].x));
        for (int i = 0; i < floorCoords.Count; i++)
            floorCoords[i].Sort((a, b) => a.z.CompareTo(b.z));

        points = floorCoords;

        //for (int i = 0; i < floorCoords.Count; i++)
          //for (int j = 0; j < floorCoords[i].Count; j++)
            //Debug.DrawLine(floorCoords[i][j], floorCoords[i][j] + Vector3.up, Color.red, 999999);

        //return floorCoords;
    }
}

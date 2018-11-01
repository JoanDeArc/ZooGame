using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour {

    private List<List<Vector3>> points;
    public List<List<Vector3>> Points { get { return points; } set { points = value; } }

    public Transform grid;
    private bool displayGrid = false;
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
        points = SortVertices();
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

    List<List<Vector3>> SortVertices()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        List<Vector3> vertices = new List<Vector3>(mesh.vertices);

        for (int i = 0; i < vertices.Count; i++)
            vertices[i] = transform.TransformPoint(vertices[i]);

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

        //for (int i = 0; i < floorCoords.Count; i++)
          //for (int j = 0; j < floorCoords[i].Count; j++)
            //Debug.DrawLine(floorCoords[i][j], floorCoords[i][j] + Vector3.up, Color.red, 999999);

        return floorCoords;
    }
}

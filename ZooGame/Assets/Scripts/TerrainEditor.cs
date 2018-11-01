using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainEditor : MonoBehaviour {

    public Floor floor;
    Mesh floorMesh;
    Mesh gridMesh;

    [Range(-0.4f, 0.4f)]
    public float strength = 0.2f;
    [Range(0, 5)]
    public int range = 0;

    // Use this for initialization
    void Start () {
        floorMesh = floor.GetComponent<MeshFilter>().mesh;
        gridMesh = floor.grid.GetComponent<MeshFilter>().mesh;
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {

                List<int> vertexIndices = NearestVertexIndexTo(hit.transform, hit.point, range);
                ManipulatePoints(vertexIndices, strength);
                
            }
        }
    }

    void ManipulatePoints(List<int> indices, float amount)
    {
        Vector3[] vertices = floorMesh.vertices;
        Vector3[] gridVertices = gridMesh.vertices;

        for (int i = 0; i < indices.Count; i++)
        {
            vertices[indices[i]].y += amount;
            gridVertices[indices[i]].y += amount;
        }

        floorMesh.vertices = vertices;
        floorMesh.RecalculateBounds();
        floor.GetComponent<MeshCollider>().sharedMesh = floorMesh;

        gridMesh.vertices = gridVertices;
        gridMesh.RecalculateBounds();
    }


    List<int> NearestVertexIndexTo(Transform target, Vector3 point, int within)
    {
        // convert point to local space
        point = target.transform.InverseTransformPoint(point);

        List<int> indices = new List<int>();
        
        Mesh mesh = target.GetComponent<MeshFilter>().mesh;
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainEditor : MonoBehaviour {

    public Floor floor;
    Mesh floorMesh;
    Mesh gridMesh;

    //public bool editorEnabled = false;

    [Range(-0.4f, 0.4f)]
    public float strength = 0.2f;
    [Range(0, 5)]
    public int range = 0;

    public void ToggleEnabled()
    {
        gameObject.SetActive(!isActiveAndEnabled);
    }

    public void SetEnabled(bool value)
    {
        gameObject.SetActive(value);
    }

    // Use this for initialization
    void Start () {
        floorMesh = floor.GetComponent<MeshFilter>().mesh;
        gridMesh = floor.grid.GetComponent<MeshFilter>().mesh;
	}
	
	// Update is called once per frame
	void Update () {

        if (!isActiveAndEnabled)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {

                int layerMask = 1 << 8;
                if (Physics.CheckSphere(floor.ClosestIndexPosition(hit.point), floor.gridSquareLength * 0.9f, layerMask))
                {
                    Debug.Log("Can't manipulate terrain under buildings!");
                    return;
                }

                List<int> vertexIndices = floor.NearestVertexIndicesTo(hit.point, range);
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

        // Moved to onDisable. Kept in case of errors
        // Should be done after exiting editor
        //floor.SortVertices();
    }

    private void OnDisable()
    {
        if (floor)
            floor.SortVertices();
    }
}

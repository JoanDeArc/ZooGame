using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FenceBuilder : MonoBehaviour
{
    public Floor floor;

    public GameObject fencePrefab;
    public GameObject markerPrefab;
    private GameObject marker;

    private Vector3 markerPosition;
    private Vector3 possibleNewMarkerPos;

    private List<GameObject> tentativeFences;
    private bool okToPlace;

    // For mesh manipulation, caluculated by floor square size
    private float diagonalScaleAmount;

    public Material outlineMat;
    public Material errorMat;
    private Material fenceMat;

    public void ToggleEnabled()
    {
        gameObject.SetActive(!isActiveAndEnabled);
    }

    private void Start()
    {
        fenceMat = fencePrefab.GetComponent<Renderer>().sharedMaterial;
        tentativeFences = new List<GameObject>();

        Vector3[] newVertices = fencePrefab.GetComponent<MeshFilter>().sharedMesh.vertices;

        diagonalScaleAmount = Mathf.Sqrt(Mathf.Pow(floor.gridSquareLength, 2) + Mathf.Pow(floor.gridSquareLength, 2)) - floor.gridSquareLength;
    }

    void Update()
    {
        // Place marker for start position
        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;


            if (Physics.Raycast(ray, out hit, 100))
            {
                foreach (GameObject fence in tentativeFences)
                    Destroy(fence);
                tentativeFences.Clear();

                PlaceMarker(hit.point);
            }
        }

        // Place outline for fence segment
        if (Input.GetMouseButtonDown(1))
        {
            if (marker == null)
                return;

            okToPlace = true;

            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                foreach (GameObject fence in tentativeFences)
                    Destroy(fence);
                tentativeFences.Clear();

                PlaceSegment(hit.point);

                possibleNewMarkerPos = hit.point;
            }
        }
    }

    // Confirm outlined positions if OK
    public void ConfirmFences()
    {
        if (!okToPlace)
            return;

        foreach (GameObject fence in tentativeFences)
        {
            fence.GetComponent<Renderer>().material = fenceMat;
            fence.GetComponent<BoxCollider>().enabled = true;
            fence.layer = 8;
        }
        tentativeFences.Clear();

        PlaceMarker(possibleNewMarkerPos);
    }

    // Place start marker for fence segment
    void PlaceMarker(Vector3 pos)
    {
        markerPosition = floor.ClosestIndexPosition(pos);

        if (marker == null)
            marker = Instantiate(markerPrefab, markerPosition, Quaternion.identity);
        else
            marker.transform.position = markerPosition;
    }

    // Place one piece of fence
    void PlaceFence(Vector3 position, Vector3 rotation)
    {
        GameObject fence = Instantiate(fencePrefab, position, Quaternion.identity);

        fence.transform.LookAt(position + rotation);
        fence.transform.Rotate(0, -90, 0);

        float height = floor.HeightAt(position + rotation * floor.gridSquareLength);
        StretchFence(fence, (rotation.x != 0 && rotation.z != 0) ? diagonalScaleAmount : 0, height - position.y);

        BoxCollider collider = fence.GetComponent<BoxCollider>();

        // Check for collisions with other fences or other objects (treated differently)
        Collider[] overlaps = Physics.OverlapBox(fence.transform.TransformPoint(collider.center), collider.size / 2, fence.transform.rotation);
        if (overlaps.Length > 0)
        {
            if (overlaps[0].tag == "Fence")
            {
                Destroy(fence);
                return;
            }
            else
            {
                fence.GetComponent<Renderer>().material = errorMat;
                okToPlace = false;
            }
        }
        else
            fence.GetComponent<Renderer>().material = outlineMat;

        tentativeFences.Add(fence);     
    }

    // Place a segment of fence
    void PlaceSegment(Vector3 to)
    {
        List<Vector3> directions = PathFence(markerPosition, to);

        Vector3 spawnPos = markerPosition;
        foreach (Vector3 direction in directions)
        {
            PlaceFence(spawnPos, direction);
            spawnPos += direction * floor.gridSquareLength;
            spawnPos.y = floor.HeightAt(spawnPos);
        }
    }

    // Find shortest path from one position to the other using floor index
    List<Vector3> PathFence(Vector3 start, Vector3 end)
    {
        List<Vector3> directions = new List<Vector3>();

        Vector3 startIndex = (floor.NearestWorldPositionIndicicesTo(start, 0)[0]);
        Vector3 endIndex = (floor.NearestWorldPositionIndicicesTo(end, 0)[0]);

        float xDifference = startIndex.x - endIndex.x;
        float yDifference = startIndex.y - endIndex.y;

        if (Mathf.Abs(xDifference) > Mathf.Abs(yDifference))
        {
            if (endIndex.x > startIndex.x)
            {
                for (int i = 0; i < endIndex.x - startIndex.x - Mathf.Abs(yDifference); i++)
                    directions.Add(Vector3.right);
                for (int i = 0; i < Mathf.Abs(yDifference); i++)
                    directions.Add(Vector3.right + (yDifference < 0 ? Vector3.forward : Vector3.back));
            }
            else
            {
                for (int i = 0; i < startIndex.x - endIndex.x - Mathf.Abs(yDifference); i++)
                    directions.Add(Vector3.left);
                for (int i = 0; i < Mathf.Abs(yDifference); i++)
                    directions.Add(Vector3.left + (yDifference < 0 ? Vector3.forward : Vector3.back));
            }
        }
        else
        {
            if (endIndex.y > startIndex.y)
            {
                for (int i = 0; i < endIndex.y - startIndex.y - Mathf.Abs(xDifference); i++)
                    directions.Add(Vector3.forward);
                for (int i = 0; i < Mathf.Abs(xDifference); i++)
                    directions.Add(Vector3.forward + (xDifference < 0 ? Vector3.right : Vector3.left));
            }
            else
            {
                for (int i = 0; i < startIndex.y - endIndex.y - Mathf.Abs(xDifference); i++)
                    directions.Add(Vector3.back);
                for (int i = 0; i < Mathf.Abs(xDifference); i++)
                    directions.Add(Vector3.back + (xDifference < 0 ? Vector3.right : Vector3.left));
            }
        }
        return directions;
    }


    // Stretch the mesh of the fence
    void StretchFence(GameObject fence, float amountX, float amountY)
    {
        if (amountX == 0 && amountY == 0)
            return;

        Mesh mesh = fence.GetComponent<MeshFilter>().mesh;

        float smallestX = 0, largestX = 0;
        Vector3[] newVertices = mesh.vertices;

        foreach (Vector3 vector in newVertices)
        {
            if (vector.x < smallestX)
                smallestX = vector.x;
            if (vector.x > largestX)
                largestX = vector.x;
        }
        float middleX = (smallestX + largestX) / 1.2f;

        for (int i = 0; i < newVertices.Length; i++)
        {
            if (newVertices[i].x > middleX)
            {
                newVertices[i].x += amountX;
                newVertices[i].y += amountY;
            }
        }

        mesh.vertices = newVertices;
        mesh.RecalculateBounds();

        // Recalculate collider
        Vector3 newPos = mesh.bounds.center;
        newPos.y += Mathf.Abs(amountY) / 2;
        fence.GetComponent<BoxCollider>().center = newPos;
    }
}
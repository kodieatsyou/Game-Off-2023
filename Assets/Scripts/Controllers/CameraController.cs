using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera cam;
    public float distance;
    public float angle;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnGameBoardInitialized()
    {
        //Set Camera Position
        Vector3 boardMiddlePos = GameObject.FindGameObjectWithTag("Board").GetComponent<BoardManager>().GetBoardMiddlePos();
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = new Vector3(distance, distance, distance);
        sphere.transform.position = boardMiddlePos;
        gameObject.transform.position = GameObject.FindGameObjectWithTag("Board").GetComponent<BoardManager>().GetBoardMiddlePos();




        // Normalize the direction vector
        Vector3 direction = new Vector3(1f, 0f, 0f);
        float radius = distance / 2;
        Vector3 normalizedDirection = direction.normalized;

        // Calculate the intersection point with the sphere
        float t = Vector3.Dot(-boardMiddlePos, normalizedDirection);
        float discriminant = t * t - (boardMiddlePos.sqrMagnitude - radius * radius);

        // Check if there is an intersection
        if (discriminant >= 0)
        {
            // Calculate the two possible intersection points
            float t1 = -t + Mathf.Sqrt(discriminant);
            float t2 = -t - Mathf.Sqrt(discriminant);

            // Choose the intersection point closer to the sphere center
            float chosenT = Mathf.Min(t1, t2);

            // Calculate the intersection point in world space
            Vector3 intersectionPoint = boardMiddlePos + chosenT * normalizedDirection;

            // Place the object on the sphere's surface

            GameObject sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere2.transform.position = intersectionPoint;
        }
        else
        {
            Debug.LogError("Direction vector does not intersect with the sphere.");
        }


    }
}

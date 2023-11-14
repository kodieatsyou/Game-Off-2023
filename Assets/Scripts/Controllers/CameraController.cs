using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float zoomSpeed = 9f;
    public float rotationSpeed = 10f;
    public float maxZoomIn = 10f;
    public float maxZoomOut = 100f;

    GameObject board;
    Vector3 mousePreviousPos = Vector3.zero;
    Vector3 mousePositionDelta = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(board != null)
        {
            transform.LookAt(board.GetComponent<BoardManager>().GetBoardMiddlePos());
            DoZoom();
            DoRotate();
        }

    }

    /// <summary>
    /// Rotates the camera around the boards center position by clicking and dragging
    /// </summary>
    void DoRotate()
    {
        Vector3 target = board.GetComponent<BoardManager>().GetBoardMiddlePos();

        if (Input.GetMouseButton(0))
        {
            mousePositionDelta = (Input.mousePosition - mousePreviousPos);

            float horizontalInput = mousePositionDelta.x;
            float verticalInput = mousePositionDelta.y;

            float rotationX = verticalInput * rotationSpeed * 10f * Time.deltaTime;
            float rotationY = horizontalInput * rotationSpeed * 10f *Time.deltaTime;

            Vector3 currentRotation = transform.eulerAngles;

            float newRotationX = currentRotation.x - rotationX;

            if (newRotationX >= 0f && newRotationX <= 80f)
            {
                transform.RotateAround(target, transform.right, -rotationX);
                transform.RotateAround(target, Vector3.up, rotationY);
            }
        }

        mousePreviousPos = Input.mousePosition;
    }

    /// <summary>
    /// Zooms the camera in and out using scroll wheel
    /// </summary>
    void DoZoom()
    {
        Vector3 target = board.GetComponent<BoardManager>().GetBoardMiddlePos();

        float scrollDelta = -Input.mouseScrollDelta.y;

        Vector3 directionToTarget = target - transform.position;

        float currentDistance = directionToTarget.magnitude;

        float newDistance = Mathf.Clamp(currentDistance + scrollDelta * zoomSpeed * 100f * Time.deltaTime, maxZoomIn, maxZoomOut);

        Vector3 newPosition = target - directionToTarget.normalized * newDistance;

        transform.position = newPosition;
    }

    /// <summary>
    /// Function is triggered with the onGameBoardInitialized game event and sets up the initial camera position
    /// </summary>
    public void OnGameBoardInitialized()
    {
        board = GameObject.FindGameObjectWithTag("Board");
        //Set Initial Camera Position
        Vector3 boardMiddlePos = board.GetComponent<BoardManager>().GetBoardMiddlePos();
        transform.position = new Vector3(boardMiddlePos.x + 50, boardMiddlePos.y + 50, boardMiddlePos.z);
    }

}

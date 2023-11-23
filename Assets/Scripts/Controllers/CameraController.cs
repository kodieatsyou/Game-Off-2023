using UnityEngine;

public class CameraController : MonoBehaviour
{
    private bool canMove = true;
    public float zoomSpeed = 9f;
    public float rotationSpeed = 10f;
    public float maxZoomIn = 10f;
    public float maxZoomOut = 100f;
    Vector3 mousePreviousPos = Vector3.zero;
    Vector3 mousePositionDelta = Vector3.zero;
    int cameraHeight = 1;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 boardMiddlePos = LocalBoardManager.Instance.GetBoardMiddlePosAtYLevel(cameraHeight);
        transform.position = new Vector3(boardMiddlePos.x + 50, boardMiddlePos.y + 50, boardMiddlePos.z);
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(LocalBoardManager.Instance.GetBoardMiddlePosAtYLevel(cameraHeight));
        if(canMove)
        {
            DoZoom();
            DoRotate();
        }
    }

    /// <summary>
    /// Rotates the camera around the boards center position by clicking and dragging
    /// </summary>
    void DoRotate()
    {
        Vector3 target = LocalBoardManager.Instance.GetBoardMiddlePosAtYLevel(cameraHeight);

        if (Input.GetMouseButton(0))
        {
            mousePositionDelta = (Input.mousePosition - mousePreviousPos);

            float horizontalInput = mousePositionDelta.x;
            float verticalInput = mousePositionDelta.y;

            float rotationX = verticalInput * rotationSpeed * 10f * Time.deltaTime;
            float rotationY = horizontalInput * rotationSpeed * 10f *Time.deltaTime;

            Vector3 currentRotation = transform.eulerAngles;

            float newRotationX = currentRotation.x - rotationX;

            if (newRotationX >= 2f && newRotationX <= 80f)
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
        Vector3 target = LocalBoardManager.Instance.GetBoardMiddlePosAtYLevel(cameraHeight);

        float scrollDelta = -Input.mouseScrollDelta.y;

        Vector3 directionToTarget = target - transform.position;

        float currentDistance = directionToTarget.magnitude;

        float newDistance = Mathf.Clamp(currentDistance + scrollDelta * zoomSpeed * 100f * Time.deltaTime, maxZoomIn, maxZoomOut);

        Vector3 newPosition = target - directionToTarget.normalized * newDistance;

        transform.position = newPosition;
    }

    public void ToggleCameraCanMove(bool toggle) { canMove = toggle; }

    public int MoveCameraUpOneBoardLevel()
    {
        if (cameraHeight + 1 >= LocalBoardManager.Instance.yOfCurrentHeighestBuiltBlock)
        {
            cameraHeight = LocalBoardManager.Instance.yOfCurrentHeighestBuiltBlock;
        }
        else
        {
            cameraHeight += 1;
        }

        return cameraHeight;
    }

    public int MoveCameraDownOneBoardLevel()
    {
        if (cameraHeight - 1 <= 1)
        {
            cameraHeight = 1;
        }
        else
        {
            cameraHeight -= 1;
        }

        return cameraHeight;
    }
}

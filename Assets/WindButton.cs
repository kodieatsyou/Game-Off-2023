using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WindDir
{
    North,
    East,
    South,
    West
}

public class WindButton : MonoBehaviour
{
    public WindDir dir;
    public void Initialize(WindDir dir)
    {
        this.dir = dir;
        SetPositionAndRotation();
        gameObject.SetActive(false);
    }

    void SetPositionAndRotation()
    {
        double offset1 = (Board.Instance.baseSize + (2.5f / 2.0f));
        double offset2 = ((Board.Instance.baseSize * 2.5f) + offset1);
        float yPos = Board.Instance.GetBoardMiddlePosAtYLevel(Board.Instance.heightSize / 2).y;

        switch (dir)
        {
            case WindDir.North:
                gameObject.transform.position = new Vector3((float)-offset1, yPos, (float)offset1);
                gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                break;
            case WindDir.East:
                gameObject.transform.position = new Vector3((float)offset1, yPos, (float)offset2);
                gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
                break;
            case WindDir.South:
                gameObject.transform.position = new Vector3((float)offset2, yPos, (float)offset1);
                gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                break;
            case WindDir.West:
                gameObject.transform.position = new Vector3((float)offset1, yPos, -(float)offset1);
                gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

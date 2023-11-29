using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindParticle : MonoBehaviour
{
    public ParticleSystem[] particles;

    private float playTime;
    private bool isPlaying = false;

    void Update() {
        if(transform.childCount <= 0) {
            Destroy(gameObject);
        }
    }

    public GameObject InitializeWindParticles(float timeToPlay, WindDir dir) {
        this.playTime = timeToPlay;
        isPlaying = true;
        SetPositionAndRotation(dir);
        foreach (ParticleSystem ps in particles) {
            var tempShapeModule = ps.shape;
            tempShapeModule.scale = new Vector3(Board.Instance.baseSize * 2.5f, Board.Instance.heightSize * 2.5f, Board.Instance.baseSize * 2.5f);
        }

        return gameObject;
    }

    void SetPositionAndRotation(WindDir dir)
    {
        double offset1 = (Board.Instance.baseSize + (2.5f / 2.0f));
        double offset2 = ((Board.Instance.baseSize * 2.5f) + offset1);
        float yPos = Board.Instance.GetBoardMiddlePosAtYLevel(Board.Instance.heightSize / 2).y;

        switch (dir)
        {
            case WindDir.North:
                transform.position = new Vector3((float)offset2, yPos, (float)offset1);
                transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                break;
            case WindDir.East:
                transform.position = new Vector3((float)offset1, yPos, -(float)offset1);
                transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
                break;
            case WindDir.South:
                transform.position = new Vector3((float)-offset1, yPos, (float)offset1);
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                break;
            case WindDir.West:
                transform.position = new Vector3((float)offset1, yPos, (float)offset2);
                transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
                break;
        }
    }
}

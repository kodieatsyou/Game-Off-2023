using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SelectionMode
{
    Build,
    Move,
    Player,
    Grapple,
    Ninja,
    None
}

public class Board : MonoBehaviour
{
    public static Board Instance;
    public BoardSpace[,,] boardArray;
    public int baseSize;
    public int heightSize;
    public int yOfCurrentHeighestBuiltBlock = 0;
    public SelectionMode selectionMode;
    public List<BoardSpace> selectedSpaces;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        
    }

    public void InitializeBoard(bool[,,] initialBoard, int baseSize, int heightSize)
    {
        selectionMode = SelectionMode.None;
        boardArray = new BoardSpace[baseSize, heightSize, baseSize];
        this.baseSize = baseSize;
        this.heightSize = heightSize;
        for (int x = 0; x < baseSize; x++)
        {
            for (int y = 0; y < heightSize; y++)
            {
                for (int z = 0; z < baseSize; z++)
                {
                    boardArray[x, y, z] = Instantiate(GameAssets.i.board_space_object_, transform).GetComponent<BoardSpace>().InitializeSpace(new Vector3(x, y, z), 2.5f, initialBoard[x, y, z]);
                }
            }
        }
        MakeWindButtons();
        CameraController.Instance.InitializeCamera();
    }

    void MakeWindButtons()
    {
        foreach (WindDir dir in Enum.GetValues(typeof(WindDir)))
        {
            GameObject b = Instantiate(GameAssets.i.wind_button_);
            b.GetComponent<WindButton>().Initialize(dir);
            UIController.Instance.RegisterWindButton(b);
        }
    }

    public Vector3 GetBoardMiddlePosAtYLevel(int yLevel)
    {
        if (Mathf.Floor(baseSize / 2.0f) == baseSize / 2.0f) //Check if the middle of the grid is a float or not. If its a whole number the middle is between 2 blocks else the middle is a whole block. 
        {
            return new Vector3(((baseSize / 2.0f) * 2.5f) - 1, yLevel * 2.5f, ((baseSize / 2.0f) * 2.5f) - 1); // Subtract one to get the location between the two "middle" blocks

        }
        else
        {
            return new Vector3((baseSize / 2.0f) * 2.5f, yLevel * 2.5f, (baseSize / 2.0f) * 2.5f); // Get the location of the center of the middle block
        }
    }

    public void ConfirmAction()
    {
        switch(selectionMode)
        {
            case SelectionMode.None:
                break;
            case SelectionMode.Build:
                UIController.Instance.ToggleBuildButton(false);
                PlayerController.Instance.ActionsRemaining -= 1;
                BuildSelected();
                break;
            case SelectionMode.Move:
                UIController.Instance.ToggleMoveButton(false);
                PlayerController.Instance.ActionsRemaining -= 1;
                MoveToSelected();
                break;
            case SelectionMode.Grapple:
                UIController.Instance.ToggleGrappleButton(false);
                PlayerController.Instance.ActionsRemaining -= 1;
                GrappleToSelected();
                break;
        }
    }

    public List<BoardSpace> GetPlayerObjectsAroundSpace(BoardSpace space) {
        List<BoardSpace> spacesWithPlayers = new List<BoardSpace>();
        int checkX = (int)space.GetPosInBoard().x;
        int checkZ = (int)space.GetPosInBoard().z;
        for(int x = checkX - 1; x < checkX + 2; x++) {
            for(int z = checkZ - 1; z < checkZ + 2; z++) {
                /*if(x == (int)space.GetPosInBoard().x && z == (int)space.GetPosInBoard().z) {
                    continue;
                }*/
                //Debug.Log("Checking space: " + x + " " + (int)space.GetPosInBoard().y + " " + z + " for players.");
                if((x >= 0 && x < baseSize) && (z >= 0 && z < baseSize)) {
                    if(boardArray[x, (int)space.GetPosInBoard().y, z].GetPlayerObjOnSpace() != null) {
                        spacesWithPlayers.Add(boardArray[x, (int)space.GetPosInBoard().y, z]);
                    }
                }
            }
        }
        return spacesWithPlayers;
    }

    void MoveToSelected()
    {
        if(selectedSpaces.Count == 1)
        {
            PlayerController.Instance.MoveTo(selectedSpaces[0]);
            selectedSpaces[0].SetIsSelected(false);
        }
        selectedSpaces.Clear();
        selectionMode = SelectionMode.None;
    }

    void GrappleToSelected()
    {
        if(selectedSpaces.Count == 1)
        {
            PlayerController.Instance.GrappleToBlock(selectedSpaces[0]);
            selectedSpaces[0].SetIsSelected(false);
        }
        selectedSpaces.Clear();
        selectionMode = SelectionMode.None;
    }
    void BuildSelected()
    {
        foreach (BoardSpace block in selectedSpaces)
        {
            block.SetIsSelected(false);
            BoardManager.Instance.BMPhotonView.RPC("BoardManagerSetSpaceIsBuilt", RpcTarget.All, block.GetPosInBoard(), true);
        }
        selectedSpaces.Clear();
        selectionMode = SelectionMode.None;
    }

    public void ClearAction()
    {
        ClearSelected();
        if(selectionMode == SelectionMode.Build) 
        {
            PlayerController.Instance.blocksLeftToPlace = 2;
        }
    }

    public void ClearSelected()
    {
        foreach (BoardSpace block in selectedSpaces)
        {
            block.SetIsSelected(false);
        }
        selectedSpaces.Clear();
    }

    public bool SelectBlock(BoardSpace blockSelected)
    {
        if(!PlayerController.Instance.moving)
        {
            switch (selectionMode)
            {
                case SelectionMode.Build:
                    if (PlayerController.Instance.blocksLeftToPlace > 0)
                    {
                        PlayerController.Instance.blocksLeftToPlace -= 1;
                        selectedSpaces.Add(blockSelected);
                        UIController.Instance.ToggleActionPanelConfirm(true);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case SelectionMode.Move:
                    if (selectedSpaces.Count == 0)
                    {
                        selectedSpaces.Add(blockSelected);
                        return true;
                    }
                    return false;
                case SelectionMode.Grapple:
                    if (selectedSpaces.Count == 0)
                    {
                        selectedSpaces.Add(blockSelected);
                        return true;
                    }
                    return false;
            }
        }
        return false;
    }

    public bool UnSelectBlock(BoardSpace blockUnSelected)
    {
        if(selectedSpaces.Count > 0 && !PlayerController.Instance.moving)
        {
            switch (selectionMode)
            {
                case SelectionMode.Build:
                    if (PlayerController.Instance.blocksLeftToPlace + 1 <= 2)
                    {
                        PlayerController.Instance.blocksLeftToPlace += 1;
                        selectedSpaces.Remove(blockUnSelected);
                        if (selectedSpaces.Count == 0)
                        {
                            UIController.Instance.ToggleActionPanelConfirm(false);
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case SelectionMode.Move:
                    selectedSpaces.Remove(blockUnSelected);
                    UIController.Instance.ToggleActionPanelConfirm(false);
                    return true;
                case SelectionMode.Grapple:
                    selectedSpaces.Remove(blockUnSelected);
                    UIController.Instance.ToggleActionPanelConfirm(false);
                    return true;
            }
            return false;
        }
        return false;
    }

    public BoardSpace GetWindPushBlock(BoardSpace curSpace, WindDir dir) {
        Vector3 curBoardPos = curSpace.GetPosInBoard();
        switch (dir)
        {
            case WindDir.North:
                if(curBoardPos.x == 0 || boardArray[(int)curBoardPos.x - 1, (int)curBoardPos.y + 1, (int)curBoardPos.z].GetIsBuilt()) {
                    return curSpace;
                } 
                curBoardPos = FindNextValidYGoingDown(new Vector3(curBoardPos.x - 1, curBoardPos.y, curBoardPos.z));
                return boardArray[(int)curBoardPos.x, (int)curBoardPos.y, (int)curBoardPos.z];
            case WindDir.East:
                if(curBoardPos.z == baseSize - 1 || boardArray[(int)curBoardPos.x, (int)curBoardPos.y + 1, (int)curBoardPos.z + 1].GetIsBuilt()) {
                    return curSpace;
                } 
                curBoardPos = FindNextValidYGoingDown(new Vector3(curBoardPos.x, curBoardPos.y, curBoardPos.z + 1));
                return boardArray[(int)curBoardPos.x, (int)curBoardPos.y, (int)curBoardPos.z];
            case WindDir.South:
                if(curBoardPos.x == baseSize - 1 || boardArray[(int)curBoardPos.x + 1, (int)curBoardPos.y + 1, (int)curBoardPos.z].GetIsBuilt()) {
                    return curSpace;
                } 
                curBoardPos = FindNextValidYGoingDown(new Vector3(curBoardPos.x + 1, curBoardPos.y, curBoardPos.z));
                return boardArray[(int)curBoardPos.x, (int)curBoardPos.y, (int)curBoardPos.z];
            case WindDir.West:
                if(curBoardPos.z == 0 || boardArray[(int)curBoardPos.x, (int)curBoardPos.y + 1, (int)curBoardPos.z - 1].GetIsBuilt()) {
                    return curSpace;
                } 
                curBoardPos = FindNextValidYGoingDown(new Vector3(curBoardPos.x, curBoardPos.y, curBoardPos.z - 1));
                return boardArray[(int)curBoardPos.x, (int)curBoardPos.y, (int)curBoardPos.z];
        }
        return null;
    }


    public BoardSpace GetPunchedBlock(BoardSpace puncherSpot, BoardSpace mySpot) {
        Vector3 direction = -(puncherSpot.GetPosInBoard() - mySpot.GetPosInBoard());
        direction.Normalize(); 

        BoardSpace currentSpace = mySpot;

        while (currentSpace != null) {
            // Get the position of the next space in the direction
            Vector3 nextPos = currentSpace.GetPosInBoard() + direction;
            if(nextPos.x < 0 || nextPos.x >= baseSize || nextPos.z < 0 || nextPos.z >= baseSize) {
                return null;
            }
            // Get the next space using the position
            BoardSpace nextSpace = boardArray[(int)nextPos.x, (int)nextPos.y, (int)nextPos.z];

            // Check if the next space is built or the block below is not built
            if (nextSpace != null && (nextSpace.GetIsBuilt() || !nextSpace.GetBelowBlock().GetIsBuilt())) {
                Vector3 blockFallDown = FindNextValidYGoingDown(nextSpace.GetPosInBoard());
                return boardArray[(int)blockFallDown.x, (int)blockFallDown.y, (int)blockFallDown.z]; // Found a suitable space
            }

            currentSpace = nextSpace;
        }
        return null;
    }


    Vector3 FindNextValidYGoingDown(Vector3 positionToCheckFrom) {
        while(!Board.Instance.boardArray[(int)positionToCheckFrom.x, (int)positionToCheckFrom.y, (int)positionToCheckFrom.z].GetIsBuilt() && positionToCheckFrom.y != 0) {
            positionToCheckFrom = new Vector3(positionToCheckFrom.x, positionToCheckFrom.y - 1, positionToCheckFrom.z);
        }
        return positionToCheckFrom;
    }
}

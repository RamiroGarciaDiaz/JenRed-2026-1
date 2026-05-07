using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class PlayerInput : MonoBehaviourPun
{
    void Update()
    {
        if (photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.AutomaticallySyncScene = false;
                SceneManager.LoadScene("StartingScene");
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CustomTransformSync : MonoBehaviourPun, IPunObservable
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        throw new System.NotImplementedException();
    }
}

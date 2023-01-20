using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PhotonMoveSystem : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
        float speed = 3f;
    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine){
            //上下方向はsin30度倍する
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            transform.position += new Vector3(x * speed * Time.deltaTime, 0, z*0.5f* speed * Time.deltaTime);
        }
        
    }
    
}

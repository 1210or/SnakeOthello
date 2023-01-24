using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerNameDisplay : MonoBehaviourPunCallbacks
{
    private void Start()
    {                
        var nameLabel = this.GetComponent<Text>();
        nameLabel.text = photonView.Owner.NickName + "_" + ((int)(photonView.OwnerActorNr)-1);
    }    

    void Update()
    {
        //　カメラと同じ向きに設定
        this.transform.parent.gameObject.transform.rotation = Camera.main.transform.rotation;
    }
}
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
    //x軸方向の入力を保存
    Vector3 inputKey;  

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine){
            // WASD入力から、XZ平面(水平な地面)を移動する方向(velocity)を得ます
            inputKey = Vector3.zero;
            
            if(Input.GetKey(KeyCode.W))
                inputKey.z += 1;
            if(Input.GetKey(KeyCode.A))
                inputKey.x -= 1;
            if(Input.GetKey(KeyCode.S))
                inputKey.z -= 1;
            if(Input.GetKey(KeyCode.D))
                inputKey.x += 1;

            //移動の向きなど座標関連はVector3で扱う
            Vector3 velocity = new Vector3(inputKey.x, 0, inputKey.z).normalized;

            //45度を30度に変換 角度だからそのままベクトルにかけれない
            float convertRot = (0.125f * Mathf.Cos(4 * (Mathf.Atan2(velocity.z, velocity.x))) + 0.875f);            
            
            //ベクトルの向きを取得 //zに角度変換をかける
            Vector3 direction = new Vector3(inputKey.x, 0, inputKey.z * convertRot).normalized;
           
            //移動距離を計算
            float distance = speed * Time.deltaTime;
            //移動先を計算
            Vector3 destination = transform.position + direction * distance;
            
            //移動先に向けて回転
            transform.LookAt(destination);
            //移動先の座標を設定
            transform.position = destination;
        }
        
    }
    
}

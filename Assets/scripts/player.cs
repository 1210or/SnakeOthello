using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{    
    //プレイヤーの属性(チーム)のための変数
    public int playerPowerValue = 0;

    //チームカラー
    public Color teamColor;

    //プレイヤーカラー
    public Color playerColor;

    //塗る色
    public Color paintColor;

    //初期位置
    public Vector3 firstPosition;

    //直前にいたマスを保存するための変数
    public GameObject solverGameObject = null;

    //新しいマスに入った瞬間のフラグ
    public bool newHexaFlag = false;

    public static Player instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public int playerID;
    // Start is called before the first frame update
    void Start()
    {
        playerID = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        //プレイヤーの子供オブジェクトの色を変える
        this.transform.Find("player").gameObject.GetComponent<Renderer>().material.color = playerColor;
    }

    // Update is called once per frame
    void Update()
    {
        //マスを塗る

        // 現在の位置から下(0,-1,0)に向かってRayをセット
        Ray ray = new Ray(transform.position + new Vector3(0,0.1f,0),Vector3.down);
        // Rayが当たった相手を保存する変数
        RaycastHit hit;

        // Rayを10.0fの距離まで発射。何かに当たればhitで受け取る
        if(Physics.Raycast(ray, out hit, 10.0f)) {
            // もし当たった相手のタグがstageなら下の足場の色を変える
            if(hit.collider.tag == "stage") {
                //プレイヤーが2人ならステージパワー値を変更
                if(PhotonNetwork.PlayerList.Length == 2 || GameManager.instance.isDebug == true){ 
                
                    //もし直前のステージと足元のステージが違うならば(新しいマスに入ったら)
                    if(hit.collider.gameObject.GetComponent<Stage>().stagePowerValue != playerPowerValue){
    
                        //マスの変数を変更する
                        hit.collider.gameObject.GetComponent<Stage>().isPlayerOn = true;
                        try{solverGameObject.GetComponent<Stage>().isPlayerOn =  false;}catch(System.Exception){/*何もしない*/}                      
                        
                        hit.collider.gameObject.GetComponent<Stage>().stagePowerValue = playerPowerValue;      
                    }
                    //solverGameObject = hit.collider.gameObject;
                }   
            }
        }


        if(this.transform.root.gameObject.transform.position.y < -2){
            
            //初期マスにプレイヤーをとばす
            this.transform.root.gameObject.transform.position = firstPosition + new Vector3(0, 2, 0);
        
        }
    }

//名前空間、継承、コンポーネントアタッチ必須、順番通りに送受信されるので順番が大切
//同期
    void IPunObservable.OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo i_info )
    {
        if(stream.IsWriting )
        {
            //データの送信
            //プレイヤーカラー
            
            stream.SendNext(this.GetComponent<Player>().playerColor.r);
            stream.SendNext(this.GetComponent<Player>().playerColor.g);
            stream.SendNext(this.GetComponent<Player>().playerColor.b);
            //stream.SendNext(this.GetComponent<Player>().playerColor.a);

            //初期位置
            stream.SendNext(this.GetComponent<Player>().firstPosition);
            
            //パワー値
            stream.SendNext(this.GetComponent<Player>().playerPowerValue);

            
            //チームカラー
            stream.SendNext(this.GetComponent<Player>().teamColor.r);
            stream.SendNext(this.GetComponent<Player>().teamColor.g);
            stream.SendNext(this.GetComponent<Player>().teamColor.b);

            //プレイヤーカラー
            stream.SendNext(this.GetComponent<Player>().playerColor.r);
            stream.SendNext(this.GetComponent<Player>().playerColor.g);
            stream.SendNext(this.GetComponent<Player>().playerColor.b);

            //塗る色
            stream.SendNext(this.GetComponent<Player>().paintColor.r);
            stream.SendNext(this.GetComponent<Player>().paintColor.g);
            stream.SendNext(this.GetComponent<Player>().paintColor.b);
        
            //stream.SendNext(this.GetComponent<Player>().solverGameObject);

        }
        else
        {            
            //データの受信
            //プレイヤーカラー            
            float r = (float)stream.ReceiveNext();
            float g = (float)stream.ReceiveNext();
            float b = (float)stream.ReceiveNext(); 

            //初期位置
            Vector3 firstPosition = (Vector3)stream.ReceiveNext();

            //パワー値        
            int playerPowerValue = (int)stream.ReceiveNext();

            float teamColorR = (float)stream.ReceiveNext();
            float teamColorG = (float)stream.ReceiveNext();
            float teamColorB = (float)stream.ReceiveNext();

            float playerColorR = (float)stream.ReceiveNext();
            float playerColorG = (float)stream.ReceiveNext();
            float playerColorB = (float)stream.ReceiveNext();

            float paintColorR = (float)stream.ReceiveNext();
            float paintColorG = (float)stream.ReceiveNext();
            float paintColorB = (float)stream.ReceiveNext();

            //this.GetComponent<Player>().solverGameObject = (GameObject)stream.ReceiveNext();
            
            

            this.transform.Find("player").gameObject.GetComponent<Renderer>().material.color = new Color(r, g, b, 1);
            
            this.GetComponent<Player>().playerPowerValue = playerPowerValue;
            
            this.GetComponent<Player>().firstPosition = firstPosition;
            


            this.GetComponent<Player>().teamColor = new Color(teamColorR, teamColorG, teamColorB, 1);

            this.GetComponent<Player>().playerColor = new Color(playerColorR, playerColorG, playerColorB, 1);

            this.GetComponent<Player>().paintColor = new Color(paintColorR, paintColorG, paintColorB, 1);

            

        }
    }
}


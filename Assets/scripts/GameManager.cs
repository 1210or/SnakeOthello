using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;


public class GameManager : MonoBehaviourPunCallbacks
{
    //タイマーのための変数
    [SerializeField, Range(0, 100)]
    public float totalTime = 30;
    int seconds;
    
    //プレイヤー作成のための変数
    public GameObject player;
    public GameObject[] players = new GameObject[2];
    public List<GameObject> playersList = null;

    //スコアのための変数
    public int totalPowerCount = 0;

    public static GameManager instance;

    // Start is called before the first frame update
    void Awake()
    {   // PhotonServerSettingsの設定内容を使ってマスターサーバーへ接続する
        PhotonNetwork.ConnectUsingSettings();

         print("プレイヤーリストの人数" + playersList.Count + "人");

        //インスタンス生成
        if (instance == null)
        {
            instance = this;
        }   
     }
    // マスターサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnConnectedToMaster() {
        // "Room"という名前のルームに参加する（ルームが存在しなければ作成して参加する）
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions(), TypedLobby.Default);
    }

    

    // Update is called once per frame
    void Update()
    {   

        //esc終了
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        //時間制限と勝敗
        if(totalTime != 0){

            if(totalTime > 0){//ゲーム中
                //ゲームタイマー
                totalTime -= Time.deltaTime;
                seconds = (int)totalTime;
                print(seconds.ToString());  
            }else{//ゲーム終了
                //勝ち負け判定
                for(int z=0;z<StageMaker.stageObject.GetLength(0);z++) //zの大きさぶんループ
                {
                    for(int x=0;x<StageMaker.stageObject.GetLength(1);x++)//xの大きさぶんループ
                    {
                        totalPowerCount += StageMaker.stageObject[x,z].GetComponent<Stage>().stagePowerValue;
                    }
                }

                if(totalPowerCount > 0){
                    print("赤の勝ち");
                }
                if(totalPowerCount < 0){
                    print("青の勝ち");
                }            
                if(totalPowerCount == 0){
                    print("引き分け");
                }
            }
        }


        
        for(int i = 0; i < playersList.Count; i++){ //プレイヤーの人数分まわす
            if(playersList[i].GetComponent<Player>().newHexaFlag == true){
                //print("どちらかがマスを移動した");
                break;
            }
        }
    }
    // ゲームサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnJoinedRoom() {
        
        //プレイヤーの人数が2人以下なら
        if(PhotonNetwork.PlayerList.Length < 3){
            
            //配列のインデックスをプレイヤーの人数から定義
            int i = PhotonNetwork.PlayerList.Length - 1;
            
            //初期位置を格納
            Vector3 firstPosition = StageMaker.stageObject[i * (StageMaker.stageSizeX - 1),i * (StageMaker.stageSizeZ - 1)].transform.position;
            
            //プレイヤーの人スタンスを生成
            GameObject player = PhotonNetwork.Instantiate("Player", firstPosition + new Vector3(0, 2, 0) , new Quaternion(1,0,0,180)); 

            
            //初期位置を決める
            player.GetComponent<Player>().firstPosition = firstPosition;

            //これが同期されていない
            playersList.Add(player);

            //プレイヤーの名前を変える
            player.name = "Player" + (i + 1) as string;

            //プレイヤーの属性(チーム)を決める
            player.GetComponent<Player>().playerPowerValue =  1 - (i*2); //1人目が1, 2人めが-1になる

            //初期位置のステージにパワー値を設定
            StageMaker.stageObject[i * (StageMaker.stageSizeX - 1),i * (StageMaker.stageSizeZ - 1)].GetComponent<Stage>().stagePowerValue = player.GetComponent<Player>().playerPowerValue;

            //プレイヤーのチーム色を決める
            player.GetComponent<Player>().teamColor = new Color(1-i,0, i); //1人目は赤, 2人目は青

            //プレイヤー自身の色を変える
            player.GetComponent<Player>().playerColor = player.GetComponent<Player>().teamColor * 0.4f + new Color(0.7f, 0.7f, 0.7f);
                
            //プレイヤーのマスの色を決める
            player.GetComponent<Player>().paintColor = player.GetComponent<Player>().teamColor * 0.3f + new Color(0.1f, 0.1f, 0.1f);

         }
        }
}

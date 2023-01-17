using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class GameManager : MonoBehaviour
{
    //タイマーのための変数
    [SerializeField, Range(0, 100)]
    public float totalTime = 30;
    int seconds;
    
    //プレイヤー作成のための変数
    public GameObject player;
    public List<GameObject> players;

    //スコアのための変数
    public int totalPowerCount = 0;

    public static GameManager instance;

    // Start is called before the first frame update
    void Awake()
    {/*
        //プレイヤーを2人生成、プレイヤーメンバ変数を設定していく
        for(int i = 0;i < players.Length; i++){
            //プレイヤーを複製する
            players[i] = Instantiate(player, StageMaker.stageObject[i * (StageMaker.stageSizeX - 1),i * (StageMaker.stageSizeZ - 1)].transform.position + new Vector3(0, 2, 0) , new Quaternion(1,0,0,180)); 
            
            //初期位置を格納
            players[i].GetComponent<Player>().firstPosition = StageMaker.stageObject[i * (StageMaker.stageSizeX - 1),i * (StageMaker.stageSizeZ - 1)].transform.position + new Vector3(0, 2, 0);

            //プレイヤーの名前を変える
            players[i].name = "Player" + (i + 1) as string;

            //プレイヤーの属性(チーム)を決める
            players[i].GetComponent<Player>().playerPowerValue =  1 - (i*2); //1人目が1, 2人めが-1になる

            //初期位置のステージにパワー値を設定
            StageMaker.stageObject[i * (StageMaker.stageSizeX - 1),i * (StageMaker.stageSizeZ - 1)].GetComponent<Stage>().stagePowerValue = players[i].GetComponent<Player>().playerPowerValue;

            //プレイヤーのチーム色を決める
            players[i].GetComponent<Player>().teamColor = new Color(1-i,0, i); //1人目は赤, 2人目は青

            //プレイヤー自身の色を変える
            players[i].GetComponent<Player>().playerColor = players[i].GetComponent<Player>().teamColor * 0.4f + new Color(0.7f, 0.7f, 0.7f);
            
            //プレイヤーのマスの色を決める
            players[i].GetComponent<Player>().paintColor = players[i].GetComponent<Player>().teamColor * 0.3f + new Color(0.1f, 0.1f, 0.1f);
            
            //プレイヤー2を十字キー操作に切り替え
            if(i == 1){
                players[i].GetComponent<MoveControl>().isWasd = false;
            }
        } */

        //インスタンス生成
        if (instance == null)
        {
            instance = this;
        }   
     }


    

    // Update is called once per frame
    void Update()
    {


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


        
        for(int i = 0; i < players.Count; i++){ //プレイヤーの人数分まわす
            if(players[i].GetComponent<Player>().newHexaFlag == true){
                //print("どちらかがマスを移動した");
                break;
            }
        }
    }
}

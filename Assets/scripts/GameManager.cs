using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [SerializeField, Range(1, 100)]
    public float totalTime = 30;
    int seconds;

    public GameObject[] players;
    
    public int totalPowerCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        print(players.Length);
    }

    // Update is called once per frame
    void Update()
    {
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
          
        
        //if(players.GetComponent<Player>().newHexaFlag.Contains(true)){
          //  print("どちらかがマスを移動した")
        //}
        
        for(int i = 0; i < players.Length; i++){ //プレイヤーの人数分まわす
            if(players[i].GetComponent<Player>().newHexaFlag == true){
                //print("どちらかがマスを移動した");
                break;
            }
        }
    }
}

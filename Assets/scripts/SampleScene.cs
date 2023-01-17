using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

// MonoBehaviourPunCallbacksを継承して、PUNのコールバックを受け取れるようにする
public class SampleScene : MonoBehaviourPunCallbacks
{
    private void Start() {
        // PhotonServerSettingsの設定内容を使ってマスターサーバーへ接続する
        PhotonNetwork.ConnectUsingSettings();
    }

    // マスターサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnConnectedToMaster() {
        // "Room"という名前のルームに参加する（ルームが存在しなければ作成して参加する）
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions(), TypedLobby.Default);
    }

    // ゲームサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnJoinedRoom() {
        if(GameManager.instance.playersList.Count  < 2){
            
            GameObject player = PhotonNetwork.Instantiate("Player", new Vector3(0, 2, 0) , new Quaternion(1,0,0,180)); 
        
            
            GameManager.instance.playersList.Add(player);
            int i = GameManager.instance.playersList.Count - 1;

            print("プレイヤーリストの人数" + (i + 1) + "人");
            //初期位置を格納
            player.GetComponent<Player>().firstPosition = StageMaker.stageObject[i * (StageMaker.stageSizeX - 1),i * (StageMaker.stageSizeZ - 1)].transform.position + new Vector3(0, 2, 0);
            
            player.transform.position =  player.GetComponent<Player>().firstPosition;

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
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class Room : MonoBehaviour
{
    public Text buttonText;

    private RoomInfo info;

    public void RegisterRoomDetails(RoomInfo info)
    {
        //ルーム情報格納
        this.info = info;

        //UI
        buttonText.text = this.info.Name;
    }

    public void OpenRoom()
    {
        //ルーム参加関数を呼び出す
        TitleUiManager.instance.JoinRoom(info);
    }
}

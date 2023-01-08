using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    //public Color playerColor;
    public static Color playerColor = Color.red * 0.5f;
    

    //直前にいたステージを保存するための変数
    private GameObject solverGameObject;

    // Start is called before the first frame update
    void Start()
    {
        //playerColor = playerColor * 0.5f;
        this.GetComponent<Renderer>().material.color = playerColor * 2;
    }

    // Update is called once per frame
    void Update()
    {
         // 現在の位置から下(0,-1,0)に向かってRayをセット
        Ray ray = new Ray(transform.position + new Vector3(0,0.1f,0),Vector3.down);
        // Rayが当たった相手を保存する変数
        RaycastHit hit;

        // Rayを10.0fの距離まで発射。何かに当たればhitで受け取る
        if(Physics.Raycast(ray, out hit, 10.0f)) {
            // もし当たった相手のタグがstageなら下の足場の色を変える
            if(hit.collider.tag == "stage" || hit.collider.tag == "stageEdge" ) {

                //もし直前のステージと足元のステージが違うならば(新しいマスに入ったら)
                if(solverGameObject != hit.collider.gameObject){
                    hit.collider.gameObject.GetComponent<Renderer>().material.color = playerColor;
                    ChangeHexColor.arroundScanFlag = true;//フラグを立てる

                    //print("new");
                }
                solverGameObject = hit.collider.gameObject;
            }else
            {
                ChangeHexColor.arroundScanFlag = false;//フラグを立てる
            }
        }

        // 左に移動
        if (Input.GetKey (KeyCode.LeftArrow)) {
            this.transform.Translate (-0.01f,0.0f,0.0f);
        }
        // 右に移動
        if (Input.GetKey (KeyCode.RightArrow)) {
            this.transform.Translate (0.01f,0.0f,0.0f);
        }
        // 前に移動
        if (Input.GetKey (KeyCode.UpArrow)) {
            this.transform.Translate (0.0f,-0.01f,0.0f);
        }
        // 後ろに移動
        if (Input.GetKey (KeyCode.DownArrow)) {
            this.transform.Translate (0.0f,0.01f,0.0f);
        }
    }
}

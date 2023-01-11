using UnityEngine;

public class MoveControl : MonoBehaviour {
    //移動速度
    public float speed = 2.0f;

    public bool isWasd = false;

    //x軸方向の入力を保存
    private float _input_x;
    //z軸方向の入力を保存
    private float _input_z;

    void Update() {

//斜め移動が早いバグあり、ノーマライズしてないため
        if(isWasd == false){ //wasd操作
            if (Input.GetKey(KeyCode.UpArrow))
            {
                transform.position += speed * transform.forward * Time.deltaTime;
            }
    
            // Sキー（後方移動）
            if (Input.GetKey(KeyCode.DownArrow))
            {
                transform.position -= speed * transform.forward * Time.deltaTime;
            }
    
            // Dキー（右移動）
            if (Input.GetKey(KeyCode.RightArrow))
            {
                transform.position += speed * transform.right * Time.deltaTime;
            }
    
            // Aキー（左移動）
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                transform.position -= speed * transform.right * Time.deltaTime;
            }

        }else{ //十字キー
            if (Input.GetKey(KeyCode.W))
            {
                transform.position += speed * transform.forward * Time.deltaTime;
            }
    
            // Sキー（後方移動）
            if (Input.GetKey(KeyCode.S))
            {
                transform.position -= speed * transform.forward * Time.deltaTime;
            }
    
            // Dキー（右移動）
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += speed * transform.right * Time.deltaTime;
            }
    
            // Aキー（左移動）
            if (Input.GetKey(KeyCode.A))
            {
                transform.position -= speed * transform.right * Time.deltaTime;
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class changeHexColor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseDown() //クリックされると起動
  {
      print("click " + this.name); //オブジェクトの名前をプリント

      //ランダムにマテリアルカラーを変える
      this.GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value, 1.0f);
  }
}

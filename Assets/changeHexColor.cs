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

    void OnMouseDown()
  {
      print("click " + this.name);
      this.GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value, 1.0f);
  }
}

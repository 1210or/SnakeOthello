using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public static Attack instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public string heroName = "Attack";
    
    public void PassiveSkill()
    {
        print("test, this is Attack's passive skill");
    }

    public void ActiveSlill()
    {
        print("test, this is Attack's active skill");
    }
}

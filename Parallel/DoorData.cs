using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorData : MonoBehaviour
{
    public int dNum;
    public bool _openCheck = false;
    public Animator anim;
    
    private int fNum;

    void Start()
    {
        anim = this.GetComponent<Animator>();
        fNum = this.dNum;
    }

    void Update()
    {
        DoorOpenCheck();
    }

    public void DoorDataInit()
    {
        _openCheck = false;
        this.dNum = fNum;
    }

    public void DoorOpenCheck()
    {
        if(_openCheck == true)
        {
            anim.SetBool("openCheck", true);
        }
        else
        {
            anim.Rebind();
            anim.SetBool("openCheck", false);
        }
    }
}

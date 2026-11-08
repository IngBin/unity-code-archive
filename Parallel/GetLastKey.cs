using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetLastKey : UnitySingleton<GetLastKey>
{
    public int EscapeNum = 0; // 탈출 비밀번호

    public GameObject LockerMask; // 마스크 이미지
    public int resultNum = 0; // 플레이어가 입력할 비밀번호
    public static bool isGetLastKey = false;

    public GameObject[] getNumObj; // 비밀번호 
    public GameObject checkbtn;

    public Animator animator;

    public GameObject LockerObject;

    
    public string n1,n2,n3,n4 = null;

    void Update()
    {
        if(LockerMask.activeSelf == true)
        {
            checkbtn.SetActive(true);
        }
        else
        {
            checkbtn.SetActive(false);
        }
    }

    public void hwackinBtnClick()
    {
        n1 = getNumObj[0].GetComponent<LockScroll>().img[getNumObj[0].GetComponent<LockScroll>().minimgNum].transform.GetChild(0).GetComponent<Text>().text;
        n2 = getNumObj[1].GetComponent<LockScroll>().img[getNumObj[1].GetComponent<LockScroll>().minimgNum].transform.GetChild(0).GetComponent<Text>().text;
        n3 = getNumObj[2].GetComponent<LockScroll>().img[getNumObj[2].GetComponent<LockScroll>().minimgNum].transform.GetChild(0).GetComponent<Text>().text;
        n4 = getNumObj[3].GetComponent<LockScroll>().img[getNumObj[3].GetComponent<LockScroll>().minimgNum].transform.GetChild(0).GetComponent<Text>().text;
        
        resultNum = int.Parse(n1 + n2 + n3 + n4);

        if(EscapeNum == resultNum)
        {
            animator.SetBool("openCheck", true);
            LockerMask.SetActive(false);
            checkbtn.SetActive(false);
            ManagerCS.MInstance.lastCheck = true;
            ManagerCS.MInstance.numLock.SetActive(false);
            LockerObject.GetComponent<BoxCollider>().enabled = false;
        }
        else
        {
            LockerMask.SetActive(false);
        }
    }

    public void LastKeyInit()
    {
        isGetLastKey = false;
        resultNum = 0;
        EscapeNum = 0;
        LockerMask.SetActive(false);
        LockerObject.GetComponent<BoxCollider>().enabled = true;
        ManagerCS.MInstance.lastCheck = false;
        animator.SetBool("openCheck", false);
        ManagerCS.MInstance.CameraBtn.SetActive(true);
        ManagerCS.MInstance.ItemUIList.SetActive(true);
    }
}

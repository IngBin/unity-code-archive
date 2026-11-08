using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HorrorEvent : UnitySingleton<HorrorEvent>
{
    public static bool isGymEvent = false;
    public static bool isAnimStart = false;
    public static bool isGymClear = false;

    public Camera playerCamera,IronDoorCamera,WoodDoorCamera;

    public GameObject EscapeCol;

    // 문 닫히는 애니메이션
    public Animator HGymIronDoorAnim;
    public Animator HGymWoodDoorAnim;
    public Animator NGymIronDoorAnim;
    public Animator NGymWoodDoorAnim;
    //

    public GameObject gCanvas;

    void Start()
    {
        if(this.GetComponent<BoxCollider>().enabled == true)
        {
            EscapeCol.GetComponent<BoxCollider>().enabled = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            isGymEvent = true;
            gymEventStart();
            isAnimStart = true;
            TouchManager.Instance.joystick.playAnim();
            StartCoroutine(PlayGymEvnetAnimation());
            this.GetComponent<BoxCollider>().enabled = false;
            EscapeCol.GetComponent<BoxCollider>().enabled = true;
        }
    }

    void gymEventStart()
    {
        TouchManager.Instance.joystick.isTouch = false;
        TouchManager.Instance.joystick.GetComponent<Image>().raycastTarget = false;
        TouchManager.Instance.rotatePad.GetComponent<Image>().raycastTarget = false;
    }
    void Endanim()
    {
        TouchManager.Instance.joystick.GetComponent<Image>().raycastTarget = true;
        TouchManager.Instance.rotatePad.GetComponent<Image>().raycastTarget = true;
        isAnimStart = false;
    }

    IEnumerator PlayGymEvnetAnimation()
    {
        gCanvas.SetActive(false);

        playerCamera.gameObject.SetActive(false);
        IronDoorCamera.gameObject.SetActive(true);
        NGymIronDoorAnim.Play("NGymIronDoorClose");
        HGymIronDoorAnim.Play("HGymIronDoorClose");

        yield return new WaitForSeconds(1f);
        IronDoorCamera.gameObject.SetActive(false);
        WoodDoorCamera.gameObject.SetActive(true);
        ManagerCS.MInstance.Doors[6].GetComponent<DoorData>()._openCheck = false;
        ManagerCS.MInstance.Doors[7].GetComponent<DoorData>()._openCheck = false;

        yield return new WaitForSeconds(1f);
        WoodDoorCamera.gameObject.SetActive(false);
        playerCamera.gameObject.SetActive(true);

        gCanvas.SetActive(true);
        yield return new WaitUntil(() => playerCamera.enabled == true);
        Endanim();
        yield break;
    }

    public void HorrorEventInit()
    {
        isGymEvent = false;
        isAnimStart = false;
        isGymClear = false;
        this.GetComponent<BoxCollider>().enabled = true;
        EscapeCol.GetComponent<BoxCollider>().enabled = false;
    }
}

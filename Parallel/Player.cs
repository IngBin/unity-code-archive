using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public GameObject FirstFloor;
    public GameObject SecondFloor;
    public PlayableDirector Ending;
    public GameObject EndingCam;
    public GameObject playerCanvas;

    public GameObject demonic;
    public Transform demonicPos;
    public Transform demonicFirstPos;
    public GameObject PlayerCam;
    private bool demonicLaugh = false;
    private bool demonicStart = false;

    public GameObject dObj;
    public RaycastHit hit;
    // [HideInInspector]
    public int dData = 0;

    [SerializeField]
    private float rayDist = 1.3f;

    AudioSource demonicSound;
    float time;

    [SerializeField]
    Vector3 FirstPos;
    [SerializeField]
    Vector3 FirstRot;

    public static int playerHP = 1;

    public static float moveSpeed = 4f;

    int layerMask;

    Rigidbody rb;
    public bool yposCheck = false;
    [SerializeField]
    Transform CameraUpPosTr, CameraDownPosTr, CameraCenterPosTr;

   

    private void Awake()
    {
        demonicSound = demonic.GetComponent<AudioSource>();
    }

    void Start()
    {
        FirstPos = this.transform.position;
        FirstRot = TouchManager.Instance.rotatePad.anlge;
        layerMask = 1 << LayerMask.NameToLayer("door");
        rb = this.GetComponent<Rigidbody>();
        // StartCoroutine(walkEffect());
    }

    public void OnCollisionEnter(Collision col)
    {
        if (this.gameObject == col.transform.CompareTag("1F"))
        {
            ManagerCS.MInstance.FloorNum = 1;
            TestSc.instance.ChangeFirstFloor();
            ManagerCS.MInstance.ChangeFirstFloor();
            prowlGhost.instance.change1F();
            col.gameObject.SetActive(false);
            SecondFloor.SetActive(true);
        }
        else if (this.gameObject == col.transform.CompareTag("2F"))
        {
            ManagerCS.MInstance.FloorNum = 2;
            TestSc.instance.ChangeSecondFloor();
            ManagerCS.MInstance.ChangeSecondFloor();
            prowlGhost.instance.change2F();
            col.gameObject.SetActive(false);
            FirstFloor.SetActive(true);
        }
    }

    public void OnCollisionExit(Collision col)
    {
        if (this.gameObject == col.transform.CompareTag("DemonicCol"))
        {
            demonicStart = true;
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name == "YPos")
        {
            if(!yposCheck)
            {
                yposCheck = true;
            }
            this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "YPos")
        {
            yposCheck = false;
            this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        }
    }

    void Update()
    {

        if (ManagerCS.MInstance.PlayingCheck)
        {
            if(!yposCheck)
                rb.velocity = Vector3.zero;

            if (!HorrorEvent.isAnimStart)
            {
                Vector3 rotateValue = TouchManager.Instance.rotatePad.anlge;
                transform.GetChild(0).rotation = Quaternion.Euler(rotateValue.x, rotateValue.y + 180, rotateValue.z);
                transform.rotation = Quaternion.Euler(transform.rotation.x, rotateValue.y + 180, transform.rotation.z);
            }
            // 문 확인
            
            if (Physics.Raycast(transform.GetChild(0).position, transform.GetChild(0).forward, out hit, rayDist, layerMask) && hit.transform.GetComponent<DoorData>())
            {
                dObj = hit.transform.gameObject;
                if (!hit.transform.GetComponent<DoorData>()._openCheck)
                {
                    ManagerCS.MInstance.Btn_Interact.transform.GetChild(0).GetComponent<Text>().text = "열기";
                }
                else
                {
                    ManagerCS.MInstance.Btn_Interact.transform.GetChild(0).GetComponent<Text>().text = "닫기";
                }
                ManagerCS.MInstance.InteractableControl();
                // dData = hit.transform.GetComponent<DoorData>().dNum;
            }
            else
            {
                ManagerCS.MInstance.InteractableControl(false);
                dObj = null;
                dData = 0;
            }

            if (Physics.Raycast(transform.GetChild(0).position, transform.GetChild(0).forward, out hit, rayDist) && hit.transform.GetComponent<ItemCS>())
            {
                ManagerCS.MInstance.InteractableControl();
                dObj = hit.transform.gameObject;
                ManagerCS.MInstance.Btn_Interact.transform.GetChild(0).GetComponent<Text>().text = "줍기";
            }

            // 마지막 나가는 문 열쇠 캐비넷 확인
            if (Physics.Raycast(transform.GetChild(0).position, transform.GetChild(0).forward, out hit, rayDist) && hit.transform.GetComponent<GetLastKey>())
            {
                dObj = hit.transform.gameObject;
                if(GetLastKey.isGetLastKey == true)
                {
                    ManagerCS.MInstance.InteractableControl();
                    ManagerCS.MInstance.Btn_Interact.transform.GetChild(0).GetComponent<Text>().text = "풀기";
                }
                else
                {
                    ManagerCS.MInstance.CaptionList(4);
                }
            }

            // 안움직이면 나오는 인형
            if (TouchManager.Instance.joystick.isTouch == false && demonicStart == true)
            {
                time += Time.deltaTime;
                if (time >= 10.0f)
                {
                    Vector3 dir = this.transform.position - demonic.transform.position;
                    demonic.transform.LookAt(this.transform.GetChild(0).position);
                    demonic.transform.position = Vector3.MoveTowards(demonic.transform.position, demonicPos.transform.position, 0.05f);
                    demonic.SetActive(true);
                    if (!demonicLaugh)
                    {
                        demonicSound.Play();
                        demonicLaugh = true;
                    }
                }
            }
            else if (TouchManager.Instance.joystick.isTouch == true)
            {
                time = 0;
                demonic.SetActive(false);
                demonic.transform.position = demonicFirstPos.transform.position;
                if (demonicLaugh)
                {
                    demonicSound.Play();
                    demonicLaugh = false;
                }
            }
        }
    }

    void FixedUpdate()
    {
        if(ManagerCS.MInstance.PlayingCheck)
        {
            if (!HorrorEvent.isAnimStart)
            {
                if (TouchManager.Instance.joystick.isTouch)
                {
                    Vector3 joyValue = TouchManager.Instance.joystick.JoyDirection;
                    Vector3 playerDirection = (transform.forward * joyValue.y + transform.right * joyValue.x).normalized;
                    transform.position += playerDirection * moveSpeed * Time.deltaTime;
                }
            }
        }
    }

    public void PlayerInit()
    {
        this.transform.position = FirstPos;
        TouchManager.Instance.rotatePad.anlge = FirstRot;
        TouchManager.Instance.rotatePad.xAngle = 0;
        TouchManager.Instance.rotatePad.yAngle = 0;

        this.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

        yposCheck = false;

        SecondFloor.SetActive(false);
        FirstFloor.SetActive(true);
    }

    IEnumerator walkEffect()
    {
        while (true)
        {
            if (ManagerCS.MInstance.cameraEffectCheck.isOn == true)
            {
                if(TouchManager.Instance.joystick.isTouch)
                {
                    float CamYpos = PlayerCam.transform.position.y;

                    while (CamYpos < CameraUpPosTr.transform.position.y)
                    {
                        CamYpos += 0.01f / 1.5f;
                        yield return new WaitForSeconds(0.01f);
                        PlayerCam.transform.position = new Vector3(this.transform.position.x, CamYpos, this.transform.position.z);
                    }

                    CamYpos = CameraUpPosTr.transform.position.y;
                    yield return new WaitForSeconds(0.1f);

                    while (CamYpos > CameraDownPosTr.transform.position.y)
                    {
                        CamYpos -= 0.01f / 1.5f;
                        yield return new WaitForSeconds(0.01f);
                        PlayerCam.transform.position = new Vector3(this.transform.position.x, CamYpos, this.transform.position.z);
                    }

                    CamYpos = CameraDownPosTr.transform.position.y;
                    yield return new WaitForSeconds(0.1f);
                }
                else
                {
                    PlayerCam.transform.position = new Vector3(this.transform.position.x, CameraCenterPosTr.transform.position.y, this.transform.position.z);
                    yield return null;
                }
            }
            else
            {
                PlayerCam.transform.position = new Vector3(this.transform.position.x, CameraCenterPosTr.transform.position.y, this.transform.position.z);
                yield return null;
            }
        }
    }
}
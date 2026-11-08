using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Expand;

public class prowlGhost : MonoBehaviour
{
    public static prowlGhost instance;

    private float prowlSpeed = 1.5f;
    private NavMeshAgent nvAgent;
    [SerializeField]
    private int waypoint;
    private Animator animator;
    private bool checkFloor = false;

    public GameObject player;
    public GameObject ghost1F;
    public GameObject ghost2F;
    public Camera ghostcam;

    public List<Transform> patrolPoint1F = new List<Transform>();
    public List<Transform> patrolPoint2F = new List<Transform>();
    public Transform door;
    public DoorData d1, d2;
    public Transform dt1, dt2;
    List<Transform> changeFloor;

    void Awake()
    {
        instance = this;
        d1 = door.FindName<DoorData>("KitchenDoor");
        dt1 = door.FindName<Transform>("KitchenDoor");
        d2 = door.FindName<DoorData>("2FBigDoor");
        dt2 = door.FindName<Transform>("2FBigDoor");
    }

    void Start()
    {
        nvAgent = this.GetComponent<NavMeshAgent>();
        nvAgent.speed = prowlSpeed;
        animator = GetComponent<Animator>();
        changeFloor = patrolPoint2F;

        StartCoroutine(patorl());
    }

    void Update()
    {
        if (ManagerCS.MInstance.PlayingCheck)
        {
            nvAgent.isStopped = false;
            if (ManagerCS.MInstance.NightMap.active && !ManagerCS.MInstance.HorrorMap.active && !HorrorEvent.isGymEvent)
            {
                if (checkFloor)
                {
                    for (int i = 0; i < this.transform.childCount; i++)
                    {
                        if (this.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>() != null)
                        {
                            ghost2F.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>().enabled = false;
                            ghost1F.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>().enabled = true;
                        }
                    }
                    this.GetComponent<SphereCollider>().enabled = true;
                }
                else
                {
                    for (int i = 0; i < this.transform.childCount; i++)
                    {
                        if (this.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>() != null)
                        {
                            ghost2F.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>().enabled = true;
                            ghost1F.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>().enabled = false;
                        }
                    }
                    this.GetComponent<SphereCollider>().enabled = true;
                }
            }
            else
            {
                this.GetComponent<SphereCollider>().enabled = false;
                for (int i = 0; i < this.transform.childCount; i++)
                {
                    if (this.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>() != null)
                    {
                        ghost2F.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>().enabled = false;
                        ghost1F.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>().enabled = false;
                    }
                }
            }
        }
        else
        {
            nvAgent.isStopped = true;

            for (int i = 0; i < this.transform.childCount; i++)
            {
                if (this.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>() != null)
                {
                    ghost2F.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>().enabled = false;
                }
            }
            this.GetComponent<SphereCollider>().enabled = false;
        }
    }

    public void change1F()
    {
        changeFloor = patrolPoint1F;
        checkFloor = true;
        for(int i=0; i< this.transform.childCount; i++)
        {
            if(this.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>() != null)
            {
                ghost2F.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>().enabled = false;
                ghost1F.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>().enabled = true;
            }
        }
        ghost1F.transform.position = changeFloor[Random.Range(2,10)].transform.position;
    }

    public void change2F()
    {
        changeFloor = patrolPoint2F;
        checkFloor = false;
        for(int i=0; i< this.transform.childCount; i++)
        {
            if(this.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>() != null)
            {
                ghost1F.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>().enabled = false;
                ghost2F.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>().enabled = true;
            }
        }
        ghost2F.transform.position = changeFloor[Random.Range(2,10)].transform.position;
    }

    IEnumerator patorl()
    {
        waypoint = 0;
        while (true)
        {
            changeFloor = checkFloor ? patrolPoint1F : patrolPoint2F;
            if(ManagerCS.MInstance.PlayingCheck && !HorrorEvent.isGymEvent)
            {
                nvAgent.SetDestination(changeFloor[waypoint].transform.position);
            }
                float distance = Vector3.Distance(this.transform.position, changeFloor[waypoint].position);

                // 순회하다 지점에 도착하면 다음 지점으로 이동
                if (distance < 0.1f)
                {
                    waypoint++;
                }
            float distance1 = Vector3.Distance(this.transform.position, dt1.position);
            float distance2 = Vector3.Distance(this.transform.position, dt2.position);
            if(distance1<1.2f)
            {
                if(!d1._openCheck)
                waypoint = 2;
            }
            if(distance2<1.2f)
            {
                if(!d2._openCheck)
                waypoint = 0;
            }

            if (waypoint == changeFloor.Count)
            {
                waypoint = 0;
            }
            if (Stun.isStop == true)
            {
                nvAgent.speed = 0;

                yield return new WaitForSeconds(2.0f);
                nvAgent.speed = prowlSpeed;
            }

            yield return StartCoroutine(TracePlayer());

            yield return null;
        }
    }

    IEnumerator TracePlayer()
    {
        while (ManagerCS.MInstance.CheckObjectIsInCamera(player, ghostcam))
        {
            if (ManagerCS.MInstance.NightMap.active && !ManagerCS.MInstance.HorrorMap.active)
            {
                nvAgent.destination = player.transform.position;
                if (Stun.isStop == true)
                {
                    nvAgent.speed = 0;
                    animator.SetBool("Trace", false);
                    yield return new WaitForSeconds(2.0f);
                    nvAgent.speed = prowlSpeed;
                }
                animator.SetBool("Trace", true);
                // 공격하는 코루틴 실행
                yield return StartCoroutine(AttackPlayer());

                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
                break;
            }
        }
        animator.SetBool("Trace", false);
    }

    // 공격하는 코루틴
    IEnumerator AttackPlayer()
    {
        // 일정거리가 되면
        float dist = Vector3.Distance(this.transform.position, player.transform.position);
        while (dist <= 1.4f)
        {
            dist = Vector3.Distance(this.transform.position, player.transform.position);
            if (Stun.isStop == true)
            {
                nvAgent.speed = 0;
                animator.SetBool("Attack", false);
                yield return new WaitForSeconds(2.0f);
                nvAgent.speed = prowlSpeed;
            }
            else
            {
                this.GetComponent<NavMeshAgent>().stoppingDistance = 1.4f;
                animator.SetBool("Trace", false);
                Vector3 direction = (player.transform.position - this.transform.position).normalized;
                float rotateAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                this.transform.rotation = Quaternion.Euler(0f, rotateAngle, 0f);
                animator.SetBool("Attack", true);
                yield return new WaitForSeconds(0.63f);
                this.GetComponent<SphereCollider>().center = new Vector3(0, 1.3f, 1.5f);
                yield return new WaitForSeconds(0.55f);
                this.GetComponent<SphereCollider>().center = new Vector3(0, 1.3f, 0.3f);
            }
            yield return null;
        }
        animator.SetBool("Attack", false);
        this.GetComponent<NavMeshAgent>().stoppingDistance = 0;
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.CompareTag("Player"))
        {
            Player.playerHP -= 1;
            ManagerCS.MInstance.PlayingCheck =false;
        }
    }

    public void patrolGhostInit()
    {
        waypoint = 0;
        checkFloor = false;
        changeFloor = patrolPoint2F;
        ghost1F.transform.position = patrolPoint1F[0].transform.position;
        ghost2F.transform.position = patrolPoint2F[0].transform.position;
    }
}
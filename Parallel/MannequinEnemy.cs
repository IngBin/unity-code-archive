using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MannequinEnemy : MonoBehaviour
{
    public Transform firstPos;

    public static bool isTrace = false;

    public Transform playerTr;
    public Camera PlayerCam;
    Vector3 dir;
    float EnemySpeed = Player.moveSpeed;

    void Start()
    {
        this.GetComponent<BoxCollider>().enabled = false;
    }

    void Update()
    {
        if (ManagerCS.MInstance.PlayingCheck && HorrorEvent.isGymEvent && !HorrorEvent.isAnimStart)
        {
            PlayerTrace();
        }
        else
        {
            this.transform.position = firstPos.transform.position;
            this.transform.rotation = firstPos.transform.rotation;
            this.GetComponent<BoxCollider>().enabled = false;
        }
    }

    void PlayerTrace()
    {
        if (!ManagerCS.MInstance.CheckObjectIsInCamera(gameObject, PlayerCam))
        {
            this.GetComponent<BoxCollider>().enabled = true;
            Vector3 direction = (playerTr.position - this.transform.position).normalized;
            float rotateAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            this.transform.rotation = Quaternion.Euler(0f, rotateAngle, 0f);
            this.transform.position += new Vector3(direction.x * EnemySpeed * Time.deltaTime, 0, direction.z * EnemySpeed * Time.deltaTime);
        }
        else
        {
            this.GetComponent<BoxCollider>().enabled = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!ManagerCS.MInstance.CheckObjectIsInCamera(gameObject, PlayerCam) && other.CompareTag("Player"))
        {
            ManagerCS.MInstance.PlayingCheck = false;
            ManagerCS.MInstance.GymGameOver_Image.gameObject.SetActive(true);
            ManagerCS.MInstance.gymGameOver = true;
            StartCoroutine(ManagerCS.MInstance.GymGameOver());
        }
    }
}
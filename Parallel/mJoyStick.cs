using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class mJoyStick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public Vector3 JoyDirection
    {
        get
        {
            if (joyDirection == null)
            {
                Debug.LogError("joy Direction Value is missing!");
                joyDirection = Vector3.zero;
            }
            return (Vector3)joyDirection;
        }
    }
    private Vector3? joyDirection;
    [SerializeField] private RectTransform rect_BackGround;
    [SerializeField] private RectTransform rect_Joystick;

    public bool isTouch = false;
    private float stickRange;

    private void Start()
    {
        stickRange = rect_BackGround.rect.width * 0.3f;
        joyDirection = Vector3.zero;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        isTouch = true;
        rect_BackGround.position = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ManagerCS.MInstance.PlayingCheck && isTouch == true)
        {
            Vector3 stickPosition = eventData.position - (Vector2)rect_BackGround.position;
            rect_BackGround.gameObject.SetActive(true);
            joyDirection = stickPosition.normalized;
            rect_Joystick.localPosition = Vector2.ClampMagnitude(stickPosition, stickRange);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isTouch = false;
        rect_Joystick.localPosition = Vector2.zero;
        rect_BackGround.gameObject.SetActive(false);
        joyDirection = Vector3.zero;
    }

    public void playAnim()
    {
        isTouch = false;
        joyDirection = Vector3.zero;
        rect_BackGround.gameObject.SetActive(false);
    }
}
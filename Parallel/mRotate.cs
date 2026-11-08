using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class mRotate : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    Vector3 FirstPoint;
    Vector3 SecondPoint;
    public float xAngle = 0f;
    public float yAngle = 0f;
    public Vector3 anlge;
    float xAngleTemp;
    float yAngleTemp;
 
    public void OnPointerDown(PointerEventData eventData)
    {
        BeginDrag(eventData.position);
    }
 
    public void OnDrag(PointerEventData eventData)
    {
        OnDrag(eventData.position);
    }
 
    public void BeginDrag(Vector2 a_FirstPoint)
    {
        FirstPoint = a_FirstPoint;
        xAngleTemp = xAngle;
        yAngleTemp = yAngle;
    }
 
    public void OnDrag(Vector2 a_SecondPoint)
    {
        SecondPoint = a_SecondPoint;
        xAngle = xAngleTemp + (SecondPoint.x - FirstPoint.x) * 180 / Screen.width;
        yAngle = yAngleTemp - (SecondPoint.y - FirstPoint.y) * 90 * 3f / Screen.height;
 
        if (yAngle < -30f)
            yAngle = -30f;
        if (yAngle > 40f)
            yAngle = 40f;

        anlge = new Vector3(yAngle, xAngle);
    }
}

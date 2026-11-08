using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockScroll : MonoBehaviour
{
    public RectTransform panel;
    public Image[] img;
    public RectTransform center;

    public float[] distance;
    public float[] distReposition;
    private bool dragging = false;
    private int imgdistance;
    public int minimgNum;
    private int imgLength;

    // Start is called before the first frame update
    void Start()
    {
        int imgLength = img.Length;
        distance = new float[imgLength];
        distReposition = new float[imgLength];

        imgdistance = (int)Mathf.Abs(img[1].GetComponent<RectTransform>().anchoredPosition.y
            - img[0].GetComponent<RectTransform>().anchoredPosition.y);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < img.Length; i++)
        {
            distReposition[i] = center.GetComponent<RectTransform>().position.y
                - img[i].GetComponent<RectTransform>().position.y;
            //center :960 ���� ����
            //distReposition[i] = ������ x��ǥ - ��ư[i]�� x��ǥ    
            distance[i] = Math.Abs(distReposition[i]);

            if (distReposition[i] >= -50 && distReposition[i] <= 50)
            {
                img[i].GetComponent<RectTransform>().localScale =
                   Vector2.Lerp(img[i].GetComponent<RectTransform>().localScale,
                   new Vector2(1.2f, 1.2f), 10f * Time.deltaTime);

            }
            else
            {
                img[i].GetComponent<RectTransform>().localScale =
                   Vector2.Lerp(img[i].GetComponent<RectTransform>().localScale,
                   new Vector2(0.8f, 0.8f), 10f * Time.deltaTime);

            }
            if (distReposition[i] > 1250)
            {

                float curX = img[i].GetComponent<RectTransform>().anchoredPosition.x;
                float curY = img[i].GetComponent<RectTransform>().anchoredPosition.y;
                Vector2 newAnchoredPos = new Vector2(curX , curY + (img.Length * imgdistance));

                img[i].GetComponent<RectTransform>().anchoredPosition = newAnchoredPos;

            }

            if (distReposition[i] < -1000)
            {
                float curX = img[i].GetComponent<RectTransform>().anchoredPosition.x;
                float curY = img[i].GetComponent<RectTransform>().anchoredPosition.y;

                Vector2 newAnchoredPos = new Vector2(curX , curY - (img.Length * imgdistance));
                img[i].GetComponent<RectTransform>().anchoredPosition = newAnchoredPos;

                img[i].GetComponent<RectTransform>().localScale =
                    Vector2.Lerp(img[i].GetComponent<RectTransform>().localScale,
                    new Vector2(0.8f, 0.8f), 1f);

            }

        }

        float minDistance = Mathf.Min(distance);

        for (int a = 0; a < img.Length; a++)
        {
            if (minDistance == distance[a])
            {
                minimgNum = a;
            }
        }

        if (!dragging)
        {
            //LerpToBtn(minButtonNum * -btndistance);
            LerpToBtn(-img[minimgNum].GetComponent<RectTransform>().anchoredPosition.y);
        }
    }
    void LerpToBtn(float position)
    {
        float newY = Mathf.Lerp(panel.anchoredPosition.y, position, Time.deltaTime * 10f);
        Vector2 newPosition = new Vector2(panel.anchoredPosition.x,newY);

        panel.anchoredPosition = newPosition;

    }
    public void StartDrag()
    {
        dragging = true;
    }

    public void EndDrag()
    {
        dragging = false;
    }
}

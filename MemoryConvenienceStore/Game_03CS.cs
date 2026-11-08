using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class Game_03CS : MonoBehaviour
{
    public static Game_03CS G3Instance;

    int Game_03_StageNum;
    int BGMovePerX, BGMovePerY;
    int RouteNum;

    bool[] DirCheck = { false, false, false, false };


    public GameObject StageObj, PlayingObj;

    public GameObject BGImg;
    public GameObject mPlayer;
    public Button[] DirBtn; //0 : Up | 1 : Down | 2 : right | 3 : left

    public Vector3[] PlayerStartPos;
    public Vector3[] PlayerEndPos;

    public Image PlayerImg;
    public Sprite[] StageImg;
    public AnimationClip[] StageAnim;
    public GameObject[] StageColBox;

    public GameObject[] StageBtn;

    AudioSource Game03_Btn_as;

    public Sprite[] PlayerAnim;

    public Sprite[] GameClearImg;
    public GameObject ClearObj;

    bool AnimCheck = false;

    public bool PlayingCheck = false;

    public Text RouteText;

    private void Awake()
    {
        G3Instance = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        Game03_Btn_as = this.GetComponent<AudioSource>();
    }


    #region Button Event
    public void UpDirButton()
    {
        for (int i = 0; i < DirBtn.Length; i++)
            DirBtn[i].gameObject.SetActive(false);

        RouteText.text = "남은 횟수 : " + (RouteNum - 1);
        AnimCheck = true;
        StartCoroutine(PlayerAnimation(0));

        BGImg.transform.DOMoveY(BGImg.transform.position.y - BGMovePerY, 1f, false).OnStepComplete(() =>
        {
            RouteNum--;
            StartCoroutine(DirCourtine(0, -BGMovePerY));

            if (RouteNum > 0)
            {
                AnimCheck = false;
                StopCoroutine(PlayerAnimation(0));
            }
            else
            {
                StartCoroutine(RouteCheckCourtine());
            }
        });
    }

    public void DownDirButton()
    {
        for (int i = 0; i < DirBtn.Length; i++)
            DirBtn[i].gameObject.SetActive(false);

        RouteText.text = "남은 횟수 : " + (RouteNum - 1);
        AnimCheck = true;
        StartCoroutine(PlayerAnimation(5));

        BGImg.transform.DOMoveY(BGImg.transform.position.y + BGMovePerY, 1f, false).OnStepComplete(() =>
        {
            RouteNum--;
            StartCoroutine(DirCourtine(0, BGMovePerY));

            if (RouteNum > 0)
            {
                AnimCheck = false;
                StopCoroutine(PlayerAnimation(5));
            }
            else
            {
                StartCoroutine(RouteCheckCourtine());
            }
        });
    }

    public void RightDirButton()
    {
        for (int i = 0; i < DirBtn.Length; i++)
            DirBtn[i].gameObject.SetActive(false);

        RouteText.text = "남은 횟수 : " + (RouteNum - 1);
        AnimCheck = true;
        StartCoroutine(PlayerAnimation(10));

        BGImg.transform.DOMoveX(BGImg.transform.position.x - BGMovePerX, 1f, false).OnStepComplete(() =>
        {
            RouteNum--;
            StartCoroutine(DirCourtine(-BGMovePerX, 0));

            if (RouteNum > 0)
            {
                AnimCheck = false;
                StopCoroutine(PlayerAnimation(10));
            }
            else
            {
                StartCoroutine(RouteCheckCourtine());
            }
        });
    }

    public void LeftDirButton()
    {
        for (int i = 0; i < DirBtn.Length; i++)
            DirBtn[i].gameObject.SetActive(false);

        RouteText.text = "남은 횟수 : " + (RouteNum - 1);
        AnimCheck = true;
        StartCoroutine(PlayerAnimation(15));

        BGImg.transform.DOMoveX(BGImg.transform.position.x + BGMovePerX, 1f, false).OnStepComplete(() =>
        {
            RouteNum--;
            StartCoroutine(DirCourtine(BGMovePerX, 0));

            if (RouteNum > 0)
            {
                AnimCheck = false;
                StopCoroutine(PlayerAnimation(15));
            }
            else
            {
                StartCoroutine(RouteCheckCourtine());
            }
        });
    }

    public void StageClick()
    {
        if (BtnManager.Binstance.GamePlaying)
        {
            Game03_StageCheck SC = EventSystem.current.currentSelectedGameObject.GetComponent<Game03_StageCheck>();

            if(SC != null)
            {
                Game_03_StageNum = SC.stageNum;
                BGMovePerX = SC.MovePerX;
                BGMovePerY = SC.MovePerY;
                RouteNum = SC.RouteNum;
            }
            PlayingCheck = true;
            BGImg.transform.localPosition = Vector3.zero;
            BGImg.GetComponent<Image>().sprite = StageImg[Game_03_StageNum];
            BGImg.GetComponent<Animation>().clip = StageAnim[Game_03_StageNum];

            mPlayer.transform.localPosition = PlayerStartPos[Game_03_StageNum];

            RouteText.text = "남은 횟수 : " + RouteNum;

            BtnManager.Binstance.AudioChanger(3);
            StageToPlaying(false);
            StartCoroutine(StageStart());

            StageColBox[Game_03_StageNum].SetActive(true);
        }
    }

    #endregion

    #region Courtine
    IEnumerator RouteCheckCourtine()
    {
        yield return new WaitForSeconds(0.5f);

        if (!ClearObj.active)
            StartCoroutine(EndCourtine(1));
    }


    IEnumerator DirCourtine(int x, int y)
    {
        StageColBox[Game_03_StageNum].transform.position += new Vector3(x, y, 0);

        yield return new WaitForSeconds(0.3f);

        RayCheck();

        for (int j = 0; j < DirBtn.Length; j++)
            DirBtn[j].gameObject.SetActive(DirCheck[j]);

    }

    IEnumerator StageStart()
    {;
        BGImg.GetComponent<Animation>().Play();

        yield return new WaitForSeconds(3f);

        AnimCheck = true;
        StartCoroutine(PlayerAnimation(0));

        mPlayer.transform.DOLocalMove(PlayerEndPos[Game_03_StageNum], 1f, false).OnStepComplete(() =>
        {
            mPlayer.GetComponent<BoxCollider>().enabled = true;
            AnimCheck = false;
            StopCoroutine(PlayerAnimation(0));
            RayCheck();

            for (int j = 0; j < DirBtn.Length; j++)
            {
                Debug.Log(DirCheck[j]);
                DirBtn[j].gameObject.SetActive(DirCheck[j]);
            }
        });
    }

    IEnumerator PlayerAnimation(int num)
    {
        for (int i = 0 + num; i < 5 + num; i++)
        {
            PlayerImg.sprite = PlayerAnim[i];
            yield return new WaitForSeconds(0.125f);
        }

        if (AnimCheck)
            StartCoroutine(PlayerAnimation(num));
    }

    public IEnumerator EndCourtine(int i) //0 : Clear | 1 : Over
    {
        BtnManager.Binstance.GameEndingSound(i);

        ClearObj.SetActive(true);
        ClearObj.GetComponent<Image>().sprite = GameClearImg[i];

        if (!ClearObj.GetComponent<Animation>().isPlaying)
            ClearObj.GetComponent<Animation>().Play();

        yield return new WaitForSeconds(2f);

        ClearObj.SetActive(false);

        if (i.Equals(0))
            GameClear();
        else if (i.Equals(1))
            GameOver();

        if (BtnManager.Binstance.interstitial.IsLoaded())
        {
            BtnManager.Binstance.interstitial.Show();
            BtnManager.Binstance.RequestInterstitial();
        }

        BtnManager.Binstance.RequestInterstitial();
    }


    #endregion

    #region Function
    public void RayCheck()
    {
        RaycastHit[] hit = new RaycastHit[4];

        bool UpCheck = Physics.Raycast(mPlayer.transform.position, mPlayer.transform.up, out hit[0], BGMovePerY + 100);
        
        if (UpCheck)
        {
            DirCheck[0] = true;
        }
        else
        {
            DirCheck[0] = false;
        }

        bool DownCheck = Physics.Raycast(mPlayer.transform.position, -mPlayer.transform.up, out hit[1], BGMovePerY + 100);

        if (DownCheck)
        {
            DirCheck[1] = true;
        }
        else
        {
            DirCheck[1] = false;
        }

        bool RightCheck = Physics.Raycast(mPlayer.transform.position, mPlayer.transform.right, out hit[2], BGMovePerX + 100);

        if (RightCheck)
        {
            DirCheck[2] = true;
        }
        else
        {
            DirCheck[2] = false;
        }

        bool LeftCheck = Physics.Raycast(mPlayer.transform.position, -mPlayer.transform.right, out hit[3], BGMovePerX + 100);

        if (LeftCheck)
        {
            DirCheck[3] = true;
        }
        else
        {
            DirCheck[3] = false;
        }
    }

    public void GameOver()
    {
        Debug.Log("Game Over");
        Init();
        StageToPlaying(true);
    }


    public void GameClear()
    {
        if (PlayerPrefs.GetInt("Game03_Stage") < Game_03_StageNum + 1)
            PlayerPrefs.SetInt("Game03_Stage", Game_03_StageNum + 1);
        Debug.Log("Game Clear");
        StageLoad();
        Init();
        StageToPlaying(true);
    }

    public void RePlay()
    {
        if (PlayingObj.active)
        {
            for (int i = 0; i < DirBtn.Length; i++)
            {
                DirBtn[i].gameObject.SetActive(false);
                DirCheck[i] = false;
            }

            mPlayer.GetComponent<BoxCollider>().enabled = false;

            StageColBox[Game_03_StageNum].transform.localPosition = Vector3.zero;

            BtnManager.Binstance.On_Close_Click();

            BGImg.transform.localPosition = Vector3.zero;

            BGImg.GetComponent<Image>().sprite = StageImg[Game_03_StageNum];
            BGImg.GetComponent<Animation>().clip = StageAnim[Game_03_StageNum];

            mPlayer.transform.localPosition = PlayerStartPos[Game_03_StageNum];

            BtnManager.Binstance.AudioChanger(3);

            StartCoroutine(StageStart());
        }
    }

    public void Init()
    {
        StopAllCoroutines();
        for (int i = 0; i < DirBtn.Length; i++)
        {
            DirBtn[i].gameObject.SetActive(false);
            DirCheck[i] = false;
        }

        PlayingCheck = false;
        StageColBox[Game_03_StageNum].transform.localPosition = Vector3.zero;

        mPlayer.GetComponent<BoxCollider>().enabled = false;

        StageColBox[Game_03_StageNum].SetActive(false);

        Game_03_StageNum = 0;

        BtnManager.Binstance.AudioChanger(0);
    }

    public void StageLoad()
    {
        int n = PlayerPrefs.GetInt("Game03_Stage");

        for (int i = 0; i < n; i++)
        {
            StageBtn[i].GetComponentInChildren<Text>().text = "Clear";
            StageBtn[i].GetComponent<Button>().enabled = true;
        }

        if (n < 6)
        {
            StageBtn[n].GetComponent<Button>().enabled = true;
            StageBtn[n].GetComponentInChildren<Text>().text = "" + (n + 1);

            for (int j = n + 1; j < StageBtn.Length; j++)
            {
                if (StageBtn[j].GetComponent<Button>().enabled)
                {
                    StageBtn[j].GetComponent<Button>().enabled = false;
                }

                StageBtn[j].GetComponentInChildren<Text>().text = "Lock";
            }
        }
    }

    public void StageToPlaying(bool b)
    {
        StageObj.SetActive(b);
        PlayingObj.SetActive(!b);
    }

    #endregion
}

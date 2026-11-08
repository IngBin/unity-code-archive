using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class BtnManager : MonoBehaviour
{
    public static BtnManager Binstance;

    #region Public
    public enum GameState
    {
        Home,
        Game_01,
        Game_02,
        Game_03,
        Game_04,
        Setting,
        Info,
        Intro,
    }

    public GameObject[] GameScreen;
    public bool GamePlaying = false;
    public GameObject[] SettingBtn;

    public GameObject[] SetArray;

    public AudioSource ButtonSource;
    public AudioMixerGroup EffectMixer;
    public AudioMixerGroup BGMMixer;

    public AudioClip[] BGMArray;
    public AudioSource BGMSource;

    public AudioSource GameEndSound;
    public AudioClip[] GameEndClip;

    public Text Effect_ONOFF_Text;
    public Text BGM_ONOFF_Text;
    public Text Game_Info_Text;

    public Image[] BGM_Image;
    public Image[] Effect_Image;

    public Sprite[] ONOFF_Image;
    #endregion

    #region Private
    GameState state = GameState.Intro;

    GameState tempState;

    public InterstitialAd interstitial;
    #endregion

    public void RequestInterstitial()
    {
#if UNITY_ANDROID
        string adUnitId = "YOUR_AD_UNIT_ID";
#elif UNITY_IPHONE
        string adUnitId = "YOUR_AD_UNIT_ID";
#else
        string adUnitId = "unexpected_platform";
#endif

        // Initialize an InterstitialAd.
        this.interstitial = new InterstitialAd(adUnitId);
        AdRequest request = new AdRequest.Builder().Build();
        // Load the interstitial with the request.
        this.interstitial.LoadAd(request);
    }

    public void Awake()
    {
        Binstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

        if (!PlayerPrefs.HasKey("Effect"))
            PlayerPrefs.SetInt("Effect", 1);

        if (!PlayerPrefs.HasKey("BGM"))
            PlayerPrefs.SetInt("BGM", 1);

        OnOffPlayer("Effect", EffectMixer, Effect_Image);
        OnOffPlayer("BGM", BGMMixer, BGM_Image);

        RequestInterstitial();
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                switch (state)
                {
                    case GameState.Game_01:
                        if (!Game_01CS.G1Instance.PlayingCheck)
                        {
                            On_HomeBtn_Click();
                            Game_01CS.G1Instance.Init();
                        }
                        else
                        {
                            On_Setting_Click();
                        }
                        BtnClickSound();
                        break;
                    case GameState.Game_02:
                        if (!Game_02Btn.instance.PlayingCheck)
                        {
                            On_HomeBtn_Click();
                        }
                        else
                        {
                            On_Setting_Click();
                        }
                        BtnClickSound();
                        break;
                    case GameState.Game_03:
                        if (!Game_03CS.G3Instance.PlayingCheck)
                        {
                            On_HomeBtn_Click();
                            Game_03CS.G3Instance.Init();
                        }
                        else
                        {
                            On_Setting_Click();
                        }
                        BtnClickSound();
                        break;
                    case GameState.Game_04:
                        if (!Game_04Manager.Instance.PlayingCheck)
                        {
                            On_HomeBtn_Click();
                        }
                        else
                        {
                            On_Setting_Click();
                        }
                        BtnClickSound();
                        break;
                    case GameState.Setting:
                        On_Close_Click();
                        BtnClickSound();
                        break;
                    case GameState.Info:
                        On_Info_Close_Click();
                        BtnClickSound();
                        break;
                }
            }
        }
    }

    #region Button Event
    public void On_Game_01_Click()
    {
        ScreenSwitch(0, 1, GameState.Game_01);
        SetSwitch(0, 1);
        GamePlaying = true;
        if(PlayerPrefs.GetInt("Game01_Info").Equals(0))
        {
            PlayerPrefs.SetInt("Game01_Info", 1);
            On_Info_Click();
        }
    }

    public void On_Game_02_Click()
    {
        ScreenSwitch(0, 2, GameState.Game_02);
        SetSwitch(0, 2);
        GamePlaying = true;
        if (PlayerPrefs.GetInt("Game02_Info").Equals(0))
        {
            PlayerPrefs.SetInt("Game02_Info", 1);
            On_Info_Click();
        }
    }

    public void On_Game_03_Click()
    {
        ScreenSwitch(0, 3, GameState.Game_03);
        SetSwitch(0, 3);
        GamePlaying = true;
        if (PlayerPrefs.GetInt("Game03_Info").Equals(0))
        {
            PlayerPrefs.SetInt("Game03_Info", 1);
            On_Info_Click();
        }
    }

    public void On_Game_04_Click()
    {
        ScreenSwitch(0, 4, GameState.Game_04);
        SetSwitch(0, 4);
        GamePlaying = true;
        if (PlayerPrefs.GetInt("Game04_Info").Equals(0))
        {
            PlayerPrefs.SetInt("Game04_Info", 1);
            On_Info_Click();
        }
    }

    public void On_IntroToHome_Click()
    {
        ScreenSwitch(7, 0, GameState.Home);
    }

    public void On_Setting_Click()
    {
        tempState = state;
        ObjActive(SettingBtn[(int)tempState], false);
        state = GameState.Setting;
        ObjActive(GameScreen[5], true);
        GamePlaying = false;
    }

    public void On_Close_Click()
    {
        ObjActive(SettingBtn[(int)tempState], true);
        state = tempState;
        ObjActive(GameScreen[5], false);
        GamePlaying = true;
    }

    public void On_Info_Click()
    {
        tempState = state;


        switch (state)
        {
            case GameState.Game_01:
                Game_Info_Text.text = "새로나온 과자를 클릭한다. 정해진 갯수만큼 클릭하면 클리어";
                break;
            case GameState.Game_02:
                Game_Info_Text.text = "없어진 삼각김밥을 채워넣는다. 모든 삼각김밥을 채워넣으면 클리어";
                break;
            case GameState.Game_03:
                Game_Info_Text.text = "시작할 때 나온 지도를 외워 편의점까지 도달한다. 편의점에 도달하면 클리어";
                break;
            case GameState.Game_04:
                Game_Info_Text.text = "버튼의 색이 바뀌지 않을때까지 색이렔뀐 순서대로 숫자를 클릭한다. 10점을 얻을 시 클리어";
                break;
        }

        state = GameState.Info;

        ObjActive(GameScreen[6], true);
        GamePlaying = false;
    }

    public void On_Info_Close_Click()
    {
        state = tempState;
        ObjActive(GameScreen[6], false);
        GamePlaying = true;
    }


    public void On_HomeBtn_Click()
    {
        AudioChanger(0);
        GamePlaying = false;
        if (state == GameState.Setting)
        {
            ObjActive(SettingBtn[(int)tempState], true);
            SetSwitch((int)tempState, 0);
            ScreenSwitch((int)tempState, 0, GameState.Home);
            ObjActive(GameScreen[5], false);
        }
        else
        {
            ObjActive(GameScreen[(int)state], false);
            SetSwitch((int)state, 0);
            ScreenSwitch((int)state, 0, GameState.Home);
        }
    }

    public void On_GameOver_Click()
    {
        AudioChanger(0);
        ObjActive(SettingBtn[(int)state], true);
        GamePlaying = false;
        SetSwitch((int)state, 0);
        ScreenSwitch((int)state, 0, GameState.Home);
    }

    public void BtnClickSound()
    {
        if (!ButtonSource.isPlaying)
            ButtonSource.PlayOneShot(ButtonSource.clip);
    }


    public void BGM_ONOFF_Click()
    {
        switch (PlayerPrefs.GetInt("BGM"))
        {
            case 0:
                PlayerPrefs.SetInt("BGM", 1);
                break;
            case 1:
                PlayerPrefs.SetInt("BGM", 0);
                break;
        }

        OnOffPlayer("BGM", BGMMixer, BGM_Image);
    }

    public void Effect_ONOFF_Click()
    {
        switch(PlayerPrefs.GetInt("Effect"))
        {
            case 0:
                PlayerPrefs.SetInt("Effect", 1);
                break;
            case 1:
                PlayerPrefs.SetInt("Effect", 0);
                break;
        }

        OnOffPlayer("Effect", EffectMixer, Effect_Image);     
    }

    #endregion

    #region Coroutine

    #endregion

    #region Function
    public void GameEndingSound(int n)//0 : 정답 , 1 : 오답
    {
        if (!GameEndSound.isPlaying)
            GameEndSound.PlayOneShot(GameEndClip[n]);
    }


    //0 : off, 1 : on
    public void OnOffPlayer(string s, AudioMixerGroup ag, Image[] t)
    {
        switch(PlayerPrefs.GetInt(s))
        {
            case 0:
                ag.audioMixer.SetFloat(s, -80f);

                for(int i = 0; i < t.Length; i++)
                    t[i].sprite = ONOFF_Image[1];
                break;
            case 1:
                ag.audioMixer.SetFloat(s, 0f);

                for (int j = 0; j < t.Length; j++)
                    t[j].sprite = ONOFF_Image[0];
                break;
        }
    }

    public void AudioChanger(int i)
    {
        if (!BGMSource.clip.Equals(BGMArray[i]))
        {
            BGMSource.clip = BGMArray[i];
            BGMSource.Stop();
            BGMSource.Play();
        }
    }

    public void SetSwitch(int i, int j)
    {
        SetArray[i].SetActive(false);
        SetArray[j].SetActive(true);
    }

    public void ScreenSwitch(int i, int j, GameState s)
    {
        ObjActive(GameScreen[i], false);
        ObjActive(GameScreen[j], true);
        state = s;
    }

    public void ObjActive(GameObject g, bool b)
    {
        if (g.active == !b)
            g.SetActive(b);
    }
    #endregion
}

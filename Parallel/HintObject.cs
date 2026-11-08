using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintObject : MonoBehaviour
{
    public GameObject LastDoor;
    public int LastNum = 0;

    public Text[] _hintText;
    public List<string> _hintAlphabet = new List<string>();
    public int[] _hintNum;

    int[] _num = { 0, 0, 0, 0 };
    string LastFNum, LastSNum, LastTNum, LastFoNum;


    void Start()
    {
        RandomHintNum();
        _hintText[3].gameObject.SetActive(false);
    }

    void Update()
    {
        if (ManagerCS.MInstance.PlayingCheck)
        {
            if (ManagerCS.MInstance.NightMap.active && !ManagerCS.MInstance.HorrorMap.active)
            {
                for (int i = 0; i < 4; i++)
                {
                    switch (int.Parse(_hintAlphabet[i]))
                    {
                        case 1:
                            _hintText[i].text = "A";
                            break;
                        case 2:
                            _hintText[i].text = "B";
                            break;
                        case 3:
                            _hintText[i].text = "C";
                            break;
                        case 4:
                            _hintText[i].text = "D";
                            break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    switch (_hintText[i].text)
                    {
                        case "A":
                            _hintText[i].text = _hintNum[0].ToString();
                            break;
                        case "B":
                            _hintText[i].text = _hintNum[1].ToString();
                            break;
                        case "C":
                            _hintText[i].text = _hintNum[2].ToString();
                            break;
                        case "D":
                            _hintText[i].text = _hintNum[3].ToString();
                            break;
                    }
                }
            }

            if(HorrorEvent.isGymEvent == true || HorrorEvent.isGymClear == true)
            {
                _hintText[3].gameObject.SetActive(true);
            }
        }
    }

    void RandomHintNum()
    {
        RandomHintAlphabet();

        for (int i = 0; i < _hintNum.Length; i++)
        {
            _hintNum[i] = Random.Range(0, 10);
        }

        for (int i = 0; i < 4; i++)
        {
            switch (int.Parse(_hintAlphabet[i]))
            {
                case 1:
                    _hintText[i].text = "A";
                    break;
                case 2:
                    _hintText[i].text = "B";
                    break;
                case 3:
                    _hintText[i].text = "C";
                    break;
                case 4:
                    _hintText[i].text = "D";
                    break;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            switch (_hintText[i].text)
            {
                case "A":
                    if(_hintNum[i] == 0)
                    {
                        _hintNum[i] = Random.Range(1, 10);
                    }
                    _hintText[i].text = _hintNum[i].ToString();
                    LastFNum = _hintText[i].text;
                    break;
                case "B":
                    _hintText[i].text = _hintNum[i].ToString();
                    LastSNum = _hintText[i].text;
                    break;
                case "C":
                    _hintText[i].text = _hintNum[i].ToString();
                    LastTNum = _hintText[i].text;
                    break;
                case "D":
                    _hintText[i].text = _hintNum[i].ToString();
                    LastFoNum = _hintText[i].text;
                    break;
            }
        }
        LastNum = int.Parse(LastFNum + LastSNum + LastTNum + LastFoNum);

        LastDoor.GetComponent<GetLastKey>().EscapeNum = LastNum;
    }

    void RandomHintAlphabet()
    {
        for (int i = 0; i < _num.Length; i++)
        {
            int rand = Random.Range(1, 5);
            _num[i] = rand;
            if (!_hintAlphabet.Contains(_num[i].ToString()))
            {
                _hintAlphabet.Add(_num[i].ToString());
            }
            else
            {
                while (_hintAlphabet.Contains(_num[i].ToString()))
                {
                    rand = Random.Range(1, 5);
                    _num[i] = rand;

                    if (!_hintAlphabet.Contains(_num[i].ToString()))
                        break;
                    else
                        continue;
                }
                _hintAlphabet.Add(_num[i].ToString());
            }
        }
    }

    public void HintInit()
    {
        LastNum = 0;
        _hintText.Initialize();
        _hintNum.Initialize();
        _num.Initialize();
        LastFNum = null;
        LastSNum = null;
        LastTNum = null;
        LastFoNum = null;
        _hintAlphabet.Clear();
        RandomHintNum();
        if(_hintText[3].gameObject.activeSelf == true)
        {
            _hintText[3].gameObject.SetActive(false);
        }
    }
}
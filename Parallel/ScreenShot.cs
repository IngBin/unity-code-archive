using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ScreenShot : MonoBehaviour
{

    [SerializeField]
    GameObject canvas;
    string[] files = null;
    int whichScreenShotIsShown = 0;

    public Text picCountText;

    void Start()
    {
        files = Directory.GetFiles(Application.persistentDataPath +"/", "*.png");
        if(files.Length>0)
        {
            GetPictureAnsShowIt();
        }
    }

    void GetPictureAnsShowIt()
    {
        string pathToFile = files[whichScreenShotIsShown];
        Texture2D texture = GetScreenshotImage(pathToFile);
        Sprite sp = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        canvas.GetComponent<Image>().sprite = sp;
        picCountText.text = (whichScreenShotIsShown+1) + "/" + files.Length.ToString();
    }
    Texture2D GetScreenshotImage(string filePath)
    {
        Texture2D texture = null;
        byte[] fileBytes;
        if(File.Exists(filePath))
        {
            fileBytes = File.ReadAllBytes(filePath);
            texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
            texture.LoadImage(fileBytes);
        }
        return texture;
    }
    public void NextPicture() // 다음 캡쳐장면으로
    {
        if(files.Length>0)
        {
            whichScreenShotIsShown += 1;
            picCountText.text = whichScreenShotIsShown.ToString() + "/" + files.Length.ToString();
            if (whichScreenShotIsShown > files.Length - 1)
                whichScreenShotIsShown = 0;
            GetPictureAnsShowIt();
        }
    }
    public void PreviousPictrue() // 이전 캡쳐장면으로
    {
        if(files.Length>0)
        {
            whichScreenShotIsShown -= 1;
            picCountText.text = whichScreenShotIsShown.ToString() +"/"+ files.Length.ToString();
            if (whichScreenShotIsShown < 0)
                whichScreenShotIsShown = files.Length - 1;
            GetPictureAnsShowIt();
        }
    }

    public void closeBtnClick()
    {
        ManagerCS.MInstance.Canvas_ScreenShot.SetActive(false);
        ManagerCS.MInstance.Btn_Main.transform.position = new Vector2(ManagerCS.MInstance.Btn_Retry.transform.position.x,ManagerCS.MInstance.Btn_Retry.transform.position.y-140);
    }

    public void ScreenShotReset()
    {
        if (System.IO.Directory.Exists(Application.persistentDataPath + "/"))
            {
                string[] files = System.IO.Directory.GetFiles(Application.persistentDataPath +"/", "*.png");
                foreach (string s in files)
                {
                    string fileName = System.IO.Path.GetFileName(s);
                    string deletefile = Application.persistentDataPath +"/" + fileName;
                    System.IO.File.Delete(deletefile);
                }
            }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Slot : MonoBehaviour
{
    [SerializeField] private MyItemData myitem;
    public MyItemData _myitem
    {
        get { return myitem; }
        set
        {
            myitem = value;
        }
    }

    [SerializeField]private StaticItemData item;
    public StaticItemData _item
    {
        get { return item; }
        set
        {
            item = value;
        }
    }

    public bool isEmpty = true;
    public Image ClassImage; // 아이템 등급 이미지
    public Image itemimage; // 아이템 이미지 (속성)
    public Image itemtypeimage; // 아이템 이미지 (스탯타입)?
    public TextMeshProUGUI itemforcetext; // 강화 수치
    public TextMeshProUGUI itemtestText; // test
    // 아이템 등급

    public Button button;
    private Sprite emptyClassSprite;

    [SerializeField] private GameObject newMark; // 신규 아이템 빨간점 (비어있으면 런타임에 자동 생성)

    [Header("신규 빨간점 깜빡임")]
    [SerializeField] private float newMarkBlinkSpeed = 8f;             // 깜빡임 속도 (클수록 빠름)
    [SerializeField, Range(0f, 1f)] private float newMarkMinAlpha = 0.2f; // 최소 밝기
    [SerializeField, Range(0f, 1f)] private float newMarkMaxAlpha = 1f;   // 최대 밝기

    [SerializeField] private GameObject selectMark; // 선택 판매 선택 표시 (비어있으면 런타임 자동 생성)

    // 슬롯이 마지막으로 그린 값. 같은 값이면 다시 그리지 않아 전체 갱신 비용을 줄입니다.
    private bool isCleared;
    private int shownInventoryID;
    private int shownItemId;
    private int shownForce;
    private bool shownIsNew;

    private void Awake()
    {
        if (ClassImage != null)
            emptyClassSprite = ClassImage.sprite;

        if (newMark != null)
            newMark.SetActive(false);
    }

    // 런타임에 생성하는 원형 스프라이트 (모든 슬롯이 공유)
    private static Sprite _circleSprite;
    private static Sprite CircleSprite
    {
        get
        {
            if (_circleSprite != null) return _circleSprite;

            const int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;

            float center = (size - 1) * 0.5f;
            float radius = size * 0.5f;
            Color32[] cols = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(radius - dist); // 가장자리 1px 안티에일리어싱
                    cols[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255));
                }
            }
            tex.SetPixels32(cols);
            tex.Apply();

            _circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            return _circleSprite;
        }
    }

    // 프리팹에 빨간점 오브젝트가 배정되지 않았을 경우 우측 상단에 원형 빨간점을 자동 생성합니다.
    void EnsureNewMark()
    {
        if (newMark != null) return;

        GameObject go = new GameObject("NewMark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-2f, -2f);
        rt.sizeDelta = new Vector2(28f, 28f);

        Image img = go.GetComponent<Image>();
        img.sprite = CircleSprite;                       // 런타임 생성 원형
        img.color = new Color(0.92f, 0.16f, 0.16f, 1f);  // 빨강
        img.raycastTarget = false;

        go.transform.SetAsLastSibling(); // 다른 요소 위에 표시
        newMark = go;
    }

    void SetNewMark(bool isNew)
    {
        EnsureNewMark();
        // 빨간점에 깜빡임 애니메이션 부착 (자동 생성/프리팹 배정 모두 대응)
        if (newMark.GetComponent<UIBlink>() == null && newMark.GetComponent<Graphic>() != null)
        {
            var blink = newMark.AddComponent<UIBlink>();
            blink.Configure(newMarkBlinkSpeed, newMarkMinAlpha, newMarkMaxAlpha); // Slot 인스펙터 값 적용
        }
        newMark.SetActive(isNew);
    }

    // 선택 판매용 선택 표시가 없으면 슬롯 전체를 덮는 반투명 하이라이트를 자동 생성합니다.
    void EnsureSelectMark()
    {
        if (selectMark != null) return;

        GameObject go = new GameObject("SelectMark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = go.GetComponent<Image>();
        img.color = new Color(0.2f, 0.7f, 1f, 0.4f); // 반투명 파란 하이라이트
        img.raycastTarget = false;

        go.transform.SetAsLastSibling();
        selectMark = go;
    }

    public void SetSelected(bool selected)
    {
        EnsureSelectMark();
        selectMark.SetActive(selected);
    }

    private void Start()
    {
        itemtestText.text = "";

        if (myitem.id != 0) // 이걸 슬롯에서 관리하는게 아니라 인벤토리 매니저에서 관리하는건?
        {
            //this.gameObject.transform.GetChild(0).GetComponent<AddressableSprite>()._Key = item.image_id.ToString();
            if (myitem.force < 1)
            {
                itemforcetext.text = "";
            }
            else
            {
                itemforcetext.text = "+" + myitem.force.ToString();
            }
            isEmpty = false;
            button.interactable = true;
            itemtestText.text = item.elementType.ToString();
        }
        else
        {
            isEmpty = true;
            button.interactable = false;
            EquipmentGradeVisualUtility.RestoreEmptySlotFrame(ClassImage, emptyClassSprite);
            itemimage.gameObject.SetActive(false);
            itemforcetext.text = "";
        }
    }

    public void Init()
    {
        if (isCleared)
            return;

        isCleared = true;
        itemforcetext.text = "";
        isEmpty = true;
        button.interactable = false;
        SetNewMark(false);
        SetSelected(false);
        itemimage.GetComponent<Image>().sprite = null;
        itemtypeimage.gameObject.SetActive(false);
        itemimage.gameObject.SetActive(false);
        EquipmentGradeVisualUtility.RestoreEmptySlotFrame(ClassImage, emptyClassSprite);
        _item = new StaticItemData();
        _myitem = new MyItemData();
    }

    public void SetData(MyItemData itemData, bool isNew = false)
    {
        // 서버 응답으로 객체만 바뀐 경우에도 참조는 최신으로 유지합니다.
        _myitem = itemData;

        if (!isEmpty && shownInventoryID == itemData.inventoryID && shownItemId == itemData.id
            && shownForce == itemData.force && shownIsNew == isNew)
            return;

        isCleared = false;
        shownInventoryID = itemData.inventoryID;
        shownItemId = itemData.id;
        shownForce = itemData.force;
        shownIsNew = isNew;

        _item = APIData.Instance.GetItem<StaticItemData>(_myitem.id);
        if (myitem.force < 1)
        {
            itemforcetext.text = "";
        }
        else
        {
            itemforcetext.text = "+" + myitem.force.ToString();
        }

        InventoryManager.Instance.LoonSetting(itemimage, itemtypeimage, (int)_item.elementType, (int)_item.imageId);
        EquipmentGradeVisualUtility.ApplySlotFrame(ClassImage, _item.rankType, emptyClassSprite);
        itemimage.gameObject.SetActive(true);
        itemtypeimage.gameObject.SetActive(true);

        isEmpty = false;
        button.interactable = true;

        SetNewMark(isNew);
    }
}

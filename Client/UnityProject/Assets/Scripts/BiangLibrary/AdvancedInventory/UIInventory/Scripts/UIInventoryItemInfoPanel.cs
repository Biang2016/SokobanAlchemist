using UnityEngine;
using UnityEngine.UI;

namespace BiangLibrary.AdvancedInventory.UIInventory
{
    public class UIInventoryItemInfoPanel : MonoBehaviour
    {
        private float ItemImageMaxHeight;

        void Awake()
        {
            ItemImageMaxHeight = ItemImageContainer.sizeDelta.y;
            m_UIInventoryPanel = GetComponentInParent<UIInventoryPanel>();
            Hide();
        }

        private IInventoryItemContentInfo IInventoryItemContentInfo;

        private UIInventoryPanel m_UIInventoryPanel;

        [SerializeField]
        private Image ItemNameBG;

        [SerializeField]
        private Text ItemNameText;

        [SerializeField]
        private RectTransform ItemImageContainer;

        [SerializeField]
        private Image ItemImage;

        [SerializeField]
        private Text ItemCategoryText;

        [SerializeField]
        private Text ItemQualityText;

        [SerializeField]
        private Text ItemBasicInfoText;

        [SerializeField]
        private Text ItemDetailedInfoText;

        [SerializeField]
        private Image[] Decorators;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Initialize(IInventoryItemContentInfo iInventoryItemContentInfo)
        {
            ((RectTransform) transform).anchoredPosition = new Vector2(-((RectTransform) m_UIInventoryPanel.Container.transform).sizeDelta.x, ((RectTransform) transform).anchoredPosition.y);

            IInventoryItemContentInfo = iInventoryItemContentInfo;
            Color bgColor = IInventoryItemContentInfo.ItemColor;
            ItemNameBG.color = bgColor;
            ItemNameText.text = IInventoryItemContentInfo.ItemName;

            ItemImage.sprite = iInventoryItemContentInfo.ItemSprite;
            Rect rect = ItemImage.sprite.rect;
            float ratio = Mathf.Min(ItemImageContainer.sizeDelta.x / rect.width, ItemImageMaxHeight / rect.height);
            rect.height = rect.height * ratio;
            ItemImageContainer.sizeDelta = new Vector2(ItemImageContainer.sizeDelta.x, rect.height);

            ItemCategoryText.text = IInventoryItemContentInfo.ItemCategoryName;
            ItemCategoryText.color = IInventoryItemContentInfo.ItemColor;
            ItemQualityText.text = IInventoryItemContentInfo.ItemQuality;
            ItemQualityText.color = IInventoryItemContentInfo.ItemColor;
            ItemBasicInfoText.text = IInventoryItemContentInfo.ItemBasicInfo;
            ItemDetailedInfoText.text = IInventoryItemContentInfo.ItemDetailedInfo;

            foreach (Image image in Decorators)
            {
                image.color = bgColor;
            }

            StartCoroutine(CommonUtils.UpdateLayout((RectTransform) transform));
        }
    }
}
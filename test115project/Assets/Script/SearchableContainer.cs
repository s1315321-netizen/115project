using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SearchableContainer : Interactable
{
    [Header("--- 翻找設定 ---")]
    [Tooltip("需要按住 E 幾秒才算完成")]
    public float searchDuration = 1.5f;
    [Tooltip("翻找出來要掉落的道具資料 (Item Asset)")]
    public Item itemToDrop;
    [Tooltip("世界中可被拾取的道具 Prefab (需掛載 PickupItem)")]
    public GameObject worldItemPrefab;
    [Tooltip("道具掉落出來的目標位置")]
    public Transform dropPoint;
    [Tooltip("是否已經翻找過了")]
    public bool hasSearched = false;

    [Header("--- UI 進度條設定 ---")]
    [Tooltip("直接放在此物件底下的進度條 Canvas")]
    public GameObject progressBarCanvas;
    public Slider progressSlider;

    private bool isSearching = false;
    private float holdTimer = 0f;
    private PlayerMovement playerMovement;
    private Vector3 initialCanvasScale;

    void Start()
    {
        if (progressBarCanvas != null)
        {
            initialCanvasScale = progressBarCanvas.transform.localScale;
            progressBarCanvas.SetActive(false); // 遊戲一開始預設隱藏
        }

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = searchDuration;
            progressSlider.value = 0f;
        }
    }

    void LateUpdate()
    {
        // 自動抵銷父物件的左右翻轉 (Scale.x 變負數時)，確保進度條永遠維持正面
        if (progressBarCanvas != null && progressBarCanvas.activeSelf)
        {
            float parentFacingX = Mathf.Sign(transform.lossyScale.x);
            progressBarCanvas.transform.localScale = new Vector3(
                Mathf.Abs(initialCanvasScale.x) * parentFacingX,
                initialCanvasScale.y,
                initialCanvasScale.z
            );
        }
    }

    public override void Interact()
    {
        if (hasSearched || isSearching) return;

        StartCoroutine(HoldToSearchRoutine());
    }

    private IEnumerator HoldToSearchRoutine()
    {
        isSearching = true;
        holdTimer = 0f;

        if (progressBarCanvas != null) progressBarCanvas.SetActive(true);
        if (progressSlider != null) progressSlider.value = 0f;

        // 凍結玩家移動與物理慣性
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.velocity = Vector2.zero;
                if (playerMovement.anim != null) playerMovement.anim.SetFloat("Speed", 0f);
            }
        }

        // 持續按住 E 鍵的跑條循環
        while (Input.GetKey(KeyCode.E))
        {
            holdTimer += Time.deltaTime;
            if (progressSlider != null)
            {
                progressSlider.value = holdTimer;
            }

            if (holdTimer >= searchDuration)
            {
                CompleteSearch();
                yield break;
            }

            yield return null;
        }

        // 中途放開 E 鍵取消
        CancelSearch();
    }

    private void CompleteSearch()
    {
        hasSearched = true;
        isSearching = false;

        if (progressBarCanvas != null) progressBarCanvas.SetActive(false);
        RestorePlayerMovement();

        // 掉落實體道具到旁邊
        if (itemToDrop != null && worldItemPrefab != null)
        {
            Vector3 spawnPos = dropPoint != null ? dropPoint.position : transform.position + new Vector3(1f, 0f, 0f);
            GameObject droppedObj = Instantiate(worldItemPrefab, spawnPos, Quaternion.identity);

            PickupItem pickup = droppedObj.GetComponent<PickupItem>();
            if (pickup != null)
            {
                pickup.itemData = itemToDrop;
            }
        }
    }

    private void CancelSearch()
    {
        isSearching = false;
        holdTimer = 0f;
        if (progressSlider != null) progressSlider.value = 0f;

        if (progressBarCanvas != null) progressBarCanvas.SetActive(false);

        RestorePlayerMovement();
    }

    private void RestorePlayerMovement()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
            playerMovement = null;
        }
    }
}
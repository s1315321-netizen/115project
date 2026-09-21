using UnityEngine;

// 繼承自你之前寫的 Interactable
public class ViewPainting : Interactable
{
    [Header("畫作的 UI 畫面")]
    public GameObject paintingUI;
    public GameObject paintingText;

    private bool isViewing = false;
    private PlayerMovement playerMovement; // 用來記錄主角的移動腳本

    void Start()
    {
        // 遊戲開始時，自動透過 Tag 找到場景中的主角，並取得他身上的 PlayerMovement 腳本
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
        }
    }

    public override void Interact()
    {
        isViewing = !isViewing;

        if (paintingUI != null)
        {
            paintingUI.SetActive(isViewing);
            paintingText.SetActive(isViewing);
        }

        // 控制主角能不能移動
        if (playerMovement != null)
        {
            if (isViewing)
            {
                // 1. 關閉移動腳本，讓玩家按 WASD 不再有反應
                playerMovement.enabled = false;

                // 2. 將剛體速度強制歸零，防止主角滑行
                playerMovement.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

                // 3. 將動畫的 Speed 設為 0，確保角色站定，不會卡在走路的動畫
                if (playerMovement.anim != null)
                {
                    playerMovement.anim.SetFloat("Speed", 0f);
                }
            }
            else
            {
                // 關閉畫作，重新啟用移動腳本，主角就可以繼續走了
                playerMovement.enabled = true;
            }
        }
    }
}
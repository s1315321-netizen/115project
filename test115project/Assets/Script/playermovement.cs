using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("--- Movement Settings ---")]
    public float speed = 5f;
    public Animator anim;

    [Header("--- Flashlight Object ---")]
    public Transform flashlight; // Inspector 拖入手電筒 Transform

    private Rigidbody2D rb;
    private Camera mainCamera;
    private Vector2 inputDir = Vector2.zero;
    private Vector3 initialScale;

    // SOCD 後輸入優先按鍵記錄清單
    private List<KeyCode> activeMoveKeys = new List<KeyCode>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialScale = transform.localScale;
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (PauseManager.IsGamePaused) return;
        HandleSOCDMovementInput();
        UpdateCharacterOrientation();
        UpdateAnimator();
    }

    void LateUpdate()
    {
        if (PauseManager.IsGamePaused) return; // 暫停時手電筒不跟隨滑鼠
        UpdateFlashlightAim();
        UpdateFlashlightAim();
    }

    void FixedUpdate()
    {
        rb.velocity = inputDir * speed;
    }

    // ==========================================
    // 1. SOCD (後輸入優先) 嚴格四方向處理
    // ==========================================
    private void HandleSOCDMovementInput()
    {
        // 監聽按下：最新按下的方向排在最末端（優先度最高）
        if (Input.GetKeyDown(KeyCode.W)) RegisterKey(KeyCode.W);
        if (Input.GetKeyDown(KeyCode.S)) RegisterKey(KeyCode.S);
        if (Input.GetKeyDown(KeyCode.A)) RegisterKey(KeyCode.A);
        if (Input.GetKeyDown(KeyCode.D)) RegisterKey(KeyCode.D);

        // 監聽放開：從清單移除
        if (Input.GetKeyUp(KeyCode.W)) UnregisterKey(KeyCode.W);
        if (Input.GetKeyUp(KeyCode.S)) UnregisterKey(KeyCode.S);
        if (Input.GetKeyUp(KeyCode.A)) UnregisterKey(KeyCode.A);
        if (Input.GetKeyUp(KeyCode.D)) UnregisterKey(KeyCode.D);

        // 防呆清理：防止視窗切換等邊緣情況遺失放開事件
        for (int i = activeMoveKeys.Count - 1; i >= 0; i--)
        {
            if (!Input.GetKey(activeMoveKeys[i]))
            {
                activeMoveKeys.RemoveAt(i);
            }
        }

        // 永遠取最後按下的那個按鍵作為前進方向
        if (activeMoveKeys.Count > 0)
        {
            KeyCode latestKey = activeMoveKeys[activeMoveKeys.Count - 1];
            switch (latestKey)
            {
                case KeyCode.W: inputDir = Vector2.up; break;
                case KeyCode.S: inputDir = Vector2.down; break;
                case KeyCode.A: inputDir = Vector2.left; break;
                case KeyCode.D: inputDir = Vector2.right; break;
            }
        }
        else
        {
            inputDir = Vector2.zero;
        }
    }

    private void RegisterKey(KeyCode key)
    {
        if (activeMoveKeys.Contains(key))
        {
            activeMoveKeys.Remove(key);
        }
        activeMoveKeys.Add(key);
    }

    private void UnregisterKey(KeyCode key)
    {
        activeMoveKeys.Remove(key);
    }

    // ==========================================
    // 2. 身體轉向：走路面向行進方向，靜止面向游標
    // ==========================================
    private void UpdateCharacterOrientation()
    {
        if (inputDir != Vector2.zero)
        {
            // 【走路中】：依照走路方向翻轉 Sprite
            if (inputDir.x != 0f)
            {
                float facingX = Mathf.Sign(inputDir.x);
                transform.localScale = new Vector3(Mathf.Abs(initialScale.x) * facingX, initialScale.y, initialScale.z);
            }
        }
        else
        {
            // 【靜止待機】：始終面向滑鼠游標所在側
            if (mainCamera != null)
            {
                Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                float facingX = (mouseWorldPos.x >= transform.position.x) ? 1f : -1f;
                transform.localScale = new Vector3(Mathf.Abs(initialScale.x) * facingX, initialScale.y, initialScale.z);
            }
        }
    }

    // ==========================================
    // 3. 動畫狀態機更新
    // ==========================================
    private void UpdateAnimator()
    {
        if (anim == null) return;

        if (inputDir != Vector2.zero)
        {
            // 【走路中】：速度為 1，動畫播放行走方向
            anim.SetFloat("Speed", 1f);
            anim.SetFloat("Vertical", inputDir.y);
            anim.SetFloat("Horizontal", Mathf.Abs(inputDir.x));
        }
        else
        {
            // 【靜止待機】：速度為 0，動畫方向跟隨游標的角度
            anim.SetFloat("Speed", 0f);

            if (mainCamera != null)
            {
                Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Vector2 aimDir = (mouseWorldPos - transform.position).normalized;

                // 判斷游標是偏上下還是偏左右
                if (Mathf.Abs(aimDir.y) > Mathf.Abs(aimDir.x))
                {
                    // 游標在上方（抬頭待機）或下方（低頭待機）
                    anim.SetFloat("Vertical", Mathf.Sign(aimDir.y));
                    anim.SetFloat("Horizontal", 0f);
                }
                else
                {
                    // 游標在左右兩側（側面待機）
                    anim.SetFloat("Vertical", 0f);
                    anim.SetFloat("Horizontal", 1f);
                }
            }
        }
    }

    // ==========================================
    // 4. 手電筒 360 度瞄準滑鼠游標
    // ==========================================
    private void UpdateFlashlightAim()
    {
        if (flashlight == null || mainCamera == null) return;

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDirection = mouseWorldPos - flashlight.position;

        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
        flashlight.rotation = Quaternion.Euler(0, 0, angle);
    }
}
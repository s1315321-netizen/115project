using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("--- 移動設定 ---")]
    public float speed = 5f;
    public Animator anim;

    [Header("--- 手電筒物件 ---")]
    public Transform flashlight; // Inspector 拖入手電筒子物件

    private Rigidbody2D rb;
    private Camera mainCamera;
    private Vector2 inputDir = Vector2.zero;
    private Vector3 initialScale;

    // 關鍵：記錄最後移動的方向（預設朝下），供待機動畫使用
    private Vector2 lastMoveDir = Vector2.down;

    // 單軸十字鎖定（嚴格四方向，防止斜向造成動畫錯亂）
    private enum LockAxis { None, Horizontal, Vertical }
    private LockAxis currentLock = LockAxis.None;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialScale = transform.localScale;
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleMovementInput();
        UpdateCharacterSprite();
        UpdateAnimator();
    }

    void LateUpdate()
    {
        UpdateFlashlightAim();
    }

    void FixedUpdate()
    {
        rb.velocity = inputDir * speed;
    }

    // 1. 單軸十字鎖定輸入處理
    private void HandleMovementInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (currentLock == LockAxis.None)
        {
            if (h != 0f) currentLock = LockAxis.Horizontal;
            else if (v != 0f) currentLock = LockAxis.Vertical;
        }

        if (currentLock == LockAxis.Horizontal)
        {
            if (h == 0f)
            {
                currentLock = LockAxis.None;
                inputDir = Vector2.zero;
            }
            else
            {
                inputDir = new Vector2(h, 0f).normalized;
            }
        }
        else if (currentLock == LockAxis.Vertical)
        {
            if (v == 0f)
            {
                currentLock = LockAxis.None;
                inputDir = Vector2.zero;
            }
            else
            {
                inputDir = new Vector2(0f, v).normalized;
            }
        }
        else
        {
            inputDir = Vector2.zero;
        }

        // 只要有在走動，立刻更新歷史方向
        if (inputDir != Vector2.zero)
        {
            lastMoveDir = inputDir;
        }
    }

    // 2. 身體左右翻轉（只在有水平移動時翻轉，維持上下行走的正確朝向）
    private void UpdateCharacterSprite()
    {
        if (inputDir.x != 0f)
        {
            float facingX = Mathf.Sign(inputDir.x);
            transform.localScale = new Vector3(Mathf.Abs(initialScale.x) * facingX, initialScale.y, initialScale.z);
        }
    }

    // 3. 動畫參數更新：Speed 用當前輸入，方向用最後記錄
    private void UpdateAnimator()
    {
        if (anim == null) return;

        // Speed 歸零時觸發切換到待機 (Idle)
        anim.SetFloat("Speed", inputDir.magnitude);

        // 關鍵：傳遞 lastMoveDir，停下時依然保有最後走動的朝向！
        anim.SetFloat("Vertical", lastMoveDir.y);
        anim.SetFloat("Horizontal", Mathf.Abs(lastMoveDir.x));
    }

    // 4. 手電筒 360 度對準滑鼠游標
    private void UpdateFlashlightAim()
    {
        if (flashlight == null || mainCamera == null) return;

        // 取得滑鼠在世界空間的座標
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDirection = (mouseWorldPos - flashlight.position);

        // 角度偏移 -90f 配合 Unity 2D 預設光源朝向
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg - 90f;
        flashlight.rotation = Quaternion.Euler(0, 0, angle);
    }
}
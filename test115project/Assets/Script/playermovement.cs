using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public Animator anim;
    public Transform flashlight; // 在 Inspector 拖入手電筒子物件

    private Rigidbody2D rb;
    private Vector2 inputDir = Vector2.zero;
    private Vector3 initialScale;

    // 記錄角色最後面向的水平方位（1 朝右，-1 朝左），預設朝右
    private float lastFacingX = 1f;

    // 四方向鎖定狀態
    private enum LockAxis { None, Horizontal, Vertical }
    private LockAxis currentLock = LockAxis.None;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialScale = transform.localScale;

        UpdateFlashlight();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 1. 單軸十字鎖定
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

        // 2. 有水平移動時，更新左右朝向並翻轉主角
        if (inputDir.x != 0f)
        {
            lastFacingX = Mathf.Sign(inputDir.x); // 記錄是左還是右
            transform.localScale = new Vector3(Mathf.Abs(initialScale.x) * lastFacingX, initialScale.y, initialScale.z);
        }

        // 3. 動畫參數更新
        if (anim != null)
        {
            anim.SetFloat("Speed", inputDir.magnitude);
            anim.SetFloat("Vertical", inputDir.y);
            anim.SetFloat("Horizontal", Mathf.Abs(inputDir.x));
        }
    }

    void LateUpdate()
    {
        // 4. 更新手電筒角度
        UpdateFlashlight();
    }

    void FixedUpdate()
    {
        rb.velocity = inputDir * speed;
    }

    private void UpdateFlashlight()
    {
        if (flashlight == null) return;

        float angle;

        if (inputDir != Vector2.zero)
        {
            // 【移動中】：手電筒跟隨當前移動方向（上下左右）
            angle = Mathf.Atan2(inputDir.y, inputDir.x) * Mathf.Rad2Deg - 90f;
        }
        else
        {
            // 【待機中】：強制切回純左或純右（依最後面向的 X）
            // -90f 為朝右，90f 為朝左
            angle = (lastFacingX > 0) ? -90f : 90f;
        }

        flashlight.rotation = Quaternion.Euler(0, 0, angle);
    }
}
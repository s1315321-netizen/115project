using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public Animator anim;

    private Rigidbody2D rb;
    private Vector2 inputDir;
    private Vector3 initialScale;

    private enum LockAxis { None, Horizontal, Vertical }
    private LockAxis currentLock = LockAxis.None;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        initialScale = transform.localScale;
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 判定哪一個方向先被按下
        if (currentLock == LockAxis.None)
        {
            if (h != 0f) currentLock = LockAxis.Horizontal;
            else if (v != 0f) currentLock = LockAxis.Vertical;
        }

        // 鎖定單一軸向，放開按鍵時才解鎖
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

        // 水平轉向翻轉
        if (inputDir.x != 0f)
        {
            float direction = Mathf.Sign(inputDir.x);
            transform.localScale = new Vector3(Mathf.Abs(initialScale.x) * direction, initialScale.y, initialScale.z);
        }

        // 動畫參數更新
        if (anim != null)
        {
            anim.SetFloat("Speed", inputDir.magnitude);
            anim.SetFloat("Vertical", inputDir.y);
            anim.SetFloat("Horizontal", Mathf.Abs(inputDir.x));
        }
    }

    void FixedUpdate()
    {
        // 2022.3.6 標準寫法
        rb.velocity = inputDir * speed;
    }
}
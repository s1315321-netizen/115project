using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 10f;
    public Animator anim;

    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal"); // A/D 或 左右箭頭
        float v = Input.GetAxisRaw("Vertical");   // W/S 或 上下箭頭

        Vector2 inputDir = new Vector2(h, v).normalized;

        Vector3 movement = new Vector3(inputDir.x, inputDir.y, 0f) * speed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // 左右翻轉角色外觀
        if (h != 0f)
        {
            float direction = Mathf.Sign(h);
            transform.localScale = new Vector3(Mathf.Abs(initialScale.x) * direction, initialScale.y, initialScale.z);
        }

        // 更新 Animator 參數
        if (anim != null)
        {
            // Speed > 0.01 代表有移動，0 代表站立
            anim.SetFloat("Speed", inputDir.magnitude);
            anim.SetFloat("Vertical", v);
            anim.SetFloat("Horizontal", Mathf.Abs(h));
        }
    }
}
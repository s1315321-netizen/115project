using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playermovement : MonoBehaviour
{
    
    public float speed = 10f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //W/S 控制垂直移動
        float v = 0f;
        if (Input.GetKey(KeyCode.W)) v = 1f;
        if (Input.GetKey(KeyCode.S)) v = -1f;

        //A/D 控制水平移動
        float h = 0f;
        if (Input.GetKey(KeyCode.D)) h = 1f;
        if (Input.GetKey(KeyCode.A)) h = -1f;

        
        Vector3 movement = new Vector3(h * speed * Time.deltaTime, v * speed * Time.deltaTime, 0f);
        transform.Translate(movement, Space.World);
        
        if (h != 0f)
        {
            transform.localScale=new Vector3(h, 1f, 1f);
        }
    }
}

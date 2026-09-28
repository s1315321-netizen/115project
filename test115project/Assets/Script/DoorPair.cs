using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorPair : Interactable
{
    [Header("--- 門的配對代號 ---")]
    public string myDoorID = "Room1_DoorA";
    public string targetDoorID = "Room2_DoorA";

    [Header("--- 跨場景設定 (同場景請留空) ---")]
    public string targetSceneName = "";

    [Header("--- 上鎖與解謎機制 ---")]
    public bool isLocked = false;
    public string requiredKeyID = "";
    public string lockedMessage = "門被鎖上了，需要鑰匙...";

    [Header("--- 傳送出來後的設定 ---")]
    public Transform spawnPoint;
    public Vector2 exitFacingDirection = Vector2.down;

    public static string nextSpawnDoorID = "";

    void Start()
    {
        if (!string.IsNullOrEmpty(nextSpawnDoorID) && nextSpawnDoorID == myDoorID)
        {
            nextSpawnDoorID = "";
            StartCoroutine(SpawnPlayerHereRoutine());
        }
    }

    public override void Interact()
    {
        if (isLocked)
        {
            if (HasKey(requiredKeyID))
            {
                Debug.Log($"使用 {requiredKeyID} 解開了門！");
                isLocked = false;
            }
            else
            {
                Debug.Log(lockedMessage);
                return;
            }
        }

        StartCoroutine(TeleportRoutine());
    }

    private IEnumerator TeleportRoutine()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerMovement movement = player != null ? player.GetComponent<PlayerMovement>() : null;
        Rigidbody2D rb = player != null ? player.GetComponent<Rigidbody2D>() : null;

        // 【按 E 的瞬間立即完全定格】
        if (movement != null)
        {
            movement.enabled = false; // 1. 禁用按鍵輸入

            if (rb != null)
            {
                rb.velocity = Vector2.zero; // 2. 強制物理煞車歸零，防止滑行溜冰
            }

            if (movement.anim != null)
            {
                movement.anim.SetFloat("Speed", 0f); // 3. 立即切回站立靜止動畫
            }
        }

        yield return new WaitForSeconds(0.2f); // 模擬推門開門的時間

        // 【情況 A：跨場景切換】
        if (!string.IsNullOrEmpty(targetSceneName) && targetSceneName != SceneManager.GetActiveScene().name)
        {
            nextSpawnDoorID = targetDoorID;
            SceneManager.LoadScene(targetSceneName);
            yield break;
        }

        // 【情況 B：同一場景傳送】
        DoorPair targetDoor = FindTargetDoor(targetDoorID);
        if (targetDoor != null)
        {
            targetDoor.SpawnPlayer(player);
        }
        else
        {
            Debug.LogError($"找不到代號為 [{targetDoorID}] 的門！");
        }

        yield return new WaitForSeconds(0.2f);

        // 傳送完畢，恢復角色移動
        if (movement != null)
        {
            movement.enabled = true;
        }
    }

    private DoorPair FindTargetDoor(string targetID)
    {
        // 修正點 2：補上 
        DoorPair[] allDoors = FindObjectsOfType<DoorPair>();
        foreach (DoorPair door in allDoors)
        {
            if (door != this && door.myDoorID == targetID)
            {
                return door;
            }
        }
        return null;
    }

    public void SpawnPlayer(GameObject player)
    {
        if (player == null) return;

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position + (Vector3)exitFacingDirection;
        player.transform.position = spawnPos;

        // 修正點 3：補上 
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null && movement.flashlight != null)
        {
            float angle = Mathf.Atan2(exitFacingDirection.y, exitFacingDirection.x) * Mathf.Rad2Deg - 90f;
            movement.flashlight.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private IEnumerator SpawnPlayerHereRoutine()
    {
        yield return null;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        SpawnPlayer(player);
    }

    private bool HasKey(string keyID)
    {
        if (string.IsNullOrEmpty(keyID)) return true;
        return false;
    }
}
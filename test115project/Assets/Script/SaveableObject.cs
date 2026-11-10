using UnityEngine;

/// <summary>掛在需要保存啟用狀態的謎題、門或拾取物件上，並填入唯一 ID。</summary>
public class SaveableObject : MonoBehaviour
{
    public string saveID;

    public void WriteTo(GameSaveData data)
    {
        if (string.IsNullOrWhiteSpace(saveID)) return;
        string value = gameObject.activeSelf ? "1" : "0";
        string prefix = "object:" + saveID + "=";
        data.objectStates.RemoveAll(x => x.StartsWith(prefix));
        data.objectStates.Add(prefix + value);
    }

    public void ReadFrom(GameSaveData data)
    {
        if (string.IsNullOrWhiteSpace(saveID) || data.objectStates == null) return;
        string prefix = "object:" + saveID + "=";
        string entry = data.objectStates.Find(x => x.StartsWith(prefix));
        if (entry != null) gameObject.SetActive(entry.Substring(prefix.Length) == "1");
    }
}

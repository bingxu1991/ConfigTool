using System;
using UnityEngine;
/// <summary>
/// 事件发布者脚本：EventPublisher.cs
/// </summary>
public class EventPublisher : MonoBehaviour
{
    // 定义事件
    public static event Action<string> OnNotice;

    // 封装一个静态方法方便外部触发
    public static void Message(string message)
    {
        //Debug.Log("EventPublisher 触发事件：" + message);
        OnNotice?.Invoke(message);
    }
}

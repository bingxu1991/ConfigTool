using UnityEngine;

// 实现普通的单例模式
// where 限制模板的类型, new()指的是这个类型必须要能被实例化
public abstract class Singleton<T> where T : new()
{
    private static T _ins;
    private static object mutex = new object();
    public static T Ins
    {
        get
        {
            if (_ins == null)
            {
                lock (mutex)
                { // 保证我们的单例，是线程安全的;
                    if (_ins == null)
                    {
                        _ins = new T();
                    }
                }
            }
            return _ins;
        }
    }
}

// Unity单例
public abstract class UTSingleton<T> : MonoBehaviour where T : Component
{
    private static T _ins = null;
    public static T Ins
    {
        get
        {
            if (_ins == null)
            {
                _ins = FindAnyObjectByType(typeof(T)) as T;
                if (_ins == null)
                {
                    GameObject obj = new GameObject();
                    _ins = (T)obj.AddComponent(typeof(T));
                    obj.hideFlags = HideFlags.DontSave;
                    // obj.hideFlags = HideFlags.HideAndDontSave;
                    obj.name = typeof(T).Name;
                }
            }
            return _ins;
        }
    }
}

//继承自这个类的子类，去创建子类的ScriptableObject 
public class SObjectSingle<T> : ScriptableObject where T : ScriptableObject
{
    private static T _ins = null;
    public static T Ins
    {
        get
        {
            //如果为空 首先应该去资源路劲下加载 对应的 数据资源文件
            if (_ins == null)
            {
                //我们定两个规则
                //1.所有的 数据资源文件都放在 Resources文件夹下的ScriptableObject中
                //2.需要复用的 唯一的数据资源文件名 我们定一个规则：和类名是一样的
                _ins = Resources.Load<T>("ScriptableObject/" + typeof(T).Name);
            }
            //如果没有这个文件 为了安全起见 我们可以在这直接创建一个数据
            if (_ins == null)
            {
                _ins = CreateInstance<T>();
            }
            //甚至可以在这里 从json当中读取数据，但是我不建议用ScriptableObject来做数据持久化
            return _ins;
        }
    }
}
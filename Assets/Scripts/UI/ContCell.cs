using UnityEngine;
using UnityEngine.UI;

public class ContCell : MonoBehaviour
{
    Text cont;

    void Awake()
    {
        cont = GetComponent<Text>();
    }

    public void SetData(string str)
    {
        cont.text = str;
    }
}

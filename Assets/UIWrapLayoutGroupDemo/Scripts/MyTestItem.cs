using Kiwi.JimmyGon;
using UnityEngine;
using UnityEngine.UI;

public class MyTestItem : UIWrapItem<MyTestData>
{
    [SerializeField]
    private Text m_Text;

    public override void SetData(MyTestData data, int index)
    {
        if (m_Text) m_Text.text = data.text;
    }
}

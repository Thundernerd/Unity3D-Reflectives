using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace TNRD.Odin
{
    public class FlexibleSpaceAttributeDrawer : OdinAttributeDrawer<FlexibleSpaceAttribute>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            GUILayout.FlexibleSpace();
            CallNextDrawer(label);
        }
    }
}
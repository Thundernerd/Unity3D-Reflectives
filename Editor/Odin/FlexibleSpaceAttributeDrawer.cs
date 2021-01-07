using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace TNRD.Odin
{
    internal class FlexibleSpaceAttributeDrawer : OdinAttributeDrawer<FlexibleSpaceAttribute>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            GUILayout.FlexibleSpace();
            CallNextDrawer(label);
        }
    }
}
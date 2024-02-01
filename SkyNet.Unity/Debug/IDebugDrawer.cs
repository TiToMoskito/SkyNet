using UnityEngine;

namespace SkyNet
{
    public interface IDebugDrawer
    {
        void IsEditor(bool isEditor);

        void Indent(int adjust);

        void Label(string text);

        void LabelBold(string text);

        void LabelField(string text, object value);

        void Separator();

        void SelectGameObject(GameObject gameObject);
    }
}

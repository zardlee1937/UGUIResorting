using UnityEditor;
using UnityEngine;
using System.Xml;

namespace UGUIGen
{
    [CustomEditor(typeof(UGUIBaker))]
    public sealed class UGUIBakerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.BeginVertical();
            if (GUILayout.Button("Bake GUI"))
            {
                var root = serializedObject.FindProperty("trans").objectReferenceValue as RectTransform;
                Bake(root);
            }
            GUILayout.EndVertical();
        }

        private void Bake(RectTransform node)
        {
            if (node.childCount < 1)
                throw new System.Exception("Node GUI in this node of hierarchy.");

            var doc = new XmlDocument();
            var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(declaration);

            var root = doc.CreateElement("Root");
            Bake(doc, root, node);
            doc.AppendChild(root);

            var path = System.IO.Path.Combine(Application.streamingAssetsPath, "gui");
            path = System.IO.Path.Combine(path, "main.xml");
            doc.Save(path);
            AssetDatabase.Refresh();
        }

        private void Bake(XmlDocument doc, XmlElement elem, RectTransform node)
        {
            for (var i = 0; i < node.childCount; ++i)
            {
                // 把递归移动到堆上免得炸了
                var action = new System.Action<XmlDocument, XmlElement, RectTransform>(Bake);
                var e = doc.CreateElement("Node");
                elem.AppendChild(e);
                action(doc, e, node.GetChild(i).GetComponent<RectTransform>());
            }
            var x = doc.CreateAttribute("x");
            x.Value = node.anchoredPosition.x.ToString();
            elem.Attributes.Append(x);
            var y = doc.CreateAttribute("y");
            y.Value = node.anchoredPosition.y.ToString();
            elem.Attributes.Append(y);
            var w = doc.CreateAttribute("width");
            w.Value = node.rect.width.ToString();
            elem.Attributes.Append(w);
            var h = doc.CreateAttribute("height");
            h.Value = node.rect.height.ToString();
            elem.Attributes.Append(h);
            var min = doc.CreateAttribute("anchor-min");
            min.Value = node.anchorMin.ToString();
            elem.Attributes.Append(min);
            var max = doc.CreateAttribute("anchor-max");
            max.Value = node.anchorMax.ToString();
            elem.Attributes.Append(max);
        }
    }
}

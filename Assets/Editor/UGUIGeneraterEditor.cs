using UnityEditor;
using UnityEngine;
using System.Xml;

namespace UGUIGen
{
    [CustomEditor(typeof(UGUIGenerater))]
    public sealed class UGUIGeneraterEditor : Editor
    {
        private SerializedProperty pData, wData, hData;
        private TextAsset data;
        private Vector2 rootSize;
        private float Width, Height;

        private void OnEnable()
        {
            pData = serializedObject.FindProperty("data");
            wData = serializedObject.FindProperty("width");
            hData = serializedObject.FindProperty("height");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(pData);
            EditorGUILayout.PropertyField(wData);
            EditorGUILayout.PropertyField(hData);
            data = pData.objectReferenceValue as TextAsset;
            Width = wData.floatValue;
            Height = hData.floatValue;
            serializedObject.ApplyModifiedProperties();

            GUILayout.BeginVertical();
            //if (GUILayout.Button("Generate GUI"))
            //{
            //    var root = serializedObject.FindProperty("trans")
            //        .objectReferenceValue as RectTransform;
            //    Generate(root);
            //}
            if (GUILayout.Button("Reset GUI transform"))
            {
                var root = serializedObject.FindProperty("trans")
                    .objectReferenceValue as RectTransform;
                Reset(root);
            }
            GUILayout.EndVertical();
        }

        private void Generate(RectTransform rootNode)
        {
            if (data == null ||
                data.bytes.Length <= 0)
                throw new System.Exception("No UGUI data inpute.");

            if (rootNode.childCount > 0)
                throw new System.Exception(
                    "UGUI harichy already exist. Try to use 'Reset GUI transform' Button.");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(data.text);

            var root = doc["Root"];
            Generate(rootNode, root);
        }

        private void Generate(RectTransform node, XmlNode data)
        {
            for (int i = 0; i < data.ChildNodes.Count; ++i)
            {
                var ndata = data.ChildNodes[i];
                var n = GenNode(node, ndata);

                var action = new System.Action<RectTransform, XmlNode>(Generate);
                action(n, ndata);
            }
        }

        private void Reset(RectTransform rootNode)
        {
            if (data == null ||
                data.bytes.Length <= 0)
                throw new System.Exception("No UGUI data inpute.");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(data.text);

            var root = doc["Root"];
            rootSize = new Vector2(
                float.Parse(root.Attributes["width"].Value),
                float.Parse(root.Attributes["height"].Value));
            Reset(rootNode, root);
        }

        private void Reset(RectTransform node, XmlNode data)
        {
            for (int i = 0; i < data.ChildNodes.Count; ++i)
            {
                var ndata = data.ChildNodes[i];
                var n = node.GetChild(i) as RectTransform;
                if (n == null)
                    throw new System.Exception("GUI node does not math the data.");
                SetNodeData(n, ndata);
                var action = new System.Action<RectTransform, XmlNode>(Reset);
                action(n, ndata);
            }
        }

        private RectTransform GenNode(RectTransform parent, XmlNode data)
        {
            var node = new GameObject("Node", typeof(RectTransform));
            var t = node.GetComponent<RectTransform>();
            t.SetParent(parent);
            SetNodeData(t, data);
            return t;
        }

        private void SetNodeData(RectTransform node, XmlNode data)
        {
            // 读取原始数据
            var x = float.Parse(data.Attributes["x"].Value);
            var y = float.Parse(data.Attributes["y"].Value);
            var w = float.Parse(data.Attributes["width"].Value);
            var h = float.Parse(data.Attributes["height"].Value);
            var anchor_min = StringToVector2(data.Attributes["anchor-min"].Value);
            var anchor_max = StringToVector2(data.Attributes["anchor-min"].Value);

            // stretch模式，按比例适配控件
            var X = x / rootSize.x * Width;
            var Y = y / rootSize.y * Height;
            var W = w / rootSize.x * Width;
            var H = h / rootSize.y * Height;
            var origin = new Rect(X, Y, W, H);

            // 先恢复anchors
            node.anchorMin = anchor_min;
            node.anchorMax = anchor_max;
            node.ForceUpdateRectTransforms();

            // 把UI约束在屏幕内
            W = w / h * H;
            var current = new Rect(X, Y, W, H);

            var align = PickSortingPoint(node);
            var pos = CalculateAnchorPos(align, origin, current);

            node.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, W);
            node.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, H);
            node.anchoredPosition = pos;
        }

        private Vector2 StringToVector2(string str)
        {
            var strs = str.Replace("(", "").Replace(")", "").Split(',');
            return new Vector2(
                float.Parse(strs[0]),
                float.Parse(strs[1]));
        }

        private Vector2 CalculateAnchorPos(UGUIGenAlignment align, Rect o, Rect c)
        {
            var result = new Vector2(c.x, c.y);

            switch (align)
            {
                case UGUIGenAlignment.RightTop:
                {
                    result.x += (o.width - c.width) * 0.5f;
                    result.y += (o.height - c.height) * 0.5f;
                    break;
                }
                case UGUIGenAlignment.RightMid:
                {
                    result.x += (o.width - c.width) * 0.5f;
                    if (result.y > 0)
                        result.y -= (o.height - c.height) * 0.5f;
                    if (result.y < 0)
                        result.y += (o.height - c.height) * 0.5f;
                    break;
                }
                case UGUIGenAlignment.RightBottom:
                {
                    result.x += (o.width - c.width) * 0.5f;
                    result.y -= (o.height - c.height) * 0.5f;
                    break;
                }
                case UGUIGenAlignment.MidTop:
                {
                    if (result.x < 0)
                        result.x += (o.width - c.width) * 0.5f;
                    if (result.x > 0)
                        result.x -= (o.width - c.width) * 0.5f;
                    result.y += (o.height - c.height) * 0.5f;
                    break;
                }
                case UGUIGenAlignment.Center:
                {
                    if (result.x < 0)
                        result.x += (o.width - c.width) * 0.5f;
                    if (result.x > 0)
                        result.x -= (o.width - c.width) * 0.5f;
                    if (result.y > 0)
                        result.y -= (o.height - c.height) * 0.5f;
                    if (result.y < 0)
                        result.y += (o.height - c.height) * 0.5f;
                    break;
                }
                case UGUIGenAlignment.MidBottom:
                {
                    if (result.x < 0)
                        result.x += (o.width - c.width) * 0.5f;
                    if (result.x > 0)
                        result.x -= (o.width - c.width) * 0.5f;
                    result.y -= (o.height - c.height) * 0.5f;
                    break;
                }
                case UGUIGenAlignment.LeftTop:
                {
                    result.x -= (o.width - c.width) * 0.5f;
                    result.y += (o.height - c.height) * 0.5f;
                    break;
                }
                case UGUIGenAlignment.LeftMid:
                {
                    result.x -= (o.width - c.width) * 0.5f;
                    if (result.y > 0)
                        result.y -= (o.height - c.height) * 0.5f;
                    if (result.y < 0)
                        result.y += (o.height - c.height) * 0.5f;
                    break;
                }
                case UGUIGenAlignment.LeftBottom:
                {
                    result.x -= (o.width - c.width) * 0.5f;
                    result.y -= (o.height - c.height) * 0.5f;
                    break;
                }
            }
            return result;
        }

        private UGUIGenAlignment PickSortingPoint(RectTransform node)
        {
            if (node.name == "Root")
                return UGUIGenAlignment.Center;

            if (node.anchorMin != node.anchorMax)
                throw new System.Exception("Gui using stretch sortting mode. Please use anchor.");

            var p = Vector2.zero;
            if (node.anchorMin == p) return UGUIGenAlignment.LeftBottom;

            p.x = 0; p.y = 0.5f;
            if (node.anchorMin == p) return UGUIGenAlignment.LeftMid;

            p.x = 0; p.y = 1;
            if (node.anchorMin == p) return UGUIGenAlignment.LeftTop;

            p.x = 0.5f; p.y = 0;
            if (node.anchorMin == p) return UGUIGenAlignment.MidBottom;

            p.x = 0.5f; p.y = 0.5f;
            if (node.anchorMin == p) return UGUIGenAlignment.Center;

            p.x = 0.5f; p.y = 1;
            if (node.anchorMin == p) return UGUIGenAlignment.MidTop;

            p.x = 1; p.y = 0;
            if (node.anchorMin == p) return UGUIGenAlignment.RightBottom;

            p.x = 1; p.y = 0.5f;
            if (node.anchorMin == p) return UGUIGenAlignment.RightMid;

            p.x = 1; p.y = 1;
            if (node.anchorMin == p) return UGUIGenAlignment.RightTop;

            return UGUIGenAlignment.LeftBottom;
        }
    }
}

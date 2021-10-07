using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(RangeBeginEndAttribute))]
public class RangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        RangeBeginEndAttribute outer_bounds = attribute as RangeBeginEndAttribute;
        SerializedProperty min_value = property.FindPropertyRelative("start");
        SerializedProperty length_value = property.FindPropertyRelative("length");

        if (fieldInfo.FieldType == typeof(RangeFloat))
        {
            position.height -= 16f;
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            float begin = min_value.floatValue;
            float end = begin + length_value.floatValue;

            Rect left = new Rect(position.x, position.y, position.width / 2 - 11f, position.height);
            Rect right = new Rect(position.x + position.width - left.width, position.y, left.width, position.height);
            Rect mid = new Rect(left.xMax, position.y, 22, position.height);
            begin = Mathf.Clamp(EditorGUI.DelayedFloatField(left, begin), outer_bounds.min, outer_bounds.max);
            EditorGUI.LabelField(mid, " to ");
            end = Mathf.Clamp(EditorGUI.DelayedFloatField(right, end), outer_bounds.min, outer_bounds.max);

            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.MinMaxSlider(position, ref begin, ref end, outer_bounds.min, outer_bounds.max);

            //Swap beginning and end if inverted
            if (end < begin)
            {
                float temp = begin;
                begin = end;
                end = temp;
            }

            min_value.floatValue = (float)System.Math.Round(begin, 2);
            length_value.floatValue = (float)System.Math.Round(end - begin, 2);

            EditorGUI.EndProperty();
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Invalid Type for Range");
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.serializedObject.isEditingMultipleObjects) return 0f;
        return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight;
    }
}

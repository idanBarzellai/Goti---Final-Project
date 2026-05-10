#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PieceData))]
public class PieceDataDrawer : PropertyDrawer
{
    private const float LineHeight = 20f;
    private const float Spacing = 4f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty pieceType = property.FindPropertyRelative("pieceType");
        SerializedProperty gridPosition = property.FindPropertyRelative("gridPosition");
        SerializedProperty direction = property.FindPropertyRelative("direction");
        SerializedProperty isRequired = property.FindPropertyRelative("isRequired");
        SerializedProperty canRotate = property.FindPropertyRelative("canRotate");

        bool isInventoryPiece = property.propertyPath.Contains("inventoryPieces");
        PieceType currentType = (PieceType)pieceType.enumValueIndex;

        bool showRequired =
            currentType != PieceType.Entry &&
            currentType != PieceType.Target;

        EditorGUI.BeginProperty(position, label, property);

        Rect foldoutRect = new Rect(position.x, position.y, position.width, LineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            float y = position.y + LineHeight + Spacing;

            DrawProperty(ref y, position, pieceType);

            if (!isInventoryPiece)
                DrawProperty(ref y, position, gridPosition);

            if (!isInventoryPiece)
                DrawProperty(ref y, position, direction);

            if (showRequired)
                DrawProperty(ref y, position, isRequired);

            if (!isInventoryPiece)
                DrawProperty(ref y, position, canRotate);

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return LineHeight;

        SerializedProperty pieceType = property.FindPropertyRelative("pieceType");

        bool isInventoryPiece = property.propertyPath.Contains("inventoryPieces");
        PieceType currentType = (PieceType)pieceType.enumValueIndex;

        bool showRequired =
            currentType != PieceType.Entry &&
            currentType != PieceType.Target;

        int lines = 1; // foldout
        lines++; // pieceType

        if (!isInventoryPiece)
            lines++; // gridPosition

        if (!isInventoryPiece)
            lines++; // direction

        if (showRequired)
            lines++; // isRequired

        if (!isInventoryPiece)
            lines++; // canRotate

        return lines * (LineHeight + Spacing);
    }

    private void DrawProperty(ref float y, Rect position, SerializedProperty property)
    {
        Rect rect = new Rect(position.x, y, position.width, LineHeight);
        EditorGUI.PropertyField(rect, property);
        y += LineHeight + Spacing;
    }
}
#endif
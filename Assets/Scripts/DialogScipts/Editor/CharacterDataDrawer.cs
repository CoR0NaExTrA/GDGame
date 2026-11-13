using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Linq;

//[CustomPropertyDrawer(typeof(CharacterData))]
public class CharacterDataDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // 1. Find all CharacterData assets in the project
        string[] guids = AssetDatabase.FindAssets("t:CharacterData");
        Debug.Log(property);

        // 2. Load them into a list
        var allData = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<CharacterData>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(data => data != null)
            .ToArray();

        // 3. Extract just the names for the dropdown
        string[] choices = allData.Select(d => d.name).ToArray();

        // 4. Figure out which one is currently selected
        int currentIndex = System.Array.IndexOf(allData, property.objectReferenceValue as CharacterData);

        // If the object hasn't been named yet, display a fallback
        if (currentIndex == -1 && property.objectReferenceValue == null)
            currentIndex = 0; // Default to the first item

        // 5. Create the Dropdown (PopupField)
        // The label comes from the variable name (e.g., "Target Actor")
        var popup = new PopupField<string>(property.displayName, choices.ToList(), currentIndex);

        // 6. Update the variable when the user changes the dropdown
        popup.RegisterValueChangedCallback(evt =>
        {
            int newIndex = popup.index;
            property.objectReferenceValue = allData[newIndex];
            property.serializedObject.ApplyModifiedProperties();
        });

        return popup;
    }
}
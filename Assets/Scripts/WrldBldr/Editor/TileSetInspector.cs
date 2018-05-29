using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(TileSet))]
public class TileSetInspector : Editor
{
	private AnimBool tilesFadeGroupVal;

	public void OnEnable()
	{
		tilesFadeGroupVal = new AnimBool (false);
		tilesFadeGroupVal.valueChanged.AddListener (Repaint);
	}

	public override void OnInspectorGUI()
	{
		TileSet set = (TileSet)target;
		SerializedObject obj = new SerializedObject (set);
		EditorGUILayout.LabelField ("Tiles", EditorStyles.boldLabel);

		//array property
		SerializedProperty tiles = obj.FindProperty ("tiles");

		//toggle fade group
		if (GUILayout.Button ("Edit"))
		{
			tilesFadeGroupVal.target = !tilesFadeGroupVal.target;
		}
		if (EditorGUILayout.BeginFadeGroup (tilesFadeGroupVal.faded))
		{
			EditorGUI.indentLevel++;
			//start array indexes
			for (int i = 0; i < tiles.arraySize; i++)
			{
				EditorGUILayout.PropertyField (tiles.GetArrayElementAtIndex (i), new GUIContent (i.ToString ()));
			}
			EditorGUI.indentLevel--;
		}
		EditorGUILayout.EndFadeGroup ();

		obj.ApplyModifiedProperties ();
	}
}
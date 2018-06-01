using UnityEngine;
using UnityEditor;

namespace WrldBldr
{
	[CustomEditor(typeof(Section))]
	public class SectionInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			Section room = (Section)target;

			EditorGUILayout.LabelField (room.getArchetype().ToString());
			EditorGUILayout.LabelField (System.Convert.ToString (room.getAdjMask (), 2).PadLeft (16, '0'));
		}
	}
}
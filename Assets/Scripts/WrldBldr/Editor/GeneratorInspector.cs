using UnityEngine;
using UnityEditor;

namespace WrldBldr
{
	[CustomEditor(typeof(Generator))]
	public class GeneratorInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector ();
			Generator g = (Generator)target;

			
			GUILayout.Label ("Stats", EditorStyles.boldLabel);

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Section Count\n(Current/Target):");
			GUILayout.Label (g.getStartRegion().getFullSectionCount() + " / " + g.getStartRegion ().getFullTargetSize ());
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Region Count:");
			GUILayout.Label (getRegionCount(g.getStartRegion ()).ToString());
			GUILayout.EndHorizontal ();

			if (GUILayout.Button ("Generate"))
			{
				Debug.Log ("".PadRight (100, '-'));
				g.generate ();
			}

			GUI.enabled = false;
			if (GUILayout.Button ("Generate NOW"))
				g.generateImmediate ();
		}

		private int getRegionCount(Region r)
		{
			int count = 0;
			if (r == null)
				return count;
			for (int i = 0; i < r.getSubRegionCount (); i++)
			{
				count += getRegionCount (r.getSubRegion (i));
			}
			return count;
		}
	}
}

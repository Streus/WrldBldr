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
			GUILayout.Label ("Section Count:");
			GUILayout.Label (g.getStartRegion ().getFullTargetSize ().ToString ());
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Region Count:");
			GUILayout.Label (getRegionCount(g.getStartRegion ()).ToString());
			GUILayout.EndHorizontal ();

			if (GUILayout.Button ("Generate"))
				g.generate ();
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

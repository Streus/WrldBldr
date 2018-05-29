using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WrldBldr
{
	public class Generator : MonoBehaviour
	{
		#region STATIC_VARS

		private static Generator instance;
		#endregion

		#region INSTANCE_VARS

		[Header("Options")]
		[SerializeField]
		private Region startRegion;

		[SerializeField]
		private Vector3 sectionScale = Vector3.one;

		[SerializeField]
		private float generationDelay = 0f;

		#endregion

		#region STATIC_METHODS

		public static Generator getInstance()
		{
			return instance;
		}
		#endregion

		#region INSTANCE_METHODS

		public void Awake()
		{
			if (instance == null)
			{
				instance = this;
			}
			else
			{
				Debug.LogWarning ("An instance of " + typeof (Generator).Name + " already exists!");
#if UNITY_EDITOR
				UnityEditor.EditorGUIUtility.PingObject (instance);
#endif
				Destroy (this);
			}
		}

		public Region getStartRegion()
		{
			return startRegion;
		}

		public Vector3 getSectionScale()
		{
			return sectionScale;
		}

		public float getGenerationDelay()
		{
			return generationDelay;
		}

		public void generate()
		{
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying)
				return;
#endif
			startRegion.beginPlacement ();
		}

		public void generateImmediate()
		{
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying)
				return;
#endif
			startRegion.beginPlacement (false);
		}
		#endregion

		#region INTERNAL_TYPES

		#endregion
	}
}

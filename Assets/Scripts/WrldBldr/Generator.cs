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
		#endregion

		#region INTERNAL_TYPES

		#endregion
	}
}

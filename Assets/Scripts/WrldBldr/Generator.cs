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

		[Header("Generation Options")]
		[SerializeField]
		private Region startRegion;

		[SerializeField]
		private Vector3 sectionScale = Vector3.one;

		[SerializeField]
		private float generationDelay = 0f;

		[Header ("Tileset Options")]
		[SerializeField]
		private TileSet[] tileSets;

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

		/// <summary>
		/// Basic generation method via coroutines. Increases runtime but less
		/// likely to freeze up.
		/// </summary>
		public void generate()
		{
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying)
				return;
#endif
			startRegion.generationCompleted += placeTiles;
			startRegion.beginPlacement ();
		}

		/// <summary>
		/// Generate everything in a single update.  Will most likely cause
		/// freezes.
		/// </summary>
		public void generateImmediate()
		{
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying)
				return;
#endif
			startRegion.generationCompleted += placeTiles;
			startRegion.beginPlacement (false);
		}

		private void placeTiles()
		{
			startRegion.generationCompleted -= placeTiles;
			Debug.Log ("Generation Complete!");
		}

		public void OnGUI()
		{
			
		}
		#endregion

		#region INTERNAL_TYPES

		#endregion
	}
}

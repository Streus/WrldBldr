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

		private string statusText;

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
				statusText = "Initializing";
			}
			else
			{
				Debug.LogWarning ("[WB] An instance of " + typeof (Generator).Name + " already exists!");
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

		public float getGenerationProgress()
		{
			return (float)startRegion.getFullSectionCount () / startRegion.getFullTargetSize ();
		}

		public string getCurrentStageText()
		{
			return statusText;
		}

		/// <summary>
		/// Create a dungeon using the current starting region
		/// </summary>
		/// <param name="immediate">True to use coroutine method, false to generate in one step</param>
		public void generate(bool immediate)
		{
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying)
				return;
#endif
			startRegion.generationCompleted += endGeneration;
			startRegion.beginPlacement (!immediate);
			statusText = "Generating Regions";
		}

		private void endGeneration()
		{
			startRegion.generationCompleted -= endGeneration;
			Debug.Log ("[WB] Generation Complete!");
			TileSet set = tileSets[Random.Range (0, tileSets.Length - 1)];
			if (set == null)
			{
				Debug.LogError ("[WB] Null tileset in tileset array.");
				return;
			}
			StartCoroutine (placeTiles (set));
			statusText = "Placing Tiles";
		}

		private IEnumerator placeTiles(TileSet set)
		{
			yield return null;
			Debug.Log ("[WB] Placing Tiles.\nUsing " + set.name + " tile set.");


		}
		#endregion

		#region INTERNAL_TYPES

		#endregion
	}
}

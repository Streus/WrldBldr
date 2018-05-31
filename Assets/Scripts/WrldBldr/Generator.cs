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

		[SerializeField]
		private bool immediateGeneration = false;

		[Header ("Tileset Options")]
		[SerializeField]
		private TileSet[] tileSets;

		private GenerationStage stage;

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
			int stages = System.Enum.GetValues (typeof (GenerationStage)).Length;
			float regionGen = (float)startRegion.getFullSectionCount () / startRegion.getFullTargetSize ();
			float tilePlacement = 0f;
			return (regionGen / stages) + (tilePlacement / stages);
		}

		public string getCurrentStageText()
		{
			switch (stage)
			{
			case GenerationStage.region_gen:
				return "Generating Regions";
			case GenerationStage.tile_placement:
				return "Placing Tiles";
			}
			return "";
		}

		/// <summary>
		/// Create a dungeon using the current starting region
		/// </summary>
		public void generate()
		{
#if UNITY_EDITOR
			if (!UnityEditor.EditorApplication.isPlaying)
				return;
#endif
			stage = GenerationStage.region_gen;
			startRegion.generationCompleted += endGeneration;
			startRegion.beginPlacement (!immediateGeneration);
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
			stage = GenerationStage.tile_placement;
			if (!immediateGeneration)
				StartCoroutine (placeTiles (set));
			else
			{
				IEnumerator m = placeTiles (set);
				while (m.MoveNext ()) { }
			}
		}

		private IEnumerator placeTiles(TileSet set)
		{
			yield return null;
			Debug.Log ("[WB] Placing Tiles.\nUsing " + set.name + " tile set.");
			Queue<Region> regions = new Queue<Region> ();
			getRegionSubregions (startRegion, regions);

			while (regions.Count > 0)
			{
				regions.Dequeue ().placeTiles (set);
				yield return new WaitForSeconds (getGenerationDelay ());
			}
		}

		private void getRegionSubregions(Region start, Queue<Region> regions)
		{
			regions.Enqueue (start);
			for (int i = 0; i < start.getSubRegionCount (); i++)
			{
				if(start.getSubRegion(i) != null)
					getRegionSubregions (start.getSubRegion (i), regions);
			}
		}
		#endregion

		#region INTERNAL_TYPES
		private enum GenerationStage
		{
			region_gen, tile_placement, feature_placement
		}
		#endregion
	}
}

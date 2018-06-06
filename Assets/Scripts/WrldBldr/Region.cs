using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace WrldBldr
{
	public class Region : MonoBehaviour
	{
		#region INSTANCE_VARS

		[SerializeField]
		private Color debugColor;

		[SerializeField]
		private int targetSize = 30;

		[SerializeField]
		private bool hasEnd = true;

		// List of all the regions that branch from this region
		[SerializeField]
		private Region[] subRegions;

		// List of all the sections that are members of this region
		private List<Section> sections;

		// Used to track generation cycle progress
		private bool generationDone = false;
		public event BasicNotify generationCompleted;

		#endregion

		#region INSTANCE_METHODS

		public void Awake()
		{
			sections = new List<Section> ();
		}

		public int getTargetSize()
		{
			return targetSize;
		}

		public int getFullTargetSize()
		{
			int fts = targetSize;
			for (int i = 0; i < subRegions.Length; i++)
			{
				if (subRegions[i] != null)
					fts += subRegions[i].getFullTargetSize ();
			}
			return fts;
		}

		public Color getDebugColor()
		{
			return debugColor;
		}

		public int getSectionCount()
		{
			if(sections != null)
				return sections.Count;
			return 0;
		}

		public int getFullSectionCount()
		{
			int fsc = getSectionCount ();
			for (int i = 0; i < subRegions.Length; i++)
			{
				if (subRegions[i] != null)
					fsc += subRegions[i].getFullSectionCount ();
			}
			return fsc;
		}

		public Section getSection(int index)
		{
			return sections[index];
		}

		public int getSubRegionCount()
		{
			return subRegions.Length;
		}

		public Region getSubRegion(int index)
		{
			return subRegions[index];
		}

		public void clear()
		{
			StopAllCoroutines ();

			//destroy old map
			if (sections != null)
			{
				for (int i = 0; i < sections.Count; i++)
				{
					try
					{
						Destroy (sections[i].gameObject);
					}
					catch (MissingReferenceException) { }
				}
				sections.Clear ();
			}

			for (int i = 0; i < subRegions.Length; i++)
			{
				if(subRegions[i] != null)
					subRegions[i].clear ();
			}
		}

		public void beginPlacement(bool distributed = true)
		{
			clear ();

			//start new map
			Section start = Section.create (this, Vector2.zero, Section.Archetype.start);
			start.gameObject.name += " 0";
			sections = new List<Section> ();
			sections.Add (start);
			start.assignSet (this);

			if (distributed)
				StartCoroutine (placeSections (start));
			else
			{
				IEnumerator m = placeSections (start, distributed);
				while (m.MoveNext ()) { }
			}
		}

		private IEnumerator placeSections(Section start, bool distributed = true)
		{
			//make the rooms
			Queue<Section> activeRooms = new Queue<Section> ();
			activeRooms.Enqueue (start);
			Section curr;
			Section prev = null;

			//main placement loop
			while (sections.Count <= targetSize)
			{
				//pop off the next room
				//if no new active rooms were made, return to the last active room processed
				try
				{
					curr = activeRooms.Dequeue ();
				}
				catch (System.InvalidOperationException)
				{
					curr = findFreeSection(sections.Count - 1);
					if (curr == null)
					{
						throw new System.Exception("Failed to finish " + gameObject.name + ". No available space to expand!");
					}
				}

				//set the color of the currently active room
				//only done in distributed generation cycles
				if (distributed)
					curr.selected = true;

				//place a random number of rooms in the available free spaces
				Section.AdjDirection[] deck = curr.getFreeRooms ();
				if (deck.Length > 0)
				{
					int subRooms = Random.Range (1, deck.Length);
					shuffleDeck (deck, 1);
					for (int i = 0; i < subRooms; i++)
					{
						Section r = makeRoom (curr, deck[i]);
						if (r != null)
						{
							activeRooms.Enqueue (r);
						}
					}
				}

				yield return new WaitForSeconds (Generator.getInstance().getGenerationDelay());

				//return the color of the active room
				if (distributed)
					curr.selected = false;

				prev = curr;
			}

			if (subRegions.Length > 0)
			{
				int lastSection = sections.Count - 1;
				//generate subregions
				for (int i = 0; i < subRegions.Length; i++)
				{
					//skip null entries
					if (subRegions[i] == null)
						continue;

					//listen to subregion's generation completion event
					subRegions[i].generationCompleted += tryNotifyCompleted;

					Section subStart = findFreeSection (ref lastSection);
					if (subStart != null)
					{
						//pass off ownership of new start to subregion
						subStart.assignSet (subRegions[i]);
						subRegions[i].sections.Add (subStart);
						sections.Remove (subStart);

						//start generation of subregion
						if (distributed)
							subRegions[i].StartCoroutine (subRegions[i].placeSections (subStart));
						else
						{
							IEnumerator m = subRegions[i].placeSections (subStart, distributed);
							while (m.MoveNext ())
							{ }
						}
						lastSection -= 3;
					}
					else
					{
						throw new System.Exception ("Failed to start " + gameObject.name + ". No available sections for a start!");
					}
				}
			}
			else
			{
				if (hasEnd)
				{
					//done, mark the end
					prev.setArchtype (Section.Archetype.end);
				}
				generationDone = true;
				if (generationCompleted != null)
					generationCompleted ();
			}
		}

		private Section makeRoom(Section parent, Section.AdjDirection dir, Section.Archetype type = Section.Archetype.normal)
		{
			//check for overlap
			Collider2D col = Physics2D.OverlapPoint (Section.getDirection (dir) + (Vector2)parent.transform.position, Physics2D.AllLayers);
			if (col != null)
			{
				//found overlap, set link to overlap
				Section r = col.GetComponent<Section> ();
				if (r.checkSet (this))
					parent.setAdjRoom (dir, r);
				return null;
			}

			//no overlap, make a new room
			Section child = parent.addAdjRoom (dir);
			child.gameObject.name += " " + sections.Count;
			child.assignSet (this);
			sections.Add (child);
			return child;
		}

		private void shuffleDeck(Section.AdjDirection[] deck, int times)
		{
			for (int i = 0; i < times; i++)
			{
				for (int j = 0; j < deck.Length; j++)
				{
					int swapIndex = (int)(Random.value * deck.Length);
					Section.AdjDirection temp = deck[swapIndex];
					deck[swapIndex] = deck[j];
					deck[j] = temp;
				}
			}
		}

		/// <summary>
		/// Traverses backwards through the section list to find a section with at least
		/// one free adjacent space
		/// </summary>
		/// <returns></returns>
		private Section findFreeSection(ref int startingIndex)
		{
			for (; startingIndex >= 0; startingIndex--)
			{
				if (sections[startingIndex].hasFreeAdjSpace())
				{
					return sections[startingIndex];
				}
			}
			return null;
		}
		private Section findFreeSection(int startingIndex)
		{
			return findFreeSection (ref startingIndex);
		}

		/// <summary>
		/// Will invoke the generationCompleted event if all subregions have finished generation
		/// </summary>
		private void tryNotifyCompleted()
		{
			if (getSubRegionCompletion () && generationCompleted != null)
				generationCompleted ();
		}

		/// <summary>
		/// Get the completion status of all subregions
		/// </summary>
		/// <returns>True if all subregions have finished, false otherwise</returns>
		private bool getSubRegionCompletion()
		{
			//FIXME this don't work in chain region cases
			if (!generationDone)
				return false;
			for (int i = 0; i < subRegions.Length; i++)
			{
				if (subRegions[i] == null)
					continue;

				if (!subRegions[i].getSubRegionCompletion())
					return false;
			}
			return true;
		}

		/// <summary>
		/// Tell each section in this region to pick and place a tile from the given set based on their adjacency state
		/// </summary>
		/// <param name="set">The set to take tiles from</param>
		public void placeTiles(TileSet set)
		{
			for (int i = 0; i < sections.Count; i++)
			{
				sections[i].chooseTile (set);
			}
		}
		#endregion

		#region INTERNAL_TYPES

		public delegate void BasicNotify();
		#endregion
	}
}

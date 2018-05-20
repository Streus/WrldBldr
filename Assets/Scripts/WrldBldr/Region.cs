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

		// 
		[SerializeField]
		private Region[] subRegions;

		// List of all the sections that are members of this region
		private List<Section> sections;

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
				subRegions[i].clear ();
		}

		public void beginPlacement()
		{
			clear ();

			//start new map
			Section start = Section.create (this, Vector2.zero, Section.Archetype.start);
			sections = new List<Section> ();
			sections.Add (start);

			StartCoroutine (placeRooms (start));
		}

		private IEnumerator placeRooms(Section start)
		{
			//make the rooms
			Queue<Section> activeRooms = new Queue<Section> ();
			activeRooms.Enqueue (start);
			Section curr;
			Section prev = null;

			System.Random rgen = new System.Random (System.DateTime.Now.Millisecond);

			//main placement loop
			while (sections.Count < targetSize + 1)
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
				Color currDefCol = curr.getColor ();
				curr.setColor (Color.yellow);

				//place a random number of rooms in the available free spaces
				Section.AdjDirection[] deck = curr.getFreeRooms ();
				if (deck.Length > 0)
				{
					int subRooms = rgen.Next (1, deck.Length);
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
				curr.setColor (currDefCol);
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

					Section subStart = findFreeSection (ref lastSection);
					if (subStart != null)
					{
						subStart.setColor (subRegions[i].debugColor);
						subStart.assignSet (subRegions[i]);
						subRegions[i].StartCoroutine (subRegions[i].placeRooms (subStart));
						lastSection -= 2;
					}
					else
					{
						throw new System.Exception ("Failed to start " + gameObject.name + ". No available sections for a start!");
					}
				}
			}
			else if (hasEnd)
				//done, mark the end
				prev.setArchtype (Section.Archetype.end);
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
				if (sections[startingIndex].getFreeRooms ().Length > 0 && sections[startingIndex].hasFreeAdjSpace())
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
		#endregion
	}
}

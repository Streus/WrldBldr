using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a physical section of a generated Region.
/// Maintains references to all adjacent sections.
/// </summary>
namespace WrldBldr
{
	public class Section : MonoBehaviour
	{
		#region STATIC_VARS

		private const string prefabPath = "Section";

		#endregion

		#region INSTANCE_VARS

		public bool selected = false;

		private Archetype archtype;

		private Section[] adjSections;

		private Region set;
		#endregion

		#region STATIC_METHODS

		public static Section create(Vector2 position, Archetype type = Archetype.normal)
		{
			GameObject sec = new GameObject (typeof (Section).Name);
			Section s = sec.AddComponent<Section> ();
			Bounds b = sec.AddComponent<BoxCollider2D> ().bounds;
			b.size = new Vector3 (0.9f, 0.9f, 0f);

			sec.transform.position = position;
			sec.transform.rotation = Quaternion.identity;
			sec.transform.localScale = Generator.getInstance ().getSectionScale ();

			s.setArchtype (type);
			return s;
		}

		public static Section create(Region set, Vector2 position, Archetype type = Archetype.normal)
		{
			Section r = create (position, type);
			r.set = set;
			r.transform.SetParent (set.transform, true);
			return r;
		}

		public static Color getArchetypeColor(Archetype type)
		{
			switch (type)
			{
			case Archetype.normal:
				return new Color(0f, 0f, 0f, 0.5f);

			case Archetype.start:
				return Color.white;

			case Archetype.end:
				return Color.gray;

			default:
				return Color.magenta;
			}
		}

		/// <summary>
		/// Gets a random cardinal direction (up, down, left, or right)
		/// </summary>
		/// <returns>AdjDirection representing a cardinal direction</returns>
		public static AdjDirection getRandomDirection()
		{
			int d = Random.Range (0, System.Enum.GetNames (typeof (AdjDirection)).Length - 1);
			d = (d / 2) * 2;

			return (AdjDirection)d;
		}

		public static AdjDirection offsetDirection(AdjDirection dir, int amount)
		{
			int max = System.Enum.GetNames (typeof (AdjDirection)).Length;
			while (amount < 0)
				amount += max;
			return (AdjDirection)(((int)dir + amount) % max);
		}

		public static Vector2 getDirection(AdjDirection dir)
		{
			int offset = (int)dir;
			float segments = System.Enum.GetNames (typeof (AdjDirection)).Length;
			float angle = (offset / segments) * 360f;
			Vector2 direction = Quaternion.Euler (0f, 0f, angle) * Vector2.right;
			direction.Scale (Generator.getInstance ().getSectionScale ());
			return direction;
		}
		#endregion

		#region INSTANCE_METHODS

		public void Awake()
		{
			adjSections = new Section[System.Enum.GetNames (typeof (AdjDirection)).Length];
		}

#if UNITY_EDITOR
		public void OnDrawGizmos()
		{
			if (selected)
				Gizmos.color = Color.yellow;
			else
				Gizmos.color = getArchetypeColor (getArchetype ());
			
			Gizmos.DrawCube (transform.position, transform.localScale);

			Gizmos.color = set.getDebugColor ();
			Gizmos.DrawWireCube (transform.position, transform.localScale);
			Gizmos.color = Color.white;
			for (int i = 0; i < adjSections.Length; i++)
			{
				if (adjSections[i] == null)
					continue;

				Gizmos.DrawLine (transform.position, adjSections[i].transform.position);
			}
		}
#endif

		public void assignSet(Region set)
		{
			this.set = set;
			transform.SetParent (set.transform, true);
		}

		/// <summary>
		/// Checks if this Section is part of the given set
		/// </summary>
		/// <param name="set"></param>
		/// <returns></returns>
		public bool checkSet(Region set)
		{
			return this.set == set;
		}

		public Archetype getArchetype()
		{
			return archtype;
		}

		public void setArchtype(Archetype type)
		{
			archtype = type;
		}

		public Section getAdjRoom(AdjDirection index)
		{
			return adjSections[(int)index];
		}

		/// <summary>
		/// Make a connection between this section and another. Also makes diagonal connections where valid.
		/// </summary>
		/// <param name="index">The direction relative to this section the connection is being made</param>
		/// <param name="room">The section being connected to</param>
		/// <param name="reverseConnections">Make connections from the other section to this section</param>
		public void setAdjRoom(AdjDirection index, Section room, bool reverseConnections = true)
		{
			AdjDirection diagDir;
			Section diag;
			float r = Random.value, chance = 0.2f;

			//outgoing connections
			adjSections[(int)index] = room;
			diag = room.adjSections[(int)offsetDirection (index, 2)];
			if (diag != null && r < chance)
			{
				//outgoing and incoming diagonal connection
				diagDir = offsetDirection (index, 1);
				adjSections[(int)diagDir] = diag;
				diag.adjSections[(int)offsetDirection(diagDir, 4)] = this;
			}
			diag = room.adjSections[(int)offsetDirection (index, -2)];
			if (diag != null && r < chance)
			{
				//outgoing and incoming diagonal connection
				diagDir = offsetDirection (index, -1);
				adjSections[(int)diagDir] = diag;
				diag.adjSections[(int)offsetDirection(diagDir, 4)] = this;
			}

			//incoming connections
			if(reverseConnections)
				room.setAdjRoom (offsetDirection(index, 4), this, false);
		}

		/// <summary>
		/// Add an adjacent room in the specified direction
		/// </summary>
		/// <param name="dir"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public Section addAdjRoom(AdjDirection dir, Archetype type = Archetype.normal)
		{
			Vector2 direction = getDirection (dir);
			Section adj = create ((Vector2)transform.position + direction, type);
			setAdjRoom (dir, adj);
			return adj;
		}

		/// <summary>
		/// Add an adjacent room in a direction not already occupied
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public Section addAdjRoom(out AdjDirection dir, Archetype type = Archetype.normal)
		{
			while (getAdjRoom (dir = getRandomDirection ()) != null)
			{ }
			return addAdjRoom (dir, type);
		}

		public AdjDirection[] getFreeRooms()
		{
			List<AdjDirection> rooms = new List<AdjDirection> ();
			for (int i = 0; i < adjSections.Length; i += 2)
			{
				if (adjSections[i] == null)
					rooms.Add ((AdjDirection)i);
			}

			return rooms.ToArray ();
		}

		public bool isAdjSpaceFree(AdjDirection dir)
		{
			Vector2 d = getDirection (dir);
			Collider2D col = Physics2D.OverlapPoint (d + (Vector2)transform.position, Physics2D.AllLayers);
			return col == null;
		}

		public bool hasFreeAdjSpace()
		{
			for (int i = 0; i < adjSections.Length; i += 2)
			{
				if (isAdjSpaceFree ((AdjDirection)i))
					return true;
			}
			return false;
		}

		public int getAdjMask()
		{
			int mask = 0;
			for (int i = 0; i < adjSections.Length; i++)
			{
				if (adjSections[i] != null)
				{
					mask |= 1 << (i * 2);
				}
			}

			return mask;
		}

		public void chooseTile(TileSet set)
		{
			float rotation;
			GameObject go = set.getTile (getAdjMask (), out rotation);
			if (go == null)
				throw new System.NullReferenceException ("Tileset returned a null tile! Set: " + set.name + ".");
			GameObject inst = Instantiate<GameObject> (go, transform, false);
			inst.transform.Rotate (0f, 0f, rotation);
		}
		#endregion

		#region INTERNAL_TYPES

		public enum Archetype
		{
			normal, start, end
		}

		public enum AdjDirection
		{
			right, up_right, up, up_left, left, down_left, down, down_right
		}
		#endregion
	}
}

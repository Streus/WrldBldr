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

		private static System.Random randGen;
		#endregion

		#region INSTANCE_VARS

		[SerializeField]
		private SpriteRenderer rend;

		private Archetype archtype;

		private Section[] adjSections;

		private Region set;
		#endregion

		#region STATIC_METHODS

		static Section()
		{
			randGen = new System.Random (System.DateTime.Now.Millisecond);
		}

		public static Section create(Vector2 position, Archetype type = Archetype.normal)
		{
			GameObject pref = Resources.Load<GameObject> (prefabPath);
			GameObject inst = Instantiate<GameObject> (pref, position, Quaternion.identity);
			Section room = inst.GetComponent<Section> ();
			room.setArchtype (type);
			return room;
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
				return Color.black;

			case Archetype.start:
				return Color.white;

			case Archetype.end:
				return Color.red;

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
			int d = randGen.Next (0, System.Enum.GetNames (typeof (AdjDirection)).Length - 1);
			d = (d / 2) * 2;

			return (AdjDirection)d;
		}

		public static Vector2 getDirection(AdjDirection dir)
		{
			int offset = (int)dir;
			float segments = System.Enum.GetNames (typeof (AdjDirection)).Length;
			float angle = (offset / segments) * 360f;
			Vector2 direction = Quaternion.Euler (0f, 0f, angle) * Vector2.right;
			return direction;
		}
		#endregion

		#region INSTANCE_METHODS

		public void Awake()
		{
			adjSections = new Section[System.Enum.GetNames (typeof (AdjDirection)).Length];
		}

		public void Update()
		{
			for (int i = 0; i < adjSections.Length; i++)
			{
				if (adjSections[i] == null)
					continue;

				Debug.DrawLine (transform.position, adjSections[i].transform.position, set.getDebugColor());
			}
		}

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

		public Color getColor()
		{
			return rend.color;
		}

		public void setColor(Color val)
		{
			rend.color = val;
		}

		public Archetype getArchetype()
		{
			return archtype;
		}

		public void setArchtype(Archetype type)
		{
			archtype = type;
			setColor (getArchetypeColor (type));
		}

		public Section getAdjRoom(AdjDirection index)
		{
			return adjSections[(int)index];
		}

		public void setAdjRoom(AdjDirection index, Section room)
		{
			adjSections[(int)index] = room;
			room.adjSections[((int)index + 4) % adjSections.Length] = this;
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
			direction.Scale (transform.localScale);
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
			Collider2D col = Physics2D.OverlapPoint (getDirection (dir) + (Vector2)transform.position, Physics2D.AllLayers);
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
					mask |= 1 << i;
				}
			}

			return mask;
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

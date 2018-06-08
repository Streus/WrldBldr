using UnityEngine;

[CreateAssetMenu(menuName = "WrldBldr/World Tile Set")]
public class TileSet : ScriptableObject
{
	#region INSTANCE_VARS

	[SerializeField]
	private Tile[] tiles = new Tile[15];
	#endregion

	#region INSTANCE_METHODS

	public void Awake()
	{
		/*
		 * Quaternary Adjacency Representation
		 * 0 -> No neighbor
		 * 1 -> Neighbor
		 * 2 -> Ignored
		 * 3 -> (unused)
		 */

		tiles = new Tile[15];
		tiles[ 0] = new Tile ("Solid Block",			0x0   , 1);
		tiles[ 1] = new Tile ("Corridor End",			0x9998, 4);
		tiles[ 2] = new Tile ("Corridor Corner",		0x811 , 4);
		tiles[ 3] = new Tile ("Room Corner",			0x8895, 4);
		tiles[ 4] = new Tile ("Corridor",				0x8989, 2);
		tiles[ 5] = new Tile ("T-Corridor",				0x1891, 4);
		tiles[ 6] = new Tile ("Corridor Side Entry 1",	0x5891, 4);
		tiles[ 7] = new Tile ("Corridor Side Entry 2",	0x1895, 4);
		tiles[ 8] = new Tile ("Room Wall",				0x5895, 4);
		tiles[ 9] = new Tile ("Corridor Cross",			0x1111, 1);
		tiles[10] = new Tile ("Corridor Corner Entry",	0x1115, 4);
		tiles[11] = new Tile ("Corridor Wall Entry",	0x5115, 4);
		tiles[12] = new Tile ("Strait",					0x1515, 2);
		tiles[13] = new Tile ("Inner Corner",			0x5515, 4);
		tiles[14] = new Tile ("Room Center",			0x5555, 1);
	}

	public void Reset()
	{
		Awake ();
	}

	public GameObject getTile(int adjMask, out float rotation)
	{
		rotation = 0f;

		for (int i = tiles.Length - 1; i >= 0; i--)
		{
			int cv = tiles[i].getCheckVector ();

			//check all four orientations of the tile
			for (int rot = 0; rot < tiles[i].getRotations(); rot++)
			{
				if (cv == QARAnd (cv, adjMask))
				{
					rotation = 90f * rot;
					return tiles[i].prefab;
				}

				//rotate the check vector
				cv = Tile.rotateCVLeft (cv, 4, 16);
			}
		}

		throw new System.ArgumentException ("Given mask (" + System.Convert.ToString (adjMask, 2).PadLeft (16, '0') + ") does not " +
			"map to a valid tile!");
	}

	private int QARAnd(int left, int right)
	{
		string l, r, f;
		l = System.Convert.ToString (left, 2).PadLeft (16, '0');
		r = System.Convert.ToString (right, 2).PadLeft (16, '0');

		int res = 0x0;
		for (int i = 0; i < 16; i += 2)
		{
			//isolate two least sig bits of each operand
			int lm = left & 0x3, rm = right & 0x3;

			//if either is the ignored value, pass it through
			if (lm == 0x2 || rm == 0x2)
				res |= 0x2 << i;
			//else do regular 'AND' stuff
			else
				res |= (lm & rm) << i;

			//shift the operands right two bits
			left >>= 2;
			right >>= 2;
		}

		f = System.Convert.ToString (res, 2).PadLeft (16, '0');
		Debug.Log (l +" & " + r + " = " + f); //DEBUG

		return res;
	}
	#endregion

	#region INTERNAL_TYPES

	[System.Serializable]
	public struct Tile
	{
		/// <summary>
		/// Bitwise rotates an int left by amount within a given width
		/// </summary>
		/// <param name="original">The int to bitwise rotate</param>
		/// <param name="amount">The amount to rotate</param>
		/// <param name="bitwidth">The maximum width of the returned int</param>
		/// <returns>original rotated left by amount within bitwidth</returns>
		public static int rotateCVLeft(int original, int amount, int bitwidth)
		{
			int shifted = original << amount;
			int overflow = original >> (bitwidth - amount);
			int widthMask = bitwidth - 1;
			return (shifted | overflow) & widthMask;
		}

		// For user-identification in the inspector
		public string name;

		// Used to identify this tile
		// Represents the adjacency conditions that must be met to place this tile
		[SerializeField]
		private int checkVector;

		// This tile represents this many distict tiles that are rotations of one another
		[SerializeField]
		private int rotations;

		// The object placed in the scene
		public GameObject prefab;

		public Tile(string name, int cv, int rotations)
		{
			this.name = name;
			checkVector = cv;
			this.rotations = rotations;
			prefab = null;
		}

		public int getCheckVector()
		{
			return checkVector;
		}

		public int getRotations()
		{
			return rotations;
		}
	}
	#endregion
}
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
		tiles[ 0] = new Tile (0x5555); //solid block
		tiles[ 1] = new Tile (0x9998); //corridor end
		tiles[ 2] = new Tile (0x5940); //room corner
		tiles[ 3] = new Tile (0x5944); //corridor turn
		tiles[ 4] = new Tile (0x8989); //corridor
		tiles[ 5] = new Tile (0x4984); //t-corridor
		tiles[ 6] = new Tile (0x4980); //corridor side entry 1
		tiles[ 7] = new Tile (0x8409); //corridor side entry 2
		tiles[ 8] = new Tile (0x980 ); //room side
		tiles[ 9] = new Tile (0x4444); //corridor cross
		tiles[10] = new Tile (0x4440); //corridor double entry
		tiles[11] = new Tile (0x440 ); //corridor entry
		tiles[12] = new Tile (0x4040); //double inner corner
		tiles[13] = new Tile (0x40  ); //inner corner
		tiles[14] = new Tile (0x0   ); //empty space
	}

	public void Reset()
	{
		Awake ();
	}

	public GameObject getTile(int adjMask, out float rotation)
	{
		rotation = 0f;

		for (int i = 0; i < tiles.Length; i++)
		{
			int cv = tiles[i].getCheckVector ();

			//check all four orientations of the tile
			for (int rot = 0; rot < 4; rot++)
			{
				if (cv == QARAnd (cv, adjMask))
				{
					rotation = 90f * rot;
					return tiles[i].prefab;
				}

				//rotate the check vector
				Tile.rotateCVLeft (cv);
			}
		}

		throw new System.ArgumentException ("Given mask (" + System.Convert.ToString (adjMask, 2).PadLeft (16, '0') + ") does not " +
			"map to a valid tile!");
	}

	private int QARAnd(int left, int right)
	{
		int res = 0x0;
		for (int i = 0; i < 8; i ++)
		{
			//shift the result left two bits
			res <<= 0x2;

			//isolate two least sig bits
			int lm = left % 4, rm = right % 4;

			//if either is the ignored value, pass it through
			if (lm == 0x2 || rm == 0x2)
				res |= 0x2;
			//else do regular 'AND' stuff
			else
				res |= lm & rm;

			//shift the operands right two bits
			left >>= 0x2;
			right >>= 0x2;
		}

		return res;
	}
	#endregion

	#region INTERNAL_TYPES

	[System.Serializable]
	public struct Tile
	{
		/// <summary>
		/// Rotates the given int four bits to the left.
		/// </summary>
		/// <param name="original">The int to rotate</param>
		/// <returns>The original int rotated left four bits (bits wrap)</returns>
		public static int rotateCVLeft(int original)
		{
			return (original << 4) | (original >> (sizeof (ushort) - 4));
		}

		// Used to identify this tile
		// Represents the adjacency conditions that must be met to place this tile
		[SerializeField]
		private int checkVector;

		// The object placed in the scene
		public GameObject prefab;

		public Tile(int cv)
		{
			checkVector = cv;
			prefab = null;
		}

		public int getCheckVector()
		{
			return checkVector;
		}
	}
	#endregion
}
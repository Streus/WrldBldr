using UnityEngine;

[CreateAssetMenu(menuName = "World Tile Set")]
public class TileSet : ScriptableObject
{
	[SerializeField]
	private GameObject[] tiles = new GameObject[15];
}
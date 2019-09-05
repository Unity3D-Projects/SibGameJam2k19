using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour {
	public Tilemap    Tilemap = null;
	public GameObject Goat    = null;
	public GameObject Farmer  = null;
	public GridLayout Grid    = null;
	[Header("Tiles")]
	public TileBase   Grass   = null;
	public TileBase   Ground  = null;

	public int DeltaToBuildSector = 5;
	int[,] buffer = new int[32, 8];
	System.Random rand;
	public float seed = 6.5f;

	void BuildSector() {
		buffer = RandomWalkTopSmoothed(buffer, rand, 2);
		buffer = SetTextureRules(buffer);
		RenderMap(buffer, Tilemap, Tilemap.cellBounds.max.x, GetNodeY());
		Tilemap.CompressBounds();
	}
	void CutSector() {
		Tilemap.SetTile(new Vector3Int(Tilemap.cellBounds.min.x, 0, 0), null);
		Tilemap.CompressBounds();
	}

	private void Start() {
		rand = new System.Random(seed.GetHashCode());
		Tilemap.ClearAllTiles();
		BuildSector();
	} 

	int[,] SetTextureRules(int[,] map) {
		for ( int x = 0; x <= map.GetUpperBound(0); x++ ) {
			for ( int y = map.GetUpperBound(1); y >= 0; y-- ) {
				if ( map[x, y] == 1 ) {
					map[x, y] = 2;
					break;
				}
			}
		}
		return map;
	}

	private void PrintArray(int[,] arr) {
		string str = "\n";
		for ( int j = arr.GetUpperBound(1); j >= 0; j-- ) {
			for ( int i = 0; i <= arr.GetUpperBound(0); i++ ) {
				str = str + arr[i, j].ToString() + " ";
			}
			str = str + "\n";
		}
		Debug.Log(str);
	}


	int GetNodeY() {
		int x = Tilemap.cellBounds.xMax - 1;
		for ( int y = Tilemap.cellBounds.yMax; y >= Tilemap.cellBounds.yMin; y-- ) {
			if ( Tilemap.GetTile(new Vector3Int(x, y, 0)) != null ) {
				return y;
			}
		}
		return 0;
	} 

	private void Update() {

		int _goatCell = Grid.WorldToCell(Goat.transform.position).x;

		if ( Mathf.Abs(_goatCell - Tilemap.cellBounds.max.x) < DeltaToBuildSector ) {
			BuildSector();
			//Debug.Log("AUTO BUILD");
		}

		if ( Input.GetKeyDown(KeyCode.N) ) {
			CutSector();
		}

	}


	//------------------Взято из юнитевских доков
	public static int[,] RandomWalkTopSmoothed(int[,] map, System.Random rnd, int minSectionWidth) {
		//System.Random rand = new System.Random(seed.GetHashCode());

		//Determine the start position
		int lastHeight = Random.Range(0, map.GetUpperBound(1));

		//Used to determine which direction to go
		int nextMove = 0;
		//Used to keep track of the current sections width
		int sectionWidth = 0;

		for ( int i = 0; i <= map.GetUpperBound(0); i++ ) {
			for ( int j = 0; j <= map.GetUpperBound(1); j++ ) {
				map[i, j] = 0;
			}
		}

		for ( int x = 0; x <= map.GetUpperBound(0); x++ ) {
			//Determine the next move
			//nextMove = rand.Next(2);
			nextMove = rnd.Next(2);

			//Only change the height if we have used the current height more than the minimum required section width
			if ( nextMove == 0 && lastHeight > 0 && sectionWidth > minSectionWidth ) {
				lastHeight--;
				sectionWidth = 0;
			} else if ( nextMove == 1 && lastHeight < map.GetUpperBound(1) && sectionWidth > minSectionWidth ) {
				lastHeight++;
				sectionWidth = 0;
			}
			//Increment the section width
			sectionWidth++;

			//Work our way from the height down to 0
			for ( int y = lastHeight; y >= 0; y-- ) {
				map[x, y] = 1;
			}
		}
		return map;
	}
	public void RenderMap(int[,] map, Tilemap tilemap, int shiftX, int shiftY) {
		int y0 = 0;
		for ( int y = map.GetUpperBound(1); y >= 0; y-- ) {
			if ( map[0, y] != 0 ) {
				y0 = y;
				break;
			}
		}
		int shift = y0 - shiftY;
		if ( shift < 0 ) {
			shift = 0;
		}
		for ( int x = 0; x <= map.GetUpperBound(0); x++ ) {
			for ( int y = 0; y <= map.GetUpperBound(1); y++ ) {
				if ( map[x, y] == 1 ) {
					tilemap.SetTile(new Vector3Int(shiftX + x, y - shift, 0), Ground);
				} else if ( map[x, y] == 2 ) {
					tilemap.SetTile(new Vector3Int(shiftX + x, y - shift, 0), Grass);
				}
			}
		}
	}

}

using UnityEngine;
using UnityEngine.Tilemaps;

using System.Collections.Generic;


public class GridManager : MonoBehaviour {

	public Tilemap    Tilemap         = null;
	public Tilemap    ForegroundGrass = null;
	public GridLayout Grid            = null;

	[Header("Tiles")]
	public TileBase       Grass      = null;
	public TileBase       Ground     = null;
	public List<TileBase> DecorGrass = null;

	[Header("GameObjects")]
	public GameObject Apple   = null;
	public GameObject Goat    = null;
	public GameObject Farmer  = null;
	public List<ObstacleData> ObstacleDatas = null;

	public int DeltaToBuildSector = 10;
	int[,] buffer = new int[32, 8];
	System.Random rand;
	public float seed = 6.5f;


	float obstacleShift = 0.2f;
	public float _obstacleDelta = 0.5f;
	public float obstacleMinScale = 0.6f;

	int[] obstacleHash = {0, 0, 0, 0, 0, 1, 1, 2, 3};
	GameObject _previousObstacle = null;

	[System.Serializable]
	public class ObstacleData {
		public GameObject Prefab = null;
		public Vector2 ScaleLimits = new Vector2(0, 0);
	}

	void BuildSector() {
		buffer = RandomWalkTopSmoothed(buffer, rand, 2);
		buffer = SetTextureRules(buffer);
		int _x = Tilemap.cellBounds.max.x;
		int _y = GetNodeY(Tilemap);
		int sh = RenderMap(buffer, Tilemap, _x, _y);
		RenderGrassMap(buffer, ForegroundGrass, _x, sh);
		Tilemap.CompressBounds();
		PlaceObstacles(Tilemap.cellBounds.max.x - buffer.GetUpperBound(0), Tilemap.cellBounds.max.x - 1); 
		PlaceApples(Tilemap.cellBounds.max.x - buffer.GetUpperBound(0), Tilemap.cellBounds.max.x - 1);
	}
	void CutSector(int _boundX) {
		for ( int x = Tilemap.cellBounds.min.x; x < Tilemap.cellBounds.min.x + _boundX; x++ ) {
			for ( int y = Tilemap.cellBounds.min.y; y < Tilemap.cellBounds.max.y; y++ ) {
				Tilemap.SetTile(new Vector3Int(x, y, 0), null); 
			}
		}
		Tilemap.CompressBounds();
	}

	private void Start() {
		rand = new System.Random(seed.GetHashCode());
		Tilemap.ClearAllTiles();
		ForegroundGrass.ClearAllTiles();
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



	int GetNodeY(Tilemap tilemap) {
		int x = tilemap.cellBounds.xMax - 1;
		for ( int y = tilemap.cellBounds.yMax; y >= tilemap.cellBounds.yMin; y-- ) {
			if ( tilemap.GetTile(new Vector3Int(x, y, 0)) != null ) {
				return y;
			}
		}
		return 0;
	} 

	private void Update() {

		int _goatCell = Grid.WorldToCell(Goat.transform.position).x;

		if ( Mathf.Abs(_goatCell - Tilemap.cellBounds.max.x) < DeltaToBuildSector ) {
			BuildSector();
			CutSector(20);
			//Debug.Log("AUTO BUILD");
		}

		if ( Input.GetKeyDown(KeyCode.N) ) {
			CutSector(10);
		}

	}
	void PlaceObstacles(int _startx, int _endx) {
		int obstaclesNum = 0;
		for ( int x = _startx; x <= _endx; x++ ) {
			obstaclesNum = obstacleHash[rand.Next(obstacleHash.Length)];
			if ( obstaclesNum > 0 ) {
				ObstacleData _obstacle = ObstacleDatas[rand.Next(ObstacleDatas.Count)];
				Vector2 pos = Grid.CellToWorld(new Vector3Int(x, GetTopGroundIndex(x), 0));
				float rndScale = _obstacle.ScaleLimits.x + rand.Next((int)(100 * (_obstacle.ScaleLimits.y-_obstacle.ScaleLimits.x))) / 100f;
				float _newXsize = _obstacle.Prefab.GetComponent<SpriteRenderer>().size.x * rndScale; 
				pos.y += Grid.cellSize.y + obstacleShift;
				pos.x += (float)rand.NextDouble() * Grid.cellSize.x + (_newXsize / 2);
				if ( _previousObstacle != null ) {
					float minimalDistance = _previousObstacle.transform.position.x + (_obstacle.Prefab.GetComponent<SpriteRenderer>().size.x) + _obstacleDelta; //может сделать точнее
					if ( pos.x < minimalDistance ) {
						pos.x = minimalDistance;
					}
				}
				Vector3Int _adjCell = Grid.WorldToCell(new Vector3(pos.x + (_newXsize / 2f), pos.y));
				if (Tilemap.GetTile(new Vector3Int(_adjCell.x, _adjCell.y - 1, _adjCell.z)) != null && Tilemap.GetTile(_adjCell) != Ground && Tilemap.GetTile(_adjCell) != Grass) {
					_previousObstacle = Instantiate(_obstacle.Prefab, pos, Quaternion.identity); 
					_previousObstacle.transform.localScale = new Vector3(rndScale, rndScale, rndScale);
					if ( rand.Next(2) == 0 ) {
						_previousObstacle.transform.Rotate(new Vector3(0, 180, 0));
					}
				} 
			}
		} 
	}

	void PlaceApples(int _startx, int _endx) {
		int _dropRate = 20;
		for ( int x = _startx; x <= _endx; x++ ) {
			if ( rand.Next(100) < _dropRate ) {
				Vector2 pos = Grid.CellToWorld(new Vector3Int(x, GetTopGroundIndex(x), 0));
				pos.y += Grid.cellSize.y + 0.9f;
				pos.x += Grid.cellSize.x / 2f;
				Instantiate(Apple, pos, Quaternion.identity);
			}
		} 
	}

	int GetTopGroundIndex(int x) {
		for ( int y = Tilemap.cellBounds.max.y; y > Tilemap.cellBounds.min.y; y-- ) {
			if ( Tilemap.GetTile(new Vector3Int(x, y, 0)) != null ) {
				return y;
			}
		}
		return 0;
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
	public int RenderMap(int[,] map, Tilemap tilemap, int shiftX, int shiftY) {
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
		for ( int x = shiftX; x < Tilemap.cellBounds.max.x; x++ ) { //дополняем снизу земли чтоб не было просветов
			for ( int k = -1; k > -4 ; k-- ) {
				tilemap.SetTile(new Vector3Int(x, k - shift, 0), Ground); 
			}
		}
		return shift;
	} 
	public void RenderGrassMap(int[,] map, Tilemap tilemap, int shiftX, int shiftY) {
		for ( int x = 0; x <= map.GetUpperBound(0); x++ ) {
			for ( int y = 0; y <= map.GetUpperBound(1); y++ ) {
				if ( map[x, y] == 2 ) {
					tilemap.SetTile(new Vector3Int(shiftX + x, y - shiftY + 1, 0), DecorGrass[rand.Next(DecorGrass.Count)]);
					break;
				}
			}
		} 
	}
}

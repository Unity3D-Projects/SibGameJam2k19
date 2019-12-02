using UnityEngine;
using UnityEngine.Tilemaps;

using System.Collections.Generic; 

using SMGCore;

public class GridManager : MonoBehaviour {

	public Tilemap    Tilemap         = null;
	public Tilemap    ForegroundGrass = null;
	public GridLayout Grid            = null;
	public Transform  Obstacles       = null;
	public Transform  Apples          = null;
	public Transform  Bees            = null;
	public Transform  Hogs            = null;
	public Transform  Islands         = null;

	[Header("Tiles")]
	public TileBase       Grass            = null;
	public TileBase       GrassLeftCorner  = null;
	public TileBase       GrassRightCorner = null;
	public TileBase       Ground           = null;
	public List<TileBase> DecorGrass       = null;

	[Header("GameObjects")]
	public GameObject Apple   = null;
	public GameObject Goat    = null;
	public GameObject Farmer  = null;
	public GameObject Island  = null;
	public List<ObstacleData> ObstacleDatas = null;

	[Header("Scenario")]
	public float GoatSpeedMax             = 3f;
	public float GoatSpeedDelta           = 0.1f;
	public float FarmerSpeedMax           = 3f;
	public float FarmerSpeedDelta         = 0.1f;
	public int   ObstacleProbability      = 10;
	public int   ObstacleProbabilityMax   = 80;
	public int   ObstacleProbabilityDelta = 1;
	public int   ApplesProbability        = 5; 

	[Header("MapBuilding")]
	public int   DeltaToBuildSector     = 16;  //расстояние от козы до правого края, когда начинается ген нового блока
	public int   DeltaToCutSector       = 30;
	public int   DeltaBeforeFarmerToCut = 6;  //количество клеток от деда влево, до куда обрежется карта
	public float ObstacleVerticalShift  = 0.2f;
	public float ObstacleDelta          = 0.5f;   //minimal distance between obstacles
	public int   MaxHogsOnTenCells      = 1;

	int[,] buffer = new int[32, 8];
	System.Random rand;
	public float seed = 6.5f; 


	GameObject _previousObstacle = null;
	HogPool hogPool = new HogPool();
	BeePool beePool = new BeePool();


	public class ObstaclePool: PrefabPool<Obstacle> { 
		public ObstaclePool(string _path) {
			PresenterPrefabPath = _path;
		}
	} 
	public class HogPool: PrefabPool<Hedgehog> {
		public HogPool() {
			PresenterPrefabPath = "Prefabs/Hedgehog";
		}
	} 
	public class BeePool: PrefabPool<Obstacle> {
		public BeePool() {
			PresenterPrefabPath = "Prefabs/bee"; 
		}
	}


	[System.Serializable]
	public class ObstacleData {
		public GameObject Prefab = null;
		public Vector2 ScaleLimits = new Vector2(0, 0);
		public string PoolPath;
		public ObstaclePool Pool;
	}

	private void InitObstacleData() {
		for ( int i = 0; i < ObstacleDatas.Count; i++ ) {
			ObstacleDatas[i].Pool = new ObstaclePool(ObstacleDatas[i].PoolPath);
			ObstacleDatas[i].Pool.Init();
		}
	}


	private void Start() { 
		InitObstacleData();
		hogPool.Init();
		beePool.Init();
		
		rand = new System.Random(seed.GetHashCode());
		Tilemap.ClearAllTiles();
		ForegroundGrass.ClearAllTiles();

		//BuildSector(); 
		//buffer = RandomWalkTopSmoothed(buffer, rand, 2);
		//buffer = SetTextureRules(buffer);
		int _x = Tilemap.cellBounds.max.x;
		int _y = GetNodeY(Tilemap);
		int[,] straight = BuildStraight(8);
		straight = SetTextureRules(straight);
		int sh = RenderMap(straight, Tilemap, _x, _y);
		RenderGrassMap(straight, ForegroundGrass, _x, sh);
		Tilemap.CompressBounds();
		//PlaceObstacles(4, Tilemap.cellBounds.max.x - 1, ObstacleProbability);  //BuildSector скопирован из-за этой строчки, чтобы не начинать в препятствии
		//PlaceApples(4, Tilemap.cellBounds.max.x - 1, ApplesProbability);

		Vector3Int goatCellIndex = Tilemap.WorldToCell(Goat.transform.position);
		Vector3 goatCellPos = Tilemap.CellToWorld(new Vector3Int(goatCellIndex.x, GetUpperBound(Tilemap, goatCellIndex.x), 0));
		Goat.transform.position = new Vector3(Goat.transform.position.x, goatCellPos.y + Tilemap.layoutGrid.cellSize.y + 0.2f);

		Vector3Int farmerCellIndex = Tilemap.WorldToCell(Farmer.transform.position);
		Vector3 farmerCellPos = Tilemap.CellToWorld(new Vector3Int(farmerCellIndex.x, GetUpperBound(Tilemap, farmerCellIndex.x), 0));
		Farmer.transform.position = new Vector3(Farmer.transform.position.x, farmerCellPos.y + Tilemap.layoutGrid.cellSize.y + 1f);

	}

	private void Update() { 
		int _goatCell = Grid.WorldToCell(Goat.transform.position).x;
		int _farmerCell = Grid.WorldToCell(Farmer.transform.position).x;

		if ( Mathf.Abs(_goatCell - Tilemap.cellBounds.max.x) < DeltaToBuildSector ) {
			BuildSector();
			UpdateScenario();
		}
		if (  Mathf.Abs((_farmerCell - DeltaBeforeFarmerToCut) - Tilemap.cellBounds.min.x) > DeltaToCutSector  ) {
			CutSector(_farmerCell - DeltaBeforeFarmerToCut);
		}

		//if ( Input.GetKeyDown(KeyCode.N) ) {
		//	CutSector(10);
		//} 
	}

	void BuildSector() {
		buffer = RandomWalkTopSmoothed(buffer, rand, 2);
		buffer = SetTextureRules(buffer);
		int _x = Tilemap.cellBounds.max.x;
		int _y = GetNodeY(Tilemap);
		int sh = RenderMap(buffer, Tilemap, _x, _y);
		RenderGrassMap(buffer, ForegroundGrass, _x, sh);
		Tilemap.CompressBounds();
		int _x0 = Tilemap.cellBounds.max.x - buffer.GetUpperBound(0);
		int _x1 = Tilemap.cellBounds.max.x - 2; // -1 потому что бауд зачем-то ставит одну пустую линию в конце и еще -1 из-за граничных условий расстановки препятствий

		var CellsStates = new Dictionary<int, int>();
		for ( int i = _x0; i < _x1; i++ ) {
			CellsStates.Add(i, 0);
		}
		// 0-empty, 1-hog, 2-bee, 3-obstacle

		PlaceHogs(MaxHogsOnTenCells, CellsStates);
		PlaceBees(MaxHogsOnTenCells, CellsStates);
		PlaceObstacles(ObstacleProbability, CellsStates);
		PlaceApples(ApplesProbability, CellsStates);
		PlaceIslands(CellsStates);
	} 

	void CutSector(int _boundX) {
		Vector3 _world = Tilemap.CellToWorld(new Vector3Int(_boundX, 0, 0));
		for ( int x = Tilemap.cellBounds.min.x; x < _boundX; x++ ) {
			for ( int y = Tilemap.cellBounds.min.y; y < Tilemap.cellBounds.max.y; y++ ) {
				Tilemap.SetTile(new Vector3Int(x, y, 0), null); 
			}
			ForegroundGrass.SetTile(new Vector3Int(x, GetUpperBound(ForegroundGrass, x), 0), null);
		}
		Tilemap.CompressBounds();
		foreach ( Transform obstacle in Obstacles.transform ) {
			if ( obstacle.position.x != 0 & obstacle.position.x < _world.x ) {
				for ( int i = 0; i < ObstacleDatas.Count; i++ ) {
					if ( ObstacleDatas[i].Prefab.GetComponent<Obstacle>().Type == obstacle.GetComponent<Obstacle>().Type ) {
						ObstacleDatas[i].Pool.Return(obstacle.GetComponent<Obstacle>());
						break;
					} 
				}
			}
		}
		foreach ( Transform apple in Apples.transform ) {
			if ( apple.position.x < _world.x ) {
				Destroy(apple.gameObject); 
			} 
		}
		foreach ( Transform bee in Bees ) {
			if ( bee.position.x != 0 & bee.position.x < _world.x ) {
				beePool.Return(bee.GetComponent<Obstacle>()); 
			} 
		}
		foreach ( Transform hog in Hogs ) {
			if ( hog.position.x != 0 & hog.position.x < _world.x ) {
				hogPool.Return(hog.GetComponent<Hedgehog>()); 
			} 
		}
	} 



	void UpdateScenario() {
		if ( Goat.GetComponent<GoatController>().RunSpeed + GoatSpeedDelta <= GoatSpeedMax  ) {
			Goat.GetComponent<GoatController>().RunSpeed += GoatSpeedDelta;
		}
		if ( Farmer.GetComponent<FarmerController>().MoveSpeed + FarmerSpeedDelta <= FarmerSpeedMax  ) {
			Farmer.GetComponent<FarmerController>().MoveSpeed += FarmerSpeedDelta;
		}
		if ( ObstacleProbability + ObstacleProbabilityDelta <= ObstacleProbabilityMax ) {
			ObstacleProbability += ObstacleProbabilityDelta;
		}
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
	int GetUpperBound(Tilemap tilemap, int x) {
		for ( int y = tilemap.cellBounds.yMax; y >= tilemap.cellBounds.yMin; y-- ) {
			if ( tilemap.GetTile(new Vector3Int(x, y, 0)) != null ) {
				return y;
			}
		}
		return 0; 
	}

	static int[,] SetTextureRules(int[,] map) {  //осталось еще края блоков обновлять
		for ( int x = 0; x <= map.GetUpperBound(0); x++ ) {
			for ( int y = map.GetUpperBound(1); y >= 0; y-- ) {
				if ( map[x, y] == 1 ) {
					if ( x > 0 ) {
						if ( map[x - 1, y] == 0 ) {
							map[x, y] = 3;
							break;
						}
					}
					if ( x > 0 & x < map.GetUpperBound(0) ) {
						if ( map[x + 1, y] == 0 ) {
							map[x, y] = 4;
							break;
						}
					}
					map[x, y] = 2;
					break;
				}
			}
		}
		return map;
	}

	void PlaceObstacles(int _probability, Dictionary<int, int> cells) {
		var freeCells = new List<int>();
		foreach ( KeyValuePair<int, int> c in cells ) {
			if ( c.Value == 0 ) {
				freeCells.Add(c.Key);
			}
		}
		for ( int i = 0; i < freeCells.Count; i++ ) {

			int x = freeCells[i];
			if ( rand.Next(100) <= _probability ) {
				ObstacleData _obstacle = ObstacleDatas[rand.Next(ObstacleDatas.Count)];
				int y = GetTopGroundIndex(x);
				Vector2 pos = Grid.CellToWorld(new Vector3Int(x, y, 0));
				float constraintLeft = pos.x;
				float constraintRight = pos.x + Grid.cellSize.x;
				float rndScale = _obstacle.ScaleLimits.x + rand.Next((int)(100 * (_obstacle.ScaleLimits.y-_obstacle.ScaleLimits.x))) / 100f;
				float _newXsize = _obstacle.Prefab.GetComponent<SpriteRenderer>().size.x * rndScale; 
				pos.y += Grid.cellSize.y + ObstacleVerticalShift;
				int leftConstraint = 0;
				if (  Tilemap.GetTile(new Vector3Int(x - 1, y + 1, 0)) != null | Tilemap.GetTile(new Vector3Int(x - 1, y, 0)) == null  ) {
					leftConstraint = 1; 
				}
				pos.x += (float)rand.NextDouble() * Grid.cellSize.x + leftConstraint * (_newXsize / 2);
				if ( _previousObstacle != null ) {
					float minimalDistance = _previousObstacle.transform.position.x + (_obstacle.Prefab.GetComponent<SpriteRenderer>().size.x) + ObstacleDelta; //может сделать точнее
					if ( pos.x < minimalDistance ) {
						pos.x = minimalDistance;
					}
				}
				pos = new Vector2(Mathf.Clamp(pos.x, constraintLeft, constraintRight), pos.y);
				Vector3Int _adjCell = Grid.WorldToCell(new Vector3(pos.x + (_newXsize / 2f), pos.y));
				if ( Tilemap.GetTile(new Vector3Int(_adjCell.x, _adjCell.y - 1, _adjCell.z)) != null && Tilemap.GetTile(_adjCell) == null ) {
					_previousObstacle = _obstacle.Pool.Get().gameObject;
					_previousObstacle.transform.SetParent(Obstacles);
					_previousObstacle.transform.position = pos;
					_previousObstacle.transform.localScale = new Vector3(rndScale, rndScale, rndScale);
					if ( rand.Next(2) == 0 ) {
						_previousObstacle.transform.Rotate(new Vector3(0, 180, 0));
					}
					cells[x] = 3;
				} 
			}
		} 
	}

	void PlaceApples(int _probability, Dictionary<int, int> cells) {
		foreach ( KeyValuePair<int, int> c in cells ) {
			if ( c.Value == 0 | c.Value == 3 ) {
				if ( rand.Next(100) <= _probability ) {
					Vector2 pos = Grid.CellToWorld(new Vector3Int(c.Key, GetTopGroundIndex(c.Key), 0));
					pos.y += Grid.cellSize.y + 0.8f;
					pos.x += Grid.cellSize.x / 2f;
					var a = Instantiate(Apple, pos, Quaternion.identity, Apples);
					if ( c.Value == 3 ) {
						a.transform.position += new Vector3(0, 0.5f, 0); 
					}
				} 
			}
		}
	}

	void PlaceHogs(int _maxNum, Dictionary<int, int> cells) {
		int hogNum = 0;
		for ( int i = 0; i <= cells.Count / 10; i++ ) {
			hogNum += rand.Next(_maxNum + 1);
		}
		var freeCells = new List<int>();
		foreach ( KeyValuePair<int, int> c in cells ) {
			if ( c.Value == 0 ) {
				if ( Tilemap.GetTile(new Vector3Int(c.Key - 1, GetUpperBound(Tilemap, c.Key) + 1, 0)) == null & Tilemap.GetTile(new Vector3Int(c.Key + 1, GetUpperBound(Tilemap, c.Key) + 1, 0)) == null ) {
					freeCells.Add(c.Key);
				} 
			} 
		}
		for ( int i = 0; i < hogNum; i++ ) {
			int rndIndex = rand.Next(freeCells.Count);
			int rndX = freeCells[rndIndex];

			for ( int j = rndX - 1; j <= rndX + 1; j++ ) {
				freeCells.Remove(j);
				if ( cells.ContainsKey(j) ) {
					cells[j] = 1; 
				}
			} 

			Vector2 pos = Grid.CellToWorld(new Vector3Int(rndX, GetTopGroundIndex(rndX), 0));
			pos.y += Grid.cellSize.y + 0.8f;
			pos.x += Grid.cellSize.x / 2f;
			var hog = hogPool.Get().gameObject;
			hog.transform.SetParent(Hogs);
			hog.transform.position = pos;
			float rndScale = 0.75f + rand.Next(20) / 100f;
			hog.transform.localScale = new Vector3(rndScale, rndScale, 1);
			if ( rand.Next(4) == 0 ) {
				hog.transform.Rotate(new Vector3(0, 180, 0));
			}
		}
	}
	void PlaceBees(int _maxNum, Dictionary<int, int> cells) {
		int beeNum = 0;
		for ( int i = 0; i <= cells.Count / 10; i++ ) {
			beeNum += rand.Next(_maxNum + 1);
		}
		var freeCells = new List<int>();
		foreach ( KeyValuePair<int, int> c in cells ) {
			if ( c.Value == 0 ) {
				if ( Tilemap.GetTile(new Vector3Int(c.Key - 1, GetUpperBound(Tilemap, c.Key) + 1, 0)) == null & Tilemap.GetTile(new Vector3Int(c.Key + 1, GetUpperBound(Tilemap, c.Key) + 1, 0)) == null ) {
					freeCells.Add(c.Key);
				} 
			} 
		}
		for ( int i = 0; i < beeNum; i++ ) {
			int rndIndex = rand.Next(freeCells.Count);
			int rndX = freeCells[rndIndex];
			for ( int j = rndX - 1; j <= rndX + 1; j++ ) {
				freeCells.Remove(j);
				if ( cells.ContainsKey(j) ) {
					cells[j] = 2; 
				}
			}
			Vector2 pos = Grid.CellToWorld(new Vector3Int(rndX, GetTopGroundIndex(rndX), 0));
			pos.y += Grid.cellSize.y + 1.4f;
			pos.x += Grid.cellSize.x / 2f;
			var bee = beePool.Get().gameObject;
			bee.transform.SetParent(Bees); 
			if ( Tilemap.GetTile(new Vector3Int(rndX - 1, GetTopGroundIndex(rndX), 0)) == null ) {
				pos += new Vector2(0, 0.25f);
			}
			bee.transform.position = pos;
			float rndScale = 0.75f + rand.Next(25) / 100f;
			bee.transform.localScale = new Vector3(rndScale, rndScale, 1);
			if ( rand.Next(4) == 0 ) {
				bee.transform.Rotate(new Vector3(0, 180, 0));
			}
		}
	}

	void PlaceIslands(Dictionary<int, int> cells) {
		float maxDeltaUp   = 2.5f;
		float maxDeltaDown = 1f;
		float minDistance  = 1.5f;
		float maxDistance  = 3.8f;
		float minHeight    = Tilemap.cellSize.y + 0.5f;
		float lastX        = 0f;
		int   recCounter   = 0; 
		var   scaleBounds  = new Vector2(0.5f, 1);

		var indexes = new List<int>();
		foreach ( KeyValuePair<int, int> c in cells ) {
			indexes.Add(c.Key);
		}
		indexes.Sort();
		var nodes = new List<int>();
		for ( int i = 0; i < indexes.Count; i++ ) {
			if ( Tilemap.GetTile(new Vector3Int(indexes[i] + 1, GetUpperBound(Tilemap, indexes[i]), 0)) == null  ) {
				nodes.Add(indexes[i]);
			}
		}
		for ( int i = 0; i < nodes.Count; i++ ) {
			Vector2 node = Tilemap.CellToWorld(new Vector3Int(nodes[i], GetUpperBound(Tilemap, nodes[i]), 0));
			node.x += Tilemap.cellSize.x;
			node.y += Tilemap.cellSize.y;
			if ( node.x < lastX) {
				continue;
			}
			recCounter = 0;
			TryPlace(node);
		}

		void TryPlace(Vector2 node) {
			recCounter++;
			if ( recCounter > 3 ) {
				return;
			}
			float scaleX = Random.Range(scaleBounds.x, scaleBounds.y);
			float islandY = 0f;
			float islandX = 0f;
			float newSizeX = Island.GetComponent<Renderer>().bounds.size.x * scaleX; 
			float baseShift = node.x + 0.5f * newSizeX;
			islandX = Random.Range(baseShift + minDistance, baseShift + maxDistance);
			if ( recCounter == 1 ) {
				islandY = Random.Range(node.y + 1, node.y + maxDeltaUp);
			} else {
				islandY = Random.Range(node.y - maxDeltaDown, node.y + maxDeltaUp);
			}
			if ( GetHeight(islandX + (newSizeX * 0.5f), islandY ) >= minHeight) {
				var island = Instantiate(Island, new Vector2(islandX, islandY), Quaternion.identity, Islands);
				island.transform.localScale = new Vector3(scaleX, 1, 1);
				lastX = islandX;
				Vector2 newNode = new Vector2(islandX + (newSizeX * 0.5f), islandY);
				TryPlace(newNode);
			} 
		}

		float GetHeight(float x, float y) {
			int cellX = Tilemap.WorldToCell(new Vector3(x, 0, 0)).x;
			Vector3 pos = Tilemap.CellToWorld(new Vector3Int(cellX, GetUpperBound(Tilemap, cellX), 0));
			return y - (pos.y + Tilemap.cellSize.y); 
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


	int[,] BuildStraight(int len) {
		int[,] map = new int[len, 4];
		for ( int i = 0; i <= map.GetUpperBound(0); i++ ) {
			for ( int j = 0; j <= map.GetUpperBound(1); j++ ) {
				map[i, j] = 1;
			}
		}
		return map;
	}

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
				int _tx = shiftX + x;
				int _ty = y - shift;
				if ( map[x, y] == 1 ) {
					tilemap.SetTile(new Vector3Int(_tx, _ty, 0), Ground);
				} else if ( map[x, y] == 2 ) {
					tilemap.SetTile(new Vector3Int(_tx, _ty, 0), Grass);
					//if ( tilemap.GetTile(new Vector3Int(_tx - 1, _ty, 0)) == null ) {
					//	tilemap.SetTile(new Vector3Int(_tx, _ty, 0), GrassLeftCorner);
					//} else if ( tilemap.GetTile(new Vector3Int(_tx + 1, _ty - shift, 0)) == null & x < map.GetUpperBound(0) ) {
					//	tilemap.SetTile(new Vector3Int(_tx, _ty, 0), GrassRightCorner);
					//} else {
					//	tilemap.SetTile(new Vector3Int(_tx, _ty, 0), Grass);
					//}
				} else if ( map[x, y] == 3 ) {
					tilemap.SetTile(new Vector3Int(_tx, _ty, 0), GrassLeftCorner);
				} else if ( map[x, y] == 4 ) {
					tilemap.SetTile(new Vector3Int(_tx, _ty, 0), GrassRightCorner);
				}
			}
		}

		if ( tilemap.GetTile(new Vector3Int(shiftX, GetTopGroundIndex(shiftX - 1), 0)) == null ) {
			tilemap.SetTile(new Vector3Int(shiftX-1, GetTopGroundIndex(shiftX - 1), 0), GrassRightCorner); 
		}

		for ( int x = shiftX; x < Tilemap.cellBounds.max.x; x++ ) { //дополняем снизу земли чтоб не было просветов
			for ( int k = -1; k > -4; k-- ) {
				tilemap.SetTile(new Vector3Int(x, k - shift, 0), Ground); 
			}
		}
		return shift;
	} 
	public void RenderGrassMap(int[,] map, Tilemap tilemap, int shiftX, int shiftY) {
		for ( int x = 0; x <= map.GetUpperBound(0); x++ ) {
			for ( int y = 0; y <= map.GetUpperBound(1); y++ ) {
				if ( map[x, y] != 0 & map[x, y] != 1 ) {
					tilemap.SetTile(new Vector3Int(shiftX + x, y - shiftY + 1, 0), DecorGrass[rand.Next(DecorGrass.Count)]);
					break;
				}
			}
		} 
	}

	public static bool IsOverlapping(GameObject o1, GameObject o2) {
		float m1 = o1.GetComponent<SpriteRenderer>().size.magnitude;
		float m2 = o2.GetComponent<SpriteRenderer>().size.magnitude;
		Vector2 result = o1.transform.position - o2.transform.position;
		return result.magnitude < (m1 + m2) / 2f; 
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
}

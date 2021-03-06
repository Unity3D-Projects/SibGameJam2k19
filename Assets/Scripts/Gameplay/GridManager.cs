using UnityEngine;
using UnityEngine.Tilemaps;

using System;
using System.Collections.Generic;

using Random = UnityEngine.Random;

using SMGCore;

public class GridManager : MonoBehaviour {

	public Tilemap    Tilemap         = null;
	public Tilemap    ForegroundGrass = null;
	public Tilemap    BackgroundGrass = null;
	public GridLayout Grid            = null;
	public Transform  Obstacles       = null;
	public Transform  Apples          = null;
	public Transform  Bees            = null;
	public Transform  Hogs            = null;
	public Transform  Islands         = null;
	public Transform  BGDecor         = null;
	public Transform  Decor           = null;

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
	public List<PoolItemData> BGPropsDatas = null;
	public List<PoolItemData> FGPropsDatas = null;

	[Header("Scenario")]
	public float GoatSpeedMax             = 3f;
	public float GoatSpeedDelta           = 0.1f;
	public float FarmerSpeedMax           = 3f;
	public float FarmerSpeedDelta         = 0.1f;
	public int   ObstacleProbability      = 10;
	public int   AnimalsSpawnValue        = 40;
	public int   ObstacleProbabilityMax   = 80;
	public int   ObstacleProbabilityDelta = 1;
	public int   ApplesProbability        = 5; 

	[Header("MapBuilding")]
	public int   DeltaToBuildSector      = 16;  //расстояние от козы до правого края, когда начинается ген нового блока
	public int   DeltaToCutSector        = 30;
	public int   DeltaBeforeFarmerToCut  = 6;  //количество клеток от деда влево, до куда обрежется карта
	public float ObstacleVerticalShift   = 0f;
	public float ObstacleDelta           = 2f;   //minimal distance between obstacles
	public int   MaxHogsOnTenCells       = 1;
	public int   MaxPropsOnTenCells      = 2;
	public int   MaxSmallPropsOnTenCells = 2;
	public float MaxY                    = 20f;
	public float MinY                    = -4f;

	int _minCellY;
	int _maxCellY;

	int[,] buffer = new int[32, 8];

	int[,] pattern1 = {
		{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
		{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
		{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2},
		{0, 0, 0, 0, 0, 2, 2, 0, 0, 0, 2, 1},
		{0, 0, 0, 0, 2, 1, 1, 2, 0, 2, 1, 1},
		{0, 0, 2, 2, 1, 1, 1, 1, 2, 1, 1, 1},
		{0, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		{2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
	}; 
	int[,] pattern2 = {
		{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
		{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
		{2, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0},
		{1, 0, 0, 0, 0, 2, 1, 2, 2, 0, 2, 0},
		{1, 0, 0, 0, 2, 1, 1, 1, 1, 2, 1, 0},
		{1, 0, 2, 2, 1, 1, 1, 1, 1, 1, 1, 2},
		{1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		{1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
	};

	int[,] pattern3 = {
		{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
		{0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 0},
		{2, 0, 0, 0, 0, 2, 1, 0, 0, 2, 0, 2},
		{1, 0, 0, 0, 2, 1, 1, 0, 2, 1, 2, 1},
		{1, 0, 2, 2, 1, 1, 1, 2, 1, 1, 1, 1},
		{1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		{1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
		{1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}
	};

	List<int[,]> Patterns = new List<int[,]>();



	HogPool      hogPool      = new HogPool();
	BeePool      beePool      = new BeePool();
	IslandsPool  islandsPool  = new IslandsPool();
	BGStonesPool bgStonesPool = new BGStonesPool();

	GameObject _previousObstacle = null;

	public class ObstaclePool: PrefabPool<Obstacle> { 
		public ObstaclePool(string _path) {
			PresenterPrefabPath = _path;
		}
	} 
	public class BGPropsPool: PrefabPool<PoolItem> { 
		public BGPropsPool(string _path) {
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

	public class IslandsPool: PrefabPool<PoolItem> {
		public IslandsPool() {
			PresenterPrefabPath = "Prefabs/ostrov";
		}
	}

	public class BGStonesPool: PrefabPool<PoolItem> {
		public BGStonesPool() {
			PresenterPrefabPath = "Prefabs/Decor/stone_back";
		}
	}


	[System.Serializable]
	public class ObstacleData {
		public GameObject Prefab = null;
		public Vector2 ScaleLimits = new Vector2(0, 0);
		public string PoolPath;
		public ObstaclePool Pool;
	}

	[System.Serializable]
	public class PoolItemData {
		public GameObject Prefab = null;
		public string PrefabPath;
		public BGPropsPool Pool;
	}

	private void InitObstacleData() {
		for ( int i = 0; i < ObstacleDatas.Count; i++ ) {
			ObstacleDatas[i].Pool = new ObstaclePool(ObstacleDatas[i].PoolPath);
			ObstacleDatas[i].Pool.Init();
		}
	}
	private void InitPropsData() {
		for ( int i = 0; i < BGPropsDatas.Count; i++ ) {
			BGPropsDatas[i].Pool = new BGPropsPool(BGPropsDatas[i].PrefabPath);
			BGPropsDatas[i].Pool.Init();
		}
		for ( int i = 0; i < FGPropsDatas.Count; i++ ) {
			FGPropsDatas[i].Pool = new BGPropsPool(FGPropsDatas[i].PrefabPath);
			FGPropsDatas[i].Pool.Init();
		}
	}

	int[,] RotateMap(int[,] map) {
		int[,] newMap = new int[map.GetUpperBound(1) + 1, map.GetUpperBound(0) + 1];
		for ( int i = 0; i <= map.GetUpperBound(0); i++ ) {
			for ( int j = 0; j <= map.GetUpperBound(1); j++ ) {
				newMap[j, newMap.GetUpperBound(1) -  i] = map[i,j]; 
			} 
		}
		return newMap; 
	}


	private void Start() { 

		Patterns.Add(RotateMap(pattern1));
		Patterns.Add(RotateMap(pattern2)); 
		Patterns.Add(RotateMap(pattern3)); 

		_minCellY = Tilemap.WorldToCell(new Vector3(0, MinY, 0)).y; 
		_maxCellY = Tilemap.WorldToCell(new Vector3(0, MaxY, 0)).y; 

		InitObstacleData();
		InitPropsData();
		hogPool.Init();
		beePool.Init();
		islandsPool.Init();
		bgStonesPool.Init();

		Tilemap.ClearAllTiles();
		ForegroundGrass.ClearAllTiles();
		BackgroundGrass.ClearAllTiles();

		int _x = Tilemap.cellBounds.max.x;
		int _y = GetNodeY(Tilemap);
		int[,] straight = BuildStraight(8);
		straight = SetTextureRules(straight);
		int sh = RenderMap(straight, Tilemap, _x, _y);
		RenderGrassMap(_x, 8);
		Tilemap.CompressBounds();
		//PlaceObstacles(4, Tilemap.cellBounds.max.x - 1, ObstacleProbability);  //BuildSector скопирован из-за этой строчки, чтобы не начинать в препятствии

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
		if ( Mathf.Abs(_farmerCell - DeltaBeforeFarmerToCut - Tilemap.cellBounds.min.x) > DeltaToCutSector ) {
			CutSector(_farmerCell - DeltaBeforeFarmerToCut);
		}
	}

	void BuildSector() {
		int _x = Tilemap.cellBounds.max.x;
		int _y = GetNodeY(Tilemap);
		int[,] buf;
		if ( Random.Range(0,3) == 2 ) {
			buf = Patterns[Random.Range(0, Patterns.Count)];
		} else {
			buf = RandomWalkTopSmoothed(buffer, 2, _y);
			buf = SetTextureRules(buf);
		}
		int sh = RenderMap(buf, Tilemap, _x, _y);

		Tilemap.CompressBounds();
		int _x0 = Tilemap.cellBounds.max.x - buf.GetLength(0);
		int _x1 = Tilemap.cellBounds.max.x - 2; // -1 потому что в конце пустой столбец и еще -1 чтобы не ставить препятствия в последнюю клетку блока 
		RenderGrassMap(_x0, _x1 + 1);
		PlaceDecor(_x0, _x1);

		var CellsStates = new Dictionary<int, int>();
		for ( int i = _x0; i <= _x1; i++ ) {
			CellsStates.Add(i, 0);
		}

		   //0-empty, 1-hog, 2-bee, 3-obstacle
		if ( ObstacleProbability > AnimalsSpawnValue ) {
			PlaceHogs(MaxHogsOnTenCells, CellsStates);
			PlaceBees(MaxHogsOnTenCells, CellsStates); 
		}
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
			BackgroundGrass.SetTile(new Vector3Int(x, GetUpperBound(BackgroundGrass, x), 0), null);
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
		foreach ( Transform island in Islands ) {
			if ( island.position.x != 0 & island.position.x < _world.x ) {
				islandsPool.Return(island.GetComponent<PoolItem>()); 
			} 
		}
		foreach ( Transform prop in BGDecor.transform ) {
			if ( prop.position.x != 0 & prop.position.x < _world.x ) {
				for ( int i = 0; i < BGPropsDatas.Count; i++ ) {
					if ( BGPropsDatas[i].Prefab.name + "(Clone)" == prop.name ) { //позор, нужен какой-то способ определения к какому пулу относится объект или чтоб объект сам мог в пул возвращаться
						BGPropsDatas[i].Pool.Return(prop.GetComponent<PoolItem>());
						break;
					} 
				}
			}
		}
		foreach ( Transform prop in Decor.transform ) {
			if ( prop.position.x != 0 & prop.position.x < _world.x ) {
				for ( int i = 0; i < FGPropsDatas.Count; i++ ) {
					if ( FGPropsDatas[i].Prefab.name + "(Clone)" == prop.name ) { //позор, нужен какой-то способ определения к какому пулу относится объект или чтоб объект сам мог в пул возвращаться
						FGPropsDatas[i].Pool.Return(prop.GetComponent<PoolItem>());
						break;
					} 
				}
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
		freeCells.Sort();
		for ( int i = 0; i < freeCells.Count; i++ ) {

			int x = freeCells[i];
			if ( Random.Range(0, 100) <= _probability ) {
				ObstacleData _obstacle = ObstacleDatas[Random.Range(0, ObstacleDatas.Count)];
				int y = GetTopGroundIndex(x);
				Vector2 pos = Grid.CellToWorld(new Vector3Int(x, y, 0));
				float constraintLeft = pos.x;
				float constraintRight = pos.x + Grid.cellSize.x;
				float rndScale = Random.Range(_obstacle.ScaleLimits.x, _obstacle.ScaleLimits.y); 
				float _newXsize = _obstacle.Prefab.GetComponent<Renderer>().bounds.size.x * rndScale; 
				float _yShift;
				if ( _obstacle.Prefab.name == "Bush" ) {
					_yShift = _obstacle.Prefab.GetComponent<Renderer>().bounds.size.y * rndScale * 0.385f;
				} else {
					_yShift = _obstacle.Prefab.GetComponent<Renderer>().bounds.size.y * rndScale * 0.5f;
				}
				pos += new Vector2(0, (Grid.cellSize.y * 0.95f ) + _yShift);
				int leftConstraint = 0;
				if (  Tilemap.GetTile(new Vector3Int(x - 1, y + 1, 0)) != null | Tilemap.GetTile(new Vector3Int(x - 1, y, 0)) == null  ) {
					leftConstraint = 1; 
				}
				pos.x += Random.Range(0, 1f) * Grid.cellSize.x + leftConstraint * (_newXsize * 0.5f); 

				if ( _previousObstacle != null ) {
					float minimalDistance = _previousObstacle.transform.position.x + (_previousObstacle.GetComponent<Renderer>().bounds.size.x * 0.5f) + (_newXsize * 0.5f) + ObstacleDelta;
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
					if ( Random.Range(0, 2) == 0 ) {
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
				if ( Random.Range(0, 100) <= _probability ) {
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
		//for ( int i = 0; i < cells.Count / 10; i++ ) {
		//	hogNum += Random.Range(0, _maxNum + 1);
		//}
		hogNum = Random.Range(0, cells.Count / 10 + 1);
		var freeCells = new List<int>();
		foreach ( KeyValuePair<int, int> c in cells ) {
			if ( c.Value == 0 ) {
				if ( Tilemap.GetTile(new Vector3Int(c.Key - 1, GetUpperBound(Tilemap, c.Key) + 1, 0)) == null & Tilemap.GetTile(new Vector3Int(c.Key + 1, GetUpperBound(Tilemap, c.Key) + 1, 0)) == null ) {
					freeCells.Add(c.Key);
				} 
			} 
		}
		for ( int i = 0; i < hogNum; i++ ) {
			int rndX = freeCells[Random.Range(0, freeCells.Count)];

			for ( int j = rndX - 1; j <= rndX + 1; j++ ) {
				freeCells.Remove(j);
				if ( cells.ContainsKey(j) ) {
					cells[j] = 1; 
				}
			} 

			Vector2 pos = Grid.CellToWorld(new Vector3Int(rndX, GetTopGroundIndex(rndX), 0));
			float rndScale = Random.Range(0.85f, 1f);
			var hog = hogPool.Get().gameObject;
			if ( hog.transform.parent == null ) {
				hog.transform.SetParent(Hogs);
			}
			hog.transform.localScale = new Vector3(rndScale, rndScale, 1);
			float _yShift = hog.transform.GetChild(0).GetComponent<Renderer>().bounds.size.y * 0.5f;
			pos.y += Grid.cellSize.y * 0.95f + _yShift;
			pos.x += Grid.cellSize.x * 0.5f;
			hog.transform.position = pos;
			if ( Random.Range(0, 4) == 0 ) {
				hog.transform.Rotate(new Vector3(0, 180, 0));
			}
		}
	}
	void PlaceBees(int _maxNum, Dictionary<int, int> cells) {
		int beeNum = 0;
		//for ( int i = 0; i < cells.Count / 10; i++ ) {
		//	beeNum += Random.Range(0, _maxNum + 1);
		//}
		beeNum = Random.Range(0, cells.Count / 10 + 1);
		var freeCells = new List<int>();
		foreach ( KeyValuePair<int, int> c in cells ) {
			if ( c.Value == 0 ) {
				if ( Tilemap.GetTile(new Vector3Int(c.Key - 1, GetUpperBound(Tilemap, c.Key) + 1, 0)) == null & Tilemap.GetTile(new Vector3Int(c.Key + 1, GetUpperBound(Tilemap, c.Key) + 1, 0)) == null ) {
					freeCells.Add(c.Key);
				} 
			} 
		}
		for ( int i = 0; i < beeNum; i++ ) {
			int rndX = freeCells[Random.Range(0, freeCells.Count)];
			for ( int j = rndX - 1; j <= rndX + 1; j++ ) {
				freeCells.Remove(j);
				if ( cells.ContainsKey(j) ) {
					cells[j] = 2; 
				}
			}
			int topGroundIndex = GetTopGroundIndex(rndX);
			Vector2 pos = Grid.CellToWorld(new Vector3Int(rndX, topGroundIndex, 0));
			pos.y += Grid.cellSize.y + 1.4f;
			pos.x += Grid.cellSize.x / 2f;
			var bee = beePool.Get().gameObject;
			bee.transform.SetParent(Bees); 
			if ( Tilemap.GetTile(new Vector3Int(rndX - 1, topGroundIndex, 0)) == null ) {
				pos += new Vector2(0.4f, 0.25f);
			} else if ( Tilemap.GetTile(new Vector3Int(rndX + 1, topGroundIndex, 0)) == null ) {
				pos += new Vector2(-0.5f, 0.25f); 
			}
			bee.transform.position = pos;
			float rndScale = Random.Range(0.85f, 1f);
			bee.transform.localScale = new Vector3(rndScale, rndScale, 1);
			if ( Random.Range(0, 4) == 0 ) {
				bee.transform.Rotate(new Vector3(0, 180, 0));
			}
		}
	}

	void PlaceIslands(Dictionary<int, int> cells) {
		float maxDeltaUp   = 2.3f;
		float maxDeltaDown = 1.5f;
		float minDistance  = 1.3f;
		float maxDistance  = 4f;
		float minHeight    = Tilemap.cellSize.y + 0.5f;
		float lastX        = 0f;
		int   recCounter   = 0; 
		var   scaleBounds  = new Vector2(0.3f, 1);
		float cutOffX      = 0f;

		var indexes = new List<int>();
		foreach ( KeyValuePair<int, int> c in cells ) {
			indexes.Add(c.Key);
		}
		indexes.Sort();
		cutOffX = Tilemap.CellToWorld(new Vector3Int(indexes[indexes.Count - 1], 0, 0)).x;
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
			if ( Random.Range(0,5) == 4) {
				return;
			}
			float scaleX = Random.Range(scaleBounds.x, scaleBounds.y);
			float newSizeX = Island.GetComponent<Renderer>().bounds.size.x * scaleX; 
			float baseShift = node.x + 0.5f * newSizeX;
			float islandY;
			float islandX;
			if ( recCounter == 1 ) {
				islandX = Random.Range(baseShift + 0.8f, baseShift + maxDistance);
				islandY = Random.Range(node.y + 1, node.y + maxDeltaUp);
			} else {
				islandX = Random.Range(baseShift + minDistance, baseShift + maxDistance);
				islandY = Random.Range(node.y - maxDeltaDown, node.y + maxDeltaUp);
			}
			if ( islandX > cutOffX ) {
				return;
			}

			int islandCellX = Tilemap.WorldToCell(new Vector3(islandX, 0, 0)).x;
			if ( cells.ContainsKey(islandCellX) ) {
				if ( cells[islandCellX] == 2 ) {
					return;
				} 
			}

			float rightEdgeX = islandX + (newSizeX * 0.5f);
			float leftEdgeX = islandX - (newSizeX * 0.5f);
			if ((GetHeight(leftEdgeX, islandY ) >= minHeight) & (GetHeight(rightEdgeX, islandY ) >= minHeight)) { 
				if ( IsPassable(rightEdgeX, islandY, scaleX) ) {
					var island = islandsPool.Get().gameObject;
					if ( island.transform.parent == null ) {
						island.transform.SetParent(Islands);
					}
					island.transform.position = new Vector2(islandX, islandY);
					island.transform.localScale = new Vector3(scaleX, 1, 1);
					lastX = rightEdgeX;

					recCounter++;
					if ( recCounter == 4 ) {
						Instantiate(Apple, new Vector2(islandX, islandY + 1f), Quaternion.identity, Apples);
						return;
					} else if(recCounter == 3){
						if ( Random.Range(0, 2) != 0 ) {
							Instantiate(Apple, new Vector2(islandX, islandY + 1f), Quaternion.identity, Apples);
						}

					}
					var newNode = new Vector2(islandX + (newSizeX * 0.5f), islandY);
					TryPlace(newNode); 
				} 
			} 
		}

		bool IsPassable(float x, float y, float scale) {
			int cellX = Tilemap.WorldToCell(new Vector3(x, 0, 0)).x + 1;
			int cellY = GetUpperBound(Tilemap, cellX);
			Vector2 leftUpperCorner = Tilemap.CellToWorld(new Vector3Int(cellX, cellY, 0));
			leftUpperCorner += new Vector2(-(Tilemap.cellSize.x / 2), Tilemap.cellSize.y);
			float magnitude = (new Vector2(x,y) - leftUpperCorner).magnitude;
			if ( magnitude > 2f ) {
				return true; 
			} else {
				return false;
			} 
		}

		float GetHeight(float x, float y) {
			int cellX = Tilemap.WorldToCell(new Vector3(x, 0, 0)).x;
			Vector3 pos = Tilemap.CellToWorld(new Vector3Int(cellX, GetUpperBound(Tilemap, cellX), 0));
			return y - (pos.y + Tilemap.cellSize.y); 
		} 
	}

	void PlaceDecor(int _x0, int _x1) {
		//Кусты и камни на заднем фоне
		Vector2 scaleLim = new Vector2(0.5f, 2.7f);
		int propsNum = 0;
		int iterations = (_x1 - _x0) / 10;
		for ( int i = 0; i < iterations; i++ ) {
			propsNum += Random.Range(0, MaxPropsOnTenCells + 1);
		}
		List<int> cells = new List<int>();
		for ( int i = 0; i < propsNum; i++ ) {
			int nextCell = Random.Range(_x0, _x1);
			while ( cells.Contains(nextCell) ) {
				nextCell = Random.Range(_x0, _x1);
			}
			cells.Add(nextCell);
			Vector3 pos = Tilemap.CellToWorld(new Vector3Int(nextCell, GetUpperBound(Tilemap, nextCell), 0));
			var dData = BGPropsDatas[Random.Range(0, BGPropsDatas.Count)];
			float rndScale = Random.Range(scaleLim.x, scaleLim.y);
			float _yShift = dData.Prefab.GetComponent<Renderer>().bounds.size.y * rndScale * 0.5f;
			pos.y += Grid.cellSize.y * 0.85f + _yShift; 
			int c = 0;
			while ( !FullyInGround(dData.Prefab.transform, rndScale, pos) ) {
				pos -= new Vector3(0, Grid.cellSize.y * 0.5f, 0);
				c++;
				if ( c == 3 ) {
					return;
				}
			}
			var decor = dData.Pool.Get();
			if ( decor.transform.parent == null ) {
				decor.transform.SetParent(BGDecor);
			}
			decor.transform.localScale = new Vector3(rndScale, rndScale); 
			decor.transform.position = pos; 

			if ( Random.Range(0, 2) == 1 ) {
				decor.GetComponent<SpriteRenderer>().flipX = true;
			} else {
				decor.GetComponent<SpriteRenderer>().flipX = false; 
			}
		} 


		//черепа и кости
		propsNum = 0;
		for ( int i = 0; i < iterations; i++ ) {
			propsNum += Random.Range(0, MaxSmallPropsOnTenCells + 1);
		} 
		scaleLim = new Vector2(0.4f, 1.3f);
		cells.Clear();
		for ( int i = 0; i < propsNum; i++ ) {
			int nextCell = Random.Range(_x0, _x1);
			while ( cells.Contains(nextCell) ) {
				nextCell = Random.Range(_x0, _x1);
			}
			cells.Add(nextCell);
			Vector3 pos = Tilemap.CellToWorld(new Vector3Int(nextCell, GetUpperBound(Tilemap, nextCell) - Random.Range(0, 2), 0));
			pos += Grid.cellSize * 0.5f; 
			var dData = FGPropsDatas[Random.Range(0, FGPropsDatas.Count)];
			float rndScale = Random.Range(scaleLim.x, scaleLim.y);
			var decor = dData.Pool.Get();
			if ( decor.transform.parent == null ) {
				decor.transform.SetParent(Decor);
			}
			decor.transform.localScale = new Vector3(rndScale, rndScale); 
			decor.transform.position = pos;
			//decor.transform.RotateAroundLocal(new Vector3(0, 0, 1), Random.Range(0, 95f));
			decor.transform.Rotate(new Vector3(0, 0, Random.Range(0, 180)));

			if ( Random.Range(0, 2) == 1 ) {
				decor.GetComponent<SpriteRenderer>().flipX = true;
			} else {
				decor.GetComponent<SpriteRenderer>().flipX = false; 
			} 
		}





		bool IsInGround(Vector3 pos) {
			if ( Tilemap.GetTile(Tilemap.WorldToCell(pos)) == null ) {
				return false;
			} else {
				return true;
			}
		}

		bool FullyInGround(Transform s, float scale, Vector3 pos) {
			Renderer rend = s.GetComponent<Renderer>();
			if ( !IsInGround(pos - (rend.bounds.extents * scale)) ) {
				return false;
			} else if ( !IsInGround(pos + new Vector3(rend.bounds.extents.x * scale, -rend.bounds.extents.y * scale, 0)) ) {
				return false; 
			}
			return true;
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

	public int[,] RandomWalkTopSmoothed(int[,] map, int minSectionWidth, int nodeY) {

		//Determine the start position
		//int lastHeight = Random.Range(0, map.GetUpperBound(1));
		int lastHeight = Random.Range(0, map.GetLength(1));

		int shift = lastHeight - nodeY;
		if ( shift < 0 ) shift = 0;

		int bottomBound;
		if ( _minCellY < -shift  ) {
			bottomBound = 0; 
		} else {
			bottomBound = _minCellY + shift;
		} 

		//Used to determine which direction to go
		int nextMove = 0;
		//Used to keep track of the current sections width
		int sectionWidth = 0;
		Array.Clear(map, 0, map.Length);

		//for ( int i = 0; i <= map.GetUpperBound(0); i++ ) {
		//	for ( int j = 0; j <= map.GetUpperBound(1); j++ ) {
		//		map[i, j] = 0;
		//	}
		//}

		for ( int x = 0; x <= map.GetUpperBound(0); x++ ) {
			//Determine the next move
			nextMove = Random.Range(0, 2);

			//Only change the height if we have used the current height more than the minimum required section width
			//if ( nextMove == 0 && lastHeight > 0 && sectionWidth > minSectionWidth ) {
			if ( nextMove == 0 && lastHeight > bottomBound && sectionWidth > minSectionWidth ) {
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
			for ( int y = map.GetUpperBound(1); y >= 0; y-- ) {
				int _tx = shiftX + x;
				int _ty = y - shift;
				if ( _ty <= _minCellY ) {
					if ( tilemap.GetTile(new Vector3Int(_tx, _ty + 1, 0)) == null ) {
						tilemap.SetTile(new Vector3Int(_tx, _ty, 0), Grass);
					} else {
						tilemap.SetTile(new Vector3Int(_tx, _ty, 0), Ground);
					}
				} else if (_ty >= _maxCellY) {
					continue;
				} else if ( map[x, y] == 1 ) {
					tilemap.SetTile(new Vector3Int(_tx, _ty, 0), Ground);
				} else if ( map[x, y] == 2 ) {
					tilemap.SetTile(new Vector3Int(_tx, _ty, 0), Grass);
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

	public void RenderGrassMap(int x0, int x1) {
		for ( int x = x0; x <= x1; x++ ) {
			int y = GetUpperBound(Tilemap, x) + 1;
			var v = new Vector3Int(x, y, 0);
			ForegroundGrass.SetTile(v, DecorGrass[Random.Range(0, DecorGrass.Count)]);
			BackgroundGrass.SetTile(v, DecorGrass[Random.Range(0, DecorGrass.Count)]);
		} 
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

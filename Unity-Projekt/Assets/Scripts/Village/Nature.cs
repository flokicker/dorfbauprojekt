using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles all nature related stuff, e.g. Flora spawning
public class Nature : Singleton<Nature> {

	// Factor for total flora spawning, currently constant
	private float floraSpawningFactor = 1.0f;

    /* Individual NatureObjectScript spawning (tree,mushroom,mushroomStump,reed,corn,rock)
    private float[] plantSpawningFactor = {
        0.5f, 2, 0, 0.5f, 4f, 0f, 0f
    };
    private float[] plantSpawningLimit = {
        1450, 420, 55, 35, 400, 50, 40
    };*/
    private float[] natureObjectSpawningTime;

    // Collection of all Flora Elements
    public static HashSet<NatureObjectScript> nature = new HashSet<NatureObjectScript>();

    // Collection of shore nodes, where reed can grow
    public static HashSet<Node> shore = new HashSet<Node>();

	// All prefabs and parents
    private Transform plantsParent;

    // Infos to handle model instantiating in array format
    private List<GameObject>[] plants;

    [SerializeField]
    public Transform forestParent, lakeParent, initialSpawnpoint;

    private void Awake()
    {
        shore = new HashSet<Node>();
        nature = new HashSet<NatureObjectScript>();
    }

    void Start () 
	{
        // initialize individual NatureObjectScript SpawningFactor times
        natureObjectSpawningTime = new float[NatureObject.Count];
        for(int i = 0; i < natureObjectSpawningTime.Length; i++)
            natureObjectSpawningTime[i] = 0f;

		// Setup references to parent transforms
        plantsParent = transform.Find("Flora");

        //System.Enum.GetValues(typeof(NatureObjectType)).Length
        typeCount = new int[NatureObject.Count];

    }

    private int[] typeCount;

    void Update () {
		
        /*for(int i = 0; i < typeCount.Length; i++)
            typeCount[i] = 0;
        foreach(NatureObjectScript p in nature)
        {
            if ((int)p.Type < typeCount.Length)
            {
                typeCount[(int)p.Type]++;
            }
        }*/
        // NatureObjectScript SpawningFactor
        //int month = GameManager.GetMonth();
        /* TODO: spawn in right month */
        for(int i = 0; i < natureObjectSpawningTime.Length; i++)
        {
            NatureObject no = NatureObject.Get(i);
            if (no.growth < float.Epsilon) continue;
            if (no.type == NatureObjectType.Water) continue;

            natureObjectSpawningTime[i] += Time.deltaTime;
            float gt = 60f / (floraSpawningFactor * no.spawningFactor);
            if(natureObjectSpawningTime[i] >= gt)
            {
                natureObjectSpawningTime[i] -= gt;
                // Limit splant spawning
                if(typeCount[i] < no.spawningLimit)
                    SpawnRandomPosNatureObject(no, 0);
            }
        }
	}

    // TODO: load/save nature to object (maybe json string?)
    public void SetupNature()
    {
        // Find places to grow reed
        Vector2[] deltas = new Vector2[] { new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0), new Vector2(0, -1) };
        for (int x = 0; x < Grid.WIDTH; x++)
        {
            for (int y = 0; y < Grid.HEIGHT; y++)
            {
                Vector3 worldPos = Grid.ToWorld(x, y);
                float smph = Terrain.activeTerrain.SampleHeight(worldPos);
                if (smph < 1-0.1f)
                {
                    bool canGrowReed = false;
                    for (int i = 0; i < deltas.Length; i++)
                    {
                        if (Grid.ValidNode((int)(x + deltas[i].x), (int)(y + deltas[i].y)) && Grid.GetNode((int)(x + deltas[i].x), (int)(y + deltas[i].y)).Walkable())
                        {
                            canGrowReed = true;
                            break;
                        }
                    }
                    if (canGrowReed)
                    {
                        shore.Add(Grid.GetNode(x,y));
                    }
                }
            }
        }

        // For now just spawn some random nature
        SetupRandomNature();
    }

    private void SetupRandomNature()
    {
        // If in editor, spawn less nature to speed up testing
        if (Application.isEditor) Spawn(30, 5, 5, 5, 30, 5, 5, 5, 5);// Spawn(50, 80, 15, 5, 30, 40, 20); //Spawn(200,50,15,10,70,15);//Spawn(50,20,5,5,30,5);

        // Spawn some random plants
        else
        {
            Spawn(850, 320, 45, 25, 340, 40, 20, 200, 200);

            for (int i = 0; i < forestParent.childCount; i++)
            {
                Forest f = forestParent.GetChild(i).GetComponent<Forest>();
                SpawnForest(f.trees, f.transform.position);
            }
        }

        /*for (int i = 0; i < lakeParent.childCount; i++)
        {
            Transform lk = lakeParent.GetChild(i);
            SpawnLake(lk);
        }*/

        int count = 0;
        int x, z;
        do
        {
            x = Random.Range(-5, 5) + Grid.SpawnX;
            z = Random.Range(-5, 5) + Grid.SpawnY;
        } while ((Grid.GetNode(x, z).IsOccupied() || Grid.GetNode(x, z).IsWater() || (Mathf.Abs(Grid.SpawnX) < 5 && Mathf.Abs(Grid.SpawnY) < 5)) && (++count) < 100);
        if (count < 100)
        {
            NatureObject no = NatureObject.Get("Pilzstrunk");
            GameNatureObject go = new GameNatureObject(no, x, z);
            go.SetPosition(Grid.ToWorld(x + no.gridWidth / 2, z + no.gridHeight / 2));
            go.SetRotation(Quaternion.Euler(0, Random.Range(0, 360), 0));
            SpawnNatureObject(go);
        }

    }

    /*public static void SpawnLake(Transform lakePos)
    {
        NatureObject no = NatureObject.Get("Teich");
        Vector3 gpos = Grid.ToGrid(lakePos.position);
        GameNatureObject go = new GameNatureObject(no, (int)gpos.x, (int)gpos.z);
        go.SetPosition(lakePos.transform.position);
        go.SetRotation(Quaternion.identity);
        SpawnNatureObject(go);
    }*/

    public static void SpawnForest(int treeAm, Vector3 pos)
    {
        NatureObject no = NatureObject.Get("Birke");
        GameNatureObject go;

        Vector3 node = Grid.ToGrid(pos);
        int gx = (int)node.x;
        int gy = (int)node.z;

        for (int i = 0; i < treeAm; i++)
        {
            int x = 0;
            int z = 0;
            int count = 0;
            do
            {
                int spread = treeAm/10 + count + 1;
                x = gx + Random.Range(-spread, spread);
                z = gy + Random.Range(-spread, spread);
            } while (!Grid.ValidNode(x,z) || (Grid.GetNode(x, z).IsOccupied() || (Mathf.Abs(Grid.SpawnY - x) < 5 && Mathf.Abs(Grid.SpawnY - z) < 5)) && (++count) < 100);

            if (count == 100) continue;

            go =  new GameNatureObject(no, x, z);
            go.SetPosition(Grid.ToWorld(x + no.gridWidth / 2, z + no.gridHeight / 2));
            go.SetRotation(Quaternion.Euler(0, Random.Range(0, 360), 0));
            SpawnNatureObject(go);
        }
    }
	
    public static void SpawnRandomPosNatureObject(NatureObject baseNo, int rndSize)
    {
        Terrain terrain = Terrain.activeTerrain;

        bool invalid = false;

        int x = 0;
        int z = 0;
        int count = 0;
        if (baseNo.type == NatureObjectType.Reed)
        {
            List<Node> sn = new List<Node>(shore);
            if (sn.Count == 0) return;
            Node rsn = sn[Random.Range(0, sn.Count)];
            while (rsn.nodeObject != null && (++count) < 100)
                rsn = sn[Random.Range(0, sn.Count)];
            if (count == 100) return;
            x = rsn.gridX;
            z = rsn.gridY;
        }
        else
        {
            x = UnityEngine.Random.Range(0, Grid.WIDTH);
            z = UnityEngine.Random.Range(0, Grid.HEIGHT);
            while ((Grid.GetNode(x, z).IsOccupied() || Grid.GetNode(x, z).IsWater() || (Mathf.Abs(Grid.SpawnX - x) < Mathf.Max(5, baseNo.minDistToCenter) && Mathf.Abs(Grid.SpawnY - z) < Mathf.Max(5, baseNo.minDistToCenter))) && (++count) < 100)
            {
                x = UnityEngine.Random.Range(0, Grid.WIDTH);
                z = UnityEngine.Random.Range(0, Grid.HEIGHT);
            }
            if (count == 100) return;
            for (int dx = 0; dx < 3; dx++)
            {
                if (invalid) continue;
                for (int dy = 0; dy < 3; dy++)
                {
                    if (invalid) continue;
                    if (!Grid.ValidNode(x + dx, z + dy)) invalid = true;
                    else if (Grid.GetNode(x + dx, z + dy).IsOccupied()) invalid = true;
                }
            }
        }
        if (invalid) return;

        GameNatureObject toSpawn = new GameNatureObject(baseNo, x, z);
        toSpawn.SetPosition(Grid.ToWorld(x + baseNo.gridWidth / 2, z + baseNo.gridHeight / 2));
        toSpawn.SetRotation(Quaternion.Euler(0, Random.Range(0, 360), 0));

        NatureObjectScript nos = SpawnNatureObject(toSpawn);
        nos.SetRandSize(rndSize);
    }

    public static NatureObjectScript SpawnNatureObject(GameNatureObject no)
    {
        GameObject obj = Instantiate(no.GetModelToSpawn(), no.GetPosition(),
                no.GetRotation(), Instance.plantsParent);
        NatureObjectScript nos = obj.AddComponent<NatureObjectScript>();
        nos.tag = "NatureObject";
        nos.SetNatureObject(no);

        nature.Add(nos);

        for (int dx = 0; dx < nos.GridWidth; dx++)
        {
            for (int dy = 0; dy < nos.GridHeight; dy++)
            {
                if(!Grid.ValidNode(nos.GridX+dx, nos.GridY+dy)) continue;
                Node n = Grid.GetNode(nos.GridX+dx, nos.GridY+dy);
                n.SetNodeObject(obj.transform);
                n.objectWalkable = nos.Walkable;
            }
        }

        return nos;
    }

    /*private void SpawnNatureObject(NatureO int randSize)
    {
        Terrain terrain = Terrain.activeTerrain;

        bool invalid = false;
        int species = Random.Range(0, plants[(int)type].Count);
        
        int x = 0;
        int z = 0;
        int count = 0;
        if(type == NatureObjectType.Reed)
        {
            List<Node> sn = new List<Node>(shore);
            if(sn.Count == 0) return;
            Node rsn = sn[Random.Range(0, sn.Count)];
            while(rsn.nodeObject != null && (++count) < 100)
                rsn = sn[Random.Range(0, sn.Count)];
            if(count == 100) return;
            x = rsn.gridX;
            z = rsn.gridY;
        }
        else
        {
            x = UnityEngine.Random.Range(0, Grid.WIDTH);
            z = UnityEngine.Random.Range(0, Grid.HEIGHT);
            while((Grid.GetNode(x,z).IsOccupied() || (Mathf.Abs(Grid.WIDTH / 2 - x) < 5 && Mathf.Abs(Grid.HEIGHT / 2 - z) < 5)) && (++count) < 100)
            {
                x = UnityEngine.Random.Range(0, Grid.WIDTH);
                z = UnityEngine.Random.Range(0, Grid.HEIGHT);
            }
            if(count == 100) return;
            for (int dx = 0; dx < 3; dx++)
            {
                if (invalid) continue;
                for (int dy = 0; dy < 3; dy++)
                {
                    if (invalid) continue;
                    if (!Grid.ValidNode(x + dx, z + dy)) invalid = true;
                    else if (Grid.GetNode(x + dx, z + dy).IsOccupied()) invalid = true;
                }
            }
        }
        if (invalid) return;

        GameNatureObject toSpawn = new GameNatureObject(NatureObject.Get(), Grid.WIDTH / 2 - 1, Grid.HEIGHT / 2 - 1, 3);
        toSpawn.SetPosition(Vector3.zero);
        toSpawn.SetRotation(Quaternion.Euler(0, -90, 0));

        GameObject obj = (GameObject)Instantiate(plants[(int)type][species], Vector3.zero
                ,Quaternion.Euler(0, Random.Range(0, 360), 0), 
            plantsParent);
        NatureObjectScript nos = obj.AddComponent<NatureObjectScript>();
        nos.tag = "NatureObject";
        nos.SetNatureObject(no)

        nos.SetPos(gridX, gridY);

        if(randSize > 0) nos.SetRandSize(randSize);

        // Set correct position of NatureObjectScript in terrain
        Vector3 worldPos = Grid.ToWorld(x + nos.gridWidth / 2, z + nos.gridHeight / 2);
        float smph = terrain.SampleHeight(worldPos);
        worldPos.y = terrain.transform.position.y + smph;

        NatureObjectScript.transform.position = worldPos;

        nature.Add(NatureObjectScript);

        for (int dx = 0; dx < NatureObjectScript.gridWidth; dx++)
        {
            for (int dy = 0; dy < NatureObjectScript.gridHeight; dy++)
            {
                Grid.GetNode(x+dx,z+dy).SetNodeObject(obj.transform);
                if(type == NatureObjectType.Rock) Grid.GetNode(x+dx, z+dy).objectWalkable = false;
            }
        }
    }*/

    // Spawn the given amount of trees,mushrooms,reeds and rocks
    private void Spawn(int countTrees, int countMushrooms, int countMushroomStumps, int countReed, int countCorn, int countRocks, int energySpots, int countPearTree, int countFirTree)
    {
        int[] counts = new int[] { countTrees, countRocks, countCorn, countMushrooms, countMushroomStumps, countReed, energySpots, countPearTree, countFirTree };
        for (int i = 0; i < counts.Length; i++)
        {
            NatureObject baseNo = NatureObject.Get(i);
            if (!baseNo) continue;
            if (baseNo.type == NatureObjectType.Water) continue;
            for (int j = 0; j < counts[i]; j++ )
            {
                SpawnRandomPosNatureObject(baseNo, 3);
            }
        }
    }
}

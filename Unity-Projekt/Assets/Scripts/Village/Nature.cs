using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles all nature related stuff, e.g. Flora spawning
public class Nature : MonoBehaviour {

	// Factor for total flora spawning, currently constant
	private float floraSpawningFactor = 1.0f;
    // Individual plant spawning
    private float[] plantSpawningFactor = {
        1, 60, 1
    };
    private float[] plantSpawningTime;

	// Collection of all Flora Elements
	public HashSet<Plant> flora = new HashSet<Plant>();

    // Collection of shore nodes, where reed can grow
    private HashSet<Node> shore = new HashSet<Node>();

	// All prefabs and parents
    [SerializeField]
    private List<GameObject> trees, mushroom, reed;
    private Transform plantsParent;

    // Infos to handle model instantiating in array format
    private List<GameObject>[] plants;

	void Start () 
	{
        plants = new List<GameObject>[] { trees, mushroom, reed};

        // initialize individual plant SpawningFactor times
        plantSpawningTime = new float[plantSpawningFactor.Length];
        for(int i = 0; i < plantSpawningTime.Length; i++)
            plantSpawningTime[i] = 0f;


		// Setup references to parent transforms
        plantsParent = transform.Find("Flora");
	}
	
	void Update () {
		
        // plant SpawningFactor
        for(int i = 0; i < plantSpawningTime.Length; i++)
        {
            plantSpawningTime[i] += Time.deltaTime;
            float gt = 60f / (floraSpawningFactor * plantSpawningFactor[i]);
            if(plantSpawningTime[i] >= gt)
            {
                plantSpawningTime[i] -= gt;
                SpawnPlant((PlantType)i, false);
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
                    if (canGrowReed && Random.Range(0, 5) == 0)
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
		// Spawn some random plants
		Spawn(80, 30, 2, 15);
    }
	
    private void SpawnPlant(PlantType type, bool randSize)
    {
        bool invalid = false;
        int species = Random.Range(0, plants[(int)type].Count);
        
        int x = 0;
        int z = 0;
        int count = 0;
        if(type == PlantType.Reed)
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
        GameObject obj = (GameObject)Instantiate(plants[(int)type][species], Vector3.zero
                ,Quaternion.Euler(0, Random.Range(0, 360), 0), 
            plantsParent);
        Plant plant = obj.AddComponent<Plant>();
        plant.Init(type,species);
        plant.tag = "Plant";

        if(randSize) plant.SetRandSize();

        // Set correct position of plant in terrain
        Vector3 worldPos = Grid.ToWorld(x + plant.gridWidth / 2, z + plant.gridHeight / 2);
        float smph = Terrain.activeTerrain.SampleHeight(worldPos);
        worldPos.y = Terrain.activeTerrain.transform.position.y + smph;

        plant.transform.position = worldPos;

        flora.Add(plant);

        for (int dx = 0; dx < plant.gridWidth; dx++)
        {
            for (int dy = 0; dy < plant.gridHeight; dy++)
            {
                Grid.GetNode(x+dx, z+dy).nodeObject = obj.transform;
            }
        }
    }

    // Spawn the given amount of trees,mushrooms,reeds and rocks
    private void Spawn(int countTrees, int countMushrooms, int countReed, int countRocks)
    {
        int[] counts = new int[] { countTrees, countMushrooms, countReed };
        PlantType[] pt = new PlantType[] { PlantType.Tree, PlantType.Mushroom, PlantType.Reed };
        for (int i = 0; i < counts.Length; i++)
        {
            for (int j = 0; j < counts[i]; j++ )
            {
                SpawnPlant(pt[i], true);
            }
        }

        /* TODO: implement rock spawning */
    }
}

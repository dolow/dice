using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;

[DataContract]
public struct Walker
{
    public static Walker Empty = new Walker(-1, -1, -1, false);

    [DataMember]
    public int type;
    [DataMember]
    public int direction;
    [DataMember]
    public int address;
    [DataMember]
    public bool cameraFollow;

    public Walker(int type, int direction, int address, bool cameraFollow)
    {
        this.type = type;
        this.direction = direction;
        this.address = address;
        this.cameraFollow = cameraFollow;
    }
}

[DataContract]
public struct Map
{
    [DataMember]
    public string name;
    [DataMember]
    public int width;
    [DataMember]
    public int height;
    [DataMember]
    public int[][] fields;
    [DataMember]
    public int[][][] verticalFields;
    [DataMember]
    public int[][] terrains;

    [DataMember]
    public Walker[] walkers;
}

public class MapLoader
{
    public const int EmptyField = 0;
    public static string MapJsonDirectory = "Data";
    public static string MapFieldMaterialDirectory = "Materials";

    public static Vector3 FieldUnit = new Vector3(1.0f, 1.0f, 1.0f);
    public static Quaternion SurfaceUp = Quaternion.Euler(new Vector3(90.0f, 0.0f, 0.0f));
    public static Dictionary<DirectionUtil.Direction, Quaternion> SurfaceDirection = new Dictionary<DirectionUtil.Direction, Quaternion>() {
        { DirectionUtil.Direction.South, Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f)) },
        { DirectionUtil.Direction.North, Quaternion.Euler(new Vector3(0.0f, 180.0f, 0.0f)) },
        { DirectionUtil.Direction.East,  Quaternion.Euler(new Vector3(0.0f, -90.0f, 0.0f)) },
        { DirectionUtil.Direction.West,  Quaternion.Euler(new Vector3(0.0f, 90.0f, 0.0f)) },
    };
    public static Dictionary<DirectionUtil.Direction, Vector3> SurfaceOffset = new Dictionary<DirectionUtil.Direction, Vector3>() {
        { DirectionUtil.Direction.South, new Vector3(0.0f,  0.0f, -0.5f) },
        { DirectionUtil.Direction.North, new Vector3(0.0f,  0.0f, 0.5f)  },
        { DirectionUtil.Direction.East,  new Vector3(0.5f,  0.0f, 0.0f)  },
        { DirectionUtil.Direction.West,  new Vector3(-0.5f, 0.0f, 0.0f)  },
    };

    private static Dictionary<string, Material> materialCache = new Dictionary<string, Material>();
    private static Map currentMap;

    public static Map CurrentMap()
    {
        return MapLoader.currentMap;
    }

    public static Map Load(
        string name,
        GameObject mapRoot,
        MeshFilter tilePool,
        FieldTile fieldPrototype,
        FieldTile verticalFieldPrototype,
        FieldCollider fieldColliderPrototype,
        WalkerBehavior walkerPrototype,
        Action<Map, MeshFilter, int, int> onEachTile
    )
    {
        string path = Path.Combine(MapLoader.MapJsonDirectory, name);
        TextAsset asset = Resources.Load<TextAsset>(path);
        
        if (asset == null)
        {
            Debug.Log("could not load map file: " + path);
            return new Map();
        }

        // JsonUtility does not support nested array
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Map));
        using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(asset.text)))
        {
            MapLoader.currentMap = (Map)serializer.ReadObject(ms);
        }

        MapLoader.currentMap.height = (int)Mathf.Floor(MapLoader.currentMap.terrains[0].Length / MapLoader.currentMap.width);

        MapLoader.InstantiateFields(mapRoot, tilePool, fieldPrototype, verticalFieldPrototype, fieldColliderPrototype, onEachTile);
        MapLoader.InstantiateWalkerBehaviors(mapRoot, walkerPrototype);

        return MapLoader.currentMap;
    }

    private static GameObject InstantiateFields(
        GameObject mapRoot,
        MeshFilter tilePool,
        FieldTile fieldPrototype,
        FieldTile verticalPrototype,
        FieldCollider fieldColliderPrototype,
        Action<Map, MeshFilter, int, int> onEachTile
    )
    {
        Map map = MapLoader.currentMap;

        mapRoot.isStatic = true;

        int fieldAspects = 4;
        int width = map.width;

        int floors = (map.fields.Length > map.verticalFields.Length)
            ? map.fields.Length
            : map.verticalFields.Length;

        Dictionary<int, List<MeshFilter>> meshFilterDictionary = new Dictionary<int, List<MeshFilter>>();
        Dictionary<int, List<MeshFilter>> verticalMeshFilterDictionary = new Dictionary<int, List<MeshFilter>>();

        // TODO: reduce draw call, RT? Mesh Combine?
        for (int floor = 0; floor < floors; floor++)
        {
            int[] floorField = (map.fields.Length > floor)
                ? map.fields[floor]
                : new int[] { };

            int[][] floorVerticalField = (map.verticalFields.Length > floor)
                ? map.verticalFields[floor]
                : new int[][]{ };

            int indexes = (floorField.Length >= floorVerticalField.Length)
                ? floorField.Length
                : floorVerticalField.Length;

            for (int i = 0; i < indexes; i++)
            {
                int col = i % width;
                int row = (int)Mathf.Floor(i / width);

                do
                {
                    if (floorField.Length <= i)
                        break;

                    int fieldId = floorField[i];
                    if (fieldId == MapLoader.EmptyField)
                        continue;

                    MeshFilter filter = GameObject.Instantiate<MeshFilter>(tilePool);
                    filter.transform.parent = mapRoot.transform.transform;
                    filter.transform.rotation = MapLoader.SurfaceUp;
                    filter.transform.position = new Vector3(col * FieldUnit.x + 0.5f, (float)floor * FieldUnit.y, -row * FieldUnit.z - 0.5f);

                    if (!meshFilterDictionary.ContainsKey(fieldId))
                        meshFilterDictionary.Add(fieldId, new List<MeshFilter> { });

                    List<MeshFilter> filters = meshFilterDictionary[fieldId];
                    filters.Add(filter);
                    onEachTile?.Invoke(MapLoader.currentMap, filter, col, row);
                } while (false);

                do
                {
                    if (floorVerticalField.Length <= i)
                        break;

                    int[] verticalFieldDirections = floorVerticalField[i];
                    if (verticalFieldDirections.Length != fieldAspects)
                        break;

                    for (int directionNumber = 0; directionNumber < verticalFieldDirections.Length; directionNumber++)
                    {
                        int verticalFieldId = verticalFieldDirections[directionNumber];
                        if (verticalFieldId == MapLoader.EmptyField)
                            continue;

                        DirectionUtil.Direction direction = (DirectionUtil.Direction)directionNumber;

                        MeshFilter verticalFilter = GameObject.Instantiate<MeshFilter>(tilePool);
                        verticalFilter.transform.parent = mapRoot.transform.transform;
                        verticalFilter.transform.rotation = MapLoader.SurfaceDirection[direction];
                        verticalFilter.transform.position = new Vector3(col * FieldUnit.x + 0.5f, ((float)floor + 0.5f) * FieldUnit.y, -row * FieldUnit.z - 0.5f) + MapLoader.SurfaceOffset[direction];

                        if (!verticalMeshFilterDictionary.ContainsKey(verticalFieldId))
                            verticalMeshFilterDictionary.Add(verticalFieldId, new List<MeshFilter> { });

                        List<MeshFilter> verticalFilters = verticalMeshFilterDictionary[verticalFieldId];
                        verticalFilters.Add(verticalFilter);
                    }
                } while (false);
            }

            FieldCollider fieldCollider = GameObject.Instantiate<FieldCollider>(fieldColliderPrototype);
            float height = Mathf.Floor(map.fields[0].Length / width);
            fieldCollider.Init(width, height, floor);
            fieldCollider.transform.parent = mapRoot.transform;
            // TODO:
            fieldCollider.gameObject.SetActive(floor == 0);
        }


        {
            foreach (KeyValuePair<int, List<MeshFilter>> kv in meshFilterDictionary)
            {
                List<MeshFilter> filters = kv.Value;
                CombineInstance[] combine = new CombineInstance[filters.Count];
                for (int i = 0; i < filters.Count; i++)
                {
                    combine[i].mesh = filters[i].sharedMesh;
                    combine[i].transform = filters[i].transform.localToWorldMatrix;
                    combine[i].subMeshIndex = 0;
                    GameObject.Destroy(filters[i].gameObject);
                }

                FieldTile tile = GameObject.Instantiate<FieldTile>(fieldPrototype);
                tile.InitCombinedFloor(combine, kv.Key - 1);
                tile.transform.parent = mapRoot.transform;
            }

            foreach (KeyValuePair<int, List<MeshFilter>> kv in verticalMeshFilterDictionary)
            {
                List<MeshFilter> filters = kv.Value;
                CombineInstance[] combine = new CombineInstance[filters.Count];
                for (int i = 0; i < filters.Count; i++)
                {
                    combine[i].mesh = filters[i].sharedMesh;
                    combine[i].transform = filters[i].transform.localToWorldMatrix;
                    combine[i].subMeshIndex = 0;
                    GameObject.Destroy(filters[i].gameObject);
                }

                FieldTile tile = GameObject.Instantiate<FieldTile>(verticalPrototype);
                tile.InitCombinedFloor(combine, kv.Key - 1);
                tile.transform.parent = mapRoot.transform;

                // skip this if scene allows penetrate path finding
                MeshCollider collider = tile.GetComponent<MeshCollider>();
                collider.sharedMesh = tile.GetComponent<MeshFilter>().sharedMesh;
            }
        }

        return mapRoot;
    }

    private static GameObject InstantiateWalkerBehaviors(GameObject mapRoot, WalkerBehavior prototype)
    {
        Map map = MapLoader.currentMap;

        for (int i = 0; i < map.walkers.Length; i++)
        {
            Walker data = map.walkers[i];
            GameObject go = GameObject.Instantiate<GameObject>(prototype.gameObject);
            WalkerBehavior walker = go.GetComponent<WalkerBehavior>();
            walker.data = data;
            walker.transform.parent = mapRoot.transform;
            walker.transform.position = new Vector3(data.address % map.width + 0.5f, 0.5f, -Mathf.Floor(data.address / map.width) - 0.5f);

            go.SetActive(true);
        }

        return mapRoot;
    }
}

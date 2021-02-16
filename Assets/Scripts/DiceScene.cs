using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;

[DataContract]
struct TileData
{
    [DataMember]
    public string title;
    [DataMember]
    public string reference;
}

struct Tile
{
    public TileData data;
    public GameObject tile;

    public Tile(TileData data, GameObject tile)
    {
        this.data = data;
        this.tile = tile;
    }
}

public class DiceScene : MonoBehaviour
{
    public GameObject mapRoot = null;
    public FieldTile fieldPool = null;
    public MeshFilter tilePool = null;
    public FieldTile verticalFieldPool = null;
    public FieldCollider fieldCollierPool = null;
    public WalkerBehavior walkerPool = null;
    public MainCamera mainCamera = null;
    public Dice dice = null;
    public GameObject textPool = null;
    public GameObject ui = null;

    private Map map = new Map();
    private WalkerBehavior mainWalker = null;
    private List<Tile> questions = new List<Tile>();

    private List<int> route = new List<int> { };
    private Dictionary<int, Tile> texts = new Dictionary<int, Tile> { };
    private List<int> openedAddress = new List<int> { };
    private bool finishedDiceCalibration = false;

    private List<int> workQuestionIndexes = null;

    private void Start()
    {
        string path = Path.Combine("Data", "Tiles");
        TextAsset asset = Resources.Load<TextAsset>(path);
        if (asset == null)
        {
            Debug.Log("could not load tile data file: " + path);
            return;
        }
        
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(TileData[]));
        using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(asset.text)))
        {
            TileData[] tileData = (TileData[])serializer.ReadObject(ms);
            for (int i = 0; i < tileData.Length; i++)
                this.questions.Add(new Tile(tileData[i], null));
        }

        this.map = MapLoader.Load("Dice", this.mapRoot, this.tilePool, this.fieldPool, this.verticalFieldPool, this.fieldCollierPool, this.walkerPool, this.PutText);
        if (this.map.fields.Length == 0)
        {
            Debug.Log("could not load map");
            return;
        }

        WalkerBehavior[] walkers = this.mapRoot.GetComponentsInChildren<WalkerBehavior>();
        for (int i = 0; i < walkers.Length; i++)
        {
            WalkerBehavior walker = walkers[i];
            if (walker.data.cameraFollow)
            {
                this.mainWalker = walker;
                this.mainCamera.SetTarget(this.mainWalker.transform);
            }
            walker.SetLookAtCamera(this.mainCamera.GetComponent<Camera>());
        }

        int height = (int)Mathf.Floor(this.map.terrains[0].Length / this.map.width);

        for (int i = 0; i < this.map.width; i++)
            this.route.Add(i);
        for (int i = 1; i < height; i++)
            this.route.Add(this.map.width * i + this.map.width - 1);
        for (int i = this.map.width * height - 2; i >= this.map.width * (height - 1); i--)
            this.route.Add(i);
        for (int i = height - 2; i > 0; i--)
            this.route.Add(this.map.width * i);

        this.mainCamera.CameraMode = MainCamera.Mode.Static;

        this.dice.OnNumberDecided = this.TryWalkerWalk;
        this.mainWalker.OnStopped = this.OpenTile;

        RectTransform rt = this.ui.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(Screen.width, Screen.height);
    }

    public void OpenReference()
    {
        Tile td = this.texts[this.mainWalker.data.address];
        if (td.data.reference.Length > 0)
            Application.OpenURL(td.data.reference);
    }

    public void FinishWalkerFocus()
    {
        this.mainCamera.CameraMode = MainCamera.Mode.Static;
        
        TextMesh mesh = this.texts[this.mainWalker.data.address].tile.GetComponentInChildren<TextMesh>();
        if (mesh)
            mesh.gameObject.SetActive(false);

        this.ToggleUI(false);
    }

    public void OpenTile()
    {
        this.openedAddress.Add(this.mainWalker.data.address);
        this.ui.SetActive(true);
        Animator animator = this.texts[this.mainWalker.data.address].tile.GetComponentInChildren<Animator>();
        if (animator)
            animator.SetTrigger("Pop");
    }

    public void ToggleUI(bool enabled)
    {
        this.ui.SetActive(enabled);
    }

    private void TryWalkerWalk(int number)
    {
        // first time
        if (!this.finishedDiceCalibration)
        {
            this.finishedDiceCalibration = true;
            return;
        }
        if (this.ui.activeSelf)
            return;

        if (this.mainWalker.IsWalking())
            return;
        
        int currentAddress = this.mainWalker.data.address;
        int begin = this.route.IndexOf(currentAddress);

        List<int> path = new List<int>() { this.route[begin] };

        List<int> tmpRoute = new List<int>(this.route);
        // D6 max
        for (int i = 0; i < 6; i++)
            tmpRoute.AddRange(this.route);
            
        for (int i = 1; i <= number; i++)
        {
            int index = begin + i;
            int address = tmpRoute[index];

            if (this.openedAddress.IndexOf(address) != -1)
                number++;
                
            path.Add(address);

            if (number >= tmpRoute.Count)
            {
                Debug.Log("all tiles are opened !");
                return;
            }
        }
            
        List<DirectionUtil.Direction> directions = DirectionUtil.AddressesToDirections(path, this.map.width, this.map.height);
        this.mainWalker.AppendWalkDirections(directions);
        this.mainCamera.CameraMode = MainCamera.Mode.Follow;
    }

    private void PutText(Map map, MeshFilter filter, int col, int row)
    {
        GameObject text = GameObject.Instantiate(this.textPool);
        text.SetActive(true);
        text.transform.parent = this.mapRoot.transform;
        text.transform.position = new Vector3(0.5f + col, 0.2f, -0.5f - row - 0.3f);
        TextMesh mesh = text.GetComponentInChildren<TextMesh>();
        System.Random r = new System.Random();

        if (this.workQuestionIndexes == null)
        {
            this.workQuestionIndexes = new List<int>();
            for (int i = 0; i < this.questions.Count; i++)
                this.workQuestionIndexes.Add(i);
        }

        int index = r.Next(0, this.workQuestionIndexes.Count - 1);
        int questionIndex = this.workQuestionIndexes[index];
        Tile d = this.questions[questionIndex];
        mesh.text = d.data.title;
        this.workQuestionIndexes.RemoveAt(index);

        d.tile = text;
        this.texts.Add(map.width * row + col, d);
    }
}

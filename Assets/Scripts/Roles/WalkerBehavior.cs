using System.Collections.Generic;
using UnityEngine;

using LookedDirection = DirectionUtil.Direction;
using WalkerDirection = DirectionUtil.Direction;

public class AspectByWalkerDirection : Dictionary<WalkerDirection, Aspect> { }
public class WalkerDirectionByAspect : Dictionary<Aspect, WalkerDirection> { }

public enum WalkerType
{
    None = -1,
    Girl,
}

public enum Aspect
{
    Front = 0,
    Back,
    Right,
    Left,
    Count,
}

public class WalkerBehavior : MonoBehaviour
{
    public const int FramesPerAspect = 3;
    public const float WalkDistanceUnit = 1.0f;

    public static Dictionary<LookedDirection, AspectByWalkerDirection> AspectByLookedDirectionAndWalkerDirection = new Dictionary<LookedDirection, AspectByWalkerDirection>() {
        {
            LookedDirection.South, new AspectByWalkerDirection() {
                { WalkerDirection.South, Aspect.Front },
                { WalkerDirection.North, Aspect.Back },
                { WalkerDirection.East, Aspect.Right},
                { WalkerDirection.West, Aspect.Left}
            }
        },
        {
            LookedDirection.North,  new AspectByWalkerDirection() {
                { WalkerDirection.South, Aspect.Back },
                { WalkerDirection.North, Aspect.Front },
                { WalkerDirection.East, Aspect.Left },
                { WalkerDirection.West, Aspect.Right }
            }
        },
        {
            LookedDirection.East,  new AspectByWalkerDirection() {
                { WalkerDirection.South, Aspect.Left },
                { WalkerDirection.North, Aspect.Right},
                { WalkerDirection.East, Aspect.Front},
                { WalkerDirection.West, Aspect.Back }
            }
        },
        {
            LookedDirection.West,  new AspectByWalkerDirection() {
                { WalkerDirection.South, Aspect.Right },
                { WalkerDirection.North, Aspect.Left },
                { WalkerDirection.East, Aspect.Back },
                { WalkerDirection.West, Aspect.Front }
            }
        },
    };
    
    public static Dictionary<LookedDirection, WalkerDirectionByAspect> WalkerDirectionByLookedDirectionAndAspect = new Dictionary<LookedDirection, WalkerDirectionByAspect>() {
        {
            LookedDirection.South, new WalkerDirectionByAspect() {
                { Aspect.Front, WalkerDirection.South },
                { Aspect.Back, WalkerDirection.North},
                { Aspect.Right, WalkerDirection.East },
                { Aspect.Left, WalkerDirection.West }
            }
        },
        {
            LookedDirection.North,  new WalkerDirectionByAspect() {
                { Aspect.Front, WalkerDirection.North },
                { Aspect.Back, WalkerDirection.South },
                { Aspect.Right, WalkerDirection.West },
                { Aspect.Left, WalkerDirection.East }
            }
        },
        {
            LookedDirection.East,  new WalkerDirectionByAspect() {
                { Aspect.Front, WalkerDirection.East },
                { Aspect.Back, WalkerDirection.West },
                { Aspect.Right, WalkerDirection.North },
                { Aspect.Left, WalkerDirection.South }
            }
        },
        {
            LookedDirection.West,  new WalkerDirectionByAspect() {
                { Aspect.Front, WalkerDirection.West},
                { Aspect.Back, WalkerDirection.East },
                { Aspect.Right, WalkerDirection.South },
                { Aspect.Left, WalkerDirection.North }
            }
        },
    };

    public delegate void Stopped();
    public Stopped OnStopped = null;

    public GameObject surface = null;
    public GameObject shadow = null;
    public Walker data = Walker.Empty;
    public float walkSpeed = 1.0f;

    private Aspect aspect = Aspect.Front;
    private AtlasMaterial surfaceAtlasRef = null;
    private AtlasMaterial shadowAtlasRef = null;
    private Billboard surfaceBillboardRef = null;
    private List<WalkerDirection> walkDirectionQueue = new List<WalkerDirection>();
    private Vector3 walkFrom = Vector3.zero;
    private Vector3 walkVector = Vector3.zero;

    public bool IsWalking()
    {
        return this.walkVector != Vector3.zero;
    }

    public void SetLookAtCamera(Camera camera)
    {
        // TODO: dirty
        this.surfaceBillboardRef = this.surface.GetComponent<Billboard>();
        if (this.surfaceBillboardRef != null)
        {
            this.surfaceBillboardRef.targetCamera = camera;
        }
    }

    public LookedDirection LookedDirectionFromRelativePosition(Vector3 relPos)
    {
        if (Mathf.Abs(relPos.x) > Mathf.Abs(relPos.z))
        {
            if (relPos.x < 0)
                return LookedDirection.West;

            return LookedDirection.East;
        }

        if (relPos.z < 0)
            return LookedDirection.South;
        
        return LookedDirection.North;
    }

    public void AppendWalkDirections(List<WalkerDirection> directions)
    {
        this.walkDirectionQueue.AddRange(directions);
    }

    public void Turn(WalkerDirection dir)
    {
        this.data.direction = (int)dir;

        Aspect asp = this.NaturalAspect();
        this.ChangeAspect(asp);
    }

    public void ChangeAspect(Aspect asp)
    {
        this.aspect = asp;
        this.surfaceAtlasRef.initialIndex = (int)this.data.type * (int)Aspect.Count * WalkerBehavior.FramesPerAspect + WalkerBehavior.FramesPerAspect * (int)this.aspect;
        this.surfaceAtlasRef.maxIndex = this.surfaceAtlasRef.initialIndex + WalkerBehavior.FramesPerAspect - 1;
        this.shadowAtlasRef.initialIndex = (int)this.data.type * (int)Aspect.Count * WalkerBehavior.FramesPerAspect + WalkerBehavior.FramesPerAspect * (int)this.aspect;
        this.shadowAtlasRef.maxIndex = this.shadowAtlasRef.initialIndex + WalkerBehavior.FramesPerAspect - 1;
    }

    public Aspect NaturalAspect()
    {
        Vector3 cameraPos = this.surfaceBillboardRef.RelativeCameraPosition();
        WalkerDirection lookedDir = this.LookedDirectionFromRelativePosition(cameraPos);
        return AspectByLookedDirectionAndWalkerDirection[lookedDir][(WalkerDirection)this.data.direction];
    }

    private void Start()
    {
        if (this.surface == null)
        {
            this.enabled = false;
            return;
        }
        if (this.shadow == null)
        {
            this.enabled = false;
            return;
        }

        this.surfaceAtlasRef = this.surface.GetComponent<AtlasMaterial>();
        if (this.surfaceAtlasRef == null)
        {
            this.enabled = false;
            return;
        }

        this.shadowAtlasRef = this.shadow.GetComponent<AtlasMaterial>();
        if (this.shadowAtlasRef == null)
        {
            this.enabled = false;
            return;
        }

        this.surfaceBillboardRef = this.surface.GetComponent<Billboard>();
        if (this.surfaceBillboardRef == null)
        {
            this.enabled = false;
            return;
        }

        this.Turn(WalkerDirection.South);
    }

    private void Update()
    {
        this.UpdateWalkPosition(Time.deltaTime);

        this.UpdateWalkVector();

        Aspect asp = this.NaturalAspect();
        if (this.aspect != asp)
            this.ChangeAspect(asp);
    }

    private void UpdateWalkPosition(float dt)
    {
        if (!this.IsWalking())
            return;
        
        this.transform.position += this.walkVector * this.walkSpeed * dt;
        Vector3 dest = this.walkFrom + this.walkVector;
        float estimatedDistance = Vector3.Distance(this.walkFrom, dest);
        float totalDistance = Vector3.Distance(this.walkFrom, this.transform.position);
        if (totalDistance >= estimatedDistance)
        {
            this.transform.position = this.walkFrom + this.walkVector;
            this.walkVector = Vector3.zero;

            // TODO: consider is this really safe ?
            if (this.walkDirectionQueue.Count == 0)
                this.OnStopped?.Invoke();
        }
    }

    private void UpdateWalkVector()
    {
        if (this.walkDirectionQueue.Count == 0)
            return;

        if (this.IsWalking())
            return;

        this.walkFrom = this.transform.position;

        Map map = MapLoader.CurrentMap();
        DirectionUtil.Direction nextDirection = this.walkDirectionQueue[0];
        this.walkDirectionQueue.RemoveAt(0);

        Map currentMap = MapLoader.CurrentMap();
        
        switch (nextDirection)
        {
            case DirectionUtil.Direction.South: this.walkVector = new Vector3(0.0f, 0.0f, -WalkDistanceUnit); break;
            case DirectionUtil.Direction.North: this.walkVector = new Vector3(0.0f, 0.0f, WalkDistanceUnit);  break;
            case DirectionUtil.Direction.East:  this.walkVector = new Vector3(WalkDistanceUnit, 0.0f, 0.0f);  break;
            case DirectionUtil.Direction.West:  this.walkVector = new Vector3(-WalkDistanceUnit, 0.0f, 0.0f); break;
            default: return;
        }

        this.data.direction = (int)nextDirection;
        this.data.address = DirectionUtil.GetNeighborAddress(this.data.address, nextDirection, currentMap.width, currentMap.height);
    }
}

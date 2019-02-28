using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//Can be used for general purpose
public class FixedQueue<T>
{
    private int _size;
    public int Size
    {
        get { return _size; }
    }
    private List<T> _array;
    private int _headIdx;

    public FixedQueue(int size)
    {
        _array = new List<T>();
        for (int i = 0; i < size; i++)
        {
            _array.Add(default(T));
        }
        _headIdx = 0;
        _size = size;
    }

    public T Enqueue(T obj)
    {
        var ret = _array[_headIdx];
        _array[_headIdx++] = obj;
        _headIdx = _headIdx % _size;
        return ret;
    }

    public T Peek()
    {
        return _array[_headIdx];
    }

    public T this[int index]
    {
        get
        {
            if (index >= Size || index < -Size)
            {
                throw new System.Exception("Index Out Of Bound Exception(Class FixedQueue)");
            }
            return _array[(_headIdx + Size + index) % Size];
        }
        set
        {
            if (index >= Size || index < -Size)
            {
                throw new System.Exception("Index Out Of Bound Exception(Class FixedQueue)");
            }
            _array[(_headIdx + Size + index) % Size] = value;
        }
    }
}

public class APEVisualization : MonoBehaviour
{
    private static APEVisualization instance = null;
    public static APEVisualization Instance
    {
        get
        {
            return instance;
        }
    }

    //identity of each joint and its related visualization configurations
    [System.Serializable]
    public class VisualizeEntry
    {
        [System.Serializable]
        public struct PressureGraphProperty
        {
            [System.NonSerialized]
            public float IntrplValue, lastValue;
            [System.NonSerialized]
            public LineRenderer visualizeLine;
            [System.NonSerialized]
            public Text visualizeLineText;
            [System.NonSerialized]
            public FixedQueue<float> valueQueue;
            public Color visualizeColor;
            public Color VisualizeColor
            {
                get
                {
                    return visualizeColor;
                }

                set
                {
                    visualizeColor = value;
                }
            }
        }

        [System.Serializable]
        public struct PressureSpriteProperty
        {
            [System.NonSerialized]
            public float IntrplValue;
            [System.NonSerialized]
            public SpriteRenderer visualizeSprite;
        }

        [System.Serializable]
        public struct PressureMeshProperty
        {
            [System.NonSerialized]
            public float IntrplValue;
        }

        public bool visualize = true;

        [SerializeField]
        private tntChildLink targetLink;
        public tntChildLink TargetLink
        {
            get
            {
                return targetLink;
            }

            set
            {
                targetLink = value;
            }
        }

        public const int impulseHistorySize = 300;
        [System.NonSerialized]
        public FixedQueue<KeyValuePair<float, float>> impulseHistory = new FixedQueue<KeyValuePair<float, float>>(impulseHistorySize); //Fixed size of history queue. pair contains time stamp and the actual value

        public PressureGraphProperty graphProperty;
        public PressureSpriteProperty spriteProperty;
        public PressureMeshProperty meshProperpty;

        public VisualizeEntry() //Doesn't seem to work well with unity serialization system when initialized
        {
            if (impulseHistory == null)
            {
                impulseHistory = new FixedQueue<KeyValuePair<float, float>>(impulseHistorySize);
            }

            visualize = true;

            graphProperty = new PressureGraphProperty()
            {
                visualizeColor = Color.white,
            };
        }

        public float GetImpulseValue(float historyTime) //an interpolated value across history values. default size of 1 is as same as target value
        {
            float timeStamp = Time.time - historyTime;

            float ret = 0f;

            int size = 0;

            for (int i = 0; i < impulseHistory.Size; i++)
            {
                var index = -(i + 1);
                ret += impulseHistory[index].Value; //at least need one value sample
                size = i + 1;
                if (impulseHistory[index].Key < timeStamp)
                {
                    break;
                }
            }

            ret /= (float)size;

            return ret;
        }
    }

    //Settings for graph visualization
    [System.Serializable]
    public struct PressureGraphSettings
    {
        [System.NonSerialized]
        public APEVisualization parent;
        public float inputScaler;
        public float zeroLevelOffset;
        [System.NonSerialized]
        public bool inited;
        public float graphSpeed;
        [SerializeField]
        private bool showJointsLabel;
        public bool ShowJointsLabel //Should only be used to assign value when you want automate corresponding changes like when you change in inspector
        {
            get
            {
                return showJointsLabel;
            }

            set
            {
                showJointsLabel = value;
                parent.SyncEditorMemorableSet();
                parent.UpdateOnInspectorChange();
            }
        }
        [System.NonSerialized]
        public float lineHeadIndexOffset;

        [System.NonSerialized]
        public int graphResolution; //Resolution
                                    //[Space(5)]
        [Header("[Do Not Touch]")]
        public GameObject graphPanel;
        public Text globalDataDisplayText;
        public Transform graphLinesContainer;
        public LineRenderer sourceRefLine;
        public Text sourceRefLineText;
    }

    //Settings for sprites visualization
    [System.Serializable]
    public struct PressureSpritesSettings
    {
        [System.NonSerialized]
        public APEVisualization parent;
        public float inputScaler;
        public float analyzeHistoryTime; //how many history values will be interpolated for the final used impulse value
        [System.NonSerialized]
        public bool inited;
        public Color fullPressureColor;
        public Color noPressureColor;
        public float spriteSizeScaler;

        //[Space(5)]
        [Header("[Do Not Touch]")]
        public Transform spriteContainer;
        public SpriteRenderer sourceRefSprite;
        [System.NonSerialized]
        public Vector3 basicScale;
    }

    //Settings for on mesh visualization
    [System.Serializable]
    public struct PressureMeshSettings
    {
        [System.NonSerialized]
        public APEVisualization parent;
        public float inputScaler;
        public float analyzeHistoryTime; //how many history values will be interpolated for the final used impulse value
        public Color fullPressureColor;
        public float pressureFalloffRadius;
        [SerializeField]
        private bool useVisualizationBaseMesh;
        public bool UseVisualizationBaseMesh //Should only be used to assign value when you want automate corresponding changes like when you change in inspector
        {
            get
            {
                return useVisualizationBaseMesh;
            }
            set
            {
                useVisualizationBaseMesh = value;
                if (parent != null)
                {
                    parent.SyncEditorMemorableSet();
                    parent.UpdateOnInspectorChange();
                }
            }
        }
        public Color baseMeshColor;
        [System.NonSerialized]
        public bool inited;

        //[Space(5)]
        [Header("[Do Not Touch]")]
        public Transform meshContainer;
        [System.NonSerialized]
        public List<Renderer> originalRenderers;
        [System.NonSerialized]
        public List<Renderer> pressureRenderers;
        [System.NonSerialized]
        public List<Renderer> pressureBaseRenderers;
        public Material pressureMeshSourceMaterial;
        public Material pressureBaseMeshSourceMaterial; //TODO: put into shader pass
        public const int fixedAdjacentJointCount = 10; //Right now set to a maximum number, need to match the Joint Visualization shader's joint array size
        [System.NonSerialized]
        public Material sharedPressureBaseMaterialInstance;
        [System.NonSerialized]
        public Dictionary<tntLink, Material> jointVisMaterials; //A dictionary containing the 1 to 1 mapping between all tntlinks and their corresponding visualization materials
        [System.NonSerialized]
        public Dictionary<Material, List<Vector4>> jointVisMatAdjacentPoints; //A dictionary containing a key of a joint's visualization material and the key's corresponding adjacent joints' position and force value infos
    }

    //Designed to be used to monitor variable change state 
    public interface IEditorMemorable<T>
    {
        T GetValue();

        void SetValue(T value, bool preventChange = false);

        //Should be used only once for comparison purpose
        bool Changed(out T lastValue, out T value);
    }

    //For native c# type variable
    private struct EditorData<T> : IEditorMemorable<T>
    {
        private T _last;
        private T _value;

        public T GetValue()
        {
            return _value;
        }

        public void SetValue(T value, bool preventChange = false)
        {
            _value = value;
            if (preventChange)
            {
                _last = value;
            }
        }

        public bool Changed(out T lastValue, out T value)
        {
            lastValue = _last;
            value = _value;
            _last = _value;
            return !EqualityComparer<T>.Default.Equals(lastValue, _value);
        }
    }

    //For unity object type variable
    private struct EditorObject<T> : IEditorMemorable<T> where T : UnityEngine.Object
    {
        private T _last;
        private T _value;

        public T GetValue()
        {
            return _value;
        }

        public void SetValue(T value, bool preventChange = false)
        {
            _value = value;
            if (preventChange)
            {
                _last = value;
            }
        }

        public bool Changed(out T lastValue, out T value)
        {
            lastValue = _last;
            value = _value;
            _last = _value;
            return lastValue != _value;
        }
    }

    //A set for storing all designed memorable editor vars
    public class MemorableSet
    {
        public IEditorMemorable<bool> enablePressureGraph_ = new EditorData<bool>();
        public IEditorMemorable<bool> enablePressureSprites_ = new EditorData<bool>();
        public IEditorMemorable<bool> enablePressureMesh_ = new EditorData<bool>();
        public IEditorMemorable<GameObject> avatar_ = new EditorObject<GameObject>();
        public IEditorMemorable<int> entryCount_ = new EditorData<int>();
        public IEditorMemorable<bool> useVisualizationBaseMesh_ = new EditorData<bool>();
        public IEditorMemorable<bool> showJointsLabel_ = new EditorData<bool>();
        public IEditorMemorable<bool> loadAllJoints_ = new EditorData<bool>();
    }
    public MemorableSet m = new MemorableSet();

    [Header("Global Settings")]
    public GameObject avatar; //This should be the parent of avatar tntBase, but can be set to any of its children as well, correct gameobject will be inferred on start. //Not supporting real-time change for now
    public Camera canvasCam;
    private tntBase tntBase;
    private List<tntChildLink> tntChildLinks;
    public float valueScaler = 1.0f;

    [Header("[Do Not Touch]")]
    public Canvas visCanvas;
    private CanvasScaler visCanvasScaler;

    private HashSet<VisualizeEntry> entrySet; //remember entries information and used for comparison and synchronication

    [Header("Visualization Modes")]
    [SerializeField]
    private bool enablePressureGraph;
    public bool EnablePressureGraph //Should only be used to assign value when you want automate corresponding changes like when you change in inspector
    {
        get
        {
            return enablePressureGraph;
        }

        set
        {
            enablePressureGraph = value;
            SyncEditorMemorableSet();
            UpdateOnInspectorChange();
        }
    }

    [SerializeField]
    private bool enablePressureSprites;
    public bool EnablePressureSprites //Should only be used to assign value when you want automate corresponding changes like when you change in inspector
    {
        get
        {
            return enablePressureSprites;
        }

        set
        {
            enablePressureSprites = value;
            SyncEditorMemorableSet();
            UpdateOnInspectorChange();
        }
    }

    [SerializeField]
    private bool enablePressureMesh;
    public bool EnablePressureMesh //Should only be used to assign value when you want automate corresponding changes like when you change in inspector
    {
        get
        {
            return enablePressureMesh;
        }

        set
        {
            enablePressureMesh = value;
            SyncEditorMemorableSet();
            UpdateOnInspectorChange();
        }
    }

    public PressureGraphSettings pressureGraphSettings = new PressureGraphSettings()
    {
        inputScaler = 25.0f,
        inited = false,
        graphResolution = 350,
        graphSpeed = .3f,
        lineHeadIndexOffset = 0f,
    };

    public PressureSpritesSettings pressureSpritesSettings = new PressureSpritesSettings()
    {
        inputScaler = 0.5f,
        analyzeHistoryTime = 0.035f,
        inited = false,
        noPressureColor = Color.blue,
        fullPressureColor = Color.red,
        spriteSizeScaler = 1.0f,
        basicScale = Vector3.one,
    };

    public PressureMeshSettings pressureMeshSettings = new PressureMeshSettings()
    {
        inputScaler = 0.5f,
        analyzeHistoryTime = 0.035f,
        fullPressureColor = Color.red,
        pressureFalloffRadius = 0.25f,
        inited = false,
        pressureRenderers = new List<Renderer>(),
        pressureBaseRenderers = new List<Renderer>(),
        jointVisMaterials = new Dictionary<tntLink, Material>(),
        jointVisMatAdjacentPoints = new Dictionary<Material, List<Vector4>>(),
    };

    [Header("Joint Entries")]
    [SerializeField]
    private bool loadAllJoints;
    public bool LoadAllJoints //Should only be used to assign value when you want automate corresponding changes like when you change in inspector
    {
        get
        {
            return loadAllJoints;
        }

        set
        {
            loadAllJoints = value;
            SyncEditorMemorableSet();
            UpdateOnInspectorChange();
        }
    }

    [SerializeField]
    private List<VisualizeEntry> entries;

    bool inited;

    //Utils
    System.Text.StringBuilder sb1 = new System.Text.StringBuilder(50);
    System.Text.StringBuilder sb2 = new System.Text.StringBuilder(50);

    void OnValidate()
    {
        if (!Application.isPlaying || !inited)
            return;

        SyncEditorMemorableSet();

        UpdateOnInspectorChange();
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        pressureGraphSettings.parent = this;
        pressureSpritesSettings.parent = this;
        pressureMeshSettings.parent = this;

        if (avatar != null)
        {
            var tntBase = avatar.GetComponentInChildren<tntBase>();
            while (tntBase == null && avatar.transform.parent != null)
            {
                tntBase = avatar.transform.parent.GetComponentInChildren<tntBase>();
                avatar = avatar.transform.parent.gameObject;
            }
            if (tntBase != null)
            {
                avatar = tntBase.transform.parent.gameObject;
            }

            tntBase = avatar.GetComponentInChildren<tntBase>();
            tntChildLinks = avatar.GetComponentsInChildren<tntChildLink>().ToList<tntChildLink>();
        }

        if (visCanvas == null)
        {
            visCanvas = GetComponentInChildren<Canvas>();
        }
        if (visCanvas != null)
        {
            visCanvas.worldCamera = canvasCam != null ? canvasCam : Camera.main;
        }

        entrySet = new HashSet<VisualizeEntry>(entries);

        visCanvasScaler = visCanvas.GetComponent<CanvasScaler>();

        SyncEditorMemorableSet();
        m.entryCount_.SetValue(entries.Count, true); //This is important to prevent a duplicated initialization

        UpdateOnInspectorChange();

        inited = true;
    }

    void Update()
    {
        UpdateInputValues();

        if (EnablePressureGraph)
        {
            UpdatePressureGraph();
        }

        if (EnablePressureSprites)
        {
            UpdatePressureSprites();
        }

        if (EnablePressureMesh)
        {
            UpdatePressureMeshes();
        }
    }

    //Sync designed editor variable with normal editor fields so that state change can be detected uniformly later
    public void SyncEditorMemorableSet()
    {
        m.avatar_.SetValue(avatar);
        m.enablePressureGraph_.SetValue(EnablePressureGraph);
        m.enablePressureSprites_.SetValue(EnablePressureSprites);
        m.enablePressureMesh_.SetValue(EnablePressureMesh);
        m.useVisualizationBaseMesh_.SetValue(pressureMeshSettings.UseVisualizationBaseMesh);
        m.showJointsLabel_.SetValue(pressureGraphSettings.ShowJointsLabel);
        m.loadAllJoints_.SetValue(LoadAllJoints);
        m.entryCount_.SetValue(entries.Count);
    }

    public void UpdateOnInspectorChange()
    {
        {
            bool lastValue, value;
            if (m.enablePressureGraph_.Changed(out lastValue, out value))
            {
                var Do = value ? (System.Action)SetupPressureGraph : (System.Action)HidePressureGraph;
                Do();
            }
        }

        {
            bool lastValue, value;
            if (m.enablePressureSprites_.Changed(out lastValue, out value))
            {
                var Do = value ? (System.Action)SetupPressureSprites : (System.Action)HidePressureSprites;
                Do();
            }
        }

        {
            bool lastValue, value;
            if (m.enablePressureMesh_.Changed(out lastValue, out value))
            {
                var Do = value ? (System.Action)SetupPressureMeshes : (System.Action)HidePressureMeshes;
                Do();
            }
        }

        {
            bool lastValue, value;
            if (m.useVisualizationBaseMesh_.Changed(out lastValue, out value))
            {
                if (pressureMeshSettings.inited)
                {
                    var Do = value ? (System.Action)SwitchToVisualizationBaseMesh : (System.Action)SwitchToOriginalMesh;
                    Do();
                }
            }
        }

        {
            bool lastValue, value;
            if (m.showJointsLabel_.Changed(out lastValue, out value))
            {
                if (EnablePressureGraph)
                {
                    var Do = value ? (System.Action)ShowGraphJointNames : (System.Action)HideGraphJointNames;
                    Do();
                }
            }
        }

        {
            bool lastValue, value;
            if (m.loadAllJoints_.Changed(out lastValue, out value))
            {
                if (value)
                {
                    entries.Clear(); //Clear entry list

                    foreach (var cl in tntChildLinks)
                    {
                        var e = new VisualizeEntry()
                        {
                            visualize = true,
                            TargetLink = cl,
                        };
                        e.graphProperty.visualizeColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
                        entries.Add(e); //construct new entry list from all joints
                    }

                    UpdateUponEntryResize();

                    //prevent entry count difference to calculate the second time
                    m.entryCount_.SetValue(entries.Count, true);
                }
            }
        }

        {
            int lastValue, value;
            if (m.entryCount_.Changed(out lastValue, out value))
            {
                UpdateUponEntryResize();
            }
        }
    }

    void ShowGraphJointNames()
    {
        foreach (var e in entries)
        {
            e.graphProperty.visualizeLineText.gameObject.SetActive(true);
        }
    }

    void HideGraphJointNames()
    {
        foreach (var e in entries)
        {
            e.graphProperty.visualizeLineText.gameObject.SetActive(false);
        }
    }

    void SwitchToVisualizationBaseMesh()
    {
        //Hide original mesh renderers
        foreach (var rend in pressureMeshSettings.originalRenderers)
        {
            rend.enabled = false;
        }
        //Show visualization base mesh renderers
        foreach (var rend in pressureMeshSettings.pressureBaseRenderers)
        {
            rend.enabled = true;
        }
    }

    void SwitchToOriginalMesh()
    {
        //Show original mesh renderers
        foreach (var rend in pressureMeshSettings.originalRenderers)
        {
            rend.enabled = true;
        }
        //Hide visualization base mesh renderers
        foreach (var rend in pressureMeshSettings.pressureBaseRenderers)
        {
            rend.enabled = false;
        }
    }

    void UpdateUponEntryResize()
    {
        var newEntrySet = new HashSet<VisualizeEntry>(entries);

        foreach (var e in entrySet)
        {
            //Clear related entry assets
            if (e.graphProperty.visualizeLine != null)
            {
                Destroy(e.graphProperty.visualizeLine.gameObject);
            }

            if (e.spriteProperty.visualizeSprite != null)
            {
                Destroy(e.spriteProperty.visualizeSprite.gameObject);
            }
        }

        foreach (var e in newEntrySet)
        {
            if (pressureGraphSettings.inited)
            {
                //Add new entry assets
                SetupNewEntryForPressureGraph(e);
            }

            if (pressureSpritesSettings.inited)
            {
                //Add new entry assets
                SetupNewEntryForPressureSprites(e);
            }
        }

        entrySet = newEntrySet;
    }

    void SetupPressureGraph()
    {
        pressureGraphSettings.graphPanel.SetActive(true);
        pressureGraphSettings.graphLinesContainer.gameObject.SetActive(true);

        if (!pressureGraphSettings.inited)
        {
            pressureGraphSettings.inited = true;
            pressureGraphSettings.sourceRefLine.gameObject.SetActive(true);

            foreach (var e in entries)
            {
                SetupNewEntryForPressureGraph(e);

                for (int i = 0; i < pressureGraphSettings.graphResolution; i++)
                {
                    float x = MathUtils_FOP.Remap((float)i, 0f, (float)(pressureGraphSettings.graphResolution - 1), -visCanvasScaler.referenceResolution.x / 2, visCanvasScaler.referenceResolution.x / 2);
                    e.graphProperty.visualizeLine.SetPosition(i, new Vector3(x, i % 2 == 0 ? 100f : -100f, 0f)); //An exaggerated shape that is easier to see line resolution. Disable update functions to see this for resolution
                }
            }

            pressureGraphSettings.sourceRefLine.gameObject.SetActive(false); //Disable sample line
        }
        else
        {
            foreach (var e in entries)
            {
                e.graphProperty.visualizeLineText.gameObject.SetActive(pressureGraphSettings.ShowJointsLabel);
            }
        }
    }

    void HidePressureGraph()
    {
        pressureGraphSettings.graphPanel.SetActive(false);
        pressureGraphSettings.graphLinesContainer.gameObject.SetActive(false);
    }

    void SetupNewEntryForPressureGraph(VisualizeEntry e)
    {
        var lineGo = Instantiate<GameObject>(pressureGraphSettings.sourceRefLine.gameObject, pressureGraphSettings.graphLinesContainer, false);
		sb1.Length = 0;
        lineGo.name = sb1.Append("Line-").Append(e.TargetLink == null ? "None" : e.TargetLink.gameObject.name).ToString();
        lineGo.SetActive(true);

        //Delete the extra line text copy
        foreach (var t in lineGo.GetComponentsInChildren<Text>())
        {
            Destroy(t.gameObject);
        }

        var textGo = Instantiate<GameObject>(pressureGraphSettings.sourceRefLineText.gameObject, lineGo.transform, false);
        textGo.SetActive(true);

        e.graphProperty.visualizeLine = lineGo.GetComponent<LineRenderer>();
        e.graphProperty.visualizeLine.sortingLayerName = "UI Overlay";
        e.graphProperty.visualizeLine.positionCount = pressureGraphSettings.graphResolution;
        e.graphProperty.visualizeLine.SetPositions(new Vector3[pressureGraphSettings.graphResolution]);
        e.graphProperty.valueQueue = new FixedQueue<float>(pressureGraphSettings.graphResolution + 1);

        e.graphProperty.visualizeLineText = textGo.GetComponent<Text>();
        e.graphProperty.visualizeLineText.color = e.graphProperty.visualizeColor;

        for (int i = 0; i < pressureGraphSettings.graphResolution; i++)
        {
            float x = MathUtils_FOP.Remap((float)i, 0f, (float)(pressureGraphSettings.graphResolution - 1), -visCanvasScaler.referenceResolution.x / 2, visCanvasScaler.referenceResolution.x / 2);
            e.graphProperty.visualizeLine.SetPosition(i, new Vector3(x, 0f, 0f)); //An initial shape
        }

        e.graphProperty.visualizeLine.gameObject.SetActive(e.visualize && e.TargetLink != null); //If joint is null or not set to visualize, disable the line
        e.graphProperty.visualizeLineText.gameObject.SetActive(pressureGraphSettings.ShowJointsLabel);
    }

    void SetupPressureSprites()
    {
        pressureSpritesSettings.spriteContainer.gameObject.SetActive(true);
        if (!pressureSpritesSettings.inited)
        {
            pressureSpritesSettings.inited = true;
            pressureSpritesSettings.sourceRefSprite.gameObject.SetActive(true);
            foreach (var e in entries)
            {
                SetupNewEntryForPressureSprites(e);
            }

            pressureSpritesSettings.sourceRefSprite.gameObject.SetActive(false); //Disable sample sprite
        }
    }

    void HidePressureSprites()
    {
        pressureSpritesSettings.spriteContainer.gameObject.SetActive(false);
    }

    void SetupNewEntryForPressureSprites(VisualizeEntry e)
    {
        var go = Instantiate<GameObject>(pressureSpritesSettings.sourceRefSprite.gameObject, pressureSpritesSettings.spriteContainer, false);
		sb1.Length = 0;
        go.name = sb1.Append("Sprite-").Append(e.TargetLink == null ? "None" : e.TargetLink.gameObject.name).ToString();
        go.SetActive(true);
        pressureSpritesSettings.basicScale = pressureSpritesSettings.sourceRefSprite.transform.localScale;

        e.spriteProperty.visualizeSprite = go.GetComponent<SpriteRenderer>();
        e.spriteProperty.visualizeSprite.sortingLayerName = "UI Overlay";
        e.spriteProperty.visualizeSprite.gameObject.SetActive(e.visualize && e.TargetLink != null); //If bool is not checked, disable the line
    }

    void SetupPressureMeshes()
    {
        pressureMeshSettings.meshContainer.gameObject.SetActive(true);

        if (!pressureMeshSettings.inited)
        {
            pressureMeshSettings.inited = true;

            //Assume only meshrenderers and renderer needs to have tntchildlink or tntbase as parent
            pressureMeshSettings.originalRenderers = avatar.GetComponentsInChildren<MeshRenderer>().ToList<Renderer>();
            pressureMeshSettings.originalRenderers = pressureMeshSettings.originalRenderers.FindAll(r => r.GetComponentInParent<tntChildLink>() != null || r.GetComponentInParent<tntBase>() != null);

            pressureMeshSettings.pressureRenderers.Clear();
            pressureMeshSettings.pressureBaseRenderers.Clear();            
            pressureMeshSettings.jointVisMaterials.Clear();
            pressureMeshSettings.sharedPressureBaseMaterialInstance = new Material(pressureMeshSettings.pressureBaseMeshSourceMaterial);

            foreach (var rend in pressureMeshSettings.originalRenderers)
            {
                if (rend.enabled && rend.gameObject.activeInHierarchy)
                {
                    //visualization mat
                    {
                        GameObject rendGo = (GameObject)Instantiate(rend.gameObject);
                        rendGo.name = rend.gameObject.name + "_VisualizationMesh";
                        rendGo.transform.SetParent(pressureMeshSettings.meshContainer); // collect all meshes in one container
                        Component[] components = rendGo.GetComponents(typeof(Component));
                        foreach (Component c in components)
                        {
                            if (!(typeof(Transform).IsAssignableFrom(c.GetType()) || typeof(Renderer).IsAssignableFrom(c.GetType()) || typeof(MeshFilter).IsAssignableFrom(c.GetType())))
                            {
                                DestroyImmediate(c);
                            }
                        }

                        var r = rendGo.GetComponent<Renderer>();

                        //mesh renderers under the same tntlink will share a unique material instance
                        var tLink = rend.GetComponentInParent<tntLink>();
                        if (pressureMeshSettings.jointVisMaterials.ContainsKey(tLink))
                        {
                            r.material = pressureMeshSettings.jointVisMaterials[tLink];
                        }
                        else
                        {
                            r.material = new Material(pressureMeshSettings.pressureMeshSourceMaterial);
                            pressureMeshSettings.jointVisMaterials.Add(tLink, r.material);
                            pressureMeshSettings.jointVisMatAdjacentPoints.Add(r.material, new List<Vector4>(PressureMeshSettings.fixedAdjacentJointCount));
                        }                      

                        var constraint = rendGo.AddComponent<TransformConstraint>();
                        constraint.SetConstrainSource(rend.gameObject);
                        pressureMeshSettings.pressureRenderers.Add(r);
                    }

                    //base visualization mat
                    {
                        GameObject rendGo = (GameObject)Instantiate(rend.gameObject);
                        rendGo.name = rend.gameObject.name + "_BaseMesh";
                        rendGo.transform.SetParent(pressureMeshSettings.meshContainer); // collect all meshes in one container
                        Component[] components = rendGo.GetComponents(typeof(Component));
                        foreach (Component c in components)
                        {
                            if (!(typeof(Transform).IsAssignableFrom(c.GetType()) || typeof(Renderer).IsAssignableFrom(c.GetType()) || typeof(MeshFilter).IsAssignableFrom(c.GetType())))
                            {
                                DestroyImmediate(c);
                            }
                        }
                        var r = rendGo.GetComponent<Renderer>();
                        r.material = pressureMeshSettings.sharedPressureBaseMaterialInstance;
                        pressureMeshSettings.sharedPressureBaseMaterialInstance.color = pressureMeshSettings.baseMeshColor;
                        var constraint = rendGo.AddComponent<TransformConstraint>();
                        constraint.SetConstrainSource(rend.gameObject);
                        pressureMeshSettings.pressureBaseRenderers.Add(r);
                    }
                }
            }

            if (pressureMeshSettings.UseVisualizationBaseMesh)
            {
                //Hide original mesh renderers
                foreach (var rend in pressureMeshSettings.originalRenderers)
                {
                    rend.enabled = false;
                }
            }
            else
            {
                //Hide visualization base mesh renderers
                foreach (var rend in pressureMeshSettings.pressureBaseRenderers)
                {
                    rend.enabled = false;
                }
            }
        }

        if (pressureMeshSettings.UseVisualizationBaseMesh)
        {
            foreach (var r in pressureMeshSettings.originalRenderers)
            {
                r.enabled = false;
            }
        }
    }

    void HidePressureMeshes()
    {
        pressureMeshSettings.meshContainer.gameObject.SetActive(false);

        //ensure original meshes are shown
        foreach (var r in pressureMeshSettings.originalRenderers)
        {
            r.enabled = true;
        }
    }

    void UpdateInputValues()
    {
        float max = 0;
        float avg = 0;
        foreach (var e in entries)
        {
            float inputValue;
            if (e.TargetLink != null)
            {
                inputValue = e.TargetLink.m_feedback.m_appliedImpulse; //sync the joint impulse value                
            }
            else
            {
                inputValue = 0f;
            }
            
            e.impulseHistory.Enqueue(new KeyValuePair<float, float>(Time.time, inputValue)); //record time stamped value

            e.graphProperty.IntrplValue = inputValue; //graph doesn't seem to require visual smoothing
            e.spriteProperty.IntrplValue = e.GetImpulseValue(pressureSpritesSettings.analyzeHistoryTime);
            e.meshProperpty.IntrplValue = e.GetImpulseValue(pressureMeshSettings.analyzeHistoryTime);

            //track max and avg value displayed in graph panel
            if (inputValue > max)            
                max = inputValue;
            
            avg += inputValue;
        }

        if (avg != 0)        
            avg /= entries.Count;        

        if (pressureGraphSettings.globalDataDisplayText != null)
        {
            //update global display data text
			sb1.Length = 0;
            pressureGraphSettings.globalDataDisplayText.text = sb1.Append("Max: ").Append((max * pressureGraphSettings.inputScaler).ToString("000.00")).Append(" | Avg: ").Append((avg * pressureGraphSettings.inputScaler).ToString("000.00")).ToString();
        }
    }

    void UpdatePressureGraph()
    {
        //Update line points value
        foreach (var e in entries)
        {
            var visualizeLine = e.graphProperty.visualizeLine;
            if (e.TargetLink != null && e.visualize)
            {
                if (!visualizeLine.gameObject.activeSelf)
                {
                    visualizeLine.gameObject.SetActive(true);/* Better make it change upon need */
                }
            }
            else
            {
                if (visualizeLine.gameObject.activeSelf)
                {
                    visualizeLine.gameObject.SetActive(false);/* Better make it change upon need */
                }
            }

			sb2.Length = 0;
            sb2.Append(e.TargetLink == null ? "None" : e.TargetLink.gameObject.name);

            //Lerp line points to real value queue data
            for (int i = 0; i < pressureGraphSettings.graphResolution; i++)
            {
                Vector3 vec = visualizeLine.GetPosition(i);
                visualizeLine.material.color = e.graphProperty.visualizeColor;
                visualizeLine.SetPosition(i, new Vector3(vec.x, Mathf.Lerp(e.graphProperty.valueQueue[i], e.graphProperty.valueQueue[i + 1], pressureGraphSettings.lineHeadIndexOffset) * pressureGraphSettings.inputScaler * valueScaler + pressureGraphSettings.zeroLevelOffset, vec.z));
				sb1.Length = 0;;
                visualizeLine.gameObject.name = sb1.Append("Line-").Append(sb2).ToString(); /* Better make it change upon need */

            }

            var visualizeLineText = e.graphProperty.visualizeLineText;

            if (pressureGraphSettings.ShowJointsLabel)
            {
                //Update line text position
                visualizeLineText.transform.position = pressureGraphSettings.graphLinesContainer.TransformPoint(visualizeLine.GetPosition(pressureGraphSettings.graphResolution - 1) + Vector3.up * 12.5f); //assign position in world position space
                visualizeLineText.color = e.graphProperty.visualizeColor;
                visualizeLineText.text = sb2.ToString(); /* Better make it change upon need */
				sb1.Length = 0;
                visualizeLineText.gameObject.name = sb1.Append("Text-").Append(sb2).ToString(); /* Better make it change upon need */
            }
        }

        MoveGraph(); //Move the data that you can see in graph

        foreach (var e in entries)
        {
            e.graphProperty.lastValue = e.graphProperty.IntrplValue; //Record to last value
        }
    }

    void UpdatePressureSprites()
    {
        //Update sprites render status, size and color value
        var adjustedSpriteScale = pressureSpritesSettings.basicScale * pressureSpritesSettings.spriteSizeScaler;

        foreach (var e in entries)
        {
            if (e.TargetLink != null && e.visualize)
            {
                //Update sprite impulse color
                e.spriteProperty.visualizeSprite.color = Color.Lerp(
                    pressureSpritesSettings.noPressureColor,
                    pressureSpritesSettings.fullPressureColor,
                    e.spriteProperty.IntrplValue * pressureSpritesSettings.inputScaler * valueScaler
                    );

                //Update sprite movement
                e.spriteProperty.visualizeSprite.transform.position = e.TargetLink.PivotAToWorld();
                e.spriteProperty.visualizeSprite.transform.LookAt(canvasCam.transform);

                if (!e.spriteProperty.visualizeSprite.gameObject.activeSelf)
                {
                    e.spriteProperty.visualizeSprite.gameObject.SetActive(true);/* Better make it change upon need */
                }

                e.spriteProperty.visualizeSprite.transform.localScale = adjustedSpriteScale;/* Better make it change upon need */
            }
            else
            {
                if (e.spriteProperty.visualizeSprite.gameObject.activeSelf)
                {
                    e.spriteProperty.visualizeSprite.gameObject.SetActive(false);/* Better make it change upon need */
                }
            }
        }
    }

    void UpdatePressureMeshes()
    {        
        //update global variables
        Shader.SetGlobalColor("_psColor", pressureMeshSettings.fullPressureColor);
        Shader.SetGlobalFloat("_psRadius", pressureMeshSettings.pressureFalloffRadius);

        //Clear Material-JointInfos dictionary list's joint infos that are going to be passed into shader
        foreach (var pair in pressureMeshSettings.jointVisMatAdjacentPoints)
        {
            var vec4List = pair.Value;
            if (vec4List != null)
            {
                vec4List.Clear();
            }
            else
            {
                vec4List = new List<Vector4>();
            }
        }

        //Assign actual meaningful values for those joint that is valid and set to visualize
        foreach (var e in entries)
        {
            var childLink = e.TargetLink;
            if (childLink != null && e.visualize)
            {
                 var linkShaderVec4 = new Vector4(
                    childLink.PivotAToWorld().x,
                    childLink.PivotAToWorld().y,
                    childLink.PivotAToWorld().z,
                    e.meshProperpty.IntrplValue * pressureMeshSettings.inputScaler * valueScaler
                    );

                if (pressureMeshSettings.jointVisMaterials.ContainsKey(childLink))
                {
                    pressureMeshSettings.jointVisMatAdjacentPoints[pressureMeshSettings.jointVisMaterials[childLink]].Add(linkShaderVec4);
                }


                if (childLink.m_parent != null && pressureMeshSettings.jointVisMaterials.ContainsKey(childLink.m_parent))
                {
                    pressureMeshSettings.jointVisMatAdjacentPoints[pressureMeshSettings.jointVisMaterials[childLink.m_parent]].Add(linkShaderVec4);
                }
            }
        }

        //Preparing and passing data into materials
        foreach (var pair in pressureMeshSettings.jointVisMaterials)
        {
            var jntMat = pair.Value;
            var vec4List = pressureMeshSettings.jointVisMatAdjacentPoints[jntMat];

            //Make up missing array elements with zeros to match shader joint array size
            if (vec4List.Count < PressureMeshSettings.fixedAdjacentJointCount)
            {
                int makeupIters = PressureMeshSettings.fixedAdjacentJointCount - vec4List.Count;
                for (int i = 0; i < makeupIters; i++)
                {
                    vec4List.Add(Vector4.zero);
                }
            }

            //Assign the joint visualization array
            jntMat.SetVectorArray("_psJoints", vec4List);
        }        

        if (pressureMeshSettings.UseVisualizationBaseMesh)
        {
            pressureMeshSettings.sharedPressureBaseMaterialInstance.color = pressureMeshSettings.baseMeshColor; //Update grayscale base mesh color
        }
    }

    public void MoveGraph()
    {
        //Update head index offset
        pressureGraphSettings.lineHeadIndexOffset += pressureGraphSettings.graphSpeed * Time.deltaTime * (float)pressureGraphSettings.graphResolution;

        if (pressureGraphSettings.lineHeadIndexOffset > 1.0f)
        {
            int overflowAmount = (int)(pressureGraphSettings.lineHeadIndexOffset / 1.0f);
            pressureGraphSettings.lineHeadIndexOffset %= 1.0f;

            for (int i = 0; i < overflowAmount; i++)
            {
                if (overflowAmount > 1) //When line moves too fast
                {
                    foreach (var e in entries)
                    {
                        var value = Mathf.Lerp(e.graphProperty.lastValue, e.graphProperty.IntrplValue, (float)i / (float)(overflowAmount - 1)); // Need some extra smoothing to erase stepping effects
                        e.graphProperty.valueQueue.Enqueue(value);
                    }
                }
                else
                {
                    foreach (var e in entries)
                    {
                        e.graphProperty.valueQueue.Enqueue(e.graphProperty.IntrplValue);
                    }
                }
            }
        }
    }
}

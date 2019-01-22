using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ItSeez3D.AvatarSdk.Cloud;
using ItSeez3D.AvatarSdk.Core;
using ItSeez3D.AvatarSdkSamples.Core;
using System.Collections;
using System.Linq;
using System.IO;

public class AvatarTest : GallerySample {

    private int currentPage = 1;
    private Dictionary<string, AvatarPreview> avatarPreviews = new Dictionary<string, AvatarPreview>();
    private bool avatarsAdded = false;

    void Start() {
        StartCoroutine(Initialize());
        SetPipelineToggleValue(pipelineType);
        if(fileBrowser != null)
            fileBrowser.fileHandler = CreateNewAvatar;
        generateFromUserPhoto.gameObject.SetActive(true);
        //UpdateAvatarList();
    }

    void Update() {
        if (avatarsAdded == false && loadedAvatars != null) {
            //UpdateAvatarList();
            iterateDictionary();
            avatarsAdded = true;
        }

        //iterateDictionary();
        /*if(loadedAvatars != null) {
            print(loadedAvatars.Length);
        }*/
    }

    public void iterateDictionary() {
        print("Attempting to iterate the dictionary..");
        int count = 0;
        
        foreach(KeyValuePair<string, AvatarPreview> preview in avatarPreviews) {
            if(count == 0) {
                print("Preview:" + preview);
                ShowAvatar(preview.Key);
                //OnShowAvatar(preview.Key);
                //CreateAvatarPrefab();
            }
            count++;
        }
    }

    protected class GalleryAvatarCloud : GalleryAvatar {
        public AvatarData avatarData;
    }

    public AvatarTest() {
        sdkType = SdkType.Cloud;
    }

    private IEnumerator CreateNewAvatar(byte[] photoBytes) {
        PipelineType pipeline = pipelineType;

        // Choose default set of resources to generate
        var resourcesRequest = avatarProvider.ResourceManager.GetResourcesAsync(AvatarResourcesSubset.DEFAULT, pipelineType);
        yield return resourcesRequest;
        if(resourcesRequest.IsError)
            yield break;

        var initializeAvatar = avatarProvider.InitializeAvatarAsync(photoBytes, "name", "description", pipeline, resourcesRequest.Result);
        yield return Await(initializeAvatar, null);

        string avatarCode = initializeAvatar.Result;
        if(initializeAvatar.IsError) {
            UpdateAvatarState(avatarCode, GalleryAvatarState.FAILED);
            yield break;
        }

        yield return UpdateAvatarList();
        UpdateAvatarState(avatarCode, GalleryAvatarState.GENERATING);

        var calculateAvatar = avatarProvider.StartAndAwaitAvatarCalculationAsync(avatarCode);
        yield return Await(calculateAvatar, avatarCode);
        if(calculateAvatar.IsError) {
            UpdateAvatarState(avatarCode, GalleryAvatarState.FAILED);
            yield break;
        }

        var downloadAvatar = avatarProvider.MoveAvatarModelToLocalStorageAsync(avatarCode, pipeline == PipelineType.FACE, true);
        yield return Await(downloadAvatar, avatarCode);
        if(downloadAvatar.IsError) {
            UpdateAvatarState(avatarCode, GalleryAvatarState.FAILED);
            yield break;
        }

        UpdateAvatarState(avatarCode, GalleryAvatarState.COMPLETED);
    }

    protected override AsyncRequest<GalleryAvatar[]> GetAllAvatarsAsync(int maxItems) {
        var request = new AsyncRequest<GalleryAvatar[]>(AvatarSdkMgr.Str(Strings.GettingAvatarState));
        AvatarSdkMgr.SpawnCoroutine(GetAllAvatarsFunc(maxItems, request));
        return request;
    }

    private IEnumerator GetAllAvatarsFunc(int maxItems, AsyncRequest<GalleryAvatar[]> request) {
        Connection connection = (avatarProvider as CloudAvatarProvider).Connection;
        var avatarsRequest = connection.GetAvatarsAsync(maxItems);
        yield return Await(avatarsRequest, null);
        if(avatarsRequest.IsError)
            yield break;

        GalleryAvatar[] avatars = new GalleryAvatar[avatarsRequest.Result.Length];
        for(int i = 0; i < avatars.Length; i++) {
            AvatarData avatarData = avatarsRequest.Result[i];
            avatars[i] = new GalleryAvatarCloud() { code = avatarData.code, state = GetAvatarState(avatarData), avatarData = avatarData };
        }

        request.Result = avatars;
        request.IsDone = true;
    }

    private GalleryAvatarState GetAvatarState(AvatarData avatarData) {
        GalleryAvatarState avatarState = GalleryAvatarState.UNKNOWN;
        // check if calculation failed on the server
        if(Strings.BadFinalStates.Contains(avatarData.status))
            avatarState = GalleryAvatarState.FAILED;
        else {
            if(Strings.GoodFinalStates.Contains(avatarData.status))
                avatarState = GalleryAvatarState.COMPLETED;
            else
                // not failed server status, but not completed either - this means avatar is still on the server
                avatarState = GalleryAvatarState.GENERATING;
        }
        return avatarState;
    }

    protected IEnumerator UpdateAvatarList() {
        Debug.LogFormat("Updating avatar list...");

        // For this sample we basically get all avatars created by the current player (but no more than a 1000,
        // just in case). Then pagination is done locally.
        // This should be all right for almost all practical situations. However if this is not suitable for your app
        // you can implement custom pagination logic using the low-level Connection API.
        const int maxAvatars = 1000;
        var avatarsRequest = GetAllAvatarsAsync(maxAvatars);
        yield return Await(avatarsRequest, null);
        if(avatarsRequest.IsError)
            yield break;

        loadedAvatars = avatarsRequest.Result;

        // If some avatars were deleted on the server we might need to return to the previous page in case the
        // current page is empty.
        while(currentPage > 1) {
            var pageAvatars = GetAvatarIdsForPage(currentPage);
            if(pageAvatars == null || pageAvatars.Length == 0) {
                --currentPage;
                continue;
            } else
                break;
        }

        // display current page using updated list of avatars
        ShowPage(currentPage);
    }

    private void ShowPage(int newPage) {
        var avatarsForPage = GetAvatarIdsForPage(newPage);
        if(avatarsForPage == null)
            return;

        if(avatarsForPage.Length == 0 && newPage > currentPage) {
            Debug.LogFormat("Next page is empty, ignore...");
            return;
        }

        UpdatePage(avatarsForPage);
        currentPage = newPage;
        //currentPageText.text = currentPage.ToString();
    }

    public void OnPrevPage() {
        ShowPage(currentPage - 1);
    }

    public void OnNextPage() {
        ShowPage(currentPage + 1);
    }



    private void UpdatePage(string[] pageAvatarIds) {
        // first - clean current previews, memory starts to leak if we don't do this
        foreach(var child in avatarsContainer.GetComponentsInChildren<AvatarPreview>()) {
            child.CleanUp();
            Destroy(child.gameObject);
        }
        avatarPreviews.Clear();

        for(int i = 0; i < pageAvatarIds.Length; ++i) {
            var avatarPreview = GameObject.Instantiate(avatarPrefab);
            avatarPreview.transform.localScale = avatarsContainer.transform.lossyScale;
            avatarPreview.transform.SetParent(avatarsContainer.transform);
            var preview = avatarPreview.GetComponent<AvatarPreview>();
            var avatarCode = pageAvatarIds[i];
            avatarPreviews[avatarCode] = preview;

            var avatar = GetAvatar(avatarCode);
            UpdateAvatarState(avatarCode, avatar.state);
            InitAvatarPreview(preview, pageAvatarIds[i], avatar.state);
        }
    }

    private GalleryAvatar GetAvatar(string avatarCode) {
        return loadedAvatars.FirstOrDefault(a => string.Compare(a.code, avatarCode) == 0);
    }


    private void UpdateAvatarState(string avatarCode, GalleryAvatarState state) {
        var avatar = GetAvatar(avatarCode);
        avatar.state = state;
        UpdateAvatarPreview(avatarCode, state);
    }

    private void UpdateAvatarPreview(string avatarCode, GalleryAvatarState state) {
        if(!avatarPreviews.ContainsKey(avatarCode))
            return;

        var preview = avatarPreviews[avatarCode];
        preview.UpdatePreview(avatarCode, state);
    }

    private string[] GetAvatarIdsForPage(int pageIdx) {
        if(loadedAvatars == null)
            return null;
        if(pageIdx < 1)
            return null;

        var panelW = avatarsContainer.GetComponent<RectTransform>().rect.width;
        var avatarW = avatarPrefab.GetComponent<RectTransform>().rect.width;
        var padding = 10;
        int numAvatarsPerPage = (int)(panelW / (avatarW + padding));
        int startIdx = (pageIdx - 1) * numAvatarsPerPage;

        var pageAvatars = new List<string>();
        for(int i = startIdx; i < loadedAvatars.Length && i < startIdx + numAvatarsPerPage; ++i)
            pageAvatars.Add(loadedAvatars[i].code);

        return pageAvatars.ToArray();
    }

    private IEnumerator Initialize() {
        // First of all, initialize the SDK. This sample shows how to provide user-defined implementations for
        // the interfaces if needed. If you don't need to override the default behavior, just pass null instead.
        if(!AvatarSdkMgr.IsInitialized) {
            AvatarSdkMgr.Init(
                stringMgr: stringManager,
                storage: persistentStorage,
                sdkType: sdkType
            );
        }

        GameObject providerContainerGameObject = GameObject.Find("AvatarProviderContainer");
        if(providerContainerGameObject != null) {
            avatarProvider = providerContainerGameObject.GetComponent<AvatarProviderContainer>().avatarProvider;
        } else {
            // Initialization of the IAvatarProvider may take some time. 
            // We don't want to initialize it each time when the Gallery scene is loaded.
            // So we store IAvatarProvider instance in the object that will not destroyed during navigation between the scenes (Gallery -> ModelViewer -> Gallery).
            providerContainerGameObject = new GameObject("AvatarProviderContainer");
            DontDestroyOnLoad(providerContainerGameObject);
            AvatarProviderContainer providerContainer = providerContainerGameObject.AddComponent<AvatarProviderContainer>();
            avatarProvider = AvatarSdkMgr.IoCContainer.Create<IAvatarProvider>();
            providerContainer.avatarProvider = avatarProvider;

            var initializeRequest = InitializeAvatarProviderAsync();
            yield return Await(initializeRequest, null);
            if(initializeRequest.IsError) {
                Debug.LogError("Avatar provider isn't initialized!");
                yield break;
            }
        }

        yield return UpdateAvatarList();

        // disable generation buttons until avatar provider initializes
        /*foreach(var button in galleryControls.GetComponentsInChildren<Button>(false))
            if(button.name.Contains("Generate"))
                button.interactable = true;*/
    }

    private const string BALD_HAIRCUT_NAME = "bald";
    private const string HEAD_OBJECT_NAME = "ItSeez3D Head";
    private const string HAIRCUT_OBJECT_NAME = "ItSeez3D Haircut";
    private const string AVATAR_OBJECT_NAME = "ItSeez3D Avatar";
    private Vector3 faceAvatarScale = new Vector3(10, 10, 10);
    private Vector3 headAvatarScale = new Vector3(7, 7, 7);

    private int currentHaircut = 0;
    private GameObject headObject = null;

    public bool IsUnlitMode {
        get;
        private set;
    }

    // Flag indicates if we need to render several models by using GPU instantiating.
    public bool IsInstantiatingMode {
        get;
        private set;
    }

    public GameObject HaircutObject {
        get {
            var haircutObj = GameObject.Find(HAIRCUT_OBJECT_NAME);
            return haircutObj;
        }
    }


    // Material uses unlit shader
    public Material unlitMaterial;

    // Material uses lit shader
    public Material litMaterial;

    // Unlit material for haircuts
    public Material haircutUnlitMaterial;

    // Lit material for haircuts
    public Material haircutLitMaterial;

    private void SetAvatarScale(string avatarCode, Transform avatarTransform) {
        Vector3 scale = faceAvatarScale;
        string pipelineInfoFile = AvatarSdkMgr.Storage().GetAvatarFilename(avatarCode, AvatarFile.PIPELINE_INFO);
        if(File.Exists(pipelineInfoFile)) {
            string fileContent = File.ReadAllText(pipelineInfoFile);

            if(fileContent == PipelineType.HEAD.GetPipelineTypeName()) {
                scale = headAvatarScale;
                avatarTransform.position = new Vector3(0, -0.3f, 0);
            }
        }
        avatarTransform.localScale = scale;
    }

    IEnumerator Await(AsyncRequest r) {
        while(!r.IsDone) {
            yield return null;

            if(r.IsError) {
                Debug.LogError(r.ErrorMessage);
                yield break;
            }
        }

    }



    private IEnumerator ShowAvatar(string avatarCode) {
        print("Showing the avatar..");
        yield return new WaitForSeconds(0.05f);

        //StartCoroutine(SampleUtils.DisplayPhotoPreview(avatarCode, photoPreview));
        currentHaircut = 0;
        var currentAvatar = GameObject.Find(AVATAR_OBJECT_NAME);
        print(currentAvatar.name);
        if(currentAvatar != null)
            Destroy(currentAvatar);
        var avatarObject = new GameObject(AVATAR_OBJECT_NAME);
        var headMeshRequest = avatarProvider.GetHeadMeshAsync(avatarCode, true);
        yield return Await(headMeshRequest);

        if(headMeshRequest.IsError) {
            Debug.LogError("Could not load avatar from disk!");
        } else {
            TexturedMesh texturedMesh = headMeshRequest.Result;

            // game object can be deleted if we opened another avatar
            if(avatarObject != null && avatarObject.activeSelf) {
                avatarObject.AddComponent<RotateByMouse>();

                headObject = new GameObject(HEAD_OBJECT_NAME);
                var meshRenderer = headObject.AddComponent<SkinnedMeshRenderer>();
                meshRenderer.sharedMesh = texturedMesh.mesh;
                meshRenderer.material = IsUnlitMode ? unlitMaterial : litMaterial;
                meshRenderer.material.mainTexture = texturedMesh.texture;
                headObject.transform.SetParent(avatarObject.transform);
                SetAvatarScale(avatarCode, avatarObject.transform);
                print("Created avatar");
            }
            }
        }


    }

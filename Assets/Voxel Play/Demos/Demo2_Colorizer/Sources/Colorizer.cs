using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using VoxelPlay;

namespace VoxelPlayDemos
{
    public class Colorizer : MonoBehaviour
    {
        public int brushSize = 1;
        private Vector3? lastPaintWorldPos = null;
        public VoxelModelData modelData;
        public Button modeSwitchButton;   // Gán trong Inspector
        public Text modeSwitchText;       // Gán trong Inspector (nếu muốn hiện trạng thái)
        bool isPaintMode = true;          // Mặc định là tô màu
        public ModelDefinition modelToLoad;
        public Font font;
        public Shader textShader;
        public Text eraseModeText, globalIllumText;
        public GameObject colorButtonTemplate;
        public GameObject modelRoot; // pivot/tâm mô hình

        VoxelPlayEnvironment env;
        float orbitDistance;
        VoxelPlayFirstPersonController fps;
        bool eraseMode;
        byte[] saveGameData;
        Color currentColor = Color.white;
        int modelSize = 32;
        int modelHeight = 32;

        // --- Tham số orbit
        float orbitYaw = 0f;
        float orbitPitch = 20f;
        float orbitRadius = 30f;

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        bool isDraggingScene = false;
        Vector2 dragStartPos;
        const float DRAG_THRESHOLD = 15f;
#endif

        void Start()
        {
            env = VoxelPlayEnvironment.instance;
            fps = (VoxelPlayFirstPersonController)env.characterController;

            CreateCube();
            CreateColorSwatch();

            env.OnVoxelClick += (chunk, voxelIndex, buttonIndex) => {
                if (buttonIndex == 0 && !eraseMode)
                    env.VoxelSetColor(chunk, voxelIndex, currentColor);
            };
            env.OnVoxelDamaged += (VoxelChunk chunk, int voxelIndex, ref int damage) => {
                if (!eraseMode) damage = 0;
            };

            SetupNavigation();
            UpdateCameraOrbit();

            if (modeSwitchButton != null)
                modeSwitchButton.onClick.AddListener(TogglePaintMode);

            UpdateModeSwitchText();

            // KHÓA input controller khi khởi động (mặc định là tô màu)
            if (fps != null)
            {
                fps.externalInputOnly = true;
                fps.blockInput = true;
            }
        }

        void Update()
        {
            if (IsPointerOverUI())
            {
                lastPaintWorldPos = null;
                return;
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            if (!isPaintMode)
                HandleOrbitInput();

            if (Input.GetKeyDown(KeyCode.X)) ToggleEraseMode();
            if (Input.GetKeyDown(KeyCode.G)) ToggleGlobalIllum();
            if (Input.GetKeyDown(KeyCode.F1)) LoadModel();
            if (Input.GetKeyDown(KeyCode.F2)) SaveGame();
            if (Input.GetKeyDown(KeyCode.F3)) LoadGame();

            if (isPaintMode && Input.GetMouseButton(0))
                HandlePaint();
            else
                lastPaintWorldPos = null;
#else
            if (!isPaintMode)
            {
                if (Input.touchCount == 1)
                {
                    Touch t = Input.GetTouch(0);
                    if (t.phase == TouchPhase.Began)
                    {
                        dragStartPos = t.position;
                        isDraggingScene = false;
                    }
                    else if (t.phase == TouchPhase.Moved)
                    {
                        if (!isDraggingScene && (t.position - dragStartPos).magnitude > DRAG_THRESHOLD)
                            isDraggingScene = true;

                        if (isDraggingScene)
                            HandleOrbitInput();
                    }
                    else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                    {
                        isDraggingScene = false;
                    }
                }
            }

            if (isPaintMode && Input.GetMouseButton(0))
                HandlePaint();
            else
                lastPaintWorldPos = null;
#endif
        }

        void HandlePaint()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            VoxelHitInfo hit;
            if (env.RayCast(ray, out hit))
            {
                Vector3 pos = hit.voxelCenter;
                VoxelChunk chunk = hit.chunk;
                int voxelIndex = hit.voxelIndex;
                if (chunk != null && voxelIndex >= 0)
                {
                    int chunkSize = VoxelPlayEnvironment.CHUNK_SIZE;
                    int cx = voxelIndex % chunkSize;
                    int cy = (voxelIndex / chunkSize) % chunkSize;
                    int cz = voxelIndex / (chunkSize * chunkSize);

                    if (lastPaintWorldPos.HasValue)
                    {
                        float dist = Vector3.Distance(lastPaintWorldPos.Value, pos);
                        int steps = Mathf.CeilToInt(dist / 0.2f);
                        for (int i = 0; i <= steps; i++)
                        {
                            Vector3 lerpPos = Vector3.Lerp(lastPaintWorldPos.Value, pos, i / (float)steps);
                            PaintBrush(chunk, lerpPos, cx, cy, cz, currentColor, brushSize);
                        }
                    }
                    else
                    {
                        PaintBrush(chunk, pos, cx, cy, cz, currentColor, brushSize);
                    }
                    lastPaintWorldPos = pos;
                }
            }
        }

        bool IsPointerOverUI()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
#else
            if (Input.touchCount > 0 && EventSystem.current != null)
                return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            else
                return false;
#endif
        }

        void TogglePaintMode()
        {
            isPaintMode = !isPaintMode;
            UpdateModeSwitchText();

            if (fps != null)
            {
                fps.externalInputOnly = isPaintMode;  // true = chỉ nhận input từ script bên ngoài
                fps.blockInput = isPaintMode;         // true = khóa input khi tô màu
                fps.orbitMode = !isPaintMode;         // true = bật orbit khi chuyển sang chế độ XOAY
            }
        }

        void UpdateModeSwitchText()
        {
            if (modeSwitchText != null)
                modeSwitchText.text = isPaintMode ? "Chế độ: TÔ MÀU" : "Chế độ: XOAY";
        }

        void HandleOrbitInput()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButton(1))
            {
                float dx = Input.GetAxis("Mouse X");
                float dy = Input.GetAxis("Mouse Y");
                orbitYaw += dx * 3f;
                orbitPitch -= dy * 3f;
            }
            orbitPitch = Mathf.Clamp(orbitPitch, -89f, 89f);
            orbitRadius -= Input.mouseScrollDelta.y * 2f;
            orbitRadius = Mathf.Clamp(orbitRadius, 5f, 200f);
#else
            if (Input.touchCount == 1)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Moved)
                {
                    float dx = t.deltaPosition.x;
                    float dy = t.deltaPosition.y;
                    orbitYaw += dx * 0.2f;
                    orbitPitch -= dy * 0.2f;
                }
            }
            orbitPitch = Mathf.Clamp(orbitPitch, -89f, 89f);
#endif
            UpdateCameraOrbit();
        }

        void UpdateCameraOrbit()
        {
            Vector3 center = modelRoot != null ? modelRoot.transform.position : new Vector3(modelData.sx / 2f, modelData.sy / 2f, modelData.sz / 2f);

            float yawRad = Mathf.Deg2Rad * orbitYaw;
            float pitchRad = Mathf.Deg2Rad * orbitPitch;
            float x = center.x + orbitRadius * Mathf.Cos(pitchRad) * Mathf.Sin(yawRad);
            float y = center.y + orbitRadius * Mathf.Sin(pitchRad);
            float z = center.z + orbitRadius * Mathf.Cos(pitchRad) * Mathf.Cos(yawRad);

            if (fps != null)
            {
                fps.transform.position = new Vector3(x, y, z);
                fps.lookAt = center;
            }
        }

        void SetupNavigation()
        {
            Vector3 center = modelRoot != null ? modelRoot.transform.position : new Vector3(modelData.sx / 2f, modelData.sy / 2f, modelData.sz / 2f);
            orbitRadius = Mathf.Max(modelData.sx, modelData.sy, modelData.sz) * 1.5f;
            orbitYaw = 0f;
            orbitPitch = 20f;
            UpdateCameraOrbit();
        }

        void PaintBrush(VoxelChunk chunk, Vector3 worldPos, int baseX, int baseY, int baseZ, Color color, int brushSize)
        {
            int chunkSize = VoxelPlayEnvironment.CHUNK_SIZE;
            var voxels = chunk.voxels;
            bool anyChange = false;

            for (int dx = -brushSize + 1; dx < brushSize; dx++)
                for (int dy = -brushSize + 1; dy < brushSize; dy++)
                    for (int dz = -brushSize + 1; dz < brushSize; dz++)
                    {
                        int x = baseX + dx, y = baseY + dy, z = baseZ + dz;
                        if (x < 0 || y < 0 || z < 0 || x >= chunkSize || y >= chunkSize || z >= chunkSize) continue;

                        int idx = x + y * chunkSize + z * chunkSize * chunkSize;
                        voxels[idx].color = color;
                        anyChange = true;
                    }

            if (anyChange)
            {
                chunk.needsMeshRebuild = true;
                chunk.modified = true;
                VoxelPlayEnvironment.instance.RegisterChunkChanges(chunk);
            }
            env.ChunkRedraw(chunk, includeNeighbours: false, refreshLightmap: false, refreshMesh: true);
        }

        void ToggleEraseMode()
        {
            eraseMode = !eraseMode;
            eraseModeText.text = eraseMode ?
                "<color=green>Erase Mode</color> <color=yellow>ON</color>" :
                "<color=green>Erase Mode</color> <color=yellow>OFF</color>";
        }

        void ToggleGlobalIllum()
        {
            env.globalIllumination = !env.globalIllumination;
            env.Redraw();
            globalIllumText.text = env.globalIllumination ?
                "<color=green>Global Illum</color> <color=yellow>ON</color>" :
                "<color=green>Global Illum</color> <color=yellow>OFF</color>";
        }

        void CreateCube()
        {
            Color[,,] myModel = modelData.To3DArray();
            env.ModelPlace(Vector3d.zero, myModel);
            SetupNavigation();
        }

        void CreateColorSwatch()
        {
            Vector2 pos = colorButtonTemplate.transform.localPosition;
            Random.InitState(0);
            for (int j = 0; j < 6; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    Vector2 newPos = new Vector2(pos.x + k * 32, pos.y - j * 32);
                    GameObject newColorSwatch = Instantiate<GameObject>(colorButtonTemplate);
                    newColorSwatch.SetActive(true);
                    newColorSwatch.transform.SetParent(colorButtonTemplate.transform.parent, false);
                    newColorSwatch.transform.localPosition = newPos;
                    Button button = newColorSwatch.GetComponent<Button>();
                    ColorBlock colorBlock = button.colors;
                    Color buttonColor = new Color(Random.value, Random.value, Random.value);
                    colorBlock.normalColor = buttonColor;
                    colorBlock.pressedColor = new Color(buttonColor.r, buttonColor.g, buttonColor.b, 0.5f);
                    colorBlock.highlightedColor = new Color(buttonColor.r * 1.1f, buttonColor.g * 1.1f, buttonColor.b * 1.1f);
                    button.colors = colorBlock;
                    button.onClick.AddListener(() => {
                        currentColor = colorBlock.normalColor;
                    });
                }
            }
        }

        void SaveGame()
        {
            saveGameData = env.SaveGameToByteArray();
            env.ShowMessage("<color=yellow>World saved into memory!</color>");
        }

        void LoadGame()
        {
            if (env.LoadGameFromByteArray(saveGameData, true))
            {
                env.ShowMessage("<color=yellow>World restored!</color>");
            }
            else
            {
                env.ShowError("<color=red>World could not be restored!</color>");
            }
        }

        void LoadModel()
        {
            if (modelToLoad != null)
            {
                env.DestroyAllVoxels();
                env.ModelPlace(Vector3d.zero, modelToLoad);
                modelSize = Mathf.Max(modelToLoad.sizeX, modelToLoad.sizeZ);
                modelHeight = modelToLoad.sizeY;
                SetupNavigation();
            }
        }
    }
}

using System.IO;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Protocols;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ROS2Unity.Editor
{
    public static class ROS2UnityProjectSetup
    {
        private const string DemoScenePath = "Assets/Scenes/ROS2UnityBridge.unity";
        private const string WarehouseScenePath = "Assets/Scenes/ROS2UnityWarehouse.unity";
        private const string MaterialDirectory = "Assets/ROS2Unity/Materials";
        private const string WarehouseModelPath =
            "Assets/ThirdParty/UnityRoboticsWarehouse/Meshes/Warehouse.fbx";
        private const string WarehouseRackPath =
            "Assets/ThirdParty/UnityRoboticsWarehouse/Meshes/ShelvingRackA.fbx";
        private const float WarehouseRackTargetHeight = 3.65f;
        private const float TurtleBotMeshMetersPerSourceUnit = 0.001f;
        private const string TurtleBotBasePath =
            "Assets/ThirdParty/TurtleBot3/Meshes/Bases/burger_base.prefab";
        private const string TurtleBotLeftWheelPath =
            "Assets/ThirdParty/TurtleBot3/Meshes/Wheels/left_tire.prefab";
        private const string TurtleBotRightWheelPath =
            "Assets/ThirdParty/TurtleBot3/Meshes/Wheels/right_tire.prefab";
        private const string TurtleBotLidarPath =
            "Assets/ThirdParty/TurtleBot3/Meshes/Sensors/lds.prefab";

        [MenuItem("ROS2Unity/Build Demo Scene")]
        public static void BuildDemoScene()
        {
            EnsureRos2Define();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.localScale = new Vector3(5f, 1f, 5f);

            GameObject robot = GameObject.CreatePrimitive(PrimitiveType.Cube);
            robot.name = "Demo Robot";
            robot.transform.position = new Vector3(0f, 0.5f, 0f);
            robot.transform.localScale = new Vector3(0.8f, 0.5f, 1.2f);

            Rigidbody body = robot.AddComponent<Rigidbody>();
            body.mass = 5f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.constraints = RigidbodyConstraints.FreezeRotationX
                | RigidbodyConstraints.FreezeRotationZ;
            DifferentialDriveController controller =
                robot.AddComponent<DifferentialDriveController>();

            AddWheelVisual(robot.transform, "Left Wheel", -0.48f);
            AddWheelVisual(robot.transform, "Right Wheel", 0.48f);

            GameObject bridge = new GameObject("ROS Bridge");
            RosConnector connector = bridge.AddComponent<RosConnector>();
            connector.selectedRosVersion = RosVersion.ROS2;
            connector.RosBridgeServerUrl = "ws://localhost:9090";
            connector.protocol = Protocol.WebSocketSharp;
            connector.Serializer = RosSocket.SerializerEnum.Microsoft;
            connector.SecondsTimeout = 10;

            BridgeStatusOverlay overlay = bridge.AddComponent<BridgeStatusOverlay>();
            overlay.Connector = connector;

            RosSharpTwistSubscriber twist = bridge.AddComponent<RosSharpTwistSubscriber>();
            twist.Topic = "/cmd_vel";
            twist.TimeStep = 0f;
            twist.EnsureThreadSafety = true;
            twist.Controller = controller;

            HeartbeatSubscriber heartbeat = bridge.AddComponent<HeartbeatSubscriber>();
            heartbeat.Topic = "/ros2unity/heartbeat";
            heartbeat.TimeStep = 0.1f;
            heartbeat.EnsureThreadSafety = true;
            heartbeat.StatusOverlay = overlay;

            RosBridgeSmokeTest smokeTest = bridge.AddComponent<RosBridgeSmokeTest>();
            smokeTest.Heartbeat = heartbeat;
            smokeTest.Controller = controller;

            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            cameraObject.transform.position = new Vector3(0f, 5f, -7f);
            cameraObject.transform.LookAt(robot.transform.position);
            camera.clearFlags = CameraClearFlags.Skybox;

            EditorSceneManager.SaveScene(scene, DemoScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(DemoScenePath, true)
            };

            Selection.activeGameObject = bridge;
            EditorGUIUtility.PingObject(bridge);
            Debug.Log("ROS2UNITY_SETUP_OK: demo scene created at " + DemoScenePath);
        }

        [MenuItem("ROS2Unity/Build All Teaching Scenes")]
        public static void BuildAllScenes()
        {
            BuildDemoScene();
            BuildWarehouseScene();
            Debug.Log("ROS2UNITY_ALL_SCENES_OK");
        }

        public static void AnalyzeWarehouseModel()
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            GameObject warehouse = InstantiateModel(WarehouseModelPath);
            Renderer[] renderers = warehouse.GetComponentsInChildren<Renderer>(true);
            Bounds bounds = CalculateBounds(warehouse);
            Debug.Log("ROS2UNITY_WAREHOUSE_BOUNDS: renderers=" + renderers.Length
                + " center=" + bounds.center + " size=" + bounds.size
                + " min=" + bounds.min + " max=" + bounds.max);
            Object.DestroyImmediate(warehouse);
            EditorSceneManager.CloseScene(scene, true);
        }

        public static void AnalyzeTurtleBotModel()
        {
            string[] paths =
            {
                TurtleBotBasePath,
                TurtleBotLeftWheelPath,
                TurtleBotRightWheelPath,
                TurtleBotLidarPath
            };

            foreach (string path in paths)
            {
                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model == null)
                {
                    throw new FileNotFoundException("TurtleBot model was not imported.", path);
                }

                foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
                {
                    Debug.Log("ROS2UNITY_TURTLEBOT_PART: " + path
                        + " / " + renderer.name + " bounds=" + renderer.bounds.size);
                }
            }
        }

        public static void AnalyzeRackModel()
        {
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);
            GameObject rack = InstantiateModel(WarehouseRackPath);
            rack.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            ScaleModelToHeight(rack, WarehouseRackTargetHeight);
            Bounds bounds = CalculateBounds(rack);
            Debug.Log("ROS2UNITY_RACK_BOUNDS: center=" + bounds.center
                + " size=" + bounds.size + " min=" + bounds.min
                + " max=" + bounds.max);
            Object.DestroyImmediate(rack);
            EditorSceneManager.CloseScene(scene, true);
        }

        [MenuItem("ROS2Unity/Capture Warehouse Preview")]
        public static void CaptureWarehousePreview()
        {
            EditorSceneManager.OpenScene(WarehouseScenePath, OpenSceneMode.Single);
            Camera camera = Camera.main;
            if (camera == null)
            {
                throw new System.InvalidOperationException(
                    "Warehouse scene does not contain a Main Camera.");
            }

            string outputPath = Path.Combine(
                Path.GetTempPath(),
                "ros2unity-warehouse-preview.png");
            RenderTexture target = new RenderTexture(1280, 720, 24);
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture previousTarget = camera.targetTexture;
            Texture2D image = new Texture2D(1280, 720, TextureFormat.RGBA32, false);

            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0f, 0f, 1280f, 720f), 0, 0);
                image.Apply();
                File.WriteAllBytes(outputPath, image.EncodeToPNG());
                Debug.Log("ROS2UNITY_WAREHOUSE_PREVIEW_OK: " + outputPath);
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                Object.DestroyImmediate(image);
                target.Release();
                Object.DestroyImmediate(target);
            }
        }

        [MenuItem("ROS2Unity/Build URP Warehouse Scene")]
        public static void BuildWarehouseScene()
        {
            EnsureRos2Define();
            WarehouseMaterials materials = CreateWarehouseMaterials();

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            GameObject warehouse = InstantiateModel(WarehouseModelPath);
            warehouse.name = "Official Unity Robotics Warehouse (Apache-2.0)";
            ConfigureWarehouseModel(warehouse, materials);
            Bounds warehouseBounds = CalculateBounds(warehouse);
            Debug.Log("ROS2UNITY_WAREHOUSE_TARGET_BOUNDS: size="
                + warehouseBounds.size);

            CreateWarehouseFloorMarkings(materials);
            CreateRackAisles(materials);
            DifferentialDriveController controller = CreateAmrRobot(materials);
            GameObject bridge = CreateRosBridge(controller);
            CreateWarehouseLighting();
            CreateCamera(controller.transform);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.38f, 0.42f, 0.48f);

            EditorSceneManager.SaveScene(scene, WarehouseScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(WarehouseScenePath, true),
                new EditorBuildSettingsScene(DemoScenePath, true)
            };

            Selection.activeGameObject = bridge;
            EditorGUIUtility.PingObject(bridge);
            Debug.Log("ROS2UNITY_WAREHOUSE_SETUP_OK: scene created at "
                + WarehouseScenePath);
        }

        [MenuItem("ROS2Unity/Build Current Platform Smoke Player")]
        public static void BuildSmokePlayer()
        {
            BuildWarehouseScene();
            Directory.CreateDirectory("Builds");

            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
            string outputPath;
            switch (target)
            {
                case BuildTarget.StandaloneOSX:
                    outputPath = "Builds/ROS2UnitySmoke.app";
                    break;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    outputPath = "Builds/ROS2UnitySmoke.exe";
                    break;
                case BuildTarget.StandaloneLinux64:
                    outputPath = "Builds/ROS2UnitySmoke.x86_64";
                    break;
                default:
                    throw new System.NotSupportedException(
                        "Smoke build supports desktop Standalone targets only: " + target);
            }

            BuildReport report = BuildPipeline.BuildPlayer(
                new[] { WarehouseScenePath },
                outputPath,
                target,
                BuildOptions.Development);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new System.InvalidOperationException(
                    "ROS2Unity smoke build failed: " + report.summary.result);
            }

            Debug.Log("ROS2UNITY_URP_BUILD_OK: " + outputPath);
        }

        private static GameObject InstantiateModel(string path)
        {
            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (model == null)
            {
                throw new FileNotFoundException("Model was not imported.", path);
            }

            return (GameObject)PrefabUtility.InstantiatePrefab(model);
        }

        private static void ConfigureWarehouseModel(
            GameObject warehouse,
            WarehouseMaterials materials)
        {
            foreach (Renderer renderer in warehouse.GetComponentsInChildren<Renderer>(true))
            {
                string partName = renderer.name.ToLowerInvariant();
                Material material = materials.Structure;

                if (partName.Contains("floor"))
                {
                    material = materials.Concrete;
                }
                else if (partName.Contains("wall"))
                {
                    material = materials.Wall;
                }
                else if (partName.Contains("ceiling"))
                {
                    material = materials.Ceiling;
                }
                else if (partName.Contains("skylight"))
                {
                    material = materials.Skylight;
                }
                else if (partName.Contains("lightfixture"))
                {
                    material = materials.Light;
                }

                SetRendererMaterial(renderer, material);
                GameObjectUtility.SetStaticEditorFlags(
                    renderer.gameObject,
                    StaticEditorFlags.BatchingStatic
                    | StaticEditorFlags.NavigationStatic
                    | StaticEditorFlags.OccluderStatic
                    | StaticEditorFlags.OccludeeStatic);

                if (partName.Contains("floor")
                    || partName.Contains("wall")
                    || partName.Contains("column"))
                {
                    AddMeshCollider(renderer.gameObject);
                }
            }
        }

        private static void CreateRackAisles(WarehouseMaterials materials)
        {
            GameObject rackRoot = new GameObject("Warehouse Rack Aisles");
            float[] aisleX = { -13f, -6f, 1f };
            float[] rackZ = { -9f, -1f, 7f, 15f };
            bool scaleLogged = false;

            foreach (float x in aisleX)
            {
                foreach (float z in rackZ)
                {
                    GameObject rack = InstantiateModel(WarehouseRackPath);
                    rack.name = $"Rack {x:0}_{z:0}";
                    rack.transform.SetParent(rackRoot.transform);
                    rack.transform.position = new Vector3(x, 0f, z);
                    rack.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                    ScaleModelToHeight(rack, WarehouseRackTargetHeight);

                    if (!scaleLogged)
                    {
                        Debug.Log("ROS2UNITY_RACK_TARGET_BOUNDS: size="
                            + CalculateBounds(rack).size);
                        scaleLogged = true;
                    }

                    foreach (Renderer renderer in rack.GetComponentsInChildren<Renderer>(true))
                    {
                        ConfigureRackRenderer(renderer, materials);
                        AddMeshCollider(renderer.gameObject);
                    }
                }
            }
        }

        private static void CreateWarehouseFloorMarkings(WarehouseMaterials materials)
        {
            GameObject markings = new GameObject("AMR Safety Lanes");
            float[] laneBoundaries = { -11f, -8f, -4f, -1f };

            foreach (float x in laneBoundaries)
            {
                GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                line.name = $"Safety Line x={x:0}";
                line.transform.SetParent(markings.transform);
                line.transform.position = new Vector3(x, 0.012f, 0f);
                line.transform.localScale = new Vector3(0.07f, 0.012f, 44f);
                line.GetComponent<Renderer>().sharedMaterial = materials.SafetyLine;
                Object.DestroyImmediate(line.GetComponent<Collider>());
            }
        }

        private static void ConfigureRackRenderer(
            Renderer renderer,
            WarehouseMaterials materials)
        {
            Material[] source = renderer.sharedMaterials;
            int count = Mathf.Max(1, source.Length);
            Material[] replacements = new Material[count];
            string rendererName = renderer.name.ToLowerInvariant();
            bool palletRenderer = rendererName.Contains("pallet");

            for (int index = 0; index < count; index++)
            {
                string materialName = source.Length > index && source[index] != null
                    ? source[index].name.ToLowerInvariant()
                    : renderer.name.ToLowerInvariant();

                if (materialName.Contains("woodpallet")
                    || (palletRenderer && index == count - 1))
                {
                    replacements[index] = materials.Pallet;
                }
                else if (materialName.Contains("boxes") || palletRenderer)
                {
                    replacements[index] = materialName.Contains("slogo") || index > 0
                        ? materials.CargoAccent
                        : materials.Cargo;
                }
                else
                {
                    replacements[index] = materials.Rack;
                }
            }

            renderer.sharedMaterials = replacements;
        }

        private static DifferentialDriveController CreateAmrRobot(
            WarehouseMaterials materials)
        {
            GameObject robot = new GameObject("TurtleBot3 Burger AMR");
            robot.transform.position = new Vector3(-2.5f, 0f, -16f);

            Rigidbody body = robot.AddComponent<Rigidbody>();
            body.mass = 1f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.constraints = RigidbodyConstraints.FreezeRotationX
                | RigidbodyConstraints.FreezeRotationZ;

            CapsuleCollider collider = robot.AddComponent<CapsuleCollider>();
            collider.radius = 0.10f;
            collider.height = 0.20f;
            collider.center = new Vector3(0f, 0.10f, -0.02f);

            DifferentialDriveController controller =
                robot.AddComponent<DifferentialDriveController>();

            GameObject visualRoot = new GameObject("Official TurtleBot3 Meshes");
            visualRoot.transform.SetParent(robot.transform, false);
            visualRoot.transform.localScale =
                Vector3.one * TurtleBotMeshMetersPerSourceUnit;

            GameObject baseVisual = InstantiateRobotPart(
                TurtleBotBasePath,
                visualRoot.transform,
                "Burger Base",
                new Vector3(0f, 0f, -32f));
            foreach (Renderer renderer in baseVisual.GetComponentsInChildren<Renderer>(true))
            {
                SetRendererMaterial(
                    renderer,
                    renderer.name.EndsWith("_0")
                        ? materials.RobotAccent
                        : materials.RobotBody);
            }

            GameObject leftWheel = InstantiateRobotPart(
                TurtleBotLeftWheelPath,
                visualRoot.transform,
                "Left Wheel",
                new Vector3(-80f, 23f, 0f));
            GameObject rightWheel = InstantiateRobotPart(
                TurtleBotRightWheelPath,
                visualRoot.transform,
                "Right Wheel",
                new Vector3(80f, 23f, 0f));
            GameObject lidar = InstantiateRobotPart(
                TurtleBotLidarPath,
                visualRoot.transform,
                "LDS-01 2D LiDAR",
                new Vector3(0f, 172f, -32f));

            foreach (Renderer renderer in leftWheel.GetComponentsInChildren<Renderer>(true))
            {
                SetRendererMaterial(renderer, materials.Wheel);
            }
            foreach (Renderer renderer in rightWheel.GetComponentsInChildren<Renderer>(true))
            {
                SetRendererMaterial(renderer, materials.Wheel);
            }
            foreach (Renderer renderer in lidar.GetComponentsInChildren<Renderer>(true))
            {
                SetRendererMaterial(renderer, materials.Lidar);
            }

            AlignModelBottomToY(visualRoot, robot.transform.position.y);
            Debug.Log("ROS2UNITY_TURTLEBOT_TARGET_BOUNDS: size="
                + CalculateBounds(visualRoot).size);

            CreateRobotVisibilityRing(robot.transform, materials.RobotMarker);

            return controller;
        }

        private static void CreateRobotVisibilityRing(
            Transform robot,
            Material markerMaterial)
        {
            GameObject markerObject = new GameObject("Robot Visibility Ring");
            markerObject.transform.SetParent(robot, false);
            markerObject.transform.localPosition = new Vector3(0f, 0.006f, 0f);

            LineRenderer marker = markerObject.AddComponent<LineRenderer>();
            marker.sharedMaterial = markerMaterial;
            marker.useWorldSpace = false;
            marker.loop = true;
            marker.positionCount = 64;
            marker.startWidth = 0.018f;
            marker.endWidth = 0.018f;
            marker.numCornerVertices = 4;
            marker.shadowCastingMode = ShadowCastingMode.Off;
            marker.receiveShadows = false;

            const float radius = 0.25f;
            for (int index = 0; index < marker.positionCount; index++)
            {
                float angle = Mathf.PI * 2f * index / marker.positionCount;
                marker.SetPosition(
                    index,
                    new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
        }

        private static GameObject InstantiateRobotPart(
            string path,
            Transform parent,
            string name,
            Vector3 localPosition,
            Quaternion? localRotation = null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                throw new FileNotFoundException("TurtleBot part was not imported.", path);
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = localRotation ?? Quaternion.identity;
            return instance;
        }

        private static GameObject CreateRosBridge(
            DifferentialDriveController controller)
        {
            GameObject bridge = new GameObject("ROS Bridge");
            RosConnector connector = bridge.AddComponent<RosConnector>();
            connector.selectedRosVersion = RosVersion.ROS2;
            connector.RosBridgeServerUrl = "ws://localhost:9090";
            connector.protocol = Protocol.WebSocketSharp;
            connector.Serializer = RosSocket.SerializerEnum.Microsoft;
            connector.SecondsTimeout = 10;

            BridgeStatusOverlay overlay = bridge.AddComponent<BridgeStatusOverlay>();
            overlay.Connector = connector;

            RosSharpTwistSubscriber twist = bridge.AddComponent<RosSharpTwistSubscriber>();
            twist.Topic = "/cmd_vel";
            twist.TimeStep = 0f;
            twist.EnsureThreadSafety = true;
            twist.Controller = controller;

            HeartbeatSubscriber heartbeat = bridge.AddComponent<HeartbeatSubscriber>();
            heartbeat.Topic = "/ros2unity/heartbeat";
            heartbeat.TimeStep = 0.1f;
            heartbeat.EnsureThreadSafety = true;
            heartbeat.StatusOverlay = overlay;

            RosBridgeSmokeTest smokeTest = bridge.AddComponent<RosBridgeSmokeTest>();
            smokeTest.Heartbeat = heartbeat;
            smokeTest.Controller = controller;
            return bridge;
        }

        private static void CreateWarehouseLighting()
        {
            GameObject sunObject = new GameObject("Warehouse Directional Light");
            Light sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.2f;
            sun.color = new Color(0.92f, 0.95f, 1f);
            sunObject.transform.rotation = Quaternion.Euler(55f, -35f, 0f);

            Vector3[] positions =
            {
                new Vector3(-13f, 5.5f, -16f),
                new Vector3(-1f, 5.5f, -16f),
                new Vector3(-13f, 5.5f, 0f),
                new Vector3(-1f, 5.5f, 0f),
                new Vector3(-13f, 5.5f, 16f),
                new Vector3(-1f, 5.5f, 16f)
            };

            foreach (Vector3 position in positions)
            {
                GameObject lightObject = new GameObject("Warehouse Area Light");
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 13f;
                light.intensity = 2.6f;
                light.color = new Color(1f, 0.92f, 0.78f);
                lightObject.transform.position = position;
            }
        }

        private static void CreateCamera(Transform target)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            camera.fieldOfView = 50f;
            camera.nearClipPlane = 0.05f;
            camera.clearFlags = CameraClearFlags.Skybox;

            RobotFollowCamera followCamera =
                cameraObject.AddComponent<RobotFollowCamera>();
            followCamera.Target = target;
            followCamera.LocalOffset = new Vector3(0.95f, 0.58f, -1.20f);
            followCamera.LookOffset = new Vector3(0f, 0.12f, -0.02f);
            followCamera.SnapToTarget();
        }

        private static WarehouseMaterials CreateWarehouseMaterials()
        {
            Directory.CreateDirectory(MaterialDirectory);
            WarehouseMaterials materials = new WarehouseMaterials
            {
                Concrete = CreateUrpMaterial(
                    "Concrete", new Color(0.38f, 0.40f, 0.42f), 0f, 0.18f),
                Wall = CreateUrpMaterial(
                    "Wall", new Color(0.64f, 0.70f, 0.75f), 0.05f, 0.24f),
                Ceiling = CreateUrpMaterial(
                    "Ceiling", new Color(0.70f, 0.74f, 0.77f), 0.08f, 0.30f),
                Structure = CreateUrpMaterial(
                    "Structure", new Color(0.18f, 0.25f, 0.32f), 0.72f, 0.46f),
                Skylight = CreateUrpMaterial(
                    "Skylight", new Color(0.34f, 0.66f, 0.90f), 0.05f, 0.72f),
                Light = CreateUrpMaterial(
                    "Light", new Color(1f, 0.88f, 0.58f), 0f, 0.45f, true),
                Rack = CreateUrpMaterial(
                    "Rack", new Color(0.16f, 0.42f, 0.62f), 0.70f, 0.44f),
                Cargo = CreateUrpMaterial(
                    "Cargo", new Color(0.65f, 0.36f, 0.12f), 0f, 0.20f),
                CargoAccent = CreateUrpMaterial(
                    "CargoAccent", new Color(0.80f, 0.66f, 0.24f), 0f, 0.22f),
                Pallet = CreateUrpMaterial(
                    "Pallet", new Color(0.42f, 0.22f, 0.08f), 0f, 0.18f),
                RobotBody = CreateUrpMaterial(
                    "RobotBody", new Color(0.96f, 0.46f, 0.08f), 0.18f, 0.45f),
                RobotAccent = CreateUrpMaterial(
                    "RobotAccent", new Color(0.12f, 0.16f, 0.20f), 0.62f, 0.56f),
                Wheel = CreateUrpMaterial(
                    "Wheel", new Color(0.025f, 0.03f, 0.035f), 0.10f, 0.22f),
                Lidar = CreateUrpMaterial(
                    "Lidar", new Color(0.08f, 0.85f, 0.92f), 0.22f, 0.65f, true),
                RobotMarker = CreateUrpMaterial(
                    "RobotMarker", new Color(0.10f, 0.88f, 1f), 0f, 0.50f, true),
                SafetyLine = CreateUrpMaterial(
                    "SafetyLine", new Color(1f, 0.72f, 0.08f), 0f, 0.38f, true)
            };
            AssetDatabase.SaveAssets();
            return materials;
        }

        private static Material CreateUrpMaterial(
            string name,
            Color color,
            float metallic,
            float smoothness,
            bool emission = false)
        {
            string path = $"{MaterialDirectory}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    throw new System.InvalidOperationException(
                        "URP Lit shader is unavailable.");
                }

                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }

            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            if (emission)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 1.8f);
            }
            else
            {
                material.DisableKeyword("_EMISSION");
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void SetRendererMaterial(Renderer renderer, Material material)
        {
            int count = Mathf.Max(1, renderer.sharedMaterials.Length);
            Material[] materials = new Material[count];
            for (int index = 0; index < count; index++)
            {
                materials[index] = material;
            }

            renderer.sharedMaterials = materials;
        }

        private static Bounds CalculateBounds(GameObject root)
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return new Bounds(root.transform.position, Vector3.zero);
            }

            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return bounds;
        }

        private static float ScaleModelToHeight(GameObject model, float targetHeight)
        {
            Bounds bounds = CalculateBounds(model);
            if (bounds.size.y <= Mathf.Epsilon)
            {
                throw new System.InvalidOperationException(
                    "Cannot normalize a model without renderer height: " + model.name);
            }

            float scale = targetHeight / bounds.size.y;
            model.transform.localScale *= scale;
            return scale;
        }

        private static void AlignModelBottomToY(GameObject model, float targetMinY)
        {
            Bounds bounds = CalculateBounds(model);
            model.transform.position += Vector3.up * (targetMinY - bounds.min.y);
        }

        private static void AddMeshCollider(GameObject target)
        {
            MeshFilter meshFilter = target.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null
                || target.GetComponent<Collider>() != null)
            {
                return;
            }

            MeshCollider collider = target.AddComponent<MeshCollider>();
            collider.sharedMesh = meshFilter.sharedMesh;
        }

        private static void AddWheelVisual(Transform parent, string name, float x)
        {
            GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = name;
            wheel.transform.SetParent(parent, false);
            wheel.transform.localPosition = new Vector3(x, -0.35f, 0f);
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            wheel.transform.localScale = new Vector3(0.45f, 0.12f, 0.45f);

            Object.DestroyImmediate(wheel.GetComponent<Collider>());
        }

        private static void EnsureRos2Define()
        {
            BuildTargetGroup group = BuildTargetGroup.Standalone;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            string[] parts = defines.Split(';');
            if (System.Array.IndexOf(parts, "ROS2") < 0)
            {
                string updated = string.IsNullOrEmpty(defines)
                    ? "ROS2"
                    : defines + ";ROS2";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(
                    group,
                    updated);
            }
        }

        private sealed class WarehouseMaterials
        {
            public Material Concrete;
            public Material Wall;
            public Material Ceiling;
            public Material Structure;
            public Material Skylight;
            public Material Light;
            public Material Rack;
            public Material Cargo;
            public Material CargoAccent;
            public Material Pallet;
            public Material RobotBody;
            public Material RobotAccent;
            public Material Wheel;
            public Material Lidar;
            public Material RobotMarker;
            public Material SafetyLine;
        }
    }
}

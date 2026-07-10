using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.Protocols;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ROS2Unity.Editor
{
    public static class ROS2UnityProjectSetup
    {
        private const string DemoScenePath = "Assets/Scenes/ROS2UnityBridge.unity";

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
    }
}

using UnityEngine;
using Cysharp.Threading.Tasks;
using toio;

    public class MyBasicScene : MonoBehaviour
    {
        [SerializeField] ConnectType type;
        float intervalTime = 0.05f;
        float elapsedTime = 0;
        Cube cube;
        bool isConnected = false;

        // 非同期初期化
        // C#標準機能であるasync/awaitキーワードを使用する事で、検索・接続それぞれで終了待ちする
        // async: 非同期キーワード
        // await: 待機キーワード
        async void Start()
        {
            // Bluetoothデバイスを検索
            var peripheral = await new NearestScanner(type).Scan();
            // デバイスへ接続してCube変数を生成
            cube = await new CubeConnecter(type).Connect(peripheral);
        }

        int cnt = 0;
        void Update()
        {
            if(cube == null && Time.frameCount % 60 == 0) Debug.Log("Connecting");
            else if(cube != null && !isConnected)
            {
                isConnected = true;
                DoSequence();
                Debug.Log("Connected");
            }
            // cube.Move(50, -50, 200);
            // Debug.Log($"### x: {cube.x}, y: {cube.y}");
        }

        async void DoSequence()
        {
            int x = 0;
            int y = 0;
            int angle = 0;
            for(int i = 0; i < 10; i++)
            {
                int remain = i % 4;
                x = (remain < 2 ? 50 : -50) + 250;
                y = (remain == 0 || remain == 3 ? 50 : -50) + 250;
                // TODO: angle
                // angle = (int)(Mathf.Atan2(cube.y - y, cube.x - x) * 180 / Mathf.PI);
                cube.TargetMove(x, y, angle);
                float dist = 9999;
                Debug.Log($"x: {x}, y: {y}");
                while(dist > 10f)
                {
                    Debug.Log($"### dist: {dist}, x: {cube.x}, y: {cube.y}");
                    await UniTask.Delay(100);
                    dist = Mathf.Sqrt(Mathf.Pow(cube.x - x, 2f) + Mathf.Pow(cube.y - y, 2f));
                }
            }
        }
    }
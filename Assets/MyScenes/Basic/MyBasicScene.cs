using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using toio;
using toio.Navigation;

public class MyBasicScene : MonoBehaviour
{

  static readonly float STAGE_SIZE = 500f;

  [SerializeField] ConnectType type;
  float intervalTime = 0.05f;
  float elapsedTime = 0;
  CubeManager cubeManager = new CubeManager();
  bool isConnected = false;
  Vector2 targetPos = Vector2.zero;
  float t = 0;

  readonly float REACH_THRESHOLD = 10f;

  /// <summary>
  /// 0-1に正規化した値を返すとステージ座標に変換するsute-zizahyounihenkannsuru
  /// </summary>
  /// <param name="x"></param>
  /// <returns></returns>
  static float GetX(float x)
  {
    return (x * .9f + .05f) * STAGE_SIZE;
  }

  static float GetY(float y)
  {
    return (y * .9f + .05f) * STAGE_SIZE;
  }

  // 非同期初期化
  // C#標準機能であるasync/awaitキーワードを使用する事で、検索・接続それぞれで終了待ちする
  // async: 非同期キーワード
  // await: 待機キーワード
  async void Start()
  {
    cubeManager = new CubeManager(type);
    cubeManager.MultiConnect(3);
  }

  int cnt = 0;
  void Update()
  {
    if (cubeManager.navigators.Count == 0) return;

    for (int i = 0; i < cubeManager.navigators.Count; i++)
    {
      CubeNavigator cn = cubeManager.navigators[i];
      cn.Update();
      if (Time.frameCount % (int)(100f / 1000f * 60f) == 0)
      {
        if (Input.GetKey(KeyCode.Alpha1)) Form1Update(cn, i, cubeManager.navigators);
        else if (Input.GetKey(KeyCode.Alpha2)) Form2Update(cn, i, cubeManager.navigators);
      }

      if (Input.GetKeyDown(KeyCode.R)) Rotate(cn, i, cubeManager.navigators);
    }
  }

  void Form1Update(CubeNavigator cn, int i, List<CubeNavigator> arr)
  {
    float phase = (float)i / (float)arr.Count * Mathf.PI * 2f;
    float xPower = (float)Input.mousePosition.x / (float)Screen.width;
    float yPower = (float)Input.mousePosition.y / (float)Screen.height;
    float rad = yPower * .85f + .15f;
    t += Time.deltaTime + xPower * (1f / 60f);
    float x = MyBasicScene.GetX((Mathf.Sin(t + phase) * rad + 1f) / 2f);
    float y = MyBasicScene.GetY((Mathf.Cos(t + phase) * rad + 1f) / 2f);
    cn.Navi2Target(new Vector2(x, y)).Exec();
  }

  // NOTE: Navigatorの直線移動が変？
  // NOTE: CubeHandleで操作しつつ斥力だけ自分で実装する形にすれば良い？
  void Form2Update(CubeNavigator cn, int i, List<CubeNavigator> arr)
  {
    float phase = (float)i / (float)arr.Count * Mathf.PI * 2f;
    float xPower = (float)Input.mousePosition.x / (float)Screen.width;
    float yPower = (float)Input.mousePosition.y / (float)Screen.height;
    t += Time.deltaTime + xPower * (1f / 30f);
    float x = MyBasicScene.GetX((float)i / (float)(arr.Count - 1));
    float y = MyBasicScene.GetY((Mathf.Sin(t + (float)i * .4f) + 1f) / 2f);
    Debug.Log($"x {x} y {y}");
    cn.Navi2Target(new Vector2(x, y), 100, 200).Exec();
  }

  async void Rotate(CubeNavigator cn, int i, List<CubeNavigator> arr)
  {
    Debug.Log("rotate");
    await UniTask.Delay(i * 200);
    cn.handle.RotateByDeg(360, 200).Exec();
  }

}
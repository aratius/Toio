using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using toio;
using toio.Navigation;

enum AIRobotScene
{
  standby,
  playing,
  clearPerformance
}

public class AIRobot : MonoBehaviour
{

  static readonly float STAGE_SIZE = 500f;

  [SerializeField] ConnectType type;
  [SerializeField] int num;
  [SerializeField] readonly float CLEAR_THRESHOLD = 90f;
  CubeManager cubeManager = new CubeManager();
  List<Vector2> startPositions = new List<Vector2>();
  AIRobotScene scene = AIRobotScene.standby;
  readonly int UPDATE_INTERVAL = 100;

  CubeHandle target
  {
    get { return cubeManager.handles[0]; }
  }
  List<CubeHandle> players
  {
    get
    {
      List<CubeHandle> cubes = new List<CubeHandle>();
      for (int i = 0; i < cubeManager.handles.Count; i++)
        if (i != 0) cubes.Add(cubeManager.handles[i]);
      return cubes;
    }
  }

  /// <summary>
  /// 0-1に正規化した値を返すとステージ座標に変換する
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

  async void Start()
  {
    cubeManager = new CubeManager(type);
    cubeManager.MultiConnect(num);
    StartStandby();
  }

  void Update()
  {
    if (players.Count == 0) return;
    if (Time.frameCount % (int)((float)UPDATE_INTERVAL / (float)1000 * (float)60) == 0)
    {
      if (scene == AIRobotScene.standby) StandbyUpdate();
      else if (scene == AIRobotScene.playing) PlayingUpdate();
      else if (scene == AIRobotScene.clearPerformance) ClearPerformanceUpdate();
    }
    if (Input.GetKey(KeyCode.Alpha1)) StartStandby();
    else if (Input.GetKey(KeyCode.Alpha2)) StartPlaying();
    else if (Input.GetKey(KeyCode.Alpha3)) StartClearPerformance();
  }

  async void StartStandby()
  {
    scene = AIRobotScene.standby;
    float dist = 0f;
    while (dist < 400f)
    {
      startPositions.Clear();
      for (int i = 0; i < num; i++)
      {
        startPositions.Add(
          new Vector2(
            AIRobot.GetX(Random.Range(0.2f, 0.8f)),
            AIRobot.GetY(Random.Range(0.2f, 0.8f))
          )
        );
      }
      dist = 0f;
      for (int i = 1; i < startPositions.Count; i++)
        dist += Mathf.Sqrt(
          Mathf.Pow((float)(startPositions[i].x - startPositions[0].x), 2f) + Mathf.Pow((float)(startPositions[i].y - startPositions[0].y), 2f)
        );
    }
    await UniTask.Delay(5000);
    StartPlaying();
  }

  void StartPlaying()
  {
    scene = AIRobotScene.playing;
    // TODO: 何かある？
    cubeManager.syncCubes[0].PlayPresetSound(0, 255);
  }

  async void StartClearPerformance()
  {
    scene = AIRobotScene.clearPerformance;

    Debug.Log("### Start clear performance");
    // TODO: 一定時間で強制的に切りたい
    // TODO: クリア音
    cubeManager.syncCubes[0].PlayPresetSound(9, 255);
    target.MoveRaw(-50, 50, 1000);
    await UniTask.Delay(3000);
    target.MoveRaw(0, 0, 1000);

    StartStandby();
    Debug.Log("### End clear performance");
  }

  void StandbyUpdate()
  {
    {
      for (int i = 0; i < cubeManager.navigators.Count; i++)
      {
        CubeNavigator cn = cubeManager.navigators[i];
        cn.Update();
        cn.Navi2Target(startPositions[i], 100, 200).Exec();
      }
    }
  }

  void PlayingUpdate()
  {
    float left = 0;
    float right = 0;

    if (Input.GetKey(KeyCode.Q))
      left = 50f;
    else if (Input.GetKey(KeyCode.A))
      left = -50f;
    if (Input.GetKey(KeyCode.W))
      right = 50f;
    else if (Input.GetKey(KeyCode.S))
      right = -50f;

    for (int i = 0; i < players.Count; i++)
    {
      CubeHandle cn = players[i];
      cn.Update();
      float power = (float)(i + 1) / (float)players.Count;
      cn.MoveRaw((int)(left * power), (int)(right * power), 1000);
    }

    // TODO: クリア判定
    float dist = 0;
    for (int i = 0; i < players.Count; i++)
      dist += Mathf.Sqrt(
        Mathf.Pow((float)(players[i].x - target.x), 2f) + Mathf.Pow((float)(players[i].y - target.y), 2f)
      );
    Debug.Log(dist);
    if (dist < CLEAR_THRESHOLD)
    {
      StartClearPerformance();
      for (int i = 0; i < players.Count; i++)
      {
        CubeHandle cn = players[i];
        cn.MoveRaw(0, 0, 1000);
      }
    }
    else
    {
      // TODO: ターゲットを動かす
      float power = Mathf.Pow((500f - dist) / 500f, 2f);
      power = Mathf.Max(power, 0.2f);
      float direction = 1f;
      float interval = 1;
      if ((float)(Time.frameCount % (interval * 60)) < ((float)(interval * 60) * 0.5f)) direction = -1f;
      target.MoveRaw((int)(50f * power * direction), (int)(50f * power * -direction), 1000);

      // 近づいている音
      if (Time.frameCount % (UPDATE_INTERVAL * 10) == 0)
      {
        int note = (int)(power * 60f) + 60;
        byte noteByte = System.Convert.ToByte(note);
        List<Cube.SoundOperation> sound = new List<Cube.SoundOperation>();
        sound.Add(new Cube.SoundOperation(100, 255, noteByte));  // note 0-128
        cubeManager.syncCubes[0].PlaySound(1, sound.ToArray());
      }
    }
  }

  async void ClearPerformanceUpdate()
  {
  }

}
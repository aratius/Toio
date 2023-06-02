using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using toio;
using toio.Navigation;
using MidiPlayerTK;

public class MyMidiScene : MonoBehaviour
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

  void Update()
  {
    if (cubeManager.navigators.Count == 0) return;

    for (int i = 0; i < cubeManager.navigators.Count; i++)
    {
      CubeNavigator cn = cubeManager.navigators[i];
      cn.Update();
    }
  }

  async void Rotate(CubeNavigator cn, double angle, int i, List<CubeNavigator> arr)
  {
    Debug.Log("rotate");
    cn.handle.RotateByDeg(angle, 200).Exec();
  }

  async void Led(Cube c)
  {
    Debug.Log("led");
    // c.TurnLedOn(255, 255, 255, 200).Exec();
    c.TurnLedOn(255, 255, 255, 200);
  }

  public void OnNotes(List<MPTKEvent> mptkEvents)
  {
    Debug.Log("Received " + mptkEvents.Count + " MIDI Events");
    // Loop on each MIDI events
    foreach (MPTKEvent mptkEvent in mptkEvents)
    {
        // Log if event is a note on
        if (mptkEvent.Command == MPTKCommand.NoteOn)
        {
            Debug.Log($"Note on Time:{mptkEvent.RealTime} millisecond  Note:{mptkEvent.Value}  Duration:{mptkEvent.Duration} millisecond  Velocity:{mptkEvent.Velocity}");
            if(mptkEvent.Value == 82)
            {
              Rotate(cubeManager.navigators[0], 90, 0, cubeManager.navigators);
              Led(cubeManager.syncCubes[0]);
            }
            else if(mptkEvent.Value == 42)
            {
              Rotate(cubeManager.navigators[1], 45, 1, cubeManager.navigators);
              Led(cubeManager.syncCubes[1]);
            }
            else if(mptkEvent.Value == 70)
            {
              Rotate(cubeManager.navigators[2], 90, 2, cubeManager.navigators);
              Led(cubeManager.syncCubes[2]);
            }
        }

        // Uncomment to display all MIDI events
        // Debug.Log(mptkEvent.ToString());
    }
  }

}
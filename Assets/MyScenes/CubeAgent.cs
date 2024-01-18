using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using toio;
using toio.Simulator;


public class CubeAgent : Agent
{
    public ConnectType connectType;


    CubeManager cm;
    Stage stage;
    Mat mat;
    Transform targetPole;
    GameObject cube;
    float matW = 0.4f;
    float matH = 0.3f;


    // インスタンス生成時の初期化　初期化時に呼ばれる
    async public override void Initialize()
    {
        this.stage = GameObject.FindObjectOfType<Stage>();
        this.mat = GameObject.FindObjectOfType<Mat>();
        this.targetPole = this.stage.transform.Find("TargetPole");
        this.cube = GameObject.Find("Cube");


        this.cm = new CubeManager(connectType);
        await this.cm.MultiConnect(1);

    }


    // エピソード開始時の初期化 エピソード開始時に呼ばれる
    public override void OnEpisodeBegin()
    {
        while (true)
        {
            float x = Random.value * matW * 0.8f - (matW * 0.8f / 2f);
            float y = Random.value * matH * 0.8f - (matH * 0.8f / 2f);
            this.targetPole.position = new Vector3(x, 0, y);
            float distance = Vector3.Distance(this.cube.transform.position, this.targetPole.position);
            if (distance > 0.06f)
            {
                this.targetPole.gameObject.SetActive(true);
                break;
            }
        }
    }


    // 人工知能への状態の提供 状態取得時に呼ばれる
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.cube.transform.position.x);
        sensor.AddObservation(this.cube.transform.position.z);


        sensor.AddObservation(this.cube.transform.rotation.eulerAngles.y / 360f);


        sensor.AddObservation(this.targetPole.position.x);
        sensor.AddObservation(this.targetPole.position.z);
    }


    // 人工知能が決定した行動を受け取り実行し、行動結果に応じて報酬を提供 行動実行時に呼ばれる
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int action = actionBuffers.DiscreteActions[0];


        foreach (var handle in cm.syncHandles)
        {
            if (action == 1) handle.Move(0, -40, 100, false);   // 左
            if (action == 2) handle.Move(0, 40, 100, false);    // 右
            if (action == 3) handle.Move(40, 0, 100, false);    // 上
            if (action == 4) handle.Move(-40, 0, 100, false);   // 下
        }


        float distance = Vector3.Distance(this.cube.transform.position, this.targetPole.position);
        if(distance < 0.03f)
        {
            AddReward(1.0f);
            EndEpisode();
        }


        Vector3 cubePos = this.cube.transform.position;
        if (cubePos.x < -matW * 0.9f /2f || matW * 0.9f /2f < cubePos.x || cubePos.z < -matH * 0.9f / 2f || matH * 0.9f / 2f < cubePos.z)
        {
            EndEpisode();


            if(this.connectType != ConnectType.Real)
            {
                this.cube.transform.position = new Vector3();
                this.cube.transform.rotation = new Quaternion();
            }
        }
    }


    // 人間による行動の決定
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        var dicreseActionsOut = actionsOut.DiscreteActions;
        dicreseActionsOut[0] = 0;


        if (h < -0.5f) dicreseActionsOut[0] = 1;  // 左
        if (h > 0.5f) dicreseActionsOut[0] = 2;   // 右
        if (v > 0.5f) dicreseActionsOut[0] = 3;   // 上
        if (v < -0.5f) dicreseActionsOut[0] = 4;  // 下
    }


    // Update is called once per frame
    void Update()
    {
        if (this.connectType == ConnectType.Real)
        {
            foreach (var c in cm.cubes)
            {
                this.cube.transform.position = this.mat.MatCoord2UnityCoord(c.x, c.y);
                this.cube.transform.rotation = Quaternion.Euler(0, c.angle + 90, 0);
            }
        }
    }
}




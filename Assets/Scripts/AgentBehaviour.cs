using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using static UnityEngine.GraphicsBuffer;

public class AgentBehaviour : Agent
{
    [Header("=====各変数値=====")]
    public Transform agent; // 磁石
    public Transform cargo; // 荷物
    public Transform target; // ゴール

    //private Rigidbody magnetRb;
    private Rigidbody cargoRb;
    private float countup = 0.0f;

    private Vector3 startAgentPos = Vector3.zero;
    private Vector3 startCargoPos = Vector3.zero;
    private float previousTargetDistance = 0.0f;
    private float previousCargoDistance = 0.0f;
    private bool m_bCurry = false;

    [Header("設定値")]
    public float movementSpeed = 2.0f;
    public float timelimit = 10.0f;

    void Start()
    {
        startAgentPos = agent.position;
        startCargoPos = cargo.position;
        //magnetRb = GetComponent<Rigidbody>();
        cargoRb = cargo.GetComponent<Rigidbody>();
    }
    private void Update()
    {
        AddReward(-0.01f);

        countup += Time.deltaTime;
    }
    public override void OnEpisodeBegin()
    {
        agent.position = startAgentPos;
        cargo.position = startCargoPos;
        // 加速度リセット
        //magnetRb.velocity = Vector3.zero;
        cargoRb.velocity = Vector3.zero;
        cargoRb.angularVelocity = Vector3.zero;
        // カウントリセット
        countup = 0.0f;
        //運んでいるフラグをリセット
        m_bCurry = false;
        // ゴールの位置をランダム化
        target.localPosition = new Vector3(
            Random.Range(-4.0f, 4.0f), 0f, Random.Range(-4.0f, 4.0f)
        );
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // マグネット、荷物、ゴールの位置情報を観測
        sensor.AddObservation(agent.position);
        sensor.AddObservation(cargo.position);
        sensor.AddObservation(target.position);

        // 荷物とマグネットの距離を観測
        sensor.AddObservation(Vector3.Distance(agent.position, cargo.position));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // 行動の取得 (左右、上下、前後)
        var move = Vector3.zero;
        move.x = actions.ContinuousActions[0];
        move.y = actions.ContinuousActions[1];
        move.z = actions.ContinuousActions[2];

        // マグネットの動き
        agent.localPosition += new Vector3(move.x, move.y, move.z) * movementSpeed * Time.deltaTime;

        float currentTargetDistance = Vector3.Distance(agent.position, target.position);
        float currentCargoDistance = Vector3.Distance(cargo.position, agent.position);
        // 荷物の吸着処理
        if (currentCargoDistance < 0.5f)
        {
            cargo.position = agent.position;
            m_bCurry = true;
            //Debug.Log("荷物を持った！");
            AddReward(0.5f); // 吸着報酬
        }
        if (m_bCurry==true)
        {
            //前フレームより距離が近くなっていたら
            if (currentTargetDistance < previousTargetDistance)
            {
                AddReward(0.05f); // ゴールに近づくたびに少し報酬
                //Debug.Log("近づいている！");
            }
            else
            {
                AddReward(-0.05f); // 遠ざかるとペナルティ
                //Debug.Log("遠ざかっている！");
            }
            previousTargetDistance = currentTargetDistance;
        }
        else
        {
            //前フレームより距離が近くなっていたら
            if (currentCargoDistance < previousCargoDistance)
            {
                AddReward(0.01f); // ゴールに近づくたびに少し報酬
                //Debug.Log("近づいている！");
            }
            else
            {
                AddReward(-0.01f); // 遠ざかるとペナルティ
                //Debug.Log("遠ざかっている！");
            }
            previousCargoDistance = currentCargoDistance;

        }

        // 報酬と失敗条件の判定
        if (currentTargetDistance < 0.5f)
        {
            AddReward(10.0f); // ゴールに運べた
            Debug.Log("ゴール！");
            EndEpisode();
        }
        if (cargo.position.y < -0.5f)
        {
            AddReward(-5.0f); // 荷物が地面に触れる
            Debug.Log("落としてしまった！");
            EndEpisode();
        }
        else if (countup >= timelimit)
        {
            AddReward(-1.0f);   //規定時間までなにも起こらなかった
            //Debug.Log("なにも起こらなかった！");
            EndEpisode();
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // 手動操作のためのデバッグ入力
        var action = actionsOut.ContinuousActions;
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        action[2] = Input.GetAxis("Vertical");
    }
}

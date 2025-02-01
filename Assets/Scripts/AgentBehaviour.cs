using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using static UnityEngine.GraphicsBuffer;

public class AgentBehaviour : Agent
{
    [Header("=====�e�ϐ��l=====")]
    public Transform agent; // ����
    public Transform cargo; // �ו�
    public Transform target; // �S�[��

    //private Rigidbody magnetRb;
    private Rigidbody cargoRb;
    private float countup = 0.0f;

    private Vector3 startAgentPos = Vector3.zero;
    private Vector3 startCargoPos = Vector3.zero;
    private float previousTargetDistance = 0.0f;
    private float previousCargoDistance = 0.0f;
    private bool m_bCurry = false;

    [Header("�ݒ�l")]
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
        // �����x���Z�b�g
        //magnetRb.velocity = Vector3.zero;
        cargoRb.velocity = Vector3.zero;
        cargoRb.angularVelocity = Vector3.zero;
        // �J�E���g���Z�b�g
        countup = 0.0f;
        //�^��ł���t���O�����Z�b�g
        m_bCurry = false;
        // �S�[���̈ʒu�������_����
        target.localPosition = new Vector3(
            Random.Range(-4.0f, 4.0f), 0f, Random.Range(-4.0f, 4.0f)
        );
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // �}�O�l�b�g�A�ו��A�S�[���̈ʒu�����ϑ�
        sensor.AddObservation(agent.position);
        sensor.AddObservation(cargo.position);
        sensor.AddObservation(target.position);

        // �ו��ƃ}�O�l�b�g�̋������ϑ�
        sensor.AddObservation(Vector3.Distance(agent.position, cargo.position));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // �s���̎擾 (���E�A�㉺�A�O��)
        var move = Vector3.zero;
        move.x = actions.ContinuousActions[0];
        move.y = actions.ContinuousActions[1];
        move.z = actions.ContinuousActions[2];

        // �}�O�l�b�g�̓���
        agent.localPosition += new Vector3(move.x, move.y, move.z) * movementSpeed * Time.deltaTime;

        float currentTargetDistance = Vector3.Distance(agent.position, target.position);
        float currentCargoDistance = Vector3.Distance(cargo.position, agent.position);
        // �ו��̋z������
        if (currentCargoDistance < 0.5f)
        {
            cargo.position = agent.position;
            m_bCurry = true;
            //Debug.Log("�ו����������I");
            AddReward(0.5f); // �z����V
        }
        if (m_bCurry==true)
        {
            //�O�t���[����苗�����߂��Ȃ��Ă�����
            if (currentTargetDistance < previousTargetDistance)
            {
                AddReward(0.05f); // �S�[���ɋ߂Â����тɏ�����V
                //Debug.Log("�߂Â��Ă���I");
            }
            else
            {
                AddReward(-0.05f); // ��������ƃy�i���e�B
                //Debug.Log("���������Ă���I");
            }
            previousTargetDistance = currentTargetDistance;
        }
        else
        {
            //�O�t���[����苗�����߂��Ȃ��Ă�����
            if (currentCargoDistance < previousCargoDistance)
            {
                AddReward(0.01f); // �S�[���ɋ߂Â����тɏ�����V
                //Debug.Log("�߂Â��Ă���I");
            }
            else
            {
                AddReward(-0.01f); // ��������ƃy�i���e�B
                //Debug.Log("���������Ă���I");
            }
            previousCargoDistance = currentCargoDistance;

        }

        // ��V�Ǝ��s�����̔���
        if (currentTargetDistance < 0.5f)
        {
            AddReward(10.0f); // �S�[���ɉ^�ׂ�
            Debug.Log("�S�[���I");
            EndEpisode();
        }
        if (cargo.position.y < -0.5f)
        {
            AddReward(-5.0f); // �ו����n�ʂɐG���
            Debug.Log("���Ƃ��Ă��܂����I");
            EndEpisode();
        }
        else if (countup >= timelimit)
        {
            AddReward(-1.0f);   //�K�莞�Ԃ܂łȂɂ��N����Ȃ�����
            //Debug.Log("�Ȃɂ��N����Ȃ������I");
            EndEpisode();
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // �蓮����̂��߂̃f�o�b�O����
        var action = actionsOut.ContinuousActions;
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        action[2] = Input.GetAxis("Vertical");
    }
}

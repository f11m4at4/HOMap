using UnityEngine;
using System.Collections;

public class EnemyControl : MonoBehaviour {

    enum EnemyState
    {
        Idle,
        Walk,
        Attack,
        Damage,
        Die,
    }
    EnemyState _state;
    NavMeshAgent agent;
    Animator anim;

    // Idle 상태에서를 유지하는 시간
    public float IDLE_WAIT_TIME = 2;
    float currentTime = 0;

    // 공격 대상 타겟
    Transform target;

    // 움직이는 속도 - 움직일때 속도와, 애니메이션 속도를 함께 제어한다.
    public float MoveSpeed = 1;
    // Attack 범위
    public float AttackRange = 2;


    // Use this for initialization
    IEnumerator Start () {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        _state = EnemyState.Idle;
        target = GameObject.Find("Tower").transform;

        while (Application.isPlaying)
        {
            yield return StartCoroutine(_state.ToString());
        }
	}
	
    // Idle 상태일때 처리
    // 일정시간 기다렸다가 목적지로 이동 상태로 전환
	IEnumerator Idle()
    {
        anim.SetTrigger("Idle");
        while(_state == EnemyState.Idle)
        {
            currentTime += Time.deltaTime;
            if(currentTime > IDLE_WAIT_TIME)
            {
                currentTime = 0;
                _state = EnemyState.Walk;
                yield break;
            }
            yield return new WaitForEndOfFrame();
        }
    }
    
    IEnumerator Walk()
    {
        anim.SetTrigger("Walk");
        
        while (_state == EnemyState.Walk)
        {
            anim.speed = MoveSpeed;
            agent.SetDestination(target.position);
            agent.speed = MoveSpeed;

            // enemy 와 target 의 위치가 일정 거리 안으로 좁혀지면 공격 상태로 전환
            if(Vector3.Distance(target.position, transform.position) < AttackRange)
            {
                _state = EnemyState.Attack;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    public void InformDelayTime()
    {
        delayStart = true;
    }
    bool delayStart = false;
    public float ATTACK_DELAY = 0.5f;
    IEnumerator Attack()
    {
        anim.speed = 1;
        currentTime = ATTACK_DELAY;
        anim.SetTrigger("Attack");
        agent.Stop();
        while (_state == EnemyState.Attack)
        {
            if (delayStart)
            {
                currentTime += Time.deltaTime;
                if (currentTime > ATTACK_DELAY)
                {
                    currentTime = 0;
                    anim.SetTrigger("Attack");
                    anim.SetInteger("AttackNumber", Random.RandomRange(0, 4));
                    delayStart = false;
                }
            }
            // enemy 와 target 의 위치가 일정 거리 안으로 좁혀지면 공격 상태로 전환
            if (Vector3.Distance(target.position, transform.position) > AttackRange)
            {
                _state = EnemyState.Walk;
            }
            yield return new WaitForEndOfFrame();
        }
    }
}

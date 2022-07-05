using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] List<Transform> patrollingPoints = new List<Transform>();
    int _currentPoint_patrolling = 0;

    [Header("Controller Variables")]
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float runSpeed = 8f;

    [Header("AI")]
    [SerializeField] float viewRadius = 15f;
    [SerializeField] float viewAngle = 90f;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] float meshResolution = 1f;
    [SerializeField] float edgeDistance = 0.5f;
    [SerializeField] int edgeiterations = 4;
    [SerializeField] float startWaitTime = 4f;
    [SerializeField] float timeToRotate = 2f;
    float _waitTime;
    float _timeToRotate;

    [Header("Health System")]
    [SerializeField] int health = 3;

    NavMeshAgent _agent;
    Rigidbody _rb;
    Animator _anim;

    Transform _player;

    Vector3 _playerLastPos = Vector3.zero;
    Vector3 _playerPos = Vector3.zero;

    bool _playerInRange;
    bool _playerNear;
    bool _isPatrol;
    bool _caughtPlayer;
    bool isAlive = true;
    bool canTakeDamage = true;
    bool canAttack = true;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponentInChildren<Animator>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        onStart();
    }
    private void Update()
    {
        if (isAlive)
        {
            enemySight();

            if (!_isPatrol)
            {
                chasePlayer();
            }
            else
            {
                doPatrolling();
            }
        }
    }

    void onStart()
    {
        _playerPos = Vector3.zero;
        _isPatrol = true;
        _caughtPlayer = false;
        _playerInRange = false;
        _waitTime = startWaitTime;
        _timeToRotate = timeToRotate;
        _currentPoint_patrolling = 0;

        _agent.isStopped = false;
        _agent.speed = walkSpeed;
        _agent.SetDestination(patrollingPoints[_currentPoint_patrolling].position);

    }

    void Move(float speed)
    {
        _agent.isStopped = false;
        _agent.speed = speed;
    }
    void Stop()
    {
        _agent.isStopped = true;
        _agent.speed = 0;
    }

    void lookingPlayer(Vector3 player)
    {
        _agent.SetDestination(player);

        if (Vector3.Distance(transform.position, player) <= 0.3f)
        {
            if (_waitTime <= 0)
            {
                _playerNear = false;
                Move(walkSpeed);
                _agent.SetDestination(patrollingPoints[_currentPoint_patrolling].position);
                _waitTime = startWaitTime;
                _timeToRotate = timeToRotate;
            }
            else
            {
                Stop();
                _waitTime -= Time.deltaTime;
            }
        }
    }

    void doPatrolling()
    {
        if (_playerNear)
        {
            if (_waitTime <= 0)
            {
                Move(walkSpeed);
                lookingPlayer(_playerLastPos);
            }
            else
            {
                Stop();
                _timeToRotate -= Time.deltaTime;
            }
        }
        else
        {
            _playerNear = false;
            _playerLastPos = Vector3.zero;
            _agent.SetDestination(patrollingPoints[_currentPoint_patrolling].position);

            if (_agent.remainingDistance <= _agent.stoppingDistance)
            {
                if (_waitTime <= 0)
                {
                    nextPatrollingPoint();
                    Move(walkSpeed);
                    _waitTime = startWaitTime;
                }
                else
                {
                    Stop();
                    _waitTime -= Time.deltaTime;
                }
            }
        }
    }
    void nextPatrollingPoint()
    {
        _currentPoint_patrolling = (_currentPoint_patrolling + 1) % patrollingPoints.Count;
        _agent.SetDestination(patrollingPoints[_currentPoint_patrolling].position);
    }

    void enemySight()
    {
        Collider[] playerinRange = Physics.OverlapSphere(transform.position, viewRadius, playerLayer);
        for (int i = 0; i < playerinRange.Length; i++)
        {
            Transform player = playerinRange[i].transform;
            Vector3 directionToPlayer = (player.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToPlayer) < viewAngle / 2)
            {
                float distToPlayer = Vector3.Distance(transform.position, player.position);

                if (!Physics.Raycast(transform.position, directionToPlayer, distToPlayer, obstacleLayer))
                {
                    _playerInRange = true;
                    _isPatrol = false;
                }
                else
                {
                    _playerInRange = false;
                }
            }

            if (Vector3.Distance(transform.position, player.position) > viewRadius)
            {
                _playerInRange = false;
            }

            if (_playerInRange)
            {
                _playerPos = player.transform.position;
            }
        }
    }

    void chasePlayer() 
    {
        _playerNear = false;
        _playerLastPos = Vector3.zero;

        if (!_caughtPlayer)
        {
            Move(runSpeed);
            _agent.SetDestination(_playerPos);
        }

        if(_agent.remainingDistance <= _agent.stoppingDistance )
        {
            if(_waitTime <=0 && !_caughtPlayer && Vector3.Distance(transform.position, _player.position)> viewRadius)
            {
                _isPatrol = true;
                _playerNear = false;
                Move(walkSpeed);
                _timeToRotate = timeToRotate;
                _waitTime = startWaitTime;
                _agent.SetDestination(patrollingPoints[_currentPoint_patrolling].position);
            }
            else
            {
                if(Vector3.Distance(transform.position,_player.position) >= 2.5f)
                {
                    Stop();

                    _waitTime -= Time.deltaTime;
                }
                else
                {

                    if (canAttack)
                    {
                        StartCoroutine(doAttack());
                    }
                }
            }
        }
    }
    void caughtPlayer()
    {
        _caughtPlayer = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Sword")
        {
            if (isAlive && canTakeDamage)
            {
                if (health > 1)
                {
                    StartCoroutine(onDamage());
                }
                else
                {
                    isAlive = false;
                    _anim.SetTrigger("Death");
                    Destroy(gameObject, 3f);
                }
            }
        }
    }

    IEnumerator onDamage()
    {
        canTakeDamage = false;
        health--;
        _anim.SetTrigger("Damage");
        yield return new WaitForSeconds(0.3f);
        canTakeDamage = true;
    }
    
    IEnumerator doAttack()
    {
        canAttack = false;
        _anim.SetTrigger("Attack");
        yield return new WaitForSeconds(0.2f);
        _player.GetComponent<playerController>().removeHealth();
        yield return new WaitForSeconds(0.8f);
        canAttack = true;
    }
}

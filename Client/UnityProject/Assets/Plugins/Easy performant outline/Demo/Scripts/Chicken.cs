using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if EPO_DOTWEEN
using DG.Tweening;
#endif

namespace EPOOutline.Demo
{
    public class Chicken : MonoBehaviour
    {
        [SerializeField]
        private bool alwaysActive = false;

        [SerializeField]
        private bool updateChicken = true;

        [SerializeField]
        private float searchRadius = 5.0f;

        private Outlinable outlinable;

        private NavMeshAgent agent;

        private Animator animator;

        private int enteredCount = 0;

        private static int priority = 0;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            outlinable = GetComponent<Outlinable>();
            animator = GetComponent<Animator>();
            if (!alwaysActive)
            { 
#if EPO_DOTWEEN
                outlinable.FrontParameters.DOFade(0.0f, 0.0f);
                outlinable.BackParameters.FillPass.DOFade("_PublicColor", 0.0f, 0.0f);
#else
                outlinable.enabled = false;
#endif
            }

            agent.avoidancePriority = priority++;

            if (updateChicken)
                StartCoroutine(UpdateChicken());
        }

        private void OnTriggerEnter(Collider other)
        {
            if (alwaysActive)
                return;

            if (!other.GetComponent<Character>())
                return;

#if EPO_DOTWEEN
            enteredCount++;
            outlinable.FrontParameters.DOKill(true);
            outlinable.FrontParameters.DOFade(1.0f, 0.5f);
            outlinable.BackParameters.FillPass.DOFade("_PublicColor", 0.5f, 0.5f);
#else
            outlinable.enabled = true;
#endif
        }

        private void OnTriggerExit(Collider other)
        {
            if (alwaysActive)
                return;

            if (!other.GetComponent<Character>())
                return;

            if (--enteredCount != 0)
                return;

#if EPO_DOTWEEN
            outlinable.FrontParameters.DOKill(true);
            outlinable.FrontParameters.DOFade(0.0f, 0.5f);
            outlinable.BackParameters.FillPass.DOFade("_PublicColor", 0.0f, 0.5f);
#else
            outlinable.enabled = false;
#endif
        }

        private IEnumerator UpdateChicken()
        {
            var path = new NavMeshPath();
            while (true)
            {
                animator.CrossFade("Walk In Place", 0.1f);

                var point = Random.insideUnitCircle;
                var shift = new Vector3(point.x, 0, point.y) * searchRadius;

                NavMeshHit hit;
                if (!NavMesh.SamplePosition(transform.position + shift, out hit, searchRadius, -1))
                {
                    yield return null;
                    continue;
                }

                Debug.DrawLine(transform.position, hit.position, Color.yellow, 3.0f);

                if (!NavMesh.CalculatePath(transform.position, hit.position, -1, path))
                {
                    yield return null;
                    continue;
                }

                agent.destination = hit.position;

                while (agent.pathStatus != NavMeshPathStatus.PathComplete)
                    yield return null;

                var timeToWait = (agent.remainingDistance / agent.speed) * 1.5f;
                while (agent.remainingDistance > agent.stoppingDistance && timeToWait > 0.0f)
                {
                    timeToWait -= Time.deltaTime;
                    yield return null;
                }

                animator.CrossFade("Eat", 0.1f);

                yield return new WaitForSeconds(Random.Range(1.0f, 5.0f));

                yield return null;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.2f);
            Gizmos.DrawSphere(transform.position, searchRadius);
        }
    }
}
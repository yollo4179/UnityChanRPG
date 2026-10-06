using UnityEngine;

public class SmaugBattleScript : MonsterBattleScript
{
    private enum MovementState
    {
        Chase,
        Stop
    }

    [SerializeField] private float _resumeChaseOffset = 0.4f;
    private MovementState _movementState;

    public override void UpdateBattle(bool isFirst)
    {
        Vector3 toTarget = VectorUtil.PlatVector(_target.position - transform.position);
        float distance = toTarget.magnitude;
        Vector3 targetDirection = distance > 0.001f ? toTarget / distance : Vector3.zero;

        // Hysteresis prevents a stop/chase loop at the stopping distance.
        if (_movementState == MovementState.Stop && distance > _rangeOffset + _resumeChaseOffset)
            _movementState = MovementState.Chase;
        else if (_movementState == MovementState.Chase && distance <= _rangeOffset)
            _movementState = MovementState.Stop;

        _nowState = _movementState == MovementState.Stop ? eNowState.Stop : eNowState.MoveF;
        if (_movementState == MovementState.Chase)
            transform.position += targetDirection * _monsterSpeed * Time.deltaTime;

        // Keep facing the player so angle-based attacks can start while stopped.
        if (targetDirection.sqrMagnitude > 0.001f)
        {
            Quaternion facing = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, facing, Time.deltaTime * 5f);
        }

        ChooseAnimation(targetDirection);
        _attackChecker.CheckAttackCondition();
    }
}

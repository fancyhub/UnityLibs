using UnityEditor;
using UnityEngine;

public class CharacterMoveSimple : MonoBehaviour
{
    [Header("Move")]
    public CharacterController targetCharacterController;
    public float moveSpeed = 5f;
    public float rotateSpeed = 10f;
    public float gravity = -9.8f;


    [Header("Animation(optional)")]
    public Animator targetAnimator;

    public enum EAnimMoveMode
    {
        AnimMove_1D,
        AnimMove_2D,
    }

    public EAnimMoveMode animMoveMode = EAnimMoveMode.AnimMove_1D;
    public string animMove1DParamName = "";
    public string animMove2DParamXName = "";
    public string animMove2DParamYName = "";

    public string animMoveTriggerName = "";

    [Min(0.1f)]
    public float animMoveParamMaxValue = 1;

    private Transform _mainCameraTrans;
    private int _animMove1DParamNameId = 0;
    private int _animMove2DParamXNameId = 0;
    private int _animMove2DParamYNameId = 0;
    private int _animMoveTriggerNameId = 0;


    private void Start()
    {
        if (!string.IsNullOrEmpty(animMove1DParamName))
            _animMove1DParamNameId = Animator.StringToHash(animMove1DParamName);

        if (!string.IsNullOrEmpty(animMoveTriggerName))
            _animMoveTriggerNameId = Animator.StringToHash(animMoveTriggerName);

        if (!string.IsNullOrEmpty(animMove2DParamXName))
            _animMove2DParamXNameId = Animator.StringToHash(animMove2DParamXName);

        if (!string.IsNullOrEmpty(animMove2DParamYName))
            _animMove2DParamYNameId = Animator.StringToHash(animMove2DParamYName);
    }


    private void Update()
    {
        if (targetCharacterController == null)
        {
            targetCharacterController = GetComponent<CharacterController>();
            if (targetCharacterController == null)
                return;
        }

        if (_mainCameraTrans == null)
        {
            if (Camera.main != null)
                _mainCameraTrans = Camera.main.transform;

            if (_mainCameraTrans == null)
                return;
        }

        Vector3 velocity = Vector3.zero;

        // 获取输入
        Vector2 inputDir = _GetInputDir();
        if (inputDir.sqrMagnitude > 0.01f)
        {
            Vector3 moveDir = _CalcMoveDir(inputDir, _mainCameraTrans);

            velocity = moveDir * moveSpeed;

            // 转向移动方向
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            targetCharacterController.transform.rotation = Quaternion.Lerp(targetCharacterController.transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            _UpdateAnim(true, velocity);
        }
        else
        {
            _UpdateAnim(false, Vector3.zero);
        }

        // 应用重力
        velocity.y += gravity * Time.deltaTime;
        targetCharacterController.Move(velocity * Time.deltaTime);
    }

    private void _UpdateAnim(bool walking, Vector3 velocity)
    {
        if (targetAnimator == null)
            return;

        if (_animMoveTriggerNameId != 0)
        {
            targetAnimator.SetBool(_animMoveTriggerNameId, walking);
        }


        if (animMoveMode == EAnimMoveMode.AnimMove_1D && _animMove1DParamNameId != 0)
        {
            if (walking)
                targetAnimator.SetFloat(_animMove1DParamNameId, animMoveParamMaxValue);
            else
                targetAnimator.SetFloat(_animMove1DParamNameId, 0);
        }
        else if (animMoveMode == EAnimMoveMode.AnimMove_2D && _animMove2DParamXNameId != 0 && _animMove2DParamYNameId != 0)
        {
            if (walking)
            {
                Vector3 localDir = targetCharacterController.transform.InverseTransformDirection(Vector3.Normalize(velocity));
                localDir.y = 0;
                localDir = Vector3.Normalize(localDir) * animMoveParamMaxValue;


                targetAnimator.SetFloat(_animMove2DParamXNameId, localDir.x);
                targetAnimator.SetFloat(_animMove2DParamYNameId, localDir.z);
            }
            else
            {
                targetAnimator.SetFloat(_animMove2DParamXNameId, 0);
                targetAnimator.SetFloat(_animMove2DParamYNameId, 0);
            }
        }
    }


    private static Vector3 _CalcMoveDir(Vector2 inputDir, Transform cameraTran)
    {
        Vector3 forward = cameraTran.forward;
        Vector3 right = cameraTran.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = forward * inputDir.y + right * inputDir.x;
        return moveDir;
    }

    // 获取输入
    private static Vector2 _GetInputDir()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        return new Vector2(h, v).normalized;
    }
}
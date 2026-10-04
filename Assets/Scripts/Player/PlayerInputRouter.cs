using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerInputRouter : MonoBehaviour
{
    private PlayerInputActions inputActions;

    public InputAction MoveAction =>
        GetActions().Player.Move;

    public InputAction JumpAction =>
        GetActions().Player.Jump;

    public InputAction PrimaryAttackAction =>
        GetActions().Player.PrimaryAttack;

    public InputAction Skill1Action =>
        GetActions().Player.Skill1;

    public InputAction Skill2Action =>
        GetActions().Player.Skill2;

    public InputAction Skill3Action =>
        GetActions().Player.Skill3;

    public InputAction Skill4Action =>
        GetActions().Player.Skill4;
    public InputAction CameraLockAction =>
        GetActions().Player.CameraLock;

    public InputAction SprintAction =>
        GetActions().Player.Sprint;

    public InputAction DodgeAction =>
        GetActions().Player.Dodge;

    public InputAction SheatheUnsheatheAction =>
        GetActions().Player.SheatheUnsheathe;

    public InputAction SwitchWeaponAction =>
        GetActions().Player.SwitchWeapon;

    public InputAction InteractAction =>
        GetActions().Player.Interact;

    public InputAction InteractionCycleAction =>
        GetActions().Player.Scroll;

    public InputAction InventoryAction =>
        GetActions().Player.Inventory;

    public InputAction RotateItemAction =>
        GetActions().Player.ReloadRotateItem;

    public InputAction DropAction =>
        GetActions().Player.Drop;

    private void Awake()
    {
        EnsureActions();
    }

    private void OnEnable()
    {
        GetActions().Player.Enable();
    }

    private void OnDisable()
    {
        if (inputActions != null)
            inputActions.Player.Disable();
    }

    private void OnDestroy()
    {
        if (inputActions == null)
            return;

        inputActions.Dispose();
        inputActions = null;
    }

    private PlayerInputActions GetActions()
    {
        EnsureActions();
        return inputActions;
    }

    private void EnsureActions()
    {
        if (inputActions == null)
            inputActions = new PlayerInputActions();
    }
}
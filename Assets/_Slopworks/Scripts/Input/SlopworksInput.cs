// GENERATED AUTOMATICALLY FROM 'Assets/_Slopworks/Scripts/Input/SlopworksInput.inputactions'
//
// com.unity.inputsystem:InputActionCodeGenerator version 1.18.0

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class SlopworksControls : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }

    // Combat
    private readonly InputActionMap m_Combat;
    private readonly InputAction m_Combat_Move;
    private readonly InputAction m_Combat_Look;
    private readonly InputAction m_Combat_Jump;
    private readonly InputAction m_Combat_Sprint;
    private readonly InputAction m_Combat_Interact;
    private readonly InputAction m_Combat_Fire;
    private readonly InputAction m_Combat_Aim;
    private readonly InputAction m_Combat_InventoryOpen;
    private readonly InputAction m_Combat_SwitchIsometric;
    private readonly InputAction m_Combat_Reload;
    private readonly InputAction m_Combat_ToggleBuildMode;

    // Command
    private readonly InputActionMap m_Command;
    private readonly InputAction m_Command_CameraPan;
    private readonly InputAction m_Command_CameraZoom;
    private readonly InputAction m_Command_CameraRotate;
    private readonly InputAction m_Command_PlaceSelect;
    private readonly InputAction m_Command_PlaceRotate;
    private readonly InputAction m_Command_PlaceCancel;
    private readonly InputAction m_Command_InventoryOpen;
    private readonly InputAction m_Command_UINavigate;
    private readonly InputAction m_Command_SwitchFPS;

    // Build
    private readonly InputActionMap m_Build;
    private readonly InputAction m_Build_ToggleBuildMode;
    private readonly InputAction m_Build_SelectTool1;
    private readonly InputAction m_Build_SelectTool2;
    private readonly InputAction m_Build_SelectTool3;
    private readonly InputAction m_Build_SelectTool4;
    private readonly InputAction m_Build_SelectTool5;
    private readonly InputAction m_Build_SelectTool6;
    private readonly InputAction m_Build_Rotate;
    private readonly InputAction m_Build_DeleteMode;
    private readonly InputAction m_Build_ZoopMode;
    private readonly InputAction m_Build_GridOverlay;
    private readonly InputAction m_Build_CycleVariant;
    private readonly InputAction m_Build_NudgeUp;
    private readonly InputAction m_Build_NudgeDown;
    private readonly InputAction m_Build_Place;
    private readonly InputAction m_Build_Remove;
    private readonly InputAction m_Build_Cancel;
    private readonly InputAction m_Build_MachineInteract;
    private readonly InputAction m_Build_SnapFilterToggle;
    private readonly InputAction m_Build_DebugDump;

    public SlopworksControls()
    {
        asset = InputActionAsset.FromJson(@"{""name"":""SlopworksInput"",""maps"":[{""name"":""Combat"",""id"":""a1b2c3d4-1111-4000-8000-000000000001"",""actions"":[{""name"":""Move"",""type"":""Value"",""id"":""a1b2c3d4-2222-4000-8000-000000000001"",""expectedControlType"":""Vector2"",""processors"":"""",""interactions"":"""",""initialStateCheck"":true},{""name"":""Look"",""type"":""Value"",""id"":""a1b2c3d4-2222-4000-8000-000000000002"",""expectedControlType"":""Vector2"",""processors"":"""",""interactions"":"""",""initialStateCheck"":true},{""name"":""Jump"",""type"":""Button"",""id"":""a1b2c3d4-2222-4000-8000-000000000003"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""Sprint"",""type"":""Button"",""id"":""a1b2c3d4-2222-4000-8000-000000000004"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""Interact"",""type"":""Button"",""id"":""a1b2c3d4-2222-4000-8000-000000000005"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""Fire"",""type"":""Button"",""id"":""a1b2c3d4-2222-4000-8000-000000000006"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""Aim"",""type"":""Button"",""id"":""a1b2c3d4-2222-4000-8000-000000000007"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""InventoryOpen"",""type"":""Button"",""id"":""a1b2c3d4-2222-4000-8000-000000000008"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""SwitchIsometric"",""type"":""Button"",""id"":""a1b2c3d4-2222-4000-8000-000000000009"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""Reload"",""type"":""Button"",""id"":""a1b2c3d4-2222-4000-8000-00000000000a"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""ToggleBuildMode"",""type"":""Button"",""id"":""a1b2c3d4-2222-4000-8000-00000000000b"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""}],""bindings"":[{""name"":""WASD"",""id"":""b1000001-0000-4000-8000-000000000001"",""path"":""2DVector"",""interactions"":"""",""processors"":"""",""groups"":"""",""action"":""Move"",""isComposite"":true,""isPartOfComposite"":false},{""name"":""up"",""id"":""b1000001-0000-4000-8000-000000000002"",""path"":""<Keyboard>/w"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""Move"",""isComposite"":false,""isPartOfComposite"":true},{""name"":""down"",""id"":""b1000001-0000-4000-8000-000000000003"",""path"":""<Keyboard>/s"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""Move"",""isComposite"":false,""isPartOfComposite"":true},{""name"":""left"",""id"":""b1000001-0000-4000-8000-000000000004"",""path"":""<Keyboard>/a"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""Move"",""isComposite"":false,""isPartOfComposite"":true},{""name"":""right"",""id"":""b1000001-0000-4000-8000-000000000005"",""path"":""<Keyboard>/d"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""Move"",""isComposite"":false,""isPartOfComposite"":true},{""name"":""Left Stick"",""id"":""b1000001-0000-4000-8000-000000000006"",""path"":""2DVector(mode=2)"",""interactions"":"""",""processors"":"""",""groups"":"""",""action"":""Move"",""isComposite"":true,""isPartOfComposite"":false},{""name"":""up"",""id"":""b1000001-0000-4000-8000-000000000007"",""path"":""<Gamepad>/leftStick/up"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""Move"",""isComposite"":false,""isPartOfComposite"":true},{""name"":""down"",""id"":""b1000001-0000-4000-8000-000000000008"",""path"":""<Gamepad>/leftStick/down"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""Move"",""isComposite"":false,""isPartOfComposite"":true},{""name"":""left"",""id"":""b1000001-0000-4000-8000-000000000009"",""path"":""<Gamepad>/leftStick/left"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""Move"",""isComposite"":false,""isPartOfComposite"":true},{""name"":""right"",""id"":""b1000001-0000-4000-8000-00000000000a"",""path"":""<Gamepad>/leftStick/right"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""Move"",""isComposite"":false,""isPartOfComposite"":true},{""name"":"""",""id"":""b2000001-0000-4000-8000-000000000001"",""path"":""<Mouse>/delta"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""Look"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""b2000001-0000-4000-8000-000000000002"",""path"":""<Gamepad>/rightStick"",""interactions"":"""",""processors"":""StickDeadzone,ScaleVector2(x=150,y=150)"",""groups"":""Gamepad"",""action"":""Look"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""b3000001-0000-4000-8000-000000000001"",""path"":""<Keyboard>/space"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""Jump"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""b3000001-0000-4000-8000-000000000002"",""path"":""<Gamepad>/buttonSouth"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""Jump"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""b4000001-0000-4000-8000-000000000001"",""path"":""<Keyboard>/leftShift"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""Sprint"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""b4000001-0000-4000-8000-000000000002"",""path"":""<Gamepad>/leftStickPress"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""Sprint"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""b5000001-0000-4000-8000-000000000001"",""path"":""<Keyboard>/e"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""Interact"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""b5000001-0000-4000-8000-000000000002"",""path"":""<Gamepad>/buttonWest"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""Interact"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""b6000001-0000-4000-8000-000000000001"",""path"":""<Mouse>/leftButton"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""Fire"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""b6000001-0000-4000-8000-000000000002"",""path"":""<Gamepad>/rightTrigger"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""Fire"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""b7000001-0000-4000-8000-000000000001"",""path"":""<Mouse>/rightButton"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""Aim"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""b7000001-0000-4000-8000-000000000002"",""path"":""<Gamepad>/leftTrigger"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""Aim"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""b8000001-0000-4000-8000-000000000001"",""path"":""<Keyboard>/tab"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""InventoryOpen"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""b8000001-0000-4000-8000-000000000002"",""path"":""<Gamepad>/start"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""InventoryOpen"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""b9000001-0000-4000-8000-000000000001"",""path"":""<Keyboard>/v"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""SwitchIsometric"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""b9000001-0000-4000-8000-000000000002"",""path"":""<Gamepad>/select"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""SwitchIsometric"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""ba000001-0000-4000-8000-000000000001"",""path"":""<Keyboard>/r"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""Reload"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""ba000001-0000-4000-8000-000000000002"",""path"":""<Gamepad>/buttonNorth"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""Reload"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""bb000001-0000-4000-8000-000000000001"",""path"":""<Keyboard>/b"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""ToggleBuildMode"",""isComposite"":false,""isPartOfComposite"":false}]},{""name"":""Command"",""id"":""a1b2c3d4-1111-4000-8000-000000000002"",""actions"":[{""name"":""CameraPan"",""type"":""Value"",""id"":""c1000001-0000-4000-8000-000000000001"",""expectedControlType"":""Vector2"",""processors"":"""",""interactions"":"""",""initialStateCheck"":true},{""name"":""CameraZoom"",""type"":""Value"",""id"":""c1000001-0000-4000-8000-000000000002"",""expectedControlType"":""Axis"",""processors"":"""",""interactions"":"""",""initialStateCheck"":false},{""name"":""CameraRotate"",""type"":""Value"",""id"":""c1000001-0000-4000-8000-000000000003"",""expectedControlType"":""Vector2"",""processors"":"""",""interactions"":"""",""initialStateCheck"":true},{""name"":""PlaceSelect"",""type"":""Button"",""id"":""c1000001-0000-4000-8000-000000000004"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""PlaceRotate"",""type"":""Button"",""id"":""c1000001-0000-4000-8000-000000000005"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""PlaceCancel"",""type"":""Button"",""id"":""c1000001-0000-4000-8000-000000000006"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""InventoryOpen"",""type"":""Button"",""id"":""c1000001-0000-4000-8000-000000000007"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""UINavigate"",""type"":""Value"",""id"":""c1000001-0000-4000-8000-000000000008"",""expectedControlType"":""Vector2"",""processors"":"""",""interactions"":"""",""initialStateCheck"":true},{""name"":""SwitchFPS"",""type"":""Button"",""id"":""c1000001-0000-4000-8000-000000000009"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""}],""bindings"":[{""name"":""WASD"",""id"":""d1000001-0000-4000-8000-000000000001"",""path"":""2DVector"",""interactions"":"""",""processors"":"""",""groups"":"""",""action"":""CameraPan"",""isComposite"":true,""isPartOfComposite"":false},{""name"":""up"",""id"":""d1000001-0000-4000-8000-000000000002"",""path"":""<Keyboard>/w"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""CameraPan"",""isComposite"":false,""isPartOfComposite"":true},{""name"":""down"",""id"":""d1000001-0000-4000-8000-000000000003"",""path"":""<Keyboard>/s"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""CameraPan"",""isComposite"":false,""isPartOfComposite"":true},{""name"":""left"",""id"":""d1000001-0000-4000-8000-000000000004"",""path"":""<Keyboard>/a"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""CameraPan"",""isComposite"":false,""isPartOfComposite"":true},{""name"":""right"",""id"":""d1000001-0000-4000-8000-000000000005"",""path"":""<Keyboard>/d"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""CameraPan"",""isComposite"":false,""isPartOfComposite"":true},{""name"":"""",""id"":""d2000001-0000-4000-8000-000000000001"",""path"":""<Mouse>/scroll/y"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""CameraZoom"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""d3000001-0000-4000-8000-000000000001"",""path"":""<Mouse>/middleButton"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""CameraRotate"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""d3000001-0000-4000-8000-000000000002"",""path"":""<Gamepad>/rightStick"",""interactions"":"""",""processors"":""StickDeadzone"",""groups"":""Gamepad"",""action"":""CameraRotate"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""d4000001-0000-4000-8000-000000000001"",""path"":""<Mouse>/leftButton"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""PlaceSelect"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""d4000001-0000-4000-8000-000000000002"",""path"":""<Gamepad>/buttonSouth"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""PlaceSelect"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""d5000001-0000-4000-8000-000000000001"",""path"":""<Keyboard>/r"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""PlaceRotate"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""d5000001-0000-4000-8000-000000000002"",""path"":""<Gamepad>/buttonNorth"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""PlaceRotate"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""d6000001-0000-4000-8000-000000000001"",""path"":""<Keyboard>/escape"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""PlaceCancel"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""d6000001-0000-4000-8000-000000000002"",""path"":""<Gamepad>/buttonEast"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""PlaceCancel"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""d7000001-0000-4000-8000-000000000001"",""path"":""<Keyboard>/tab"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""InventoryOpen"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""d7000001-0000-4000-8000-000000000002"",""path"":""<Gamepad>/start"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""InventoryOpen"",""isComposite"":false,""isPartOfComposite"":false},{""name"":""Arrow Keys"",""id"":""d8000001-0000-4000-8000-000000000001"",""path"":""2DVector"",""interactions"":"""",""processors"":"""",""groups"":"""",""action"":""UINavigate"",""isComposite"":true,""isPartOfComposite"":false},{""name"":""up"",""id"":""d8000001-0000-4000-8000-000000000002"",""path"":""<Keyboard>/upArrow"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""UINavigate"",""isComposite"":false,""isPartOfComposite"":true},{""name"":""down"",""id"":""d8000001-0000-4000-8000-000000000003"",""path"":""<Keyboard>/downArrow"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""UINavigate"",""isComposite"":false,""isPartOfComposite"":true},{""name"":""left"",""id"":""d8000001-0000-4000-8000-000000000004"",""path"":""<Keyboard>/leftArrow"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""UINavigate"",""isComposite"":false,""isPartOfComposite"":true},{""name"":""right"",""id"":""d8000001-0000-4000-8000-000000000005"",""path"":""<Keyboard>/rightArrow"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""UINavigate"",""isComposite"":false,""isPartOfComposite"":true},{""name"":"""",""id"":""d8000001-0000-4000-8000-000000000006"",""path"":""<Gamepad>/dpad"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""UINavigate"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""d9000001-0000-4000-8000-000000000001"",""path"":""<Keyboard>/v"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""SwitchFPS"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""d9000001-0000-4000-8000-000000000002"",""path"":""<Gamepad>/select"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""SwitchFPS"",""isComposite"":false,""isPartOfComposite"":false}]},{""name"":""Build"",""id"":""a1b2c3d4-1111-4000-8000-000000000003"",""actions"":[{""name"":""ToggleBuildMode"",""type"":""Button"",""id"":""e1000001-0000-4000-8000-000000000001"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""SelectTool1"",""type"":""Button"",""id"":""e1000001-0000-4000-8000-000000000002"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""SelectTool2"",""type"":""Button"",""id"":""e1000001-0000-4000-8000-000000000003"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""SelectTool3"",""type"":""Button"",""id"":""e1000001-0000-4000-8000-000000000004"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""SelectTool4"",""type"":""Button"",""id"":""e1000001-0000-4000-8000-000000000005"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""SelectTool5"",""type"":""Button"",""id"":""e1000001-0000-4000-8000-000000000006"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""SelectTool6"",""type"":""Button"",""id"":""e1000001-0000-4000-8000-000000000007"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""Rotate"",""type"":""Button"",""id"":""e1000001-0000-4000-8000-000000000008"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""DeleteMode"",""type"":""Button"",""id"":""e1000001-0000-4000-8000-000000000009"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""ZoopMode"",""type"":""Button"",""id"":""e1000001-0000-4000-8000-00000000000a"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""GridOverlay"",""type"":""Button"",""id"":""e1000001-0000-4000-8000-00000000000b"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""CycleVariant"",""type"":""Button"",""id"":""e1000001-0000-4000-8000-00000000000c"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""NudgeUp"",""type"":""Button"",""id"":""e1000001-0000-4000-8000-00000000000d"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""NudgeDown"",""type"":""Button"",""id"":""e1000001-0000-4000-8000-00000000000e"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""Place"",""type"":""Button"",""id"":""e1000001-0000-4000-8000-00000000000f"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""Remove"",""type"":""Button"",""id"":""e1000001-0000-4000-8000-000000000010"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""Cancel"",""type"":""Button"",""id"":""e1000001-0000-4000-8000-000000000011"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""MachineInteract"",""type"":""Button"",""id"":""e1000001-0000-4000-8000-000000000012"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""},{""name"":""SnapFilterToggle"",""type"":""Value"",""id"":""e1000001-0000-4000-8000-000000000013"",""expectedControlType"":""Axis"",""processors"":"""",""interactions"":"""",""initialStateCheck"":false},{""name"":""DebugDump"",""type"":""Button"",""id"":""e1000001-0000-4000-8000-000000000014"",""expectedControlType"":""Button"",""processors"":"""",""interactions"":""""}],""bindings"":[{""name"":"""",""id"":""e2000001-0000-4000-8000-000000000001"",""path"":""<Keyboard>/b"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""ToggleBuildMode"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-000000000002"",""path"":""<Keyboard>/1"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""SelectTool1"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-000000000003"",""path"":""<Keyboard>/2"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""SelectTool2"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-000000000004"",""path"":""<Keyboard>/3"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""SelectTool3"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-000000000005"",""path"":""<Keyboard>/4"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""SelectTool4"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-000000000006"",""path"":""<Keyboard>/5"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""SelectTool5"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-000000000007"",""path"":""<Keyboard>/6"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""SelectTool6"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-000000000008"",""path"":""<Keyboard>/r"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""Rotate"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-000000000009"",""path"":""<Gamepad>/buttonNorth"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""Rotate"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-00000000000a"",""path"":""<Keyboard>/x"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""DeleteMode"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-00000000000b"",""path"":""<Keyboard>/z"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""ZoopMode"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-00000000000c"",""path"":""<Keyboard>/g"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""GridOverlay"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-00000000000d"",""path"":""<Keyboard>/tab"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""CycleVariant"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-00000000000e"",""path"":""<Keyboard>/pageUp"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""NudgeUp"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-00000000000f"",""path"":""<Keyboard>/pageDown"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""NudgeDown"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-000000000010"",""path"":""<Mouse>/leftButton"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""Place"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-000000000011"",""path"":""<Gamepad>/buttonSouth"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""Place"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-000000000012"",""path"":""<Mouse>/rightButton"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""Remove"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-000000000013"",""path"":""<Keyboard>/escape"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""Cancel"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-000000000014"",""path"":""<Gamepad>/buttonEast"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""Cancel"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-000000000015"",""path"":""<Keyboard>/f"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""MachineInteract"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-000000000016"",""path"":""<Gamepad>/buttonWest"",""interactions"":"""",""processors"":"""",""groups"":""Gamepad"",""action"":""MachineInteract"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-000000000017"",""path"":""<Mouse>/scroll/y"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""SnapFilterToggle"",""isComposite"":false,""isPartOfComposite"":false},{""name"":"""",""id"":""e2000001-0000-4000-8000-000000000018"",""path"":""<Keyboard>/0"",""interactions"":"""",""processors"":"""",""groups"":""Keyboard&Mouse"",""action"":""DebugDump"",""isComposite"":false,""isPartOfComposite"":false}]}],""controlSchemes"":[{""name"":""Keyboard&Mouse"",""bindingGroup"":""Keyboard&Mouse"",""devices"":[{""devicePath"":""<Keyboard>"",""isOptional"":false,""isOR"":false},{""devicePath"":""<Mouse>"",""isOptional"":false,""isOR"":false}]},{""name"":""Gamepad"",""bindingGroup"":""Gamepad"",""devices"":[{""devicePath"":""<Gamepad>"",""isOptional"":false,""isOR"":false}]}]}");

        // Combat
        m_Combat = asset.FindActionMap("Combat", throwIfNotFound: true);
        m_Combat_Move = m_Combat.FindAction("Move", throwIfNotFound: true);
        m_Combat_Look = m_Combat.FindAction("Look", throwIfNotFound: true);
        m_Combat_Jump = m_Combat.FindAction("Jump", throwIfNotFound: true);
        m_Combat_Sprint = m_Combat.FindAction("Sprint", throwIfNotFound: true);
        m_Combat_Interact = m_Combat.FindAction("Interact", throwIfNotFound: true);
        m_Combat_Fire = m_Combat.FindAction("Fire", throwIfNotFound: true);
        m_Combat_Aim = m_Combat.FindAction("Aim", throwIfNotFound: true);
        m_Combat_InventoryOpen = m_Combat.FindAction("InventoryOpen", throwIfNotFound: true);
        m_Combat_SwitchIsometric = m_Combat.FindAction("SwitchIsometric", throwIfNotFound: true);
        m_Combat_Reload = m_Combat.FindAction("Reload", throwIfNotFound: true);
        m_Combat_ToggleBuildMode = m_Combat.FindAction("ToggleBuildMode", throwIfNotFound: true);

        // Command
        m_Command = asset.FindActionMap("Command", throwIfNotFound: true);
        m_Command_CameraPan = m_Command.FindAction("CameraPan", throwIfNotFound: true);
        m_Command_CameraZoom = m_Command.FindAction("CameraZoom", throwIfNotFound: true);
        m_Command_CameraRotate = m_Command.FindAction("CameraRotate", throwIfNotFound: true);
        m_Command_PlaceSelect = m_Command.FindAction("PlaceSelect", throwIfNotFound: true);
        m_Command_PlaceRotate = m_Command.FindAction("PlaceRotate", throwIfNotFound: true);
        m_Command_PlaceCancel = m_Command.FindAction("PlaceCancel", throwIfNotFound: true);
        m_Command_InventoryOpen = m_Command.FindAction("InventoryOpen", throwIfNotFound: true);
        m_Command_UINavigate = m_Command.FindAction("UINavigate", throwIfNotFound: true);
        m_Command_SwitchFPS = m_Command.FindAction("SwitchFPS", throwIfNotFound: true);

        // Build
        m_Build = asset.FindActionMap("Build", throwIfNotFound: true);
        m_Build_ToggleBuildMode = m_Build.FindAction("ToggleBuildMode", throwIfNotFound: true);
        m_Build_SelectTool1 = m_Build.FindAction("SelectTool1", throwIfNotFound: true);
        m_Build_SelectTool2 = m_Build.FindAction("SelectTool2", throwIfNotFound: true);
        m_Build_SelectTool3 = m_Build.FindAction("SelectTool3", throwIfNotFound: true);
        m_Build_SelectTool4 = m_Build.FindAction("SelectTool4", throwIfNotFound: true);
        m_Build_SelectTool5 = m_Build.FindAction("SelectTool5", throwIfNotFound: true);
        m_Build_SelectTool6 = m_Build.FindAction("SelectTool6", throwIfNotFound: true);
        m_Build_Rotate = m_Build.FindAction("Rotate", throwIfNotFound: true);
        m_Build_DeleteMode = m_Build.FindAction("DeleteMode", throwIfNotFound: true);
        m_Build_ZoopMode = m_Build.FindAction("ZoopMode", throwIfNotFound: true);
        m_Build_GridOverlay = m_Build.FindAction("GridOverlay", throwIfNotFound: true);
        m_Build_CycleVariant = m_Build.FindAction("CycleVariant", throwIfNotFound: true);
        m_Build_NudgeUp = m_Build.FindAction("NudgeUp", throwIfNotFound: true);
        m_Build_NudgeDown = m_Build.FindAction("NudgeDown", throwIfNotFound: true);
        m_Build_Place = m_Build.FindAction("Place", throwIfNotFound: true);
        m_Build_Remove = m_Build.FindAction("Remove", throwIfNotFound: true);
        m_Build_Cancel = m_Build.FindAction("Cancel", throwIfNotFound: true);
        m_Build_MachineInteract = m_Build.FindAction("MachineInteract", throwIfNotFound: true);
        m_Build_SnapFilterToggle = m_Build.FindAction("SnapFilterToggle", throwIfNotFound: true);
        m_Build_DebugDump = m_Build.FindAction("DebugDump", throwIfNotFound: true);
    }

    ~SlopworksControls()
    {
        UnityEngine.Debug.Assert(!m_Combat.enabled, "This will cause a leak and performance issues, SlopworksControls.Combat.Disable() has not been called.");
        UnityEngine.Debug.Assert(!m_Command.enabled, "This will cause a leak and performance issues, SlopworksControls.Command.Disable() has not been called.");
        UnityEngine.Debug.Assert(!m_Build.enabled, "This will cause a leak and performance issues, SlopworksControls.Build.Disable() has not been called.");
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Control schemes
    private int m_KeyboardMouseSchemeIndex = -1;
    public InputControlScheme KeyboardMouseScheme
    {
        get
        {
            if (m_KeyboardMouseSchemeIndex == -1)
                m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard&Mouse");
            return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
        }
    }

    private int m_GamepadSchemeIndex = -1;
    public InputControlScheme GamepadScheme
    {
        get
        {
            if (m_GamepadSchemeIndex == -1)
                m_GamepadSchemeIndex = asset.FindControlSchemeIndex("Gamepad");
            return asset.controlSchemes[m_GamepadSchemeIndex];
        }
    }

    // Combat action map
    public struct CombatActions
    {
        private SlopworksControls m_Wrapper;
        public CombatActions(SlopworksControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Combat_Move;
        public InputAction @Look => m_Wrapper.m_Combat_Look;
        public InputAction @Jump => m_Wrapper.m_Combat_Jump;
        public InputAction @Sprint => m_Wrapper.m_Combat_Sprint;
        public InputAction @Interact => m_Wrapper.m_Combat_Interact;
        public InputAction @Fire => m_Wrapper.m_Combat_Fire;
        public InputAction @Aim => m_Wrapper.m_Combat_Aim;
        public InputAction @InventoryOpen => m_Wrapper.m_Combat_InventoryOpen;
        public InputAction @SwitchIsometric => m_Wrapper.m_Combat_SwitchIsometric;
        public InputAction @Reload => m_Wrapper.m_Combat_Reload;
        public InputAction @ToggleBuildMode => m_Wrapper.m_Combat_ToggleBuildMode;
        public InputActionMap Get() { return m_Wrapper.m_Combat; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CombatActions set) { return set.Get(); }

        public void AddCallbacks(ICombatActions instance)
        {
            if (instance == null || m_Wrapper.m_CombatActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_CombatActionsCallbackInterfaces.Add(instance);
            @Move.started += instance.OnMove;
            @Move.performed += instance.OnMove;
            @Move.canceled += instance.OnMove;
            @Look.started += instance.OnLook;
            @Look.performed += instance.OnLook;
            @Look.canceled += instance.OnLook;
            @Jump.started += instance.OnJump;
            @Jump.performed += instance.OnJump;
            @Jump.canceled += instance.OnJump;
            @Sprint.started += instance.OnSprint;
            @Sprint.performed += instance.OnSprint;
            @Sprint.canceled += instance.OnSprint;
            @Interact.started += instance.OnInteract;
            @Interact.performed += instance.OnInteract;
            @Interact.canceled += instance.OnInteract;
            @Fire.started += instance.OnFire;
            @Fire.performed += instance.OnFire;
            @Fire.canceled += instance.OnFire;
            @Aim.started += instance.OnAim;
            @Aim.performed += instance.OnAim;
            @Aim.canceled += instance.OnAim;
            @InventoryOpen.started += instance.OnInventoryOpen;
            @InventoryOpen.performed += instance.OnInventoryOpen;
            @InventoryOpen.canceled += instance.OnInventoryOpen;
            @SwitchIsometric.started += instance.OnSwitchIsometric;
            @SwitchIsometric.performed += instance.OnSwitchIsometric;
            @SwitchIsometric.canceled += instance.OnSwitchIsometric;
            @Reload.started += instance.OnReload;
            @Reload.performed += instance.OnReload;
            @Reload.canceled += instance.OnReload;
            @ToggleBuildMode.started += instance.OnToggleBuildMode;
            @ToggleBuildMode.performed += instance.OnToggleBuildMode;
            @ToggleBuildMode.canceled += instance.OnToggleBuildMode;
        }

        private void UnregisterCallbacks(ICombatActions instance)
        {
            @Move.started -= instance.OnMove;
            @Move.performed -= instance.OnMove;
            @Move.canceled -= instance.OnMove;
            @Look.started -= instance.OnLook;
            @Look.performed -= instance.OnLook;
            @Look.canceled -= instance.OnLook;
            @Jump.started -= instance.OnJump;
            @Jump.performed -= instance.OnJump;
            @Jump.canceled -= instance.OnJump;
            @Sprint.started -= instance.OnSprint;
            @Sprint.performed -= instance.OnSprint;
            @Sprint.canceled -= instance.OnSprint;
            @Interact.started -= instance.OnInteract;
            @Interact.performed -= instance.OnInteract;
            @Interact.canceled -= instance.OnInteract;
            @Fire.started -= instance.OnFire;
            @Fire.performed -= instance.OnFire;
            @Fire.canceled -= instance.OnFire;
            @Aim.started -= instance.OnAim;
            @Aim.performed -= instance.OnAim;
            @Aim.canceled -= instance.OnAim;
            @InventoryOpen.started -= instance.OnInventoryOpen;
            @InventoryOpen.performed -= instance.OnInventoryOpen;
            @InventoryOpen.canceled -= instance.OnInventoryOpen;
            @SwitchIsometric.started -= instance.OnSwitchIsometric;
            @SwitchIsometric.performed -= instance.OnSwitchIsometric;
            @SwitchIsometric.canceled -= instance.OnSwitchIsometric;
            @Reload.started -= instance.OnReload;
            @Reload.performed -= instance.OnReload;
            @Reload.canceled -= instance.OnReload;
            @ToggleBuildMode.started -= instance.OnToggleBuildMode;
            @ToggleBuildMode.performed -= instance.OnToggleBuildMode;
            @ToggleBuildMode.canceled -= instance.OnToggleBuildMode;
        }

        public void RemoveCallbacks(ICombatActions instance)
        {
            if (m_Wrapper.m_CombatActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(ICombatActions instance)
        {
            foreach (var item in m_Wrapper.m_CombatActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_CombatActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }

    public CombatActions @Combat => new CombatActions(this);
    private List<ICombatActions> m_CombatActionsCallbackInterfaces = new List<ICombatActions>();

    // Command action map
    public struct CommandActions
    {
        private SlopworksControls m_Wrapper;
        public CommandActions(SlopworksControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @CameraPan => m_Wrapper.m_Command_CameraPan;
        public InputAction @CameraZoom => m_Wrapper.m_Command_CameraZoom;
        public InputAction @CameraRotate => m_Wrapper.m_Command_CameraRotate;
        public InputAction @PlaceSelect => m_Wrapper.m_Command_PlaceSelect;
        public InputAction @PlaceRotate => m_Wrapper.m_Command_PlaceRotate;
        public InputAction @PlaceCancel => m_Wrapper.m_Command_PlaceCancel;
        public InputAction @InventoryOpen => m_Wrapper.m_Command_InventoryOpen;
        public InputAction @UINavigate => m_Wrapper.m_Command_UINavigate;
        public InputAction @SwitchFPS => m_Wrapper.m_Command_SwitchFPS;
        public InputActionMap Get() { return m_Wrapper.m_Command; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CommandActions set) { return set.Get(); }

        public void AddCallbacks(ICommandActions instance)
        {
            if (instance == null || m_Wrapper.m_CommandActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_CommandActionsCallbackInterfaces.Add(instance);
            @CameraPan.started += instance.OnCameraPan;
            @CameraPan.performed += instance.OnCameraPan;
            @CameraPan.canceled += instance.OnCameraPan;
            @CameraZoom.started += instance.OnCameraZoom;
            @CameraZoom.performed += instance.OnCameraZoom;
            @CameraZoom.canceled += instance.OnCameraZoom;
            @CameraRotate.started += instance.OnCameraRotate;
            @CameraRotate.performed += instance.OnCameraRotate;
            @CameraRotate.canceled += instance.OnCameraRotate;
            @PlaceSelect.started += instance.OnPlaceSelect;
            @PlaceSelect.performed += instance.OnPlaceSelect;
            @PlaceSelect.canceled += instance.OnPlaceSelect;
            @PlaceRotate.started += instance.OnPlaceRotate;
            @PlaceRotate.performed += instance.OnPlaceRotate;
            @PlaceRotate.canceled += instance.OnPlaceRotate;
            @PlaceCancel.started += instance.OnPlaceCancel;
            @PlaceCancel.performed += instance.OnPlaceCancel;
            @PlaceCancel.canceled += instance.OnPlaceCancel;
            @InventoryOpen.started += instance.OnInventoryOpen;
            @InventoryOpen.performed += instance.OnInventoryOpen;
            @InventoryOpen.canceled += instance.OnInventoryOpen;
            @UINavigate.started += instance.OnUINavigate;
            @UINavigate.performed += instance.OnUINavigate;
            @UINavigate.canceled += instance.OnUINavigate;
            @SwitchFPS.started += instance.OnSwitchFPS;
            @SwitchFPS.performed += instance.OnSwitchFPS;
            @SwitchFPS.canceled += instance.OnSwitchFPS;
        }

        private void UnregisterCallbacks(ICommandActions instance)
        {
            @CameraPan.started -= instance.OnCameraPan;
            @CameraPan.performed -= instance.OnCameraPan;
            @CameraPan.canceled -= instance.OnCameraPan;
            @CameraZoom.started -= instance.OnCameraZoom;
            @CameraZoom.performed -= instance.OnCameraZoom;
            @CameraZoom.canceled -= instance.OnCameraZoom;
            @CameraRotate.started -= instance.OnCameraRotate;
            @CameraRotate.performed -= instance.OnCameraRotate;
            @CameraRotate.canceled -= instance.OnCameraRotate;
            @PlaceSelect.started -= instance.OnPlaceSelect;
            @PlaceSelect.performed -= instance.OnPlaceSelect;
            @PlaceSelect.canceled -= instance.OnPlaceSelect;
            @PlaceRotate.started -= instance.OnPlaceRotate;
            @PlaceRotate.performed -= instance.OnPlaceRotate;
            @PlaceRotate.canceled -= instance.OnPlaceRotate;
            @PlaceCancel.started -= instance.OnPlaceCancel;
            @PlaceCancel.performed -= instance.OnPlaceCancel;
            @PlaceCancel.canceled -= instance.OnPlaceCancel;
            @InventoryOpen.started -= instance.OnInventoryOpen;
            @InventoryOpen.performed -= instance.OnInventoryOpen;
            @InventoryOpen.canceled -= instance.OnInventoryOpen;
            @UINavigate.started -= instance.OnUINavigate;
            @UINavigate.performed -= instance.OnUINavigate;
            @UINavigate.canceled -= instance.OnUINavigate;
            @SwitchFPS.started -= instance.OnSwitchFPS;
            @SwitchFPS.performed -= instance.OnSwitchFPS;
            @SwitchFPS.canceled -= instance.OnSwitchFPS;
        }

        public void RemoveCallbacks(ICommandActions instance)
        {
            if (m_Wrapper.m_CommandActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(ICommandActions instance)
        {
            foreach (var item in m_Wrapper.m_CommandActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_CommandActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }

    public CommandActions @Command => new CommandActions(this);
    private List<ICommandActions> m_CommandActionsCallbackInterfaces = new List<ICommandActions>();

    // Build action map
    public struct BuildActions
    {
        private SlopworksControls m_Wrapper;
        public BuildActions(SlopworksControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @ToggleBuildMode => m_Wrapper.m_Build_ToggleBuildMode;
        public InputAction @SelectTool1 => m_Wrapper.m_Build_SelectTool1;
        public InputAction @SelectTool2 => m_Wrapper.m_Build_SelectTool2;
        public InputAction @SelectTool3 => m_Wrapper.m_Build_SelectTool3;
        public InputAction @SelectTool4 => m_Wrapper.m_Build_SelectTool4;
        public InputAction @SelectTool5 => m_Wrapper.m_Build_SelectTool5;
        public InputAction @SelectTool6 => m_Wrapper.m_Build_SelectTool6;
        public InputAction @Rotate => m_Wrapper.m_Build_Rotate;
        public InputAction @DeleteMode => m_Wrapper.m_Build_DeleteMode;
        public InputAction @ZoopMode => m_Wrapper.m_Build_ZoopMode;
        public InputAction @GridOverlay => m_Wrapper.m_Build_GridOverlay;
        public InputAction @CycleVariant => m_Wrapper.m_Build_CycleVariant;
        public InputAction @NudgeUp => m_Wrapper.m_Build_NudgeUp;
        public InputAction @NudgeDown => m_Wrapper.m_Build_NudgeDown;
        public InputAction @Place => m_Wrapper.m_Build_Place;
        public InputAction @Remove => m_Wrapper.m_Build_Remove;
        public InputAction @Cancel => m_Wrapper.m_Build_Cancel;
        public InputAction @MachineInteract => m_Wrapper.m_Build_MachineInteract;
        public InputAction @SnapFilterToggle => m_Wrapper.m_Build_SnapFilterToggle;
        public InputAction @DebugDump => m_Wrapper.m_Build_DebugDump;
        public InputActionMap Get() { return m_Wrapper.m_Build; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(BuildActions set) { return set.Get(); }

        public void AddCallbacks(IBuildActions instance)
        {
            if (instance == null || m_Wrapper.m_BuildActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_BuildActionsCallbackInterfaces.Add(instance);
            @ToggleBuildMode.started += instance.OnToggleBuildMode;
            @ToggleBuildMode.performed += instance.OnToggleBuildMode;
            @ToggleBuildMode.canceled += instance.OnToggleBuildMode;
            @SelectTool1.started += instance.OnSelectTool1;
            @SelectTool1.performed += instance.OnSelectTool1;
            @SelectTool1.canceled += instance.OnSelectTool1;
            @SelectTool2.started += instance.OnSelectTool2;
            @SelectTool2.performed += instance.OnSelectTool2;
            @SelectTool2.canceled += instance.OnSelectTool2;
            @SelectTool3.started += instance.OnSelectTool3;
            @SelectTool3.performed += instance.OnSelectTool3;
            @SelectTool3.canceled += instance.OnSelectTool3;
            @SelectTool4.started += instance.OnSelectTool4;
            @SelectTool4.performed += instance.OnSelectTool4;
            @SelectTool4.canceled += instance.OnSelectTool4;
            @SelectTool5.started += instance.OnSelectTool5;
            @SelectTool5.performed += instance.OnSelectTool5;
            @SelectTool5.canceled += instance.OnSelectTool5;
            @SelectTool6.started += instance.OnSelectTool6;
            @SelectTool6.performed += instance.OnSelectTool6;
            @SelectTool6.canceled += instance.OnSelectTool6;
            @Rotate.started += instance.OnRotate;
            @Rotate.performed += instance.OnRotate;
            @Rotate.canceled += instance.OnRotate;
            @DeleteMode.started += instance.OnDeleteMode;
            @DeleteMode.performed += instance.OnDeleteMode;
            @DeleteMode.canceled += instance.OnDeleteMode;
            @ZoopMode.started += instance.OnZoopMode;
            @ZoopMode.performed += instance.OnZoopMode;
            @ZoopMode.canceled += instance.OnZoopMode;
            @GridOverlay.started += instance.OnGridOverlay;
            @GridOverlay.performed += instance.OnGridOverlay;
            @GridOverlay.canceled += instance.OnGridOverlay;
            @CycleVariant.started += instance.OnCycleVariant;
            @CycleVariant.performed += instance.OnCycleVariant;
            @CycleVariant.canceled += instance.OnCycleVariant;
            @NudgeUp.started += instance.OnNudgeUp;
            @NudgeUp.performed += instance.OnNudgeUp;
            @NudgeUp.canceled += instance.OnNudgeUp;
            @NudgeDown.started += instance.OnNudgeDown;
            @NudgeDown.performed += instance.OnNudgeDown;
            @NudgeDown.canceled += instance.OnNudgeDown;
            @Place.started += instance.OnPlace;
            @Place.performed += instance.OnPlace;
            @Place.canceled += instance.OnPlace;
            @Remove.started += instance.OnRemove;
            @Remove.performed += instance.OnRemove;
            @Remove.canceled += instance.OnRemove;
            @Cancel.started += instance.OnCancel;
            @Cancel.performed += instance.OnCancel;
            @Cancel.canceled += instance.OnCancel;
            @MachineInteract.started += instance.OnMachineInteract;
            @MachineInteract.performed += instance.OnMachineInteract;
            @MachineInteract.canceled += instance.OnMachineInteract;
            @SnapFilterToggle.started += instance.OnSnapFilterToggle;
            @SnapFilterToggle.performed += instance.OnSnapFilterToggle;
            @SnapFilterToggle.canceled += instance.OnSnapFilterToggle;
            @DebugDump.started += instance.OnDebugDump;
            @DebugDump.performed += instance.OnDebugDump;
            @DebugDump.canceled += instance.OnDebugDump;
        }

        private void UnregisterCallbacks(IBuildActions instance)
        {
            @ToggleBuildMode.started -= instance.OnToggleBuildMode;
            @ToggleBuildMode.performed -= instance.OnToggleBuildMode;
            @ToggleBuildMode.canceled -= instance.OnToggleBuildMode;
            @SelectTool1.started -= instance.OnSelectTool1;
            @SelectTool1.performed -= instance.OnSelectTool1;
            @SelectTool1.canceled -= instance.OnSelectTool1;
            @SelectTool2.started -= instance.OnSelectTool2;
            @SelectTool2.performed -= instance.OnSelectTool2;
            @SelectTool2.canceled -= instance.OnSelectTool2;
            @SelectTool3.started -= instance.OnSelectTool3;
            @SelectTool3.performed -= instance.OnSelectTool3;
            @SelectTool3.canceled -= instance.OnSelectTool3;
            @SelectTool4.started -= instance.OnSelectTool4;
            @SelectTool4.performed -= instance.OnSelectTool4;
            @SelectTool4.canceled -= instance.OnSelectTool4;
            @SelectTool5.started -= instance.OnSelectTool5;
            @SelectTool5.performed -= instance.OnSelectTool5;
            @SelectTool5.canceled -= instance.OnSelectTool5;
            @SelectTool6.started -= instance.OnSelectTool6;
            @SelectTool6.performed -= instance.OnSelectTool6;
            @SelectTool6.canceled -= instance.OnSelectTool6;
            @Rotate.started -= instance.OnRotate;
            @Rotate.performed -= instance.OnRotate;
            @Rotate.canceled -= instance.OnRotate;
            @DeleteMode.started -= instance.OnDeleteMode;
            @DeleteMode.performed -= instance.OnDeleteMode;
            @DeleteMode.canceled -= instance.OnDeleteMode;
            @ZoopMode.started -= instance.OnZoopMode;
            @ZoopMode.performed -= instance.OnZoopMode;
            @ZoopMode.canceled -= instance.OnZoopMode;
            @GridOverlay.started -= instance.OnGridOverlay;
            @GridOverlay.performed -= instance.OnGridOverlay;
            @GridOverlay.canceled -= instance.OnGridOverlay;
            @CycleVariant.started -= instance.OnCycleVariant;
            @CycleVariant.performed -= instance.OnCycleVariant;
            @CycleVariant.canceled -= instance.OnCycleVariant;
            @NudgeUp.started -= instance.OnNudgeUp;
            @NudgeUp.performed -= instance.OnNudgeUp;
            @NudgeUp.canceled -= instance.OnNudgeUp;
            @NudgeDown.started -= instance.OnNudgeDown;
            @NudgeDown.performed -= instance.OnNudgeDown;
            @NudgeDown.canceled -= instance.OnNudgeDown;
            @Place.started -= instance.OnPlace;
            @Place.performed -= instance.OnPlace;
            @Place.canceled -= instance.OnPlace;
            @Remove.started -= instance.OnRemove;
            @Remove.performed -= instance.OnRemove;
            @Remove.canceled -= instance.OnRemove;
            @Cancel.started -= instance.OnCancel;
            @Cancel.performed -= instance.OnCancel;
            @Cancel.canceled -= instance.OnCancel;
            @MachineInteract.started -= instance.OnMachineInteract;
            @MachineInteract.performed -= instance.OnMachineInteract;
            @MachineInteract.canceled -= instance.OnMachineInteract;
            @SnapFilterToggle.started -= instance.OnSnapFilterToggle;
            @SnapFilterToggle.performed -= instance.OnSnapFilterToggle;
            @SnapFilterToggle.canceled -= instance.OnSnapFilterToggle;
            @DebugDump.started -= instance.OnDebugDump;
            @DebugDump.performed -= instance.OnDebugDump;
            @DebugDump.canceled -= instance.OnDebugDump;
        }

        public void RemoveCallbacks(IBuildActions instance)
        {
            if (m_Wrapper.m_BuildActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        public void SetCallbacks(IBuildActions instance)
        {
            foreach (var item in m_Wrapper.m_BuildActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_BuildActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }

    public BuildActions @Build => new BuildActions(this);
    private List<IBuildActions> m_BuildActionsCallbackInterfaces = new List<IBuildActions>();

    public interface ICombatActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnLook(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
        void OnSprint(InputAction.CallbackContext context);
        void OnInteract(InputAction.CallbackContext context);
        void OnFire(InputAction.CallbackContext context);
        void OnAim(InputAction.CallbackContext context);
        void OnInventoryOpen(InputAction.CallbackContext context);
        void OnSwitchIsometric(InputAction.CallbackContext context);
        void OnReload(InputAction.CallbackContext context);
        void OnToggleBuildMode(InputAction.CallbackContext context);
    }

    public interface ICommandActions
    {
        void OnCameraPan(InputAction.CallbackContext context);
        void OnCameraZoom(InputAction.CallbackContext context);
        void OnCameraRotate(InputAction.CallbackContext context);
        void OnPlaceSelect(InputAction.CallbackContext context);
        void OnPlaceRotate(InputAction.CallbackContext context);
        void OnPlaceCancel(InputAction.CallbackContext context);
        void OnInventoryOpen(InputAction.CallbackContext context);
        void OnUINavigate(InputAction.CallbackContext context);
        void OnSwitchFPS(InputAction.CallbackContext context);
    }

    public interface IBuildActions
    {
        void OnToggleBuildMode(InputAction.CallbackContext context);
        void OnSelectTool1(InputAction.CallbackContext context);
        void OnSelectTool2(InputAction.CallbackContext context);
        void OnSelectTool3(InputAction.CallbackContext context);
        void OnSelectTool4(InputAction.CallbackContext context);
        void OnSelectTool5(InputAction.CallbackContext context);
        void OnSelectTool6(InputAction.CallbackContext context);
        void OnRotate(InputAction.CallbackContext context);
        void OnDeleteMode(InputAction.CallbackContext context);
        void OnZoopMode(InputAction.CallbackContext context);
        void OnGridOverlay(InputAction.CallbackContext context);
        void OnCycleVariant(InputAction.CallbackContext context);
        void OnNudgeUp(InputAction.CallbackContext context);
        void OnNudgeDown(InputAction.CallbackContext context);
        void OnPlace(InputAction.CallbackContext context);
        void OnRemove(InputAction.CallbackContext context);
        void OnCancel(InputAction.CallbackContext context);
        void OnMachineInteract(InputAction.CallbackContext context);
        void OnSnapFilterToggle(InputAction.CallbackContext context);
        void OnDebugDump(InputAction.CallbackContext context);
    }
}

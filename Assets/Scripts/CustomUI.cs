using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class MyCustomUIElement : UIElement
{
    public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.GetBooleanAction("InteractWithUI");

    protected override void HandHoverUpdate(Hand hand)
    {
        base.HandHoverUpdate(hand);

        if (interactWithUI.GetStateDown(hand.handType))
        {
            OnButtonClick();
        }
    }

    protected override void OnButtonClick()
    {
        base.OnButtonClick();

        Debug.Log("wheeee!");
        // Perform your button action here
    }
}
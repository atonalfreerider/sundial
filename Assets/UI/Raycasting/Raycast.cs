#nullable enable
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.UI.Raycasting
{
    public static class Raycast
    {
        public static RaycastTarget? TargetAfterCasting()
        {
            RaycastHit? hit = CastRay(RayFromMouseCursor());
            if (hit.HasValue)
            {
                return RaycastTarget.SelectableTarget(
                    hit.Value.point,
                    hit.Value.transform);
            }
            
            return null;
        }

        static Ray RayFromMouseCursor() => Camera.main.ScreenPointToRay(
            Application.platform == RuntimePlatform.Android
                ? Touchscreen.current.primaryTouch.position.ReadValue()
                : Mouse.current.position.ReadValue());

        static RaycastHit? CastRay(Ray ray)
        {
            bool didHit = Physics.Raycast(
                ray,
                out RaycastHit hitInfo);
            return didHit ? hitInfo : null;
        }
    }
}
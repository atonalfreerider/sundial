using UnityEngine;

namespace Assets.UI.Raycasting
{
    public class RaycastTarget
    {
        public readonly ISelectable AsSelectable;
    
        RaycastTarget(Vector3 collisionPoint, ISelectable target)
        {
            AsSelectable = target;
        }
    
        public static RaycastTarget SelectableTarget(Vector3 collisionPoint, Transform target) =>
            new RaycastTarget(collisionPoint, TargetAsSelectable(target));

        static ISelectable TargetAsSelectable(Transform target)
        {
            // Find either the target or one of the target's parents that contains the ISelectable. The only target that
            // does not have ISelectable on the same object as the collider is the RenderedTimeline, which instead has a
            // child that contains the collider.
            Transform candidate = target;
            while (candidate != null)
            {
                ISelectable selectable = candidate.GetComponent<ISelectable>();
                if (selectable != null)
                {
                    return selectable;
                }

                candidate = candidate.parent;
            }

            Debug.LogError(
                $"Transform {target.name} is not part of an ISelectable",
                target);
            return null;
        }

        public void Highlight()
        {
            // We are always using this method during the same frame the target was hit. We don't have to worry about
            // the target being destroyed in that time, so we can skip the UnityUtil.IsDestroyedUnityObject that is part
            // of the IsSelectable boolean.
            AsSelectable?.Highlight();
        }

        public void Unhighlight()
        {
            // This method may be called on a destroyed target. We need to check if `IsSelectable` is true because that
            // property includes a check to see if the game object of the AsSelectable was destroyed.
            AsSelectable.Unhighlight();
        }
    }
}
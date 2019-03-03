
using UnityEngine;

namespace Assets.UI
{
    public interface ISelectable
    {
        Transform SelectionTarget { get; }

        void RequestSelection();
    }
}

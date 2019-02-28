
using UnityEngine;

namespace Assets.UI
{
    public interface ISelectable
    {
        Transform SelectionTarget { get; }

        void Highlight();

        void Unhighlight();

        void RequestSelection();

        void RequestDeselection();
    }
}

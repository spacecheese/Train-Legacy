using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Splines
{
    [Serializable]
    public class SerializableCurveNode
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public bool BeforeHandleSet;
        public Vector3 BeforeHandle;

        public bool AfterHandleSet;
        public Vector3 AfterHandle;

        public CurveNode.HandleConstraintType HandleConstraint;

        public static explicit operator CurveNode(SerializableCurveNode node)
        {
            var newNode = new CurveNode(node.Position, node.Rotation,
                node.BeforeHandle, node.AfterHandle, node.HandleConstraint);

            if (!node.BeforeHandleSet) newNode.BeforeHandle = null;
            if (!node.AfterHandleSet) newNode.AfterHandle = null;

            return newNode;
        }

        public static explicit operator SerializableCurveNode(CurveNode node)
        {
            return new SerializableCurveNode(node.Position, node.Rotation,
                node.BeforeHandle, node.AfterHandle, node.HandleConstraint);
        }

        public SerializableCurveNode(Vector3 position, Quaternion rotation, 
            Vector3? beforeHandle, Vector3? afterHandle, 
            CurveNode.HandleConstraintType handleConstraint)
        {
            Position = position;
            Rotation = rotation;

            BeforeHandleSet = beforeHandle.HasValue;
            BeforeHandle = beforeHandle.GetValueOrDefault();

            AfterHandleSet = afterHandle.HasValue;
            AfterHandle = afterHandle.GetValueOrDefault();

            HandleConstraint = handleConstraint;
        }
    }

    [Serializable]
    public partial class Spline : ISerializationCallbackReceiver
    {
        /// <summary>
        /// Serialization only list of nodes only updated when the spline is about to be serialized
        /// or deserialized.
        /// </summary>
        [SerializeField]
        private List<SerializableCurveNode> serializableNodes;

        /// <summary>
        /// Serialization only field that represents if the spline is closed. This is required
        /// because only the spline curve nodes are serialized.
        /// </summary>
        [SerializeField]
        private bool serializableIsClosed = false;

        public void OnAfterDeserialize()
        {
            if (serializableNodes.Count == 0) return;

            CurveNode start = (CurveNode)serializableNodes[0];
            CurveNode end;

            if (serializableNodes.Count == 1)
                Curves.Add(new Curve(start, null));

            // Build curves from all the serializableNodes.
            for (int i = 1; i < serializableNodes.Count; i++)
            {
                if (Curves.Count > 0)
                    start = Curves[Curves.Count - 1].End;

                if (serializableIsClosed && 
                    i == serializableNodes.Count - 1)
                    // Close the spline if this is the last node and the original spline was closed.
                    end = Start;
                else
                    end = (CurveNode)serializableNodes[i];

                Curves.Add(new Curve(start, end));
            }

            serializableNodes.Clear();
        }

        public void OnBeforeSerialize()
        {
            serializableNodes = new List<SerializableCurveNode>(Nodes.Select((item) => (SerializableCurveNode)item));
            serializableIsClosed = IsClosed;
        }
    }
}

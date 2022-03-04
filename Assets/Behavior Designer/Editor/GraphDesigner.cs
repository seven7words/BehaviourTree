﻿// Decompiled with JetBrains decompiler
// Type: BehaviorDesigner.Editor.GraphDesigner
// Assembly: BehaviorDesigner.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BCDC5FA8-C213-4FB9-87DF-CDF716000D6A
// Assembly location: D:\StudyProject\BehaviourTreeEditor\Assets\Behavior Designer\Editor\BehaviorDesigner.Editor.dll

using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BehaviorDesigner.Editor
{
  [Serializable]
  public class GraphDesigner : ScriptableObject
  {
    private NodeDesigner mEntryNode;
    private NodeDesigner mRootNode;
    private List<NodeDesigner> mDetachedNodes = new List<NodeDesigner>();
    [SerializeField]
    private List<NodeDesigner> mSelectedNodes = new List<NodeDesigner>();
    private NodeDesigner mHoverNode;
    private NodeConnection mActiveNodeConnection;
    [SerializeField]
    private List<NodeConnection> mSelectedNodeConnections = new List<NodeConnection>();
    [SerializeField]
    private int mNextTaskID;
    private List<int> mNodeSelectedID = new List<int>();
    [SerializeField]
    private int[] mPrevNodeSelectedID;

    public NodeDesigner RootNode => this.mRootNode;

    public List<NodeDesigner> DetachedNodes => this.mDetachedNodes;

    public List<NodeDesigner> SelectedNodes => this.mSelectedNodes;

    public NodeDesigner HoverNode
    {
      get => this.mHoverNode;
      set => this.mHoverNode = value;
    }

    public NodeConnection ActiveNodeConnection
    {
      get => this.mActiveNodeConnection;
      set => this.mActiveNodeConnection = value;
    }

    public List<NodeConnection> SelectedNodeConnections => this.mSelectedNodeConnections;

    public void OnEnable() => this.hideFlags = HideFlags.HideAndDontSave;

    public NodeDesigner AddNode(
      BehaviorSource behaviorSource,
      System.Type type,
      Vector2 position)
    {
      if (!(Activator.CreateInstance(type, true) is Task instance))
      {
        EditorUtility.DisplayDialog("Unable to Add Task", string.Format("Unable to create task of type {0}. Is the class name the same as the file name?", type), "OK");
        return (NodeDesigner) null;
      }
      try
      {
        instance.OnReset();
      }
      catch (Exception ex)
      {
      }
      return this.AddNode(behaviorSource, instance, position);
    }

    private NodeDesigner AddNode(
      BehaviorSource behaviorSource,
      Task task,
      Vector2 position)
    {
      if (this.mEntryNode == null)
      {
        Task instance = Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.Tasks.EntryTask")) as Task;
        this.mEntryNode = ScriptableObject.CreateInstance<NodeDesigner>();
        this.mEntryNode.LoadNode(instance, behaviorSource, new Vector2(position.x, position.y - 120f), ref this.mNextTaskID);
        this.mEntryNode.MakeEntryDisplay();
      }
      NodeDesigner instance1 = ScriptableObject.CreateInstance<NodeDesigner>();
      instance1.LoadNode(task, behaviorSource, position, ref this.mNextTaskID);
      TaskNameAttribute[] customAttributes;
      if ((customAttributes = task.GetType().GetCustomAttributes(typeof (TaskNameAttribute), false) as TaskNameAttribute[]).Length > 0)
        task.FriendlyName = customAttributes[0].Name;
      if (this.mEntryNode.OutgoingNodeConnections.Count == 0)
      {
        this.mActiveNodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
        this.mActiveNodeConnection.LoadConnection(this.mEntryNode, NodeConnectionType.Outgoing);
        this.ConnectNodes(behaviorSource, instance1);
      }
      else
        this.mDetachedNodes.Add(instance1);
      return instance1;
    }

    public NodeDesigner NodeAt(Vector2 point, Vector2 offset)
    {
      if (this.mEntryNode == null)
        return (NodeDesigner) null;
      for (int index = 0; index < this.mSelectedNodes.Count; ++index)
      {
        if (this.mSelectedNodes[index].Contains(point, offset, false))
          return this.mSelectedNodes[index];
      }
      for (int index = this.mDetachedNodes.Count - 1; index > -1; --index)
      {
        NodeDesigner nodeDesigner;
        if (this.mDetachedNodes[index] != null && (nodeDesigner = this.NodeChildrenAt(this.mDetachedNodes[index], point, offset)) != null)
          return nodeDesigner;
      }
      NodeDesigner nodeDesigner1;
      if (this.mRootNode != null && (nodeDesigner1 = this.NodeChildrenAt(this.mRootNode, point, offset)) != null)
        return nodeDesigner1;
      return this.mEntryNode.Contains(point, offset, true) ? this.mEntryNode : (NodeDesigner) null;
    }

    private NodeDesigner NodeChildrenAt(
      NodeDesigner nodeDesigner,
      Vector2 point,
      Vector2 offset)
    {
      if (nodeDesigner == null)
        return (NodeDesigner) null;
      if (nodeDesigner.Contains(point, offset, true))
        return nodeDesigner;
      if (nodeDesigner.IsParent)
      {
        ParentTask task = nodeDesigner.Task as ParentTask;
        if (!task.NodeData.Collapsed && task.Children != null)
        {
          for (int index = 0; index < task.Children.Count; ++index)
          {
            NodeDesigner nodeDesigner1;
            if (task.Children[index] != null && (nodeDesigner1 = this.NodeChildrenAt(task.Children[index].NodeData.NodeDesigner as NodeDesigner, point, offset)) != null)
              return nodeDesigner1;
          }
        }
      }
      return (NodeDesigner) null;
    }

    public List<NodeDesigner> NodesAt(Rect rect, Vector2 offset)
    {
      List<NodeDesigner> nodes = new List<NodeDesigner>();
      if (this.mRootNode != null)
        this.NodesChildrenAt(this.mRootNode, rect, offset, ref nodes);
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
        this.NodesChildrenAt(this.mDetachedNodes[index], rect, offset, ref nodes);
      return nodes.Count > 0 ? nodes : (List<NodeDesigner>) null;
    }

    private void NodesChildrenAt(
      NodeDesigner nodeDesigner,
      Rect rect,
      Vector2 offset,
      ref List<NodeDesigner> nodes)
    {
      if (nodeDesigner.Intersects(rect, offset))
        nodes.Add(nodeDesigner);
      if (!nodeDesigner.IsParent)
        return;
      ParentTask task = nodeDesigner.Task as ParentTask;
      if (task.NodeData.Collapsed || task.Children == null)
        return;
      for (int index = 0; index < task.Children.Count; ++index)
      {
        if (task.Children[index] != null)
          this.NodesChildrenAt(task.Children[index].NodeData.NodeDesigner as NodeDesigner, rect, offset, ref nodes);
      }
    }

    public bool IsSelected(NodeDesigner nodeDesigner) => this.mSelectedNodes.Contains(nodeDesigner);

    public bool IsParentSelected(NodeDesigner nodeDesigner)
    {
      if (!(nodeDesigner.ParentNodeDesigner != null))
        return false;
      return this.IsSelected(nodeDesigner.ParentNodeDesigner) || this.IsParentSelected(nodeDesigner.ParentNodeDesigner);
    }

    public void Select(NodeDesigner nodeDesigner) => this.Select(nodeDesigner, true);

    public void Select(NodeDesigner nodeDesigner, bool addHash)
    {
      if (this.mSelectedNodes.Contains(nodeDesigner))
        return;
      if (this.mSelectedNodes.Count == 1)
        this.IndicateReferencedTasks(this.mSelectedNodes[0].Task, false);
      this.mSelectedNodes.Add(nodeDesigner);
      if (addHash)
        this.mNodeSelectedID.Add(nodeDesigner.Task.ID);
      nodeDesigner.Select();
      if (this.mSelectedNodes.Count != 1)
        return;
      this.IndicateReferencedTasks(this.mSelectedNodes[0].Task, true);
    }

    public void Deselect(NodeDesigner nodeDesigner)
    {
      this.mSelectedNodes.Remove(nodeDesigner);
      this.mNodeSelectedID.Remove(nodeDesigner.Task.ID);
      nodeDesigner.Deselect();
      this.IndicateReferencedTasks(nodeDesigner.Task, false);
    }

    public void DeselectAll(NodeDesigner exceptionNodeDesigner)
    {
      for (int index = this.mSelectedNodes.Count - 1; index >= 0; --index)
      {
        if (exceptionNodeDesigner == null || !this.mSelectedNodes[index].Equals(exceptionNodeDesigner))
        {
          this.mSelectedNodes[index].Deselect();
          this.mSelectedNodes.RemoveAt(index);
          this.mNodeSelectedID.RemoveAt(index);
        }
      }
      if (!(exceptionNodeDesigner != null))
        return;
      this.IndicateReferencedTasks(exceptionNodeDesigner.Task, false);
    }

    public void ClearNodeSelection()
    {
      if (this.mSelectedNodes.Count == 1)
        this.IndicateReferencedTasks(this.mSelectedNodes[0].Task, false);
      for (int index = 0; index < this.mSelectedNodes.Count; ++index)
        this.mSelectedNodes[index].Deselect();
      this.mSelectedNodes.Clear();
      this.mNodeSelectedID.Clear();
    }

    public void DeselectWithParent(NodeDesigner nodeDesigner)
    {
      for (int index = this.mSelectedNodes.Count - 1; index >= 0; --index)
      {
        if (this.mSelectedNodes[index].HasParent(nodeDesigner))
          this.Deselect(this.mSelectedNodes[index]);
      }
    }

    public bool ReplaceSelectedNodes(BehaviorSource behaviorSource, System.Type taskType)
    {
      if (this.SelectedNodes.Count == 0)
        return false;
      for (int index1 = this.SelectedNodes.Count - 1; index1 > -1; --index1)
      {
        Vector2 absolutePosition = this.SelectedNodes[index1].GetAbsolutePosition();
        NodeDesigner parentNodeDesigner = this.SelectedNodes[index1].ParentNodeDesigner;
        List<Task> taskList1 = !this.SelectedNodes[index1].IsParent ? (List<Task>) null : (this.SelectedNodes[index1].Task as ParentTask).Children;
        UnknownTask task1 = this.SelectedNodes[index1].Task as UnknownTask;
        this.RemoveNode(this.SelectedNodes[index1]);
        this.mSelectedNodes.RemoveAt(index1);
        TaskReferences.CheckReferences(behaviorSource);
        NodeDesigner nodeDesigner = (NodeDesigner) null;
        if (task1 != null)
        {
          Task task2 = (Task) null;
          if (!string.IsNullOrEmpty(task1.JSONSerialization))
          {
            Dictionary<int, Task> IDtoTask = new Dictionary<int, Task>();
            Dictionary<string, object> dict = MiniJSON.Deserialize(task1.JSONSerialization) as Dictionary<string, object>;
            if (dict.ContainsKey("Type"))
              dict["Type"] = taskType.ToString();
            task2 = JSONDeserialization.DeserializeTask(behaviorSource, dict, ref IDtoTask, (List<Object>) null);
          }
          else
          {
            TaskSerializationData taskSerializationData = new TaskSerializationData();
            taskSerializationData.types.Add(taskType.ToString());
            taskSerializationData.startIndex.Add(0);
            FieldSerializationData fieldSerializationData = new FieldSerializationData();
            fieldSerializationData.fieldNameHash = task1.fieldNameHash;
            fieldSerializationData.startIndex = task1.startIndex;
            fieldSerializationData.dataPosition = task1.dataPosition;
            fieldSerializationData.unityObjects = task1.unityObjects;
            fieldSerializationData.byteDataArray = task1.byteData.ToArray();
            List<Task> taskList2 = new List<Task>();
            //TODO:解析二进制
            
            //BinaryDeserialization.LoadTask(taskSerializationData, fieldSerializationData, ref taskList2, ref behaviorSource);
            if (taskList2.Count > 0)
              task2 = taskList2[0];
          }
          if (task2 != null)
            nodeDesigner = this.AddNode(behaviorSource, task2, absolutePosition);
        }
        else
          nodeDesigner = this.AddNode(behaviorSource, taskType, absolutePosition);
        if (!(nodeDesigner == null))
        {
          if (parentNodeDesigner != null)
          {
            this.ActiveNodeConnection = parentNodeDesigner.CreateNodeConnection(false);
            this.ConnectNodes(behaviorSource, nodeDesigner);
          }
          if (nodeDesigner.IsParent && taskList1 != null)
          {
            for (int index2 = 0; index2 < taskList1.Count; ++index2)
            {
              this.ActiveNodeConnection = nodeDesigner.CreateNodeConnection(false);
              this.ConnectNodes(behaviorSource, taskList1[index2].NodeData.NodeDesigner as NodeDesigner);
              if (index2 >= (nodeDesigner.Task as ParentTask).MaxChildren())
                break;
            }
          }
          this.Select(nodeDesigner);
        }
      }
      BehaviorUndo.RegisterUndo("Replace", behaviorSource.Owner.GetObject());
      return true;
    }

    public void Hover(NodeDesigner nodeDesigner)
    {
      if (nodeDesigner.ShowHoverBar)
        return;
      nodeDesigner.ShowHoverBar = true;
      this.HoverNode = nodeDesigner;
    }

    public void ClearHover()
    {
      if (!(bool) this.HoverNode)
        return;
      this.HoverNode.ShowHoverBar = false;
      this.HoverNode = (NodeDesigner) null;
    }

    private void IndicateReferencedTasks(Task task, bool indicate)
    {
      List<Task> referencedTasks = TaskInspector.GetReferencedTasks(task);
      if (referencedTasks == null || referencedTasks.Count <= 0)
        return;
      for (int index = 0; index < referencedTasks.Count; ++index)
      {
        if (referencedTasks[index] != null && referencedTasks[index].NodeData != null)
        {
          NodeDesigner nodeDesigner = referencedTasks[index].NodeData.NodeDesigner as NodeDesigner;
          if (nodeDesigner != null)
            nodeDesigner.ShowReferenceIcon = indicate;
        }
      }
    }

    public bool DragSelectedNodes(Vector2 delta, bool dragChildren)
    {
      if (mSelectedNodes.Count == 0)
        return false;
      bool flag = mSelectedNodes.Count == 1;
      for (int index = 0; index < mSelectedNodes.Count; ++index)
        DragNode(mSelectedNodes[index], delta, dragChildren);
      if (flag && dragChildren && (mSelectedNodes[0].IsEntryDisplay && mRootNode != null))
        DragNode(mRootNode, delta, dragChildren);
      return true;
    }

    private void DragNode(NodeDesigner nodeDesigner, Vector2 delta, bool dragChildren)
    {
      if (IsParentSelected(nodeDesigner) && dragChildren)
        return;
      nodeDesigner.ChangeOffset(delta);
      if (nodeDesigner.ParentNodeDesigner != null)
      {
        int index1 = nodeDesigner.ParentNodeDesigner.ChildIndexForTask(nodeDesigner.Task);
        if (index1 != -1)
        {
          int index2 = index1 - 1;
          bool flag = false;
          NodeDesigner nodeDesigner1 = nodeDesigner.ParentNodeDesigner.NodeDesignerForChildIndex(index2);
          if (nodeDesigner1 != null && nodeDesigner.Task.NodeData.Offset.x < (double) nodeDesigner1.Task.NodeData.Offset.x)
          {
            nodeDesigner.ParentNodeDesigner.MoveChildNode(index1, true);
            flag = true;
          }
          if (!flag)
          {
            int index3 = index1 + 1;
            NodeDesigner nodeDesigner2 = nodeDesigner.ParentNodeDesigner.NodeDesignerForChildIndex(index3);
            if (nodeDesigner2 != null && (double) nodeDesigner.Task.NodeData.Offset.x > (double) nodeDesigner2.Task.NodeData.Offset.x)
              nodeDesigner.ParentNodeDesigner.MoveChildNode(index1, false);
          }
        }
      }
      if (nodeDesigner.IsParent && !dragChildren)
      {
        ParentTask task = nodeDesigner.Task as ParentTask;
        if (task.Children != null)
        {
          for (int index = 0; index < task.Children.Count; ++index)
            (task.Children[index].NodeData.NodeDesigner as NodeDesigner).ChangeOffset(-delta);
        }
      }
      this.MarkNodeDirty(nodeDesigner);
    }

    public bool DrawNodes(Vector2 mousePosition, Vector2 offset)
    {
      if (this.mEntryNode == null)
        return false;
      this.mEntryNode.DrawNodeConnection(offset, false);
      if (this.mRootNode != null)
        this.DrawNodeConnectionChildren(this.mRootNode, offset, this.mRootNode.Task.Disabled);
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
        this.DrawNodeConnectionChildren(this.mDetachedNodes[index], offset, this.mDetachedNodes[index].Task.Disabled);
      for (int index = 0; index < this.mSelectedNodeConnections.Count; ++index)
        this.mSelectedNodeConnections[index].DrawConnection(offset, this.mSelectedNodeConnections[index].OriginatingNodeDesigner.IsDisabled());
      if (mousePosition != new Vector2(-1f, -1f) && this.mActiveNodeConnection != null)
      {
        this.mActiveNodeConnection.HorizontalHeight = (float) (((double) this.mActiveNodeConnection.OriginatingNodeDesigner.GetConnectionPosition(offset, this.mActiveNodeConnection.NodeConnectionType).y + (double) mousePosition.y) / 2.0);
        this.mActiveNodeConnection.DrawConnection(this.mActiveNodeConnection.OriginatingNodeDesigner.GetConnectionPosition(offset, this.mActiveNodeConnection.NodeConnectionType), mousePosition, this.mActiveNodeConnection.NodeConnectionType == NodeConnectionType.Outgoing && this.mActiveNodeConnection.OriginatingNodeDesigner.IsDisabled());
      }
      this.mEntryNode.DrawNode(offset, false, false);
      bool flag = false;
      if (this.mRootNode != null && this.DrawNodeChildren(this.mRootNode, offset, this.mRootNode.Task.Disabled))
        flag = true;
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
      {
        if (this.DrawNodeChildren(this.mDetachedNodes[index], offset, this.mDetachedNodes[index].Task.Disabled))
          flag = true;
      }
      for (int index = 0; index < this.mSelectedNodes.Count; ++index)
      {
        if (this.mSelectedNodes[index].DrawNode(offset, true, this.mSelectedNodes[index].IsDisabled()))
          flag = true;
      }
      if (this.mRootNode != null)
        this.DrawNodeCommentChildren(this.mRootNode, offset);
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
        this.DrawNodeCommentChildren(this.mDetachedNodes[index], offset);
      return flag;
    }

    private bool DrawNodeChildren(NodeDesigner nodeDesigner, Vector2 offset, bool disabledNode)
    {
      if (nodeDesigner == null)
        return false;
      bool flag = false;
      if (nodeDesigner.DrawNode(offset, false, disabledNode))
        flag = true;
      if (nodeDesigner.IsParent)
      {
        ParentTask task = nodeDesigner.Task as ParentTask;
        if (!task.NodeData.Collapsed && task.Children != null)
        {
          for (int index = task.Children.Count - 1; index > -1; --index)
          {
            if (task.Children[index] != null && this.DrawNodeChildren(task.Children[index].NodeData.NodeDesigner as NodeDesigner, offset, task.Disabled || disabledNode))
              flag = true;
          }
        }
      }
      return flag;
    }

    private void DrawNodeConnectionChildren(
      NodeDesigner nodeDesigner,
      Vector2 offset,
      bool disabledNode)
    {
      if (nodeDesigner == null || nodeDesigner.Task.NodeData.Collapsed)
        return;
      nodeDesigner.DrawNodeConnection(offset, nodeDesigner.Task.Disabled || disabledNode);
      if (!nodeDesigner.IsParent)
        return;
      ParentTask task = nodeDesigner.Task as ParentTask;
      if (task.Children == null)
        return;
      for (int index = 0; index < task.Children.Count; ++index)
      {
        if (task.Children[index] != null)
          this.DrawNodeConnectionChildren(task.Children[index].NodeData.NodeDesigner as NodeDesigner, offset, task.Disabled || disabledNode);
      }
    }

    private void DrawNodeCommentChildren(NodeDesigner nodeDesigner, Vector2 offset)
    {
      if (nodeDesigner == null)
        return;
      nodeDesigner.DrawNodeComment(offset);
      if (!nodeDesigner.IsParent)
        return;
      ParentTask task = nodeDesigner.Task as ParentTask;
      if (task.NodeData.Collapsed || task.Children == null)
        return;
      for (int index = 0; index < task.Children.Count; ++index)
      {
        if (task.Children[index] != null)
          this.DrawNodeCommentChildren(task.Children[index].NodeData.NodeDesigner as NodeDesigner, offset);
      }
    }

    private void RemoveNode(NodeDesigner nodeDesigner)
    {
      if (nodeDesigner.IsEntryDisplay)
        return;
      if (nodeDesigner.IsParent)
      {
        for (int index = 0; index < nodeDesigner.OutgoingNodeConnections.Count; ++index)
        {
          NodeDesigner destinationNodeDesigner = nodeDesigner.OutgoingNodeConnections[index].DestinationNodeDesigner;
          this.mDetachedNodes.Add(destinationNodeDesigner);
          destinationNodeDesigner.Task.NodeData.Offset = destinationNodeDesigner.GetAbsolutePosition();
          destinationNodeDesigner.ParentNodeDesigner = (NodeDesigner) null;
        }
      }
      if (nodeDesigner.ParentNodeDesigner != null)
        nodeDesigner.ParentNodeDesigner.RemoveChildNode(nodeDesigner);
      if (this.mRootNode != null && this.mRootNode.Equals(nodeDesigner))
      {
        this.mEntryNode.RemoveChildNode(nodeDesigner);
        this.mRootNode = (NodeDesigner) null;
      }
      if (this.mRootNode != null)
        this.RemoveReferencedTasks(this.mRootNode, nodeDesigner.Task);
      if (this.mDetachedNodes != null)
      {
        for (int index = 0; index < this.mDetachedNodes.Count; ++index)
          this.RemoveReferencedTasks(this.mDetachedNodes[index], nodeDesigner.Task);
      }
      this.mDetachedNodes.Remove(nodeDesigner);
      BehaviorUndo.DestroyObject(nodeDesigner, false);
    }

    private void RemoveReferencedTasks(NodeDesigner nodeDesigner, Task task)
    {
      bool fullSync = false;
      bool doReference = false;
      FieldInfo[] serializableFields = TaskUtility.GetSerializableFields(nodeDesigner.Task.GetType());
      for (int index1 = 0; index1 < serializableFields.Length; ++index1)
      {
        if (!serializableFields[index1].IsPrivate && !serializableFields[index1].IsFamily || BehaviorDesignerUtility.HasAttribute(serializableFields[index1], typeof (SerializeField)))
        {
          if (typeof (IList).IsAssignableFrom(serializableFields[index1].FieldType))
          {
            if ((typeof (Task).IsAssignableFrom(serializableFields[index1].FieldType.GetElementType()) || serializableFields[index1].FieldType.IsGenericType && typeof (Task).IsAssignableFrom(serializableFields[index1].FieldType.GetGenericArguments()[0])) && serializableFields[index1].GetValue(nodeDesigner.Task) is Task[] taskArray)
            {
              for (int index2 = taskArray.Length - 1; index2 > -1; --index2)
              {
                if (taskArray[index2] != null && (nodeDesigner.Task.Equals(task) || taskArray[index2].Equals(task)))
                  TaskInspector.ReferenceTasks(nodeDesigner.Task, task, serializableFields[index1], ref fullSync, ref doReference, false, false);
              }
            }
          }
          else if (typeof (Task).IsAssignableFrom(serializableFields[index1].FieldType) && serializableFields[index1].GetValue(nodeDesigner.Task) is Task task4 && (nodeDesigner.Task.Equals(task) || task4.Equals(task)))
            TaskInspector.ReferenceTasks(nodeDesigner.Task, task, serializableFields[index1], ref fullSync, ref doReference, false, false);
        }
      }
      if (!nodeDesigner.IsParent)
        return;
      ParentTask task5 = nodeDesigner.Task as ParentTask;
      if (task5.Children == null)
        return;
      for (int index = 0; index < task5.Children.Count; ++index)
      {
        if (task5.Children[index] != null)
          this.RemoveReferencedTasks(task5.Children[index].NodeData.NodeDesigner as NodeDesigner, task);
      }
    }

    public bool NodeCanOriginateConnection(NodeDesigner nodeDesigner, NodeConnection connection)
    {
      if (!nodeDesigner.IsEntryDisplay)
        return true;
      return nodeDesigner.IsEntryDisplay && connection.NodeConnectionType == NodeConnectionType.Outgoing;
    }

    public bool NodeCanAcceptConnection(NodeDesigner nodeDesigner, NodeConnection connection)
    {
      if ((!nodeDesigner.IsEntryDisplay || connection.NodeConnectionType != NodeConnectionType.Incoming) && (nodeDesigner.IsEntryDisplay || !nodeDesigner.IsParent && (nodeDesigner.IsParent || connection.NodeConnectionType != NodeConnectionType.Outgoing)))
        return false;
      if (nodeDesigner.IsEntryDisplay || connection.OriginatingNodeDesigner.IsEntryDisplay)
        return true;
      HashSet<NodeDesigner> set = new HashSet<NodeDesigner>();
      NodeDesigner nodeDesigner1 = connection.NodeConnectionType != NodeConnectionType.Outgoing ? connection.OriginatingNodeDesigner : nodeDesigner;
      NodeDesigner nodeDesigner2 = connection.NodeConnectionType != NodeConnectionType.Outgoing ? nodeDesigner : connection.OriginatingNodeDesigner;
      return !this.CycleExists(nodeDesigner1, ref set) && !set.Contains(nodeDesigner2);
    }

    private bool CycleExists(NodeDesigner nodeDesigner, ref HashSet<NodeDesigner> set)
    {
      if (set.Contains(nodeDesigner))
        return true;
      set.Add(nodeDesigner);
      if (nodeDesigner.IsParent)
      {
        ParentTask task = nodeDesigner.Task as ParentTask;
        if (task.Children != null)
        {
          for (int index = 0; index < task.Children.Count; ++index)
          {
            if (this.CycleExists(task.Children[index].NodeData.NodeDesigner as NodeDesigner, ref set))
              return true;
          }
        }
      }
      return false;
    }

    public void ConnectNodes(BehaviorSource behaviorSource, NodeDesigner nodeDesigner)
    {
      NodeConnection activeNodeConnection = this.mActiveNodeConnection;
      this.mActiveNodeConnection = (NodeConnection) null;
      if (!(activeNodeConnection != null) || activeNodeConnection.OriginatingNodeDesigner.Equals(nodeDesigner))
        return;
      NodeDesigner originatingNodeDesigner = activeNodeConnection.OriginatingNodeDesigner;
      if (activeNodeConnection.NodeConnectionType == NodeConnectionType.Outgoing)
      {
        this.RemoveParentConnection(nodeDesigner);
        this.CheckForLastConnectionRemoval(originatingNodeDesigner);
        originatingNodeDesigner.AddChildNode(nodeDesigner, activeNodeConnection, true, false);
      }
      else
      {
        this.RemoveParentConnection(originatingNodeDesigner);
        this.CheckForLastConnectionRemoval(nodeDesigner);
        nodeDesigner.AddChildNode(originatingNodeDesigner, activeNodeConnection, true, false);
      }
      if (activeNodeConnection.OriginatingNodeDesigner.IsEntryDisplay)
        this.mRootNode = activeNodeConnection.DestinationNodeDesigner;
      this.mDetachedNodes.Remove(activeNodeConnection.DestinationNodeDesigner);
    }

    private void RemoveParentConnection(NodeDesigner nodeDesigner)
    {
      if (!(nodeDesigner.ParentNodeDesigner != null))
        return;
      NodeDesigner parentNodeDesigner = nodeDesigner.ParentNodeDesigner;
      NodeConnection nodeConnection = (NodeConnection) null;
      for (int index = 0; index < parentNodeDesigner.OutgoingNodeConnections.Count; ++index)
      {
        if (parentNodeDesigner.OutgoingNodeConnections[index].DestinationNodeDesigner.Equals(nodeDesigner))
        {
          nodeConnection = parentNodeDesigner.OutgoingNodeConnections[index];
          break;
        }
      }
      if (!(nodeConnection != null))
        return;
      this.RemoveConnection(nodeConnection);
    }

    private void CheckForLastConnectionRemoval(NodeDesigner nodeDesigner)
    {
      if (nodeDesigner.IsEntryDisplay)
      {
        if (nodeDesigner.OutgoingNodeConnections.Count != 1)
          return;
        this.RemoveConnection(nodeDesigner.OutgoingNodeConnections[0]);
      }
      else
      {
        ParentTask task = nodeDesigner.Task as ParentTask;
        if (task.Children == null || task.Children.Count + 1 <= task.MaxChildren())
          return;
        NodeConnection nodeConnection = (NodeConnection) null;
        for (int index = 0; index < nodeDesigner.OutgoingNodeConnections.Count; ++index)
        {
          if (nodeDesigner.OutgoingNodeConnections[index].DestinationNodeDesigner.Equals((task.Children[task.Children.Count - 1].NodeData.NodeDesigner as NodeDesigner)))
          {
            nodeConnection = nodeDesigner.OutgoingNodeConnections[index];
            break;
          }
        }
        if (!(nodeConnection != null))
          return;
        this.RemoveConnection(nodeConnection);
      }
    }

    public void NodeConnectionsAt(
      Vector2 point,
      Vector2 offset,
      ref List<NodeConnection> nodeConnections)
    {
      if (this.mEntryNode == null)
        return;
      this.NodeChildrenConnectionsAt(this.mEntryNode, point, offset, ref nodeConnections);
      if (this.mRootNode != null)
        this.NodeChildrenConnectionsAt(this.mRootNode, point, offset, ref nodeConnections);
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
        this.NodeChildrenConnectionsAt(this.mDetachedNodes[index], point, offset, ref nodeConnections);
    }

    private void NodeChildrenConnectionsAt(
      NodeDesigner nodeDesigner,
      Vector2 point,
      Vector2 offset,
      ref List<NodeConnection> nodeConnections)
    {
      if (nodeDesigner.Task.NodeData.Collapsed)
        return;
      nodeDesigner.ConnectionContains(point, offset, ref nodeConnections);
      if (!nodeDesigner.IsParent || !(nodeDesigner.Task is ParentTask task) || task.Children == null)
        return;
      for (int index = 0; index < task.Children.Count; ++index)
      {
        if (task.Children[index] != null)
          this.NodeChildrenConnectionsAt(task.Children[index].NodeData.NodeDesigner as NodeDesigner, point, offset, ref nodeConnections);
      }
    }

    public void RemoveConnection(NodeConnection nodeConnection)
    {
      nodeConnection.DestinationNodeDesigner.Task.NodeData.Offset = nodeConnection.DestinationNodeDesigner.GetAbsolutePosition();
      this.mDetachedNodes.Add(nodeConnection.DestinationNodeDesigner);
      nodeConnection.OriginatingNodeDesigner.RemoveChildNode(nodeConnection.DestinationNodeDesigner);
      if (!nodeConnection.OriginatingNodeDesigner.IsEntryDisplay)
        return;
      this.mRootNode = (NodeDesigner) null;
    }

    public bool IsSelected(NodeConnection nodeConnection)
    {
      for (int index = 0; index < this.mSelectedNodeConnections.Count; ++index)
      {
        if (this.mSelectedNodeConnections[index].Equals(nodeConnection))
          return true;
      }
      return false;
    }

    public void Select(NodeConnection nodeConnection)
    {
      this.mSelectedNodeConnections.Add(nodeConnection);
      nodeConnection.select();
    }

    public void Deselect(NodeConnection nodeConnection)
    {
      this.mSelectedNodeConnections.Remove(nodeConnection);
      nodeConnection.deselect();
    }

    public void ClearConnectionSelection()
    {
      for (int index = 0; index < this.mSelectedNodeConnections.Count; ++index)
        this.mSelectedNodeConnections[index].deselect();
      this.mSelectedNodeConnections.Clear();
    }

    public void GraphDirty()
    {
      if (this.mEntryNode == null)
        return;
      this.mEntryNode.MarkDirty();
      if (this.mRootNode != null)
        this.MarkNodeDirty(this.mRootNode);
      for (int index = this.mDetachedNodes.Count - 1; index > -1; --index)
        this.MarkNodeDirty(this.mDetachedNodes[index]);
    }

    private void MarkNodeDirty(NodeDesigner nodeDesigner)
    {
      nodeDesigner.MarkDirty();
      if (nodeDesigner.IsEntryDisplay)
      {
        if (nodeDesigner.OutgoingNodeConnections.Count <= 0 || !(nodeDesigner.OutgoingNodeConnections[0].DestinationNodeDesigner != null))
          return;
        this.MarkNodeDirty(nodeDesigner.OutgoingNodeConnections[0].DestinationNodeDesigner);
      }
      else
      {
        if (!nodeDesigner.IsParent)
          return;
        ParentTask task = nodeDesigner.Task as ParentTask;
        if (task.Children == null)
          return;
        for (int index = 0; index < task.Children.Count; ++index)
        {
          if (task.Children[index] != null)
            this.MarkNodeDirty(task.Children[index].NodeData.NodeDesigner as NodeDesigner);
        }
      }
    }

    public void Find(string findTaskValue, SharedVariable findSharedVariable)
    {
      if (findTaskValue != null)
        findTaskValue = findTaskValue.ToLower();
      if (this.mRootNode != null)
        this.Find(this.mRootNode, findTaskValue, findSharedVariable);
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
        this.Find(this.mDetachedNodes[index], findTaskValue, findSharedVariable);
    }

    private void Find(
      NodeDesigner nodeDesigner,
      string findTaskValue,
      SharedVariable findSharedVariable)
    {
      if (nodeDesigner == null || nodeDesigner.Task == null)
        return;
      bool found = false;
      if (!string.IsNullOrEmpty(findTaskValue) && findTaskValue.Length > 2)
      {
        if (nodeDesigner.Task.FriendlyName.ToLower().Contains(findTaskValue))
          found = true;
        else if (nodeDesigner.Task.GetType().FullName.ToLower().Contains(findTaskValue))
          found = true;
      }
      if (!found)
      {
        FieldInfo[] serializableFields = TaskUtility.GetSerializableFields(nodeDesigner.Task.GetType());
        for (int index1 = 0; index1 < serializableFields.Length; ++index1)
        {
          System.Type fieldType = serializableFields[index1].FieldType;
          if (findSharedVariable != null && typeof (SharedVariable).IsAssignableFrom(fieldType))
          {
            if (serializableFields[index1].GetValue(nodeDesigner.Task) is SharedVariable sharedVariable16 && sharedVariable16.Name == findSharedVariable.Name && sharedVariable16.IsGlobal == findSharedVariable.IsGlobal)
            {
              found = true;
              break;
            }
          }
          else if (BehaviorDesignerUtility.HasAttribute(serializableFields[index1], typeof (InspectTaskAttribute)) && serializableFields[index1].GetValue(nodeDesigner.Task) is Task task5)
          {
            if (!string.IsNullOrEmpty(findTaskValue) && findTaskValue.Length > 2 && (task5.FriendlyName.ToLower().Contains(findTaskValue) || task5.GetType().FullName.ToLower().Contains(findTaskValue)))
            {
              found = true;
              break;
            }
            if (!found)
            {
              FieldInfo[] publicFields = TaskUtility.GetPublicFields(task5.GetType());
              for (int index2 = 0; index2 < publicFields.Length; ++index2)
              {
                if (findSharedVariable != null && typeof (SharedVariable).IsAssignableFrom(publicFields[index2].FieldType) && (publicFields[index2].GetValue(task5) is SharedVariable sharedVariable17 && sharedVariable17.Name == findSharedVariable.Name) && sharedVariable17.IsGlobal == findSharedVariable.IsGlobal)
                {
                  found = true;
                  break;
                }
              }
            }
          }
        }
      }
      nodeDesigner.FoundTask(found);
      if (!nodeDesigner.IsParent)
        return;
      ParentTask task6 = nodeDesigner.Task as ParentTask;
      if (task6.Children == null)
        return;
      for (int index = 0; index < task6.Children.Count; ++index)
      {
        if (task6.Children[index] != null)
          this.Find(task6.Children[index].NodeData.NodeDesigner as NodeDesigner, findTaskValue, findSharedVariable);
      }
    }

    public List<BehaviorSource> FindReferencedBehaviors()
    {
      List<BehaviorSource> behaviors = new List<BehaviorSource>();
      if (this.mRootNode != null)
        this.FindReferencedBehaviors(this.mRootNode, ref behaviors);
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
        this.FindReferencedBehaviors(this.mDetachedNodes[index], ref behaviors);
      return behaviors;
    }

    public void FindReferencedBehaviors(
      NodeDesigner nodeDesigner,
      ref List<BehaviorSource> behaviors)
    {
      FieldInfo[] publicFields = TaskUtility.GetPublicFields(nodeDesigner.Task.GetType());
      for (int index1 = 0; index1 < publicFields.Length; ++index1)
      {
        System.Type fieldType = publicFields[index1].FieldType;
        if (typeof (IList).IsAssignableFrom(fieldType))
        {
          System.Type type = fieldType;
          System.Type c;
          if (fieldType.IsGenericType)
          {
            while (!type.IsGenericType)
              type = type.BaseType;
            c = fieldType.GetGenericArguments()[0];
          }
          else
            c = fieldType.GetElementType();
          if (!(c == (System.Type) null))
          {
            if (typeof (ExternalBehavior).IsAssignableFrom(c) || typeof (Behavior).IsAssignableFrom(c))
            {
              if (publicFields[index1].GetValue(nodeDesigner.Task) is IList list6)
              {
                for (int index2 = 0; index2 < list6.Count; ++index2)
                {
                  if (list6[index2] != null)
                  {
                    BehaviorSource behaviorSource;
                    if (list6[index2] is ExternalBehavior)
                    {
                      behaviorSource = (list6[index2] as ExternalBehavior).BehaviorSource;
                      if (behaviorSource.Owner == null)
                        behaviorSource.Owner = (IBehavior) (list6[index2] as ExternalBehavior);
                    }
                    else
                    {
                      behaviorSource = (list6[index2] as Behavior).GetBehaviorSource();
                      if (behaviorSource.Owner == null)
                        behaviorSource.Owner = (IBehavior) (list6[index2] as Behavior);
                    }
                    behaviors.Add(behaviorSource);
                  }
                }
              }
            }
            else if (!typeof (Behavior).IsAssignableFrom(c))
              ;
          }
        }
        else if (typeof (ExternalBehavior).IsAssignableFrom(fieldType) || typeof (Behavior).IsAssignableFrom(fieldType))
        {
          object obj = publicFields[index1].GetValue(nodeDesigner.Task);
          if (obj != null)
          {
            BehaviorSource behaviorSource;
            if (obj is ExternalBehavior)
            {
              behaviorSource = (obj as ExternalBehavior).BehaviorSource;
              if (behaviorSource.Owner == null)
                behaviorSource.Owner = (IBehavior) (obj as ExternalBehavior);
              behaviors.Add(behaviorSource);
            }
            else
            {
              behaviorSource = (obj as Behavior).GetBehaviorSource();
              if (behaviorSource.Owner == null)
                behaviorSource.Owner = (IBehavior) (obj as Behavior);
            }
            behaviors.Add(behaviorSource);
          }
        }
      }
      if (!nodeDesigner.IsParent)
        return;
      ParentTask task = nodeDesigner.Task as ParentTask;
      if (task.Children == null)
        return;
      for (int index = 0; index < task.Children.Count; ++index)
      {
        if (task.Children[index] != null)
          this.FindReferencedBehaviors(task.Children[index].NodeData.NodeDesigner as NodeDesigner, ref behaviors);
      }
    }

    public void SelectAll()
    {
      for (int index = this.mSelectedNodes.Count - 1; index > -1; --index)
        this.Deselect(this.mSelectedNodes[index]);
      if (this.mRootNode != null)
        this.SelectAll(this.mRootNode);
      for (int index = this.mDetachedNodes.Count - 1; index > -1; --index)
        this.SelectAll(this.mDetachedNodes[index]);
    }

    private void SelectAll(NodeDesigner nodeDesigner)
    {
      this.Select(nodeDesigner);
      if (!nodeDesigner.Task.GetType().IsSubclassOf(typeof (ParentTask)))
        return;
      ParentTask task = nodeDesigner.Task as ParentTask;
      if (task.Children == null)
        return;
      for (int index = 0; index < task.Children.Count; ++index)
        this.SelectAll(task.Children[index].NodeData.NodeDesigner as NodeDesigner);
    }

    public int GetTaskCount()
    {
      int count = this.mDetachedNodes.Count;
      if (this.mRootNode != null)
        count += this.GetTaskCount(this.mRootNode);
      return count;
    }

    private int GetTaskCount(NodeDesigner nodeDesigner)
    {
      int num = 1;
      if (nodeDesigner.Task.GetType().IsSubclassOf(typeof (ParentTask)))
      {
        ParentTask task = nodeDesigner.Task as ParentTask;
        if (task.Children != null)
        {
          for (int index = 0; index < task.Children.Count; ++index)
            num += this.GetTaskCount(task.Children[index].NodeData.NodeDesigner as NodeDesigner);
        }
      }
      return num;
    }

    public void IdentifyNode(NodeDesigner nodeDesigner) => nodeDesigner.IdentifyNode();

    public List<TaskSerializer> Copy(Vector2 graphOffset, float graphZoom)
    {
      List<TaskSerializer> taskSerializerList = new List<TaskSerializer>();
      for (int index1 = 0; index1 < this.mSelectedNodes.Count; ++index1)
      {
        TaskSerializer taskSerializer;
        if ((taskSerializer = TaskCopier.CopySerialized(this.mSelectedNodes[index1].Task)) != null)
        {
          if (this.mSelectedNodes[index1].IsParent)
          {
            ParentTask task = this.mSelectedNodes[index1].Task as ParentTask;
            if (task.Children != null)
            {
              List<int> intList = new List<int>();
              for (int index2 = 0; index2 < task.Children.Count; ++index2)
              {
                int num;
                if ((num = this.mSelectedNodes.IndexOf(task.Children[index2].NodeData.NodeDesigner as NodeDesigner)) != -1)
                  intList.Add(num);
              }
              taskSerializer.childrenIndex = intList;
            }
          }
          taskSerializer.offset = (taskSerializer.offset + graphOffset) * graphZoom;
          taskSerializerList.Add(taskSerializer);
        }
      }
      return taskSerializerList.Count > 0 ? taskSerializerList : (List<TaskSerializer>) null;
    }

    public bool Paste(
      BehaviorSource behaviorSource,
      Vector3 position,
      List<TaskSerializer> copiedTasks,
      Vector2 graphOffset,
      float graphZoom)
    {
      if (copiedTasks == null || copiedTasks.Count == 0)
        return false;
      this.ClearNodeSelection();
      this.ClearConnectionSelection();
      this.RemapIDs();
      List<NodeDesigner> nodeDesignerList = new List<NodeDesigner>();
      for (int index = 0; index < copiedTasks.Count; ++index)
      {
        TaskSerializer copiedTask = copiedTasks[index];
        Task task = TaskCopier.PasteTask(behaviorSource, copiedTask);
        NodeDesigner instance = ScriptableObject.CreateInstance<NodeDesigner>();
        instance.LoadTask(task, behaviorSource.Owner == null ? (Behavior) null : behaviorSource.Owner.GetObject() as Behavior, ref this.mNextTaskID);
        instance.Task.NodeData.Offset = copiedTask.offset / graphZoom - graphOffset;
        nodeDesignerList.Add(instance);
        this.mDetachedNodes.Add(instance);
        this.Select(instance);
      }
      for (int index1 = 0; index1 < copiedTasks.Count; ++index1)
      {
        TaskSerializer copiedTask = copiedTasks[index1];
        if (copiedTask.childrenIndex != null)
        {
          for (int index2 = 0; index2 < copiedTask.childrenIndex.Count; ++index2)
          {
            NodeDesigner nodeDesigner = nodeDesignerList[index1];
            NodeConnection instance = ScriptableObject.CreateInstance<NodeConnection>();
            instance.LoadConnection(nodeDesigner, NodeConnectionType.Outgoing);
            nodeDesigner.AddChildNode(nodeDesignerList[copiedTask.childrenIndex[index2]], instance, true, false);
            this.mDetachedNodes.Remove(nodeDesignerList[copiedTask.childrenIndex[index2]]);
          }
        }
      }
      if (this.mEntryNode == null)
      {
        Task instance = Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.Tasks.EntryTask")) as Task;
        this.mEntryNode = ScriptableObject.CreateInstance<NodeDesigner>();
        this.mEntryNode.LoadNode(instance, behaviorSource, new Vector2(position.x, position.y - 120f), ref this.mNextTaskID);
        this.mEntryNode.MakeEntryDisplay();
        if (this.mDetachedNodes.Count > 0)
        {
          this.mActiveNodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
          this.mActiveNodeConnection.LoadConnection(this.mEntryNode, NodeConnectionType.Outgoing);
          this.ConnectNodes(behaviorSource, this.mDetachedNodes[0]);
        }
      }
      this.Save(behaviorSource);
      return true;
    }

    public bool Delete(
      BehaviorSource behaviorSource,
      BehaviorDesignerWindow.TaskCallbackHandler callback)
    {
      bool flag = false;
      if (this.mSelectedNodeConnections != null)
      {
        for (int index = 0; index < this.mSelectedNodeConnections.Count; ++index)
          this.RemoveConnection(this.mSelectedNodeConnections[index]);
        this.mSelectedNodeConnections.Clear();
        flag = true;
      }
      if (this.mSelectedNodes != null)
      {
        for (int index = 0; index < this.mSelectedNodes.Count; ++index)
        {
          if (callback != null)
            callback(behaviorSource, this.mSelectedNodes[index].Task);
          this.RemoveNode(this.mSelectedNodes[index]);
        }
        this.mSelectedNodes.Clear();
        flag = true;
      }
      if (flag)
      {
        BehaviorUndo.RegisterUndo(nameof (Delete), behaviorSource.Owner.GetObject());
        TaskReferences.CheckReferences(behaviorSource);
        this.Save(behaviorSource);
      }
      return flag;
    }

    public bool RemoveSharedVariableReferences(SharedVariable sharedVariable)
    {
      if (this.mEntryNode == null)
        return false;
      bool flag = false;
      if (this.mRootNode != null && this.RemoveSharedVariableReference(this.mRootNode, sharedVariable))
        flag = true;
      if (this.mDetachedNodes != null)
      {
        for (int index = 0; index < this.mDetachedNodes.Count; ++index)
        {
          if (this.RemoveSharedVariableReference(this.mDetachedNodes[index], sharedVariable))
            flag = true;
        }
      }
      return flag;
    }

    private bool RemoveSharedVariableReference(
      NodeDesigner nodeDesigner,
      SharedVariable sharedVariable)
    {
      bool flag = false;
      FieldInfo[] serializableFields = TaskUtility.GetSerializableFields(nodeDesigner.Task.GetType());
      for (int index = 0; index < serializableFields.Length; ++index)
      {
        if (typeof (SharedVariable).IsAssignableFrom(serializableFields[index].FieldType) && serializableFields[index].GetValue(nodeDesigner.Task) is SharedVariable sharedVariable2 && (!string.IsNullOrEmpty(sharedVariable2.Name) && sharedVariable2.IsGlobal == sharedVariable.IsGlobal) && sharedVariable2.Name.Equals(sharedVariable.Name))
        {
          if (!serializableFields[index].FieldType.IsAbstract)
          {
            SharedVariable instance = Activator.CreateInstance(serializableFields[index].FieldType) as SharedVariable;
            instance.IsShared = true;
            serializableFields[index].SetValue(nodeDesigner.Task, instance);
          }
          flag = true;
        }
      }
      if (nodeDesigner.IsParent)
      {
        ParentTask task = nodeDesigner.Task as ParentTask;
        if (task.Children != null)
        {
          for (int index = 0; index < task.Children.Count; ++index)
          {
            if (task.Children[index] != null && this.RemoveSharedVariableReference(task.Children[index].NodeData.NodeDesigner as NodeDesigner, sharedVariable))
              flag = true;
          }
        }
      }
      return flag;
    }

    private void RemapIDs()
    {
      if (this.mEntryNode == null)
        return;
      this.mNextTaskID = 0;
      this.mEntryNode.SetID(ref this.mNextTaskID);
      if (this.mRootNode != null)
        this.mRootNode.SetID(ref this.mNextTaskID);
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
        this.mDetachedNodes[index].SetID(ref this.mNextTaskID);
      this.mNodeSelectedID.Clear();
      for (int index = 0; index < this.mSelectedNodes.Count; ++index)
        this.mNodeSelectedID.Add(this.mSelectedNodes[index].Task.ID);
    }

    public Rect GraphSize(Vector3 offset)
    {
      if (this.mEntryNode == null)
        return new Rect();
      Rect minMaxRect = new Rect();
      minMaxRect.xMin = float.MaxValue;
      minMaxRect.xMax = float.MinValue;
      minMaxRect.yMin = float.MaxValue;
      minMaxRect.yMax = float.MinValue;
      this.GetNodeMinMax((Vector2) offset, this.mEntryNode, ref minMaxRect);
      if (this.mRootNode != null)
        this.GetNodeMinMax((Vector2) offset, this.mRootNode, ref minMaxRect);
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
        this.GetNodeMinMax((Vector2) offset, this.mDetachedNodes[index], ref minMaxRect);
      return minMaxRect;
    }

    private void GetNodeMinMax(Vector2 offset, NodeDesigner nodeDesigner, ref Rect minMaxRect)
    {
      Rect rect = nodeDesigner.Rectangle(offset, true, true);
      if ((double) rect.xMin < (double) minMaxRect.xMin)
        minMaxRect.xMin = rect.xMin;
      if ((double) rect.yMin < (double) minMaxRect.yMin)
        minMaxRect.yMin = rect.yMin;
      if ((double) rect.xMax > (double) minMaxRect.xMax)
        minMaxRect.xMax = rect.xMax;
      if ((double) rect.yMax > (double) minMaxRect.yMax)
        minMaxRect.yMax = rect.yMax;
      if (!nodeDesigner.IsParent)
        return;
      ParentTask task = nodeDesigner.Task as ParentTask;
      if (task.Children == null)
        return;
      for (int index = 0; index < task.Children.Count; ++index)
        this.GetNodeMinMax(offset, task.Children[index].NodeData.NodeDesigner as NodeDesigner, ref minMaxRect);
    }

    public void Save(BehaviorSource behaviorSource)
    {
      if (object.ReferenceEquals(behaviorSource.Owner.GetObject(), null))
        return;
      this.RemapIDs();
      List<Task> detachedTasks = new List<Task>();
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
        detachedTasks.Add(this.mDetachedNodes[index].Task);
      behaviorSource.Save(!(this.mEntryNode != null) ? (Task) null : this.mEntryNode.Task, !(this.mRootNode != null) ? (Task) null : this.mRootNode.Task, detachedTasks);
      if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
        BinarySerialization.Save(behaviorSource);
      else
        JSONSerialization.Save(behaviorSource);
    }

    public bool Load(BehaviorSource behaviorSource, bool loadPrevBehavior, Vector2 nodePosition)
    {
      if (behaviorSource == null)
      {
        this.Clear(false);
        return false;
      }
      this.DestroyNodeDesigners();
      if (behaviorSource.Owner != null && behaviorSource.Owner is Behavior && (behaviorSource.Owner as Behavior).ExternalBehavior != null)
      {
        List<SharedVariable> sharedVariableList = (List<SharedVariable>) null;
        bool force = !Application.isPlaying;
        if (Application.isPlaying && !(behaviorSource.Owner as Behavior).HasInheritedVariables)
        {
          behaviorSource.CheckForSerialization(true);
          sharedVariableList = behaviorSource.GetAllVariables();
          (behaviorSource.Owner as Behavior).HasInheritedVariables = true;
          force = true;
        }
        ExternalBehavior externalBehavior = (behaviorSource.Owner as Behavior).ExternalBehavior;
        externalBehavior.BehaviorSource.Owner = (IBehavior) externalBehavior;
        externalBehavior.BehaviorSource.CheckForSerialization(force, behaviorSource);
        if (sharedVariableList != null)
        {
          for (int index = 0; index < sharedVariableList.Count; ++index)
            behaviorSource.SetVariable(sharedVariableList[index].Name, sharedVariableList[index]);
        }
      }
      else
        behaviorSource.CheckForSerialization(!Application.isPlaying);
      if (behaviorSource.EntryTask == null && behaviorSource.RootTask == null && behaviorSource.DetachedTasks == null)
      {
        this.Clear(false);
        return false;
      }
      if (loadPrevBehavior)
      {
        this.mSelectedNodes.Clear();
        this.mSelectedNodeConnections.Clear();
        if (this.mPrevNodeSelectedID != null)
        {
          for (int index = 0; index < this.mPrevNodeSelectedID.Length; ++index)
            this.mNodeSelectedID.Add(this.mPrevNodeSelectedID[index]);
          this.mPrevNodeSelectedID = (int[]) null;
        }
      }
      else
        this.Clear(false);
      this.mNextTaskID = 0;
      this.mEntryNode = (NodeDesigner) null;
      this.mRootNode = (NodeDesigner) null;
      this.mDetachedNodes.Clear();
      Task entryTask;
      Task rootTask;
      List<Task> detachedTasks;
      behaviorSource.Load(out entryTask, out rootTask, out detachedTasks);
      if (BehaviorDesignerUtility.AnyNullTasks(behaviorSource) || behaviorSource.TaskData != null && BehaviorDesignerUtility.HasRootTask(behaviorSource.TaskData.JSONSerialization) && behaviorSource.RootTask == null)
      {
        behaviorSource.CheckForSerialization(true);
        behaviorSource.Load(out entryTask, out rootTask, out detachedTasks);
      }
      if (entryTask == null)
      {
        if (rootTask != null || detachedTasks != null && detachedTasks.Count > 0)
        {
          behaviorSource.EntryTask = entryTask = Activator.CreateInstance(TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.Tasks.EntryTask"), true) as Task;
          this.mEntryNode = ScriptableObject.CreateInstance<NodeDesigner>();
          if (rootTask != null)
            this.mEntryNode.LoadNode(entryTask, behaviorSource, new Vector2(rootTask.NodeData.Offset.x, rootTask.NodeData.Offset.y - 120f), ref this.mNextTaskID);
          else
            this.mEntryNode.LoadNode(entryTask, behaviorSource, new Vector2(nodePosition.x, nodePosition.y - 120f), ref this.mNextTaskID);
          this.mEntryNode.MakeEntryDisplay();
        }
      }
      else
      {
        this.mEntryNode = ScriptableObject.CreateInstance<NodeDesigner>();
        this.mEntryNode.LoadTask(entryTask, behaviorSource.Owner == null ? (Behavior) null : behaviorSource.Owner.GetObject() as Behavior, ref this.mNextTaskID);
        this.mEntryNode.MakeEntryDisplay();
      }
      if (rootTask != null)
      {
        this.mRootNode = ScriptableObject.CreateInstance<NodeDesigner>();
        this.mRootNode.LoadTask(rootTask, behaviorSource.Owner == null ? (Behavior) null : behaviorSource.Owner.GetObject() as Behavior, ref this.mNextTaskID);
        NodeConnection instance = ScriptableObject.CreateInstance<NodeConnection>();
        instance.LoadConnection(this.mEntryNode, NodeConnectionType.Fixed);
        this.mEntryNode.AddChildNode(this.mRootNode, instance, false, false);
        this.LoadNodeSelection(this.mRootNode);
        if (this.mEntryNode.OutgoingNodeConnections.Count == 0)
        {
          this.mActiveNodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
          this.mActiveNodeConnection.LoadConnection(this.mEntryNode, NodeConnectionType.Outgoing);
          this.ConnectNodes(behaviorSource, this.mRootNode);
        }
      }
      if (detachedTasks != null)
      {
        for (int index = 0; index < detachedTasks.Count; ++index)
        {
          if (detachedTasks[index] != null)
          {
            NodeDesigner instance = ScriptableObject.CreateInstance<NodeDesigner>();
            instance.LoadTask(detachedTasks[index], behaviorSource.Owner == null ? (Behavior) null : behaviorSource.Owner.GetObject() as Behavior, ref this.mNextTaskID);
            this.mDetachedNodes.Add(instance);
            this.LoadNodeSelection(instance);
          }
        }
      }
      return true;
    }

    public bool HasEntryNode() => this.mEntryNode != null && this.mEntryNode.Task != null;

    public Vector2 EntryNodeOffset() => this.mEntryNode.Task.NodeData.Offset;

    public void SetStartOffset(Vector2 offset)
    {
      Vector2 vector2 = offset - this.mEntryNode.Task.NodeData.Offset;
      this.mEntryNode.Task.NodeData.Offset = offset;
      for (int index = 0; index < this.mDetachedNodes.Count; ++index)
        this.mDetachedNodes[index].Task.NodeData.Offset += vector2;
    }

    private void LoadNodeSelection(NodeDesigner nodeDesigner)
    {
      if (nodeDesigner == null)
        return;
      if (this.mNodeSelectedID != null && this.mNodeSelectedID.Contains(nodeDesigner.Task.ID))
        this.Select(nodeDesigner, false);
      if (!nodeDesigner.IsParent)
        return;
      ParentTask task = nodeDesigner.Task as ParentTask;
      if (task.Children == null)
        return;
      for (int index = 0; index < task.Children.Count; ++index)
      {
        if (task.Children[index] != null && task.Children[index].NodeData != null)
          this.LoadNodeSelection(task.Children[index].NodeData.NodeDesigner as NodeDesigner);
      }
    }

    public void Clear(bool saveSelectedNodes)
    {
      if (saveSelectedNodes)
      {
        if (this.mNodeSelectedID.Count > 0)
          this.mPrevNodeSelectedID = this.mNodeSelectedID.ToArray();
      }
      else
        this.mPrevNodeSelectedID = (int[]) null;
      this.mNodeSelectedID.Clear();
      this.mSelectedNodes.Clear();
      this.mSelectedNodeConnections.Clear();
      this.DestroyNodeDesigners();
    }

    public void DestroyNodeDesigners()
    {
      if (this.mEntryNode != null)
        this.Clear(this.mEntryNode);
      if (this.mRootNode != null)
        this.Clear(this.mRootNode);
      for (int index = this.mDetachedNodes.Count - 1; index > -1; --index)
        this.Clear(this.mDetachedNodes[index]);
      this.mEntryNode = (NodeDesigner) null;
      this.mRootNode = (NodeDesigner) null;
      this.mDetachedNodes = new List<NodeDesigner>();
    }

    private void Clear(NodeDesigner nodeDesigner)
    {
      if (nodeDesigner == null)
        return;
      if (nodeDesigner.IsParent && nodeDesigner.Task is ParentTask task && task.Children != null)
      {
        for (int index = task.Children.Count - 1; index > -1; --index)
        {
          if (task.Children[index] != null)
            this.Clear(task.Children[index].NodeData.NodeDesigner as NodeDesigner);
        }
      }
      nodeDesigner.DestroyConnections();
      Object.DestroyImmediate(nodeDesigner, true);
    }
  }
}

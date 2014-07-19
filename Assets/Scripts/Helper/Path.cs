using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Path {
    private List<Vector2Int> _nodes = new List<Vector2Int>();
    private float _length = 0;
    private int _currentNode = 0;

    public void addNodeToEnd(Vector2Int node){
        if (_nodes.Count != 0) {
            Vector2Int delta = getEnd() - node;
            _length += new Vector2(delta.x, delta.y).magnitude;
        }
        _nodes.Add(node);
    }

    public void addNodeToStart(Vector2Int node) {
        if (_nodes.Count != 0) {
            Vector2Int delta = getStart() - node;
            _length += new Vector2(delta.x, delta.y).magnitude;
        }
        _nodes.Insert(0, node);
    }

    public Vector2Int removeNodeFromStart() {
        Vector2Int ret = null;
        if (_nodes.Count != 0) {
            ret = _nodes[0];
            _nodes.RemoveAt(0);
        }
        return ret;
    }

    public Vector2Int removeNodeFromEnd() {
        Vector2Int ret = null;
        if (_nodes.Count != 0) {
            ret = _nodes[_nodes.Count - 1];
            _nodes.RemoveAt(_nodes.Count - 1);
        }
        return ret;
    }

    public Vector2Int getStart() {
        return _nodes[0];
    }

    public Vector2Int getEnd() {
        return _nodes[_nodes.Count - 1];
    }

    public Vector2Int getCurrent() {
        return _nodes[_currentNode];
    }

    public bool hasNext() {
        return _currentNode < _nodes.Count;
    }

    public Vector2Int getNext() {
        return _nodes[_currentNode++];
    }

    public float getLength() {
        return _length;
    }

    public int getNodeCount() {
        return _nodes.Count;
    }

    public int getNodesLeft() {
        return _nodes.Count - _currentNode;
    }
}

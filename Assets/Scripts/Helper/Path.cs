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

    public Vector2Int this[int i]{
        get {
            return _nodes[i];
        }
    }

    public int Count {
        get {
            return _nodes.Count;
        }
    }

    public bool hasNext() {
        return _currentNode < _nodes.Count - 1;
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

    public Vector2 getSmoothPoint(float distance) {
        int currentNode = Mathf.RoundToInt(distance);
        int previousNode = currentNode - 1;
        int nextNode = currentNode + 1;

        Vector2 p1 = _nodes[currentNode];
        Vector2 p2 = Vector2.zero, p0 = Vector2.zero;
        if (previousNode != -1) {
            p0 = p1 + (Tilemap.getWorldLocation(_nodes[previousNode]) - p1) / 2.0f;
        }
        if (nextNode != _nodes.Count) {
            p2 = p1 + (Tilemap.getWorldLocation(_nodes[nextNode]) - p1) / 2.0f;
        }
        if (previousNode == -1) {
            p0 = p1 + (p1 - p2);
        }
        if (nextNode == _nodes.Count) {
            p2 = p1 + (p1 - p0);
        }

        float t = distance - currentNode + 0.5f;

        return (1 - t) * ((1 - t) * p0 + t * p1) + t * ((1 - t) * p1 + t * p2);
    }

    public Vector2 getSmoothDirection(float distance) {
        Vector2 point = getSmoothPoint(distance);
        Vector2 dir = distance == 0.0f ? getSmoothPoint(distance + 0.1f) - point : point - getSmoothPoint(distance - 0.1f);
        return dir.normalized;
    }

    public void truncateBegining(Vector2Int currentPosition) {
        Vector2Int currNode = getCurrent();
        while (true) {
            if (!hasNext()) {
                break;
            }

            Vector2Int nextNode = getNext();

            float currDist = Vector2Int.distance(currentPosition, currNode);
            float nextDist = Vector2Int.distance(currentPosition, nextNode);

            if (currDist < nextDist) {
                break;
            }
        }
    }
}

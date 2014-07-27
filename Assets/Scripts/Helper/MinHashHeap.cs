using UnityEngine;
using System.Collections.Generic;
using System;


public class MinHashHeap<GenericItem> where GenericItem: IComparable{
    private List<GenericItem> heap = new List<GenericItem>();//list of "nodes" that will make up heap structure
    private Dictionary<GenericItem, int> itemIndexMap = new Dictionary<GenericItem,int>();
    private GenericItem root;
    private int cachedHeapSize = -1;

    public void insert(GenericItem node_data) {
       // heap.Add(node_data);//adds element and updates length of list
        addToEnd(node_data);
        cachedHeapSize = -1;
        bubbleUp(heap.Count - 1);
    }

    //removes the minimum element in the tree and restructures tree with minHeapify to maintain integrity
    public GenericItem extractElement(int i) {
        if (getHeapSize() <= 0) {
            throw new ArgumentOutOfRangeException();
        }
        GenericItem extractedElement = heap[i];
        replaceWithEnd(i); ;
        cachedHeapSize = -1;
        if (i >= getHeapSize()) {
            return extractedElement;
        }
        if (extractedElement.CompareTo(heap[i]) < 0) {
            this.minHeapify(i);
        } else {
            bubbleUp(i);
        }

        return extractedElement;
    }

    public GenericItem extractElement(GenericItem element) {
        int index = 0;
        if(!itemIndexMap.TryGetValue(element, out index)){
            throw new System.Exception("Requested item was not found in heap");
        }
        return extractElement(index);
    }

    public bool contains(GenericItem element) {
        return itemIndexMap.ContainsKey(element);
    }

    public GenericItem peekAtElement(int i) {
        return heap[i];
    }
    private void minHeapify(int i) {
        int smallest;
        int l = 2 * i + 1;
        int r = 2 * i + 2;

        if (l < heap.Count && (heap[l].CompareTo(heap[i]) < 0)) {
            smallest = l;
        } else {
            smallest = i;
        }

        if (r < heap.Count && (heap[r].CompareTo(heap[smallest]) < 0)) {
            smallest = r;
        }

        if (smallest != i) {
            swapHeapElements(i, smallest);
            this.minHeapify(smallest);
        }
    }
    private void bubbleUp(int i) {
        while (i > 0) {
            int j = ((i + 1) / 2) - 1;
            //compares two elements to see if child is in correct place
            if (heap[j].CompareTo(heap[i]) <= 0) {
                break;
            }
            swapHeapElements(i, j);
            i = j;
        }
    }
    public int getHeapSize() {
        if (cachedHeapSize == -1) {
            cachedHeapSize = heap.Count;
        }
        return cachedHeapSize;
    }

    public GenericItem getRoot() {
        return heap[0];
    }
    private void swapHeapElements(int i, int j) {
        GenericItem store_heap_element = heap[i];
        heap[i] = heap[j];
        heap[j] = store_heap_element;
        itemIndexMap[heap[i]] = i;
        itemIndexMap[heap[j]] = j;
    }

    private void addToEnd(GenericItem item) {
        itemIndexMap.Add(item, heap.Count);
        heap.Add(item);
    }
    private void replaceWithEnd(int index) {
        if (index == heap.Count - 1) {
            itemIndexMap.Remove(heap[index]);
            heap.RemoveAt(heap.Count - 1);
            return;
        }
        itemIndexMap.Remove(heap[index]);
        heap[index] = heap[heap.Count - 1];
        itemIndexMap[heap[index]] = index;
        heap.RemoveAt(heap.Count - 1);

    }
}

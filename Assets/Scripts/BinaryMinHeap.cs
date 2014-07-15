using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class BinaryMinHeap<GenericItem> : MonoBehaviour where GenericItem : IComparable {
    private List<GenericItem> heap = new List<GenericItem>();//list of "nodes" that will make up heap structure
    private int i, j;
    private GenericItem root;

    public void insert(GenericItem node_data) { 
        heap.Add(node_data);//adds element and updates length of list
        bubbleUp();
    }

    //removes the minimum element in the tree and restructures tree with minHeapify to maintain integrity
    public GenericItem extractRoot() {
        if (getHeapSize() < 0) {
            throw new ArgumentOutOfRangeException();
        }
        GenericItem minElement = getRoot();
        heap[0] = heap[getHeapSize() - 1];
        heap.RemoveAt(getHeapSize() - 1);
        this.minHeapify(0);
        return minElement;
    }

    public GenericItem peekAtElement(int i) {
        return heap[i];
    }
    private void minHeapify(int i) {
        int smallest;
        int l = 2 * i + 1 ;
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
    private void bubbleUp(){
        i = getHeapSize() - 1;
        while (i > 0) {
            j = ((i + 1)/2 )- 1;
            //compares two elements to see if child is in correct place
            if (heap[j].CompareTo(heap[i]) <= 0) {
                break;
            }
            swapHeapElements(i, j);
            i = j;
        }
    }
    public int getHeapSize() {
        return heap.Count();
    }

    public GenericItem getRoot() {
        return heap[0];
    }
    private void swapHeapElements(int i, int j) {
        GenericItem store_heap_element = heap[i];
        heap[i] = heap[j];
        heap[j] = store_heap_element;
    }
}
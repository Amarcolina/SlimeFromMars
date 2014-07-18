using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class BinaryMinHeap<GenericItem> : MonoBehaviour where GenericItem : IComparable {
    private List<GenericItem> heap = new List<GenericItem>();//list of "nodes" that will make up heap structure
    private GenericItem root;

    public void insert(GenericItem node_data) { 
        heap.Add(node_data);//adds element and updates length of list
        bubbleUp(heap.Count - 1);
    }

    //removes the minimum element in the tree and restructures tree with minHeapify to maintain integrity
    public GenericItem extractElement(int i) {
        if (getHeapSize() < 0) {
            throw new ArgumentOutOfRangeException();
        }
        GenericItem extractedElement = heap[i];
        heap[i] = heap[getHeapSize() - 1];
        heap.RemoveAt(getHeapSize() - 1);
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

    public GenericItem extractElement(GenericItem element){
        GenericItem extractedElement = element;
        int index = 0;
        while(index < getHeapSize()){
          if(element.Equals(heap[index])){//if we've found the elment we're looking for, extract it
              extractedElement = heap[index];//save the value
              break;//no need to keep searching
          } 
            index++;
        }
        heap[index] = heap[getHeapSize() - 1];//fill empty spot left by extractedElement with last element in heap
        heap.RemoveAt(getHeapSize() - 1);//remove erroneous last element
        if (index >= getHeapSize()) {
            return extractedElement;
        }
        if (extractedElement.CompareTo(heap[index]) < 0) {//if the extractedElement was less than the element at that index, bubbleDown
            this.minHeapify(index);
        } else {//else bubbleUp   
            bubbleUp(index);
        }
        return extractedElement;
    }

    public bool contains(GenericItem element) {
        bool exists = false;
        for (int i = 0; i < getHeapSize(); i++) {
            if (element.Equals(heap[i])) {
                exists = true;
                break;
            }       
        }
        return exists;
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
    private void bubbleUp(int i){
        while (i > 0) {
            int j = ((i + 1)/2 )- 1;
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
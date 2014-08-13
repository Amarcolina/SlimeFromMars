using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProximitySearcher<T> where T : Component{
    private byte[] _indices;
    private T[] _objects;
    private int _length;

    //This pointer points to the START of the discard pile
    private int _thresholdPointer = 0;
    private float _distanceThresholdSqrd = 0;

    private T _currentClosest = null;

    private int _currentGlobalElement = 0;
    private int _currentLocalElement = 0;

    /* Creates a new proximity searcher.  There needs to be one proximity searcher for object
     * that wants to use it.  There is no sharing allowed because the searcher searches over
     * a period of time.  
     * 
     * The constructor needs an array of objects to search through.  These objects can be moving
     * or static, although moving will give worse accuracy.  The distance threshold is the distance
     * that objects are considered 'close' if they are within.  The searcher will search within
     * the close elements to find the closest, and search all the objects to decide if they
     * should be put into the close list.
     */
    public ProximitySearcher(T[] objects, float distanceThreshold = 25.0f) {
        if (objects.Length >= 255) {
            throw new System.Exception("Cannot operate on more than 256 objects at once");
        }

        _length = objects.Length;

        _distanceThresholdSqrd = distanceThreshold * distanceThreshold;
        _objects = objects;
        _indices = new byte[objects.Length];
        for (int i = 0; i < _indices.Length; i++) {
            _indices[i] = (byte) i;
        }

        _currentClosest = _objects[0];
    }

    /* Proccess a given number of global elements and local elements given the current 
     * position.  The globalCount parameter specified the number of elements in the world
     * to search through to decide if they should be put into the close list.  The local
     * count specifies the number of elements to search through in the close list.
     * 
     * This method returns the closest element so far that has been found.  
     */
    public T searchForClosest(Vector3 position, int globalCount = 1, int localCount = 1) {
        for (int i = 0; i < globalCount; i++) {
            proccessGlobalElement(position);
        }
        for (int i = 0; i < localCount; i++) {
            proccessLocalElement(position);
        }
        return _currentClosest;
    }

    public T getCurrentClosest() {
        return _currentClosest;
    }

    /* This returns the number of objects that are currently in the close list.  Note that this
     * might not accurately represent the actual state of the world, it is simply an aproximation
     */
    public int getNumberInLocalGroup() {
        return _thresholdPointer;
    }

    /* Access the internal array of close objects by index.  
     */
    public T this[int i]{
        get{
            return _objects[_indices[i]];
        }
    }

    private void proccessLocalElement(Vector3 position) {
        if (_thresholdPointer == 0) {
            return;
        }

        if (_currentClosest == null) {
            _currentClosest = _objects[_indices[0]];
        }

        if (checkNull(_currentLocalElement)) {
            _currentLocalElement = (_currentLocalElement + 1) % _thresholdPointer;
            return;
        }

        _currentLocalElement = _currentLocalElement % _thresholdPointer;
        int elementIndex = _indices[_currentLocalElement];

        T checkingElement = _objects[elementIndex];
        float distanceToCheckingElementSqrd = (checkingElement.transform.position - position).sqrMagnitude;
        float distanceToCurrElementSqrd = (_currentClosest.transform.position - position).sqrMagnitude;

        if (distanceToCheckingElementSqrd < distanceToCurrElementSqrd) {
            _currentClosest = checkingElement;
        }

        _currentLocalElement = (_currentLocalElement + 1) % _thresholdPointer;
    }

    private void proccessGlobalElement(Vector3 position) {
        if (checkNull(_currentGlobalElement)) {
            _currentGlobalElement = (_currentGlobalElement + 1) % _length;
            return;
        }

        _currentGlobalElement = _currentGlobalElement % _length;
        int elementIndex = _indices[_currentGlobalElement];  

        T element = _objects[elementIndex];
        float distSqrd = (position - element.transform.position).sqrMagnitude;

        if (_currentGlobalElement < _thresholdPointer && distSqrd > _distanceThresholdSqrd) {
            //Put it in the discard puile
            _thresholdPointer--;
            swap(_currentGlobalElement, _thresholdPointer);
        } else if (_currentGlobalElement >= _thresholdPointer && distSqrd < _distanceThresholdSqrd) {
            //Put it in the important pile
            swap(_currentGlobalElement, _thresholdPointer);
            _thresholdPointer++;
        }

        _currentGlobalElement = (_currentGlobalElement + 1) % _length;
    }

    private bool checkNull(int index) {
        int elementIndex = _indices[index];
        if (_objects[elementIndex] == null) {
            if (index < _thresholdPointer) {
                _thresholdPointer--;
                _length--;
                swap(index, _thresholdPointer);
                swap(_thresholdPointer, _length);
            } else {
                _length--;
                swap(index, _length);
            }
            return true;
        }
        return false;
    }

    private void swap(int index0, int index1) {
        byte value = _indices[index0];
        _indices[index0] = _indices[index1];
        _indices[index1] = value;
    }
}

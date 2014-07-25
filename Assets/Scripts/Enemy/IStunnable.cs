using UnityEngine;
using System.Collections;

/* The generic IStunnable interface.  Any component that wants to be
 * able to be stunned should implement this interface.
 */
public interface IStunnable {
    public void stun(float duration);
}

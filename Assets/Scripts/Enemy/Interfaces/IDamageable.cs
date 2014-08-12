using UnityEngine;
using System.Collections;

/* The generic IDamageable interface.  Any component that wants to be
 * able to recieve damage should implement this interface.
 */
public interface IDamageable {
    void damage(float damage);
    float getHealth();
}

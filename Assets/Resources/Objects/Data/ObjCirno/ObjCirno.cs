using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjCirno : MonoBehaviour {
    static GameObject bulletGameObject;

    static bool _initStaticDone = false;
    static void _InitStatic() {
        if (_initStaticDone) return;

        bulletGameObject = Resources.Load<GameObject>("Objects/Data/ObjCirno/Bullet");

        _initStaticDone = true;
    }

    public void PlayMusic() {
        Utils.GetMusicManager().Add(new MusicManager.MusicStackEntry {
            loopPath = "Music/Cirno Easter Egg",
            priority = 999999999
        });
    }

    public void ExplodeBullets() {
        _InitStatic();
        Vector2 origin = transform.position;
        int count = 16;

        count = Mathf.Min(count, 256);
        float angle = 0; // assuming 0=right, 90=up, 180=left, 270=down
        bool flipHSpeed = false;
        float speed = 1F;
        
        for (int t = 0; t < count; t++) {
            if (t % 16 == 0) {
                angle = 101.25F + 90F + (Time.time * 100); // and reset the angle
            }

            // create a bullet
            GameObject bullet = Instantiate(
                bulletGameObject,
                origin,
                Quaternion.identity
            );
            Rigidbody rigidbody = bullet.GetComponent<Rigidbody>();

            // set the ring's vertical speed to sine(angle)*speed
            // set the ring's horizontal speed to -cosine(angle)*speed
            rigidbody.velocity = new Vector2(
                Mathf.Sin(angle * Mathf.Deg2Rad) * speed,
                -Mathf.Cos(angle * Mathf.Deg2Rad) * speed
            ) * Utils.physicsScale;

            if (flipHSpeed) {
                // multiply the ring's horizontal speed by -1
                rigidbody.velocity = new Vector2(
                    -rigidbody.velocity.x,
                    rigidbody.velocity.y
                );
                // increase angle by 22.5
                angle += 22.5F;
            }
            flipHSpeed = !flipHSpeed; // if n is false, n becomes true and vice versa
        }
    }
}

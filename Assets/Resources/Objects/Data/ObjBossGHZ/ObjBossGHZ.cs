using UnityEngine;
using System.Collections.Generic;

public class ObjBossGHZ : ObjBoss {
    Transform eggmanTransform;
    Animator eggmanAnimator;

    float timer = 0;
    float stateTimer = 0;

    enum GHZBossState {
        Descending,
        Centering,
        LoweringPendulum,
        Starting,
        WaitRight,
        MovingRight,
        WaitLeft,
        MovingLeft,
        Exploding,
        Falling,
        Recovering,
        Escaping
    }
    GHZBossState state = GHZBossState.Descending;
    struct GHZBossTransition {
        public float stateTimerMax;
        public GHZBossState stateNext;
    }

    Dictionary <GHZBossState, GHZBossTransition> transitions = new Dictionary<GHZBossState, GHZBossTransition> {
        [GHZBossState.Descending] = new GHZBossTransition {
            stateTimerMax = 4,
            stateNext = GHZBossState.Centering
        },
        [GHZBossState.Centering] = new GHZBossTransition {
            stateTimerMax = 2F,
            stateNext = GHZBossState.LoweringPendulum
        },
        [GHZBossState.LoweringPendulum] = new GHZBossTransition {
            stateTimerMax = 2,
            stateNext = GHZBossState.Starting
        },
        [GHZBossState.Starting] = new GHZBossTransition {
            stateTimerMax = 2,
            stateNext = GHZBossState.WaitRight
        },
        [GHZBossState.WaitRight] = new GHZBossTransition {
            stateTimerMax = 60F / 60F,
            stateNext = GHZBossState.MovingRight
        },
        [GHZBossState.MovingRight] = new GHZBossTransition {
            stateTimerMax = 1,
            stateNext = GHZBossState.WaitLeft
        },
        [GHZBossState.WaitLeft] = new GHZBossTransition {
            stateTimerMax = 60F / 60F,
            stateNext = GHZBossState.MovingLeft
        },
        [GHZBossState.MovingLeft] = new GHZBossTransition {
            stateTimerMax = 1,
            stateNext = GHZBossState.WaitRight
        },
        [GHZBossState.Exploding] = new GHZBossTransition {
            stateTimerMax = 3,
            stateNext = GHZBossState.Falling
        },
        [GHZBossState.Falling] = new GHZBossTransition {
            stateTimerMax = 40F / 60F,
            stateNext = GHZBossState.Recovering
        },
        [GHZBossState.Recovering] = new GHZBossTransition {
            stateTimerMax = 50F / 60F,
            stateNext = GHZBossState.Escaping
        },
        [GHZBossState.Escaping] = new GHZBossTransition {
            stateTimerMax = 99999,
            stateNext = GHZBossState.Escaping
        }
    };

    GHZBossTransition transitionCurrent => transitions[state];

    public override void Awake() {
        base.Awake();
        StateInit();

        eggmanTransform = transform.Find("Eggman");
        eggmanAnimator = eggmanTransform.GetComponent<Animator>();
    }

    public void StateInit() {
        velocity = Vector3.zero;
        switch (state) {
            case GHZBossState.Descending:
                velocity.y = -1F * Utils.physicsScale;
                break;
            case GHZBossState.Starting:
                eggmanAnimator.Play("Idle");
                velocity.x = -0.25F * Utils.physicsScale;
                GetComponentInChildren<ObjSwing>().enabled = true;
                break;
            case GHZBossState.MovingLeft:
            case GHZBossState.Centering:
                velocity.x = -1F * Utils.physicsScale;
                transform.Find("Eggman").localScale = new Vector2(1, 1);
                break;
            case GHZBossState.MovingRight:
                velocity.x = 1F * Utils.physicsScale;
                transform.Find("Eggman").localScale = new Vector2(-1, 1);
                break;
            case GHZBossState.Escaping:
                MusicManager.current.Remove(musicStackEntry);
                eggmanAnimator.Play("Exploded Idle");
                transform.Find("Eggman").localScale = new Vector2(-1, 1);
                velocity.Set(
                    4 * Utils.physicsScale,
                    0.25F * Utils.physicsScale,
                    0
                );
                cameraZonePost.gameObject.SetActive(true);
                Destroy(cameraZonePre.gameObject);
                break;
            case GHZBossState.Exploding:
                eggmanAnimator.Play("Exploding");
                GetComponent<CreateExplosions>().enabled = true;
                
                foreach (ObjHarmful component in GetComponentsInChildren<ObjHarmful>())
                    component.enabled = true;
                
                foreach (Transform child in transform.Find("Swing")) {
                    Instantiate(
                        Constants.Get<GameObject>("prefabExplosionBoss"),
                        child.position,
                        Quaternion.identity
                    ).GetComponent<AudioSource>().Stop();
                }

                Instantiate(
                    Constants.Get<GameObject>("prefabGHZBall"),
                    transform.Find("Swing/Ball").position,
                    Quaternion.identity
                );

                Destroy(transform.Find("Swing").gameObject);
                break;
            case GHZBossState.Falling:
                GetComponent<CreateExplosions>().enabled = false;
                eggmanAnimator.Play("Exploded");
                break;
            case GHZBossState.LoweringPendulum:
                eggmanAnimator.Play("Laugh Loop");
                transform.Find("Swing").gameObject.SetActive(true);
                break;
        }
    }

    public override void Defeat(Character sourceCharacter) {
        base.Defeat(sourceCharacter);
        state = GHZBossState.Exploding;
        StateInit();
    }

    Vector3 velocity = Vector3.zero;

    public override void UpdateDelta(float modDeltaTime) {
        base.UpdateDelta(modDeltaTime);
        Vector3 position = transform.position;
        
        timer += modDeltaTime;
        stateTimer += modDeltaTime;

        if (stateTimer >= transitionCurrent.stateTimerMax) {
            stateTimer %= transitionCurrent.stateTimerMax;
            state = transitionCurrent.stateNext;
            StateInit();
        }

        // Hovering
        switch (state) {
            case GHZBossState.Centering:
            case GHZBossState.LoweringPendulum:
            case GHZBossState.Starting:
            case GHZBossState.MovingLeft:
            case GHZBossState.WaitRight:
            case GHZBossState.MovingRight:
            case GHZBossState.WaitLeft:
                position.y += Mathf.Sin((timer / 2F) * Mathf.PI * 2) * 0.005F;
                break;
        }

        switch (state) {
            case GHZBossState.Falling:
                velocity.y -= 0.09375F * Utils.physicsScale;
                break;
            case GHZBossState.Recovering:
                velocity.y += 0.03125F * Utils.physicsScale;
                break;

        }

        position += velocity * modDeltaTime;
        transform.position = position;
    }

    public override void Hurt(Character sourceCharacter) {
        base.Hurt(sourceCharacter);
        eggmanAnimator.Play("Hurt", -1, 0);
    }

    public override void Laugh() {
        eggmanAnimator.Play("Laugh");
    }
}
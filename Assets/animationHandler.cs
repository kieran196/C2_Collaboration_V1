using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationHandler : MonoBehaviour {

    public RuntimeAnimatorController animatorController;
    private Animator animator = null;
    private int currentAnimationIdx = 0;

    public static float[] BLINK_AMOUNT = {4, 8}; // Blink randomly between x and y seconds
    private float BLINK_TIMER;

    private List<string> animations = new List<string> {
            "smile",
            "blink",
            "kiss",
            "puff",
            "yawning",
            "chewing",
            "mouth_left_right",
        };

    public void CreateAnimator() {
        ChangeCurrentAnimation(0);

        animator = this.gameObject.AddComponent<Animator>();
        animator.applyRootMotion = true;
        animator.runtimeAnimatorController = animatorController;
        BLINK_TIMER = Random.Range(BLINK_AMOUNT[0], BLINK_AMOUNT[1]);
    }

    private void ChangeCurrentAnimation(int delta) {
        var newIdx = currentAnimationIdx + delta;
        if(newIdx < 0 || newIdx >= animations.Count)
            return;
        currentAnimationIdx = newIdx;
        PlayCurrentAnimation();
    }

    void Start() {
        CreateAnimator();
    }

    public void OnPrevAnimation() {
        ChangeCurrentAnimation(-1);
    }

    public void OnNextAnimation() {
        ChangeCurrentAnimation(+1);
    }

    public void DestroyAnimator() {
        animator = null;
    }

    public void PlayCurrentAnimation() {
        if(animator != null)
            animator.Play(animations[currentAnimationIdx]);
    }

    public void Blink() {
        if(animator != null)
            animator.Play(animations[1]);
    }

    private float timer = 0f;

    void Update() {
        timer += Time.deltaTime;
        if (timer >= BLINK_TIMER) {
            Blink();
            //print("Playing anim at index:"+ currentAnimationIdx);
            //PlayCurrentAnimation();
            //OnNextAnimation();
            timer = 0f;
            BLINK_TIMER = Random.Range(BLINK_AMOUNT[0], BLINK_AMOUNT[1]);
            print("New blink timer set:" + BLINK_TIMER);
        }
    }


}

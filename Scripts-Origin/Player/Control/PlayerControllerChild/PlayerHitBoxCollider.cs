using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitBoxCollider : MonoBehaviour
{
    [SerializeField] private CheatMode cheatMode;
    [SerializeField] private MaskChange maskChange;
    
    [SerializeField] private GameTimeScale playerTimeScale;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private PlayerEffect playerEffect;
    [SerializeField] private PlayerCameraEffect playerCameraEffect;
    [SerializeField] private PlayerSound playerSound;

    private PlayerCommonData commonData;
    private PlayerHumanMaskData humanData;
    private PlayerAnimalMaskData animalData;

    private float damage;
    private GameObject damager;
    private char color;
    private float value;

    private bool canShakeCamera;
    private bool canSetTimeScale;
    private bool canPlayAudio;

    private void OnEnable()
    {
        commonData = PlayerCommonData.Instance;
        humanData = PlayerHumanMaskData.Instance;
        animalData = PlayerAnimalMaskData.Instance;

        canShakeCamera = true;
        canSetTimeScale = true;
        canPlayAudio = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Enemy") ||
            collision.gameObject.GetComponent<Enemy>() == null ||
            collision.gameObject.GetComponent<Enemy>().isDead) 
        {
            return;
        }

        //hitOnce = false;

        Vector3 pointOfContact = collision.contacts[0].point;

        float damage = 0;
        char color = 'B';
        float stack = 0;

        if (this.gameObject.CompareTag("HumanNormalAttack"))
        {
            damage = humanData.normalAttactStat.damage;
            color = 'B';
            stack = humanData.normalAttactStat.inkStack;

            switch (playerState.playerCurrentSubState)
            {
                case PlayerSubStateType.HUMAN_FIRSTNORMALATTACK:
                    playerEffect.StartCoroutine(playerEffect.TogglePlayerHitEffect(humanData.firstNormalAttackHitEffect, playerEffect.InkHitEffect, pointOfContact));
                    if (canSetTimeScale)
                    { 
                        playerTimeScale.StartCoroutine(playerTimeScale.CoSetTimeScale(humanData.firstNormalAttackHitTimeScale)); 
                        canSetTimeScale = false;
                    }
                    if (canShakeCamera)
                    {
                        playerCameraEffect.ShakeCamera(humanData.firstNormalAttackHitCameraShake);
                        canShakeCamera = false;
                    }
                    if (canPlayAudio)
                    {
                        playerSound.SetPlayerSound(humanData.firstNormalAttackHitSound,pointOfContact,Time.time);
                        canPlayAudio = false;
                    }
                    break;

                case PlayerSubStateType.HUMAN_SECONDNORMALATTACK:
                    playerEffect.StartCoroutine(playerEffect.TogglePlayerHitEffect(humanData.secondNormalAttackHitEffect, playerEffect.InkHitEffect, pointOfContact));
                    if (canSetTimeScale)
                    {
                        playerTimeScale.StartCoroutine(playerTimeScale.CoSetTimeScale(humanData.secondNormalAttackHitTimeScale));
                        canSetTimeScale = false;
                    }
                    if (canShakeCamera)
                    {
                    playerCameraEffect.ShakeCamera(humanData.secondNormalAttackHitCameraShake);
                        canShakeCamera = false;
                    }
                    if (canPlayAudio)
                    {
                        playerSound.SetPlayerSound(humanData.secondNormalAttackHitSound, pointOfContact, Time.time);
                        canPlayAudio = false;
                    }
                    break;

                case PlayerSubStateType.HUMAN_THIRDNORMALATTACK:
                    playerEffect.StartCoroutine(playerEffect.TogglePlayerHitEffect(humanData.thirdNormalAttackHitEffect, playerEffect.InkHitEffect, pointOfContact));
                    if (canSetTimeScale)
                    {
                    playerTimeScale.StartCoroutine(playerTimeScale.CoSetTimeScale(humanData.thirdNormalAttackHitTimeScale));
                        canSetTimeScale = false;
                    }
                    if (canShakeCamera)
                    {
                    playerCameraEffect.ShakeCamera(humanData.thirdNormalAttackHitCameraShake);
                        canShakeCamera = false;
                    }
                    if (canPlayAudio)
                    {
                        playerSound.SetPlayerSound(humanData.thirdNormalAttackHitSound, pointOfContact, Time.time);
                        canPlayAudio = false;
                    }
                    break;
            }
        }
        else if (this.gameObject.CompareTag("HumanFirstSkill"))
        {
            damage = humanData.inkShapeStat.damage;
            color = 'B';
            stack = humanData.inkShapeStat.inkStack;

            playerEffect.StartCoroutine(playerEffect.TogglePlayerHitEffect(humanData.inkShapeHitEffect, playerEffect.InkHitEffect, pointOfContact));
            if (canSetTimeScale)
            {
                playerTimeScale.StartCoroutine(playerTimeScale.CoSetTimeScale(humanData.inkShapeHitTimeScale));
                canSetTimeScale = false;
            }
            if (canShakeCamera)
            {
                playerCameraEffect.ShakeCamera(humanData.inkShapeHitCameraShake);
                canShakeCamera = false;
            }
            if (canPlayAudio)
            {
                playerSound.SetPlayerSound(humanData.inkShapeHitSound, pointOfContact, Time.time);
                canPlayAudio = false;
            }

        }
        else if (this.gameObject.CompareTag("HumanSecondSkill"))
        {
            damage = humanData.inkFloorStat.damage;
            color = 'B';
            stack = humanData.inkFloorStat.inkStack;
        }
        else if (this.gameObject.CompareTag("AnimalNormalAttack"))
        {
            damage = animalData.normalAttactStat.damage;
            color = 'W';
            stack = animalData.normalAttactStat.inkStack;

            switch (playerState.playerCurrentSubState)
            {
                case PlayerSubStateType.ANIMAL_FIRSTNORMALATTACK:
                    playerEffect.StartCoroutine(playerEffect.TogglePlayerHitEffect(animalData.firstNormalAttackHitEffect, playerEffect.InkHitEffect, pointOfContact));
                    if (canSetTimeScale)
                    {
                        playerTimeScale.StartCoroutine(playerTimeScale.CoSetTimeScale(animalData.firstNormalAttackHitTimeScale));
                        canSetTimeScale = false;
                    }
                    if (canShakeCamera)
                    {
                        playerCameraEffect.ShakeCamera(animalData.firstNormalAttackHitCameraShake);
                        canShakeCamera = false;
                    }
                    if (canPlayAudio)
                    {
                        playerSound.SetPlayerSound(animalData.firstNormalAttackHitSound, pointOfContact, Time.time);
                        canPlayAudio = false;
                    }
                    break;
                case PlayerSubStateType.ANIMAL_SECONDNORMALATTACK:
                    playerEffect.StartCoroutine(playerEffect.TogglePlayerHitEffect(animalData.secondNormalAttackHitEffect, playerEffect.InkHitEffect, pointOfContact));
                    if (canSetTimeScale)
                    {
                        playerTimeScale.StartCoroutine(playerTimeScale.CoSetTimeScale(animalData.secondNormalAttackHitTimeScale));
                        canSetTimeScale = false;
                    }
                    if (canShakeCamera)
                    {
                        playerCameraEffect.ShakeCamera(animalData.secondNormalAttackHitCameraShake);
                        canShakeCamera = false;
                    }
                    if (canPlayAudio)
                    {
                        playerSound.SetPlayerSound(animalData.secondNormalAttackHitSound, pointOfContact, Time.time);
                        canPlayAudio = false;
                    }
                    break;

                case PlayerSubStateType.ANIMAL_THIRDNORMALATTACK:
                    playerEffect.StartCoroutine(playerEffect.TogglePlayerHitEffect(animalData.thirdNormalAttackHitEffect, playerEffect.InkHitEffect, pointOfContact));
                    if (canSetTimeScale)
                    {
                        playerTimeScale.StartCoroutine(playerTimeScale.CoSetTimeScale(animalData.thirdNormalAttackHitTimeScale));
                        canSetTimeScale = false;
                    }
                    if (canShakeCamera)
                    {
                        playerCameraEffect.ShakeCamera(animalData.thirdNormalAttackHitCameraShake);
                        canShakeCamera = false;
                    }
                    if (canPlayAudio)
                    {
                        playerSound.SetPlayerSound(animalData.thirdNormalAttackHitSound, pointOfContact, Time.time);
                        canPlayAudio = false;
                    }
                    break;
            }

        }
        else if (this.gameObject.CompareTag("AnimalFirstSkill"))
        {
            damage = animalData.leapStrikeStat.damage;
            color = 'W';
            stack = animalData.leapStrikeStat.inkStack;
            playerEffect.StartCoroutine(playerEffect.TogglePlayerHitEffect(animalData.leapStrikeHitEffect, playerEffect.InkHitEffect, pointOfContact));
            if (canSetTimeScale)
            {
                playerTimeScale.StartCoroutine(playerTimeScale.CoSetTimeScale(animalData.leapStrikeHitTimeScale));
                canSetTimeScale = false;
            }
            if (canShakeCamera)
            {
                playerCameraEffect.ShakeCamera(animalData.leapStrikeHitCameraShake);
                canShakeCamera = false;
            }
            if (canPlayAudio)
            {
                playerSound.SetPlayerSound(animalData.leapStrikeHitSound, pointOfContact, Time.time);
                canPlayAudio = false;
            }
        }
        else if (this.gameObject.CompareTag("AnimalSecondSkill"))
        {
            damage = animalData.roarStat.damage;
            color = 'W';
            stack = animalData.roarStat.inkStack;
        }
        else
        {
            return;
        }

        #region 치트
        //데미지 9999 치트
        if (cheatMode.isDamageMax)
        {
            damage = 9999;
        }
        
        //덧칠 풀 스택 치트
        if (cheatMode.isPaintOverlapMax)
        {
            collision.gameObject.GetComponent<CalliSystem>().Painting('B', 10);
            collision.gameObject.GetComponent<CalliSystem>().Painting('W', 10);
            collision.gameObject.GetComponent<CalliSystem>().Painting('B', 10);
            collision.gameObject.GetComponent<CalliSystem>().Painting('W', 10);
        }
        #endregion

        SetMessage(damage, color, stack);

        //적에게 데미지 정보 전달
        var atkTarget = collision.gameObject.GetComponent<Enemy>();
        if (atkTarget != null)
        {
            var message = new DamageMessage();
            message.amount = damage;
            message.damager = damager;
            message.color = color;
            message.value = stack;

            atkTarget.ApplyDamage(message);
        }
    }
    private void SetMessage(float setDamage, char setColor, float setStackValue)
    {
        damage = setDamage;
        damager = maskChange.CurrentMask;
        color = setColor;
        value = setStackValue;
    }
}

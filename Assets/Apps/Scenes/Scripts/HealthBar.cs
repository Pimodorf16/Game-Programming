using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Sprite fullHeart;
    public Sprite threeQuarterHeart;
    public Sprite halfHeart;
    public Sprite quarterHeart;
    public Sprite emptyHeart;

    Image heartImage;

    private void Awake()
    {
        heartImage = GetComponent<Image>();
    }

    public void SetHeartImage(HeartStatus status)
    {
        switch (status)
        {
            case HeartStatus.Empty:
                heartImage.sprite = emptyHeart;
                break;
            case HeartStatus.Quarter:
                heartImage.sprite = quarterHeart;
                break;
            case HeartStatus.Half:
                heartImage.sprite = halfHeart;
                break;
            case HeartStatus.ThreeQuarter:
                heartImage.sprite = threeQuarterHeart;
                break;
            case HeartStatus.Full:
                heartImage.sprite = fullHeart;
                break;
            default:
                break;
        }
    }
}

public enum HeartStatus
{
    Empty = 0,
    Quarter = 1,
    Half = 2,
    ThreeQuarter = 3,
    Full = 4
}

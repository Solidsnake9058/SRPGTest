using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteMetarial : MonoBehaviour
{
    public SpriteRenderer tileMetarial;
    public List<Sprite> sprites;

    public int spriteCount { get { return sprites.Count; } }

    public Sprite SetSprite(int index)
    {
        tileMetarial.sprite = sprites[index];
        return sprites[index];
    }
}

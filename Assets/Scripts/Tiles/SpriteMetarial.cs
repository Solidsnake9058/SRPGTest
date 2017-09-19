using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteMetarial : MonoBehaviour
{
    public SpriteRenderer tileMetarial;
    public List<Sprite> sprites;
    public List<Sprite> chestSprites;

    public int spriteCount { get { return sprites.Count; } }

    public int GetSpritesCount()
    {
        return sprites.Count;
    }

    public Sprite SetSprite(int index)
    {
        if (index >= sprites.Count)
        {
            tileMetarial.sprite = chestSprites[index- sprites.Count];
            return chestSprites[index - sprites.Count];
        }

        tileMetarial.sprite = sprites[index];
        return sprites[index];
    }
}

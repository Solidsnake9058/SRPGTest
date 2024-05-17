using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteMetarial : MonoBehaviour
{
    [SerializeField]
    public SpriteRenderer m_TileMetarial;
    [SerializeField]
    public SpriteRenderer m_TileHighLightMetarial;

    [SerializeField]
    public List<Sprite> _Sprites;
    public List<Sprite> _ChestSprites;

    public int SpriteCount => _Sprites.Count;

    public int GetSpritesCount()
    {
        return _Sprites.Count;
    }

    public Sprite SetSprite(int index)
    {
        SetHighlightColor(Color.clear);
        if (index >= _Sprites.Count)
        {
            m_TileMetarial.sprite = _ChestSprites[index - _Sprites.Count];
            return _ChestSprites[index - _Sprites.Count];
        }

        m_TileMetarial.sprite = _Sprites[index];
        return _Sprites[index];
    }

    public void SetHighlightColor(Color color)
    {
        m_TileHighLightMetarial.color = color;
    }
}
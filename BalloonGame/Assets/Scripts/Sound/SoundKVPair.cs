using System.Collections.Generic;
using System;
using UnityEngine;

// SoundSourceを基準にソートするための比較器クラス
public class SoundKVPairComparer : IComparer<SoundKVPair>
{
    public int Compare(SoundKVPair x, SoundKVPair y)
    {
        if (x == null || y == null)
        {
            throw new ArgumentNullException();
        }

        //ソートを文字列にすると辞書順になってしまうため
        //enumの値でソートするようにする
        return x.Source.CompareTo(y.Source);
    }
}

[Serializable]
public class SoundKVPair
{
    //文字列で管理することでenumが変更されても対応可能にする
    [SerializeField, HideInInspector] string _key;
    [SerializeField] SoundSettings _value;
    readonly SoundSource _source;

    public SoundKVPair(SoundSource key, SoundSettings value)
    {
        _key = key.ToString();
        _value = value;
        _source = key;
    }

    public string Key => _key;
    public SoundSettings Value => _value;
    public SoundSource Source => _source;
}
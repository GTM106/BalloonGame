using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class SoundList
{
    [SerializeField] List<SoundKVPair> _soundKVPairs;
    static readonly SoundKVPairComparer kVPairComparer = new();

    public void Add(SoundKVPair pair)
    {
        _soundKVPairs.Add(pair);

        //insertなどで変更に対する複雑な処理をするよりソートして確実に行う。
        //実行速度は遅いが、EditorなのでOKとする
        _soundKVPairs.Sort(kVPairComparer);
    }

    public bool TryGetValue(SoundSource key, out SoundSettings soundSettings)
    {
        //通常プレイ時はここしかこないはず
        if (_soundKVPairs.Count > (int)key)
        {
            if (_soundKVPairs[(int)key].Key.Equals(key.ToString()))
            {
                soundSettings = _soundKVPairs[(int)key].Value;
                return true;
            }
        }

        //エディタ時のみこれ以降に遷移する
        foreach (var pair in _soundKVPairs)
        {
            if (!pair.Key.Equals(key.ToString())) continue;

            soundSettings = pair.Value;
            return true;
        }

        SoundKVPair newKVP = new(key, new());
        _soundKVPairs.Add(newKVP);
        soundSettings = newKVP.Value;
        _soundKVPairs.Sort(kVPairComparer);
        return false;
    }

    public void CheckSourceChanges()
    {
        //予めソートしておく
        _soundKVPairs.Sort(kVPairComparer);

        for (int i = 0; i < (int)SoundSource.Max; i++)
        {
            SoundSource soundSource = (SoundSource)i;

            //Pairが少なかったら、上から確認しているため確実に(i)に該当するものは存在しない
            if (_soundKVPairs.Count <= i)
            {
                //新たに追加する
                _soundKVPairs.Add(new(soundSource, new()));

                //追加したものは位置が正しいため次へ
                continue;
            }

            //要素が正しい位置にくるまで繰り返す
            int result = -1;
            while (result != 0)
            {
                result = string.Compare(_soundKVPairs[i].Key, soundSource.ToString());

                //次のenum要素が先なら消されたものが存在する
                if (result < 0)
                {
                    _soundKVPairs.RemoveAt(i);
                }
                //配列要素が先なら追加がある
                else if (result > 0)
                {
                    _soundKVPairs.Insert(i, new(soundSource, new()));
                }
            }
        }
    }
}

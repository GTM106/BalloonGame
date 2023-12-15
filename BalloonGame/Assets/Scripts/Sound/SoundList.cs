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

        //insert�ȂǂŕύX�ɑ΂��镡�G�ȏ�����������\�[�g���Ċm���ɍs���B
        //���s���x�͒x�����AEditor�Ȃ̂�OK�Ƃ���
        _soundKVPairs.Sort(kVPairComparer);
    }

    public bool TryGetValue(SoundSource key, out SoundSettings soundSettings)
    {
        //�ʏ�v���C���͂����������Ȃ��͂�
        if (_soundKVPairs.Count > (int)key)
        {
            if (_soundKVPairs[(int)key].Key.Equals(key.ToString()))
            {
                soundSettings = _soundKVPairs[(int)key].Value;
                return true;
            }
        }

        //�G�f�B�^���݂̂���ȍ~�ɑJ�ڂ���
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
        //�\�߃\�[�g���Ă���
        _soundKVPairs.Sort(kVPairComparer);

        for (int i = 0; i < (int)SoundSource.Max; i++)
        {
            SoundSource soundSource = (SoundSource)i;

            //Pair�����Ȃ�������A�ォ��m�F���Ă��邽�ߊm����(i)�ɊY��������̂͑��݂��Ȃ�
            if (_soundKVPairs.Count <= i)
            {
                //�V���ɒǉ�����
                _soundKVPairs.Add(new(soundSource, new()));

                //�ǉ��������͈̂ʒu�����������ߎ���
                continue;
            }

            //�v�f���������ʒu�ɂ���܂ŌJ��Ԃ�
            int result = -1;
            while (result != 0)
            {
                result = string.Compare(_soundKVPairs[i].Key, soundSource.ToString());

                //�����l�Ȃ�X�L�b�v
                if (result == 0) continue;

                //����enum�v�f����Ȃ�����ꂽ���̂����݂���
                if (result < 0)
                {
                    _soundKVPairs.RemoveAt(i);
                }
                //�z��v�f����Ȃ�ǉ�������
                else
                {
                    _soundKVPairs.Insert(i, new(soundSource, new()));
                }
            }
        }
    }
}

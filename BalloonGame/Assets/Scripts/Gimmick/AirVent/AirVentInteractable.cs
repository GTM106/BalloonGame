using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//インスペクターで管理したい為、インターフェイスではなく基底クラスで管理します。
public abstract class AirVentInteractable : MonoBehaviour
{
    public abstract void Interact();
}

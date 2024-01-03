using Cysharp.Threading.Tasks;

internal interface ITransition
{
    UniTask StartTransition(TransitionData trantisionData);
}
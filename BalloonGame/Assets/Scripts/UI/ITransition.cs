using Cysharp.Threading.Tasks;

internal interface ITransition
{
    UniTask StartTransition(TrantisionData trantisionData);
    bool IsTransitionComplete();
}
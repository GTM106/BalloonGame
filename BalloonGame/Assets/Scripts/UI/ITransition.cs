using Cysharp.Threading.Tasks;

internal interface ITransition
{
    UniTask StartTransition(TrantisionData trantisionData);
    UniTask StartTransition();
    bool IsTransitionComplete();
}
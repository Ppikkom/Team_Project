// BehaviorGraphAgentExtensions.cs
using Unity.Behavior;

public static class BehaviorGraphAgentExtensions
{
    /// 블랙보드에서 name 변수의 실제 T 값을 꺼내서 out으로 넘겨주고,
    /// 변수 존재 여부를 bool로 리턴한다.
    public static bool TryGetValue<T>(this BehaviorGraphAgent agent, string name, out T value)
    {
        if (agent.GetVariable(name, out BlackboardVariable<T> var))
        {
            value = var.Value;
            return true;
        }
        value = default;
        return false;
    }

    /// 블랙보드에 name이 반드시 있다고 가정하고 값을 바로 리턴.
    /// 없으면 default(T)을 리턴한다.
    public static T GetValueOrDefault<T>(this BehaviorGraphAgent agent, string name)
    {
        return agent.TryGetValue<T>(name, out var v) ? v : default;
    }
}
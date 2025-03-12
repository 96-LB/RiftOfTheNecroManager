using System.Reflection;

namespace LalaDancer;

internal class FieldRef<V>(object instance, FieldInfo field) {
    public V Value => (V)field.GetValue(instance);

    public FieldRef(object instance, string name) :
        this(instance, instance.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)) { }

    public FieldRef<V> Set(V value) {
        field.SetValue(instance, value);
        return this;
    }

    public static implicit operator V(FieldRef<V> fieldRef) {
        return fieldRef.Value;
    }

    public static FieldRef<V> operator <<(FieldRef<V> fieldRef, V value) {
        return fieldRef.Set(value);
    }
}

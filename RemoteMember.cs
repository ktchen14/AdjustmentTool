using System;
using System.Reflection;
using JetBrains.Annotations;

namespace AdjustmentTool {
  /// Used to access a member of a "remote" class regardless of its visibility
  [MeansImplicitUse(ImplicitUseKindFlags.Assign)]
  [AttributeUsage(AttributeTargets.Field)]
  internal class RemoteMemberAttribute : Attribute {
    public const BindingFlags Search =
      BindingFlags.Instance | BindingFlags.NonPublic;

    private string RemoteName { get; }
    private (FieldInfo f, PropertyInfo p, MethodInfo m) remoteInfo;

    internal RemoteMemberAttribute() { }
    internal RemoteMemberAttribute(string name) => RemoteName = name;

    internal static void Load(object source, object remote) {
      foreach (var info in source.GetType().GetFields(Search)) {
        var item = info.GetCustomAttribute<RemoteMemberAttribute>();
        if (item == null)
          continue;
        item.Load(source, remote, info);
      }
    }

    private void Load(object source, object remote, FieldInfo info) {
      var type = remote.GetType();
      var name = RemoteName ?? info.Name;
      object result;

      if ((remoteInfo.f = type.GetField(name, Search)) != null)
        result = remoteInfo.f.GetValue(remote);
      else if ((remoteInfo.p = type.GetProperty(name, Search)) != null)
        result = remoteInfo.p.GetValue(remote);
      else if ((remoteInfo.m = type.GetMethod(name, Search)) != null)
        result = remoteInfo.m.CreateDelegate(info.FieldType, remote);
      else throw new Exception($"No such member {name} in {type}");

      info.SetValue(source, result);
    }
  }
}
